using System.Text;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     A description of the scenario that we are running.
    /// </summary>
    public sealed class Scenario
    {
        /// <summary>
        ///     The starting year of the simulation.
        /// </summary>
        /// <value>The start.</value>
        public MonthYear Start { get; set; } = new MonthYear(1, 1972);

        /// <summary>
        ///     The ending year of the simulation.
        ///     The simulation may actually end earlier if we don't have data or we can't fulfill other conditions.
        /// </summary>
        /// <value>The start.</value>
        public MonthYear End { get; set; } = new MonthYear(1, 2018);

        /// <summary>
        ///     Gets or sets the number of years that any particular simulation runs.
        /// </summary>
        /// <value>The mortgage years.</value>
        public int SimulationYears { get; set; } = 10;

        /// <summary>
        ///     Gets or sets the cost of the home we have.
        ///     Down payment/etc are not included since it doesn't really matter.
        /// </summary>
        /// <value>The home cost.</value>
        public decimal HomeValue { get; set; } = 200000;

        /// <summary>
        ///     Gets or sets the monthly income that we have to either pay the mortgage or invest.
        /// </summary>
        /// <value>The monthly income.</value>
        public decimal MonthlyIncome { get; set; } = 1500;

        /// <summary>
        ///     Gets or sets the amount of cash we have at the start.
        /// </summary>
        /// <value>The monthly income.</value>
        public decimal StartingCash { get; set; } = 200000;

        /// <summary>
        ///     Gets or sets the mortgage interest rate.
        ///     If not specified, we will use the average rate at the start month.
        /// </summary>
        /// <value>The mortgage interest rate.</value>
        public decimal? MortgageInterestRate { get; set; }

        /// <summary>
        ///     Gets or sets the percentage we invest in stocks.
        /// </summary>
        /// <value>The stock percentage.</value>
        public decimal StockPercentage { get; set; } = .80m;

        /// <summary>
        ///     Gets or sets the mortgage term in years.
        /// </summary>
        /// <value>The mortgage years.</value>
        public MortgageTerm MortgageTerm { get; set; } = MortgageTerm.ThirtyYear;

        /// <summary>
        /// Gets or sets the origination fee.
        /// This is a percentage of the total loan.
        /// The amount is added into the loan.
        /// </summary>
        /// <value>The origination fee.</value>
        public decimal OriginationFee { get; set; } = .0125m;

        /// <summary>
        ///     Gets or sets a value indicating whether should pay off house at end of simulation.
        /// </summary>
        /// <value><c>true</c> if should pay off house; otherwise, <c>false</c>.</value>
        public bool ShouldPayOffHouse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should avoid mortgage and pay mortgage down when possible.
        /// </summary>
        /// <value><c>true</c> if avoid mortgage; otherwise, <c>false</c>.</value>
        public bool AvoidMortgage { get; set; }

        /// <summary>
        /// Gets or sets the number of months between re-balancing.
        /// </summary>
        /// <value>The number months.</value>
        public int? RebalanceMonths { get; set; } = 12;

        /// <summary>
        /// Gets or sets a value indicating whether allow refinance.
        /// </summary>
        /// <value><c>true</c> if allow refinance; otherwise, <c>false</c>.</value>
        public bool AllowRefinance { get; set; } = true;

        /// <summary>
        /// Gets or sets the refinance pay back months.
        /// We refinance if we regain our refinancing fee within this number of months.
        /// </summary>
        /// <value>The refinance pay back months.</value>
        public int RefinancePayBackMonths { get; set; } = 60;


        /// <summary>
        /// Gets or sets the marginal tax rate.
        /// This is used to calculate the value of the mortgage interest deduction
        /// </summary>
        /// <value>The marginal tax rate.</value>
        public decimal MarginalTaxRate { get; set; } = .38m;

        /// <summary>
        /// Gets or sets a value indicating whether mortgage interest deduction is used.
        /// </summary>
        /// <value><c>true</c> if mortgage interest deduction; otherwise, <c>false</c>.</value>
        public bool AllowMortgageInterestDeduction { get; set; }



        /// <summary>
        /// Gets or sets the dividend tax rate.
        /// </summary>
        /// <value>The dividend tax rate.</value>
        public decimal DividendTaxRate { get; set; } = .15m;

        /// <summary>
        /// Gets or sets the capital gains tax rate.
        /// </summary>
        /// <value>The capital gains tax rate.</value>
        public decimal CapitalGainsTaxRate { get; set; } = .15m;

        /// <summary>
        /// Gets or sets the treasury interest tax rate.
        /// Normally, this is only the fed rate. Not the state rate.
        /// </summary>
        /// <value>The treasury interest tax rate.</value>
        public decimal TreasuryInterestTaxRate { get; set; } = .32m;

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine($"Starts {Start} and ends {End}");
            text.AppendLine($"Each simulation is {SimulationYears} years");
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (MonthlyIncome > 0)
                text.AppendLine($"Monthly income is {MonthlyIncome:C0}");
            if (StartingCash > 0)
                text.AppendLine($"Starting cash is {StartingCash:C0}");
            text.AppendLine($"{MortgageTerm} mortgage");
            text.AppendLine(MortgageInterestRate != null ? $"Mortgage interest rate is {MortgageInterestRate:P2}" : "Mortgage interest rate is monthly average.");
            if (StockPercentage > 0)
                text.AppendLine($"Invest {StockPercentage:P0} in stocks");
            if (ShouldPayOffHouse)
                text.AppendLine("Must pay off house at end of simulation");

            return text.ToString();
        }
    }
}
