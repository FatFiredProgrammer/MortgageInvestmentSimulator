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

        public List<decimal> NetWorths { get; set; } = new List<decimal>();

        public List<decimal> NetGains { get; set; } = new List<decimal>();

        public List<string> Errors { get; set; } = new List<string>();

        public decimal AverageNetGain => GetAverage(NetGains);

        public decimal MedianNetGain => GetMedian(NetGains);

        public decimal AverageNetWorth => GetAverage(NetWorths);

        public decimal MedianNetWorth => GetMedian(NetWorths);

        private static decimal GetAverage(ICollection<decimal> netWorths)
            => netWorths.Count == 0 ? 0 : netWorths.Average();

        private static decimal GetMedian(IList<decimal> netWorths)
        {
            if (netWorths.Count <= 0)
                return 0;
            if (netWorths.Count == 1)
                return netWorths[0];

            if (netWorths.Count % 2 == 1)
                return (netWorths[netWorths.Count / 2] + netWorths[netWorths.Count / 2 + 1]) / 2;

            return netWorths[netWorths.Count / 2];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine($"Simulation from {Start} to {End}");
            text.AppendLine(AvoidMortgage ? "*Should avoid having a mortgage*" : "*Should invest money*");
            text.AppendLine($"{Success:N0} successful and {Failed:N0} failures");
            text.AppendLine($"Net worth average {AverageNetWorth:C0}; median {MedianNetWorth:C0}");
            text.AppendLine($"Net worth gain average {AverageNetGain:C0}; median {MedianNetGain:C0}");
            return text.ToString().TrimEnd();
        }
    }
}
