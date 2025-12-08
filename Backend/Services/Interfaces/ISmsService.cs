namespace Backend.Services.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string toPhoneNumber, string messageBody);
    }
}
