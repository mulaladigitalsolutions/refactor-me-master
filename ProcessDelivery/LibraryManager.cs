using ProcessDelivery.Domain.Interfaces;
using ProcessDelivery.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;


public class LibraryManager
{
    private readonly IEnumerable<IRiskStrategy> _riskStrategies;
    private readonly IAIRiskService _aiRiskService;
    private readonly ILogger<LibraryManager> _logger;

    public LibraryManager(IEnumerable<IRiskStrategy> strategies, IAIRiskService aiService, ILogger<LibraryManager> logger)
    {
        _riskStrategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        _aiRiskService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task<string> ReturnBook(Book book, DateTime dateReturned)
    {
        _logger.LogInformation("Book return initiated for {Date}", dateReturned);

        try
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book), "Book cannot be null.");

            var strategy = _riskStrategies.FirstOrDefault(s => s.IsMatch(book, dateReturned));

            if (strategy != null && book.CurrentDueDate != default)
            {
                _logger.LogInformation("Using strategy: {StrategyName}", strategy.GetType().Name);
                var result = strategy.Evaluate(book, dateReturned);
                return result.ToString();
            }

            _logger.LogWarning("No matching strategy. Falling back to AI prediction.");
            var aiResult = await _aiRiskService.PredictRisk(book, dateReturned);
            return aiResult.ToString();
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError("Validation failed: " + ex.Message, ex);
            throw new ApplicationException("Validation failed: " + ex.Message, ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Risk Strategy error: " + ex.Message, ex);
            throw new ApplicationException("Risk Strategy error: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred in ReturnBook. " + ex);
            throw new ApplicationException("An unexpected error occurred in ReturnBook.", ex);
        }
    }
}