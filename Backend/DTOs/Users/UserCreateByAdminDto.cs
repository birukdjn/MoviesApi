
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Users
{
    public class UserCreateByAdminDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

    }
}