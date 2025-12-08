namespace Backend.DTOs.Movies
{
    public record MovieDto(int Id, string Title, DateTime ReleaseYear, string Description,string Director, string Genre, double? AverageRating);
    
    
}
