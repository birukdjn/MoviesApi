using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Users
{
    public record UserRegisterDto(
        [Required]string Username, 
        [Required][EmailAddress]string Email, 
        [Required][MinLength(8)]string Password,
         string Phone,
          string? Avatar );

}
