using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs.Ratings;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RatingController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private int GetCurrentProfileId()
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            return int.Parse(profileIdClaim ?? "0");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRating([FromBody] RatingSubmitDto dto)
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;

            if (string.IsNullOrEmpty(profileIdClaim))
            {
                return Forbid("Profile not selected. Please select a profile first.");
            }
            int profileId = GetCurrentProfileId();

            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.ProfileId == profileId && r.MovieId == dto.MovieId);

            if (rating == null)
            {
                rating = new Rating
                {
                    ProfileId = profileId,
                    MovieId = dto.MovieId
                };
                _context.Ratings.Add(rating);
            }

            rating.Score = dto.Score;
            rating.RatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await UpdateMovieAverageRating(dto.MovieId);

            return NoContent(); 
        }

        [HttpGet("{movieId}")]
        public async Task<ActionResult<RatingDisplayDto>> GetRatingInfo(int movieId)
        {
            int profileId = GetCurrentProfileId();

            var profileRating = await _context.Ratings
                .Where(r => r.ProfileId == profileId && r.MovieId == movieId)
                .Select(r => (int?)r.Score)
                .FirstOrDefaultAsync();

            var movie = await _context.Movies
                .Select(m => new { m.Id, m.AverageRating })
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null) return NotFound();

            var dto = new RatingDisplayDto
            {
                CurrentProfileScore = profileRating,
                AverageCommunityRating = movie.AverageRating
            };

            return Ok(dto);
        }
        private async Task UpdateMovieAverageRating(int movieId)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
            if (movie == null) return;

            double newAverage = await _context.Ratings
                .Where(r => r.MovieId == movieId)
                .AverageAsync(r => r.Score);

            movie.AverageRating = Math.Round(newAverage, 2);
            await _context.SaveChangesAsync();
        }
    }
}