using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class EarlyThenEarlyStrategyTests
    {
        private readonly EarlyThenEarlyStrategy _strategy = new EarlyThenEarlyStrategy();

        [Fact]
        public void ShouldMatch_When_LastReturnWasEarly_And_CurrentDueDateValid()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(-1),
                CurrentDueDate = DateTime.Today
            };
            Assert.True(_strategy.IsMatch(book, DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void ShouldNotMatch_When_LastReturnedDate_IsNotEarlier()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(1),
                CurrentDueDate = DateTime.Today
            };
            Assert.False(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldReturnLowRisk_When_ReturnedEarlyAgain()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(-1));
            Assert.Equal(RiskLevel.Low, result.Level);
        }

        [Fact]
        public void ShouldReturnMediumRisk_When_ReturnedLate()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(1));
            Assert.Equal(RiskLevel.Medium, result.Level);
        }

        [Fact]
        public void ShouldReturnLowRisk_When_ReturnedOnDueDate()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today);
            Assert.Equal(RiskLevel.Low, result.Level);
        }
    }
}