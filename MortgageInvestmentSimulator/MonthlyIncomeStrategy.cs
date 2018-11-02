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
        Mortgage
    }
}
