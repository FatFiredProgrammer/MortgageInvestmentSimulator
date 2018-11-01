using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Class representing a single purchase of a 1 year US treasury.
    ///     To simplify, we allow any dollar amount. Not just multiples of 100.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Treasury
    {
        /// <summary>
        ///     Gets or sets the par value.
        /// </summary>
        /// <value>The par.</value>
        public decimal Par { get; set; }

        /// <summary>
        ///     Gets or sets the discounted purchase value.
        /// </summary>
        /// <value>The purchase.</value>
        public decimal Purchase { get; set; }

        /// <summary>
        /// Gets or sets the initial interest rate.
        /// </summary>
        /// <value>Interest rate.</value>
        public decimal InitialInterestRate { get; set; }

        /// <summary>
        ///     Gets or sets the maturity date.
        /// </summary>
        /// <value>The maturity.</value>
        public MonthYear Maturity { get; set; }

        public decimal GetFaceValue(MonthYear now)
        {
            if (IsMatured(now))
                return Par;

            var rate = TreasuryInterestRates.GetRate(now);
            var months = MonthYear.MonthDifference(Maturity, now);
            Debug.Assert(months > 0);
            return GetPresentValue(Par, rate.InterestRate, (decimal)months / 12);
        }

        /// <summary>
        ///     Gets the future value.
        /// </summary>
        /// <param name="presentValue">The present value.</param>
        /// <param name="interestRate">The interest rate.</param>
        /// <param name="years">The years.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal GetFutureValue(decimal presentValue, decimal interestRate, decimal years = 1.0m)
            => presentValue / (1 - interestRate / 12);

        /// <summary>
        ///     Calculate present value
        /// </summary>
        /// <param name="futureValue">The future value.</param>
        /// <param name="interestRate">The interest rate.</param>
        /// <param name="years">The years.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal GetPresentValue(decimal futureValue, decimal interestRate, decimal years = 1.0m)
            => futureValue * (1 - interestRate / 12);

        /// <summary>
        ///     Determines whether the specified now is mature.
        /// </summary>
        /// <param name="now">The now.</param>
        /// <returns><c>true</c> if the specified now is mature; otherwise, <c>false</c>.</returns>
        public bool IsMatured(MonthYear now) => now >= Maturity;

        public override string ToString()
            => $"{Par:C0} bond maturing {Maturity}; {Purchase:C0} price with {InitialInterestRate:P2} interest";
    }
}
