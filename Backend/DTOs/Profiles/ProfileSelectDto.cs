namespace Backend.DTOs.Profiles
{
    public class ProfileSelectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsKidsProfile { get; set; }

        public DateTime? LastActiveDate { get; set; } 
    }
}