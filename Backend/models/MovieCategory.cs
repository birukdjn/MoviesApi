namespace Backend.Models
{
    public class MovieCategory
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
