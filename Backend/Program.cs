using Backend.Attributes;
using Backend.Configuration;
using Backend.Data;
using Backend.Services.Implementations;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Stripe;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// =========================================================================
// 1. Service Registration (Configuration)
// =========================================================================

// Email Configuration and Service
var emailConfig = configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();

if (emailConfig is not null)
{
    services.AddSingleton(emailConfig);
}
services.AddScoped<IEmailSender, EmailSender>();

// Third-Party Service Clients
services.AddHttpClient<ChapaService>();

// Program.cs
services.AddScoped<IPasswordService, PasswordService>();
services.AddScoped<IJwtService, JwtService>();

// Twilio Configuration and Service
services.Configure<TwilioSettings>(
    configuration.GetSection("Twilio"));
services.AddScoped<ISmsService, SmsService>();

// Database Context (EF Core)
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// MVC/API Controllers configuration
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
services.AddEndpointsApiExplorer();

services.AddScoped<RequireSubscriptionAttribute>();

// Authentication & Performance Middleware Services

services.AddResponseCompression();
services.AddResponseCaching();
services.AddLocalization();

services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.PermitLimit = 20;
        config.Window = TimeSpan.FromSeconds(10);
        config.QueueLimit = 10;
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = 429;
});

var jwtSection = configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("Key")!);

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


services.AddAuthorizationBuilder()
        .AddPolicy("SubscribedOnly", policy =>
        {
            policy.RequireAssertion(context =>
            {
                var isSubscribed = context.User.HasClaim(c => c.Type == "IsSubscribed" && c.Value == "true");
                return isSubscribed;
            });
        });


services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MoviesStore", Version = "v1" });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement

    {

        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});


services.AddCors(options =>
{
    options.AddPolicy("allowedDomains", policy =>
    {
        // For production, list specific origins and ensure AllowCredentials is set if needed
        policy.WithOrigins("http://localhost:3000", "http://192.168.100.167:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Stripe Configuration (Payment)
var stripeSection = configuration.GetSection("Stripe");
StripeConfiguration.ApiKey = stripeSection.GetValue<string>("SecretKey");

var app = builder.Build();


// =========================================================================
// 2. Middleware Pipeline Configuration (Order is Critical)
// =========================================================================

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Swagger/SwaggerUI is typically enabled only in development/staging
    app.UseSwagger(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MoviesStore V1");
    });

    // Development-only: Run database seeding
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    DbSeeder.Seed(db, passwordService);
}
else
{
    // Production Exception Handling
    app.UseExceptionHandler(errApp =>
    {
        errApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            var exception = errorFeature?.Error;
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "An internal server error occurred." });
            await context.Response.WriteAsync(result);
        });
    });

    // Enforce HTTPS
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseCors("allowedDomains"); 
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("fr-FR"), new CultureInfo("es-ES"), new CultureInfo("de-DE"), new CultureInfo("it-IT") };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures([.. supportedCultures.Select(c => c.Name)])
    .AddSupportedUICultures([.. supportedCultures.Select(c => c.Name)]);

app.UseRequestLocalization(localizationOptions);

app.UseResponseCaching();
app.UseResponseCompression();

app.MapControllers();



app.Run();