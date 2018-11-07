using System;
using System.Diagnostics;
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
                //Date = new MonthYear(4, 1972),
                //Start =  new MonthYear(9, 1991),
                //End =  new MonthYear(1, 1984),
                SimulationYears = 15,
                HomeValue = 200000,
                MonthlyIncome = 2500,
                //StartingCash = 200000,
                StockPercentage = .8m,

                //MortgageTerm = MortgageTerm.FifteenYear,
                //OriginationFee = .0125,
                // ShouldPayOffHouseAtCompletion = true,
                //ExistingLoanYears = 20,
                //CashOutAtRefinance = true,
                //AllowRefinance = false,
                AllowMortgageInterestDeduction = false,
                // AllowRefinance = false,
                MonthlyIncomeStrategy = MonthlyIncomeStrategy.MortgagePlus50Percent,
                //ShouldAdjustForInflation = true,
                // ExtraPayment = 400,

            };

            var verbose = args.Any(c => string.Equals(c, "-v", StringComparison.CurrentCultureIgnoreCase)) || args.Any(c => string.Equals(c, "--verbose", StringComparison.CurrentCultureIgnoreCase));
#if DEBUG

            // var output = verbose ? new VerboseOutput() : (IOutput)new DebugOutput();
            // var output = verbose ? new VerboseOutput() : (IOutput)new ConsoleOutput();
            var output = verbose ? new TempFileOutput() : (IOutput)new TempFileOutput();
#else
            var output = verbose? new VerboseOutput() : (IOutput)new ConsoleOutput();
#endif

            try
            {
                var simulator = new Simulator(output);
                var summary = simulator.Run(scenario);
                foreach (var item in summary.Items.OrderBy(c => c.Start))
                {
                    Debug.WriteLine(item.ToString());
                    output.VerboseLine(item.ToString());
                }
                output.WriteLine(summary.ToString());
            }
            catch (Exception exception)
            {
                output.WriteLine("===== Failed =====");
                output.WriteLine($"{exception.GetType()}: {exception.Message}");
            }
            finally
            {
                output.Flush();
            }
        }
    }
}
