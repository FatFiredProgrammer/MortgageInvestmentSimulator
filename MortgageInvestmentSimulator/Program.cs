using System;

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
