using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace MortgageInvestmentSimulator
{
    /// <summary>
    ///     A class representing a month and year.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class MonthYear : IEquatable<MonthYear>, IComparable<MonthYear>
    {
        public MonthYear(int month, int year)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("month < 1 || month > 12", nameof(month));
            if (year < 1900 || year > 2100)
                throw new ArgumentException("year < 1900 || year > 2100", nameof(year));

            Month = month;
            Year = year;
        }

        public MonthYear(MonthYear other)
        {
            if (other == null)
                return;

            Year = other.Year;
            Month = other.Month;
        }

        public MonthYear(DateTime other)
        {
            Year = other.Year;
            Month = other.Month;
        }

        /// <summary>
        ///     Gets the minimum month year.
        /// </summary>
        /// <value>The minimum month year.</value>
        /// <remarks>We start a little later so we can go back and get average dividend.</remarks>
        public static MonthYear Min => new MonthYear(4, 1972);

        public static MonthYear Max => new MonthYear(9, 2018);

        /// <summary>
        ///     Gets the month year used for the adjustment point for inflation calculations..
        /// </summary>
        /// <value>The maximum month year.</value>
        public static MonthYear BaseLine => new MonthYear(9, 2018);

        public int Year { get; }

        public int Month { get; }

        public MonthYear AddMonths(int months)
        {
            var dateTime = (DateTime)this;
            return new MonthYear(dateTime.AddMonths(months));
        }

        public MonthYear AddYears(int years)
        {
            var dateTime = (DateTime)this;
            return new MonthYear(dateTime.AddYears(years));
        }

        /// <inheritdoc />
        public int CompareTo(MonthYear other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;

            var yearComparison = Year.CompareTo(other.Year);
            return yearComparison != 0 ? yearComparison : Month.CompareTo(other.Month);
        }

        public static MonthYear Constrain(MonthYear monthYear)
        {
            if (monthYear < Min)
                return Min;
            if (monthYear > Max)
                return Max;

            return monthYear;
        }

        /// <inheritdoc />
        public bool Equals(MonthYear other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Year == other.Year && Month == other.Month;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is MonthYear other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Year * 12 + Month;

        public static int MonthDifference(MonthYear left, MonthYear right)
            => Math.Abs((left.Month - right.Month) + 12 * (left.Year - right.Year));

        /// <inheritdoc />
        public override string ToString()
            => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)} {Year}";

        public static bool operator >(MonthYear left, MonthYear right) => Comparer<MonthYear>.Default.Compare(left, right) > 0;

        public static bool operator >=(MonthYear left, MonthYear right) => Comparer<MonthYear>.Default.Compare(left, right) >= 0;

        public static implicit operator DateTime(MonthYear now) => new DateTime(now.Year, now.Month, 1);

        public static bool operator <(MonthYear left, MonthYear right) => Comparer<MonthYear>.Default.Compare(left, right) < 0;

        public static bool operator <=(MonthYear left, MonthYear right) => Comparer<MonthYear>.Default.Compare(left, right) <= 0;

        public static TimeSpan operator -(MonthYear left, MonthYear right) => (DateTime)left - (DateTime)right;
    }
}
