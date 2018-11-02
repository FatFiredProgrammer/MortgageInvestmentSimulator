using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     A description of the scenario that we are running.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Scenario
    {
        public Scenario()
        {
        }

        public Scenario(Scenario other)
        {
            if (other == null)
                return;

            AvoidMortgage = other.AvoidMortgage;
            Start = other.Start;
            End = other.End;
            Date = other.Date;
            SimulationYears = other.SimulationYears;
            HomeValue = other.HomeValue;
            MonthlyIncome = other.MonthlyIncome;
            StartingCash = other.StartingCash;
            MortgageInterestRate = other.MortgageInterestRate;
            StockPercentage = other.StockPercentage;
            MortgageTerm = other.MortgageTerm;
            OriginationFee = other.OriginationFee;
            ShouldPayOffHouseAtCompletion = other.ShouldPayOffHouseAtCompletion;
            RebalanceMonths = other.RebalanceMonths;
            AllowRefinance = other.AllowRefinance;
            RefinancePayBackMonths = other.RefinancePayBackMonths;
            MarginalTaxRate = other.MarginalTaxRate;
            AllowMortgageInterestDeduction = other.AllowMortgageInterestDeduction;
            DividendTaxRate = other.DividendTaxRate;
            CapitalGainsTaxRate = other.CapitalGainsTaxRate;
            TreasuryInterestTaxRate = other.TreasuryInterestTaxRate;
            MinimumCash = other.MinimumCash;
            MinimumBond = other.MinimumBond;
            MinimumStock = other.MinimumStock;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether we should avoid mortgage and pay mortgage down when possible.
        /// </summary>
        /// <value><c>true</c> if avoid mortgage; otherwise, <c>false</c>.</value>
        public bool AvoidMortgage { get; set; }

        /// <summary>
        ///     The starting year of the simulation.
        /// </summary>
        /// <value>The start.</value>
        public MonthYear Start { get; set; } = MonthYear.Min;

        /// <summary>
        ///     The ending year of the simulation.
        ///     The simulation may actually end earlier if we don't have data or we can't fulfill other conditions.
        /// </summary>
        /// <value>The start.</value>
        public MonthYear End { get; set; } = MonthYear.Max.AddYears(-5);

        /// <summary>
        ///     Gets or sets the date. If not null, the simulation only runs on this particular date.
        ///     Useful for debugging.
        /// </summary>
        /// <value>The date.</value>
        public MonthYear Date { get; set; }

        /// <summary>
        ///     Gets or sets the number of years that any particular simulation runs.
        /// </summary>
        /// <value>The mortgage years.</value>
        public int SimulationYears { get; set; } = 20;

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
        ///     Gets or sets the mortgage interest rate used when no other data is present.
        ///     This value was the median monthly rate from 1970 - 2018 for a 30 yr loan.
        /// </summary>
        /// <value>The mortgage interest rate.</value>
        public decimal? MortgageInterestRate { get; set; } = .0768m;

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
        ///     Gets or sets the origination fee.
        ///     This is a percentage of the total loan.
        ///     The amount is added into the loan.
        /// </summary>
        /// <value>The origination fee.</value>
        public decimal OriginationFee { get; set; } = .0125m;

        /// <summary>
        ///     Gets or sets a value indicating whether should pay off house at end of simulation.
        /// </summary>
        /// <value><c>true</c> if should pay off house; otherwise, <c>false</c>.</value>
        public bool ShouldPayOffHouseAtCompletion { get; set; } = true;

        /// <summary>
        ///     Gets or sets the number of months between re-balancing.
        /// </summary>
        /// <value>The number months.</value>
        public int? RebalanceMonths { get; set; } = 12;

        /// <summary>
        ///     Gets or sets a value indicating whether allow refinance.
        /// </summary>
        /// <value><c>true</c> if allow refinance; otherwise, <c>false</c>.</value>
        public bool AllowRefinance { get; set; } = true;

        /// <summary>
        ///     Gets or sets the refinance pay back months.
        ///     We refinance if we regain our refinancing fee within this number of months.
        /// </summary>
        /// <value>The refinance pay back months.</value>
        public int RefinancePayBackMonths { get; set; } = 4 * 12;

        /// <summary>
        ///     Gets or sets the marginal tax rate.
        ///     This is used to calculate the value of the mortgage interest deduction
        /// </summary>
        /// <value>The marginal tax rate.</value>
        public decimal MarginalTaxRate { get; set; } = .38m;

        /// <summary>
        ///     Gets or sets a value indicating whether mortgage interest deduction is used.
        /// </summary>
        /// <value><c>true</c> if mortgage interest deduction; otherwise, <c>false</c>.</value>
        public bool AllowMortgageInterestDeduction { get; set; } = true;

        /// <summary>
        ///     Gets or sets the dividend tax rate.
        /// </summary>
        /// <value>The dividend tax rate.</value>
        public decimal DividendTaxRate { get; set; } = .15m;

        /// <summary>
        ///     Gets or sets the capital gains tax rate.
        /// </summary>
        /// <value>The capital gains tax rate.</value>
        public decimal CapitalGainsTaxRate { get; set; } = .15m;

        /// <summary>
        ///     Gets or sets the treasury interest tax rate.
        ///     Normally, this is only the fed rate. Not the state rate.
        /// </summary>
        /// <value>The treasury interest tax rate.</value>
        public decimal TreasuryInterestTaxRate { get; set; } = .32m;

        /// <summary>
        ///     Gets or sets the minimum cash.
        ///     We don't invest or buy bonds if our cash balance falls below this.
        ///     Transaction costs would kill us and it just adds a lot of nickels and dimes to the simulation.
        /// </summary>
        /// <value>The minimum cash.</value>
        public decimal MinimumCash { get; set; } = 1000;

        /// <summary>
        ///     Gets or sets the minimum bond purchase.
        /// </summary>
        /// <value>The minimum bond.</value>
        public decimal MinimumBond { get; set; } = 500;

        /// <summary>
        ///     Gets or sets the minimum stock purchase
        /// </summary>
        /// <value>The minimum stock.</value>
        public decimal MinimumStock { get; set; } = 500;

        public string GetSummary()
        {
            var text = new StringBuilder();
            text.AppendLine(Date != null ? $"{SimulationYears} year (max) simulation in {Date}" : $"{SimulationYears} year (max) simulations starting {Start} until {End}");
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (StartingCash > 0)
                text.AppendLine($"Starting cash is {StartingCash:C0}");
            if (MonthlyIncome > 0)
                text.AppendLine($"Monthly income is {MonthlyIncome:C0}");
            text.AppendLine($"{MortgageTerm.GetYears()} year mortgage");
            if (StockPercentage > 0)
                text.AppendLine($"Invest {StockPercentage:P0} in stocks");

            return text.ToString().TrimEnd();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine(Date != null ? $"One simulation in {Date}" : $"Starts {Start} and ends {End}");
            text.AppendLine($"Each simulation is {SimulationYears} years");
            text.AppendLine($"Home value is {HomeValue:C0}");
            text.AppendLine(AvoidMortgage ? "*Should avoid having a mortgage*" : "*Should invest money*");
            text.AppendLine($"Starting cash is {StartingCash:C0}");
            text.AppendLine($"Monthly income is {MonthlyIncome:C0}");
            text.AppendLine($"{MortgageTerm.GetYears()} year mortgage");
            text.AppendLine(MortgageInterestRate != null ? $"Mortgage interest rate is {MortgageInterestRate:P2}" : "Mortgage interest rate is monthly average");
            text.AppendLine($"{OriginationFee:P2} origination fee on loan");
            text.AppendLine($"Invest {StockPercentage:P0} in stocks");
            text.AppendLine($"{(ShouldPayOffHouseAtCompletion ? "Must" : "Need not")} pay off house at end of simulation");
            text.AppendLine(AllowRefinance ? $"Allow mortgage refinance if costs recouped in {RefinancePayBackMonths} months" : "Do not allow mortgage refinancing");
            text.AppendLine($"{MinimumCash:C0} minimum cash to invest");
            text.AppendLine($"{MinimumStock:C0} minimum stock to invest");
            text.AppendLine($"{MinimumBond:C0} minimum bond to invest");
            text.AppendLine(RebalanceMonths.HasValue ? $"Should rebalance portfolio every {RebalanceMonths} months" : "No rebalancing of portfolio");
            text.AppendLine($"{(AllowMortgageInterestDeduction ? "Allow" : "Do NOT allow")} mortgage interest deduction with a {MarginalTaxRate:P2} marginal tax rate");
            text.AppendLine($"{DividendTaxRate:P2} dividend tax rate");
            text.AppendLine($"{CapitalGainsTaxRate:P2} capital gains tax rate");
            text.AppendLine($"{TreasuryInterestTaxRate:P2} treasury tax rate");

            return text.ToString().TrimEnd();
        }
    }
}
