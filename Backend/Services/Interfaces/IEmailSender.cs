using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IEmailSender
    {
        void SendEmail(Message message);
    }
}
