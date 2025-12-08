using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Ratings
{
    public class RatingSubmitDto
    {
        [Required]
        public int MovieId { get; set; }

        [Required]
        [Range(1, 5)] 
        public int Score { get; set; }
    }
}