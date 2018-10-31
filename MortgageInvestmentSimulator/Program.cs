using System;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Program entry point.
    /// </summary>
    internal class Program
    {
        private static void Main()
        {
            IOutput output = new ConsoleOutput();
            try
            {
                var scenario = new Scenario();

                var simulator = new Simulator(output);
                simulator.Run(scenario);
            }
            catch (Exception exception)
            {
                output.WriteLine("*** Simulator Failed ***");
                output.WriteLine(exception.Message);
            }
        }
    }
}
