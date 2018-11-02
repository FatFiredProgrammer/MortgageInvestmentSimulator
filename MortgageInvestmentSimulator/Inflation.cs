using System;
using System.Collections.Generic;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     A class to adjust values for inflation.
    /// </summary>
    public static class Inflation
    {
        private static readonly Dictionary<MonthYearRange, decimal> _percents = new Dictionary<MonthYearRange, decimal>();

        public static decimal Adjust(decimal value, MonthYear start, MonthYear end)
        {
            var percent = GetPercent(start, end);
            return value * percent;
        }

        public static decimal GetPercent(MonthYear start, MonthYear end)
        {
            var range = new MonthYearRange(start, end);
            if (_percents.TryGetValue(range, out var percent))
                return percent;

            var inverted = start > end;
            if (inverted)
            {
                var temp = start;
                start = end;
                end = temp;
            }

            var now = new MonthYear(start);
            decimal value = 1;
            while (now < end)
            {
                var rate = InflationRates.GetRate(now);
                value = value + value * rate.Percent;
                now = now.AddMonths(1);
            }

            percent = 1 + (value - 1) / 1;
            if (inverted)
                percent = 1 / percent;

            _percents[range] = percent;

            return Math.Round(percent, 5);
        }
    }
}
