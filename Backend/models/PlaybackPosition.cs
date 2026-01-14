
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class PlaybackPosition
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public Double PositionSeconds { get; set; }
        public double DurationSeconds { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        [NotMapped]
        public bool IsFinished => PositionSeconds / DurationSeconds > 0.9;
    }
}