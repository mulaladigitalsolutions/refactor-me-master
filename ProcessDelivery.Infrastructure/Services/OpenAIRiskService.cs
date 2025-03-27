// ProcessDelivery.Infrastructure/Services/OpenAIRiskService.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Infrastructure.Services
{
    public class OpenAIRiskService : IAIRiskService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIRiskService(string apiKey)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<RiskAssessmentResult> PredictRisk(Book book, DateTime dateReturned)
        {
            var prompt = $"""
                A user is returning a book with the following details:
                - Last Due Date: {book.LastDueDate?.ToString("yyyy-MM-dd") ?? "None"}
                - Last Returned Date: {book.LastReturnedDate?.ToString("yyyy-MM-dd") ?? "None"}
                - Current Due Date: {book.CurrentDueDate:yyyy-MM-dd}
                - Date Returned: {dateReturned:yyyy-MM-dd}

                Classify the return risk as LowRisk, MediumRisk, or HighRisk, and explain why.
                """;

            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant helping a library classify book return risk." },
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var message = error.GetProperty("message").GetString();
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Medium,
                    Reason = $"AI Error: {message}"
                };
            }

            var content = doc.RootElement
               .GetProperty("choices")[0]
               .GetProperty("message")
               .GetProperty("content")
               .GetString();

            // Simple parsing logic (customize based on actual response format)
            RiskLevel level = RiskLevel.Medium;
            if (content.Contains("LowRisk")) level = RiskLevel.Low;
            else if (content.Contains("HighRisk")) level = RiskLevel.High;

            return new RiskAssessmentResult
            {
                Level = level,
                Reason = content.Trim()
            };
        }
    }
}
