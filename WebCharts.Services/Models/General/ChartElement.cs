// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	The chart element is base class for the big number
//				of classes. It stores common methods and data.
//

using System;

namespace WebCharts.Services
{
    #region ChartElement

    /// <summary>
    /// Common chart helper methods used across different chart elements.
    /// </summary>
    internal class ChartHelper
    {
        #region Fields

        /// <summary>
        /// Maximum number of grid lines per Axis
        /// </summary>
        internal const int MaxNumOfGridlines = 10000;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Private constructor to avoid instantiating the class
        /// </summary>
        private ChartHelper() { }

        #endregion Constructor

        #region Methods

        /// <summary>
		/// Adjust the beginnin of the first interval depending on the type and size.
		/// </summary>
		/// <param name="start">Original start point.</param>
		/// <param name="intervalSize">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <returns>Adjusted interval start position as double.</returns>
        internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type)
        {
            return AlignIntervalStart(start, intervalSize, type, null);
        }

        /// <summary>
        /// Adjust the beginnin of the first interval depending on the type and size.
        /// </summary>
        /// <param name="start">Original start point.</param>
        /// <param name="intervalSize">Interval size.</param>
        /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
        /// <param name="series">First series connected to the axis.</param>
        /// <returns>Adjusted interval start position as double.</returns>
        internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type, Series series)
        {
            return AlignIntervalStart(start, intervalSize, type, series, true);
        }

        /// <summary>
        /// Adjust the beginnin of the first interval depending on the type and size.
        /// </summary>
        /// <param name="start">Original start point.</param>
        /// <param name="intervalSize">Interval size.</param>
        /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
        /// <param name="series">First series connected to the axis.</param>
        /// <param name="majorInterval">Interval is used for major gridlines or tickmarks.</param>
        /// <returns>Adjusted interval start position as double.</returns>
        internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type, Series series, bool majorInterval)
        {
            // Special case for indexed series
            if (series != null && series.IsXValueIndexed)
            {
                if (type == DateTimeIntervalType.Auto ||
                    type == DateTimeIntervalType.Number)
                {
                    if (majorInterval)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }

                return -(series.Points.Count + 1);
            }

            // Non indexed series
            else
            {
                // Do not adjust start position for these interval type
                if (type == DateTimeIntervalType.Auto ||
                    type == DateTimeIntervalType.Number)
                {
                    return start;
                }

                // Get the beginning of the interval depending on type
                DateTime newStartDate = DateTime.FromOADate(start);

                // Adjust the months interval depending on size
                if (intervalSize > 0.0 && intervalSize != 1.0 && type == DateTimeIntervalType.Months && intervalSize <= 12.0 && intervalSize > 1)
                {
                    // Make sure that the beginning is aligned correctly for cases
                    // like quarters and half years
                    DateTime resultDate = newStartDate;
                    DateTime sizeAdjustedDate = new(newStartDate.Year, 1, 1, 0, 0, 0);
                    while (sizeAdjustedDate < newStartDate)
                    {
                        resultDate = sizeAdjustedDate;
                        sizeAdjustedDate = sizeAdjustedDate.AddMonths((int)intervalSize);
                    }

                    newStartDate = resultDate;
                    return newStartDate.ToOADate();
                }

                // Check interval type
                switch (type)
                {
                    case (DateTimeIntervalType.Years):
                        int year = (int)((int)(newStartDate.Year / intervalSize) * intervalSize);
                        if (year <= 0)
                        {
                            year = 1;
                        }
                        newStartDate = new DateTime(year,
                            1, 1, 0, 0, 0);
                        break;

                    case (DateTimeIntervalType.Months):
                        int month = (int)((int)(newStartDate.Month / intervalSize) * intervalSize);
                        if (month <= 0)
                        {
                            month = 1;
                        }
                        newStartDate = new DateTime(newStartDate.Year,
                            month, 1, 0, 0, 0);
                        break;

                    case (DateTimeIntervalType.Days):
                        int day = (int)((int)(newStartDate.Day / intervalSize) * intervalSize);
                        if (day <= 0)
                        {
                            day = 1;
                        }
                        newStartDate = new DateTime(newStartDate.Year,
                            newStartDate.Month, day, 0, 0, 0);
                        break;

                    case (DateTimeIntervalType.Hours):
                        int hour = (int)((int)(newStartDate.Hour / intervalSize) * intervalSize);
                        newStartDate = new DateTime(newStartDate.Year,
                            newStartDate.Month, newStartDate.Day, hour, 0, 0);
                        break;

                    case (DateTimeIntervalType.Minutes):
                        int minute = (int)((int)(newStartDate.Minute / intervalSize) * intervalSize);
                        newStartDate = new DateTime(newStartDate.Year,
                            newStartDate.Month,
                            newStartDate.Day,
                            newStartDate.Hour,
                            minute,
                            0);
                        break;

                    case (DateTimeIntervalType.Seconds):
                        int second = (int)((int)(newStartDate.Second / intervalSize) * intervalSize);
                        newStartDate = new DateTime(newStartDate.Year,
                            newStartDate.Month,
                            newStartDate.Day,
                            newStartDate.Hour,
                            newStartDate.Minute,
                            second,
                            0);
                        break;

                    case (DateTimeIntervalType.Milliseconds):
                        int milliseconds = (int)((int)(newStartDate.Millisecond / intervalSize) * intervalSize);
                        newStartDate = new DateTime(newStartDate.Year,
                            newStartDate.Month,
                            newStartDate.Day,
                            newStartDate.Hour,
                            newStartDate.Minute,
                            newStartDate.Second,
                            milliseconds);
                        break;

                    case (DateTimeIntervalType.Weeks):

                        // NOTE: Code below was changed to fix issue #5962
                        // Elements that have interval set to weeks should be aligned to the
                        // nearest Monday no matter how many weeks is the interval.
                        //newStartDate = newStartDate.AddDays(-((int)newStartDate.DayOfWeek * intervalSize));
                        newStartDate = newStartDate.AddDays(-((int)newStartDate.DayOfWeek));
                        newStartDate = new DateTime(newStartDate.Year,
                            newStartDate.Month, newStartDate.Day, 0, 0, 0);
                        break;
                }

                return newStartDate.ToOADate();
            }
        }

        /// <summary>
        /// Gets interval size as double number.
        /// </summary>
        /// <param name="current">Current value.</param>
        /// <param name="interval">Interval size.</param>
        /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
        /// <returns>Interval size as double.</returns>
        internal static double GetIntervalSize(double current, double interval, DateTimeIntervalType type)
        {
            return GetIntervalSize(
                current,
                interval,
                type,
                null,
                0,
                DateTimeIntervalType.Number,
                true,
                true);
        }

        /// <summary>
        /// Gets interval size as double number.
        /// </summary>
        /// <param name="current">Current value.</param>
        /// <param name="interval">Interval size.</param>
        /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
        /// <param name="series">First series connected to the axis.</param>
        /// <param name="intervalOffset">Offset size.</param>
        /// <param name="intervalOffsetType">Offset type(Month, Year, ...).</param>
        /// <param name="forceIntIndex">Force Integer indexed</param>
        /// <returns>Interval size as double.</returns>
        internal static double GetIntervalSize(
            double current,
            double interval,
            DateTimeIntervalType type,
            Series series,
            double intervalOffset,
            DateTimeIntervalType intervalOffsetType,
            bool forceIntIndex)
        {
            return GetIntervalSize(
                current,
                interval,
                type,
                series,
                intervalOffset,
                intervalOffsetType,
                forceIntIndex,
                true);
        }

        /// <summary>
        /// Gets interval size as double number.
        /// </summary>
        /// <param name="current">Current value.</param>
        /// <param name="interval">Interval size.</param>
        /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
        /// <param name="series">First series connected to the axis.</param>
        /// <param name="intervalOffset">Offset size.</param>
        /// <param name="intervalOffsetType">Offset type(Month, Year, ...).</param>
        /// <param name="forceIntIndex">Force Integer indexed</param>
        /// <param name="forceAbsInterval">Force Integer indexed</param>
        /// <returns>Interval size as double.</returns>
        internal static double GetIntervalSize(
            double current,
            double interval,
            DateTimeIntervalType type,
            Series series,
            double intervalOffset,
            DateTimeIntervalType intervalOffsetType,
            bool forceIntIndex,
            bool forceAbsInterval)
        {
            // AxisName is not date.
            if (type == DateTimeIntervalType.Number || type == DateTimeIntervalType.Auto)
            {
                return interval;
            }

            // Special case for indexed series
            if (series != null && series.IsXValueIndexed)
            {
                // Check point index
                int pointIndex = (int)Math.Ceiling(current - 1);
                if (pointIndex < 0)
                {
                    pointIndex = 0;
                }
                if (pointIndex >= series.Points.Count || series.Points.Count <= 1)
                {
                    return interval;
                }

                // Get starting and ending values of the closest interval
                double adjuster = 0;
                double xValue = series.Points[pointIndex].XValue;
                xValue = AlignIntervalStart(xValue, 1, type, null);
                double xEndValue = xValue + GetIntervalSize(xValue, interval, type);
                xEndValue += GetIntervalSize(xEndValue, intervalOffset, intervalOffsetType);
                xValue += GetIntervalSize(xValue, intervalOffset, intervalOffsetType);
                if (intervalOffset < 0)
                {
                    xValue += GetIntervalSize(xValue, interval, type);
                    xEndValue += GetIntervalSize(xEndValue, interval, type);
                }

                // The first point in the series
                if (pointIndex == 0 && current < 0)
                {
                    // Round the first point value depending on the interval type
                    DateTime dateValue = DateTime.FromOADate(series.Points[pointIndex].XValue);
                    DateTime roundedDateValue = dateValue;
                    switch (type)
                    {
                        case (DateTimeIntervalType.Years): // Ignore hours,...
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month, dateValue.Day, 0, 0, 0);
                            break;

                        case (DateTimeIntervalType.Months): // Ignore hours,...
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month, dateValue.Day, 0, 0, 0);
                            break;

                        case (DateTimeIntervalType.Days): // Ignore hours,...
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month, dateValue.Day, 0, 0, 0);
                            break;

                        case (DateTimeIntervalType.Hours): //
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month, dateValue.Day, dateValue.Hour,
                                dateValue.Minute, 0);
                            break;

                        case (DateTimeIntervalType.Minutes):
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month,
                                dateValue.Day,
                                dateValue.Hour,
                                dateValue.Minute,
                                dateValue.Second);
                            break;

                        case (DateTimeIntervalType.Seconds):
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month,
                                dateValue.Day,
                                dateValue.Hour,
                                dateValue.Minute,
                                dateValue.Second,
                                0);
                            break;

                        case (DateTimeIntervalType.Weeks):
                            roundedDateValue = new DateTime(dateValue.Year,
                                dateValue.Month, dateValue.Day, 0, 0, 0);
                            break;
                    }

                    // The first point value is exactly on the interval boundaries
                    if (roundedDateValue.ToOADate() == xValue || roundedDateValue.ToOADate() == xEndValue)
                    {
                        return -current + 1;
                    }
                }

                // Adjuster of 0.5 means that position should be between points
                ++pointIndex;
                while (pointIndex < series.Points.Count)
                {
                    if (series.Points[pointIndex].XValue >= xEndValue)
                    {
                        if (series.Points[pointIndex].XValue > xEndValue && !forceIntIndex)
                        {
                            adjuster = -0.5;
                        }
                        break;
                    }

                    ++pointIndex;
                }

                // If last point outside of the max series index
                if (pointIndex == series.Points.Count)
                {
                    pointIndex += series.Points.Count / 5 + 1;
                }

                double size = (pointIndex + 1) - current + adjuster;

                return (size != 0) ? size : interval;
            }

            // Non indexed series
            else
            {
                DateTime date = DateTime.FromOADate(current);
                TimeSpan span = new(0);

                if (type == DateTimeIntervalType.Days)
                {
                    span = TimeSpan.FromDays(interval);
                }
                else if (type == DateTimeIntervalType.Hours)
                {
                    span = TimeSpan.FromHours(interval);
                }
                else if (type == DateTimeIntervalType.Milliseconds)
                {
                    span = TimeSpan.FromMilliseconds(interval);
                }
                else if (type == DateTimeIntervalType.Seconds)
                {
                    span = TimeSpan.FromSeconds(interval);
                }
                else if (type == DateTimeIntervalType.Minutes)
                {
                    span = TimeSpan.FromMinutes(interval);
                }
                else if (type == DateTimeIntervalType.Weeks)
                {
                    span = TimeSpan.FromDays(7.0 * interval);
                }
                else if (type == DateTimeIntervalType.Months)
                {
                    // Special case handling when current date points
                    // to the last day of the month
                    bool lastMonthDay = false;
                    if (date.Day == DateTime.DaysInMonth(date.Year, date.Month))
                    {
                        lastMonthDay = true;
                    }

                    // Add specified amount of months
                    date = date.AddMonths((int)Math.Floor(interval));
                    span = TimeSpan.FromDays(30.0 * (interval - Math.Floor(interval)));

                    // Check if last month of the day was used
                    if (lastMonthDay && span.Ticks == 0)
                    {
                        // Make sure the last day of the month is selected
                        int daysInMobth = DateTime.DaysInMonth(date.Year, date.Month);
                        date = date.AddDays(daysInMobth - date.Day);
                    }
                }
                else if (type == DateTimeIntervalType.Years)
                {
                    date = date.AddYears((int)Math.Floor(interval));
                    span = TimeSpan.FromDays(365.0 * (interval - Math.Floor(interval)));
                }

                // Check if an absolute interval size must be returned
                double result = date.Add(span).ToOADate() - current;
                if (forceAbsInterval)
                {
                    result = Math.Abs(result);
                }
                return result;
            }
        }

        /// <summary>
        /// Check if series is indexed. IsXValueIndexed flag is set or all X values are zeros.
        /// </summary>
        /// <param name="series">Data series to test.</param>
        /// <returns>True if series is indexed.</returns>
        static internal bool IndexedSeries(Series series)
        {
            // X value indexed flag set
            if (series.IsXValueIndexed)
            {
                return true;
            }

            if (CustomPropertyRegistry.IsXAxisQuantitativeChartTypes.Contains(series.ChartType) &&
                series.IsCustomPropertySet(CustomPropertyName.IsXAxisQuantitative))
            {
                string attribValue = series[CustomPropertyName.IsXAxisQuantitative];
                if (string.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }

            // Check if series has all X values set to zero
            return SeriesXValuesZeros(series);
        }

        /// <summary>
        /// Check if all data points in the series have X value set to 0.
        /// </summary>
        /// <param name="series">Data series to check.</param>
        static private bool SeriesXValuesZeros(Series series)
        {
            // Check if X value zeros check was already done
            if (series.xValuesZerosChecked)
            {
                return series.xValuesZeros;
            }

            // Data point loop
            series.xValuesZerosChecked = true;
            series.xValuesZeros = true;
            foreach (DataPoint point in series.Points)
            {
                if (point.XValue != 0.0)
                {
                    // If any data point has value different than 0 return false
                    series.xValuesZeros = false;
                    break;
                }
            }
            return series.xValuesZeros;
        }

        /// <summary>
        /// Check if any series is indexed. IsXValueIndexed flag is set or all X values are zeros.
        /// </summary>
        /// <param name="common">Reference to common chart classes.</param>
        /// <param name="series">Data series names.</param>
        /// <returns>True if any series is indexed.</returns>
        static internal bool IndexedSeries(CommonElements common, params string[] series)
        {
            // Data series loop
            bool zeroXValues = true;
            foreach (string ser in series)
            {
                Series localSeries = common.DataManager.Series[ser];

                // Check series indexed flag
                if (localSeries.IsXValueIndexed)
                {
                    // If flag set in at least one series - all series are indexed
                    return true;
                }

                // Check if series has all X values set to zero
                if (zeroXValues && !IndexedSeries(localSeries))
                {
                    zeroXValues = false;
                }
            }

            return zeroXValues;
        }

        /// <summary>
        /// Check if all data points in many series have X value set to 0.
        /// </summary>
        /// <param name="common">Reference to common chart classes.</param>
        /// <param name="series">Data series.</param>
        /// <returns>True if all data points have value 0.</returns>
        static internal bool SeriesXValuesZeros(CommonElements common, params string[] series)
        {
            // Data series loop
            foreach (string ser in series)
            {
                // Check one series X values
                if (!SeriesXValuesZeros(common.DataManager.Series[ser]))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion Methods
    }

    #endregion ChartElement
}