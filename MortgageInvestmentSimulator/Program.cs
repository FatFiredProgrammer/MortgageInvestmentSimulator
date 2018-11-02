using System;
using System.Linq;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Program entry point.
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     Defines the entry point of the application.
        /// </summary>
        private static void Main(string[] args)
        {
            var scenario = new Scenario
            {
                // Date = new MonthYear(1, 1974),
                 Start =  new MonthYear(1, 1973),
                 End =  new MonthYear(1, 1984),
                SimulationYears = 15,
                HomeValue = 200000,
                MonthlyIncome = 3000,
                StartingCash = 0,
                StockPercentage = .80m,
                MortgageTerm = MortgageTerm.ThirtyYear,
                // OriginationFee = .0125,
                ShouldPayOffHouseAtCompletion = false,
                // AllowRefinance = false,
                // AllowMortgageInterestDeduction = true,
            };

            var verbose = args.Any(c => string.Equals(c, "-v", StringComparison.CurrentCultureIgnoreCase)) || args.Any(c => string.Equals(c, "--verbose", StringComparison.CurrentCultureIgnoreCase));
#if DEBUG

            // var output = verbose ? new VerboseOutput() : (IOutput)new DebugOutput();

            var output = verbose ? new VerboseOutput() : (IOutput)new ConsoleOutput();
#else
            var output = verbose? new VerboseOutput() : (IOutput)new ConsoleOutput();
#endif
            try
            {
                var simulator = new Simulator(output);
                var summary = simulator.Run(scenario);
                output.WriteLine(summary.ToString());
            }
            catch (Exception exception)
            {
                output.WriteLine("===== Failed =====");
                output.WriteLine($"{exception.GetType()}: {exception.Message}");
            }
        }
    }
}
