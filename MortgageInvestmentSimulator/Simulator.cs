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

        public void Run(Scenario scenario)
        {
            Output.WriteLine("Mortgage vs Investment Simulator v1.0");
            Output.WriteLine("Current Scenario:");
            Output.WriteLine(scenario.ToString());
            var start = scenario.Start;
            var success = 0;
            var failed = 0;
            while (start <= scenario.End)
            {
                try
                {
                    var simulation = new Simulation(Output);
                    simulation.Run(scenario, start);
                    success++;
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"*** Simulation Failed in {start} ***");
                    Output.VerboseLine(exception.Message);
                    failed++;
                }

                Output.WriteLine($"*** Simulator completed with {success} successful and {failed} failures ***");
                start = start.AddMonths(1);
            }
        }
    }
}
