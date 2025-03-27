using System;
using System.Threading.Tasks;
using ProcessDelivery.Domain.Enums;
using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Tests
{
    public class FakeAIRiskService : IAIRiskService
    {
        public Task<RiskAssessmentResult> PredictRisk(Book book, DateTime dateReturned)
        {
            return Task.FromResult(new RiskAssessmentResult
            {
                Level = RiskLevel.Medium,
                Reason = "AI Risk Prediction: MediumRisk (fake)"
            });
        }
    }
}
