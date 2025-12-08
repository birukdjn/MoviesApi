
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Backend.Models
{
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string Director { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public int RuntimeMinutes { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string BackdropUrl { get; set; } = string.Empty;

        public string VideoUrl { get; set; } = string.Empty;
        public string YoutubeId { get; set; } = string.Empty;
        public string AgeRating { get; set; } = "TV-MA";
        public bool IsOriginal { get; set; } = false;
        public double AverageRating { get; set; } = 0.0;
        public ICollection<Rating> Ratings { get; set; } = [];
        public ICollection<MovieCategory> MovieCategories { get; set; } = [];
        public ICollection<MovieGenre> MovieGenres { get; set; } = [];
        public ICollection<PlaybackPosition> PlaybackPositions { get; set; } = [];
        public ICollection<Favorite> Favorites { get; set; } = [];
  
    }
}
