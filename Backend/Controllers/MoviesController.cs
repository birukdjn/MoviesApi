using Backend.Data;
using Backend.DTOs.Movies;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MoviesController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private MoviePublicDto MapToMoviePublicDto(Movie movie)
        {
            return new MoviePublicDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                RuntimeMinutes = movie.RuntimeMinutes,
                ThumbnailUrl = movie.ThumbnailUrl ?? string.Empty,
                BackdropUrl = movie.BackdropUrl ?? string.Empty,
                AgeRating = movie.AgeRating ?? string.Empty,
                IsOriginal = movie.IsOriginal,
                AverageRating = movie.AverageRating,

                Genres = movie.MovieGenres?.Select(mg => mg.Genre.Name).ToList() ?? [],
                Categories = movie.MovieCategories?.Select(mc => mc.Category.Name).ToList() ?? []
            };
        }

        [HttpGet]
        [Authorize(Roles ="Admin,User")]
        public async Task<ActionResult<IEnumerable<MoviePublicDto>>> GetMovies()
        {
            var movies = await _context.Movies
           .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
           .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
           .ToListAsync();

            if (movies.Count==0)
                return NotFound("No movies found.");

            var movieDtos = movies.Select(MapToMoviePublicDto).ToList(); 
            return Ok(movieDtos);

        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<MoviePublicDto>>> SearchMovies(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty.");

            var movies = await _context.Movies
                .Include(m => m.MovieGenres!)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCategories!)
                    .ThenInclude(mc => mc.Category)
                .Where(m =>
                    m.Title.Contains(query) ||
                    m.Director.Contains(query) ||
                    m.MovieGenres.Any(mg => mg.Genre.Name.Contains(query)) ||
                    m.MovieCategories.Any(mc => mc.Category.Name.Contains(query))
                    ).ToListAsync();

                    var movieDtos = movies.Select(MapToMoviePublicDto).ToList();
                    return Ok(movieDtos);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<MoviePublicDto>>> GetPagedMovies(int page = 1, int pageSize = 10) 
        {
            var movies = await _context.Movies
                .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); 

            var movieDtos = movies.Select(MapToMoviePublicDto).ToList();
            return Ok(movieDtos);
        }

        [HttpGet("filter")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<MoviePublicDto>>> FilterMovies(int? genreId, int? categoryId, int? year, string? director)
        {
            var query = _context.Movies.AsQueryable();

            if (genreId.HasValue)
                query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));

            if (categoryId.HasValue)
                query = query.Where(m => m.MovieCategories.Any(mc => mc.CategoryId == categoryId.Value));

            if (year.HasValue)
                query = query.Where(m => m.ReleaseYear == year.Value);

            if (!string.IsNullOrEmpty(director))
                query = query.Where(m => m.Director.Contains(director));

            var filteredMovies = await query 
                .Include(m => m.MovieGenres!)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCategories!)
                    .ThenInclude(mc => mc.Category)
                .ToListAsync();

            var movieDtos = filteredMovies.Select(MapToMoviePublicDto).ToList();
            return Ok(movieDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<MoviePublicDto>> GetMovie(int id)
        {
          var movie = await _context.Movies
         .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
         .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
         .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound($"Movie with id {id} not found.");

            return Ok(MapToMoviePublicDto(movie));
        }

        [HttpGet("recommend/{movieId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<MoviePublicDto>>> RecommendMovies(int movieId) 
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null)
                return NotFound("Movie not found.");

            var sharedCategoryIds = movie.MovieCategories.Select(mc => mc.CategoryId).ToList();

            var recommended = await _context.Movies
                .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
                .Where(m => m.Id != movieId &&
                            m.MovieCategories.Any(mc => sharedCategoryIds.Contains(mc.CategoryId)))
                .Take(5)
                .ToListAsync();

            var movieDtos = recommended.Select(MapToMoviePublicDto).ToList();
            return Ok(movieDtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MoviePublicDto>> AddMovie([FromBody] MovieCreateDto dto)
        {

            var movie = new Movie
            {
                
                Title = dto.Title,
                Description = dto.Description,
                ReleaseYear = dto.ReleaseYear,
                RuntimeMinutes = dto.RuntimeMinutes,
                ThumbnailUrl = dto.ThumbnailUrl ?? string.Empty,
                BackdropUrl = dto.BackdropUrl ?? string.Empty,
                // Add YouTube fields
                VideoUrl = dto.VideoUrl ?? string.Empty,
                YoutubeId = dto.YoutubeId ?? string.Empty,
                AgeRating = dto.AgeRating ?? string.Empty,
                IsOriginal = dto.IsOriginal,
    
            };

            foreach (var categoryId in dto.CategoryIds)
            {
                var categiroy = await _context.Categories.FindAsync(categoryId);
                if (categiroy ==null)
                    return BadRequest($"Invalid category ID: {categoryId}.");

                movie.MovieCategories.Add(new MovieCategory { CategoryId = categoryId });

            }
            foreach (var genreId in dto.GenreIds)
            {
                var genre = await _context.Genres.FindAsync(genreId);
                if (genre == null)
                    return BadRequest($"Invalid genre ID: {genreId}.");

                movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            await _context.Entry(movie)
        .Collection(m => m.MovieCategories).LoadAsync();
            await _context.Entry(movie)
                .Collection(m => m.MovieGenres).LoadAsync();

        var createdMovie = await _context.Movies
        .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
        .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
        .FirstAsync(m => m.Id == movie.Id);

        var movieDto = MapToMoviePublicDto(createdMovie);

            return CreatedAtAction(nameof(GetMovie), new { id = movieDto.Id }, movieDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] MovieCreateDto dto)
        {
            int currentYear = DateTime.Now.Year;

            var existing = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieCategories)
                .FirstOrDefaultAsync(m => m.Id == id);

            
            if (existing == null)
                return NotFound($"Movie with id {id} not found.");

            if (dto.ReleaseYear > currentYear || dto.ReleaseYear < 1900)
                return BadRequest("Release year must be between 1900 and the current year.");

            
            existing.Title = dto.Title;
            existing.ReleaseYear = dto.ReleaseYear;
            existing.Director = dto.Director;
            existing.Description = dto.Description;

            existing.MovieGenres.Clear();

            foreach (var genreId in dto.GenreIds)
            {
                var genreExists = await _context.Genres.AnyAsync(g => g.Id == genreId);
                if (!genreExists)
                    return BadRequest($"Invalid genre ID: {genreId}.");

                existing.MovieGenres.Add(new MovieGenre { MovieId = id, GenreId = genreId });
            }

            existing.MovieCategories.Clear();
            foreach (var categoryId in dto.CategoryIds)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
                if (!categoryExists)
                    return BadRequest($"Invalid category ID: {categoryId}.");

                existing.MovieCategories.Add(new MovieCategory { MovieId = id, CategoryId = categoryId });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound($"Movie with id {id} not found.");

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
