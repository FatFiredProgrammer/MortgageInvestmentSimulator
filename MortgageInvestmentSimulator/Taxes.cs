namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     An accumulation of taxes for one year.
    /// </summary>
    public sealed class Taxes
    {
        /// <summary>
        /// Gets or sets the mortgage interest amount.
        /// Used to calculate what we saved due to mortgage interest deduction.
        /// </summary>
        /// <value>The mortgage interest.</value>
        public decimal MortgageInterest { get; set; }

        public decimal Dividends { get; set; }

        public decimal CapitalGains { get; set; }
        public decimal TreasuryInterest { get; set; }

        public decimal GetTaxesOwed(Scenario scenario)
        {
            return 0;

        }
        public decimal GetMortgage(Scenario scenario)
        {
            return 0;
        }

        public override string ToString()
        {
            // TODO: Code needs work
#if false
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
#endif
            return string.Empty;
        }
    }
}

// TODO: 