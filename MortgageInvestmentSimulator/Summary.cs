using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Summary
    {
        public Summary(Scenario scenario, Results invest, Results avoidMortgage)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            _invest = invest ?? throw new ArgumentNullException(nameof(invest));
            _avoidMortgage = avoidMortgage ?? throw new ArgumentNullException(nameof(avoidMortgage));

            foreach (var result in invest.Items)
            {
                var item = new MonthYearSummary(result.Start);
                item.AddInvest(result);
                Add(item);
            }

            foreach (var result in avoidMortgage.Items)
            {
                if (!_summaries.TryGetValue(result.Start, out var item))
                {
                    item = new MonthYearSummary(result.Start);
                    Add(item);
                }

                item.AddAvoidMortgage(result);
            }
        }

        private Dictionary<MonthYear, MonthYearSummary> _summaries = new Dictionary<MonthYear, MonthYearSummary>();

        private Results _invest;

        private Results _avoidMortgage;

        public Scenario Scenario { get; set; }

        public MonthYear Start { get; set; }

        public MonthYear End { get; set; }

        public MonthYearSummary this[MonthYear monthYear]
        {
            get
            {
                if (monthYear == null)
                    throw new ArgumentNullException(nameof(monthYear));

                return _summaries.TryGetValue(monthYear, out var result) ? result : null;
            }
        }

        public IEnumerable<MonthYearSummary> Items => _summaries.Values;

        private void Add(MonthYearSummary result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            _summaries[result.Start] = result;
        }

        private decimal CalculatePercent(decimal numerator, decimal divisor)
        {
            if (divisor == 0)
                return 0m;

            return (numerator / divisor).ToPercent();
        }

        private string FormatDollarCents(decimal invest, decimal avoidMortgage)
        {
            if (invest == avoidMortgage)
                return $"Both {Strategy.Invest.GetName()} and {Strategy.AvoidMortgage.GetName()} are {invest:C0}";

            if (invest > avoidMortgage)
            {
                if (avoidMortgage > 0)
                {
                    var percent = ((invest - avoidMortgage) / avoidMortgage).ToPercent();
                    return
                        $"{Strategy.Invest.GetName()} is {percent:P2} better than {Strategy.AvoidMortgage.GetName()} by {invest - avoidMortgage:C0} on average";
                }

                return $"{Strategy.Invest.GetName()} {invest:C0}; {Strategy.AvoidMortgage.GetName()} {avoidMortgage:C0}";
            }

            if (invest > 0)
            {
                var percent = ((avoidMortgage - invest) / invest).ToPercent();
                return
                    $"{Strategy.AvoidMortgage.GetName()} is {percent:P2} better than {Strategy.Invest.GetName()} by {avoidMortgage - invest:C0} on average";
            }

            return $"{Strategy.AvoidMortgage.GetName()} {avoidMortgage:C0}; {Strategy.Invest.GetName()} {invest:C0}";
        }

        private string FormatPercent(decimal investPercent, decimal avoidMortgagePercent)
        {
            if (investPercent == avoidMortgagePercent)
                return $"Both {Strategy.Invest.GetName()} and {Strategy.AvoidMortgage.GetName()} are {investPercent:P2}";

            if (investPercent > avoidMortgagePercent)
            {
                if (avoidMortgagePercent > 0)
                {
                    var percent = ((investPercent - avoidMortgagePercent) / avoidMortgagePercent).ToPercent();
                    return
                        $"{Strategy.Invest.GetName()} is {percent:P2} better than {Strategy.AvoidMortgage.GetName()}; {Strategy.Invest.GetName()} {investPercent:P2}; {Strategy.AvoidMortgage.GetName()} {avoidMortgagePercent:P2}";
                }

                return $"{Strategy.Invest.GetName()} {investPercent:P2}; {Strategy.AvoidMortgage.GetName()} {avoidMortgagePercent:P2}";
            }

            if (investPercent > 0)
            {
                var percent = ((avoidMortgagePercent - investPercent) / investPercent).ToPercent();
                return
                    $"{Strategy.AvoidMortgage.GetName()} is {percent:P2} better than {Strategy.Invest.GetName()}; {Strategy.AvoidMortgage.GetName()} {avoidMortgagePercent:P2}; {Strategy.Invest.GetName()} {investPercent:P2}";
            }

            return $"{Strategy.AvoidMortgage.GetName()} {avoidMortgagePercent:P2}; {Strategy.Invest.GetName()} {investPercent:P2}";
        }

        private string FormatValue(decimal invest, decimal avoidMortgage)
        {
            if (invest == avoidMortgage)
                return $"Both {Strategy.Invest.GetName()} and {Strategy.AvoidMortgage.GetName()} are {invest:N0}";

            if (invest > avoidMortgage)
            {
                if (avoidMortgage > 0)
                {
                    var percent = ((invest - avoidMortgage) / avoidMortgage).ToPercent();
                    return
                        $"{Strategy.Invest.GetName()} is {percent:P2} better than {Strategy.AvoidMortgage.GetName()} by {invest - avoidMortgage:N0}";
                }

                return $"{Strategy.Invest.GetName()} {invest:N0}; {Strategy.AvoidMortgage.GetName()} {avoidMortgage:N0}";
            }

            if (invest > 0)
            {
                var percent = ((avoidMortgage - invest) / invest).ToPercent();
                return
                    $"{Strategy.AvoidMortgage.GetName()} is {percent:P2} better than {Strategy.Invest.GetName()} by {avoidMortgage - invest:N0}";
            }

            return $"{Strategy.AvoidMortgage.GetName()} {avoidMortgage:N0}; {Strategy.Invest.GetName()} {invest:N0}";
        }

        private string GetScenario()
        {
            var text = new StringBuilder();
            text.AppendLine(Scenario.Date != null
                                ? $"* {Scenario.SimulationYears} year (max) simulation in {Scenario.Date}"
                                : $"* {Scenario.SimulationYears} year (max) simulations starting {Scenario.Start} until {Scenario.End}");
            text.AppendLine($"* Home value is {Scenario.HomeValue:C0}");
            if (Scenario.StartingCash > 0)
                text.AppendLine($"* Starting cash is {Scenario.StartingCash:C0}");
            if (Scenario.MonthlyIncome > 0)
                text.AppendLine($"* Monthly income is {Scenario.MonthlyIncome:C0} with strategy of {Scenario.MonthlyIncomeStrategy}");
            text.AppendLine($"* {Scenario.MortgageTerm.GetYears()} year mortgage");
            if (Scenario.StockPercentage > 0)
                text.AppendLine($"* Invest {Scenario.StockPercentage:P0} in stocks");

            return text.ToString().TrimEnd();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine(null);
            text.AppendLine("# Summary");
            text.AppendLine(null);
            text.AppendLine($"* {_summaries.Count} {Scenario.SimulationYears}-year simulations monthly from {Start} until {End}");
            text.AppendLine($"* {Strategy.Invest.GetName()}: {_invest.GetCounts()}");
            text.AppendLine($"* {Strategy.AvoidMortgage.GetName()}: {_avoidMortgage.GetCounts()}");

            var nonFailed = Items.Count(c => c.NotFailed);
            var bothFailed = Items.Count(c => c.BothFailed);
            if (bothFailed > 0)
                text.AppendLine($"* {Strategy.Invest.GetName()} and {Strategy.AvoidMortgage.GetName()} both were not successful {bothFailed:N0} times");

            var investOnlyFailed = Items.Count(c => c.InvestOnlyFailed);
            if (investOnlyFailed > 0)
                text.AppendLine($"* {Strategy.Invest.GetName()} was not successful {investOnlyFailed:N0} times against {Strategy.AvoidMortgage.GetName()} failed");

            var avoidMortgageOnlyFailed = Items.Count(c => c.AvoidMortgageOnlyFailed);
            if (avoidMortgageOnlyFailed > 0)
                text.AppendLine($"* {Strategy.AvoidMortgage.GetName()} was not successful {avoidMortgageOnlyFailed:N0} times against {Strategy.Invest.GetName()} failed");

            var investIsBetter = Items.Count(c => c.InvestIsBetter);
            if (investIsBetter > 0)
                text.AppendLine($"* {Strategy.Invest.GetName()} is better than {Strategy.AvoidMortgage.GetName()} {investIsBetter:N0} times ({CalculatePercent(investIsBetter, nonFailed):P2})");

            var avoidMortgageIsBetter = Items.Count(c => c.AvoidMortgageIsBetter);
            if (avoidMortgageIsBetter > 0)
            {
                text.AppendLine(
                    $"* {Strategy.AvoidMortgage.GetName()} is better than {Strategy.Invest.GetName()} {avoidMortgageIsBetter:N0} times ({CalculatePercent(avoidMortgageIsBetter, nonFailed):P2})");
            }

            text.AppendLine($"* {Strategy.Invest.GetName()} {_invest.GetStatistics()}");
            text.AppendLine($"* {Strategy.AvoidMortgage.GetName()} {_avoidMortgage.GetStatistics()}");

            text.AppendLine($"* Average Net Worth: {FormatDollarCents(_invest.AverageNetWorth, _avoidMortgage.AverageNetWorth)}");
            text.AppendLine($"* Median Net Worth: {FormatDollarCents(_invest.MedianNetWorth, _avoidMortgage.MedianNetWorth)}");

            var investWorst = _invest.FindWorst();
            if (investWorst != null)
                text.AppendLine($"* {Strategy.Invest.GetName()} worst result {investWorst}");
            var investBest = _invest.FindBest();
            if (investBest != null)
                text.AppendLine($"* {Strategy.Invest.GetName()} best result {investBest}");
            var avoidMortgageWorst = _avoidMortgage.FindWorst();
            if (avoidMortgageWorst != null)
                text.AppendLine($"* {Strategy.AvoidMortgage.GetName()} worst result {avoidMortgageWorst}");
            var avoidMortgageBest = _avoidMortgage.FindBest();
            if (avoidMortgageBest != null)
                text.AppendLine($"* {Strategy.AvoidMortgage.GetName()} best result {investBest}");

            text.AppendLine($"* Financial Security: {FormatPercent(_invest.FinanciallySecurePercent, _avoidMortgage.FinanciallySecurePercent)}");
            text.AppendLine($"* {Strategy.Invest.GetName()} was financially secure {_invest.GetFinancialSecurity()}");
            text.AppendLine($"* {Strategy.AvoidMortgage.GetName()} was financially secure {_avoidMortgage.GetFinancialSecurity()}");
            var investFinanciallySecureMonths = _invest.FinanciallySecureMonths;
            var investTotalMonths = _invest.TotalMonths;
            var avoidMortgageFinanciallySecureMonths = _avoidMortgage.FinanciallySecureMonths;
            var avoidMortgageTotalMonths = _avoidMortgage.TotalMonths;
            if (investFinanciallySecureMonths != avoidMortgageFinanciallySecureMonths)
            {
                text.AppendLine(
                    $"* Financial Security Months: {FormatValue(investFinanciallySecureMonths, avoidMortgageFinanciallySecureMonths)} of {(investTotalMonths + avoidMortgageTotalMonths) / 2:N0} months");
            }

            if (Scenario.ShouldAdjustForInflation)
            {
                text.AppendLine(null);
                text.AppendLine("All values are _inflation adjusted_ during the simulation.");
                text.AppendLine("Results are always shown in today's dollars.");
            }

            text.AppendLine(null);
            text.AppendLine("_Financial security_ security implies you either own your house or have sufficient assets that you could pay off your house if you wanted.");
            text.AppendLine("Financial security is measured each month of the simulation.");
            text.AppendLine("Financial security implies that there is **no** risk that you might lose your house that month.");

            var invalid = Items.Count(c => c.InvestOutcome == Outcome.Invalid || c.AvoidMortgageOutcome == Outcome.Invalid);
            if (invalid > 0)
            {
                text.AppendLine(null);
                text.AppendLine("_Invalid_ results are obtained when the simulation is simply unable to purchase a house in any scenario.");
                text.AppendLine("For example, a $1,000,000 house @ 12% interest when monthly income is $1,000.");
                text.AppendLine("Neither investing nor avoiding a mortgage can resolve this.");
            }

            if (bothFailed > 0)
            {
                text.AppendLine(null);
                text.AppendLine("_Both_ scenarios failing can also result from negative inflation adjusting your monthly income such that you can no longer afford your mortgage.");
                text.AppendLine("This can be avoided by disabling inflation adjustment of monthly income.");
            }

            text.AppendLine(null);
            text.AppendLine("# Scenario");
            text.AppendLine(null);
            text.AppendLine(GetScenario());

            return text.ToString().TrimEnd();
        }
    }
}
