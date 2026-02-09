using System.Net;
using System.Text.Json;
using Backend.DTOs;

namespace Backend.Data.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // In development, show the full error. In production, show a generic message.
            var response = env.IsDevelopment()
                ? new ErrorResponse(context.Response.StatusCode, ex.Message, ex.StackTrace)
                : new ErrorResponse(context.Response.StatusCode, "Internal Server Error. Please try again later.");

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}