namespace Backend.Services.Interfaces
{
    public interface IEmailVerificationService
    {
        Task<EmailVerificationResult> VerifyEmailAsync(string token);
    }
}