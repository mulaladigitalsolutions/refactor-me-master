using System;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.RiskStrategies
{
    public class OnTimeTwiceStrategy : IRiskStrategy
    {
        public bool IsMatch(Book book, DateTime dateReturned)
        {
            return book.LastDueDate == book.LastReturnedDate
                && book.LastDueDate.HasValue
                && book.CurrentDueDate != default;
        }

        public RiskAssessmentResult Evaluate(Book book, DateTime dateReturned)
        {
            if (book.CurrentDueDate == dateReturned)
            {
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Low,
                    Reason = "returned on due date last 2 times"
                };
            }
            else if (book.CurrentDueDate < dateReturned)
            {
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Medium,
                    Reason = "returned on due date last time but late this time"
                };
            }
            else
            {
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Medium,
                    Reason = "returned on due date last time but early this time"
                };
            }
        }
    }
}