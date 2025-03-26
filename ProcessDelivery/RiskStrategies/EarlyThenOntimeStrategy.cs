using System;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.RiskStrategies
{
    public class EarlyThenOntimeStrategy : IRiskStrategy
    {
        public bool IsMatch(Book book, DateTime dateReturned)
        {
            return book.LastDueDate.HasValue
                && book.LastReturnedDate.HasValue
                && book.LastReturnedDate < book.LastDueDate
                && book.CurrentDueDate == dateReturned;
        }

        public RiskAssessmentResult Evaluate(Book book, DateTime dateReturned)
        {
            return new RiskAssessmentResult
            {
                Level = RiskLevel.Low,
                Reason = "returned on early last time and on due date this time"
            };
        }
    }
}