// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality
//              for the Column and RangeColumn charts.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace WebCharts.Services
{
    /// <summary>
    /// ColumnChart class provides 2D/3D drawing and hit testing
    /// functionality for the Column and RangeColumn charts. The
    /// only difference between the RangeColumn and Column chart
    /// is that 2 Y values are used to position top and bottom
    /// side of each RangeColumn column.
    /// </summary>
    internal class ColumnChart : PointChart
    {
        #region Fields

        /// <summary>
        /// Indicates that two Y values are used to calculate column position
        /// </summary>
        protected bool useTwoValues = false;

        /// <summary>
        /// Indicates that columns from different series are drawn side by side
        /// </summary>
        protected bool drawSeriesSideBySide = true;

        /// <summary>
        /// Coordinates of COP used when sorting 3D points order
        /// </summary>
        protected COPCoordinates coordinates = COPCoordinates.X;

        #endregion Fields

        #region IChartType interface implementation

        /// <summary>
        /// Chart type name
        /// </summary>
        override public string Name { get { return ChartTypeNames.Column; } }

        /// <summary>
        /// Gets chart type image.
        /// </summary>
        /// <param name="registry">Chart types registry object.</param>
        /// <returns>Chart type image.</returns>
        override public SKImage GetImage(ChartTypeRegistry registry)
        {
            return (SKImage)registry.ResourceManager.GetObject(Name + "ChartType");
        }

        /// <summary>
        /// True if chart type is stacked
        /// </summary>
        override public bool Stacked { get { return false; } }

        /// <summary>
        /// True if chart type supports axeses
        /// </summary>
        override public bool RequireAxes { get { return true; } }

        /// <summary>
        /// True if chart type supports logarithmic axes
        /// </summary>
        override public bool SupportLogarithmicAxes { get { return true; } }

        /// <summary>
        /// True if chart type requires to switch the value (Y) axes position
        /// </summary>
        override public bool SwitchValueAxes { get { return false; } }

        /// <summary>
        /// True if chart series can be placed side-by-side.
        /// </summary>
        override public bool SideBySideSeries { get { return true; } }

        /// <summary>
        /// True if each data point of a chart must be represented in the legend
        /// </summary>
        override public bool DataPointsInLegend { get { return false; } }

        /// <summary>
        /// Indicates that extra Y values are connected to the scale of the Y axis
        /// </summary>
        override public bool ExtraYValuesConnectedToYAxis { get { return false; } }

        /// <summary>
        /// True if palette colors should be applied for each data paoint.
        /// Otherwise the color is applied to the series.
        /// </summary>
        override public bool ApplyPaletteColorsToPoints { get { return false; } }

        /// <summary>
        /// How to draw series/points in legend:
        /// Filled rectangle, Line or Marker
        /// </summary>
        /// <param name="series">Legend item series.</param>
        /// <returns>Legend item style.</returns>
        override public LegendImageStyle GetLegendImageStyle(Series series)
        {
            return LegendImageStyle.Rectangle;
        }

        /// <summary>
        /// Number of supported Y value(s) per point
        /// </summary>
        override public int YValuesPerPoint { get { return 1; } }

        /// <summary>
        /// If the crossing value is auto Crossing value should be
        /// automatically set to zero for some chart
        /// types (Bar, column, area etc.)
        /// </summary>
        override public bool ZeroCrossing { get { return true; } }

        #endregion IChartType interface implementation

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ColumnChart() : base(false)
        {
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Labels and markers have to be shifted if there
        /// is more than one series for column chart.
        /// </summary>
        public override double ShiftedX { get; set; }

        /// <summary>
        /// Labels and markers have to be shifted if there
        /// is more than one series for column chart. This property
        /// will give a name of the series, which is used, for
        /// labels and markers.
        /// </summary>
        public override string ShiftedSerName { get; set; }

        #endregion Properties

        #region Painting and selection methods

        /// <summary>
        /// Paint Column Chart.
        /// </summary>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="seriesToDraw">Chart series to draw.</param>
        override public void Paint(
            ChartGraphics graph,
            CommonElements common,
            ChartArea area,
            Series seriesToDraw
            )
        {
            Common = common;
            // Draw columns
            ProcessChartType(false, false, graph, common, area, seriesToDraw);

            // Draw labels and markers
            ProcessChartType(true, false, graph, common, area, seriesToDraw);
        }

        /// <summary>
        /// This method recalculates size of the columns and paint them or do the hit test.
        /// This method is used from Paint or Select method.
        /// </summary>
        /// <param name="labels">Mode which draws only labels and markers.</param>
        /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="seriesToDraw">Chart series to draw.</param>
        private void ProcessChartType(
            bool labels,
            bool selection,
            ChartGraphics graph,
            CommonElements common,
            ChartArea area,
            Series seriesToDraw)
        {
            // Prosess 3D chart type
            if (area.Area3DStyle.Enable3D)
            {
                ProcessChartType3D(labels, selection, graph, common, area, seriesToDraw);
                return;
            }

            // Get pixel size
            SKSize pixelRelSize = graph.GetRelativeSize(new SKSize(1.1f, 1.1f));

            // All data series from chart area which have Column chart type
            List<string> typeSeries = area.GetSeriesFromChartType(Name);

            // Check if series should be drawn side by side
            bool currentDrawSeriesSideBySide = drawSeriesSideBySide;
            foreach (string seriesName in typeSeries)
            {
                if (common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
                {
                    string attribValue = common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
                    if (String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        currentDrawSeriesSideBySide = false;
                    }
                    else if (String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        currentDrawSeriesSideBySide = true;
                    }
                    else if (String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // Do nothing
                    }
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
                    }
                }
            }

            // Find the number of "Column chart" data series
            double numOfSeries = typeSeries.Count;
            if (!currentDrawSeriesSideBySide)
            {
                numOfSeries = 1;
            }

            // Check if column chart series are indexed
            bool indexedSeries = ChartHelper.IndexedSeries(Common, area.GetSeriesFromChartType(Name).ToArray());

            //************************************************************
            //** Loop through all series
            //************************************************************
            int seriesIndx = 0;
            foreach (Series ser in common.DataManager.Series)
            {
                // Process non empty series of the area with Column chart type
                if (String.Compare(ser.ChartTypeName, Name, true, System.Globalization.CultureInfo.CurrentCulture) != 0
                    || ser.ChartArea != area.Name || ser.Points.Count == 0 || !ser.IsVisible())
                {
                    continue;
                }

                // Set shifted series name property
                ShiftedSerName = ser.Name;

                // Set active vertical/horizontal axis
                Axis vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, Series.YSubAxisName);
                Axis hAxis = area.GetAxis(AxisName.X, ser.XAxisType, Series.XSubAxisName);
                double horizontalViewMax = hAxis.ViewMaximum;
                double horizontalViewMin = hAxis.ViewMinimum;
                double verticalViewMax = vAxis.ViewMaximum;
                double verticalViewMin = vAxis.ViewMinimum;
                double verticalAxisCrossing = vAxis.GetPosition(vAxis.Crossing);

                // Get points interval:
                //  - set interval to 1 for indexed series
                //  - if points are not equaly spaced, the minimum interval between points is selected.
                //  - if points have same interval bars do not overlap each other.
                bool sameInterval = false;
                double interval = 1;
                if (!indexedSeries)
                {
                    if (ser.Points.Count == 1 &&
                        (ser.XValueType == ChartValueType.Date ||
                         ser.XValueType == ChartValueType.DateTime ||
                         ser.XValueType == ChartValueType.Time ||
                         ser.XValueType == ChartValueType.DateTimeOffset))
                    {
                        // Check if interval is the same
                        area.GetPointsInterval(typeSeries, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out sameInterval);

                        // Special case when there is only one data point and date scale is used.
                        if (!double.IsNaN(hAxis.majorGrid.GetInterval()) && hAxis.majorGrid.GetIntervalType() != DateTimeIntervalType.NotSet)
                        {
                            interval = ChartHelper.GetIntervalSize(hAxis.minimum, hAxis.majorGrid.GetInterval(), hAxis.majorGrid.GetIntervalType());
                        }
                        else
                        {
                            interval = ChartHelper.GetIntervalSize(hAxis.minimum, hAxis.Interval, hAxis.IntervalType);
                        }
                    }
                    else
                    {
                        interval = area.GetPointsInterval(typeSeries, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out sameInterval);
                    }
                }

                // Get column width
                double width = ser.GetPointWidth(graph, hAxis, interval, 0.8) / numOfSeries;

                // Call Back Paint event
                if (!selection)
                {
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
                }

                //************************************************************
                //** Loop through all points in series
                //************************************************************
                int index = 0;
                foreach (DataPoint point in ser.Points)
                {
                    // Change Y value if Column is out of plot area
                    double yValue = vAxis.GetLogValue(GetYValue(common, area, ser, point, index, (useTwoValues) ? 1 : 0));

                    if (yValue > verticalViewMax)
                    {
                        yValue = verticalViewMax;
                    }
                    if (yValue < verticalViewMin)
                    {
                        yValue = verticalViewMin;
                    }

                    // Recalculates Height position and zero position of Columns
                    double height = vAxis.GetLinearPosition(yValue);

                    // Set start position for a column
                    double columnStartPosition = 0;
                    if (useTwoValues)
                    {
                        // Point Y value (first) is used to determine the column starting position
                        double yValueStart = vAxis.GetLogValue(GetYValue(common, area, ser, point, index, 0));
                        if (yValueStart > verticalViewMax)
                        {
                            yValueStart = verticalViewMax;
                        }
                        else if (yValueStart < verticalViewMin)
                        {
                            yValueStart = verticalViewMin;
                        }

                        columnStartPosition = vAxis.GetLinearPosition(yValueStart);
                    }
                    else
                    {
                        // Column starts on the horizontal axis crossing
                        columnStartPosition = verticalAxisCrossing;
                    }

                    // Increase point index
                    index++;

                    // Set x position
                    double xCenterVal;
                    double xPosition;
                    if (indexedSeries)
                    {
                        // The formula for position is based on a distance
                        //from the grid line or nPoints position.
                        xPosition = hAxis.GetPosition(index) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width;
                        xCenterVal = hAxis.GetPosition(index);
                    }
                    else if (sameInterval)
                    {
                        xPosition = hAxis.GetPosition(point.XValue) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width;
                        xCenterVal = hAxis.GetPosition(point.XValue);
                    }
                    else
                    {
                        xPosition = hAxis.GetPosition(point.XValue);
                        xCenterVal = hAxis.GetPosition(point.XValue);
                    }

                    // Labels and markers have to be shifted if there
                    // is more than one series for column chart.
                    ShiftedX = xPosition - xCenterVal;

                    // Make sure that points with small values are still visible
                    if (height < columnStartPosition &&
                        (columnStartPosition - height) < pixelRelSize.Height)
                    {
                        height = columnStartPosition - pixelRelSize.Height;
                    }
                    if (height > columnStartPosition &&
                        (height - columnStartPosition) < pixelRelSize.Height)
                    {
                        height = columnStartPosition + pixelRelSize.Height;
                    }

                    // Get column rectangle
                    SKRect rectSize = SKRect.Empty;
                    try
                    {
                        // Set the Column rectangle
                        rectSize.Left = (float)(xPosition - width / 2);
                        rectSize.Right = rectSize.Left + (float)width;

                        // The top side of rectangle has always
                        // smaller value than a bottom value
                        if (columnStartPosition < height)
                        {
                            rectSize.Top = (float)columnStartPosition;
                            rectSize.Bottom = (float)height;
                        }
                        else
                        {
                            rectSize.Top = (float)height;
                            rectSize.Bottom = (float)columnStartPosition;
                        }
                    }
                    catch (OverflowException)
                    {
                        continue;
                    }

                    // if data point is not empty
                    if (point.IsEmpty)
                    {
                        continue;
                    }

                    //************************************************************
                    // Painting mode
                    //************************************************************
                    if (common.ProcessModePaint)
                    {
                        if (!labels)
                        {
                            // Check if column is completly out of the data scaleView
                            double xValue = (indexedSeries) ? index : point.XValue;
                            xValue = hAxis.GetLogValue(xValue);
                            if (xValue < horizontalViewMin || xValue > horizontalViewMax)
                            {
                                continue;
                            }

                            // Check if column is partialy in the data scaleView
                            bool clipRegionSet = false;
                            if (rectSize.Left < area.PlotAreaPosition.X || rectSize.Right > area.PlotAreaPosition.Right)
                            {
                                // Set clipping region for line drawing
                                graph.SetClip(area.PlotAreaPosition.ToSKRect());
                                clipRegionSet = true;
                            }

                            // Draw the Column rectangle
                            DrawColumn2D(graph, vAxis, rectSize, point, ser);

                            // Reset Clip Region
                            if (clipRegionSet)
                            {
                                graph.ResetClip();
                            }
                        }
                        else if (useTwoValues)
                        {
                            // Draw labels and markers
                            DrawLabel(
                                area,
                                graph,
                                common,
                                rectSize,
                                point,
                                ser,
                                index);
                        }
                    }

                    //************************************************************
                    // Hot Regions mode used for image maps, tool tips and
                    // hit test function
                    //************************************************************
                    if (common.ProcessModeRegions && !labels)
                    {
                        common.HotRegionsList.AddHotRegion(rectSize, point, ser.Name, index - 1);
                    }
                }

                // Call Paint event
                if (!selection)
                {
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
                }

                // Data series index
                if (currentDrawSeriesSideBySide)
                {
                    seriesIndx++;
                }

                // Draw labels and markers using the base class algorithm
                if (labels && !useTwoValues)
                {
                    base.ProcessChartType(false, graph, common, area, seriesToDraw);
                }
            }
        }

        /// <summary>
        /// Draws 2D column.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="vAxis">Vertical axis.</param>
        /// <param name="rectSize">Column position and size.</param>
        /// <param name="point">Column data point.</param>
        /// <param name="ser">Column series.</param>
        protected virtual void DrawColumn2D(
            ChartGraphics graph,
            Axis vAxis,
            SKRect rectSize,
            DataPoint point,
            Series ser)
        {
            graph.FillRectangleRel(
                rectSize,
                point.Color,
                point.BackHatchStyle,
                point.BackImage,
                point.BackImageWrapMode,
                point.BackImageTransparentColor,
                point.BackImageAlignment,
                point.BackGradientStyle,
                point.BackSecondaryColor,
                point.BorderColor,
                point.BorderWidth,
                point.BorderDashStyle,
                ser.ShadowColor,
                ser.ShadowOffset,
                PenAlignment.Inset,
                ChartGraphics.GetBarDrawingStyle(point),
                true);
        }

        /// <summary>
        /// Gets label position for the column depending on the Y value.
        /// </summary>
        /// <returns>Return automaticly detected label position.</returns>
        /// <param name="series">Data series.</param>
        /// <param name="pointIndex">Point index.</param>
        /// <returns>Label aligning.</returns>
        override protected LabelAlignmentStyles GetAutoLabelPosition(Series series, int pointIndex)
        {
            if (series.Points[pointIndex].YValues[0] >= 0)
                return LabelAlignmentStyles.Top;
            else
                return LabelAlignmentStyles.Bottom;
        }

        /// <summary>
        /// Indicates that markers are drawnd on the X edge of the data scaleView.
        /// </summary>
        /// <returns>False. Column chart never draws markers on the edge.</returns>
        override protected bool ShouldDrawMarkerOnViewEdgeX()
        {
            return false;
        }

        #endregion Painting and selection methods

        #region 3D painting and selection methods

        /// <summary>
        /// This method recalculates size of the columns and paint them or do the hit test in 3d space.
        /// This method is used from Paint or Select method.
        /// </summary>
        /// <param name="labels">Mode which draws only labels and markers.</param>
        /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="seriesToDraw">Chart series to draw.</param>
        private void ProcessChartType3D(bool labels, bool selection, ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw)
        {
            // Labels & markers are drawn with the data points in the first iteration
            if (labels && !selection)
            {
                return;
            }

            // Get pixel size
            SKSize pixelRelSize = graph.GetRelativeSize(new SKSize(1.1f, 1.1f));

            // Get list of series to draw
            List<string> typeSeries;
            bool currentDrawSeriesSideBySide = drawSeriesSideBySide;
            if ((area.Area3DStyle.IsClustered && SideBySideSeries) ||
                Stacked)
            {
                // Draw all series of the same chart type
                typeSeries = area.GetSeriesFromChartType(Name);

                // Check if series should be drawn side by side
                foreach (string seriesName in typeSeries)
                {
                    if (common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
                    {
                        string attribValue = common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
                        if (String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            currentDrawSeriesSideBySide = false;
                        }
                        else if (String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            currentDrawSeriesSideBySide = true;
                        }
                        else if (String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // Do nothing
                        }
                        else
                        {
                            throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
                        }
                    }
                }
            }
            else
            {
                // Draw just one chart series
                typeSeries = new List<string>
                {
                    seriesToDraw.Name
                };
            }

            //************************************************************
            //** Get order of data points drawing
            //************************************************************
            ArrayList dataPointDrawingOrder = area.GetDataPointDrawingOrder(typeSeries, this, selection, coordinates, null, YValueIndex, currentDrawSeriesSideBySide);

            //************************************************************
            //** Loop through all data poins
            //************************************************************
            foreach (object obj in dataPointDrawingOrder)
            {
                // Get point & series
                DataPoint3D pointEx = (DataPoint3D)obj;
                DataPoint point = pointEx.dataPoint;
                Series ser = point.series;

                // Get point bar drawing style
                BarDrawingStyle barDrawingStyle = ChartGraphics.GetBarDrawingStyle(point);

                // Set active vertical/horizontal axis
                Axis vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, Series.YSubAxisName);
                Axis hAxis = area.GetAxis(AxisName.X, ser.XAxisType, Series.XSubAxisName);

                // Change Y value if Column is out of plot area
                float topDarkening = 0f;
                float bottomDarkening = 0f;
                double yValue = GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, (useTwoValues) ? 1 : 0);
                yValue = vAxis.GetLogValue(yValue);
                if (yValue > vAxis.ViewMaximum)
                {
                    topDarkening = 0.5f;
                    yValue = vAxis.ViewMaximum;
                }
                if (yValue < vAxis.ViewMinimum)
                {
                    topDarkening = 0.5f;
                    yValue = vAxis.ViewMinimum;
                }

                // Recalculates Height position and zero position of Columns
                double height = vAxis.GetLinearPosition(yValue);

                // Set start position for a column
                double columnStartPosition;
                if (useTwoValues)
                {
                    // Point Y value (first) is used to determine the column starting position
                    double yValueStart = vAxis.GetLogValue(GetYValue(common, area, ser, point, pointEx.index - 1, 0));
                    if (yValueStart > vAxis.ViewMaximum)
                    {
                        bottomDarkening = 0.5f;
                        yValueStart = vAxis.ViewMaximum;
                    }
                    else if (yValueStart < vAxis.ViewMinimum)
                    {
                        bottomDarkening = 0.5f;
                        yValueStart = vAxis.ViewMinimum;
                    }

                    columnStartPosition = vAxis.GetLinearPosition(yValueStart);
                }
                else
                {
                    // Column starts on the horizontal axis crossing
                    columnStartPosition = vAxis.GetPosition(vAxis.Crossing);
                }

                // Labels and markers have to be shifted if there
                // is more than one series for column chart.
                if (!currentDrawSeriesSideBySide)
                {
                    pointEx.xPosition = pointEx.xCenterVal;
                }
                ShiftedX = pointEx.xPosition - pointEx.xCenterVal;

                // Make sure that points with small values are still visible
                if (height < columnStartPosition &&
                    (columnStartPosition - height) < pixelRelSize.Height)
                {
                    height = columnStartPosition - pixelRelSize.Height;
                }
                if (height > columnStartPosition &&
                    (height - columnStartPosition) < pixelRelSize.Height)
                {
                    height = columnStartPosition + pixelRelSize.Height;
                }

                // Get column rectangle
                SKRect rectSize = SKRect.Empty;
                try
                {
                    // Set the Column rectangle
                    rectSize.Left = (float)(pointEx.xPosition - pointEx.width / 2);
                    rectSize.Right = rectSize.Left + (float)pointEx.width;

                    // The top side of rectangle has always
                    // smaller value than a bottom value
                    if (columnStartPosition < height)
                    {
                        float temp = bottomDarkening;
                        bottomDarkening = topDarkening;
                        topDarkening = temp;

                        rectSize.Left = (float)columnStartPosition;
                        rectSize.Bottom = (float)height;
                    }
                    else
                    {
                        rectSize.Top = (float)height;
                        rectSize.Bottom = (float)columnStartPosition;
                    }
                }
                catch (OverflowException)
                {
                    continue;
                }

                //************************************************************
                //** Painting mode
                //************************************************************
                // Path projection of 3D rect.
                SKPath rectPath;

                // Check if column is completly out of the data scaleView
                double xValue = (pointEx.indexedSeries) ? pointEx.index : point.XValue;
                xValue = hAxis.GetLogValue(xValue);
                if (xValue < hAxis.ViewMinimum || xValue > hAxis.ViewMaximum)
                {
                    continue;
                }

                // Check if column is partialy in the data scaleView
                bool clipRegionSet = false;
                if (rectSize.Right <= area.PlotAreaPosition.X || rectSize.Left >= area.PlotAreaPosition.Right)
                {
                    continue;
                }

                if (rectSize.Left < area.PlotAreaPosition.X)
                {
                    rectSize.Right -= area.PlotAreaPosition.X - rectSize.Left;
                    rectSize.Left = area.PlotAreaPosition.X;
                }
                if (rectSize.Right > area.PlotAreaPosition.Right)
                {
                    rectSize.Right -= rectSize.Right - area.PlotAreaPosition.Right;
                }
                if (rectSize.Width < 0)
                {
                    rectSize.Right = rectSize.Left;
                }

                // Detect if we need to get graphical path of drawn object
                DrawingOperationTypes drawingOperationType = DrawingOperationTypes.DrawElement;

                if (common.ProcessModeRegions)
                {
                    drawingOperationType |= DrawingOperationTypes.CalcElementPath;
                }

                if (!point.IsEmpty &&
                    rectSize.Height > 0f &&
                    rectSize.Width > 0f)
                {
                    rectPath = graph.Fill3DRectangle(
                        rectSize,
                        pointEx.zPosition,
                        pointEx.depth,
                        area.matrix3D,
                        area.Area3DStyle.LightStyle,
                        point.Color,
                        topDarkening,
                        bottomDarkening,
                        point.BorderColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        barDrawingStyle,
                        true,
                        drawingOperationType);

                    //************************************************************
                    // Hot Regions mode used for image maps, tool tips and
                    // hit test function
                    //************************************************************
                    if (common.ProcessModeRegions && !labels)
                    {
                        common.HotRegionsList.AddHotRegion(
                            rectPath,
                            false,
                            point,
                            ser.Name,
                            pointEx.index - 1
                            );
                    }
                    if (rectPath != null)
                    {
                        rectPath.Dispose();
                    }
                }

                // Reset Clip Region
                if (clipRegionSet)
                {
                    graph.ResetClip();
                }

                // Draw Labels & markers for each data point
                ProcessSinglePoint3D(
                    pointEx,
                    selection,
                    graph,
                    common,
                    area,
                    rectSize,
                    pointEx.index - 1
                    );
            }

            // Finish processing 3D labels
            DrawAccumulated3DLabels(graph, common, area);
        }

        #endregion 3D painting and selection methods

        #region 2D and 3D Labels Drawing

        /// <summary>
        /// This method draws label.
        /// </summary>
        /// <param name="graph">The Chart Graphics object</param>
        /// <param name="common">The Common elements object</param>
        /// <param name="area">Chart area for this chart</param>
        /// <param name="columnPosition">Column position</param>
        /// <param name="point">Data point</param>
        /// <param name="ser">Data series</param>
        /// <param name="pointIndex">Data point index.</param>
        protected virtual void DrawLabel(
            ChartArea area,
            ChartGraphics graph,
            CommonElements common,
            SKRect columnPosition,
            DataPoint point,
            Series ser,
            int pointIndex)
        {
            // Labels drawing functionality is inhereted from the PointChart class.
        }

        /// <summary>
        /// Draws\Hit tests single 3D point.
        /// </summary>
        /// <param name="pointEx">3D point information.</param>
        /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="columnPosition">Column position</param>
        /// <param name="pointIndex">Point index.</param>
        protected virtual void ProcessSinglePoint3D(
            DataPoint3D pointEx,
            bool selection,
            ChartGraphics graph,
            CommonElements common,
            ChartArea area,
            SKRect columnPosition,
            int pointIndex
            )
        {
            // Draw Labels & markers for each data point
            base.ProcessSinglePoint3D(
                pointEx,
                graph,
                common,
                area
                );
        }

        #endregion 2D and 3D Labels Drawing
    }

    /// <summary>
    /// ColumnChart class contains all the code necessary to draw
    /// both Column and RangeColumn charts. The RangeColumnChart class
    /// is used to override few default settings, so that 2 Y values
    /// will be used to define top and bottom position of each column.
    /// </summary>
    internal class RangeColumnChart : ColumnChart
    {
        #region Constructor

        /// <summary>
        /// Public constructor
        /// </summary>
        public RangeColumnChart()
        {
            // Set the flag to use two Y values, while drawing the columns
            useTwoValues = true;

            // Coordinates of COP used when sorting 3D points order
            coordinates = COPCoordinates.X | COPCoordinates.Y;

            // Index of the main Y value
            YValueIndex = 1;
        }

        #endregion Constructor

        #region IChartType interface implementation

        /// <summary>
        /// Chart type name
        /// </summary>
        override public string Name { get { return ChartTypeNames.RangeColumn; } }

        /// <summary>
        /// If the crossing value is auto Crossing value should be
        /// automatically set to zero for some chart
        /// types (Bar, column, area etc.)
        /// </summary>
        override public bool ZeroCrossing { get { return true; } }

        /// <summary>
        /// Number of supported Y value(s) per point
        /// </summary>
        override public int YValuesPerPoint { get { return 2; } }

        /// <summary>
        /// Indicates that extra Y values are connected to the scale of the Y axis
        /// </summary>
        override public bool ExtraYValuesConnectedToYAxis { get { return true; } }

        #endregion IChartType interface implementation

        #region Y values related methods

        /// <summary>
        /// Helper function, which returns the Y value of the point.
        /// </summary>
        /// <param name="common">Chart common elements.</param>
        /// <param name="area">Chart area the series belongs to.</param>
        /// <param name="series">Sereis of the point.</param>
        /// <param name="point">Point object.</param>
        /// <param name="pointIndex">Index of the point.</param>
        /// <param name="yValueIndex">Index of the Y value to get.</param>
        /// <returns>Y value of the point.</returns>
        override public double GetYValue(
            CommonElements common,
            ChartArea area,
            Series series,
            DataPoint point,
            int pointIndex,
            int yValueIndex)
        {
            // Calculate column height
            if (yValueIndex == -1)
            {
                return -(base.GetYValue(common, area, series, point, pointIndex, 1) -
                     base.GetYValue(common, area, series, point, pointIndex, 0));
            }

            return base.GetYValue(common, area, series, point, pointIndex, yValueIndex);
        }

        #endregion Y values related methods

        #region 2D and 3D Labels Drawing

        /// <summary>
        /// This method draws label.
        /// </summary>
        /// <param name="graph">The Chart Graphics object</param>
        /// <param name="common">The Common elements object</param>
        /// <param name="area">Chart area for this chart</param>
        /// <param name="columnPosition">Column position</param>
        /// <param name="point">Data point</param>
        /// <param name="ser">Data series</param>
        /// <param name="pointIndex">Data point index.</param>
        protected override void DrawLabel(
            ChartArea area,
            ChartGraphics graph,
            CommonElements common,
            SKRect columnPosition,
            DataPoint point,
            Series ser,
            int pointIndex)
        {
            //************************************************************
            //** Get marker position and size
            //************************************************************

            // Get intersection between column rectangle and plotting area rectangle
            SKRect intersection = SKRect.Intersect(
                columnPosition, area.PlotAreaPosition.ToSKRect());

            // If intersection is empty no drawing required
            if (intersection.Height <= 0f || intersection.Width <= 0f)
            {
                return;
            }

            // Get marker position
            SKPoint markerPosition = SKPoint.Empty;
            markerPosition.X = intersection.Left + intersection.Width / 2f;
            markerPosition.Y = intersection.Top;

            // Remeber pre-calculated point position
            point.positionRel = new SKPoint(markerPosition.X, markerPosition.Y);

            // Get point some point properties and save them in variables
            int pointMarkerSize = point.MarkerSize;
            string pointMarkerImage = point.MarkerImage;
            MarkerStyle pointMarkerStyle = point.MarkerStyle;

            // Get marker size
            SKSize markerSize = base.GetMarkerSize(
                graph,
                common,
                area,
                point,
                pointMarkerSize,
                pointMarkerImage);

            //************************************************************
            //** Draw point chart
            //************************************************************
            if (pointMarkerStyle != MarkerStyle.None ||
                pointMarkerImage.Length > 0)
            {
                // Draw the marker
                graph.DrawMarkerRel(markerPosition,
                    (pointMarkerStyle == MarkerStyle.None) ? MarkerStyle.Circle : pointMarkerStyle,
                    (int)markerSize.Height,
                    (point.MarkerColor == SKColor.Empty) ? point.Color : point.MarkerColor,
                    (point.MarkerBorderColor == SKColor.Empty) ? point.BorderColor : point.MarkerBorderColor,
                    GetMarkerBorderSize(point),
                    pointMarkerImage,
                    point.MarkerImageTransparentColor,
                    (point.series != null) ? point.series.ShadowOffset : 0,
                    (point.series != null) ? point.series.ShadowColor : SKColor.Empty,
                    new SKRect(markerPosition.X, markerPosition.Y, markerSize.Width, markerSize.Height));

                //************************************************************
                // Hot Regions mode used for image maps, tool tips and
                // hit test function
                //************************************************************
                if (common.ProcessModeRegions)
                {
                    // Get relative marker size
                    SKSize relativeMarkerSize = graph.GetRelativeSize(markerSize);

                    // Insert area just after the last custom area
                    int insertIndex = HotRegionsList.FindInsertIndex();

                    // Insert circle area
                    if (pointMarkerStyle == MarkerStyle.Circle)
                    {
                        float[] circCoord = new float[3];
                        circCoord[0] = markerPosition.X;
                        circCoord[1] = markerPosition.Y;
                        circCoord[2] = relativeMarkerSize.Width / 2f;

                        common.HotRegionsList.AddHotRegion(
                            insertIndex,
                            graph,
                            circCoord[0],
                            circCoord[1],
                            circCoord[2],
                            point,
                            ser.Name,
                            pointIndex - 1);
                    }
                    // All other markers represented as rectangles
                    else
                    {
                        common.HotRegionsList.AddHotRegion(
                            new SKRect(markerPosition.X - relativeMarkerSize.Width / 2f, markerPosition.Y - relativeMarkerSize.Height / 2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
                            point,
                            ser.Name,
                            pointIndex - 1);
                    }
                }
            }

            //************************************************************
            //** Draw LabelStyle
            //************************************************************

            // Label text format
            using StringFormat format = new();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // Disable the clip region
            SKRegion oldClipRegion = graph.Clip;
            graph.Clip = new();

            if (point.IsValueShownAsLabel || point.Label.Length > 0)
            {
                // Get label text
                string text;
                if (point.Label.Length == 0)
                {
                    // Round Y values for 100% stacked area
                    double pointLabelValue = GetYValue(common, area, ser, point, pointIndex, 0);

                    text = ValueConverter.FormatValue(
                        ser.Chart,
                        point,
                        point.Tag,
                        pointLabelValue,
                        point.LabelFormat,
                        ser.YValueType,
                        ChartElementType.DataPoint);
                }
                else
                {
                    text = point.ReplaceKeywords(point.Label);
                }

                // Calculate label position
                SKPoint labelPosition = SKPoint.Empty;
                labelPosition.X = intersection.Left + intersection.Width / 2f;
                labelPosition.Y = intersection.Top + intersection.Height / 2f;

                // Get string size
                SKSize SKSizeont = graph.GetRelativeSize(ChartGraphics.MeasureString(text, point.Font, new SKSize(1000f, 1000f), StringFormat.GenericTypographic));

                // Get label background position
                SKRect labelBackPosition = SKRect.Empty;
                SKSize sizeLabel = new(SKSizeont.Width, SKSizeont.Height);
                sizeLabel.Width += sizeLabel.Width / text.Length;
                sizeLabel.Height += SKSizeont.Height / 8;
                labelBackPosition = GetLabelPosition(
                    graph,
                    labelPosition,
                    sizeLabel,
                    format,
                    true);

                // Draw label text
                using SKPaint brush = new() { Color = point.LabelForeColor, Style = SKPaintStyle.Fill };
                graph.DrawPointLabelStringRel(
                    common,
                    text,
                    point.Font,
                    brush,
                    labelPosition,
                    format,
                    point.LabelAngle,
                    labelBackPosition,
                    point.LabelBackColor,
                    point.LabelBorderColor,
                    point.LabelBorderWidth,
                    point.LabelBorderDashStyle,
                    ser,
                    point,
                    pointIndex - 1);
            }

            // Restore old clip region
            graph.Clip = oldClipRegion;
        }

        /// <summary>
        /// Draws\Hit tests single 3D point.
        /// </summary>
        /// <param name="pointEx">3D point information.</param>
        /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="columnPosition">Column position</param>
        /// <param name="pointIndex">Point index.</param>
        protected override void ProcessSinglePoint3D(
            DataPoint3D pointEx,
            bool selection,
            ChartGraphics graph,
            CommonElements common,
            ChartArea area,
            SKRect columnPosition,
            int pointIndex
            )
        {
            DataPoint point = pointEx.dataPoint;

            // Check required Y values number
            if (point.YValues.Length < YValuesPerPoint)
            {
                throw (new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(Name, YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
            }

            // Label text format
            using StringFormat format = new();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // Disable the clip region
            SKRegion oldClipRegion = graph.Clip;
            graph.Clip = new();

            if (point.IsValueShownAsLabel || point.Label.Length > 0)
            {
                // Get label text
                string text;
                if (point.Label.Length == 0)
                {
                    // Get Y value
                    double pointLabelValue = GetYValue(common, area, pointEx.dataPoint.series, point, pointEx.index - 1, 0);
                    text = ValueConverter.FormatValue(
                        pointEx.dataPoint.series.Chart,
                        point,
                        point.Tag,
                        pointLabelValue,
                        point.LabelFormat,
                        pointEx.dataPoint.series.YValueType,
                        ChartElementType.DataPoint);
                }
                else
                {
                    text = point.ReplaceKeywords(point.Label);
                }

                // Calculate label position
                SKPoint labelPosition = SKPoint.Empty;
                labelPosition.X = columnPosition.Left + columnPosition.Width / 2f;
                labelPosition.Y = columnPosition.Top + columnPosition.Height / 2f;

                // Transform coordinates
                Point3D[] marker3DPosition = new Point3D[1];
                marker3DPosition[0] = new Point3D(labelPosition.X, labelPosition.Y, pointEx.zPosition + pointEx.depth);
                area.matrix3D.TransformPoints(marker3DPosition);

                labelPosition.X = marker3DPosition[0].X;
                labelPosition.Y = marker3DPosition[0].Y;

                // Get string size
                SKSize SKSizeont = graph.GetRelativeSize(ChartGraphics.MeasureString(text, point.Font, new SKSize(1000f, 1000f), StringFormat.GenericTypographic));

                // Get label background position
                SKRect labelBackPosition = SKRect.Empty;
                SKSize sizeLabel = new(SKSizeont.Width, SKSizeont.Height);
                sizeLabel.Width += sizeLabel.Width / text.Length;
                sizeLabel.Height += SKSizeont.Height / 8;
                labelBackPosition = GetLabelPosition(
                    graph,
                    labelPosition,
                    sizeLabel,
                    format,
                    true);

                // Draw label text
                using SKPaint brush = new() { Color = point.LabelForeColor, Style = SKPaintStyle.Fill };
                graph.DrawPointLabelStringRel(
                    common,
                    text,
                    point.Font,
                    brush,
                    labelPosition,
                    format,
                    point.LabelAngle,
                    labelBackPosition,
                    point.LabelBackColor,
                    point.LabelBorderColor,
                    point.LabelBorderWidth,
                    point.LabelBorderDashStyle,
                    point.series,
                    point,
                    pointIndex);
            }

            // Restore old clip region
            graph.Clip = oldClipRegion;
        }

        #endregion 2D and 3D Labels Drawing
    }
}