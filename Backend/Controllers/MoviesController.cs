using Backend.DTOs.Movies;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoviesController(IMovieService movieService) : ControllerBase
{
    private MoviePublicDto MapToMoviePublicDto(Movie movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        Description = movie.Description,
        ReleaseYear = movie.ReleaseYear,
        RuntimeMinutes = movie.RuntimeMinutes,
        VideoUrl = movie.VideoUrl ?? string.Empty,
        ThumbnailUrl = movie.ThumbnailUrl ?? string.Empty,
        BackdropUrl = movie.BackdropUrl ?? string.Empty,
        AgeRating = movie.AgeRating ?? string.Empty,
        IsOriginal = movie.IsOriginal,
        AverageRating = movie.AverageRating,
        Genres = movie.MovieGenres?.Select(mg => mg.Genre.Name).ToList() ?? [],
        Categories = movie.MovieCategories?.Select(mc => mc.Category.Name).ToList() ?? []
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MoviePublicDto>>> GetMovies()
    {
        var movies = await movieService.GetAllAsync();
        return Ok(movies.Select(MapToMoviePublicDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MoviePublicDto>> GetMovie(int id)
    {
        var movie = await movieService.GetByIdAsync(id);
        return movie == null ? NotFound() : Ok(MapToMoviePublicDto(movie));
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MoviePublicDto>>> GetTrending([FromQuery] int count = 10)
    {
        var movies = await movieService.GetTrendingAsync(count);
        return Ok(movies.Select(MapToMoviePublicDto));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MoviePublicDto>>> Search([FromQuery] string q)
    {
        var movies = await movieService.SearchAsync(q);
        return Ok(movies.Select(MapToMoviePublicDto));
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<MoviePublicDto>>> Filter(
        [FromQuery] int? genreId,
        [FromQuery] int? categoryId,
        [FromQuery] int? year,
        [FromQuery] string? director)
    {
        var movies = await movieService.FilterAsync(genreId, categoryId, year, director);
        return Ok(movies.Select(MapToMoviePublicDto));
    }

    [HttpGet("{id}/recommendations")]
    public async Task<ActionResult<IEnumerable<MoviePublicDto>>> GetRecommendations(int id)
    {
        var movies = await movieService.GetRecommendationsAsync(id);
        return Ok(movies.Select(MapToMoviePublicDto));
    }

    [HttpGet("{id}/next")]
    public async Task<ActionResult<MoviePublicDto>> GetNext(int id)
    {
        var next = await movieService.GetNextEpisodeAsync(id);
        return next == null ? NoContent() : Ok(MapToMoviePublicDto(next));
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> SaveProgress(int id, [FromBody] double seconds)
    {
        // Replace '1' with User.FindFirstValue(ClaimTypes.NameIdentifier) in production
        await movieService.SyncPlaybackProgressAsync(1, id, seconds);
        return NoContent();
    }
}