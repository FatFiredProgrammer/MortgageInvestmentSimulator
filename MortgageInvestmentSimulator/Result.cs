using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Result
    {
        public Result(MonthYear start, Outcome outcome)
        {
            Start = start;
            Outcome = outcome;
        }

        public MonthYear Start { get; }

        public Outcome Outcome { get; }

        public decimal NetWorth { get; set; }

        public string Error { get; set; }

        public MonthYear WhenError { get; set; }

        /// <summary>
        ///     Gets or sets the total months the simulation ran.
        /// </summary>
        /// <value>The total months.</value>
        public int TotalMonths { get; set; }

        /// <summary>
        ///     Gets or sets the number financially secure months.
        ///     These are months where we either do or we could own our house free and clear.
        /// </summary>
        /// <value>The financially secure months.</value>
        public int FinanciallySecureMonths { get; set; }

        public string Status { get; set; }

        /// <inheritdoc />
        public override string ToString()
            => Outcome == Outcome.Success ? $"{Start}: {NetWorth:C0}" : $"{Start}: {Outcome}: {Error}";
    }
}
