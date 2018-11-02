using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Output to console/debug.
    ///     Our standard output method.
    /// </summary>
    [PublicAPI]
    public sealed class ConsoleOutput : IOutput
    {
        /// <inheritdoc />
        public void Flush()
        {
        }

        /// <inheritdoc />
        public void VerboseLine(string text)
        {
        }

        /// <inheritdoc />
        public void WriteLine(string text)
        {
            Console.WriteLine(text ?? string.Empty);
            Debug.WriteLine(text ?? string.Empty);
        }
    }
}
