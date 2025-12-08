using Backend.Models;

namespace Backend.Services.Interfaces 
{
    public interface IJwtService
    {
        string GenerateUserToken(User user);
        string GenerateRefreshToken();
        string GenerateProfileToken(Profile profile);
    }
}
