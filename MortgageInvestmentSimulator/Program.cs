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
                SimulationYears = 20,
                HomeValue = 200000,
                MonthlyIncome = 2000,
                StartingCash = 0,
                StockPercentage = .80m,
                MortgageTerm = MortgageTerm.ThirtyYear,
                OriginationFee = .0125m,
                ShouldPayOffHouseAtCompletion = false,
                AllowRefinance = true,
                AllowMortgageInterestDeduction = true,
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
                scenario.AvoidMortgage = false;
                var resultInvesting = simulator.Run(scenario);
                scenario.AvoidMortgage = true;
                var resultAvoidMortgage = simulator.Run(scenario);

                output.WriteLine("***** Simulation Results *****");
                output.WriteLine(scenario.GetSummary());
                output.WriteLine("***** Investing *****");
                output.WriteLine(resultInvesting.ToString());
                output.WriteLine("***** Avoiding Mortgage *****");
                output.WriteLine(resultAvoidMortgage.ToString());
                output.WriteLine("***** Summary *****");
                var failurePercentage = (decimal)resultInvesting.Failed / (resultInvesting.Failed + resultInvesting.Success);
                output.WriteLine(
                    $"* Investing had an {resultInvesting.AverageNetGain - resultAvoidMortgage.AverageNetGain:C0} average change in net worth and failed {failurePercentage:P2} of the time.");
                if (resultInvesting.NetLossCount > 0)
                    output.WriteLine($"* Investing had {resultInvesting.NetLossCount:N0} of {resultInvesting.Total:N0} simulations ({(decimal)resultInvesting.NetLossCount/resultInvesting.Total:P2}) resulting in a loss in net worth");
                if (resultAvoidMortgage.NetLossCount > 0)
                    output.WriteLine($"* Avoiding mortgage had {resultAvoidMortgage.NetLossCount:N0} of {resultAvoidMortgage.Total:N0} simulations ({(decimal)resultAvoidMortgage.NetLossCount / resultAvoidMortgage.Total:P2}) resulting in a loss in net worth");
            }
            catch (Exception exception)
            {
                output.WriteLine("***** Failed *****");
                output.WriteLine($"{exception.GetType()}: {exception.Message}");
            }
        }
    }
}
