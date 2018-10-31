using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Average mortgage interest rates at a point in time.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class MortgageInterestRate
    {
        public MortgageInterestRate(int month, int year, decimal interestRate)
        {
            Date = new MonthYear(month, year);
            InterestRate = interestRate;
        }

        public MonthYear Date { get; }

        public decimal InterestRate { get; }

        /// <inheritdoc />
        public override string ToString()
            => $"{Date} => {InterestRate:P2}";
    }
}
