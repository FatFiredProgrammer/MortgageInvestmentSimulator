using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Price of S&P 500 at some point in time.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Sp500Price
    {
        public Sp500Price(int month, int year, decimal price)
        {
            Date = new MonthYear(month, year);
            Price = price;
        }

        public MonthYear Date { get; }
        public decimal Price { get; }

        /// <inheritdoc />
        public override string ToString() => $"{Date} => {Price:C0}";
    }
}
