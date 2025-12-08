using Backend.DTOs.Movies;

namespace Backend.DTOs.Favorites
{
    public class FavoriteDisplayDto
    {
        public MoviePublicDto Movie { get; set; } = new();
        public DateTime AddedDate { get; set; }
    }
}