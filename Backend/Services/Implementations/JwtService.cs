using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Services.Implementations
{
    public class JwtService(IConfiguration config) : IJwtService
    {
        private readonly IConfiguration _config = config;

        private SymmetricSecurityKey GetSigningKey() =>
            new(Encoding.UTF8.GetBytes(_config.GetSection("Jwt").GetValue<string>("Key")!));

        // Token properties
        private string Issuer => _config["Jwt:Issuer"]!;
        private string Audience => _config["Jwt:Audience"]!;
        private SigningCredentials Creds => new(GetSigningKey(), SecurityAlgorithms.HmacSha256);

        public string GenerateUserToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
               issuer: Issuer,
               audience: Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(60), 
               signingCredentials: Creds
           );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }


        public string GenerateProfileToken(Profile profile)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, profile.UserId.ToString()), 
                new Claim("ProfileId", profile.Id.ToString()), 
                new Claim(ClaimTypes.Role, profile.User.Role) 
            };

            var token = new JwtSecurityToken(
               issuer: Issuer,
               audience: Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddHours(6), 
               signingCredentials: Creds
           );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}