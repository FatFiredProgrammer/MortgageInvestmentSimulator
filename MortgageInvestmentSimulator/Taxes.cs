using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     An accumulation of taxes for one year.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Taxes
    {
        /// <summary>
        ///     Gets or sets the mortgage interest amount.
        ///     Used to calculate what we saved due to mortgage interest deduction.
        /// </summary>
        /// <value>The mortgage interest.</value>
        public decimal MortgageInterest { get; set; }

        /// <summary>
        ///     Gets or sets the dividends earned this year.
        /// </summary>
        /// <value>The dividends.</value>
        public decimal Dividends { get; set; }

        /// <summary>
        ///     Gets or sets the capital gains this year.
        /// </summary>
        /// <value>The capital gains.</value>
        public decimal CapitalGains { get; set; }

        /// <summary>
        ///     Gets or sets the treasury interest this year.
        /// </summary>
        /// <value>The treasury interest.</value>
        public decimal TreasuryInterest { get; set; }

        public override string ToString()
        {
            var text = new StringBuilder();
            if (MortgageInterest > 0)
                text.AppendLine($"{MortgageInterest:C0} mortgage interest");
            if (Dividends > 0)
                text.AppendLine($"{Dividends:C0} dividends");
            if (CapitalGains > 0)
                text.AppendLine($"{CapitalGains:C0} capital gains");
            if (TreasuryInterest > 0)
                text.AppendLine($"{TreasuryInterest:C0} bond interest");
            return text.ToString();
        }
    }
}
