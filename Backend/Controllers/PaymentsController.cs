using Backend.Data;
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
    public class PaymentsController(AppDbContext context, ChapaService chapa, IConfiguration config) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ChapaService _chapa = chapa;
        private readonly IConfiguration _config = config;

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

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            var callbackUrl = _config["Chapa:CallbackUrl"];

            if (string.IsNullOrEmpty(callbackUrl))
            {
                return StatusCode(500, "Payment configuration error: Callback URL is missing.");
            }

            // FIX: Pass required user details to the Chapa service.
            var firstName = user.Name.Split(' ').FirstOrDefault() ?? user.Username;
            var lastName = user.Name.Split(' ').Skip(1).FirstOrDefault() ?? user.Username;

            var (checkoutUrl, txRef) = await _chapa.CreatePaymentAsync(
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

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { url = checkoutUrl });
        }

        // ---------------------------------------------------------
        // 2. CALLBACK: Find the User using TxRef and Activate Subscription
        // ---------------------------------------------------------
        [HttpGet("chapa-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> ChapaCallback([FromQuery] string tx_ref)
        {
            if (string.IsNullOrEmpty(tx_ref))
                return BadRequest("Transaction reference is required.");

            bool isPaidAtChapa = await _chapa.VerifyPaymentAsync(tx_ref);
            if (!isPaidAtChapa) return BadRequest("Payment verification failed at Chapa.");

            var transaction = await _context.PaymentTransactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TxRef == tx_ref);

            if (transaction == null) return NotFound("Transaction record not found in local DB.");

            // 3. Prevent Double Processing (Idempotency)
            if (transaction.Status == "Success")
            {
                // Already processed, just redirect
                return Redirect("http://localhost:5173/success");
            }
           
            // 4. Update Transaction Status
            transaction.Status = "Success";

            // 5. Activate User Subscription
            var user = transaction.User;
            if (user == null)
            {
                // This scenario indicates a major data integrity issue (transaction without a linked user)
                return StatusCode(500, "Internal Error: Transaction is missing a linked user profile.");
            }

            // Logic to calculate end date (1 month from now)
            var expiryDate = DateTime.UtcNow.AddMonths(1);

            // Add Subscription Record
            var subscription = new Subscription
            {
                UserId = user.Id,
                Plan = SubscriptionPlan.Standard,
                StartDate = DateTime.UtcNow,
                EndDate = expiryDate
            };
            _context.Subscriptions.Add(subscription);

            // Update User Profile
            user.IsSubscribed = true;
            user.SubscriptionExpiresAt = expiryDate;

            // Save all changes in one transaction
            await _context.SaveChangesAsync();

            // 6. Redirect to Frontend
            return Redirect("http://localhost:5173/success");
        }
    }
}