using Backend.DTOs.Playback;
using Backend.Models;

namespace Backend.Services.Interfaces;

public interface IMovieService
{
    // Standard CRUD
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(int id);
    Task<Movie> CreateAsync(Movie movie);
    Task UpdateAsync(int id, Movie movie);
    Task DeleteAsync(int id);

    // Netflix Specific Logic
    Task<IEnumerable<Movie>> GetTrendingAsync(int count);
    Task<IEnumerable<Movie>> SearchAsync(string query);
    Task<IEnumerable<Movie>> FilterAsync(int? genreId, int? categoryId, int? year, string? director);
    Task<IEnumerable<Movie>> GetRecommendationsAsync(int movieId);
    Task<Movie?> GetNextEpisodeAsync(int currentMovieId);

    // Playback Logic
    Task SyncPlaybackProgressAsync(int profileId, int movieId, double seconds);
    Task<IEnumerable<PlaybackPosition>> GetContinueWatchingAsync(int profileId);
    Task UpdatePlaybackPositionAsync(int profileId, PlaybackUpdateDto dto);
}