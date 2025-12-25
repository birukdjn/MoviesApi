namespace Backend.DTOs.Favorites
{
    public class FavoriteMovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RuntimeMinutes { get; set; }
        public int ReleaseYear { get; set; }
    }
}
