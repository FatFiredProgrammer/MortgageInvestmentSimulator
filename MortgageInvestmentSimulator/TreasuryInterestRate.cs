using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     1 year treasury interest rate at a point in time.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class TreasuryInterestRate
    {
        public TreasuryInterestRate(int month, int year, decimal interestRate)
        {
            Date = new MonthYear(month, year);
            InterestRate = interestRate.ToPercent();
        }

        public MonthYear Date { get; }

        public decimal InterestRate { get; }

        /// <inheritdoc />
        public override string ToString() => $"{Date} => {InterestRate:P2}";
    }
}
