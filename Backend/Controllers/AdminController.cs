using Backend.Data;
using Backend.DTOs.Admin;
using Backend.DTOs.Users;
using Backend.Enums;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(AppDbContext context, IPasswordService  passwordService, IEmailSender emailSender) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IEmailSender _emailSender = emailSender;

        [HttpGet("stats")]
        [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var stats = new AdminStatsDto
            {

                Users = new UserStats
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                    TotalProfiles = await _context.Profiles.CountAsync()
                },
                Content = new ContentStats
                {
                    TotalMovies = await _context.Movies.CountAsync(m => m.MovieCategories.Any(c => c.Category.Name == "Movie")),
                    TotalSeries = await _context.Movies.CountAsync(m => m.MovieCategories.Any(c => c.Category.Name == "Series")),
                    TotalEpisodes = await _context.Movies.CountAsync(m => m.MovieCategories.Any(c => c.Category.Name == "Episode")),

                    TotalCategories = await _context.Categories.CountAsync(),
                    TotalGenres = await _context.Genres.CountAsync(),
                    TotalDirectors = await _context.Movies.Select(m => m.Director).Distinct().CountAsync(),
                    TotalLanguages = await _context.Movies.Select(m => m.Language).Distinct().CountAsync()
                },
                Engagement = new EngagementStats
                {
                    TotalFavorites = await _context.Favorites.CountAsync(),
                    TotalRatings = await _context.Ratings.CountAsync(),
                    AverageRating = await _context.Ratings.AnyAsync()
                ? await _context.Ratings.AverageAsync(r => (double)r.Score)
                : 0
                },

                Subscriptions = new SubscriptionStats
                {
                    TotalSubscriptions = await _context.Subscriptions.CountAsync(),
                    BasicSubscriptions = await _context.Subscriptions.CountAsync(s => s.Plan == SubscriptionPlan.Basic),
                    StandardSubscriptions = await _context.Subscriptions.CountAsync(s => s.Plan == SubscriptionPlan.Standard),
                    PremiumSubscriptions = await _context.Subscriptions.CountAsync(s => s.Plan == SubscriptionPlan.Premium)
                },

                Revenue = new RevenueStats
                {
                    TotalRevenue = await _context.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active).SumAsync(s => s.Price),
                    BasicRevenue = await _context.Subscriptions.Where(s => s.Plan == SubscriptionPlan.Basic && s.Status == SubscriptionStatus.Active).SumAsync(s => s.Price),
                    StandardRevenue = await _context.Subscriptions.Where(s => s.Plan == SubscriptionPlan.Standard && s.Status == SubscriptionStatus.Active).SumAsync(s => s.Price),
                    PremiumRevenue = await _context.Subscriptions.Where(s => s.Plan == SubscriptionPlan.Premium && s.Status == SubscriptionStatus.Active).SumAsync(s => s.Price),
                    MonthlyRevenue = await _context.Subscriptions
                    .Where(s => s.Status == SubscriptionStatus.Active && s.StartDate >= DateTime.UtcNow.AddDays(-30))
                    .SumAsync(s => s.Price)
                }

            };
            return Ok(stats);
        }


        [HttpGet("users")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new AdminUserViewDto
                {
                   Id= u.Id,
                   Username= u.Username,
                   Name=  u.Name,
                   Email = u.Email,
                   Phone = u.Phone,
                   Role = u.Role,
                   CreatedAt = u.CreatedAt,
                   IsActive = u.IsActive,
                   IsSubscribed =  u.IsSubscribed,
                   LastLoginIp =  u.LastLoginIp,
                   Avatar = u.Avatar,
                    Profiles = string.Join(", ", u.Profiles.Select(p => p.Name))
                }).ToListAsync();
            return Ok(users);
        }

        private static string GenerateRandomPassword(int length = 12)
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length));
        }


        [HttpPost("create-admin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<object>> CreateUserByAdmin([FromBody] UserCreateByAdminDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already exists.");

            // 1. Generate Temporary Password
            string tempPassword = GenerateRandomPassword();
            string passwordHash = _passwordService.HashPassword(tempPassword);

            // 2. Create Admin Record
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Phone = dto.Phone ?? string.Empty,
                PasswordHash = passwordHash,
                Role = "Admin"

            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
                
                var emailBody = $"<h3>Account Created Successfully!</h3>" +
                                $"<p>An administrator has created an account for you with the following details:</p>" +
                                $"<ul>" +
                                $"<li><strong>Username:</strong> {user.Username}</li>" +
                                $"<li><strong>Temporary Password:</strong> <code>{tempPassword}</code></li>" +
                                $"</ul>" +
                                $"<p>Please log in immediately and change your password for security purposes.</p>";

                var message = new Message(
                    [user.Email],
                    "Your New Account Credentials",
                    emailBody
                );

                _emailSender.SendEmail(message);

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    Message = "User created and credentials sent."
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error Creating User or Sending Email. Please try again");

            }
        }

    }
}