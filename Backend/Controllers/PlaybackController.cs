using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs.Playback;

namespace Backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PlaybackController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private int GetCurrentProfileId()
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            return int.Parse(profileIdClaim ?? "0");
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdatePosition([FromBody] PlaybackUpdateDto dto)
        {
            int profileId = GetCurrentProfileId();

            var position = await _context.PlaybackPositions
                .FirstOrDefaultAsync(p => p.ProfileId == profileId && p.MovieId == dto.MovieId);

            if (position == null)
            {
                position = new PlaybackPosition
                {
                    ProfileId = profileId,
                    MovieId = dto.MovieId,
                    TotalDurationInSeconds = dto.TotalDurationInSeconds
                };
                _context.PlaybackPositions.Add(position);
            }

            position.PositionInSeconds = dto.PositionInSeconds;
            position.LastWatchedDate = DateTime.UtcNow;

            if (dto.PositionInSeconds >= dto.TotalDurationInSeconds - 60)
            {
                _context.PlaybackPositions.Remove(position);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("continue")]
        public async Task<ActionResult<IEnumerable<PlaybackDisplayDto>>> GetContinueWatchingList()
        {
            int profileId = GetCurrentProfileId();

            var positions = await _context.PlaybackPositions
                .Where(p => p.ProfileId == profileId)
                .Include(p => p.Movie)
                .OrderByDescending(p => p.LastWatchedDate)
                .Take(10)
                .ToListAsync();

            var dtos = positions.Select(p => new PlaybackDisplayDto
            {
                MovieId = p.MovieId,
                Title = p.Movie.Title,
                ThumbnailUrl = p.Movie.ThumbnailUrl,
                PositionInSeconds = p.PositionInSeconds,
                TotalDurationInSeconds = p.TotalDurationInSeconds,
                LastWatchedDate = p.LastWatchedDate
            }).ToList();

            return Ok(dtos);
        }
    }
}