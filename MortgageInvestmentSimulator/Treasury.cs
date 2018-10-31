using System;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Class representing a single purchase of a 1 year US treasury.
    ///     To simplify, we allow any dollar amount. Not just multiples of 100.
    /// </summary>
    public sealed class Treasury
    {
        /// <summary>
        ///     Gets the identifier which is used as kind of a lot number.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; } = Guid.NewGuid();

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

        public MonthYear Maturity { get; set; }

        public static decimal GetFutureValue(decimal presentValue, decimal interestRate, decimal years = 1.0m)
            => presentValue / (1 - interestRate / 12);

        public decimal GetFaceValue(MonthYear now, decimal interestRate)
        {
            if (IsMature(now))
                return Par;

            var months = MonthYear.MonthDifference(Maturity, now);
            return GetPresentValue(Par, interestRate, (decimal)months / 12);
        }

        public bool IsMature(MonthYear now) => now >= Maturity;

        /// <summary>
        ///     Calculate present value
        /// </summary>
        /// <param name="futureValue">The future value.</param>
        /// <param name="interestRate">The interest rate.</param>
        /// <param name="years">The years.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal GetPresentValue(decimal futureValue, decimal interestRate, decimal years = 1.0m)
            => futureValue * (1 - interestRate / 12);
    }
}

// TODO: 