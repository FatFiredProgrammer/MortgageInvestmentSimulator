using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    public static class DecimalExtensions
    {
        public static decimal GetAverage(this ICollection<decimal> values)
            => values.Count == 0 ? 0 : values.Average().ToDollarCents();

        public static decimal GetMedian(this IList<decimal> values)
        {
            if (values.Count <= 0)
                return 0;
            if (values.Count == 1)
                return values[0].ToDollarCents();

            if (values.Count % 2 == 1)
                return ((values[values.Count / 2] + values[values.Count / 2 + 1]) / 2).ToDollarCents();

            return values[values.Count / 2].ToDollarCents();
        }

        public static decimal ToDollarCents(this decimal value)
            => Math.Round(value, 2);

        public static decimal ToPercent(this decimal value)
            => Math.Round(value, 4);
    }
}
