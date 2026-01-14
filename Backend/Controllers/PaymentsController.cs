using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Payments;
using Backend.Enums;
using Backend.Models;
using Backend.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(AppDbContext context, ChapaService chapaService, IConfiguration config) : ControllerBase
    {

        // ---------------------------------------------------------
        // 1. START PAYMENT: Save the TxRef + UserId to Database
        // ---------------------------------------------------------
        [Authorize]
        [HttpPost("create-chapa-session")]
        public async Task<IActionResult> CreateChapaSession([FromQuery] decimal amount)
        {
            // FIX: Retrieve and validate User ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Invalid user token or user ID.");

            var user = await context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            var callbackUrl = config["Chapa:CallbackUrl"];

            if (string.IsNullOrEmpty(callbackUrl))
            {
                return StatusCode(500, "Payment configuration error: Callback URL is missing.");
            }

            // FIX: Pass required user details to the Chapa service.
            var firstName = user.Name.Split(' ').FirstOrDefault() ?? user.Username;
            var lastName = user.Name.Split(' ').Skip(1).FirstOrDefault() ?? user.Username;

            var (checkoutUrl, txRef) = await chapaService.CreatePaymentAsync(
                user.Email,
                amount,
                callbackUrl,
                firstName,
                lastName,
                user.Phone 
            );

            if (string.IsNullOrEmpty(checkoutUrl))
                return StatusCode(500, "Failed to create payment session.");

            var transaction = new PaymentTransaction
            {
                UserId = userId,
                TxRef = txRef,
                Amount = amount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            context.PaymentTransactions.Add(transaction);
            await context.SaveChangesAsync();

            return Ok(new { url = checkoutUrl });
        }

        // Inside PaymentsController...

        [HttpPost("initialize-payment")]
        public async Task<IActionResult> Initialize([FromBody] CreateCheckoutDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var price = SubscriptionPricing.GetPrice(dto.Plan);

            // FIX: Using the correct service variable name (_chapa) 
            // and method name (CreatePaymentAsync)
            var (checkoutUrl, txRef) = await chapaService.CreatePaymentAsync(
                dto.Email,
                price,
                config["Chapa:CallbackUrl"]!,
                dto.FirstName,
                dto.LastName,
                null);

            if (checkoutUrl == null) return BadRequest("Could not initialize payment.");

            // Create a Pending Subscription
            var sub = new Subscription
            {
                UserId = userId,
                Plan = dto.Plan,
                Price = price,
                TxRef = txRef,
                Status = SubscriptionStatus.Pending
            };

            context.Subscriptions.Add(sub);
            await context.SaveChangesAsync();

            return Ok(new { checkoutUrl });
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