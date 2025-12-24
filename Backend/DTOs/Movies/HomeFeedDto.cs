namespace Backend.DTOs.Movies;

public class HomeFeedDto
{
    public List<HomeFeedRowDto> CategoryRows { get; set; } = new();
}

public class HomeFeedRowDto
{
    public string RowTitle { get; set; } = string.Empty;
    public List<MoviePublicDto> Movies { get; set; } = [];
}