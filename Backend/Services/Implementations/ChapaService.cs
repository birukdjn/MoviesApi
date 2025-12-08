using System.Net.Http.Headers;
using System.Text.Json;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace Backend.Services.Implementations
{
    public class ChapaService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;

        // FIX: Constructor to handle Dependency Injection
        public ChapaService(IConfiguration config, HttpClient client)
        {
            _config = config;
            _client = client;

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config["Chapa:SecretKey"]);
        }

        public async Task<(string? checkoutUrl, string txRef)> CreatePaymentAsync(
            string email,
            decimal amount,
            string callbackUrl,
            string firstName,
            string lastName,
            string phoneNumber)
        {
            var txRef = Guid.NewGuid().ToString();

            var data = new
            {
                email,
                amount, // Fixed to send decimal amount directly
                currency = "ETB",
                callback_url = callbackUrl,
                tx_ref = txRef,
                first_name = firstName, // Required Chapa field
                last_name = lastName,   // Required Chapa field
                phone_number = phoneNumber, // Required Chapa field

                customization = new
                {
                    title = "MoviesStore Subscription",
                    description = $"1 Month of Standard Plan"
                }
            };

            try
            {
                var response = await _client.PostAsJsonAsync("https://api.chapa.co/v1/transaction/initialize", data);

                if (!response.IsSuccessStatusCode)
                {
                    // Log the detailed error message from Chapa's response content
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Chapa API failed with status {response.StatusCode}. Details: {errorContent}");
                    // Exception will be caught below, returning null
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var url = doc.RootElement.GetProperty("data").GetProperty("checkout_url").GetString();
                return (url, txRef);
            }
            catch (Exception ex)
            {
                // Log the exception detail
                Console.WriteLine($"Chapa Payment Error: {ex.Message}");
                return (null, txRef);
            }
        }

        public async Task<bool> VerifyPaymentAsync(string txRef)
        {
            try
            {
                var response = await _client.GetAsync($"https://api.chapa.co/v1/transaction/verify/{txRef}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var status = doc.RootElement.GetProperty("data").GetProperty("status").GetString();
                return status?.ToLower() == "success";
            }
            catch
            {
                return false;
            }
        }
    }
}