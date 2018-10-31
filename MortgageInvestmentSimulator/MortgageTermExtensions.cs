using System;

namespace MortgageInvestmentSimulator
{
    public static class MortgageTermExtensions
    {
        public static int GetYears(this MortgageTerm term)
        {
            switch (term)
            {
                case MortgageTerm.FifteenYear:
                    return 15;

                case MortgageTerm.ThirtyYear:
                    return 30;

                default:
                    throw new ArgumentOutOfRangeException(nameof(term), term, null);
            }
        }
    }
}
