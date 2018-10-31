using System;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Class representing a single purchase of SP 500.
    ///     We allow a fractional number of shares as a simplification (much like a mutual fund).
    /// </summary>
    public sealed class Sp500
    {
        /// <summary>
        ///     Gets the identifier which is used as kind of a lot number.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        ///     Gets or sets the fractional number of shares.
        /// </summary>
        /// <value>The shares.</value>
        public decimal Shares { get; set; }

        /// <summary>
        ///     Gets or sets the basis price for tax purposes.
        /// </summary>
        /// <value>The basis.</value>
        public decimal PurchasePrice { get; set; }
    }
}
