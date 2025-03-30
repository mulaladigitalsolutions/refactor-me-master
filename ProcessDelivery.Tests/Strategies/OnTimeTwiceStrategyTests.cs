using System;
using ProcessDelivery.Application.RiskStrategies;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Models;
using Xunit;

namespace ProcessDelivery.Tests.Strategies
{
    public class OnTimeTwiceStrategyTests
    {
        private readonly OnTimeTwiceStrategy _strategy = new OnTimeTwiceStrategy();

        [Fact]
        public void ShouldMatch_When_ReturnedOnDueDateLastTime_And_DatesValid()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = DateTime.Today
            };
            Assert.True(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_LastReturnedDate_DoesNotEqual_LastDueDate()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today.AddDays(1),
                CurrentDueDate = DateTime.Today.AddDays(1)
            };
            Assert.False(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_LastDueDate_IsNull()
        {
            var book = new Book
            {
                LastDueDate = null,
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = DateTime.Today
            };
            Assert.False(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldNotMatch_When_CurrentDueDate_IsDefault()
        {
            var book = new Book
            {
                LastDueDate = DateTime.Today,
                LastReturnedDate = DateTime.Today,
                CurrentDueDate = default
            };
            Assert.False(_strategy.IsMatch(book, DateTime.Today));
        }

        [Fact]
        public void ShouldReturnLowRisk_When_ReturnedOnDueDate()
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
        public void ShouldReturnMediumRisk_When_ReturnedEarly()
        {
            var book = new Book { CurrentDueDate = DateTime.Today };
            var result = _strategy.Evaluate(book, DateTime.Today.AddDays(-1));
            Assert.Equal(RiskLevel.Medium, result.Level);
        }
    }
}