using Backend.Data;
using Backend.DTOs.Playback;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Implementations;

public class MovieService(AppDbContext context) : IMovieService
{
    // 1. Standard CRUD
    public async Task<IEnumerable<Movie>> GetAllAsync() =>
        await context.Movies
            .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Movie?> GetByIdAsync(int id) =>
        await context.Movies
            .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Movie> CreateAsync(Movie movie)
    {
        context.Movies.Add(movie);
        await context.SaveChangesAsync();
        return movie;
    }

    public async Task UpdateAsync(int id, Movie movie)
    {
        var existing = await context.Movies.FindAsync(id);
        if (existing == null) return;

        // Efficiently updates all scalar properties at once
        context.Entry(existing).CurrentValues.SetValues(movie);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await context.Movies.FindAsync(id);
        if (movie != null)
        {
            context.Movies.Remove(movie);
            await context.SaveChangesAsync();
        }
    }

    // 2. Search & Filtering (The "Discovery" Engine)
    public async Task<IEnumerable<Movie>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        return await context.Movies
            .Where(m => m.Title.Contains(query) || m.Director.Contains(query))
            .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>> FilterAsync(int? genreId, int? categoryId, int? year, string? director)
    {
        var dbQuery = context.Movies.AsQueryable();

        if (genreId.HasValue)
            dbQuery = dbQuery.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));
        if (categoryId.HasValue)
            dbQuery = dbQuery.Where(m => m.MovieCategories.Any(mc => mc.CategoryId == categoryId));
        if (year.HasValue)
            dbQuery = dbQuery.Where(m => m.ReleaseYear == year);
        if (!string.IsNullOrEmpty(director))
            dbQuery = dbQuery.Where(m => m.Director.Contains(director));

        return await dbQuery
            .Include(m => m.MovieGenres!).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCategories!).ThenInclude(mc => mc.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    // 3. Netflix Specific Logic
    public async Task<IEnumerable<Movie>> GetTrendingAsync(int count) =>
        await context.Movies
            .OrderByDescending(m => m.AverageRating)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Movie>> GetRecommendationsAsync(int movieId)
    {
        var movie = await context.Movies
            .Include(m => m.MovieCategories)
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null) return [];

        var categoryIds = movie.MovieCategories.Select(c => c.CategoryId);

        return await context.Movies
            .Where(m => m.Id != movieId && m.MovieCategories.Any(c => categoryIds.Contains(c.CategoryId)))
            .Take(6)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Movie?> GetNextEpisodeAsync(int currentMovieId)
    {
        var current = await context.Movies.FindAsync(currentMovieId);
        if (current == null || current.SeriesId == null) return null;

        return await context.Movies
            .Where(m => m.SeriesId == current.SeriesId && m.Order > current.Order)
            .OrderBy(m => m.Order)
            .FirstOrDefaultAsync();
    }

    // 4. Playback Logic
    public async Task SyncPlaybackProgressAsync(int profileId, int movieId, double seconds)
    {
        var progress = await context.PlaybackPositions
            .FirstOrDefaultAsync(p => p.ProfileId == profileId && p.MovieId == movieId);

        if (progress == null)
        {
            context.PlaybackPositions.Add(new PlaybackPosition
            {
                ProfileId = profileId,
                MovieId = movieId,
                PositionSeconds = seconds,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            progress.PositionSeconds = seconds;
            progress.LastUpdated = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }
    public async Task<IEnumerable<PlaybackPosition>> GetContinueWatchingAsync(int profileId)
    {
        return await context.PlaybackPositions
            .Where(p => p.ProfileId == profileId)
            .Include(p => p.Movie)
            .OrderByDescending(p => p.LastUpdated)
            .Take(10)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdatePlaybackPositionAsync(int profileId, PlaybackUpdateDto dto)
    {
        var position = await context.PlaybackPositions
            .FirstOrDefaultAsync(p => p.ProfileId == profileId && p.MovieId == dto.MovieId);

        // If near the end (60s left), remove from "Continue Watching"
        if (dto.PositionInSeconds >= dto.TotalDurationInSeconds - 60)
        {
            if (position != null)
            {
                context.PlaybackPositions.Remove(position);
                await context.SaveChangesAsync();
            }
            return;
        }

        if (position == null)
        {
            context.PlaybackPositions.Add(new PlaybackPosition
            {
                ProfileId = profileId,
                MovieId = dto.MovieId,
                PositionSeconds = dto.PositionInSeconds,
                DurationSeconds = dto.TotalDurationInSeconds,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            position.PositionSeconds = dto.PositionInSeconds;
            position.LastUpdated = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }
}