using System;

namespace MortgageInvestmentSimulator
{
    public sealed partial class MortgageInterestRate
    {
        public MortgageInterestRate(int month, int year, decimal interestRate)
        {
            Month = month;
            Year = year;
            InterestRate = interestRate;
        }

        public int Month { get; }

        public int Year { get; }

        public decimal InterestRate { get; }

        public static MortgageInterestRate GetFifteenYearRate(MonthYear monthYear)
            => _fifteenYearRates.TryGetValue(monthYear, out var mortgageInterestRate) ? mortgageInterestRate : null;

        public static MortgageInterestRate GetRate(MonthYear monthYear, MortgageTerm term)
        {
            switch (term)
            {
                case MortgageTerm.FifteenYear:
                    return GetFifteenYearRate(monthYear);

                case MortgageTerm.ThirtyYear:
                    return GetThirtyYearRate(monthYear);

                default:
                    throw new ArgumentOutOfRangeException(nameof(term), term, null);
            }
        }

        public static MortgageInterestRate GetThirtyYearRate(MonthYear monthYear)
            => _thirtyYearRates.TryGetValue(monthYear, out var mortgageInterestRate) ? mortgageInterestRate : null;
    }
}
