namespace Backend.DTOs.Playback
{
    public class PlaybackDisplayDto
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int PositionInSeconds { get; set; }
        public int TotalDurationInSeconds { get; set; }
        public double WatchPercentage =>
            TotalDurationInSeconds > 0 ? (double)PositionInSeconds / TotalDurationInSeconds : 0;
        public DateTime LastWatchedDate { get; set; }
    }
}