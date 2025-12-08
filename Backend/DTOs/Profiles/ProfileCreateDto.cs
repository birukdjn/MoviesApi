using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Profiles
{
    public class ProfileCreateDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsKidsProfile { get; set; } = false;
    }
}