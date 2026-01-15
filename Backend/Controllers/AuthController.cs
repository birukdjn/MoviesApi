using Backend.Data;
using Backend.DTOs;
using Backend.DTOs.Users;
using Backend.Models;
using Backend.Services.Implementations;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context, IJwtService jwt, IEmailSender emailSender,IPasswordService passwordService,IEmailVerificationService emailVerificationService, IConfiguration configuration) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IJwtService _jwt = jwt;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IEmailVerificationService _emailVerificationService = emailVerificationService;
        private readonly IConfiguration _configuration = configuration;


        [HttpGet("check-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EmailCheckResponse),200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<ActionResult<EmailCheckResponse>> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var exists = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail);

            return Ok(new EmailCheckResponse(exists));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserRegisterDto),201)]
        [ProducesResponseType(typeof(object),400)]
        [ProducesResponseType(typeof(object),500)]
        public async Task<ActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return BadRequest("Username or Email already exists.");

            string passwordHash = _passwordService.HashPassword(dto.Password);

            string verificationToken = Guid.NewGuid().ToString("N");
            DateTime verificationExpiry = DateTime.UtcNow.AddDays(1);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Avatar = dto.Avatar ??  "default_avatar.png",
                Role = "User",
                Phone = dto.Phone,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiry = verificationExpiry
            };
            user.Email = user.Email.Trim().ToLowerInvariant();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var defaultProfile = new Profile
            {
                UserId = user.Id,
                Name = user.Username,
                Avatar = user.Avatar ?? "default_avatar.png",
                IsKidsProfile = false
            };

            _context.Profiles.Add(defaultProfile);
            await _context.SaveChangesAsync();
            
            var frontendBaseUrl = _configuration.GetValue<string>("App:FrontendBaseUrl") ?? "http://192.168.100.167:3000";

            var verificationLink = $"{frontendBaseUrl}/verify-email?token={verificationToken}";

            try
            {
                var verificationMessage = new Message(
                    [user.Email],
                    "Action Required: Verify Your Email Address",
                    $"""
                    <h1>Welcome to MoviesStore!</h1>
                    <p>Thank you for registering. Please click the link below to verify your email address and activate your account:</p>
                    <p><a href='{verificationLink}'>Verify My Email Address</a></p>
                    <p>This link is valid for 24 hours.</p>
                    """

                );

                await _emailSender.SendEmailAsync(verificationMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email to {user.Email}: {ex.Message}");
            }


            return Ok(new
            {
                message = "User registered successfully. **Please check your email to verify your account and activate login.**",
                userId = user.Id,
                username = user.Username,
                defaultProfile = new
                {
                    id = defaultProfile.Id,
                    name = defaultProfile.Name,
                    defaultProfile.Avatar,
                }
            });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EmailVerificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmailVerificationResponse>> VerifyEmail(
    [FromBody] EmailVerificationRequest request)
        {
            // Automatic validation if using [ApiController]
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            var result = await _emailVerificationService.VerifyEmailAsync(request.Token);

            return result switch
            {
                EmailVerificationResult.Success success => Ok(new EmailVerificationResponse(
                    "Email verified successfully. You can now log in.",
                    success.UserToken,
                    success.ProfileToken,
                    success.RefreshToken)),

                EmailVerificationResult.AlreadyVerified => Ok(new EmailVerificationResponse(
                    "Email is already verified. You can now log in.")),

                EmailVerificationResult.InvalidToken => BadRequest(new ProblemDetails
                {
                    Title = "Invalid token",
                    Detail = "The verification token is invalid or expired.",
                    Instance = $"{Request.Path}",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                }),

                EmailVerificationResult.ProfileMissing => StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Profile missing",
                        Detail = "Verification succeeded but profile data is missing.",
                        Instance = $"{Request.Path}",
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                    }),

                EmailVerificationResult.Failure failure => StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Title = "Verification failed",
                        Detail = failure.Error,
                        Instance = $"{Request.Path}",
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                    }),

                _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unknown error",
                    Detail = "An unexpected error occurred.",
                    Instance = $"{Request.Path}",
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                })
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.LoginIdentifier || u.Username == dto.LoginIdentifier);

            if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            if (user.MustChangePassword)
            {
                return StatusCode(403, new
                {
                    message = "Password change required",
                    mustChangePassword = true
                });
            }


            if (!user.IsEmailVerified)
            {
                return Unauthorized(new { message = "Your email address has not been verified. Please check your inbox for the verification link." });
            }


            var currentIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? HttpContext.Connection.RemoteIpAddress?.ToString();


            if (user.LastLoginIp != currentIp)
            {
                try
                {
                    var msg = new Message(
                        [user.Email],
                        "Security Alert: New Login Location Detected",
                        $"""
                        <p>A login was detected from IP: <strong>{currentIp}</strong>.</p>
                        <p>If this wasn't you, please change your password.</p>
                        """
                    );

                    await _emailSender.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending IP alert: {ex.Message}");
                }

            }
            user.LastLoginIp = currentIp;
            await _context.SaveChangesAsync();

            
            var userToken = _jwt.GenerateUserToken(user);

            // ✅ Separate logic for Admin
            if (user.Role == "Admin")
            {
                var refreshToken = _jwt.GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    token = userToken,
                    refreshToken,
                    role = user.Role
                });
            }

            // ✅ Regular user flow (with profiles)
            var profiles = await _context.Profiles
                .Where(p => p.UserId == user.Id)
                .Select(p => new { p.Id, p.Name, p.Avatar })
                .ToListAsync();

            var refresh = _jwt.GenerateRefreshToken();
            user.RefreshToken = refresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = userToken,
                profiles,
                refreshToken = refresh,
                role = user.Role
            });
        }


        private int GetCurrentUserId()
        {
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userClaimId, out int userId) ? userId : 0;
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto dto)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            bool changesMade = false;

            if (!string.IsNullOrEmpty(dto.Username) && user.Username != dto.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != userId))
                    return BadRequest("New username is already taken.");

                user.Username = dto.Username;
                changesMade = true;
            }

            if (!string.IsNullOrEmpty(dto.Email) && user.Email != dto.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId))
                    return BadRequest("New email is already taken.");

                user.Email = dto.Email;
                changesMade = true;
            }

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword))
                    return BadRequest("Current password is required to change password.");

                if (!_passwordService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                    return Unauthorized(new { message = "Incorrect current password." });

                user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
                changesMade = true;
            }

            if (changesMade)
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "User updated successfully." });
            }

            return Ok(new { message = "No changes submitted." });
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            var newToken = _jwt.GenerateUserToken(user);
            var newRefreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _context.SaveChanges();

            return Ok(new { token = newToken, refreshToken = newRefreshToken });
        }
        private string GeneratePasswordResetToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return Ok(new { message = "If an account associated with this email exists, a password reset link has been sent." });
            }

            var resetToken = GeneratePasswordResetToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); 

            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:3000/reset-password?token={resetToken}";

            // 3. Send Email
            try
            {
                var emailBody = $"<h1>Password Reset Request</h1>" +
                                $"<p>You requested a password reset. Click the link below to set a new password:</p>" +
                                $"<p><a href='{resetLink}'>Reset Your Password</a></p>" +
                                $"<p>This link will expire in 1 hour. If you didn't request this, ignore this email.</p>";

                var message = new Message(
                    [user.Email],
                    "Password Reset Request",
                    emailBody
                );

                await _emailSender.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email to {user.Email}: {ex.Message}");
                // Log the error but still return success to the user
            }

            return Ok(new { message = "If an account associated with this email exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            // 1. Find User by Token
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                // Use a generic error message for security
                return BadRequest(new { message = "Invalid or expired reset token." });
            }

            // 2. Validate New Password (Optional: add length/complexity checks here)
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters long." });
            }

            // 3. Hash and Update Password
            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;

            // 4. Invalidate Token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Your password has been reset successfully. You can now log in." });
        }
    }
}
