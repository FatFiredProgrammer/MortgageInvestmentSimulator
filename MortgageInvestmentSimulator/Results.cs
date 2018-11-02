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
    public sealed class Results
    {
        private Dictionary<MonthYear, Result> _results = new Dictionary<MonthYear, Result>();

        /// <summary>
        ///     Gets or sets a value indicating whether we should avoid mortgage and pay mortgage down when possible.
        /// </summary>
        /// <value><c>true</c> if avoid mortgage; otherwise, <c>false</c>.</value>
        public Strategy Strategy { get; set; }

        public Result this[MonthYear monthYear]
        {
            get
            {
                if (monthYear == null)
                    throw new ArgumentNullException(nameof(monthYear));

                return _results.TryGetValue(monthYear, out var result) ? result : null;
            }
        }

        public IEnumerable<Result> Items => _results.Values;

        public int TotalMonths
        {
            get { return Items.Where(c => c.Outcome == Outcome.Success || c.Outcome == Outcome.Failed).Sum(c => c.TotalMonths); }
        }

        public int FinanciallySecureMonths
        {
            get { return Items.Where(c => c.Outcome == Outcome.Success || c.Outcome == Outcome.Failed).Sum(c => c.FinanciallySecureMonths); }
        }

        public decimal FinanciallySecurePercent
        {
            get
            {
                var total = TotalMonths;
                var secure = FinanciallySecureMonths;
                return total == 0 ? 0m : ((decimal)secure / total).ToPercent();
            }
        }

        public int Count => _results.Count;

        public decimal AverageNetWorth
        {
            get
            {
                var list = Items.Where(c => c.Outcome == Outcome.Success).ToList();
                if (list.Count == 0)
                    return 0;

                var netWorths = list.Select(c => c.NetWorth).ToList();
                var average = netWorths.GetAverage();
                return average;
            }
        }

        public decimal MedianNetWorth
        {
            get
            {
                var list = Items.Where(c => c.Outcome == Outcome.Success).ToList();
                if (list.Count == 0)
                    return 0;

                var netWorths = list.Select(c => c.NetWorth).ToList();
                var median = netWorths.GetMedian();
                return median;
            }
        }

        public int NetLossCount
        {
            get
            {
                var list = Items.Where(c => c.Outcome == Outcome.Success).ToList();
                if (list.Count == 0)
                    return 0;

                var count = list.Count(c => c.NetWorth < 0);
                return count;
            }
        }

        public int NetGainCount
        {
            get
            {
                var list = Items.Where(c => c.Outcome == Outcome.Success).ToList();
                if (list.Count == 0)
                    return 0;

                var count = list.Count(c => c.NetWorth > 0);
                return count;
            }
        }

        public void Add(Result result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            _results[result.Start] = result;
        }

        public Result FindBest()
        {
            Result best = null;
            foreach (var item in Items)
            {
                if (item.Outcome != Outcome.Success)
                    continue;

                if (best == null)
                    best = item;
                else if (best.NetWorth < item.NetWorth)
                    best = item;
            }

            return best;
        }

        public Result FindWorst()
        {
            Result worst = null;
            foreach (var item in Items)
            {
                if (item.Outcome != Outcome.Success)
                    continue;

                if (worst == null)
                    worst = item;
                else if (worst.NetWorth > item.NetWorth)
                    worst = item;
            }

            return worst;
        }

        public string GetCounts()
        {
            if (Count == 0)
                return "; None";

            var list = Items.ToList();
            var text = new StringBuilder();
            text.Append($"{list.Count} simulations");

            var success = list.Count(c => c.Outcome == Outcome.Success);
            if (success > 0)
                text.Append($"; {success} {nameof(Outcome.Success)} ({(decimal)success / list.Count:P2})");
            var failed = list.Count(c => c.Outcome == Outcome.Failed);
            if (failed > 0)
                text.Append($"; {failed} {nameof(Outcome.Failed)} ({(decimal)failed / list.Count:P2})");
            var invalid = list.Count(c => c.Outcome == Outcome.Invalid);
            if (invalid > 0)
                text.Append($"; {invalid} {nameof(Outcome.Invalid)} ({(decimal)invalid / list.Count:P2})");
            var error = list.Count(c => c.Outcome == Outcome.Error);
            if (error > 0)
                text.Append($"; {error} {nameof(Outcome.Error)} ({(decimal)error / list.Count:P2})");
            var undefined = list.Count(c => c.Outcome == Outcome.Undefined);
            if (undefined > 0)
                text.Append($"; {undefined} {nameof(Outcome.Undefined)} ({(decimal)undefined / list.Count:P2})");
            return text.ToString();
        }

        public string GetFinancialSecurity()
            => TotalMonths == 0 ? "No Data" : $"{FinanciallySecureMonths:N0} of {TotalMonths:N0} months; {FinanciallySecurePercent:P2}";

        public string GetStatistics()
        {
            var list = Items.Where(c => c.Outcome == Outcome.Success).ToList();
            if (list.Count == 0)
                return "; None";

            var text = new StringBuilder();
            text.Append($"{list.Count} successful simulations");
            text.Append($"; {AverageNetWorth:C0} average net worth");
            text.Append($"; {MedianNetWorth:C0} median net worth");
            var netLossCount = NetLossCount;
            if(netLossCount > 0)
                text.Append($"; {NetLossCount:N0} net loss simulations ({((decimal)netLossCount / list.Count).ToPercent():P2})");
            var netGainCount = NetGainCount;
            if (netGainCount > 0)
                text.Append($"; {NetGainCount:N0} net gain simulations ({((decimal)netGainCount / list.Count).ToPercent():P2})");

            return text.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{Strategy.GetName()}: {GetCounts()}";
    }
}
