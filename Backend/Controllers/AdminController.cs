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
    public class AdminController(AppDbContext context, IPasswordService  passwordService, IEmailSender emailSender, ILogger<AdminController> logger) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly ILogger<AdminController> _logger = logger;

        [HttpGet("stats")]
        [ProducesResponseType(typeof(AdminStatsDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            // 1. Get User and Profile stats in ONE trip
            var userStats = await _context.Users
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(u => u.IsActive)
                    
                }).FirstOrDefaultAsync() ?? new { Total = 0, Active = 0 };
            var totalProfiles = await _context.Profiles.CountAsync();

            var contentStats = await _context.Movies
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Movies = g.Count(m => m.MovieCategories.Any(c => c.Category.Name == "Movie")),
                    Series = g.Count(m => m.MovieCategories.Any(c => c.Category.Name == "Series")),
                    Episodes = g.Count(m => m.MovieCategories.Any(c => c.Category.Name == "Episode")),
                    Languages = g.Select(m => m.Language).Distinct().Count(),
                    Directors = g.Select(m => m.Director).Distinct().Count()
                }).FirstOrDefaultAsync() ?? new { Movies = 0, Series = 0, Episodes = 0, Languages = 0, Directors = 0 };

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var subStats = await _context.Subscriptions
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalSubs = g.Count(),
                    BasicCount = g.Count(s => s.Plan == SubscriptionPlan.Basic),
                    StandardCount = g.Count(s => s.Plan == SubscriptionPlan.Standard),
                    PremiumCount = g.Count(s => s.Plan == SubscriptionPlan.Premium),
                    TotalRev = g.Where(s => s.Status == SubscriptionStatus.Active).Sum(s => (decimal?)s.Price) ?? 0,
                    MonthlyRev = g.Where(s => s.Status == SubscriptionStatus.Active && s.StartDate >= thirtyDaysAgo)
                                  .Sum(s => (decimal?)s.Price) ?? 0
                }).FirstOrDefaultAsync() ?? new { TotalSubs = 0, BasicCount = 0, StandardCount = 0, PremiumCount = 0, TotalRev = 0m, MonthlyRev = 0m };

            return Ok(new AdminStatsDto
            {
                Users = new UserStats { TotalUsers = userStats.Total, ActiveUsers = userStats.Active, TotalProfiles = totalProfiles },
                Content = new ContentStats { TotalMovies = contentStats.Movies, TotalSeries = contentStats.Series, TotalEpisodes = contentStats.Episodes /* ... */ },
                Subscriptions = new SubscriptionStats { TotalSubscriptions = subStats.TotalSubs /* ... */ },
                Revenue = new RevenueStats { TotalRevenue = subStats.TotalRev, MonthlyRevenue = subStats.MonthlyRev /* ... */ }
            });
        }

        [HttpGet("users")]
        [ProducesResponseType(typeof(AdminUserViewDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<IEnumerable<AdminUserViewDto>>> GetAllUsers()
        {
            if (!await _context.Users.AnyAsync())
                return Ok(new List<AdminUserViewDto>());


            var Users = await _context.Users
                .Select(u => new AdminUserViewDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive,
                    IsSubscribed = u.IsSubscribed,
                    LastLoginIp = u.LastLoginIp,
                    Avatar = u.Avatar,
                    Profiles = string.Join(", ", u.Profiles.Select(p => p.Name))
                }).ToListAsync();
            return Ok(Users);
        }

       private static string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            return new string(Enumerable
                .Repeat(chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)])
                .ToArray());
        }



        [HttpPost("create-admin")]
        [ProducesResponseType(typeof(UserCreateByAdminDto), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 403)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<object>> CreateUserByAdmin([FromBody] UserCreateByAdminDto dto)
        {
            
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
                return BadRequest("Username or Email already exists.");

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
                Role = "Admin",
                MustChangePassword = true

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

                await transaction.CommitAsync();

                try
                {
                    await _emailSender.SendEmailAsync(message);
                    
                }
                catch (Exception ex)
                {
                    
                    _logger.LogError(ex, $"Failed to send credentials email to user {user.Id} {user.Email}");


                }



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