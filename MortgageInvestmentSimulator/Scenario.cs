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
        public MonthYear End { get; set; } = new MonthYear(1, 2019);

        /// <summary>
        ///     Gets or sets the simulation strategy.
        /// </summary>
        /// <value>The strategy.</value>
        public Strategy? Strategy { get; set; }

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
        ///     Gets or sets the mortgage term in years.
        /// </summary>
        /// <value>The mortgage years.</value>
        public MortgageTerm MortgageTerm { get; set; } = MortgageTerm.ThirtyYear;

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine($"Starts {Start} and ends {End}");
            text.AppendLine($"Each simulation is {SimulationYears} years");
            text.AppendLine(Strategy != null ? $"Strategy is {Strategy}" : "All strategies");
            text.AppendLine($"Home value is {HomeValue:C0}");
            if (MonthlyIncome > 0)
                text.AppendLine($"Monthly income is {MonthlyIncome:C0}");
            if (StartingCash > 0)
                text.AppendLine($"Starting cash is {StartingCash:C0}");
            text.AppendLine($"{MortgageTerm} mortgage");
            text.AppendLine(MortgageInterestRate != null ? $"Mortgage interest rate is {MortgageInterestRate:P2}" : "Mortgage interest rate is monthly average.");
            return text.ToString();
        }
    }
}
