using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Output to verbose console/debug.
    ///     Our standard output method.
    /// </summary>
    [PublicAPI]
    public sealed class VerboseOutput : IOutput
    {
        /// <inheritdoc />
        public void VerboseLine(string text)
        {
            Console.WriteLine(text ?? string.Empty);
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
