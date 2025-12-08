
namespace Backend.Models
{
    public class PlaybackPosition
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public int PositionInSeconds { get; set; }
        public int TotalDurationInSeconds { get; set; }
        public DateTime LastWatchedDate { get; set; } = DateTime.UtcNow;
    }
}