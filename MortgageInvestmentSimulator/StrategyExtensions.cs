using System;

namespace MortgageInvestmentSimulator
{
    public static class StrategyExtensions
    {
        public static string GetName(this Strategy strategy)
        {
            switch (strategy)
            {
                case Strategy.AvoidMortgage:
                    return "Avoiding-Mortgage";

                case Strategy.Invest:
                    return "Investing";

                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }
        }
    }
}
