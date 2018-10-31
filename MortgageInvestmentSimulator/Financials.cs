using System.Collections.Generic;
using System.Text;

namespace MortgageInvestmentSimulator
{
    public sealed class Financials
    {
        /// <summary>
        ///     Gets or sets the cost of the home we have.
        ///     Down payment/etc are not included since it doesn't really matter.
        /// </summary>
        /// <value>The home cost.</value>
        public decimal HomeValue { get; set; }

        /// <summary>
        ///     Gets or sets the cash on hand.
        /// </summary>
        /// <value>The cash.</value>
        public decimal Cash { get; set; }

        /// <summary>
        /// Gets or sets the mortgage if we have one..
        /// </summary>
        /// <value>The mortgage.</value>
        public Mortgage Mortgage { get; set; }
        /// <summary>
        /// Gets or sets the current year's taxes.
        /// </summary>
        /// <value>The current taxes.</value>
        public Taxes CurrentTaxes { get; set; } = new Taxes();

		public List<Treasury> Bonds { get; } = new List<Treasury>();

        public List<Sp500> Stocks { get; } = new List<Sp500>();

        /// <summary>
        /// Gets or sets the current previous year's taxes.
        /// </summary>
        /// <value>The current taxes.</value>
        public Taxes PreviousTaxes { get; set; }

        public int MonthsUntilRebalance { set; get; }

        public decimal GetNetWorth(MonthYear now)
        {
            return HomeValue + Cash - (Mortgage?.Balance ?? 0m);
        }

        public override string ToString()
        {
            var text = new StringBuilder();
//            text.AppendLine($"Net worth is {NetWorth:C0}");
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (Cash > 0)
                text.AppendLine($"Cash on hand is {Cash:C0}");

            // TODO: Code needs work
#if false
                 if (MortgageAmount > 0)
                text.AppendLine($"{MortgageYears} year loan for {MortgageAmount:C0} @ {MortgageInterestRate:P2}");
            if (MonthlyPayment > 0)
                text.AppendLine($"Monthly mortgage payment is {MonthlyPayment:C0}");
            if (MortgageBalance > 0)
                text.AppendLine($"Mortgage balance is {MortgageBalance:C0}"); 
#endif

            if (CurrentTaxes != null)
            {
                text.AppendLine("Current Year Taxes");
                text.AppendLine(CurrentTaxes.ToString());
            }
            if (PreviousTaxes != null)
            {
                text.AppendLine("Previous Year Taxes");
                text.AppendLine(PreviousTaxes.ToString());
            }
            return text.ToString();
        }
    }
}
