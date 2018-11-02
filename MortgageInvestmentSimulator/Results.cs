using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Results
    {
        /// <summary>
        ///     Gets or sets a value indicating whether we should avoid mortgage and pay mortgage down when possible.
        /// </summary>
        /// <value><c>true</c> if avoid mortgage; otherwise, <c>false</c>.</value>
        public Strategy Strategy { get; set; }

        private Dictionary<MonthYear, Result> _results = new Dictionary<MonthYear, Result>();

        public Result this[MonthYear monthYear]
        {
            get
            {
                if (monthYear == null)
                    throw new ArgumentNullException(nameof(monthYear));

                return _results.TryGetValue(monthYear, out var result) ? result : null;
            }
        }

        public IEnumerable<Result> Items => _results.Values;

        public void Add(Result result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            _results[result.Start] = result;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{_results.Count:N0} results";
    }
}
