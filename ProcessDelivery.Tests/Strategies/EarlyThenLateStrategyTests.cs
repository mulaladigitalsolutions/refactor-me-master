using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class EarlyThenLateStrategyTests
    {
        private readonly EarlyThenLateStrategy _strategy = new();

        [Fact]
        public void ShouldMatch_When_LastReturnWasEarly()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1),
                CurrentDueDate = DateTime.Today
            };
            Assert.True(_strategy.IsMatch(book, DateTime.Today.AddDays(1)));
        }

        [Fact]
        public void ShouldNotMatch_When_LastReturnedDate_IsNotEarlierThanLastDueDate()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = DateTime.Today
            };
            Assert.False(_strategy.IsMatch(book, DateTime.Today.AddDays(1)));
        }

        [Fact]
        public void ShouldReturnMediumRisk_When_ReturnedLateAfterEarlyLastTime()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(1));
            Assert.Equal(RiskLevel.Medium, result.Level);
        }
    }
}
