using System;

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
                Start = MonthYear.Constrain(scenario.Date ?? scenario.Start),
                End = MonthYear.Constrain(scenario.Date ?? scenario.End),
                AvoidMortgage = scenario.AvoidMortgage,
            };
            var now = result.Start;
            while (now <= result.End)
            {
                var simulation = new Simulation(scenario, Output);
                try
                {
                    var netWorth = simulation.Run(now);
                    result.NetWorths.Add(new MonthYear(now), netWorth);
                    result.NetGains.Add(new MonthYear(now), netWorth - simulation.ExternalCapital);
                    result.Success++;
                }
                catch (SimulationInvalidException exception)
                {
                    // This is a simulation where, for example, we simply don't make enough money to pay the mortgage
                    // on the first month. I.e. We should never be given a loan.
                    Output.WriteLine($"=== Simulation {now} invalid : {exception.Message} ===");
                    result.Invalid++;
                }
                catch (SimulationFailedException exception)
                {
                    Output.WriteLine($"=== Simulation {now} failed {exception.When} : {exception.Message} ===");
                    Output.VerboseLine($"{exception.GetType()}: {exception.Message}");
                    Output.WriteLine($"{simulation.GetStatus(exception.When)}");
                    result.Errors.Add($"{now}: {exception.GetType()}: {exception.Message}{Environment.NewLine}{simulation.GetStatus(exception.When)}");
                    result.Failed++;
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"=== Simulation {now} failed : {exception.Message} ===");
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
