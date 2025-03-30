using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class EarlyThenOnTimeStrategyTests
    {
        private readonly EarlyThenOnTimeStrategy _strategy = new EarlyThenOnTimeStrategy();

        [Fact]
        public void ShouldMatch_When_LastReturnWasEarly_And_ReturnedOnTimeNow()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1),
                CurrentDueDate = DateTime.Today
            };
            Assert.True(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_LastReturnedDate_NotEarlierThanDueDate()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = DateTime.Today
            };
            Assert.False(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldReturnLowRisk_When_ReturnedOnTime_AfterEarlyLastTime()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today);
            Assert.Equal(RiskLevel.Low, result.Level);
        }
    }
}
