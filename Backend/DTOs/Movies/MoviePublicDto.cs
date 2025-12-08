namespace Backend.DTOs.Movies
{
    public class MoviePublicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int RuntimeMinutes { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string BackdropUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string YoutubeId { get; set; } = string.Empty;
        public string AgeRating { get; set; } = string.Empty;
        public bool IsOriginal { get; set; }
        public double AverageRating { get; set; }

        public List<string> Genres { get; set; } = [];
        public List<string> Categories { get; set; } = [];
    }
}