﻿using Backend.DTOs.Movies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContentController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        private int GetCurrentProfileId()
        {
            var profileIdClaim = User.FindFirst("ProfileId")?.Value;
            return int.Parse(profileIdClaim ?? "0");
        }

        [HttpGet("{movieId}")]
        public async Task<ActionResult<MoviePublicDto>> GetMovieDetail(int movieId)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null) return NotFound();

            var dto = new MoviePublicDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                RuntimeMinutes = movie.RuntimeMinutes,
                ThumbnailUrl = movie.ThumbnailUrl,
                BackdropUrl = movie.BackdropUrl,
                VideoUrl = movie.VideoUrl,
                YoutubeId = movie.YoutubeId,
                AgeRating = movie.AgeRating,
                AverageRating = movie.AverageRating,
                IsOriginal = movie.IsOriginal,
                Genres = [.. movie.MovieGenres.Select(mg => mg.Genre.Name)],
                Categories = [.. movie.MovieCategories.Select(mc => mc.Category.Name)]
            };

            return Ok(dto);
        }

        [HttpGet("home")]
        public async Task<ActionResult<HomeFeedDto>> GetHomeFeed()
        {
            var categoryRows = await _context.Categories
                .Take(5)
                .Select(c => new HomeFeedRowDto
                {
                    RowTitle = c.Name,
                    Movies = c.MovieCategories
                        .Take(10)
                        .Select(mc => new MoviePublicDto
                        {
                            Id = mc.Movie.Id,
                            Title = mc.Movie.Title,
                            Description = mc.Movie.Description,
                            ReleaseYear = mc.Movie.ReleaseYear,
                            RuntimeMinutes = mc.Movie.RuntimeMinutes,
                            ThumbnailUrl = mc.Movie.ThumbnailUrl,
                            BackdropUrl = mc.Movie.BackdropUrl,
                            VideoUrl = mc.Movie.VideoUrl,
                            YoutubeId = mc.Movie.YoutubeId,
                            AgeRating = mc.Movie.AgeRating,
                            IsOriginal = mc.Movie.IsOriginal,
                            AverageRating = mc.Movie.AverageRating,
                            Genres = mc.Movie.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
                            Categories = mc.Movie.MovieCategories.Select(mcat => mcat.Category.Name).ToList()
                        }).ToList()
                }).ToListAsync();

            var homeFeed = new HomeFeedDto
            {
                CategoryRows = categoryRows
            };

            return Ok(homeFeed);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MoviePublicDto>>> SearchMovies([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var movies = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieCategories).ThenInclude(mc => mc.Category)
                .Where(m => EF.Functions.Like(m.Title, $"%{query}%") || EF.Functions.Like(m.Description, $"%{query}%"))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = movies.Select(m => new MoviePublicDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                RuntimeMinutes = m.RuntimeMinutes,
                ThumbnailUrl = m.ThumbnailUrl,
                BackdropUrl = m.BackdropUrl,
                VideoUrl = m.VideoUrl,
                YoutubeId = m.YoutubeId,
                AgeRating = m.AgeRating,
                IsOriginal = m.IsOriginal,
                AverageRating = m.AverageRating,
                Genres = [.. m.MovieGenres.Select(mg => mg.Genre.Name)],
                Categories = [.. m.MovieCategories.Select(mc => mc.Category.Name)]
            }).ToList();

            return Ok(dtos);
        }
    }
}