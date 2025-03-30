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

            // Act
            var result = await service.PredictRisk(book, dateReturned);

            // Assert
            Assert.Equal(RiskLevel.Low, result.Level);
            Assert.Contains("returned on time", result.Reason);
        }

        [Fact]
        public async Task ShouldThrow_When_ResponseIsError()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\":\"Invalid request\"}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new OpenAIRiskService(httpClient, FakeApiKey);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.PredictRisk(new Book(), DateTime.Today));
        }
    }
}