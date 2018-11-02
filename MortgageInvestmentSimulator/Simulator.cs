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

        public Summary Run(Scenario scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException(nameof(scenario));

            scenario = new Scenario(scenario);
            scenario.Clean();

            Output.WriteLine("Mortgage vs Investment Simulator v1.0");
            Output.WriteLine(null);
            Output.WriteLine("Current Scenario:");
            Output.WriteLine(scenario.ToString());
            Output.WriteLine(null);
            Output.WriteLine("# Simulation");
            Output.WriteLine(null);
            Output.WriteLine(scenario.GetSummary().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));

            var simulator = new Simulator(Output);
            var invest = simulator.Run(scenario, Strategy.Invest);
            var avoidMortgage = simulator.Run(scenario, Strategy.AvoidMortgage);
            return new Summary(scenario, invest, avoidMortgage)
            {
                Start = scenario.Start,
                End = scenario.End,
            };
        }

        private Results Run(Scenario scenario, Strategy strategy)
        {
            var results = new Results
            {
                Strategy = strategy,
            };
            var now = scenario.Start;
            while (now <= scenario.End)
            {
                var simulation = new Simulation(scenario, strategy, Output);
                try
                {
                    var result = simulation.Run(now);
                    results.Add(result);
                }
                catch (SimulationInvalidException exception)
                {
                    // This is a simulation where, for example, we simply don't make enough money to pay the mortgage
                    // on the first month. I.e. We should never be given a loan.
                    Output.WriteLine($"=== Simulation {now} invalid : {exception.Message} ===");
                    results.Add(new Result(now, Outcome.Invalid));
                }
                catch (SimulationFailedException exception)
                {
                    Output.WriteLine($"=== Simulation {now} failed {exception.When} : {exception.Message} ===");
                    Output.VerboseLine($"{exception.GetType()}: {exception.Message}");

                    results.Add(new Result(now, Outcome.Failed)
                    {
                        Error = exception.Message,
                        WhenError = exception.When,
                        Status = simulation.GetStatus(exception.When)
                    });
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"=== Simulation {now} error : {exception.Message} ===");
                    Output.VerboseLine($"{exception.GetType()}: {exception.Message}");
                    results.Add(new Result(now, Outcome.Error)
                    {
                        Error = exception.Message,
                    });
                    throw;
                }

                now = now.AddMonths(1);
            }

            Output.WriteLine($"{results}");
            return results;
        }
    }
}
