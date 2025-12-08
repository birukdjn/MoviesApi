using Backend.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;
using Backend.Services.Interfaces;

namespace Backend.Services.Implementations
{
    public class SmsService: ISmsService
    {
        private readonly TwilioSettings _twilioSettings;

        public SmsService( IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public async Task SendSmsAsync(string toPhoneNumber, string messageBody)
        {
            // Twilio requires phone numbers to be in E.164 format (e.g., +12025550100)
            MessageResource messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(_twilioSettings.PhoneNumber),
                body: messageBody
            );
            MessageResource message = messageResource;

            // Optional: Check the status of the message
            // Console.WriteLine($"Twilio Message Status: {message.Status}");
        }
    }
}