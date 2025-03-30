using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using ProcessDelivery.Infrastructure.Services;
using Xunit;

namespace ProcessDelivery.Tests.Services
{
    public class OpenAIRiskServiceTests
    {
        private const string FakeApiKey = "sk-fake-key";

        [Fact]
        public async Task ShouldReturn_RiskAssessmentResult_When_ResponseIsValid()
        {
            // Arrange
            var jsonResponse = """
            {
                "choices": [
                    {
                        "message": {
                            "content": "LowRisk: returned on time"
                        }
                    }
                ]
            }
            """;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new OpenAIRiskService(httpClient, FakeApiKey);

            var book = new Book { CurrentDueDate = DateTime.Today };
            var dateReturned = DateTime.Today;

            var result = await service.PredictRisk(book, dateReturned);

            Assert.Equal(RiskLevel.Low, result.Level);
            Assert.Contains("returned on time", result.Reason);
        }

        [Fact]
        public async Task ShouldThrow_ApplicationException_When_ResponseIsError()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\":\"Invalid request\"}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new OpenAIRiskService(httpClient, "fake-key");

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                service.PredictRisk(new Book(), DateTime.Today));

            Assert.Contains("unexpected error", ex.Message.ToLower());
            Assert.IsType<HttpRequestException>(ex.InnerException);
        }

        [Fact]
        public async Task ShouldReturn_MediumRisk_WithAIError_When_ResponseIsMalformed()
        {
            var jsonResponse = @"{ ""unexpected"": ""structure"" }";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new OpenAIRiskService(httpClient, "fake-key");

            var result = await service.PredictRisk(new Book(), DateTime.Today);

            Assert.Equal(RiskLevel.Medium, result.Level);
            Assert.StartsWith("AI Error:", result.Reason);
        }

        [Fact]
        public async Task ShouldReturn_HighRisk_When_AIRespondsWithHighRisk()
        {
            var jsonResponse = @"
            {
                ""choices"": [
                    {
                        ""message"": {
                            ""content"": ""HighRisk: returned late repeatedly""
                        }
                    }
                ]
            }";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new OpenAIRiskService(httpClient, "fake-key");

            var result = await service.PredictRisk(new Book(), DateTime.Today);

            Assert.Equal(RiskLevel.High, result.Level);
            Assert.Contains("HighRisk", result.Reason);
        }
    }
}