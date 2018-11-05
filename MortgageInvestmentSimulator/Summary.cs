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
        private sealed class Specification
        {
            public string Text { get; set; }

            public string Superlative { private get; set; } = "better";

            public string FormattedSuperlative => string.IsNullOrWhiteSpace(Superlative) ? string.Empty : $" {Superlative}";

            public string Units { private get; set; }

            public string FormattedUnits => string.IsNullOrWhiteSpace(Units) ? string.Empty : $" {Units}";

            public bool LargerIsBetter { get; set; } = true;

            public bool SmallerIsBetter
            {
                set => LargerIsBetter = false;
            }

            public string FormatSpecifier { get; set; } = "P2";

            public bool IgnoreIfEqual { get; set; }
        }

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

        public int Count => _summaries.Count;

        public int BothInvalidCount => Items.Count(c => c.BothInvalid);

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

        private static string Format(decimal value, string specifier)
            => string.Format(string.Format("{{0:{0}}}", specifier), value);

        private string Show(decimal investValue, decimal avoidMortgageValue, Specification specification)
        {
            specification = specification ?? new Specification();

            // Both are the same
            if (investValue == avoidMortgageValue)
            {
                return specification.IgnoreIfEqual
                           ? string.Empty
                           : $"Both {specification.Text} {Format(investValue, specification.FormatSpecifier)}{specification.FormattedUnits}.{Environment.NewLine}";
            }

            var investIsBetter = (specification.LargerIsBetter && investValue > avoidMortgageValue) || (!specification.LargerIsBetter && investValue < avoidMortgageValue);
            var betterName = (investIsBetter ? Strategy.Invest : Strategy.AvoidMortgage).GetName();
            var betterValue = investIsBetter ? investValue : avoidMortgageValue;
            var worseValue = investIsBetter ? avoidMortgageValue : investValue;
            var better = Format(betterValue, specification.FormatSpecifier);
            var worse = Format(worseValue, specification.FormatSpecifier);

            // One or the other is zero so we can't do a "percent better"
            if (investValue == 0 || avoidMortgageValue == 0)
                return $"{betterName} {specification.Text} {better} vs {worse}{specification.FormattedUnits}.{Environment.NewLine}";

            var percent = Math.Abs((betterValue - worseValue) / worseValue).ToPercent();
            return $"{betterName} {specification.Text} {percent:P2}{specification.FormattedSuperlative}. {better} vs {worse}{specification.FormattedUnits}.{Environment.NewLine}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine();
            text.AppendLine("# Elevator Speech");
            text.AppendLine();

            var bothInvalidCount = BothInvalidCount;
            var count = Count - bothInvalidCount;
            if (count <= 0)
                return text.ToString();

            var investIsBetter = CalculatePercent(Items.Count(c => c.InvestIsBetter), count);
            var avoidMortgageIsBetter = CalculatePercent(Items.Count(c => c.AvoidMortgageIsBetter), count);
            text.AppendLine(investIsBetter > avoidMortgageIsBetter
                                ? $"{Strategy.Invest.GetName()} was better {investIsBetter:P02} of the time."
                                : $"{Strategy.AvoidMortgage.GetName()} was better {avoidMortgageIsBetter:P02} of the time.");

            text.Append(Show(
                _invest.AverageNetWorth,
                _avoidMortgage.AverageNetWorth,
                new Specification
                {
                    Text = "generated",
                    Superlative = "more average net worth",
                    Units = "",
                    LargerIsBetter = true,
                    FormatSpecifier = "C0",
                    IgnoreIfEqual = true,
                }));
            text.Append(Show(
                _invest.MedianNetWorth,
                _avoidMortgage.MedianNetWorth,
                new Specification
                {
                    Text = "generated",
                    Superlative = "more median net worth",
                    Units = "",
                    LargerIsBetter = true,
                    FormatSpecifier = "C0",
                    IgnoreIfEqual = true,
                }));

            text.AppendLine($"{count:N0} {Scenario.SimulationYears}-year simulations each month from {Start} until {End}.");
            var investSuccessPercent = CalculatePercent(Items.Count(c => c.InvestSuccess), count);
            var avoidMortgageSuccessPercent = CalculatePercent(Items.Count(c => c.AvoidMortgageSuccess), count);
            text.Append(Show(
                investSuccessPercent,
                avoidMortgageSuccessPercent,
                new Specification
                {
                    Text = "succeeded",
                    Superlative = "more often",
                    Units = "of the time",
                    LargerIsBetter = true,
                    FormatSpecifier = "P2",
                    IgnoreIfEqual = false,
                }));
            var bothFailed = Items.Count(c => c.BothInvalid) - bothInvalidCount;
            if (bothFailed > 0)
                text.AppendLine($"Both scenarios failed {CalculatePercent(bothFailed, Count):P2} of the time.");
            if (bothInvalidCount > 0)
                text.AppendLine($"Both scenarios were invalid {CalculatePercent(bothInvalidCount, Count):P2} of the time.");

            var investNotSuccessPercent = CalculatePercent(Items.Count(c => c.InvestNotSuccess), count);
            var avoidMortgageNotSuccessPercent = CalculatePercent(Items.Count(c => c.AvoidMortgageNotSuccess), count);
            text.Append(Show(
                investNotSuccessPercent,
                avoidMortgageNotSuccessPercent,
                new Specification
                {
                    Text = "failed",
                    Superlative = "more often",
                    Units = "of the time",
                    LargerIsBetter = true,
                    FormatSpecifier = "P2",
                    IgnoreIfEqual = true,
                }));

            var investAverageInterestRate = _invest.AverageInterestRate;
            if (investAverageInterestRate != 0)
            {
                text.Append($"{Strategy.Invest.GetName()} average interest rate {investAverageInterestRate:P2}");
                var investEffectiveAverageInterestRate = _invest.AverageEffectiveInterestRate;
                if (investEffectiveAverageInterestRate != 0)
                    text.Append($" ({investEffectiveAverageInterestRate:P2} effective rate)");
                text.AppendLine(".");
            }

            var avoidMortgageAverageInterestRate = _avoidMortgage.AverageInterestRate;
            if (avoidMortgageAverageInterestRate != 0)
            {
                text.Append($"{Strategy.AvoidMortgage.GetName()} average interest rate {avoidMortgageAverageInterestRate:P2}.");

                var avoidMortgageEffectiveAverageInterestRate = _avoidMortgage.AverageEffectiveInterestRate;
                if (avoidMortgageEffectiveAverageInterestRate != 0)
                    text.Append($" ({avoidMortgageEffectiveAverageInterestRate:P2} effective rate)");
                text.AppendLine(".");
            }

            var investAverageYearsUntilFinanciallySecure = _invest.AverageYearsUntilFinanciallySecure;
            var avoidMortgageAverageYearsUntilFinanciallySecure = _avoidMortgage.AverageYearsUntilFinanciallySecure;
            text.Append(Show(
                investAverageYearsUntilFinanciallySecure,
                avoidMortgageAverageYearsUntilFinanciallySecure,
                new Specification
                {
                    Text = "could/did own house",
                    Superlative = "faster",
                    Units = "years",
                    SmallerIsBetter = true,
                    FormatSpecifier = "N1",
                    IgnoreIfEqual = false,
                }));
            var investFailedToOwnHouse = _invest.Items.Count(c => !c.FinanciallySecure);
            var avoidMortgageFailedToOwnHouse = _invest.Items.Count(c => !c.FinanciallySecure);
            text.Append(Show(
                investFailedToOwnHouse,
                avoidMortgageFailedToOwnHouse,
                new Specification
                {
                    Text = "failed to own house",
                    Superlative = "more often",
                    Units = "times",
                    SmallerIsBetter = true,
                    FormatSpecifier = "N0",
                    IgnoreIfEqual = false,
                }));

            text.AppendLine();
            text.AppendLine($"{Scenario.HomeValue:C0} home value.");
            if (Scenario.StartingCash > 0)
                text.AppendLine($"{Scenario.StartingCash:C0} starting cash.");
            if (Scenario.MonthlyIncome > 0)
                text.AppendLine($"{Scenario.MonthlyIncome:C0} monthly income with strategy of {Scenario.MonthlyIncomeStrategy}.");
            text.AppendLine($"{Scenario.MortgageTerm.GetYears()} year mortgage with {Scenario.OriginationFee:P2} origination fee.");
            if (Scenario.ExtraPayment > 0)
                text.AppendLine($"{Scenario.ExtraPayment:C0} payment per month.");
            if (Scenario.StockPercentage > 0)
                text.AppendLine($"Invest {Scenario.StockPercentage:P0} in stocks.");
            if (!Scenario.ShouldAdjustForInflation)
                text.AppendLine("Not adjusted for inflation.");
            if (!Scenario.AllowRefinance)
                text.AppendLine("Do not refinance.");
            else if (!Scenario.CashOutAtRefinance)
                text.AppendLine("Cash out at refinance.");
            if (!Scenario.AllowMortgageInterestDeduction)
                text.AppendLine("No mortgage interest deduction.");
            if (!Scenario.ShouldAdjustForInflation)
                text.AppendLine("Not adjusted for inflation.");

            return text.ToString().TrimEnd();
        }
    }
}
