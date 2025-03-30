using ProcessDelivery.Domain.Enums;
using System;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Application.RiskStrategies
{
    public class EarlyThenEarlyStrategy : IRiskStrategy
    {
        public bool IsMatch(Book book, DateTime dateReturned)
        {
            return book.LastReturnedDate < book.LastDueDate
                && book.LastDueDate.HasValue
                && book.LastReturnedDate.HasValue
                && book.CurrentDueDate > dateReturned;
        }

        public RiskAssessmentResult Evaluate(Book book, DateTime dateReturned)
        {
            if (book.CurrentDueDate == dateReturned)
            {
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Low,
                    Reason = "returned early last time and on due date this time"
                };
            }
            else if (book.CurrentDueDate < dateReturned)
            {
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Medium,
                    Reason = "returned early last time but late this time"
                };
            }
            else
            {
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Low,
                    Reason = "returned early last time and early this time"
                };
            }
        }

    }
}