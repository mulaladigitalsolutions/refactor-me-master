using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class LateThenLateStrategyTests
    {
        private readonly LateThenLateStrategy _strategy = new();

        [Fact]
        public void ShouldMatch_When_LastReturnWasLate_And_CurrentIsLateToo()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today.AddDays(-2),
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = DateTime.Today.AddDays(-1)
            };
            Assert.True(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_LastReturnWasNotLate()
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
        public void ShouldReturnHighRisk_When_ReturnedLateAgain()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(1));
            Assert.Equal(RiskLevel.High, result.Level);
        }
    }
}
