using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Payments;
using Backend.Enums;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubscriptionController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // -------------------------------------------------------------
        // 1. GET /my: List active subscriptions for user
        // -------------------------------------------------------------
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetMySubscriptions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid or missing user ID in token.");
            }

            var subs = await _context.Subscriptions
          .Where(s => s.UserId == userId
                   && s.Status == SubscriptionStatus.Active
                   && (s.EndDate == null || s.EndDate > DateTime.UtcNow)
          )
          .Select(s => new SubscriptionDto
          {
              Id = s.Id,
              UserId = s.UserId,
              Plan = s.Plan.ToString(),
              StartDate = s.StartDate,
              EndDate = s.EndDate,
              TxRef = s.TxRef,
              Status = s.Status.ToString(),
              IsActive = s.Status == SubscriptionStatus.Active && (s.EndDate == null || s.EndDate > DateTime.UtcNow)
          })
          .ToListAsync();

            return Ok(subs);
        }

        // -------------------------------------------------------------
        // 2. POST /subscribe: Create a new subscription
        // -------------------------------------------------------------
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromQuery] SubscriptionPlan plan)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid or missing user ID in token.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Authenticated user not found in database.");
            }

            DateTime? endDate = DateTime.UtcNow.AddMonths(1);

            var subscription = new Subscription
            {
                UserId = userId,
                Plan = plan,
                Price = SubscriptionPricing.GetPrice(plan),
                StartDate = DateTime.UtcNow,
                EndDate = endDate,
                Status = SubscriptionStatus.Active,
                TxRef = $"SUB-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            };

            _context.Subscriptions.Add(subscription);

            user.IsSubscribed = true;
            user.SubscriptionExpiresAt = endDate;

            await _context.SaveChangesAsync();

            await _context.Entry(subscription).ReloadAsync();

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                Plan = subscription.Plan.ToString(),
                Price = subscription.Price,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                TxRef = subscription.TxRef,
                Status = subscription.Status.ToString(),
                IsActive = subscription.IsActive
            };

            return Ok(subscriptionDto);
        }


        // -------------------------------------------------------------
        // 3. POST /cancel/{subscriptionId}: Cancel subscription
        // -------------------------------------------------------------
        [HttpPost("cancel/{subscriptionId}")]
        public async Task<IActionResult> Cancel(int subscriptionId)
        {
            var sub = await _context.Subscriptions.FindAsync(subscriptionId);
            if (sub == null) return NotFound("Subscription not found.");

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out int currentUserId) && currentUserId != sub.UserId)
            {
                return Forbid("You can only cancel your own subscriptions.");
            }

            sub.Status = SubscriptionStatus.Cancelled;

            var user = await _context.Users.FindAsync(sub.UserId);

            if (user != null)
            {
                user.IsSubscribed = false;
                user.SubscriptionExpiresAt = null;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}