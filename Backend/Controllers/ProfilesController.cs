using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System.Security.Claims;
using Backend.Services.Interfaces;
using Backend.DTOs.Profiles;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController(AppDbContext context, IJwtService jwt) : ControllerBase
    {
        private int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ProfileSelectDto>>> GetProfiles()
        {
            int userId = GetCurrentUserId();

            var profiles = await context.Profiles
                .Where(p => p.UserId == userId)
                .Select(p => new ProfileSelectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Avatar = p.Avatar ?? "default_avatar.png",
                    IsKidsProfile = p.IsKidsProfile
                })
                .ToListAsync();

            return Ok(profiles);
        }

        [HttpPost("select/{profileId}")]
        public async Task<IActionResult> SelectProfile(int profileId)
        {
            int userId = GetCurrentUserId();
            var profile = await context.Profiles
                .Include(p => p.User) 
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null) return NotFound("Profile not found or access denied.");

            var profileToken = jwt.GenerateProfileToken(profile);

            return Ok(new { token = profileToken });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProfile([FromBody] ProfileCreateDto dto)
        {
            int userId = GetCurrentUserId();

            var newProfile = new Profile
            {
                UserId = userId,
                Name = dto.Name,
                Avatar = dto.AvatarUrl,
                IsKidsProfile = dto.IsKidsProfile
            };

            context.Profiles.Add(newProfile);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfiles), new { id = newProfile.Id },
                new ProfileSelectDto { Id = newProfile.Id, Name = newProfile.Name });
        }
    }
}