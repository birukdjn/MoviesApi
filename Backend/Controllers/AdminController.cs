using Backend.Data;
using Backend.DTOs.Users;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalProfiles = await _context.Profiles.CountAsync();
            var totalMovies = await _context.Movies.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalGenres = await _context.Genres.CountAsync();
            var totalFavorites = await _context.Favorites.CountAsync();
            var totalRatings = await _context.Ratings.CountAsync();
            var averageRating = await _context.Ratings.AverageAsync(r => (double?)r.Score) ?? 0;
            var totalPlaybackPositions = await _context.PlaybackPositions.CountAsync();
            var totalSubscriptions = await _context.Subscriptions.CountAsync();

            // Fix for IsActive
            var activeSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.EndDate == null || s.EndDate > DateTime.UtcNow);

            var planDistribution = await _context.Subscriptions
                .GroupBy(s => s.Plan)
                .Select(g => new { Plan = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var stats = new
            {
                users = new
                {
                    totalUsers,
                    activeUsers,
                    totalProfiles
                },
                content = new
                {
                    totalMovies,
                    totalCategories,
                    totalGenres
                },
                engagement = new
                {
                    totalFavorites,
                    totalRatings,
                    averageRating,
                    totalPlaybackPositions
                },
                subscriptions = new
                {
                    totalSubscriptions,
                    activeSubscriptions,
                    planDistribution
                }
            };

            return Ok(stats);
        }
        [HttpGet("users")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Name,
                    u.Email,
                    u.Phone,
                    u.Role,
                    u.CreatedAt,
                    u.IsActive,
                    u.IsSubscribed,
                    u.LastLoginIp,
                    u.Avatar,

                    u.Profiles,
                    u.Subscriptions
                })
                .ToListAsync();
            return Ok(users);


        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var random = new Random();
            var password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(password);
        }

        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUserByAdmin([FromBody] UserCreateByAdminDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return BadRequest("Username or Email already exists.");

            // 1. Generate Temporary Password
            string tempPassword = GenerateRandomPassword();
            string passwordHash = _passwordService.HashPassword(tempPassword);

            // 2. Create User Record
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Phone = dto.Phone ?? string.Empty,
                PasswordHash = passwordHash,
                Role = "Admin",
                Avatar = "default_avatar.png"


            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            // 4. Send Email with Temporary Password
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending temporary password email to {user.Email}: {ex.Message}");

            }

            return Ok(new
            {
                message = $"User {user.Username} created and temporary password sent to {user.Email}.",
                userId = user.Id,
                username = user.Username,
                role = user.Role

            });
        }

    }
}

