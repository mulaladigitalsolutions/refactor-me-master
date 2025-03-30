using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class LateThenEarlyStrategyTests
    {
        private readonly LateThenEarlyStrategy _strategy = new LateThenEarlyStrategy();

        [Fact]
        public void ShouldMatch_When_LastReturnWasLate_And_CurrentIsEarly()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today.AddDays(-2),
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = DateTime.Today.AddDays(1)
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
            Assert.False(_strategy.IsMatch(book, DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void ShouldReturnMediumRisk_When_ReturnedEarly_AfterLateLastTime()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(-1));
            Assert.Equal(RiskLevel.Medium, result.Level);
        }
    }
}
