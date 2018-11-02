using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    public enum Outcome
    {
        /// <summary>
        /// The outcome is undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// The scenario succeeded.
        /// It ran to completion without going broke.
        /// </summary>
        Success,

        /// <summary>
        /// The scenario failed.
        /// At some point, we ran out of money.
        /// </summary>
        Failed,

        /// <summary>
        /// The scenario was invalid.
        /// Typically this means that we simply could not afford the house we
        /// wanted based on the income/assets we have.
        /// Often, high interest rates render some scenarios invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Programming error. Fix it!
        /// </summary>
        Error,
    }
}
