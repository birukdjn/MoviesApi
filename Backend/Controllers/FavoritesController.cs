using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private int GetCurrentProfileId()
        {
            
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;

            if (profileIdClaim != null && int.TryParse(profileIdClaim, out int profileId))
            {
                return profileId;
            }
            return 0; 
        }

        // Add a movie to favorites
        [HttpPost("{movieId}")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult AddToFavorites(int movieId)
        {
            int profileId = GetCurrentProfileId();
            if (profileId == 0) return Unauthorized("Profile not selected or token invalid.");

            if (!_context.Movies.Any(m => m.Id == movieId))
                return NotFound("Movie not found.");

            if (_context.Favorites.Any(f => f.ProfileId == profileId && f.MovieId == movieId))
                return BadRequest("Movie already in favorites.");

            var favorite = new Favorite
            {
                ProfileId = profileId,
                MovieId = movieId
            };
            _context.Favorites.Add(favorite);
            _context.SaveChanges();

            return Ok("Added to favorites");
        }
           
        [HttpGet]
        [Authorize(Roles = "Admin,User")]

        public IActionResult GetFavorites()
        {
            var profileId = GetCurrentProfileId();
            if (profileId == 0) return Unauthorized("Profile not selected or token invalid.");

            var favorites = _context.Favorites
                .Where(f => f.ProfileId == profileId)
                .Include(f => f.Movie)
                .Select(f => f.Movie)
                .ToList();

            return Ok(favorites);
        }


        [HttpDelete("{movieId}")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult RemoveFavorite(int movieId)
        {
            var profileId = GetCurrentProfileId();
            if (profileId == 0) return Unauthorized("Profile not selected or token invalid.");

            var favorite = _context.Favorites
                .FirstOrDefault(f => f.ProfileId == profileId && f.MovieId == movieId);
            if (favorite == null) return NotFound("Favorite not found.");

            _context.Favorites.Remove(favorite);
            _context.SaveChanges();

            return Ok("Removed from favorites");
        }
    }
}
