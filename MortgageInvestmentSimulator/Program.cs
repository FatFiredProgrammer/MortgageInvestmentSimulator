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
                SimulationYears = 30,
                HomeValue = 200000,
                MonthlyIncome = 0,
                StartingCash = 200000,
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
                var failurePercentage = resultInvesting.Failed / (resultInvesting.Failed + resultInvesting.Success);
                output.WriteLine(
                    $"Investing had an average {resultInvesting.AverageNetGain - resultAvoidMortgage.AverageNetGain:C0} better/worse change in net worth and failed {failurePercentage:P2} of the time.");
                if (resultInvesting.NetLossCount > 0)
                    output.WriteLine($"Investing had {resultInvesting.NetLossCount:N0} simulations resulting in a loss");
                if (resultAvoidMortgage.NetLossCount > 0)
                    output.WriteLine($"Avoiding mortgage had {resultAvoidMortgage.NetLossCount:N0} simulations resulting in a loss");
            }
            catch (Exception exception)
            {
                output.WriteLine("***** Failed *****");
                output.WriteLine($"{exception.GetType()}: {exception.Message}");
            }
        }
    }
}
