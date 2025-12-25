using Backend.Data;
using Backend.DTOs.Admin;
using Backend.DTOs.Users;
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
        [ProducesResponseType(typeof(AdminStatsDto),200)]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var stats = await _context.Users
                .Select(stat => new AdminStatsDto
                {
                    Users = new UserStats
                    {
                        TotalUsers = _context.Users.Count(),
                        ActiveUsers = _context.Users.Count(u => u.IsActive),
                        TotalProfiles = _context.Profiles.Count()
                    },
                    Content = new ContentStats
                    {
                        TotalMovies = _context.Movies.Count(),
                        TotalCategories = _context.Categories.Count(),
                        TotalGenres = _context.Genres.Count()
                    },
                    Engagement = new EngagementStats
                    {
                        TotalFavorites = _context.Favorites.Count(),
                        TotalRatings = _context.Ratings.Count(),
                        AverageRating = _context.Ratings.Average(r => (double?)r.Score) ?? 0
                    },
                    Subscriptions = new SubscriptionStats
                    {
                        TotalSubscriptions = _context.Subscriptions.Count()
                    }
                }).FirstOrDefaultAsync() ?? new AdminStatsDto();
            return Ok(stats);
        }


        [HttpGet("users")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
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