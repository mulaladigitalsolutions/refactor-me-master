using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Domain.Interfaces
{
    public interface IAIRiskService
    {
        Task<RiskAssessmentResult> PredictRisk(Book book, DateTime dateReturned);
    }
}
