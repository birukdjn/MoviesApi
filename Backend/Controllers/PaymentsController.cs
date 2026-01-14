using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Payments;
using Backend.Enums;
using Backend.Models;
using Backend.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;
using System.Transactions;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(AppDbContext context, ChapaService chapaService, IConfiguration config) : ControllerBase
    {      
        [HttpPost("initialize-payment")]
        public async Task<ActionResult> Initialize([FromBody] CreateCheckoutDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var price = SubscriptionPricing.GetPrice(dto.Plan);
            var user = await context.Users.FindAsync(userId);
            string FirstName = user.Name.Split(' ').FirstOrDefault() ?? user.Username;
            string LastName = user.Name.Split(' ').Skip(1).FirstOrDefault() ?? user.Username;
            string Email = user.Email;
            string Phone = user.Phone;


            var (checkoutUrl, txRef) = await chapaService.CreatePaymentAsync(
                Email,
                price,
                config["Chapa:CallbackUrl"]!,
                FirstName,
                LastName,
                Phone);

            if (checkoutUrl == null) return BadRequest("Could not initialize payment.");

            var transaction = new PaymentTransaction
            {
                UserId = userId,                               
                Status = SubscriptionStatus.Pending.ToString(),
                TxRef = txRef,   
                Plan = (int)dto.Plan,
                CreatedAt = DateTime.UtcNow
            };

            context.PaymentTransactions.Add(transaction);
            await context.SaveChangesAsync();

            return Ok(new { url = checkoutUrl  });
        }

        [HttpGet("chapa-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ChapaCallback( [FromQuery] string trx_ref)
        {
            
            if (string.IsNullOrEmpty(trx_ref))
                return BadRequest(new { message = "Transaction reference is required." });

            // 1. Verify with Chapa API
            var isPaidAtChapa = await chapaService.VerifyPaymentAsync(trx_ref);
            if (!isPaidAtChapa) return BadRequest(new { message = "Payment verification failed at Chapa." });

            // 2. Find the pending subscription
            var subscription = await context.Subscriptions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.TxRef == trx_ref);

            if (subscription == null) return NotFound(new { message = "Subscription record not found." });

            // 3. If already active, just return success
            if (subscription.Status == SubscriptionStatus.Active)
            {
                return Ok(new { status = "success", message = "Subscription already active." });
            }

            // 4. Activate Subscription and User
            var expiryDate = DateTime.UtcNow.AddMonths(1);
            subscription.Status = SubscriptionStatus.Active;
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = expiryDate;

            if (subscription.User != null)
            {
                subscription.Status = SubscriptionStatus.Active;
                subscription.User.IsSubscribed = true;
                subscription.User.SubscriptionExpiresAt = expiryDate;
            }

            await context.SaveChangesAsync();

            // 5. IMPORTANT: Return JSON, not a Redirect!
            // Your React frontend is waiting for this response.
            return Ok(new { status = "success", message = "Subscription activated successfully." });
        }
    }
}