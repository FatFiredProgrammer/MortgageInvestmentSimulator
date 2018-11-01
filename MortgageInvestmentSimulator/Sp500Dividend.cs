using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     The dividend paid on S&P 500 as a percentage of the value on a given date
    ///     This is a 12 month percentage .
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Sp500Dividend
    {
        public Sp500Dividend(int month, int year, decimal dividendPercentage)
        {
            Date = new MonthYear(month, year);
            DividendPercentage = dividendPercentage.ToPercent();
        }

        public MonthYear Date { get; }

        public decimal DividendPercentage { get; }

        /// <inheritdoc />
        public override string ToString() => $"{Date} => {DividendPercentage:P2}";
    }
}
