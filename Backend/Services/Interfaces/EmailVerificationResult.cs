namespace Backend.Services.Interfaces
{
    public abstract record EmailVerificationResult
    {
        public sealed record Success(
            string UserToken,
            string ProfileToken,
            string RefreshToken
        ) : EmailVerificationResult;

        public sealed record AlreadyVerified() : EmailVerificationResult;

        public sealed record InvalidToken() : EmailVerificationResult;

        public sealed record ProfileMissing() : EmailVerificationResult;

        public sealed record Failure(string Error) : EmailVerificationResult;
    }
}