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

        public int Months { get; set; }

        public decimal AverageMortgageInterestRate { get; set; }

        public decimal AverageEffectiveMortgageInterestRate { get; set; }

        public bool FinanciallySecure => FinanciallySecureMonthYear != null;

        public bool Success => Outcome == Outcome.Success;

        public bool Valid => Success || Failed;

        public bool Failed => Outcome == Outcome.Failed;

        public bool Invalid => Outcome == Outcome.Invalid;

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

        public MonthYear FinanciallySecureMonthYear { get; set; }

        public decimal YearsUntilFinanciallySecure => FinanciallySecure ? MonthYear.MonthDifference(FinanciallySecureMonthYear, Start) / 12 : 0;

        public string Status { get; set; }

        /// <inheritdoc />
        public override string ToString()
            => Success ? $"{Start}: {NetWorth:C0}" : $"{Start}: {Outcome}: {Error}";
    }
}
