using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class MonthYearSummary
    {
        public MonthYearSummary(MonthYear start)
        {
            Start = start;
        }

        public MonthYear Start { get; }

        public Outcome InvestOutcome { get; private set; }
        public decimal InvestNetWorth { get; private set; }
        public Outcome AvoidMortgageOutcome { get; private set; }
        public decimal AvoidMortgageNetWorth { get; private set; }

        public void AddInvest(Result result)
        {

        }
        /// <inheritdoc />
        public override string ToString() => base.ToString();
    }
}
// TODO: 
