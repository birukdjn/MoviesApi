namespace Backend.DTOs.Admin
{
    public class AdminUserViewDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubscribed { get; set; }
        public string? LastLoginIp { get; set; }

        public required string Profiles { get; set; }

        public string? Avatar { get; set; }
    }
}
