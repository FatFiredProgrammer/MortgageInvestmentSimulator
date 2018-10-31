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

        public decimal MonthlyPayment { get; set; }

        public int MortgageYears { get; set; }

        public decimal MortgageInterestRate { get; set; }

        public decimal MortgageAmount { get; set; }

        public decimal MortgageBalance { get; set; }

        public decimal NetWorth
            => HomeValue + Cash - MortgageBalance;

        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine($"Net worth is {NetWorth:C0}");
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (Cash > 0)
                text.AppendLine($"Cash on hand is {Cash:C0}");
            if (MortgageAmount > 0)
                text.AppendLine($"{MortgageYears} year loan for {MortgageAmount:C0} @ {MortgageInterestRate:P2}");
            if (MonthlyPayment > 0)
                text.AppendLine($"Monthly mortgage payment is {MonthlyPayment:C0}");
            if (MortgageBalance > 0)
                text.AppendLine($"Mortgage balance is {MortgageBalance:C0}");
            return text.ToString();
        }
    }
}
