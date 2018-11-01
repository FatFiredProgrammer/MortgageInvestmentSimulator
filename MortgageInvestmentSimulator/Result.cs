using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortgageInvestmentSimulator
{
    public sealed class Result
    {
        /// <summary>
        ///     Gets or sets a value indicating whether we should avoid mortgage and pay mortgage down when possible.
        /// </summary>
        /// <value><c>true</c> if avoid mortgage; otherwise, <c>false</c>.</value>
        public bool AvoidMortgage { get; set; }

        public MonthYear Start { get; set; }

        public MonthYear End { get; set; }

        public int Success { get; set; }

        public int Failed { get; set; }

        public Dictionary<MonthYear, decimal> NetWorths { get; set; } = new Dictionary<MonthYear, decimal>();

        public Dictionary<MonthYear,decimal> NetGains { get; set; } = new Dictionary<MonthYear, decimal>();

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
            get { return NetGains.Values.Where(c => c < 0).Sum(c => Math.Abs(c)); }
        }

        private static decimal GetAverage(ICollection<decimal> values)
            => values.Count == 0 ? 0 : values.Average();

        private static decimal GetMedian(IList<decimal> values)
        {
            if (values.Count <= 0)
                return 0;
            if (values.Count == 1)
                return values[0];

            if (values.Count % 2 == 1)
                return (values[values.Count / 2] + values[values.Count / 2 + 1]) / 2;

            return values[values.Count / 2];
        }

        private KeyValuePair<MonthYear, decimal>? FindWorst()
        {
            KeyValuePair<MonthYear, decimal>? worst = null;
            foreach (var item in NetGains)
            {
                if (!worst.HasValue)
                    worst = item;
                else if(worst.Value.Value > item.Value)
                    worst = item;
            }

            return worst;
        }
        private KeyValuePair<MonthYear, decimal>? FindBest()
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

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine($"Simulation from {Start} to {End}");
            text.AppendLine(AvoidMortgage ? "*Should avoid having a mortgage*" : "*Should invest money*");
            text.AppendLine($"{Success:N0} successful and {Failed:N0} failures");
            text.AppendLine($"Final Net worth {AverageNetWorth:C0} average; {MedianNetWorth:C0} median");
            if (NetGains.Count > 0)
            {
                text.AppendLine($"Net worth *gain* {AverageNetGain:C0} average; {MedianNetGain:C0} median; {NetGains.Values.Min():C0} minimum; {NetGains.Values.Max():C0} maximum");
                var lossCount = NetLossCount;
                if (lossCount > 0)
                    text.AppendLine($"{lossCount:N0} net worth losses; {NetLossTotal / NetLossCount:C0} average loss; {Math.Abs(NetGains.Values.Min()):C0} worst loss");
            }

            var worst = FindWorst();
            if (worst.HasValue)
                text.AppendLine($"Worst loss of {worst.Value.Value:C0} in {worst.Value.Key}");

            var best = FindBest();
            if (best.HasValue)
                text.AppendLine($"Best gain of {best.Value.Value:C0} in {best.Value.Key}");

            if (Errors.Count > 0)
            {
                foreach (var error in Errors)
                {
                    text.AppendLine(error.TrimEnd());
                }
            }

            return text.ToString().TrimEnd();
        }
    }
}
