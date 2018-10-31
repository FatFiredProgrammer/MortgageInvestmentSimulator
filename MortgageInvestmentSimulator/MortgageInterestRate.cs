namespace MortgageInvestmentSimulator
{
    public sealed class MortgageInterestRate
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
    }
}
