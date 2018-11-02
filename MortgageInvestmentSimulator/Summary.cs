using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Summary
    {
        
        public Summary(Results investing, Results avoidMortgage)
        {
        }
        public MonthYear Start { get; set; }

        public MonthYear End { get; set; }
        private Dictionary<MonthYear, MonthYearSummary> _results = new Dictionary<MonthYear, MonthYearSummary>();

        public MonthYearSummary this[MonthYear monthYear]
        {
            get
            {
                if (monthYear == null)
                    throw new ArgumentNullException(nameof(monthYear));

                return _results.TryGetValue(monthYear, out var result) ? result : null;
            }
        }

        public IEnumerable<MonthYearSummary> Items => _results.Values;

        public void Add(MonthYearSummary result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            _results[result.Start] = result;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{_results.Count:N0} results";
    }
}
#if false

// TODO: Code needs work
                output.WriteLine("# Simulation");
                output.WriteLine(null);
                output.WriteLine(scenario.GetSummary().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));
                output.WriteLine(null);
                output.WriteLine("# Summary");
                output.WriteLine(null);
                var failurePercentage = (decimal)resultInvesting.Failed / (resultInvesting.Failed + resultInvesting.Success);
                output.WriteLine(
                    $"* Investing had an {resultInvesting.AverageNetGain - resultAvoidMortgage.AverageNetGain:C0} average improvement in net worth over avoiding a mortgage. ");
                if(resultAvoidMortgage.AverageNetGain > 0)
                    output.WriteLine(
                        $"* Investing had an {(resultInvesting.AverageNetGain - resultAvoidMortgage.AverageNetGain) / (resultAvoidMortgage.AverageNetGain):P2} average improvement in net worth over avoiding a mortgage. ");
                output.WriteLine(
                    $"* Investing failed {failurePercentage:P2} of the time.");
                if (resultInvesting.NetLossCount > 0)
                    output.WriteLine($"* Investing had {resultInvesting.NetLossCount:N0} of {resultInvesting.Total:N0} simulations ({(decimal)resultInvesting.NetLossCount / resultInvesting.Total:P2}) resulting in a loss in net worth.");
                var worst = resultInvesting.FindWorst();
                if (worst.HasValue)
                    output.WriteLine($"* Investing had a worst loss of {worst.Value.Value.ToDollarCents():C0} net worth in simulation starting {worst.Value.Key}.");
                var best = resultInvesting.FindBest();
                if (best.HasValue)
                    output.WriteLine($"* Investing had best gain of {best.Value.Value.ToDollarCents():C0} net worth in simulation starting {best.Value.Key}.");
                if (resultAvoidMortgage.NetLossCount > 0)
                    output.WriteLine($"* Avoiding mortgage had {resultAvoidMortgage.NetLossCount:N0} of {resultAvoidMortgage.Total:N0} simulations ({(decimal)resultAvoidMortgage.NetLossCount / resultAvoidMortgage.Total:P2}) resulting in a loss in net worth.");
                worst = resultAvoidMortgage.FindWorst();
                if (worst.HasValue)
                    output.WriteLine($"* Avoiding mortgage had a worst gain/loss of {worst.Value.Value.ToDollarCents():C0} net worth in simulation starting {worst.Value.Key}.");
                best = resultAvoidMortgage.FindBest();
                if (best.HasValue)
                    output.WriteLine($"* Avoiding mortgage had best gain/loss of {best.Value.Value.ToDollarCents():C0} net worth in simulation starting {best.Value.Key}.");
                output.WriteLine(null);
                output.WriteLine("# Investing");
                output.WriteLine(null);
                output.WriteLine(resultInvesting.ToString().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));
                output.WriteLine(null);
                output.WriteLine("# Avoiding Mortgage");
                output.WriteLine(null);
                output.WriteLine(resultAvoidMortgage.ToString().Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine));

#endif
// TODO: 
#if false

// TODO: Code needs work

        public int Total => Success + Failed;

        public int Success { get; set; }

        public int Failed { get; set; }

        public int Invalid { get; set; }

        public Dictionary<MonthYear, decimal> NetWorths { get; set; } = new Dictionary<MonthYear, decimal>();

        public Dictionary<MonthYear, decimal> NetGains { get; set; } = new Dictionary<MonthYear, decimal>();

        public List<string> Errors { get; set; } = new List<string>();

        public decimal AverageNetGain => GetAverage(NetGains.Values);

        public decimal MedianNetGain => GetMedian(NetGains.Values.ToList());

        public decimal AverageNetWorth => GetAverage(NetWorths.Values);

        public decimal MedianNetWorth => GetMedian(NetWorths.Values.ToList());

        public int NetLossCount
        {
            get { return NetGains.Values.Count(c => c < 0); }
        }

        public decimal NetLossTotal
        {
            get { return NetGains.Values.Where(c => c < 0).Sum(c => Math.Abs(c)).ToDollarCents(); }
        }

        public KeyValuePair<MonthYear, decimal>? FindBest()
        {
            KeyValuePair<MonthYear, decimal>? best = null;
            foreach (var item in NetGains)
            {
                if (!best.HasValue)
                    best = item;
                else if (best.Value.Value < item.Value)
                    best = item;
            }

            return best;
        }

        public KeyValuePair<MonthYear, decimal>? FindWorst()
        {
            KeyValuePair<MonthYear, decimal>? worst = null;
            foreach (var item in NetGains)
            {
                if (!worst.HasValue)
                    worst = item;
                else if (worst.Value.Value > item.Value)
                    worst = item;
            }

            return worst;
        }

        private static decimal GetAverage(ICollection<decimal> values)
            => values.Count == 0 ? 0 : values.Average().ToDollarCents();

        private static decimal GetMedian(IList<decimal> values)
        {
            if (values.Count <= 0)
                return 0;
            if (values.Count == 1)
                return values[0].ToDollarCents();

            if (values.Count % 2 == 1)
                return ((values[values.Count / 2] + values[values.Count / 2 + 1]) / 2).ToDollarCents();

            return values[values.Count / 2].ToDollarCents();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();

            // TODO: Code needs work
#if false
                 text.AppendLine($"Simulation from {Start} to {End}");
            text.AppendLine($"{Strategy}");
            text.AppendLine($"{Success:N0} successful and {Failed:N0} failures of {Total:N0} ({(decimal)Success / Total:P2})");
            if (Total > 0)
            {
                text.AppendLine($"Final net worth {AverageNetWorth:C0} average; {MedianNetWorth:C0} median");
                if (NetGains.Count > 0)
                {
                    text.AppendLine($"Net worth *gain* {AverageNetGain:C0} average; {MedianNetGain:C0} median; {NetGains.Values.Min().ToDollarCents():C0} minimum; {NetGains.Values.Max().ToDollarCents():C0} maximum");
                    var lossCount = NetLossCount;
                    if (lossCount > 0)
                    {
                        text.AppendLine(
                            $"{lossCount:N0} net worth losses in {Total:N0} simulations ({(decimal)NetLossCount / Total:P2}); {(NetLossTotal / NetLossCount).ToDollarCents():C0} average loss; {Math.Abs(NetGains.Values.Min().ToDollarCents()):C0} worst loss");
                    }
                }

                var worst = FindWorst();
                if (worst.HasValue)
                    text.AppendLine($"Worst loss of {worst.Value.Value.ToDollarCents():C0} net worth in simulation starting {worst.Value.Key}");

                var best = FindBest();
                if (best.HasValue)
                    text.AppendLine($"Best gain of {best.Value.Value.ToDollarCents():C0} net worth in simulation starting {best.Value.Key}");
            }

            if (Invalid > 0)
                text.AppendLine($"{Invalid} scenarios were invalid - typically this means the monthly income could not cover the required loan");

            if (Errors.Count > 0)
            {
                foreach (var error in Errors)
                {
                    text.AppendLine(error.TrimEnd());
                }
            }
 
#endif

            return text.ToString().TrimEnd();
        }
    
#endif