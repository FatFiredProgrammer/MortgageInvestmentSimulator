using System;
using System.Diagnostics;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Output to console/debug.
    ///     Our standard output method.
    /// </summary>
    public sealed class ConsoleOutput : IOutput
    {
        /// <inheritdoc />
        public void VerboseLine(string text)
        {
            Debug.WriteLine(text ?? string.Empty);
        }

        /// <inheritdoc />
        public void WriteLine(string text)
        {
            Console.WriteLine(text ?? string.Empty);
            Debug.WriteLine(text ?? string.Empty);
        }
    }
}
