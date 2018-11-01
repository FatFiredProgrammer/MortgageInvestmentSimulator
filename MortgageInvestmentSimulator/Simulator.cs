using System;
using System.Collections.Generic;
using System.Linq;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Simulator which runs the scenario over a number of different starting years.
    /// </summary>
    public sealed class Simulator
    {
        public Simulator(IOutput output)
            => Output = output ?? throw new ArgumentNullException(nameof(output));

        public IOutput Output { get; }


        public Result Run(Scenario scenario)
        {
            Output.WriteLine("Mortgage vs Investment Simulator v1.0");
            Output.WriteLine(null);
            Output.WriteLine("Current Scenario:");
            Output.WriteLine(scenario.ToString());
            var result = new Result
            {
                Start = MonthYear.Constrain(scenario.Start),
                End = MonthYear.Constrain(scenario.End),
                AvoidMortgage = scenario.AvoidMortgage,
            };
            var now = result.Start;
            while (now <= result.End)
            {
                var simulation = new Simulation(scenario, Output);
                try
                {
                    var netWorth = simulation.Run(now);
                    result.NetWorths.Add(netWorth);
                    result.NetGains.Add(netWorth - simulation.ExternalCapital);
                   result.Success++;
                }
                catch (SimulationFailedException exception)
                {
                    Output.WriteLine($"*** Simulation {now} failed {exception.When} : {exception.Message} ***");
                    Output.VerboseLine($"{exception.GetType()}: {exception.Message}");
                    Output.WriteLine($"{simulation.GetStatus(exception.When)}");
                    result.Errors.Add($"{now}: {exception.GetType()}: {exception.Message}{Environment.NewLine}{simulation.GetStatus(exception.When)}");
                    result.Failed++;
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"*** Simulation {now} failed : {exception.Message} ***");
                    Output.VerboseLine($"{exception.GetType()}: {exception.Message}");
                    result.Errors.Add($"{now}: {exception.GetType()}: {exception.Message}");
                    result.Failed++;
                    throw;
                }

                now = now.AddMonths(1);
            }

            Output.WriteLine($"{result}");
            return result;
        }
    }
}
