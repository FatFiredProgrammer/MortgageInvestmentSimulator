using System;

namespace MortgageInvestmentSimulator
{
    public static class DecimalExtensions
    {
        public static decimal ToDollarCents(this decimal value)
            => Math.Round(value, 2);

        public static decimal ToPercent(this decimal value)
            => Math.Round(value, 4);
    }
}
