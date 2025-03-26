using System;
using ProcessDelivery.Domain.Interfaces;
using System.Collections.Generic;
using ProcessDelivery.Domain.Models;
using System.Linq;
using ProcessDelivery.Application.RiskStrategies;


namespace ProcessDelivery.Application
{
    public class LibraryManager
    {
        private readonly IEnumerable<IRiskStrategy> _riskStrategies;

        public LibraryManager()
        {
            _riskStrategies =
            [
                new InitialReturnStrategy(),
                new OnTimeTwiceStrategy(),
                new EarlyThenEarlyStrategy(),
                new EarlyThenLateStrategy(),
                new EarlyThenOnTimeStrategy(),
                new LateThenLateStrategy(),
                new LateThenOnTimeStrategy(),
                new LateThenEarlyStrategy()
            ];
        }

        public string ReturnBook(Book book, DateTime dateReturned)
        {
            try
            {
                if (book == null)
                    throw new ArgumentNullException(nameof(book), "Book cannot be null.");

                var strategy = _riskStrategies.FirstOrDefault(s => s.IsMatch(book, dateReturned));

                if (strategy != null)
                {
                    var result = strategy.Evaluate(book, dateReturned);
                    return result.ToString();
                }

                throw new InvalidOperationException("No matching risk strategy found.");
            }
            catch (ArgumentNullException ex)
            {
                throw new ApplicationException("Applicatiion Validation failed: " + ex.Message, ex);
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
}
