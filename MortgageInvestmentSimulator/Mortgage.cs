namespace MortgageInvestmentSimulator
{
    /// <summary>
    /// Our mortgage.
    /// </summary>
    public sealed class Mortgage
    {
        public decimal Amount { get; set; }

        public decimal Balance { get; set; }

        public int Years { get; set; }

        public decimal InterestRate { get; set; }

        public decimal Payment { get; set; }

        /// <inheritdoc />
        public override string ToString() => base.ToString();
    }
}
