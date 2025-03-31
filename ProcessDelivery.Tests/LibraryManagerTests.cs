using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;
using ProcessDelivery.Tests.Fake;
using Xunit;

namespace ProcessDelivery.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ShouldReturn_LowRisk_When_NoReturnHistoryExistsAndReturnedOnDueDateThisTime()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var strategies = new List<IRiskStrategy> { new InitialReturnStrategy() };
            var manager = new LibraryManager(strategies, new FakeAIRiskService(), mockLogger.Object);

            var currentDueDate = DateTime.Today;
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = currentDueDate };

            var result = await manager.ReturnBook(book, currentDueDate);

            Assert.Equal("LowRisk: first time being returned and returned on time", result);
        }

        [Fact]
        public async Task ShouldReturn_MediumRisk_When_NoReturnHistoryExistsAndReturnedLateThisTime()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var strategies = new List<IRiskStrategy> { new InitialReturnStrategy() };
            var manager = new LibraryManager(strategies, new FakeAIRiskService(), mockLogger.Object);

            var currentDueDate = DateTime.Today;
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = currentDueDate };

            var result = await manager.ReturnBook(book, currentDueDate.AddDays(1));

            Assert.Equal("MediumRisk: first time being returned and returned late", result);
        }

        [Fact]
        public async Task ShouldReturn_LowRisk_When_NoReturnHistoryExistsAndReturnedEarlyThisTime()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var strategies = new List<IRiskStrategy> { new InitialReturnStrategy() };
            var manager = new LibraryManager(strategies, new FakeAIRiskService(), mockLogger.Object);

            var currentDueDate = DateTime.Today;
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = currentDueDate };

            var result = await manager.ReturnBook(book, currentDueDate.AddDays(-1));

            Assert.Equal("LowRisk: first time being returned and returned early", result);
        }

        [Fact]
        public async Task ShouldReturn_HighRisk_When_BookWasReturnedLateLastTime_AndLateAgain()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var strategies = new List<IRiskStrategy> { new LateThenLateStrategy() };
            var manager = new LibraryManager(strategies, new FakeAIRiskService(), mockLogger.Object);

            var lastDueDate = DateTime.Today.AddDays(-2);
            var currentDueDate = DateTime.Today.AddDays(-1);
            var book = new Book
            {
                LastDueDate = lastDueDate,
                LastReturnedDate = lastDueDate.AddDays(1),
                CurrentDueDate = currentDueDate
            };

            var result = await manager.ReturnBook(book, currentDueDate.AddDays(1));

            Assert.Equal("HighRisk: returned late last time and late this time", result);
        }

        [Fact]
        public async Task ShouldReturn_AIResult_When_NoStrategyMatches()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var strategies = new List<IRiskStrategy>(); // no match
            var manager = new LibraryManager(strategies, new FakeAIRiskService(), mockLogger.Object);

            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1),
                CurrentDueDate = default
            };

            var result = await manager.ReturnBook(book, DateTime.Today);

            Assert.StartsWith("MediumRisk:", result);
            Assert.Contains("AI Risk Prediction:", result);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString().Contains("No matching strategy")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public async Task ShouldThrow_ApplicationException_When_BookIsNull()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var manager = new LibraryManager([], new FakeAIRiskService(), mockLogger.Object);

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                manager.ReturnBook(null, DateTime.Today));

            Assert.Contains("Validation failed", ex.Message);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public async Task ShouldThrow_ApplicationException_When_AIServiceFails()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var manager = new LibraryManager([], new FailingAIRiskService(), mockLogger.Object);
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1),
                CurrentDueDate = default
            };

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                manager.ReturnBook(book, DateTime.Today));

            Assert.Contains("Risk Strategy error", ex.Message);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task ShouldThrow_ApplicationException_When_UnexpectedErrorOccursInAIService()
        {
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var manager = new LibraryManager([], new ExplodingAIRiskService(), mockLogger.Object);

            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1), // breaks strategy match
                CurrentDueDate = default
            };

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                manager.ReturnBook(book, DateTime.Today));

            Assert.Contains("unexpected error", ex.Message.ToLower());
            Assert.IsType<NullReferenceException>(ex.InnerException);
        }
    }
}