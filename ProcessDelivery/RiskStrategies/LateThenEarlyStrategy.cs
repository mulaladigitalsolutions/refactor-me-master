using System;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.RiskStrategies
{
    public class LateThenEarlyStrategy : IRiskStrategy
    {
        public bool IsMatch(Book book, DateTime dateReturned)
        {
            return book.LastDueDate.HasValue
                && book.LastReturnedDate.HasValue
                && book.LastReturnedDate > book.LastDueDate
                && book.CurrentDueDate > dateReturned;
        }

        public RiskAssessmentResult Evaluate(Book book, DateTime dateReturned)
        {
            return new RiskAssessmentResult
            {
                Level = RiskLevel.Medium,
                Reason = "returned late last time but early this time"
            };
        }
    }
}