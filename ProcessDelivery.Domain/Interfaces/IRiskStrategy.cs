using ProcessDelivery.Domain.Models;

namespace ProcessDelivery.Domain.Interfaces
{
    public interface IRiskStrategy
    {
        bool IsMatch(Book book, DateTime dateReturned);
        RiskAssessmentResult Evaluate(Book book, DateTime dateReturned);
    }
}
