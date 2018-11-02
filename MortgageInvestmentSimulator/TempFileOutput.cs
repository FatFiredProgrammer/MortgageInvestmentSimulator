using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     Output to console/debug.
    ///     Our standard output method.
    /// </summary>
    [PublicAPI]
    public sealed class TempFileOutput : IOutput
    {
        private StringBuilder _text = new StringBuilder();

        /// <inheritdoc />
        public void Flush()
        {
            try
            {
                var fileName = Path.GetTempFileName() + ".txt";
                File.WriteAllText(fileName, _text.ToString());
                Debug.WriteLine($"Data written to {fileName}");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            _text = new StringBuilder();
        }

        /// <inheritdoc />
        public void VerboseLine(string text)
        {
            _text.AppendLine(text ?? string.Empty);
        }

        /// <inheritdoc />
        public void WriteLine(string text)
        {
            _text.AppendLine(text ?? string.Empty);
            Console.WriteLine(text ?? string.Empty);
            Debug.WriteLine(text ?? string.Empty);
        }
    }
}
