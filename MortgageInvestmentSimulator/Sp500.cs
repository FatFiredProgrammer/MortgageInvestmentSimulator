using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Class representing a single purchase of SP 500.
    ///     We allow a fractional number of shares as a simplification (much like a mutual fund).
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Sp500
    {
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

        /// <inheritdoc />
        public override string ToString() => $"{Shares:N2} @ {PurchasePrice:C2}";
    }
}
