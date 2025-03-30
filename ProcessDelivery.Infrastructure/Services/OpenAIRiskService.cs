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

        public OpenAIRiskService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<RiskAssessmentResult> PredictRisk(Book book, DateTime dateReturned)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");

                var prompt = $"Evaluate the risk of a book being returned. " +
                             $"LastDueDate: {book.LastDueDate}, " +
                             $"LastReturnedDate: {book.LastReturnedDate}, " +
                             $"CurrentDueDate: {book.CurrentDueDate}, " +
                             $"ReturnedDate: {dateReturned}";

                var payload = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                new { role = "system", content = "You are a library risk evaluator. Return one of: LowRisk, MediumRisk, HighRisk." },
                new { role = "user", content = prompt }
            }
                };

                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(stream);

                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("choices", out var choices) ||
                    choices.GetArrayLength() == 0 ||
                    !choices[0].TryGetProperty("message", out var message) ||
                    !message.TryGetProperty("content", out var contentElement))
                {
                    return new RiskAssessmentResult
                    {
                        Level = RiskLevel.Medium,
                        Reason = "AI Error: Missing or malformed response from OpenAI."
                    };
                }

                var content = contentElement.GetString() ?? string.Empty;
                var level = RiskLevel.Medium;

                if (content.Contains("LowRisk", StringComparison.OrdinalIgnoreCase))
                    level = RiskLevel.Low;
                else if (content.Contains("HighRisk", StringComparison.OrdinalIgnoreCase))
                    level = RiskLevel.High;

                return new RiskAssessmentResult
                {
                    Level = level,
                    Reason = content.Trim()
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An unexpected error occurred while predicting risk with OpenAI.", ex);
            }
        }
    }
}
