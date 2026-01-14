using Backend.DTOs.Playback;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaybackController(IMovieService movieService) : ControllerBase
{
    private int GetCurrentProfileId() =>
        int.Parse(User.FindFirst("ProfileId")?.Value ?? "0");

    [HttpPost("update")]
    public async Task<IActionResult> UpdatePosition([FromBody] PlaybackUpdateDto dto)
    {
        await movieService.UpdatePlaybackPositionAsync(GetCurrentProfileId(), dto);
        return NoContent();
    }

    [HttpGet("continue")]
    public async Task<ActionResult<IEnumerable<PlaybackDisplayDto>>> GetContinueWatching()
    {
        var positions = await movieService.GetContinueWatchingAsync(GetCurrentProfileId());

        var dtos = positions.Select(p => new PlaybackDisplayDto
        {
            MovieId = p.MovieId,
            Title = p.Movie.Title,
            ThumbnailUrl = p.Movie.ThumbnailUrl,
            PositionSeconds = p.PositionSeconds,
            DurationSeconds = p.DurationSeconds,
            LastUpdated = p.LastUpdated
        });

        return Ok(dtos);
    }
}