// Backend/DTOs/UserCreateByAdminDto.cs

using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Users
{
    // Note: No Password field here, as the server will generate it
    public class UserCreateByAdminDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = "Admin";

    }
}