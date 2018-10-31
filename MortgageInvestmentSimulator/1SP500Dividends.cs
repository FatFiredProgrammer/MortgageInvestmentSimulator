using System.Collections.Generic;

namespace MortgageInvestmentSimulator
{
    public static class Sp500Dividends
    {
        private static readonly Dictionary<MonthYear, Sp500Dividend> _rates = new Dictionary<MonthYear, Sp500Dividend>
        {
        };

        public static Sp500Dividend GetDividend(MonthYear monthYear)
            => _rates.TryGetValue(monthYear, out var dividend) ? dividend : null;

    }
}
