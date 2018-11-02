using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     A class representing a month and year.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class MonthYearRange : IEquatable<MonthYearRange>
    {
        public MonthYearRange(int startMonth, int startYear, int endMonth, int endYear)
        {
            if (startMonth < 1 || startMonth > 12)
                throw new ArgumentException("startMonth < 1 || startMonth > 12", nameof(startMonth));
            if (startYear < 1900 || startYear > 2100)
                throw new ArgumentException("startYear < 1900 || startYear > 2100", nameof(startYear));
            if (endMonth < 1 || endMonth > 12)
                throw new ArgumentException("endMonth < 1 || endMonth > 12", nameof(endMonth));
            if (endYear < 1900 || endYear > 2100)
                throw new ArgumentException("endYear < 1900 || endYear > 2100", nameof(endYear));

            StartMonth = startMonth;
            StartYear = startYear;
            EndMonth = endMonth;
            EndYear = endYear;
        }

        public MonthYearRange(MonthYear start, MonthYear end)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));
            if (end == null)
                throw new ArgumentNullException(nameof(end));

            StartYear = start.Year;
            StartMonth = start.Month;
            EndYear = end.Year;
            EndMonth = end.Month;
        }

        public MonthYearRange(MonthYearRange other)
        {
            if (other == null)
                return;

            StartYear = other.StartYear;
            StartMonth = other.StartMonth;
            EndYear = other.EndYear;
            EndMonth = other.EndMonth;
        }

        public int StartYear { get; }

        public int StartMonth { get; }

        public int EndYear { get; }

        public int EndMonth { get; }

        public MonthYear End => new MonthYear(EndMonth, EndYear);

        public MonthYear Start => new MonthYear(StartMonth, StartYear);

        /// <inheritdoc />
        public bool Equals(MonthYearRange other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return StartYear == other.StartYear && StartMonth == other.StartMonth && EndYear == other.EndYear && EndMonth == other.EndMonth;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is MonthYearRange other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => StartYear * 12 + StartMonth + (EndYear * 12 + EndMonth) * 100000;

        public int MonthDifference()
            => MonthYear.MonthDifference(Start, End);

        /// <inheritdoc />
        public override string ToString()
            => $"{Start} => {End}";
    }
}
