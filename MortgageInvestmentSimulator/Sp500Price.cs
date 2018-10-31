namespace MortgageInvestmentSimulator
{
    public sealed class Sp500Price
    {
        public Sp500Price(int month, int year, decimal price)
        {
            Month = month;
            Year = year;
            Price = price;
        }

        public int Month { get; }

        public int Year { get; }

        public decimal Price { get; }
    }
}
