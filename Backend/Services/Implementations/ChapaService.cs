using Backend.DTOs.Payments;
using Newtonsoft.Json;
using System.Text;

namespace Backend.Services.Implementations
{
    public class ChapaService(IConfiguration config, HttpClient httpClient)
    {
        private readonly string _secretKey = config["Chapa:SecretKey"]!;

        // This replaces "InitializePayment" to match your Controller's call
      
        public async Task<(string? CheckoutUrl, string TxRef)> CreatePaymentAsync(
    string email, decimal amount, string callbackUrl, string firstName, string lastName, string? phone)
        {
            var txRef = $"TX-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _secretKey);

            var payload = new
            {
                // Chapa expects a string or decimal, but specifically "currency" must be correct
                amount = amount.ToString("0.00"),
                currency = "ETB",
                email,
                first_name = firstName,
                last_name = lastName,
                phone_number = phone ?? "0900000000", // Chapa sometimes requires a phone number
                tx_ref = txRef,
                callback_url = callbackUrl,
                // FIX: Redirect back to your Next.js Success page
                return_url = "http://localhost:3000/payment-success",
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.chapa.co/v1/transaction/initialize", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Chapa Error: {errorContent}"); // Check your console for this!
                return (null, txRef);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var chapaRes = JsonConvert.DeserializeObject<ChapaResponse>(jsonResponse);

            return (chapaRes?.Data?.CheckoutUrl, txRef);
        }
        public async Task<bool> VerifyPaymentAsync(string txRef)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _secretKey);

            var response = await httpClient.GetAsync($"https://api.chapa.co/v1/transaction/verify/{txRef}");

            if (!response.IsSuccessStatusCode) return false;

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(jsonResponse)!;

            return result.status == "success";
        }
    }
}