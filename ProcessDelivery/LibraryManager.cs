using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

public class LibraryManager
{
    private readonly IEnumerable<IRiskStrategy> _riskStrategies;
    private readonly IAIRiskService _aiRiskService;

    public LibraryManager(IEnumerable<IRiskStrategy> riskStrategies, IAIRiskService aiRiskService)
    {
        _riskStrategies = riskStrategies ?? throw new ArgumentNullException(nameof(riskStrategies));
        _aiRiskService = aiRiskService ?? throw new ArgumentNullException(nameof(aiRiskService));
    }

    public async Task<string> ReturnBook(Book book, DateTime dateReturned)
    {
        try
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book), "Book cannot be null.");

            var strategy = _riskStrategies.FirstOrDefault(s => s.IsMatch(book, dateReturned));

            if (strategy != null && book.CurrentDueDate != default)
            {
                var result = strategy.Evaluate(book, dateReturned);
                return result.ToString();
            }

            // No matching strategy — fallback to AI
            var aiResult = await _aiRiskService.PredictRisk(book, dateReturned);
            return aiResult.ToString();
        }
        catch (ArgumentNullException ex)
        {
            throw new ApplicationException("Validation failed: " + ex.Message, ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new ApplicationException("Risk Strategy error: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An unexpected error occurred in ReturnBook.", ex);
        }
    }
}