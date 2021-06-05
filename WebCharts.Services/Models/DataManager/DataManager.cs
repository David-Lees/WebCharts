// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Series storage and manipulation class.
//

using SkiaSharp;
using System;

namespace WebCharts.Services
{
    internal interface IDataManager
    {
        ChartColorPalette Palette { get; set; }
        SKColor[] PaletteCustomColors { get; set; }
        SeriesCollection Series { get; }

        double GetMaxHundredPercentStackedYValue(bool supportNegative, params string[] series);

        double GetMaxStackedXValue(params string[] series);

        double GetMaxStackedYValue(int valueIndex, params string[] series);

        double GetMaxUnsignedStackedYValue(int valueIndex, params string[] series);

        double GetMaxXValue(params string[] series);

        double GetMaxXWithRadiusValue(ChartArea area, params string[] series);

        double GetMaxYValue(int valueIndex, params string[] series);

        double GetMaxYValue(params string[] series);

        double GetMaxYWithRadiusValue(ChartArea area, params string[] series);

        double GetMinHundredPercentStackedYValue(bool supportNegative, params string[] series);

        void GetMinMaxXValue(out double min, out double max, params string[] series);

        void GetMinMaxYValue(int valueIndex, out double min, out double max, params string[] series);

        void GetMinMaxYValue(out double min, out double max, params string[] series);

        void GetMinMaxYValue(System.Collections.ArrayList seriesList, out double min, out double max);

        double GetMinStackedXValue(params string[] series);

        double GetMinStackedYValue(int valueIndex, params string[] series);

        double GetMinUnsignedStackedYValue(int valueIndex, params string[] series);

        double GetMinXValue(params string[] series);

        double GetMinXWithRadiusValue(ChartArea area, params string[] series);

        double GetMinYValue(int valueIndex, params string[] series);

        double GetMinYValue(params string[] series);

        double GetMinYWithRadiusValue(ChartArea area, params string[] series);

        int GetNumberOfPoints(params string[] series);
    }

    /// <summary>
    /// Data Manager.
    /// </summary>
    internal class DataManager : ChartElement, IServiceProvider, IDataManager
    {
        #region Fields

        // Chart color palette

        // Series collection
        private SeriesCollection _series = null;
        #endregion Fields

        #region Constructors and initialization

        /// <summary>
        /// Data manager public constructor
        /// </summary>
        /// <param name="container">Service container object.</param>
        public DataManager(CommonElements common)
        {
            Common = common;
            _series = new SeriesCollection(this);
        }

        /// <summary>
        /// Returns Data Manager service object.
        /// </summary>
        /// <param name="serviceType">Service type requested.</param>
        /// <returns>Data Manager service object.</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(DataManager))
            {
                return this;
            }
            throw (new ArgumentException(SR.ExceptionDataManagerUnsupportedType(serviceType.ToString())));
        }

        /// <summary>
        /// Initialize data manger object
        /// </summary>
        internal void Initialize(ChartImage chartPicture)
        {
            // Attach to the Chart Picture painting events
            chartPicture.BeforePaint += new EventHandler<ChartPaintEventArgs>(ChartPicture_BeforePaint);
            chartPicture.AfterPaint += new EventHandler<ChartPaintEventArgs>(ChartPicture_AfterPaint);
        }

        #endregion Constructors and initialization

        #region Chart picture painting events hanlers

        /// <summary>
        /// Event fired after chart picture was painted.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
		private void ChartPicture_AfterPaint(object sender, ChartPaintEventArgs e)
        {
            ChartService control = Common.Chart;
            if (control != null)
            {
                // Clean up series after drawing
                for (int index = 0; index < Series.Count; index++)
                {
                    Series series = Series[index];
                    if (series.UnPrepareData())
                    {
                        --index;
                    }
                }
            }
        }

        /// <summary>
        /// Event fired when chart picture is going to be painted.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void ChartPicture_BeforePaint(object sender, ChartPaintEventArgs e)
        {
            // Prepare series for drawing
            int markerIndex = 1;
            for (int index = 0; index < Series.Count; index++)
            {
                Series series = Series[index];

                // Reset series "X values are zeros" flag
                series.xValuesZerosChecked = false;
                series.xValuesZeros = false;

                // Set series colors from palette
                IChartType chartType = e.CommonElements.ChartTypeRegistry.GetChartType(series.ChartTypeName);
                bool paletteColorsInPoints = chartType.ApplyPaletteColorsToPoints;
                // if the series palette is set the we can color all data points, even on column chart.
                if (series.Palette != ChartColorPalette.None)
                {
                    paletteColorsInPoints = true;
                }

                PrepareData(
                    paletteColorsInPoints,
                    series.Name);

                // Clear temp. marker style
                if (series.tempMarkerStyleIsSet)
                {
                    series.MarkerStyle = MarkerStyle.None;
                    series.tempMarkerStyleIsSet = false;
                }

                // Set marker style for chart types based on markes
                if (chartType.GetLegendImageStyle(series) == LegendImageStyle.Marker && series.MarkerStyle == MarkerStyle.None)
                {
                    series.MarkerStyle = (MarkerStyle)markerIndex++;
                    series.tempMarkerStyleIsSet = true;

                    if (markerIndex > 9)
                    {
                        markerIndex = 1;
                    }
                }
            }
        }
        #endregion Chart picture painting events hanlers

        #region Series data preparation methods

        /// <summary>
        /// Apply palette colors to the data series if UsePaletteColors property is set.
        /// </summary>
        internal void ApplyPaletteColors()
        {
            ChartColorPalette palette = Palette;
            // switch to default pallette if is none and custom collors array is empty.
            if (palette == ChartColorPalette.None && PaletteCustomColors.Length == 0)
            {
                palette = ChartColorPalette.BrightPastel;
            }

            // Get palette colors
            int colorIndex = 0;
            SKColor[] paletteColors = (palette == ChartColorPalette.None) ?
                PaletteCustomColors : ChartPaletteColors.GetPaletteColors(palette);

            foreach (Series dataSeries in _series)
            {
                // Check if chart area name is valid
                bool validAreaName = false;
                if (Chart != null)
                {
                    validAreaName = Chart.ChartAreas.IsNameReferenceValid(dataSeries.ChartArea);
                }

                // Change color of the series only if valid chart area name is specified
                if (validAreaName)
                {
                    // Change color of the series only if default color is set
                    if (dataSeries.Color == SKColor.Empty || dataSeries.tempColorIsSet)
                    {
                        dataSeries.color = paletteColors[colorIndex++];
                        dataSeries.tempColorIsSet = true;
                        if (colorIndex >= paletteColors.Length)
                        {
                            colorIndex = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called just before the data from the series to be used to perform these operations:
        ///  - apply palette colors to the data series
        ///  - prepare data in series
        /// </summary>
        /// <param name="pointsApplyPaletteColors">If true each data point will be assigned a color from the palette (if it's set)</param>
        /// <param name="series">List of series indexes, which requires data preparation</param>
        internal void PrepareData(bool pointsApplyPaletteColors, params string[] series)
        {
            ApplyPaletteColors();

            // Prepare data in series
            ChartService control = Common.Chart;
            if (control != null)
            {
                foreach (string seriesName in series)
                {
                    Series[seriesName].PrepareData(pointsApplyPaletteColors);
                }
            }
        }

        #endregion Series data preparation methods

        #region Series Min/Max values methods

        /// <summary>
        /// Gets maximum hundred percent stacked Y value
        /// </summary>
        /// <param name="supportNegative">Indicates that negative values are shown on the other side of the axis.</param>
        /// <param name="series">Series names</param>
        /// <returns>Maximum 100% stacked Y value</returns>
        public double GetMaxHundredPercentStackedYValue(bool supportNegative, params string[] series)
        {
            double returnValue = 0;

            // Convert array of series names into array of series
            Series[] seriesArray = new Series[series.Length];
            int seriesIndex = 0;
            foreach (string seriesName in series)
            {
                seriesArray[seriesIndex++] = _series[seriesName];
            }

            // Loop through all dat points
            try
            {
                for (int pointIndex = 0; pointIndex < _series[series[0]].Points.Count; pointIndex++)
                {
                    // Calculate the total for all series
                    double totalPerPoint = 0;
                    double positiveTotalPerPoint = 0;
                    foreach (Series ser in seriesArray)
                    {
                        if (supportNegative)
                        {
                            totalPerPoint += Math.Abs(ser.Points[pointIndex].YValues[0]);
                        }
                        else
                        {
                            totalPerPoint += ser.Points[pointIndex].YValues[0];
                        }

                        if (ser.Points[pointIndex].YValues[0] > 0 || supportNegative == false)
                        {
                            positiveTotalPerPoint += ser.Points[pointIndex].YValues[0];
                        }
                    }
                    totalPerPoint = Math.Abs(totalPerPoint);

                    // Calculate percentage of total
                    if (totalPerPoint != 0)
                    {
                        returnValue = Math.Max(returnValue,
                            (positiveTotalPerPoint / totalPerPoint) * 100.0);
                    }
                }
            }
            catch (System.Exception)
            {
                throw (new InvalidOperationException(SR.ExceptionDataManager100StackedSeriesPointsNumeberMismatch));
            }

            return returnValue;
        }

        /// <summary>
        /// Gets maximum stacked X value from many series
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum stacked X value</returns>
        public double GetMaxStackedXValue(params string[] series)
        {
            double returnValue = 0;
            double numberOfPoints = GetNumberOfPoints(series);
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                double doubleIndexValue = 0;
                foreach (string seriesName in series)
                {
                    if (_series[seriesName].Points.Count > pointIndex)
                    {
                        if (_series[seriesName].Points[pointIndex].XValue > 0)
                        {
                            doubleIndexValue += _series[seriesName].Points[pointIndex].XValue;
                        }
                    }
                }
                returnValue = Math.Max(returnValue, doubleIndexValue);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets maximum stacked Y value from many series
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum stacked Y value</returns>
        public double GetMaxStackedYValue(int valueIndex, params string[] series)
        {
            double returnValue = 0;
            double numberOfPoints = GetNumberOfPoints(series);
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                double stackedMax = 0;
                double noStackedMax = 0;
                foreach (string seriesName in series)
                {
                    if (_series[seriesName].Points.Count > pointIndex)
                    {
                        // Take chart type from the series
                        ChartTypeRegistry chartTypeRegistry = Common.ChartTypeRegistry;
                        IChartType chartType = chartTypeRegistry.GetChartType(_series[seriesName].ChartTypeName);

                        // If stacked area
                        if (!chartType.StackSign)
                            continue;

                        if (chartType.Stacked)
                        {
                            if (_series[seriesName].Points[pointIndex].YValues[valueIndex] > 0)
                            {
                                stackedMax += _series[seriesName].Points[pointIndex].YValues[valueIndex];
                            }
                        }
                        else
                        {
                            noStackedMax = Math.Max(noStackedMax, _series[seriesName].Points[pointIndex].YValues[valueIndex]);
                        }
                    }
                }
                stackedMax = Math.Max(stackedMax, noStackedMax);
                returnValue = Math.Max(returnValue, stackedMax);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets maximum Unsigned stacked Y value from many series ( Stacked Area chart )
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum stacked Y value</returns>
        public double GetMaxUnsignedStackedYValue(int valueIndex, params string[] series)
        {
            double returnValue = 0;
            double maxValue = Double.MinValue;
            double numberOfPoints = GetNumberOfPoints(series);
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                double stackedMax = 0;
                double noStackedMax = 0;
                foreach (string seriesName in series)
                {
                    if (_series[seriesName].Points.Count > pointIndex)
                    {
                        // Take chart type from the series
                        ChartTypeRegistry chartTypeRegistry = Common.ChartTypeRegistry;
                        IChartType chartType = chartTypeRegistry.GetChartType(_series[seriesName].ChartTypeName);

                        // If stacked column and bar
                        if (chartType.StackSign || double.IsNaN(_series[seriesName].Points[pointIndex].YValues[valueIndex]))
                        {
                            continue;
                        }

                        if (chartType.Stacked)
                        {
                            maxValue = Double.MinValue;
                            stackedMax += _series[seriesName].Points[pointIndex].YValues[valueIndex];
                            if (stackedMax > maxValue)
                                maxValue = stackedMax;
                        }
                        else
                        {
                            noStackedMax = Math.Max(noStackedMax, _series[seriesName].Points[pointIndex].YValues[valueIndex]);
                        }
                    }
                }
                maxValue = Math.Max(maxValue, noStackedMax);
                returnValue = Math.Max(returnValue, maxValue);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets maximum X value from many series
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum X value</returns>
        public double GetMaxXValue(params string[] series)
        {
            double returnValue = Double.MinValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    returnValue = Math.Max(returnValue, seriesPoint.XValue);
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Get Maximum value for X and Radius (Y2) ( used for bubble chart )
        /// </summary>
        /// <param name="area">Chart Area</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum X value</returns>
        public double GetMaxXWithRadiusValue(ChartArea area, params string[] series)
        {
            double returnValue = Double.MinValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    if (!double.IsNaN(seriesPoint.XValue))
                    {
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.XValue + BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.XValue, false));
                        }
                        else
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.XValue);
                        }
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets maximum Y value from many series
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum Y value</returns>
        public double GetMaxYValue(int valueIndex, params string[] series)
        {
            double returnValue = Double.MinValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    if (!double.IsNaN(seriesPoint.YValues[valueIndex]))
                    {
                        returnValue = Math.Max(returnValue, seriesPoint.YValues[valueIndex]);
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets maximum Y value from many series
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum Y value</returns>
        public double GetMaxYValue(params string[] series)
        {
            double returnValue = Double.MinValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    foreach (double y in seriesPoint.YValues)
                    {
                        if (!double.IsNaN(y))
                        {
                            returnValue = Math.Max(returnValue, y);
                        }
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Get Maximum value for Y and and Radius (Y2) ( used for bubble chart )
        /// </summary>
        /// <param name="area">Chart Area</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum Y value</returns>
        public double GetMaxYWithRadiusValue(ChartArea area, params string[] series)
        {
            double returnValue = Double.MinValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    if (!double.IsNaN(seriesPoint.YValues[0]))
                    {
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.YValues[0] + BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], true));
                        }
                        else
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.YValues[0]);
                        }
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets minimum hundred percent stacked Y value
        /// </summary>
        /// <param name="supportNegative">Indicates that negative values are shown on the other side of the axis.</param>
        /// <param name="series">Series names</param>
        /// <returns>Minimum 100% stacked Y value</returns>
        public double GetMinHundredPercentStackedYValue(bool supportNegative, params string[] series)
        {
            double returnValue = 0.0;

            // Convert array of series names into array of series
            Series[] seriesArray = new Series[series.Length];
            int seriesIndex = 0;
            foreach (string seriesName in series)
            {
                seriesArray[seriesIndex++] = _series[seriesName];
            }

            // Loop through all dat points
            try
            {
                for (int pointIndex = 0; pointIndex < _series[series[0]].Points.Count; pointIndex++)
                {
                    // Calculate the total for all series
                    double totalPerPoint = 0;
                    double negativeTotalPerPoint = 0;
                    foreach (Series ser in seriesArray)
                    {
                        if (supportNegative)
                        {
                            totalPerPoint += Math.Abs(ser.Points[pointIndex].YValues[0]);
                        }
                        else
                        {
                            totalPerPoint += ser.Points[pointIndex].YValues[0];
                        }

                        if (ser.Points[pointIndex].YValues[0] < 0 || supportNegative == false)
                        {
                            negativeTotalPerPoint += ser.Points[pointIndex].YValues[0];
                        }
                    }

                    totalPerPoint = Math.Abs(totalPerPoint);

                    // Calculate percentage of total
                    if (totalPerPoint != 0)
                    {
                        returnValue = Math.Min(returnValue,
                            (negativeTotalPerPoint / totalPerPoint) * 100.0);
                    }
                }
            }
            catch (System.Exception)
            {
                throw (new InvalidOperationException(SR.ExceptionDataManager100StackedSeriesPointsNumeberMismatch));
            }

            return returnValue;
        }

        /// <summary>
        /// Gets minimum and maximum X value from many series.
        /// </summary>
        /// <param name="min">Returns maximum X value.</param>
        /// <param name="max">Returns minimum X value.</param>
        /// <param name="series">Series IDs</param>
        public void GetMinMaxXValue(out double min, out double max, params string[] series)
        {
            max = Double.MinValue;
            min = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    max = Math.Max(max, seriesPoint.XValue);
                    min = Math.Min(min, seriesPoint.XValue);
                }
            }
        }

        /// <summary>
        /// Gets minimum and maximum Y value from many series.
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use.</param>
        /// <param name="min">Returns maximum Y value.</param>
        /// <param name="max">Returns minimum Y value.</param>
        /// <param name="series">Series IDs</param>
        public void GetMinMaxYValue(int valueIndex, out double min, out double max, params string[] series)
        {
            max = Double.MinValue;
            min = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // Skip empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    double yValue = seriesPoint.YValues[valueIndex];
                    if (!double.IsNaN(yValue))
                    {
                        max = Math.Max(max, yValue);
                        min = Math.Min(min, yValue);
                    }
                }
            }
        }

        /// <summary>
        /// Gets minimum and maximum Y value from many series.
        /// </summary>
        /// <param name="min">Returns maximum Y value.</param>
        /// <param name="max">Returns minimum Y value.</param>
        /// <param name="series">Series IDs</param>
        public void GetMinMaxYValue(out double min, out double max, params string[] series)
        {
            max = Double.MinValue;
            min = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // Skip empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    // Iterate through all Y values
                    foreach (double y in seriesPoint.YValues)
                    {
                        if (!double.IsNaN(y))
                        {
                            max = Math.Max(max, y);
                            min = Math.Min(min, y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets minimum and maximum Y value from many series.
        /// </summary>
        /// <param name="seriesList">Series objects list.</param>
        /// <param name="min">Returns maximum Y value.</param>
        /// <param name="max">Returns minimum Y value.</param>
        public void GetMinMaxYValue(System.Collections.ArrayList seriesList, out double min, out double max)
        {
            max = Double.MinValue;
            min = Double.MaxValue;
            foreach (Series series in seriesList)
            {
                foreach (DataPoint seriesPoint in series.Points)
                {
                    // Skip empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    // Iterate through all Y values
                    foreach (double y in seriesPoint.YValues)
                    {
                        if (!double.IsNaN(y))
                        {
                            max = Math.Max(max, y);
                            min = Math.Min(min, y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets minimum stacked X value from many series
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum stacked X value</returns>
        public double GetMinStackedXValue(params string[] series)
        {
            double returnValue = 0;
            double numberOfPoints = GetNumberOfPoints(series);
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                double doubleIndexValue = 0;
                foreach (string seriesName in series)
                {
                    if (_series[seriesName].Points[pointIndex].XValue < 0)
                    {
                        doubleIndexValue += _series[seriesName].Points[pointIndex].XValue;
                    }
                }
                returnValue = Math.Min(returnValue, doubleIndexValue);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets minimum stacked Y value from many series
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum stacked Y value</returns>
        public double GetMinStackedYValue(int valueIndex, params string[] series)
        {
            double returnValue = Double.MaxValue;
            double numberOfPoints = GetNumberOfPoints(series);
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                double stackedMin = 0;
                double noStackedMin = 0;
                foreach (string seriesName in series)
                {
                    if (_series[seriesName].Points.Count > pointIndex)
                    {
                        // Take chart type from the series
                        ChartTypeRegistry chartTypeRegistry = Common.ChartTypeRegistry;
                        IChartType chartType = chartTypeRegistry.GetChartType(_series[seriesName].ChartTypeName);

                        // If stacked area
                        if (!chartType.StackSign || double.IsNaN(_series[seriesName].Points[pointIndex].YValues[valueIndex]))
                            continue;

                        if (chartType.Stacked)
                        {
                            if (_series[seriesName].Points[pointIndex].YValues[valueIndex] < 0)
                            {
                                stackedMin += _series[seriesName].Points[pointIndex].YValues[valueIndex];
                            }
                        }
                        else
                        {
                            noStackedMin = Math.Min(noStackedMin, _series[seriesName].Points[pointIndex].YValues[valueIndex]);
                        }
                    }
                }
                stackedMin = Math.Min(stackedMin, noStackedMin);
                if (stackedMin == 0)
                {
                    stackedMin = _series[series[0]].Points[^1].YValues[valueIndex];
                }
                returnValue = Math.Min(returnValue, stackedMin);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets minimum Unsigned stacked Y value from many series
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum stacked Y value</returns>
        public double GetMinUnsignedStackedYValue(int valueIndex, params string[] series)
        {
            double returnValue = double.MaxValue;
            double numberOfPoints = GetNumberOfPoints(series);
            for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
            {
                double stackedMin = 0;
                double noStackedMin = 0;
                double minValue = double.MaxValue;
                foreach (string seriesName in series)
                {
                    if (_series[seriesName].Points.Count > pointIndex)
                    {
                        // Take chart type from the series
                        ChartTypeRegistry chartTypeRegistry = Common.ChartTypeRegistry;
                        IChartType chartType = chartTypeRegistry.GetChartType(_series[seriesName].ChartTypeName);

                        // If stacked column and bar
                        if (chartType.StackSign || double.IsNaN(_series[seriesName].Points[pointIndex].YValues[valueIndex]))
                        {
                            continue;
                        }

                        if (chartType.Stacked)
                        {
                            if (_series[seriesName].Points[pointIndex].YValues[valueIndex] < 0)
                            {
                                stackedMin += _series[seriesName].Points[pointIndex].YValues[valueIndex];
                                if (stackedMin < minValue)
                                    minValue = stackedMin;
                            }
                        }
                        else
                        {
                            noStackedMin = Math.Min(noStackedMin, _series[seriesName].Points[pointIndex].YValues[valueIndex]);
                        }
                    }
                }
                minValue = Math.Min(noStackedMin, minValue);
                returnValue = Math.Min(returnValue, minValue);
            }
            return returnValue;
        }

        /// <summary>
        /// Gets minimum X value from many series
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum X value</returns>
        public double GetMinXValue(params string[] series)
        {
            double returnValue = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    returnValue = Math.Min(returnValue, seriesPoint.XValue);
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Get Minimum value for X and Radius Y2 ( used for bubble chart )
        /// </summary>
        /// <param name="area">Chart Area</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum X value</returns>
        public double GetMinXWithRadiusValue(ChartArea area, params string[] series)
        {
            double returnValue = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    if (!double.IsNaN(seriesPoint.XValue))
                    {
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.XValue - BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], false));
                        }
                        else
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.XValue);
                        }
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets minimum Y value from many series
        /// </summary>
        /// <param name="valueIndex">Index of Y value to use</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum Y value</returns>
        public double GetMinYValue(int valueIndex, params string[] series)
        {
            double returnValue = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    if (!double.IsNaN(seriesPoint.YValues[valueIndex]))
                    {
                        returnValue = Math.Min(returnValue, seriesPoint.YValues[valueIndex]);
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets minimum Y value from many series
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum Y value</returns>
        public double GetMinYValue(params string[] series)
        {
            double returnValue = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    foreach (double y in seriesPoint.YValues)
                    {
                        if (!double.IsNaN(y))
                        {
                            returnValue = Math.Min(returnValue, y);
                        }
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Get Minimum value for Y and and Radius (Y2) ( used for bubble chart )
        /// </summary>
        /// <param name="area">Chart Area</param>
        /// <param name="series">Series IDs</param>
        /// <returns>Minimum Y value</returns>
        public double GetMinYWithRadiusValue(ChartArea area, params string[] series)
        {
            double returnValue = Double.MaxValue;
            foreach (string seriesName in series)
            {
                foreach (DataPoint seriesPoint in _series[seriesName].Points)
                {
                    // The empty point
                    if (IsPointSkipped(seriesPoint))
                    {
                        continue;
                    }

                    if (!double.IsNaN(seriesPoint.YValues[0]))
                    {
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.YValues[0] - BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], true));
                        }
                        else
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.YValues[0]);
                        }
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets max number of data points in specified series.
        /// </summary>
        /// <param name="series">Series IDs</param>
        /// <returns>Maximum number of data points</returns>
        public int GetNumberOfPoints(params string[] series)
        {
            int numberOfPoints = 0;
            foreach (string seriesName in series)
            {
                numberOfPoints = Math.Max(numberOfPoints, _series[seriesName].Points.Count);
            }
            return numberOfPoints;
        }

        /// <summary>
        /// This method checks if data point should be skipped. This
        /// method will return true if data point is empty.
        /// </summary>
        /// <param name="point">Data point</param>
        /// <returns>This method returns true if data point is empty.</returns>
        private static bool IsPointSkipped(DataPoint point)
        {
            if (point.IsEmpty)
            {
                return true;
            }

            return false;
        }
        #endregion Series Min/Max values methods

        #region DataManager Properties

        // Array of custom palette colors.

        /// <summary>
        /// Color palette to use
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributePalette"),
        ]
        public ChartColorPalette Palette { get; set; } = ChartColorPalette.BrightPastel;

        /// <summary>
        /// Array of custom palette colors.
        /// </summary>
        /// <remarks>
        /// When this custom colors array is non-empty the <b>Palette</b> property is ignored.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeDataManager_PaletteCustomColors"),
        ]
        public SKColor[] PaletteCustomColors { set; get; } = Array.Empty<SKColor>();

        /// <summary>
        /// Chart series collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        ]
        public SeriesCollection Series
        {
            get
            {
                return _series;
            }
        }
        #endregion DataManager Properties

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_series != null)
                {
                    _series.Dispose();
                    _series = null;
                }
            }
        }

        #endregion IDisposable Members
    }
}