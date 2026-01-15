using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Backend.Data;
using Backend.Services.Interfaces;

namespace Backend.Services.Implementations
{
    public class EmailVerificationService(
        AppDbContext context,
        IJwtService jwtService,
        ILogger<EmailVerificationService> logger) : IEmailVerificationService
    {
        private readonly AppDbContext _context = context;
        private readonly IJwtService _jwtService = jwtService;
        private readonly ILogger<EmailVerificationService> _logger = logger;

        public async Task<EmailVerificationResult> VerifyEmailAsync(string token)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

                if (user == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired email verification token attempted");
                    return new EmailVerificationResult.InvalidToken();
                }

                if (user.IsEmailVerified)
                {
                    _logger.LogInformation("Email already verified for user {UserId}", user.Id);
                    return new EmailVerificationResult.AlreadyVerified();
                }

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;

                var userToken = _jwtService.GenerateUserToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                var defaultProfile = await _context.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (defaultProfile == null)
                {
                    _logger.LogError(
                        "No profile found for user {UserId} during email verification",
                        user.Id
                    );
                    return new EmailVerificationResult.ProfileMissing();
                }

                var profileToken = _jwtService.GenerateProfileToken(defaultProfile);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Email verified successfully for user {UserId} ({Email})",
                    user.Id, user.Email
                );

                return new EmailVerificationResult.Success(
                    userToken,
                    profileToken,
                    refreshToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error verifying email with token: {TokenPrefix}...",
                    token[..Math.Min(10, token.Length)]
                );
                return new EmailVerificationResult.Failure(
                    "An error occurred during email verification"
                );
            }
        }
    }
}