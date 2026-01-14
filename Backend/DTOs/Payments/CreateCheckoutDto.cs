using Backend.Enums;
using Newtonsoft.Json;

namespace Backend.DTOs.Payments
{
    public class CreateCheckoutDto
    {
        public SubscriptionPlan Plan { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class ChapaResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ChapaData Data { get; set; } = new();
    }
    public class ChapaData
    {
        [JsonProperty("checkout_url")]
        public string CheckoutUrl { get; set; } = string.Empty;
    }
}
