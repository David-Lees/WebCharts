using System;
using System.Globalization;
using System.Linq;

namespace WebCharts.Services
{
    public class YaxisSettings
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="parCode"></param>
        /// <param name="seriesCollection"></param>
        public YaxisSettings(string parCode, SeriesCollection seriesCollection)
        {
            switch (parCode.ToUpper())
            {
                case "W_PDIR":
                    Minimum = 0;
                    Maximum = 360;
                    Interval = 30;
                    break;

                case "W_SPR":
                    Minimum = 0;
                    Maximum = 90;
                    Interval = 10;
                    break;

                default:
                    // Get value range for y-Axis
                    double valuesMin;
                    double valuesMax;
                    try
                    {
                        valuesMin =
                            (from series
                             in seriesCollection
                             select series.Points.FindMinByValue().YValues[0])
                             .Min();
                        valuesMax =
                            (from series
                             in seriesCollection
                             select series.Points.FindMaxByValue().YValues[0])
                             .Max();
                    }
                    catch
                    {
                        valuesMin = 0;
                        valuesMax = 100;
                    }

                    // Derive Minimum and Maximum
                    const double MINMAX_ADJUSTMENT = 0.001;
                    var minFactor = GetFactor(valuesMin);
                    var minimum = SignificantDigits.ToSignificantDigits(Math.Floor(valuesMin * minFactor) / minFactor, 3);
                    while (minimum > valuesMin - valuesMin * MINMAX_ADJUSTMENT)
                    {
                        minimum -= minimum * MINMAX_ADJUSTMENT;
                    }
                    Minimum = minimum;
                    var maxFactor = GetFactor(valuesMax);
                    var maximum = SignificantDigits.ToSignificantDigits(Math.Ceiling(valuesMax * maxFactor) / maxFactor, 3);
                    while (maximum < valuesMax + valuesMax * MINMAX_ADJUSTMENT)
                    {
                        maximum += maximum * MINMAX_ADJUSTMENT;
                    }
                    Maximum = maximum;

                    // Ensure minimum range
                    const double RANGE_MIN = 0.3;
                    if (Range < RANGE_MIN)
                    {
                        var correction = (RANGE_MIN - Range) / 2;
                        Minimum -= correction;
                        Maximum += correction;
                    }

                    // 'Normalise' min/max and derive interval
                    if (Range <= 0.6)
                    {
                        Minimum = Math.Floor(Minimum * 20) / 20;
                        Maximum = Math.Ceiling(Maximum * 20) / 20;
                        Interval = 0.05;
                    }
                    else if (Range <= 2.5)
                    {
                        Minimum = Math.Floor(Minimum * 10) / 10;
                        Maximum = Math.Ceiling(Maximum * 10) / 10;
                        Interval = 0.25;
                    }
                    else if (Range <= 5)
                    {
                        Minimum = Math.Floor(Minimum * 5) / 5;
                        Maximum = Math.Ceiling(Maximum * 5) / 5;
                        Interval = 0.5;
                    }
                    else if (Range <= 10)
                    {
                        Minimum = Adjust(AdjustType.Down, Minimum, 2);
                        Maximum = Adjust(AdjustType.Up, Maximum, 2);
                        Interval = 1;
                    }
                    else
                    {
                        Minimum = Adjust(AdjustType.Down, Minimum, 1);
                        Maximum = Adjust(AdjustType.Up, Maximum, 1);
                        Interval = SignificantDigits.ToSignificantDigits(Range / 10, 2);
                    }

                    // Minor tick interval
                    MinorTickInterval = Range / Interval < 6
                        ? Interval / 2
                        : default(double?);

                    // Derive label format (defaults to "f0"):
                    for (var format = 0; format < 4; format++)
                    {
                        var formatString = $"f{format}";
                        if (double.Parse(Interval.ToString(formatString)) == Interval)
                        {
                            LabelFormat = formatString;
                            break;
                        }
                    }

                    break;
            }
        }

        public double Minimum { get; }
        public double Maximum { get; }
        private double Range => Maximum - Minimum;
        public double Interval { get; }
        public string LabelFormat { get; }
        public double? MinorTickInterval { get; }

        private static double GetFactor(double value)
        {
            var factor = 0d;
            var valueToSF = SignificantDigits.ToSignificantDigits(value, 6);
            while (valueToSF * Math.Pow(10, factor) != (int)(valueToSF * Math.Pow(10, factor)) && factor < 10)
            {
                factor++;
            }
            return Math.Pow(10, factor);
        }

        private static double Adjust(AdjustType adjustType, double value, int sf)
        {
            var adjusted = adjustType == AdjustType.Down
                ? SignificantDigits.ToSignificantDigits(Math.Floor(value), sf)
                : SignificantDigits.ToSignificantDigits(Math.Ceiling(value), sf);
            var adjust = adjustType == AdjustType.Down
                ? adjusted > value
                : adjusted < value;
            while (adjust)
            {
                if (adjustType == AdjustType.Down)
                {
                    adjusted -= 1;
                    adjust = adjusted > value;
                }
                else
                {
                    adjusted += 1;
                    adjust = adjusted < value;
                }
            }
            return adjusted;
        }

        private enum AdjustType
        {
            Up,
            Down
        }
    }

    /// <summary>
    ///
    /// </summary>
    public static class SignificantDigits
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="significantDigits"></param>
        /// <returns></returns>
        public static double ToSignificantDigits(this double value, int significantDigits)
        {
            return RoundSignificantDigits(value, significantDigits, out _);
        }

        /// <summary>
        /// this method will round and then append zeros if needed.
        /// i.e. if you round .002 to two significant figures, the resulting number should be .0020.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="significantDigits"></param>
        /// <returns></returns>
        public static string AsFormattedString(this double value, int significantDigits)
        {
            var currentInfo = CultureInfo.CurrentCulture.NumberFormat;
            if (double.IsNaN(value))
            {
                return currentInfo.NaNSymbol;
            }
            if (double.IsPositiveInfinity(value))
            {
                return currentInfo.PositiveInfinitySymbol;
            }
            if (double.IsNegativeInfinity(value))
            {
                return currentInfo.NegativeInfinitySymbol;
            }

            var roundedValue = RoundSignificantDigits(value, significantDigits, out _);
            // when rounding causes a cascading round affecting digits of greater significance,
            // need to re-round to get a correct rounding position afterwards
            // this fixes a bug where rounding 9.96 to 2 figures yeilds 10.0 instead of 10
            RoundSignificantDigits(roundedValue, significantDigits, out int roundingPosition);

            if (Math.Abs(roundingPosition) > 9)
            {
                return string.Format(currentInfo, "{0:E" + (significantDigits - 1) + "}", roundedValue);
            }

            // string.format is only needed with decimal numbers (whole numbers won't need to be padded with zeros to the right.)
            return roundingPosition > 0 ?
                string.Format(currentInfo, "{0:F" + roundingPosition + "}", roundedValue)
                : roundedValue.ToString(currentInfo);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NearEnoughZero(double value)
        {
            if (value == 0d)
            {
                return true;
            }
            return Math.Abs(value - 0d) < Math.Pow(2, -53);
        }

        /// <summary>
        /// this method will return a rounded double value at a number of signifigant figures.
        /// the sigFigures parameter must be between 0 and 15, exclusive.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="significantDigits"></param>
        /// <param name="roundingPosition"></param>
        /// <returns></returns>
        private static double RoundSignificantDigits(double value, int significantDigits, out int roundingPosition)
        {
            // Check for (near enough) zero
            if (NearEnoughZero(value))
            {
                roundingPosition = significantDigits - 1;
                return 0d;
            }

            // Check for various 'special' values and return as appropriate
            roundingPosition = 0;
            if (double.IsNaN(value))
            {
                return double.NaN;
            }
            if (double.IsPositiveInfinity(value))
            {
                return double.PositiveInfinity;
            }
            if (double.IsNegativeInfinity(value))
            {
                return double.NegativeInfinity;
            }

            // Check argument (don't use it until now)
            if (significantDigits < 1 || significantDigits > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(significantDigits), value, "Must be in the range 1 to 15");
            }

            // The resulting rounding position will be negative for rounding at whole numbers, and positive for decimal places.
            roundingPosition = significantDigits - 1 - (int)Math.Floor(Math.Log10(Math.Abs(value)));

            // Try to use a rounding position directly, if no scale is needed.
            // This is because the scale mutliplication after the rounding can introduce error,
            // although this only happens when dealing with really tiny numbers, i.e 9.9e-14.
            if (roundingPosition > 0 && roundingPosition < 16)
            {
                return Math.Round(value, roundingPosition, MidpointRounding.AwayFromZero);
            }

            // Shouldn't get here unless we need to scale it.
            // Set the scaling value, for rounding whole numbers or decimals past 15 places
            var scale = Math.Pow(10, Math.Ceiling(Math.Log10(Math.Abs(value))));
            return Math.Round(value / scale, significantDigits, MidpointRounding.AwayFromZero) * scale;
        }
    }
}