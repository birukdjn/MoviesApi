using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Users
{
    public record EmailVerificationRequest(
        [Required(ErrorMessage = "Token is required")]
        string Token
    );

    public record EmailVerificationResponse(
        string Message,
        string? UserToken = null,
        string? ProfileToken = null,
        string? RefreshToken = null
    );
}