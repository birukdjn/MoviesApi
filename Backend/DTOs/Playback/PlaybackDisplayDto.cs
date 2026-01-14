namespace Backend.DTOs.Playback
{
    public class PlaybackDisplayDto
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public double PositionSeconds { get; set; }
        public double DurationSeconds { get; set; }
        public double WatchPercentage =>
            DurationSeconds > 0 ? (double)PositionSeconds / DurationSeconds : 0;
        public DateTime LastUpdated { get; set; }
    }
}