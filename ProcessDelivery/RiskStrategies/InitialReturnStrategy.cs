using System;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Application.RiskStrategies
{
    public class InitialReturnStrategy : IRiskStrategy
    {
        public bool IsMatch(Book book, DateTime dateReturned)
        {
            return book.LastDueDate == null || book.LastReturnedDate == null;
        }

        public RiskAssessmentResult Evaluate(Book book, DateTime dateReturned)
        {
            if (book.CurrentDueDate == dateReturned)
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Low,
                    Reason = "first time being returned and returned on time"
                };

            if (book.CurrentDueDate < dateReturned)
                return new RiskAssessmentResult
                {
                    Level = RiskLevel.Medium,
                    Reason = "first time being returned and returned late"
                };

            return new RiskAssessmentResult
            {
                Level = RiskLevel.Low,
                Reason = "first time being returned and returned early"
            };
        }
    }
}
