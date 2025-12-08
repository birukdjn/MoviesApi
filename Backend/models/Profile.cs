

namespace Backend.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsKidsProfile { get; set; } = false;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<PlaybackPosition> PlaybackPositions { get; set; } = [];
        public List<Favorite> Favorites { get; set; } = [];
        public List<Rating> Ratings { get; set; } = [];
    }
}
