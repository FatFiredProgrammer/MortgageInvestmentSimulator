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

            public string Superlative { get; set; } = "better";

            public string FormattedSuperlative => string.IsNullOrWhiteSpace(Superlative) ? string.Empty : $" {Superlative}";

            public string Units { get; set; }

            public string FormattedUnits => string.IsNullOrWhiteSpace(Units) ? string.Empty : $" {Units}";

            public bool LargerIsBetter { get; set; } = true;
            public bool SmallerIsBetter
            {
                get => !LargerIsBetter;
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
                return specification.IgnoreIfEqual
                           ? string.Empty
                           : $"Both {specification.Text} {Format(investValue, specification.FormatSpecifier)}{specification.FormattedUnits}.{Environment.NewLine}";

            var investIsBetter = (specification.LargerIsBetter && investValue < avoidMortgageValue) || (!specification.LargerIsBetter && investValue < avoidMortgageValue);
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
            text.AppendLine($"{count:N0} {Scenario.SimulationYears}-year simulations each month from {Start} until {End}.");
            if (count <= 0)
                return text.ToString();

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
                text.AppendLine($"{Strategy.Invest.GetName()} average interest rate {investAverageInterestRate:P2}.");
                var investEffectiveAverageInterestRate = _invest.AverageEffectiveInterestRate;
                if (investEffectiveAverageInterestRate != 0)
                    text.AppendLine($"{Strategy.Invest.GetName()} average effective interest rate {investEffectiveAverageInterestRate:P2}.");
            }

            var avoidMortgageAverageInterestRate = _avoidMortgage.AverageInterestRate;
            if (avoidMortgageAverageInterestRate != 0)
            {
                text.AppendLine($"{Strategy.AvoidMortgage.GetName()} average interest rate {avoidMortgageAverageInterestRate:P2}.");

                var avoidMortgageEffectiveAverageInterestRate = _avoidMortgage.AverageEffectiveInterestRate;
                if (avoidMortgageEffectiveAverageInterestRate != 0)
                    text.AppendLine($"{Strategy.AvoidMortgage.GetName()} average effective interest rate {avoidMortgageEffectiveAverageInterestRate:P2}.");
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

            // TODO: Code needs work
#if false
years until paid off
    Months

 Summary
            //            text.AppendLine($"* {Strategy.Invest.GetName()}: {_invest.GetCounts()}");
            //            text.AppendLine($"* {Strategy.AvoidMortgage.GetName()}: {_avoidMortgage.GetCounts()}");

            *378 15 - year simulations monthly from April 1972 until September 2003
                * Investing: 378 simulations; 353 Success(93.39 %); 25 Invalid(6.61 %)
                * Avoiding Mortgage: 378 simulations; 353 Success(93.39 %); 25 Invalid(6.61 %)
                * Investing and Avoiding Mortgage both were not successful 25 times
                                                                              * Investing is better than Avoiding Mortgage 305 times(86.40 %)
                * Avoiding Mortgage is better than Investing 48 times(13.60 %)
                * Investing 353 successful simulations; $416,590 average net worth; $402,633 median net worth; 353 net gain simulations(100.00 %)
                * Avoiding Mortgage 353 successful simulations; $349,861 average net worth; $325,098 median net worth; 353 net gain simulations(100.00 %)
                * Average Net Worth: Investing is 19.07 % better than Avoiding Mortgage by $66,729 on average
                *Median Net Worth: Investing is 23.85 % better than Avoiding Mortgage by $77,535 on average
                *Investing worst result March 1994: $267,430
                * Investing best result April 1985: $577,913
                * Avoiding Mortgage worst result October 1987: $291,409
                * Avoiding Mortgage best result April 1985: $577,913
                * Financial Security: Avoiding Mortgage is 8.38 % better than Investing; Avoiding Mortgage 42.27 %; Investing 39.00 %
                *Investing was financially secure 24,778 of 63,540 months; 39.00 %
                *Avoiding Mortgage was financially secure 26,859 of 63,540 months; 42.27 %
                *Financial Security Months: Avoiding Mortgage is 8.40 % better than Investing by 2,081 of 63,540 months 
#endif

            // TODO: Code needs work
#if false
                 text.AppendLine(null);

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
                    $"* Financial Security Months: {FormatValueLowest(investFinanciallySecureMonths, avoidMortgageFinanciallySecureMonths)} of {(investTotalMonths + avoidMortgageTotalMonths) / 2:N0} months");
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
 
#endif
            text.AppendLine();
            text.AppendLine($"{Scenario.HomeValue:C0} home value.");
            if (Scenario.StartingCash > 0)
                text.AppendLine($"{Scenario.StartingCash:C0} starting cash.");
            if (Scenario.MonthlyIncome > 0)
                text.AppendLine($"{Scenario.MonthlyIncome:C0} monthly income with strategy of {Scenario.MonthlyIncomeStrategy}.");
            text.AppendLine($"{Scenario.MortgageTerm.GetYears()} year mortgage with {Scenario.OriginationFee:P0} origination fee.");
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
#if false

// TODO: Code needs work
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
                    $"* Financial Security Months: {FormatValueLowest(investFinanciallySecureMonths, avoidMortgageFinanciallySecureMonths)} of {(investTotalMonths + avoidMortgageTotalMonths) / 2:N0} months");
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
#endif
        }
    }
}
