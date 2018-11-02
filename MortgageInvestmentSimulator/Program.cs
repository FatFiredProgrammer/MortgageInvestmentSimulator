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
                SimulationYears = 10,
                HomeValue = 150000,
                MonthlyIncome = 7083,
                StartingCash = 0,
                StockPercentage = .80m,
                MortgageTerm = MortgageTerm.ThirtyYear,
                OriginationFee = 0m,
                ShouldPayOffHouseAtCompletion = false,
                AllowRefinance = false,
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

                output.WriteLine("# Simulation");
                output.WriteLine(null);
                output.WriteLine(scenario.GetSummary().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));
                output.WriteLine(null);
                output.WriteLine("# Summary");
                output.WriteLine(null);
                var failurePercentage = (decimal)resultInvesting.Failed / (resultInvesting.Failed + resultInvesting.Success);
                output.WriteLine(
                    $"* Investing had an {resultInvesting.AverageNetGain - resultAvoidMortgage.AverageNetGain:C0} average improvement in net worth over avoiding a mortgage. ");
                if(resultAvoidMortgage.AverageNetGain > 0)
                    output.WriteLine(
                        $"* Investing had an {(resultInvesting.AverageNetGain - resultAvoidMortgage.AverageNetGain) / (resultAvoidMortgage.AverageNetGain):P2} average improvement in net worth over avoiding a mortgage. ");
                output.WriteLine(
                    $"* Investing failed {failurePercentage:P2} of the time.");
                if (resultInvesting.NetLossCount > 0)
                    output.WriteLine($"* Investing had {resultInvesting.NetLossCount:N0} of {resultInvesting.Total:N0} simulations ({(decimal)resultInvesting.NetLossCount / resultInvesting.Total:P2}) resulting in a loss in net worth.");
                var worst = resultInvesting.FindWorst();
                if (worst.HasValue)
                    output.WriteLine($"* Investing had a worst loss of {worst.Value.Value.ToDollarCents():C0} net worth in simulation starting {worst.Value.Key}.");
                var best = resultInvesting.FindBest();
                if (best.HasValue)
                    output.WriteLine($"* Investing had best gain of {best.Value.Value.ToDollarCents():C0} net worth in simulation starting {best.Value.Key}.");
                if (resultAvoidMortgage.NetLossCount > 0)
                    output.WriteLine($"* Avoiding mortgage had {resultAvoidMortgage.NetLossCount:N0} of {resultAvoidMortgage.Total:N0} simulations ({(decimal)resultAvoidMortgage.NetLossCount / resultAvoidMortgage.Total:P2}) resulting in a loss in net worth.");
                worst = resultAvoidMortgage.FindWorst();
                if (worst.HasValue)
                    output.WriteLine($"* Avoiding mortgage had a worst gain/loss of {worst.Value.Value.ToDollarCents():C0} net worth in simulation starting {worst.Value.Key}.");
                best = resultAvoidMortgage.FindBest();
                if (best.HasValue)
                    output.WriteLine($"* Avoiding mortgage had best gain/loss of {best.Value.Value.ToDollarCents():C0} net worth in simulation starting {best.Value.Key}.");
                output.WriteLine(null);
                output.WriteLine("# Investing");
                output.WriteLine(null);
                output.WriteLine(resultInvesting.ToString().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));
                output.WriteLine(null);
                output.WriteLine("# Avoiding Mortgage");
                output.WriteLine(null);
                output.WriteLine(resultAvoidMortgage.ToString().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));
            }
            catch (Exception exception)
            {
                output.WriteLine("===== Failed =====");
                output.WriteLine($"{exception.GetType()}: {exception.Message}");
            }
        }
    }
}
