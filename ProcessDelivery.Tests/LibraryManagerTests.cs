using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;
using ProcessDelivery.Tests.Fake;
using Xunit;

namespace ProcessDelivery.Tests
{
    public class LibraryManagerTests
    {
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            var strategies = new List<IRiskStrategy>
            {
                new InitialReturnStrategy(),
                new OnTimeTwiceStrategy(),
                new EarlyThenEarlyStrategy(),
                new EarlyThenLateStrategy(),
                new EarlyThenOnTimeStrategy(),
                new LateThenLateStrategy(),
                new LateThenOnTimeStrategy(),
                new LateThenEarlyStrategy()
            };

            _libraryManager = new LibraryManager(strategies, new FakeAIRiskService());
        }

        [Fact]
        public async Task ShouldReturn_LowRisk_When_NoReturnHistoryExistsAndReturnedOnDueDateThisTime()
        {
            var currentDueDate = DateTime.Today;
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = currentDueDate };

            var result = await _libraryManager.ReturnBook(book, currentDueDate);

            Assert.Equal("LowRisk: first time being returned and returned on time", result);
        }

        [Fact]
        public async Task ShouldReturn_MediumRisk_When_NoReturnHistoryExistsAndReturnedLateThisTime()
        {
            var currentDueDate = DateTime.Today;
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = currentDueDate };

            var result = await _libraryManager.ReturnBook(book, currentDueDate.AddDays(1));

            Assert.Equal("MediumRisk: first time being returned and returned late", result);
        }

        [Fact]
        public async Task ShouldReturn_LowRisk_When_NoReturnHistoryExistsAndReturnedEarlyThisTime()
        {
            var currentDueDate = DateTime.Today;
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = currentDueDate };

            var result = await _libraryManager.ReturnBook(book, currentDueDate.AddDays(-1));

            Assert.Equal("LowRisk: first time being returned and returned early", result);
        }

        [Fact]
        public async Task ShouldReturn_HighRisk_When_BookWasReturnedLateLastTime_AndLateAgain()
        {
            var lastDueDate = DateTime.Today.AddDays(-2);
            var currentDueDate = DateTime.Today.AddDays(-1);
            var book = new Book
            {
                LastDueDate = lastDueDate,
                LastReturnedDate = lastDueDate.AddDays(1), // late
                CurrentDueDate = currentDueDate
            };

            var result = await _libraryManager.ReturnBook(book, currentDueDate.AddDays(1)); // late again

            Assert.Equal("HighRisk: returned late last time and late this time", result);
        }

        [Fact]
        public async Task ShouldReturn_AIResult_When_NoStrategyMatches()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1), // breaks match
                CurrentDueDate = default                         // invalid due date
            };

            var result = await _libraryManager.ReturnBook(book, DateTime.Today);
            Assert.StartsWith("MediumRisk:", result);
            Assert.Contains("AI Risk Prediction:", result);
        }

        [Fact]
        public async Task ShouldThrow_ApplicationException_When_BookIsNull()
        {
            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                _libraryManager.ReturnBook(null, DateTime.Today));

            Assert.Contains("Validation failed", ex.Message);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        public async Task ShouldThrow_ApplicationException_When_AIServiceFails()
        {
            var manager = new LibraryManager([], new FailingAIRiskService());
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
            var manager = new LibraryManager([], new ExplodingAIRiskService());

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