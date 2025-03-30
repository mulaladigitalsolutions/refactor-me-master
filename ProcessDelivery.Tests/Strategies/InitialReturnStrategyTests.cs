using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class InitialReturnStrategyTests
    {
        private readonly InitialReturnStrategy _strategy = new InitialReturnStrategy();

        [Fact]
        public void ShouldMatch_When_LastDatesAreNull()
        {
            var book = new Book { LastDueDate = null, LastReturnedDate = null, CurrentDueDate = DateTime.Today };
            Assert.True(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_LastDatesAreNotNull()
        {
            var book = new Book { LastDueDate = DateTime.Today, LastReturnedDate = DateTime.Today, CurrentDueDate = DateTime.Today };
            Assert.False(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldReturnLowRisk_When_ReturnedOnTime()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today);
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
        public void ShouldReturnLowRisk_When_ReturnedEarly()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(-1));
            Assert.Equal(RiskLevel.Low, result.Level);
        }
    }
}