using System.Runtime.CompilerServices;

namespace MortgageInvestmentSimulator
{
    public enum MonthlyIncomeStrategy
    {
        /// <summary>
        /// Inflation adjusted fixed amount
        /// </summary>
        FixedInflationAdjusted,

        /// <summary>
        /// Inflation adjusted fixed amount
        /// </summary>
        FixedInflationAdjustedMonthly,

        /// <summary>
        /// Whatever amount the scenario says
        /// </summary>
        Fixed,

        /// <summary>
        /// Monthly mortgage amount.
        /// </summary>
        Mortgage,

        /// <summary>
        /// Monthly mortgage amount plus 25% extra to pay down or invest
        /// </summary>
        MortgagePlus25Percent,

        /// <summary>
        /// Monthly mortgage amount plus 50% extra to pay down or invest
        /// </summary>
        MortgagePlus50Percent,
    }
}
