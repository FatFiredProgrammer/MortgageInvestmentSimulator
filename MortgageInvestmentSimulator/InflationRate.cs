using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Monthly inflation rate
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class InflationRate
    {
        public InflationRate(int month, int year, decimal percent)
        {
            Date = new MonthYear(month, year);
            Percent = percent.ToPercent();
        }

        public MonthYear Date { get; }

        public decimal Percent { get; }

        /// <inheritdoc />
        public override string ToString() => $"{Date} => {Percent:P2}";
    }
}
