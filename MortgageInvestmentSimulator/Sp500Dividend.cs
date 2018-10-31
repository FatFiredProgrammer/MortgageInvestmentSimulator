using System;

namespace MortgageInvestmentSimulator
{
    public sealed class Sp500Dividend
    {
        public Sp500Dividend(int month, int year, decimal interestRate)
        {
            Month = month;
            Year = year;
            InterestRate = interestRate;
        }

        public int Month { get; }

        public int Year { get; }

        public decimal InterestRate { get; }
    }
}
