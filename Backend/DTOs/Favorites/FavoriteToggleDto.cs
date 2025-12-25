using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Favorites
{
    // Used for POST /api/Favorites/toggle
    public class FavoriteToggleDto
    {
        [Required]
        public int MovieId { get; set; }
    }
}