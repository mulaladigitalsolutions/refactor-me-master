using System;
using System.Threading.Tasks;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Tests.Fake
{
    public class FailingAIRiskService : IAIRiskService
    {
        public Task<RiskAssessmentResult> PredictRisk(Book book, DateTime dateReturned)
        {
            throw new InvalidOperationException("AI failed");
        }
    }
}
