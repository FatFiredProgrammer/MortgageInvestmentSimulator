namespace MortgageInvestmentSimulator
{
    public sealed class Sp500Dividend
    {
        public Sp500Dividend(int month, int year, decimal dividendPercentage)
        {
            Month = month;
            Year = year;
            DividendPercentage = dividendPercentage;
        }

        public int Month { get; }

        public int Year { get; }

        public decimal DividendPercentage { get; }
    }
}
