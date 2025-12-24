using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Users
{
    public record EmailVerificationDto(
        [Required] string Token 
    );
}
