using Backend.Data;
using Backend.DTOs.Favorites;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class FavoritesController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private int GetCurrentProfileId()
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            return int.TryParse(profileIdClaim, out int profileId) ? profileId : 0;
        }

        [HttpPost("{movieId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddToFavorites(int movieId)
        {
            int profileId = GetCurrentProfileId();
            if (profileId == 0) return Unauthorized("Profile not selected or token invalid.");

            if (!await _context.Movies.AnyAsync(m => m.Id == movieId))
                return NotFound("Movie not found.");

            if (await _context.Favorites.AnyAsync(f => f.ProfileId == profileId && f.MovieId == movieId))
                return BadRequest("Movie already in favorites.");

            var favorite = new Favorite
            {
                ProfileId = profileId,
                MovieId = movieId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok("Added to favorites");
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FavoriteMovieDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<object>>> GetFavorites()
        {
            var profileId = GetCurrentProfileId();
            if (profileId == 0) return Unauthorized("Profile not selected or token invalid.");

            var favorites = await _context.Favorites
                .Where(f => f.ProfileId == profileId)
                .Select(f => new FavoriteMovieDto
                {
                   Id = f.Movie.Id,
                   Title = f.Movie.Title,
                   Description = f.Movie.Description,
                   RuntimeMinutes = f.Movie.RuntimeMinutes,
                   ReleaseYear = f.Movie.ReleaseYear,
                    
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpDelete("{movieId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveFavorite(int movieId)
        {
            var profileId = GetCurrentProfileId();
            if (profileId == 0) return Unauthorized("Profile not selected or token invalid.");

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ProfileId == profileId && f.MovieId == movieId);

            if (favorite == null) return NotFound("Favorite not found.");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok("Removed from favorites");
        }
    }
}