using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class LateThenOnTimeStrategyTests
    {
        private readonly LateThenOnTimeStrategy _strategy = new();

        [Fact]
        public void ShouldMatch_When_LastReturnWasLate_And_ReturnedOnTimeNow()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(1),
                CurrentDueDate = DateTime.Today
            };
            Assert.True(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_LastReturnedDate_NotLaterThanDueDate()
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
        public void ShouldReturnMediumRisk_When_ReturnedOnTime_AfterLateLastTime()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today);
            Assert.Equal(RiskLevel.Medium, result.Level);
        }
    }
}
