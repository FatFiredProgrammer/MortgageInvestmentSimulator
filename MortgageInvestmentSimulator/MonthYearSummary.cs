using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class MonthYearSummary
    {
        public MonthYearSummary(MonthYear start)
            => Start = start;

        public MonthYear Start { get; }

        public Outcome InvestOutcome { get; private set; }

        public decimal InvestNetWorth { get; private set; }

        public Outcome AvoidMortgageOutcome { get; private set; }

        public decimal AvoidMortgageNetWorth { get; private set; }

        public bool NotFailed => InvestOutcome == Outcome.Success || AvoidMortgageOutcome == Outcome.Success;

        public bool BothFailed => InvestOutcome != Outcome.Success && AvoidMortgageOutcome != Outcome.Success;

        public bool InvestOnlyFailed => InvestOutcome != Outcome.Success && AvoidMortgageOutcome == Outcome.Success;

        public bool AvoidMortgageOnlyFailed => InvestOutcome == Outcome.Success && AvoidMortgageOutcome != Outcome.Success;

        public bool InvestIsBetter
        {
            get
            {
                if (BothFailed)
                    return false;
                if (AvoidMortgageOnlyFailed)
                    return true;

                return InvestNetWorth > AvoidMortgageNetWorth;
            }
        }

        public bool AvoidMortgageIsBetter
        {
            get
            {
                if (BothFailed)
                    return false;
                if (InvestOnlyFailed)
                    return true;

                return AvoidMortgageNetWorth > InvestNetWorth;
            }
        }

        public void AddAvoidMortgage(Result result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            AvoidMortgageOutcome = result.Outcome;
            AvoidMortgageNetWorth = result.NetWorth;
        }

        public void AddInvest(Result result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            InvestOutcome = result.Outcome;
            InvestNetWorth = result.NetWorth;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append($"{Start};");
            text.Append($" Invest: {InvestOutcome}");
            text.Append(InvestOutcome == Outcome.Success ? $"={InvestNetWorth:C0};" : ";");
            text.Append($" Avoid Mortgage: {AvoidMortgageOutcome}");
            text.Append(AvoidMortgageOutcome == Outcome.Success ? $"={AvoidMortgageNetWorth:C0};" : ";");
            return text.ToString();
        }
    }
}
