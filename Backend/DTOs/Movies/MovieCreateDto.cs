using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Movies
{
    public class MovieCreateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title must not exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one Genre ID is required")]
        public List<int> GenreIds { get; set; } = [];

        [Required(ErrorMessage = "Release Year is required")]
        public int ReleaseYear { get; set; }

        [Required(ErrorMessage = "Director is required")]
        public string Director { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one Category ID is required")]

        public int RuntimeMinutes { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string BackdropUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string YoutubeId { get; set; } = string.Empty;
        public string AgeRating { get; set; } = "TV-MA";
        public bool IsOriginal { get; set; }

        public List<int> CategoryIds { get; set; } = [];

    }
}

