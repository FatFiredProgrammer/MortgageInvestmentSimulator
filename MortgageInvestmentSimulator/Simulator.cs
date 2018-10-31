using System;

namespace MortgageInvestmentSimulator
{
    public sealed class Simulator
    {
        public Simulator(IOutput output)
            => Output = output ?? throw new ArgumentNullException(nameof(output));

        public IOutput Output { get; }

        public void Run(Scenario scenario)
        {
            Output.WriteLine("Mortgage vs Investment Simulator v1.0");
            Output.WriteLine("Current Scenario:");
            Output.WriteLine(scenario.ToString());
            var start = scenario.Start;
            while (start <= scenario.End)
            {
                start = start.AddMonths(1);
                if (scenario.Strategy != null)
                {
                    var simulation = new Simulation(Output);
                    simulation.Run(scenario, start, scenario.Strategy.Value);
                }
                else
                {
                    foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
                    {
                        var simulation = new Simulation(Output);
                        simulation.Run(scenario, start, strategy);
                    }
                }
            }

            Output.WriteLine("*** Simulator Successful ***");
        }
    }
}
