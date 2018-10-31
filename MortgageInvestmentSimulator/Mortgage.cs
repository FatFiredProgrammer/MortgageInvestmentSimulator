namespace MortgageInvestmentSimulator
{
    /// <summary>
    /// Our mortgage.
    /// </summary>
    public sealed class Mortgage
    {
        /// <summary>
        /// Gets or sets the amount of the mortgage.
        /// </summary>
        /// <value>The amount.</value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the balance of the mortgage.
        /// </summary>
        /// <value>The balance.</value>
        public decimal Balance { get; set; }

        /// <summary>
        /// Gets or sets the years left on the mortgage..
        /// </summary>
        /// <value>The years.</value>
        public int Years { get; set; }

        public decimal InterestRate { get; set; }

        public decimal Payment { get; set; }

        /// <inheritdoc />
        public override string ToString() => base.ToString();
    }
}

// TODO: 
