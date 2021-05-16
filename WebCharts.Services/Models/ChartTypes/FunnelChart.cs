// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:    Provides 2D/3D drawing and hit testing functionality 
//              for the Funnel and Pyramid charts. 
//				Funnel and Pyramid Chart types display data that 
//				equals 100% when totalled. This type of chart is a 
//				single series chart representing the data as portions 
//				of 100%, and this chart does not use any axes.
//


using SkiaSharp;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using WebCharts.Services.Enums;
using WebCharts.Services.Interfaces;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.General;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.ChartTypes
{
    #region Enumerations

    /// <summary>
    /// Value type of the pyramid chart.
    /// </summary>
    internal enum PyramidValueType
    {
        /// <summary>
        /// Each point value defines linear height of each segment.
        /// </summary>
        Linear,

        /// <summary>
        /// Each point value defines surface of each segment.
        /// </summary>
        Surface
    }

    /// <summary>
    /// Funnel chart drawing style.
    /// </summary>
    internal enum FunnelStyle
    {
        /// <summary>
        /// Shape of the funnel is fixed and point Y value controls the height of the segments.
        /// </summary>
        YIsHeight,

        /// <summary>
        /// Height of each segment is the same and point Y value controls the diameter of the segment.
        /// </summary>
        YIsWidth
    }

    /// <summary>
    /// Outside labels placement.
    /// </summary>
    internal enum FunnelLabelPlacement
    {
        /// <summary>
        /// Labels are placed on the right side of the funnel.
        /// </summary>
        Right,

        /// <summary>
        /// Labels are placed on the left side of the funnel.
        /// </summary>
        Left
    }

    /// <summary>
    /// Vertical alignment of the data point labels
    /// </summary>
    internal enum FunnelLabelVerticalAlignment
    {
        /// <summary>
        /// Label placed in the middle.
        /// </summary>
        Center,

        /// <summary>
        /// Label placed on top.
        /// </summary>
        Top,

        /// <summary>
        /// Label placed on the bottom.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Funnel chart 3D drawing style.
    /// </summary>
    internal enum Funnel3DDrawingStyle
    {
        /// <summary>
        /// Circle will be used as a shape of the base.
        /// </summary>
        CircularBase,

        /// <summary>
        /// Square will be used as a shape of the base.
        /// </summary>
        SquareBase
    }


    /// <summary>
    /// Funnel chart labels style enumeration.
    /// </summary>
    internal enum FunnelLabelStyle
    {
        /// <summary>
        /// Data point labels are located inside of the funnel.
        /// </summary>
        Inside,

        /// <summary>
        /// Data point labels are located outside of the funnel.
        /// </summary>
        Outside,

        /// <summary>
        /// Data point labels are located outside of the funnel in a column.
        /// </summary>
        OutsideInColumn,

        /// <summary>
        /// Data point labels are disabled.
        /// </summary>
        Disabled
    }

    #endregion // Enumerations

    /// <summary>
    /// FunnelChart class provides 2D/3D drawing and hit testing functionality 
    /// for the Funnel and Pyramid charts.
    /// </summary>
    internal class FunnelChart : IChartType
    {
        #region Fields and Constructor

        // Array list of funnel segments
        internal ArrayList segmentList = null;

        // List of data point labels information 
        internal ArrayList labelInfoList = null;

        // Chart graphics object.
        internal ChartGraphics Graph { get; set; }

        // Chart area the chart type belongs to.
        internal ChartArea Area { get; set; }

        // Common chart elements.
        internal CommonElements Common { get; set; }

        // Spacing between each side of the funnel and chart area.
        internal SKRect plotAreaSpacing = new(3f, 3f, 3f, 3f);

        // Current chart type series
        private Series _chartTypeSeries = null;

        // Sum of all Y values in the data series
        internal double yValueTotal = 0.0;

        // Maximum Y value in the data series
        private double _yValueMax = 0.0;

        // Sum of all X values in the data series
        private double _xValueTotal = 0.0;

        // Number of points in the series
        internal int pointNumber;

        // Funnel chart drawing style
        private FunnelStyle _funnelStyle = FunnelStyle.YIsHeight;

        // Define the shape of the funnel neck
        private SKSize _funnelNeckSize = new(50f, 30f);

        // Gap between funnel segments
        internal float funnelSegmentGap = 0f;

        // 3D funnel rotation angle
        private int _rotation3D = 5;

        // Indicates that rounded shape is used to draw 3D chart type instead of square
        internal bool round3DShape = true;

        // Indicates that Pyramid chart is rendered.
        internal bool isPyramid = false;

        // Minimum data point height
        private float _funnelMinPointHeight = 0f;

        // Name of the attribute that controls the height of the gap between the points
        internal string funnelPointGapAttributeName = CustomPropertyName.FunnelPointGap;

        // Name of the attribute that controls the 3D funnel rotation angle
        internal string funnelRotationAngleAttributeName = CustomPropertyName.Funnel3DRotationAngle;

        // Name of the attribute that controls the minimum height of the point
        protected string funnelPointMinHeight = CustomPropertyName.FunnelMinPointHeight;

        // Name of the attribute that controls the minimum height of the point
        internal string funnel3DDrawingStyleAttributeName = CustomPropertyName.Funnel3DDrawingStyle;

        // Name of the attribute that controls inside labels vertical alignment
        internal string funnelInsideLabelAlignmentAttributeName = CustomPropertyName.FunnelInsideLabelAlignment;

        // Name of the attribute that controls outside labels placement (Left vs. Right)
        protected string funnelOutsideLabelPlacementAttributeName = CustomPropertyName.FunnelOutsideLabelPlacement;

        // Name of the attribute that controls labels style
        internal string funnelLabelStyleAttributeName = CustomPropertyName.FunnelLabelStyle;

        // Array of data point value adjusments in percentage
        private double[] _valuePercentages = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FunnelChart()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the calculted plotting area of the chart 
        /// </summary>
        internal SKRect PlotAreaPosition { get; set; } = SKRect.Empty;

        #endregion // Properties

        #region IChartType interface implementation

        /// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name { get { return ChartTypeNames.Funnel; } }

        /// <summary>
        /// True if chart type is stacked
        /// </summary>
        virtual public bool Stacked { get { return false; } }


        /// <summary>
        /// True if stacked chart type supports groups
        /// </summary>
        virtual public bool SupportStackedGroups { get { return false; } }


        /// <summary>
        /// True if stacked chart type should draw separately positive and 
        /// negative data points ( Bar and column Stacked types ).
        /// </summary>
        public bool StackSign { get { return false; } }

        /// <summary>
        /// True if chart type supports axeses
        /// </summary>
        virtual public bool RequireAxes { get { return false; } }

        /// <summary>
        /// Chart type with two y values used for scale ( bubble chart type )
        /// </summary>
        virtual public bool SecondYScale { get { return false; } }

        /// <summary>
        /// True if chart type requires circular chart area.
        /// </summary>
        public bool CircularChartArea { get { return false; } }

        /// <summary>
        /// True if chart type supports logarithmic axes
        /// </summary>
        virtual public bool SupportLogarithmicAxes { get { return true; } }

        /// <summary>
        /// True if chart type requires to switch the value (Y) axes position
        /// </summary>
        virtual public bool SwitchValueAxes { get { return false; } }

        /// <summary>
        /// True if chart series can be placed side-by-side.
        /// </summary>
        virtual public bool SideBySideSeries { get { return false; } }

        /// <summary>
        /// True if each data point of a chart must be represented in the legend
        /// </summary>
        virtual public bool DataPointsInLegend { get { return true; } }

        /// <summary>
        /// If the crossing value is auto Crossing value should be 
        /// automatically set to zero for some chart 
        /// types (Bar, column, area etc.)
        /// </summary>
        virtual public bool ZeroCrossing { get { return false; } }

        /// <summary>
        /// True if palette colors should be applied for each data paoint.
        /// Otherwise the color is applied to the series.
        /// </summary>
        virtual public bool ApplyPaletteColorsToPoints { get { return true; } }

        /// <summary>
        /// Indicates that extra Y values are connected to the scale of the Y axis
        /// </summary>
        virtual public bool ExtraYValuesConnectedToYAxis { get { return false; } }

        /// <summary>
        /// Indicates that it's a hundredred percent chart.
        /// Axis scale from 0 to 100 percent should be used.
        /// </summary>
        virtual public bool HundredPercent { get { return false; } }

        /// <summary>
        /// Indicates that it's a hundredred percent chart.
        /// Axis scale from 0 to 100 percent should be used.
        /// </summary>
        virtual public bool HundredPercentSupportNegative { get { return false; } }

        /// <summary>
        /// How to draw series/points in legend:
        /// Filled rectangle, Line or Marker
        /// </summary>
        /// <param name="series">Legend item series.</param>
        /// <returns>Legend item style.</returns>
        virtual public LegendImageStyle GetLegendImageStyle(Series series)
        {
            return LegendImageStyle.Rectangle;
        }

        /// <summary>
        /// Number of supported Y value(s) per point 
        /// </summary>
        virtual public int YValuesPerPoint { get { return 1; } }

        /// <summary>
        /// Gets chart type image.
        /// </summary>
        /// <param name="registry">Chart types registry object.</param>
        /// <returns>Chart type image.</returns>
        virtual public SKImage GetImage(ChartTypeRegistry registry)
        {
            return (SKImage)registry.ResourceManager.GetObject(Name + "ChartType");
        }

        #endregion

        #region Painting

        /// <summary>
        /// Paint Funnel Chart.
        /// </summary>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="seriesToDraw">Chart series to draw.</param>
        virtual public void Paint(
            ChartGraphics graph,
            CommonElements common,
            ChartArea area,
            Series seriesToDraw)
        {
            // Reset fields
            _chartTypeSeries = null;
            _funnelMinPointHeight = 0f;

            // Save reference to the input parameters 
            Graph = graph;
            Common = common;
            Area = area;

            // Funnel chart like a Pie chart shows each data point as part of the whole (100%).
            // Calculate the sum of all Y and X values, which will be used to calculate point percentage.
            GetDataPointValuesStatistic();

            // Check if there are non-zero points 
            if (yValueTotal == 0.0 || pointNumber == 0)
            {
                return;
            }

            // When Y value is funnel width at least 2 points required
            _funnelStyle = GetFunnelStyle(GetDataSeries());
            if (_funnelStyle == FunnelStyle.YIsWidth &&
                pointNumber == 1)
            {
                // At least 2 points required
                return;
            }

            // Get minimum point height
            GetFunnelMinPointHeight(GetDataSeries());

            // Fill list of data point labels information
            labelInfoList = CreateLabelsInfoList();

            // Calculate the spacing required for the labels.
            GetPlotAreaSpacing();

            // Draw funnel
            ProcessChartType();

            // Draw data point labels
            DrawLabels();
        }

        /// <summary>
        /// Process chart type drawing.
        /// </summary>
        private void ProcessChartType()
        {
            // Reversed drawing order in 3D with positive rotation angle
            if (Area.Area3DStyle.Enable3D &&
                ((_rotation3D > 0 && !isPyramid) || (_rotation3D < 0 && isPyramid)))
            {
                segmentList.Reverse();
            }

            // Check if series shadow should be drawn separatly
            bool drawSegmentShadow = !Area.Area3DStyle.Enable3D;

            // Process all funnel segments shadows
            Series series = GetDataSeries();
            if (drawSegmentShadow &&
                series != null &&
                series.ShadowOffset != 0)
            {
                foreach (FunnelSegmentInfo segmentInfo in segmentList)
                {
                    // Draw funnel segment
                    DrawFunnelCircularSegment(
                        segmentInfo.Point,
                        segmentInfo.PointIndex,
                        segmentInfo.StartWidth,
                        segmentInfo.EndWidth,
                        segmentInfo.Location,
                        segmentInfo.Height,
                        segmentInfo.NothingOnTop,
                        segmentInfo.NothingOnBottom,
                        false,
                        true);
                }

                drawSegmentShadow = false;
            }

            // Process all funnel segments
            foreach (FunnelSegmentInfo segmentInfo in segmentList)
            {
                // Draw funnel segment
                DrawFunnelCircularSegment(
                    segmentInfo.Point,
                    segmentInfo.PointIndex,
                    segmentInfo.StartWidth,
                    segmentInfo.EndWidth,
                    segmentInfo.Location,
                    segmentInfo.Height,
                    segmentInfo.NothingOnTop,
                    segmentInfo.NothingOnBottom,
                    true,
                    drawSegmentShadow);
            }
        }

        /// <summary>
        /// Gets funnel data point segment height and width.
        /// </summary>
        /// <param name="series">Chart type series.</param>
        /// <param name="pointIndex">Data point index in the series.</param>
        /// <param name="location">Segment top location. Bottom location if reversed drawing order.</param>
        /// <param name="height">Returns the height of the segment.</param>
        /// <param name="startWidth">Returns top width of the segment.</param>
        /// <param name="endWidth">Returns botom width of the segment.</param>
        protected virtual void GetPointWidthAndHeight(
            Series series,
            int pointIndex,
            float location,
            out float height,
            out float startWidth,
            out float endWidth)
        {

            // Get plotting area position in pixels
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(PlotAreaPosition);

            // Calculate total height of plotting area minus reserved space for the gaps
            float plotAreaHeightAbs = plotAreaPositionAbs.Height -
                funnelSegmentGap * (pointNumber - (ShouldDrawFirstPoint() ? 1 : 2));
            if (plotAreaHeightAbs < 0f)
            {
                plotAreaHeightAbs = 0f;
            }

            SKPoint pointPositionAbs;
            if (_funnelStyle == FunnelStyle.YIsWidth)
            {
                // Check if X values are provided
                if (_xValueTotal == 0.0)
                {
                    // Calculate segment height in pixels by deviding 
                    // plotting area height by number of points.
                    height = plotAreaHeightAbs / (pointNumber - 1);
                }
                else
                {
                    // Calculate segment height as a part of total Y values in series
                    height = (float)(plotAreaHeightAbs * (GetXValue(series.Points[pointIndex]) / _xValueTotal));
                }

                // Check for minimum segment height
                height = CheckMinHeight(height);

                // Calculate start and end width of the segment based on Y value
                // of previous and current data point.
                startWidth = (float)(plotAreaPositionAbs.Width * (GetYValue(series.Points[pointIndex - 1], pointIndex - 1) / _yValueMax));
                endWidth = (float)(plotAreaPositionAbs.Width * (GetYValue(series.Points[pointIndex], pointIndex) / _yValueMax));

                // Set point position for annotation anchoring
                pointPositionAbs = new SKPoint(
                    plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f,
                    location + height);
            }
            else if (_funnelStyle == FunnelStyle.YIsHeight)
            {
                // Calculate segment height as a part of total Y values in series
                height = (float)(plotAreaHeightAbs * (GetYValue(series.Points[pointIndex], pointIndex) / yValueTotal));

                // Check for minimum segment height
                height = CheckMinHeight(height);

                // Get intersection point of the horizontal line at the start of the segment
                // with the left pre-defined wall of the funnel.
                SKPoint startIntersection = ChartGraphics.GetLinesIntersection(
                    plotAreaPositionAbs.Left, location,
                    plotAreaPositionAbs.Right, location,
                    plotAreaPositionAbs.Left, plotAreaPositionAbs.Top,
                    plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f - _funnelNeckSize.Width / 2f,
                    plotAreaPositionAbs.Bottom - _funnelNeckSize.Height);

                // Get intersection point of the horizontal line at the end of the segment
                // with the left pre-defined wall of the funnel.
                SKPoint endIntersection = ChartGraphics.GetLinesIntersection(
                    plotAreaPositionAbs.Left, location + height,
                    plotAreaPositionAbs.Right, location + height,
                    plotAreaPositionAbs.Left, plotAreaPositionAbs.Top,
                    plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f - _funnelNeckSize.Width / 2f,
                    plotAreaPositionAbs.Bottom - _funnelNeckSize.Height);

                // Get segment start and end width
                startWidth = (plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f -
                    startIntersection.X) * 2f;
                endWidth = (plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f -
                    endIntersection.X) * 2f;

                // Set point position for annotation anchoring
                pointPositionAbs = new SKPoint(
                    plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f,
                    location + height / 2f);
            }
            else
            {
                throw (new InvalidOperationException(SR.ExceptionFunnelStyleUnknown(_funnelStyle.ToString())));
            }

            // Set pre-calculated point position
            series.Points[pointIndex].positionRel = Graph.GetRelativePoint(pointPositionAbs);
        }

        /// <summary>
        /// Checks if first point in the series should be drawn.
        /// When point Y value is used to define the diameter of the funnel
        /// segment 2 points are required to draw 1 segment. In this case first
        /// data point is not drawn.
        /// </summary>
        /// <returns>True if first point in the series should be drawn.</returns>
        protected virtual bool ShouldDrawFirstPoint()
        {
            return (_funnelStyle == FunnelStyle.YIsHeight || isPyramid);
        }

        /// <summary>
        /// Draws funnel 3D square segment.
        /// </summary>
        /// <param name="point">Data point</param>
        /// <param name="pointIndex">Data point index.</param>
        /// <param name="startWidth">Segment top width.</param>
        /// <param name="endWidth">Segment bottom width.</param>
        /// <param name="location">Segment top location.</param>
        /// <param name="height">Segment height.</param>
        /// <param name="nothingOnTop">True if nothing is on the top of that segment.</param>
        /// <param name="nothingOnBottom">True if nothing is on the bottom of that segment.</param>
        /// <param name="drawSegment">True if segment shadow should be drawn.</param>
        /// <param name="drawSegmentShadow">True if segment shadow should be drawn.</param>
        private void DrawFunnel3DSquareSegment(
            DataPoint point,
            int pointIndex,
            float startWidth,
            float endWidth,
            float location,
            float height,
            bool nothingOnTop,
            bool nothingOnBottom,
            bool drawSegment,
            bool drawSegmentShadow)
        {
            // Increase the height of the segment to make sure there is no gaps between segments 
            if (!nothingOnBottom)
            {
                height += 0.3f;
            }

            // Get lighter and darker back colors
            SKColor lightColor = ChartGraphics.GetGradientColor(point.Color, SKColors.White, 0.3);
            SKColor darkColor = ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.3);

            // Segment width can't be smaller than funnel neck width
            if (_funnelStyle == FunnelStyle.YIsHeight && !isPyramid)
            {
                if (startWidth < _funnelNeckSize.Width)
                {
                    startWidth = _funnelNeckSize.Width;
                }
                if (endWidth < _funnelNeckSize.Width)
                {
                    endWidth = _funnelNeckSize.Width;
                }
            }

            // Get 3D rotation angle
            float topRotationHeight = (float)((startWidth / 2f) * Math.Sin(_rotation3D / 180F * Math.PI));
            float bottomRotationHeight = (float)((endWidth / 2f) * Math.Sin(_rotation3D / 180F * Math.PI));

            // Get plotting area position in pixels
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(PlotAreaPosition);

            // Get the horizontal center point in pixels
            float xCenterPointAbs = plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f;

            // Create segment path
            SKPath segmentPath = new();

            // Draw left part of the pyramid segment
            // Add top line
            if (startWidth > 0f)
            {
                segmentPath.AddLine(
                    xCenterPointAbs - startWidth / 2f, location,
                    xCenterPointAbs, location + topRotationHeight);
            }

            // Add middle line
            segmentPath.AddLine(
                xCenterPointAbs, location + topRotationHeight,
                xCenterPointAbs, location + height + bottomRotationHeight);

            // Add bottom line
            if (endWidth > 0f)
            {
                segmentPath.AddLine(
                    xCenterPointAbs, location + height + bottomRotationHeight,
                    xCenterPointAbs - endWidth / 2f, location + height);
            }

            // Add left line
            segmentPath.AddLine(
                xCenterPointAbs - endWidth / 2f, location + height,
                xCenterPointAbs - startWidth / 2f, location);

            if (Common.ProcessModePaint)
            {
                // Fill graphics path
                Graph.DrawPathAbs(
                    segmentPath,
                    (drawSegment) ? lightColor : SKColors.Transparent,
                    point.BackHatchStyle,
                    point.BackImage,
                    point.BackImageWrapMode,
                    point.BackImageTransparentColor,
                    point.BackImageAlignment,
                    point.BackGradientStyle,
                    (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                    (drawSegment) ? point.BorderColor : SKColors.Transparent,
                    point.BorderWidth,
                    point.BorderDashStyle,
                    PenAlignment.Center,
                    (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                    point.series.ShadowColor);
            }

            if (Common.ProcessModeRegions)
            {
                // Add hot region
                Common.HotRegionsList.AddHotRegion(
                    segmentPath,
                    false,
                    Graph,
                    point,
                    point.series.Name,
                    pointIndex);
            }
            segmentPath.Dispose();



            // Draw right part of the pyramid segment
            // Add top line
            segmentPath = new SKPath();
            if (startWidth > 0f)
            {
                segmentPath.AddLine(
                    xCenterPointAbs + startWidth / 2f, location,
                    xCenterPointAbs, location + topRotationHeight);
            }

            // Add middle line
            segmentPath.AddLine(
                xCenterPointAbs, location + topRotationHeight,
                xCenterPointAbs, location + height + bottomRotationHeight);

            // Add bottom line
            if (endWidth > 0f)
            {
                segmentPath.AddLine(
                    xCenterPointAbs, location + height + bottomRotationHeight,
                    xCenterPointAbs + endWidth / 2f, location + height);
            }

            // Add right line
            segmentPath.AddLine(
                xCenterPointAbs + endWidth / 2f, location + height,
                xCenterPointAbs + startWidth / 2f, location);

            if (Common.ProcessModePaint)
            {
                // Fill graphics path
                Graph.DrawPathAbs(
                    segmentPath,
                    (drawSegment) ? darkColor : SKColors.Transparent,
                    point.BackHatchStyle,
                    point.BackImage,
                    point.BackImageWrapMode,
                    point.BackImageTransparentColor,
                    point.BackImageAlignment,
                    point.BackGradientStyle,
                    (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                    (drawSegment) ? point.BorderColor : SKColors.Transparent,
                    point.BorderWidth,
                    point.BorderDashStyle,
                    PenAlignment.Center,
                    (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                    point.series.ShadowColor);
            }

            if (Common.ProcessModeRegions)
            {
                // Add hot region
                Common.HotRegionsList.AddHotRegion(
                    segmentPath,
                    false,
                    Graph,
                    point,
                    point.series.Name,
                    pointIndex);
            }
            segmentPath.Dispose();


            // Add top 3D surface
            if (_rotation3D > 0f && startWidth > 0f && nothingOnTop && Area.Area3DStyle.Enable3D)
            {
                SKPoint[] sidePoints = new SKPoint[4];
                sidePoints[0] = new SKPoint(xCenterPointAbs + startWidth / 2f, location);
                sidePoints[1] = new SKPoint(xCenterPointAbs, location + topRotationHeight);
                sidePoints[2] = new SKPoint(xCenterPointAbs - startWidth / 2f, location);
                sidePoints[3] = new SKPoint(xCenterPointAbs, location - topRotationHeight);
                SKPath topCurve = new();
                topCurve.AddLines(sidePoints);
                topCurve.Close();

                if (Common.ProcessModePaint)
                {
                    // Fill graphics path
                    Graph.DrawPathAbs(
                        topCurve,
                        (drawSegment) ? ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.4) : SKColors.Transparent,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                        (drawSegment) ? point.BorderColor : SKColors.Transparent,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        PenAlignment.Center,
                        (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                        point.series.ShadowColor);
                }

                if (Common.ProcessModeRegions)
                {
                    // Add hot region
                    Common.HotRegionsList.AddHotRegion(
                        topCurve,
                        false,
                        Graph,
                        point,
                        point.series.Name,
                        pointIndex);
                }
                topCurve.Dispose();
            }

            // Add bottom 3D surface
            if (_rotation3D < 0f && startWidth > 0f && nothingOnBottom && Area.Area3DStyle.Enable3D)
            {
                SKPoint[] sidePoints = new SKPoint[4];
                sidePoints[0] = new SKPoint(xCenterPointAbs + endWidth / 2f, location + height);
                sidePoints[1] = new SKPoint(xCenterPointAbs, location + height + bottomRotationHeight);
                sidePoints[2] = new SKPoint(xCenterPointAbs - endWidth / 2f, location + height);
                sidePoints[3] = new SKPoint(xCenterPointAbs, location + height - bottomRotationHeight);
                SKPath topCurve = new();
                topCurve.AddLines(sidePoints);
                topCurve.Close();

                if (Common.ProcessModePaint)
                {
                    // Fill graphics path
                    Graph.DrawPathAbs(
                        topCurve,
                        (drawSegment) ? ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.4) : SKColors.Transparent,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                        (drawSegment) ? point.BorderColor : SKColors.Transparent,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        PenAlignment.Center,
                        (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                        point.series.ShadowColor);
                }

                if (Common.ProcessModeRegions)
                {
                    // Add hot region
                    Common.HotRegionsList.AddHotRegion(
                        topCurve,
                        false,
                        Graph,
                        point,
                        point.series.Name,
                        pointIndex);
                }
                topCurve.Dispose();

            }
        }

        /// <summary>
        /// Draws funnel segment.
        /// </summary>
        /// <param name="point">Data point</param>
        /// <param name="pointIndex">Data point index.</param>
        /// <param name="startWidth">Segment top width.</param>
        /// <param name="endWidth">Segment bottom width.</param>
        /// <param name="location">Segment top location.</param>
        /// <param name="height">Segment height.</param>
        /// <param name="nothingOnTop">True if nothing is on the top of that segment.</param>
        /// <param name="nothingOnBottom">True if nothing is on the bottom of that segment.</param>
        /// <param name="drawSegment">True if segment shadow should be drawn.</param>
        /// <param name="drawSegmentShadow">True if segment shadow should be drawn.</param>
        private void DrawFunnelCircularSegment(
            DataPoint point,
            int pointIndex,
            float startWidth,
            float endWidth,
            float location,
            float height,
            bool nothingOnTop,
            bool nothingOnBottom,
            bool drawSegment,
            bool drawSegmentShadow)
        {
            // Check if square 3D segment should be drawn
            if (Area.Area3DStyle.Enable3D && !round3DShape)
            {
                DrawFunnel3DSquareSegment(
                    point,
                    pointIndex,
                    startWidth,
                    endWidth,
                    location,
                    height,
                    nothingOnTop,
                    nothingOnBottom,
                    drawSegment,
                    drawSegmentShadow);
                return;
            }

            // Increase the height of the segment to make sure there is no gaps between segments 
            if (!nothingOnBottom)
            {
                height += 0.3f;
            }

            // Segment width can't be smaller than funnel neck width
            float originalStartWidth = startWidth;
            float originalEndWidth = endWidth;
            if (_funnelStyle == FunnelStyle.YIsHeight && !isPyramid)
            {
                if (startWidth < _funnelNeckSize.Width)
                {
                    startWidth = _funnelNeckSize.Width;
                }
                if (endWidth < _funnelNeckSize.Width)
                {
                    endWidth = _funnelNeckSize.Width;
                }
            }

            // Get 3D rotation angle
            //float tension = 0.8f
            float topRotationHeight = (float)((startWidth / 2f) * Math.Sin(_rotation3D / 180F * Math.PI));
            float bottomRotationHeight = (float)((endWidth / 2f) * Math.Sin(_rotation3D / 180F * Math.PI));

            // Get plotting area position in pixels
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(PlotAreaPosition);

            // Get the horizontal center point in pixels
            float xCenterPointAbs = plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f;

            // Create segment path
            SKPath segmentPath = new();

            // Add top line
            if (startWidth > 0f)
            {
                if (Area.Area3DStyle.Enable3D)
                {
                    SKPoint[] sidePoints = new SKPoint[4];
                    sidePoints[0] = new SKPoint(xCenterPointAbs + startWidth / 2f, location);
                    sidePoints[1] = new SKPoint(xCenterPointAbs, location + topRotationHeight);
                    sidePoints[2] = new SKPoint(xCenterPointAbs - startWidth / 2f, location);
                    sidePoints[3] = new SKPoint(xCenterPointAbs, location - topRotationHeight);
                    SKPath topCurve = new();
                    topCurve.AddPath(SkiaSharpExtensions.CreateSpline(sidePoints));
                    topCurve.Reverse();

                    Graph.AddEllipseSegment(
                        segmentPath,
                        topCurve,
                        null,
                        true,
                        0f,
                        out _,
                        out _);
                }
                else
                {
                    segmentPath.AddLine(
                        xCenterPointAbs - startWidth / 2f, location,
                        xCenterPointAbs + startWidth / 2f, location);
                }
            }

            // Add right line
            if (_funnelStyle == FunnelStyle.YIsHeight &&
                !isPyramid &&
                startWidth > _funnelNeckSize.Width &&
                endWidth <= _funnelNeckSize.Width)
            {
                // Get intersection point of the vertical line at the neck border
                // with the left pre-defined wall of the funnel.
                SKPoint intersection = ChartGraphics.GetLinesIntersection(
                    xCenterPointAbs + _funnelNeckSize.Width / 2f, plotAreaPositionAbs.Top,
                    xCenterPointAbs + _funnelNeckSize.Width / 2f, plotAreaPositionAbs.Bottom,
                    xCenterPointAbs + originalStartWidth / 2f, location,
                    xCenterPointAbs + originalEndWidth / 2f, location + height);

                // Adjust intersection point with top of the neck
                intersection.Y = plotAreaPositionAbs.Bottom - _funnelNeckSize.Height;

                // Add two segment line
                segmentPath.AddLine(
                    xCenterPointAbs + startWidth / 2f, location,
                    intersection.X, intersection.Y);
                segmentPath.AddLine(
                    intersection.X, intersection.Y,
                    intersection.X, location + height);
            }
            else
            {
                // Add straight line
                segmentPath.AddLine(
                    xCenterPointAbs + startWidth / 2f, location,
                    xCenterPointAbs + endWidth / 2f, location + height);
            }

            // Add bottom line
            if (endWidth > 0f)
            {
                if (Area.Area3DStyle.Enable3D)
                {
                    SKPoint[] sidePoints = new SKPoint[4];
                    sidePoints[0] = new SKPoint(xCenterPointAbs + endWidth / 2f, location + height);
                    sidePoints[1] = new SKPoint(xCenterPointAbs, location + height + bottomRotationHeight);
                    sidePoints[2] = new SKPoint(xCenterPointAbs - endWidth / 2f, location + height);
                    sidePoints[3] = new SKPoint(xCenterPointAbs, location + height - bottomRotationHeight);
                    SKPath topCurve = new();
                    topCurve.AddPath(SkiaSharpExtensions.CreateSpline(sidePoints));
                    topCurve.Reverse();

                    using SKPath tmp = new();
                    Graph.AddEllipseSegment(
                        tmp,
                        topCurve,
                        null,
                        true,
                        0f,
                        out SKPoint leftSideLinePoint,
                        out SKPoint rightSideLinePoint);

                    tmp.Reverse();
                    if (tmp.PointCount > 0)
                    {
                        segmentPath.AddPath(tmp);
                    }
                }
                else
                {
                    segmentPath.AddLine(
                        xCenterPointAbs + endWidth / 2f, location + height,
                        xCenterPointAbs - endWidth / 2f, location + height);
                }
            }

            // Add left line
            if (_funnelStyle == FunnelStyle.YIsHeight &&
                !isPyramid &&
                startWidth > _funnelNeckSize.Width &&
                endWidth <= _funnelNeckSize.Width)
            {
                // Get intersection point of the horizontal line at the start of the segment
                // with the left pre-defined wall of the funnel.
                SKPoint intersection = ChartGraphics.GetLinesIntersection(
                    xCenterPointAbs - _funnelNeckSize.Width / 2f, plotAreaPositionAbs.Top,
                    xCenterPointAbs - _funnelNeckSize.Width / 2f, plotAreaPositionAbs.Bottom,
                    xCenterPointAbs - originalStartWidth / 2f, location,
                    xCenterPointAbs - originalEndWidth / 2f, location + height);

                // Adjust intersection point with top of the neck
                intersection.Y = plotAreaPositionAbs.Bottom - _funnelNeckSize.Height;

                // Add two segment line
                segmentPath.AddLine(
                    intersection.X, location + height,
                    intersection.X, intersection.Y);
                segmentPath.AddLine(
                    intersection.X, intersection.Y,
                    xCenterPointAbs - startWidth / 2f, location);
            }
            else
            {
                segmentPath.AddLine(
                    xCenterPointAbs - endWidth / 2f, location + height,
                    xCenterPointAbs - startWidth / 2f, location);
            }

            if (Common.ProcessModePaint)
            {
                // Draw lightStyle source blink effect in 3D
                if (Area.Area3DStyle.Enable3D &&
                    Graph.ActiveRenderingType == RenderingType.Gdi)
                {
                    // Get lighter and darker back colors
                    SKColor lightColor = ChartGraphics.GetGradientColor(point.Color, SKColors.White, 0.3);
                    SKColor darkColor = ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.3);

                    // Create linear gradient brush
                    SKRect boundsRect = segmentPath.GetBounds();
                    if (boundsRect.Width == 0f)
                    {
                        boundsRect.Right = boundsRect.Left + 1f;
                    }
                    if (boundsRect.Height == 0f)
                    {
                        boundsRect.Bottom = boundsRect.Top + 1f;
                    }
                    using SKPaint brush = new()
                    {
                        Style = SKPaintStyle.Fill,
                        Shader = SKShader.CreateLinearGradient(
                            boundsRect.Location, boundsRect.Location + boundsRect.Size,
                            new SKColor[] { darkColor, darkColor, lightColor, darkColor, darkColor },
                            new float[] { 0.0f, 0.0f, 0.5f, 1.0f, 1.0f }, SKShaderTileMode.Clamp
                            )
                    };

                    // Fill path
                    Graph.Graphics.DrawPath(segmentPath, brush);

                    // Draw path border
                    SKPaint pen = new() { Style = SKPaintStyle.Stroke, Color = point.BorderColor, StrokeWidth = point.BorderWidth };
                    pen.PathEffect = ChartGraphics.GetPenStyle(point.BorderDashStyle, point.BorderWidth);
                    if (point.BorderWidth == 0 ||
                        point.BorderDashStyle == ChartDashStyle.NotSet ||
                        point.BorderColor == SKColor.Empty)
                    {
                        // Draw line of the darker color inside the cylinder
                        pen.Color = ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.3);
                        pen.StrokeWidth = 1;
                    }
                    pen.StrokeCap = SKStrokeCap.Round;
                    pen.StrokeJoin = SKStrokeJoin.Bevel;
                    Graph.DrawPath(pen, segmentPath);
                    pen.Dispose();
                }
                else
                {
                    // Fill graphics path
                    Graph.DrawPathAbs(
                        segmentPath,
                        (drawSegment) ? point.Color : SKColors.Transparent,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                        (drawSegment) ? point.BorderColor : SKColors.Transparent,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        PenAlignment.Center,
                        (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                        point.series.ShadowColor);
                }
            }

            if (Common.ProcessModeRegions)
            {
                // Add hot region
                Common.HotRegionsList.AddHotRegion(
                    segmentPath,
                    false,
                    Graph,
                    point,
                    point.series.Name,
                    pointIndex);
            }
            segmentPath.Dispose();


            // Add top 3D surface
            if (_rotation3D > 0f && startWidth > 0f && nothingOnTop && Area.Area3DStyle.Enable3D)
            {
                SKPoint[] sidePoints = new SKPoint[4];
                sidePoints[0] = new SKPoint(xCenterPointAbs + startWidth / 2f, location);
                sidePoints[1] = new SKPoint(xCenterPointAbs, location + topRotationHeight);
                sidePoints[2] = new SKPoint(xCenterPointAbs - startWidth / 2f, location);
                sidePoints[3] = new SKPoint(xCenterPointAbs, location - topRotationHeight);
                SKPath topCurve = new();
                topCurve.AddPoly(sidePoints, true); // tension ?

                if (Common.ProcessModePaint)
                {
                    // Fill graphics path
                    Graph.DrawPathAbs(
                        topCurve,
                        (drawSegment) ? ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.4) : SKColors.Transparent,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                        (drawSegment) ? point.BorderColor : SKColors.Transparent,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        PenAlignment.Center,
                        (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                        point.series.ShadowColor);
                }

                if (Common.ProcessModeRegions)
                {
                    // Add hot region
                    Common.HotRegionsList.AddHotRegion(
                        topCurve,
                        false,
                        Graph,
                        point,
                        point.series.Name,
                        pointIndex);
                }
                topCurve.Dispose();
            }

            // Add bottom 3D surface
            if (_rotation3D < 0f && startWidth > 0f && nothingOnBottom && Area.Area3DStyle.Enable3D)
            {
                SKPoint[] sidePoints = new SKPoint[4];
                sidePoints[0] = new SKPoint(xCenterPointAbs + endWidth / 2f, location + height);
                sidePoints[1] = new SKPoint(xCenterPointAbs, location + height + bottomRotationHeight);
                sidePoints[2] = new SKPoint(xCenterPointAbs - endWidth / 2f, location + height);
                sidePoints[3] = new SKPoint(xCenterPointAbs, location + height - bottomRotationHeight);
                SKPath topCurve = new();
                topCurve.AddPoly(sidePoints, true); // tension?

                if (Common.ProcessModePaint)
                {
                    // Fill graphics path
                    Graph.DrawPathAbs(
                        topCurve,
                        (drawSegment) ? ChartGraphics.GetGradientColor(point.Color, SKColors.Black, 0.4) : SKColors.Transparent,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        (drawSegment) ? point.BackSecondaryColor : SKColors.Transparent,
                        (drawSegment) ? point.BorderColor : SKColors.Transparent,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        PenAlignment.Center,
                        (drawSegmentShadow) ? point.series.ShadowOffset : 0,
                        point.series.ShadowColor);
                }

                if (Common.ProcessModeRegions)
                {
                    // Add hot region
                    Common.HotRegionsList.AddHotRegion(
                        topCurve,
                        false,
                        Graph,
                        point,
                        point.series.Name,
                        pointIndex);
                }
                topCurve.Dispose();
            }
        }


        /// <summary>
        /// Fill list with information about every segment of the funnel.
        /// </summary>
        /// <returns>Funnel segment information list.</returns>
        private ArrayList GetFunnelSegmentPositions()
        {
            // Create new list
            ArrayList list = new();

            // Funnel chart process only first series in the chart area
            // and cannot be combined with any other chart types.
            Series series = GetDataSeries();
            if (series != null)
            {
                // Get funnel drawing style 
                _funnelStyle = GetFunnelStyle(series);

                // Check if round or square base is used in 3D chart
                round3DShape = (GetFunnel3DDrawingStyle(series) == Funnel3DDrawingStyle.CircularBase);

                // Get funnel points gap
                funnelSegmentGap = GetFunnelPointGap(series);

                // Get funnel neck size
                _funnelNeckSize = GetFunnelNeckSize(series);

                // Loop through all ponts in the data series
                float currentLocation = Graph.GetAbsolutePoint(PlotAreaPosition.Location).Y;
                if (isPyramid)
                {
                    // Pyramid is drawn in reversed order. 
                    currentLocation = Graph.GetAbsoluteRectangle(PlotAreaPosition).Bottom;
                }
                for (int pointIndex = 0; pointIndex >= 0 && pointIndex < series.Points.Count; pointIndex += 1)
                {
                    DataPoint point = series.Points[pointIndex];

                    // Check if first data point should be drawn
                    if (pointIndex > 0 || ShouldDrawFirstPoint())
                    {
                        // Get height and width of each data point segment
                        GetPointWidthAndHeight(
                            series,
                            pointIndex,
                            currentLocation,
                            out float height,
                            out float startWidth,
                            out float endWidth);

                        // Check visibility of previous and next points
                        bool nothingOnTop = false;
                        bool nothingOnBottom = false;
                        if (funnelSegmentGap > 0)
                        {
                            nothingOnTop = true;
                            nothingOnBottom = true;
                        }
                        else
                        {
                            if (ShouldDrawFirstPoint())
                            {
                                if (pointIndex == 0 ||
                                    series.Points[pointIndex - 1].Color.Alpha != 255)
                                {
                                    if (isPyramid)
                                    {
                                        nothingOnBottom = true;
                                    }
                                    else
                                    {
                                        nothingOnTop = true;
                                    }
                                }
                            }
                            else
                            {
                                if (pointIndex == 1 ||
                                    series.Points[pointIndex - 1].Color.Alpha != 255)
                                {
                                    if (isPyramid)
                                    {
                                        nothingOnBottom = true;
                                    }
                                    else
                                    {
                                        nothingOnTop = true;
                                    }
                                }
                            }
                            if (pointIndex == series.Points.Count - 1)
                            {
                                if (isPyramid)
                                {
                                    nothingOnTop = true;
                                }
                                else
                                {
                                    nothingOnBottom = true;
                                }
                            }
                            else if (series.Points[pointIndex + 1].Color.Alpha != 255)
                            {
                                if (isPyramid)
                                {
                                    nothingOnTop = true;
                                }
                                else
                                {
                                    nothingOnBottom = true;
                                }
                            }
                        }

                        // Add segment information
                        FunnelSegmentInfo info = new();
                        info.Point = point;
                        info.PointIndex = pointIndex;
                        info.StartWidth = startWidth;
                        info.EndWidth = endWidth;
                        info.Location = (isPyramid) ? currentLocation - height : currentLocation;
                        info.Height = height;
                        info.NothingOnTop = nothingOnTop;
                        info.NothingOnBottom = nothingOnBottom;
                        list.Add(info);

                        // Increase current Y location 
                        if (isPyramid)
                        {
                            currentLocation -= height + funnelSegmentGap;
                        }
                        else
                        {
                            currentLocation += height + funnelSegmentGap;
                        }
                    }
                }
            }

            return list;
        }

        #endregion

        #region Labels Methods

        /// <summary>
        /// Draws funnel data point labels.
        /// </summary>
        private void DrawLabels()
        {
            // Loop through all labels
            foreach (FunnelPointLabelInfo labelInfo in labelInfoList)
            {
                if (!labelInfo.Position.IsEmpty &&
                    !float.IsNaN(labelInfo.Position.Left) &&
                    !float.IsNaN(labelInfo.Position.Top) &&
                    !float.IsNaN(labelInfo.Position.Width) &&
                    !float.IsNaN(labelInfo.Position.Height))
                {
                    // Get size of a single character used for spacing
                    SKSize spacing = Graph.MeasureString(
                        "W",
                        labelInfo.Point.Font,
                        new SKSize(1000f, 1000F),
                        StringFormat.GenericTypographic);

                    // Draw a callout line
                    if (!labelInfo.CalloutPoint1.IsEmpty &&
                        !labelInfo.CalloutPoint2.IsEmpty &&
                        !float.IsNaN(labelInfo.CalloutPoint1.X) &&
                        !float.IsNaN(labelInfo.CalloutPoint1.Y) &&
                        !float.IsNaN(labelInfo.CalloutPoint2.X) &&
                        !float.IsNaN(labelInfo.CalloutPoint2.Y))
                    {
                        // Add spacing between text and callout line
                        if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                        {
                            labelInfo.CalloutPoint2.X -= spacing.Width / 2f;

                            // Add a small spacing between a callout line and a segment
                            labelInfo.CalloutPoint1.X += 2;
                        }
                        else
                        {
                            labelInfo.CalloutPoint2.X += spacing.Width / 2f;

                            // Add a small spacing between a callout line and a segment
                            labelInfo.CalloutPoint1.X += 2;
                        }

                        // Get callout line color
                        SKColor lineColor = GetCalloutLineColor(labelInfo.Point);

                        // Draw callout line
                        Graph.DrawLineAbs(
                            lineColor,
                            1,
                            ChartDashStyle.Solid,
                            labelInfo.CalloutPoint1,
                            labelInfo.CalloutPoint2);

                    }

                    // Get label background position
                    SKRect labelBackPosition = labelInfo.Position;
                    labelBackPosition.Inflate(spacing.Width / 2f, spacing.Height / 8f);
                    labelBackPosition = Graph.GetRelativeRectangle(labelBackPosition);

                    // Center label in the middle of the background rectangle
                    using StringFormat format = new();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    // Draw label text
                    using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = labelInfo.Point.LabelForeColor };
                    Graph.DrawPointLabelStringRel(
                        Common,
                        labelInfo.Text,
                        labelInfo.Point.Font,
                        brush,
                        labelBackPosition,
                        format,
                        labelInfo.Point.LabelAngle,
                        labelBackPosition,

                        labelInfo.Point.LabelBackColor,
                        labelInfo.Point.LabelBorderColor,
                        labelInfo.Point.LabelBorderWidth,
                        labelInfo.Point.LabelBorderDashStyle,
                        labelInfo.Point.series,
                        labelInfo.Point,
                        labelInfo.PointIndex);
                }
            }
        }

        /// <summary>
        /// Creates a list of structures with the data point labels information.
        /// </summary>
        /// <returns>Array list of labels information.</returns>
        private ArrayList CreateLabelsInfoList()
        {
            ArrayList list = new();

            // Get area position in pixels
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(Area.Position.ToSKRect());

            // Get funnel chart type series
            Series series = GetDataSeries();
            if (series != null)
            {
                // Loop through all ponts in the data series
                int pointIndex = 0;
                foreach (DataPoint point in series.Points)
                {
                    // Ignore empty points
                    if (!point.IsEmpty)
                    {
                        // Get some properties for performance
                        string pointLabel = point.Label;
                        bool pointShowLabelAsValue = point.IsValueShownAsLabel;

                        // Check if label text exists
                        if (pointShowLabelAsValue || pointLabel.Length > 0)
                        {
                            // Create new point label information class
                            FunnelPointLabelInfo labelInfo = new();
                            labelInfo.Point = point;
                            labelInfo.PointIndex = pointIndex;

                            // Get point label text
                            if (pointLabel.Length == 0)
                            {
                                labelInfo.Text = ValueConverter.FormatValue(
                                    point.series.Chart,
                                    point,
                                    point.Tag,
                                    point.YValues[0],
                                    point.LabelFormat,
                                    point.series.YValueType,
                                    ChartElementType.DataPoint);
                            }
                            else
                            {
                                labelInfo.Text = point.ReplaceKeywords(pointLabel);
                            }

                            // Get label style
                            labelInfo.Style = GetLabelStyle(point);

                            // Get inside label vertical alignment
                            if (labelInfo.Style == FunnelLabelStyle.Inside)
                            {
                                labelInfo.VerticalAlignment = GetInsideLabelAlignment(point);
                            }

                            // Get outside labels placement
                            if (labelInfo.Style != FunnelLabelStyle.Inside)
                            {
                                labelInfo.OutsidePlacement = GetOutsideLabelPlacement(point);
                            }

                            // Measure string size
                            labelInfo.Size = Graph.MeasureString(
                                labelInfo.Text,
                                point.Font,
                                plotAreaPositionAbs.Size,
                                StringFormat.GenericTypographic);

                            // Add label information into the list
                            if (labelInfo.Text.Length > 0 &&
                                labelInfo.Style != FunnelLabelStyle.Disabled)
                            {
                                list.Add(labelInfo);
                            }
                        }
                    }
                    ++pointIndex;
                }
            }
            return list;
        }

        /// <summary>
        /// Changes required plotting area spacing, so that all labels fit.
        /// </summary>
        /// <returns>Return True if no resizing required.</returns>
        private bool FitPointLabels()
        {
            // Convert plotting area position to pixels.
            // Make rectangle 4 pixels smaller on each side.
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(PlotAreaPosition);
            plotAreaPositionAbs.Inflate(-4f, -4f);

            // Get position of each label
            GetLabelsPosition();

            // Get spacing required to draw labels
            SKRect requiredSpacing = Graph.GetAbsoluteRectangle(new SKRect(1f, 1f, 1f, 1f));
            foreach (FunnelPointLabelInfo labelInfo in labelInfoList)
            {
                // Add additional horizontal spacing for outside labels
                SKRect position = labelInfo.Position;
                if (labelInfo.Style == FunnelLabelStyle.Outside ||
                    labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
                {
                    float spacing = 10f;
                    if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                    {
                        position.Right += spacing;
                    }
                    else if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Left)
                    {
                        position.Left -= spacing;
                        position.Right += spacing;
                    }
                }

                // Horizontal coordinates are ignored for Inside label style
                if (labelInfo.Style != FunnelLabelStyle.Inside)
                {
                    if ((plotAreaPositionAbs.Left - position.Left) > requiredSpacing.Left)
                    {
                        requiredSpacing.Left = plotAreaPositionAbs.Left - position.Left;
                    }

                    if ((position.Right - plotAreaPositionAbs.Right) > requiredSpacing.Width)
                    {
                        requiredSpacing.Size = new(position.Right - plotAreaPositionAbs.Right, requiredSpacing.Height);
                    }
                }

                // Vertical spacing
                if ((plotAreaPositionAbs.Top - position.Top) > requiredSpacing.Top)
                {
                    requiredSpacing.Top = plotAreaPositionAbs.Top - position.Top;
                }

                if ((position.Bottom - plotAreaPositionAbs.Bottom) > requiredSpacing.Height)
                {
                    requiredSpacing.Size = new(requiredSpacing.Width, position.Bottom - plotAreaPositionAbs.Bottom);
                }
            }

            // Convert spacing rectangle to relative coordinates
            requiredSpacing = Graph.GetRelativeRectangle(requiredSpacing);

            // Check if non-default spacing was used
            if (requiredSpacing.Left > 1f ||
                requiredSpacing.Top > 1f ||
                requiredSpacing.Width > 1f ||
                requiredSpacing.Height > 1f)
            {
                plotAreaSpacing = requiredSpacing;

                // Get NEW plotting area position
                PlotAreaPosition = GetPlotAreaPosition();

                // Get NEW list of segments
                segmentList = GetFunnelSegmentPositions();

                // Get NEW position of each label
                GetLabelsPosition();

                return false;
            }

            return true;
        }

        /// <summary>
        /// Loops through the point labels list and calculates labels position
        /// based on their size, position and funnel chart shape.
        /// </summary>
        private void GetLabelsPosition()
        {
            // Convert plotting area position to pixels
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(PlotAreaPosition);
            float plotAreaCenterXAbs = plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f;

            // Define label spacing
            SKSize labelSpacing = new(3f, 3f);

            //Loop through all labels
            foreach (FunnelPointLabelInfo labelInfo in labelInfoList)
            {
                // Get assosiated funnel segment information
                bool lastLabel = false;
                int pointIndex = labelInfo.PointIndex + ((ShouldDrawFirstPoint()) ? 0 : 1);
                if (pointIndex > segmentList.Count && !ShouldDrawFirstPoint())
                {
                    // Use last point index if first point is not drawn
                    pointIndex = segmentList.Count;
                    lastLabel = true;
                }
                FunnelSegmentInfo segmentInfo = null;
                foreach (FunnelSegmentInfo info in segmentList)
                {
                    if (info.PointIndex == pointIndex)
                    {
                        segmentInfo = info;
                        break;
                    }
                }

                // Check if segment was found
                if (segmentInfo != null)
                {
                    // Set label width and height
                    labelInfo.Position.Size = new(labelInfo.Size.Width, labelInfo.Size.Height);

                    //******************************************************
                    //** Labels are placed OUTSIDE of the funnel
                    //******************************************************
                    if (labelInfo.Style == FunnelLabelStyle.Outside ||
                        labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
                    {
                        // Define position
                        if (_funnelStyle == FunnelStyle.YIsHeight)
                        {
                            // Get segment top and bottom diameter
                            float topDiameter = segmentInfo.StartWidth;
                            float bottomDiameter = segmentInfo.EndWidth;
                            if (!isPyramid)
                            {
                                if (topDiameter < _funnelNeckSize.Width)
                                {
                                    topDiameter = _funnelNeckSize.Width;
                                }
                                if (bottomDiameter < _funnelNeckSize.Width)
                                {
                                    bottomDiameter = _funnelNeckSize.Width;
                                }

                                // Adjust label position because segment is bent to make a neck
                                if (segmentInfo.StartWidth >= _funnelNeckSize.Width &&
                                    segmentInfo.EndWidth < _funnelNeckSize.Width)
                                {
                                    bottomDiameter = segmentInfo.EndWidth;
                                }
                            }

                            // Get Y position
                            labelInfo.Position.Top = (segmentInfo.Location + segmentInfo.Height / 2f) -
                                labelInfo.Size.Height / 2f;

                            // Get X position
                            if (labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
                            {
                                if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                                {
                                    labelInfo.Position.Left = plotAreaPositionAbs.Right +
                                        4f * labelSpacing.Width;

                                    // Set callout line coordinates
                                    if (!isPyramid)
                                    {
                                        labelInfo.CalloutPoint1.X = plotAreaCenterXAbs +
                                            Math.Max(_funnelNeckSize.Width / 2f, (topDiameter + bottomDiameter) / 4f);
                                    }
                                    else
                                    {
                                        labelInfo.CalloutPoint1.X = plotAreaCenterXAbs +
                                            (topDiameter + bottomDiameter) / 4f;
                                    }
                                    labelInfo.CalloutPoint2.X = labelInfo.Position.Left;
                                }
                                else
                                {
                                    labelInfo.Position.Left = plotAreaPositionAbs.Left -
                                        labelInfo.Size.Width -
                                        4f * labelSpacing.Width;

                                    // Set callout line coordinates
                                    if (!isPyramid)
                                    {
                                        labelInfo.CalloutPoint1.X = plotAreaCenterXAbs -
                                            Math.Max(_funnelNeckSize.Width / 2f, (topDiameter + bottomDiameter) / 4f);
                                    }
                                    else
                                    {
                                        labelInfo.CalloutPoint1.X = plotAreaCenterXAbs -
                                            (topDiameter + bottomDiameter) / 4f;
                                    }
                                    labelInfo.CalloutPoint2.X = labelInfo.Position.Right;
                                }

                                // Fill rest of coordinates required for the callout line
                                labelInfo.CalloutPoint1.Y = segmentInfo.Location + segmentInfo.Height / 2f;
                                labelInfo.CalloutPoint2.Y = labelInfo.CalloutPoint1.Y;
                            }
                            else
                            {
                                if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                                {
                                    labelInfo.Position.Left = plotAreaCenterXAbs +
                                        (topDiameter + bottomDiameter) / 4f +
                                        4f * labelSpacing.Width;
                                }
                                else
                                {
                                    labelInfo.Position.Left = plotAreaCenterXAbs -
                                        labelInfo.Size.Width -
                                        (topDiameter + bottomDiameter) / 4f -
                                        4f * labelSpacing.Width;
                                }
                            }
                        }
                        else
                        {
                            // Use bottom part of the segment for the last point
                            if (lastLabel)
                            {
                                if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                                {
                                    labelInfo.Position.Left = plotAreaCenterXAbs +
                                        segmentInfo.EndWidth / 2f +
                                        4f * labelSpacing.Width;
                                }
                                else
                                {
                                    labelInfo.Position.Left = plotAreaCenterXAbs -
                                        labelInfo.Size.Width -
                                        segmentInfo.EndWidth / 2f -
                                        4f * labelSpacing.Width;
                                }
                                labelInfo.Position.Top = segmentInfo.Location +
                                    segmentInfo.Height -
                                    labelInfo.Size.Height / 2f;
                            }
                            else
                            {
                                if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                                {
                                    labelInfo.Position.Left = plotAreaCenterXAbs +
                                        segmentInfo.StartWidth / 2f +
                                        4f * labelSpacing.Width;
                                }
                                else
                                {
                                    labelInfo.Position.Left = plotAreaCenterXAbs -
                                        labelInfo.Size.Width -
                                        segmentInfo.StartWidth / 2f -
                                        4f * labelSpacing.Width;
                                }
                                labelInfo.Position.Top = segmentInfo.Location -
                                    labelInfo.Size.Height / 2f;
                            }

                            if (labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
                            {
                                if (labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
                                {
                                    labelInfo.Position.Left = plotAreaPositionAbs.Right +
                                        4f * labelSpacing.Width;

                                    // Set callout line coordinates
                                    labelInfo.CalloutPoint1.X = plotAreaCenterXAbs +
                                        ((lastLabel) ? segmentInfo.EndWidth : segmentInfo.StartWidth) / 2f;
                                    labelInfo.CalloutPoint2.X = labelInfo.Position.Left;

                                }
                                else
                                {
                                    labelInfo.Position.Left = plotAreaPositionAbs.Left -
                                        labelInfo.Size.Width -
                                        4f * labelSpacing.Width;

                                    // Set callout line coordinates
                                    labelInfo.CalloutPoint1.X = plotAreaCenterXAbs -
                                        ((lastLabel) ? segmentInfo.EndWidth : segmentInfo.StartWidth) / 2f;
                                    labelInfo.CalloutPoint2.X = labelInfo.Position.Right;
                                }

                                // Fill rest of coordinates required for the callout line
                                labelInfo.CalloutPoint1.Y = segmentInfo.Location;
                                if (lastLabel)
                                {
                                    labelInfo.CalloutPoint1.Y += segmentInfo.Height;
                                }
                                labelInfo.CalloutPoint2.Y = labelInfo.CalloutPoint1.Y;

                            }
                        }
                    }

                    //******************************************************
                    //** Labels are placed INSIDE of the funnel
                    //******************************************************
                    else if (labelInfo.Style == FunnelLabelStyle.Inside)
                    {
                        // Define position
                        labelInfo.Position.Left = plotAreaCenterXAbs - labelInfo.Size.Width / 2f;
                        if (_funnelStyle == FunnelStyle.YIsHeight)
                        {
                            labelInfo.Position.Top = (segmentInfo.Location + segmentInfo.Height / 2f) -
                                labelInfo.Size.Height / 2f;
                            if (labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Top)
                            {
                                labelInfo.Position.Top -= segmentInfo.Height / 2f - labelInfo.Size.Height / 2f - labelSpacing.Height;
                            }
                            else if (labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Bottom)
                            {
                                labelInfo.Position.Top += segmentInfo.Height / 2f - labelInfo.Size.Height / 2f - labelSpacing.Height;
                            }
                        }
                        else
                        {
                            labelInfo.Position.Top = segmentInfo.Location - labelInfo.Size.Height / 2f;
                            if (labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Top)
                            {
                                labelInfo.Position.Top -= labelInfo.Size.Height / 2f + labelSpacing.Height;
                            }
                            else if (labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Bottom)
                            {
                                labelInfo.Position.Top += labelInfo.Size.Height / 2f + labelSpacing.Height;
                            }

                            // Use bottom part of the segment for the last point
                            if (lastLabel)
                            {
                                labelInfo.Position.Top += segmentInfo.Height;
                            }
                        }

                        // Adjust label Y position in 3D
                        if (Area.Area3DStyle.Enable3D)
                        {
                            labelInfo.Position.Top += (float)((segmentInfo.EndWidth + segmentInfo.StartWidth) / 4f * Math.Sin(_rotation3D / 180F * Math.PI));
                        }
                    }

                    //******************************************************
                    //** Check if label overlaps any previous label
                    //******************************************************
                    int interation = 0;
                    while (IsLabelsOverlap(labelInfo) && interation < 1000)
                    {
                        float shiftSize = (isPyramid) ? -3f : 3f;

                        // Move label down
                        labelInfo.Position.Top += shiftSize;

                        // Move callout second point down
                        if (!labelInfo.CalloutPoint2.IsEmpty)
                        {
                            labelInfo.CalloutPoint2.Y += shiftSize;
                        }

                        ++interation;
                    }

                }
            }
        }

        /// <summary>
        /// Checks if specified label overlaps any previous labels.
        /// </summary>
        /// <param name="testLabelInfo">Label to test.</param>
        /// <returns>True if labels overlapp.</returns>
        private bool IsLabelsOverlap(FunnelPointLabelInfo testLabelInfo)
        {
            // Increase rectangle size by 1 pixel
            SKRect rect = testLabelInfo.Position;
            rect.Inflate(1f, 1f);

            // Increase label rectangle if border is drawn around the label
            if (testLabelInfo.Point.LabelBackColor != SKColor.Empty ||
                (testLabelInfo.Point.LabelBorderWidth > 0 &&
                testLabelInfo.Point.LabelBorderColor != SKColor.Empty &&
                testLabelInfo.Point.LabelBorderDashStyle != ChartDashStyle.NotSet))
            {
                rect.Inflate(4f, 4f);
            }

            //Loop through all labels
            foreach (FunnelPointLabelInfo labelInfo in labelInfoList)
            {
                // Stop searching
                if (labelInfo.PointIndex == testLabelInfo.PointIndex)
                {
                    break;
                }

                // Check if label position overlaps
                if (!labelInfo.Position.IsEmpty &&
                    labelInfo.Position.IntersectsWith(rect))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets label style of the data point.
        /// </summary>
        /// <returns>Label style of the data point.</returns>
        private FunnelLabelStyle GetLabelStyle(DataPointCustomProperties properties)
        {
            // Set default label style
            FunnelLabelStyle labelStyle = FunnelLabelStyle.OutsideInColumn;

            // Get string value of the custom attribute
            string attrValue = properties[funnelLabelStyleAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the labels style
                try
                {
                    labelStyle = (FunnelLabelStyle)Enum.Parse(typeof(FunnelLabelStyle), attrValue, true);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(labelStyle.ToString(), funnelLabelStyleAttributeName)));
                }
            }
            return labelStyle;
        }

        #endregion // Labels Methods

        #region Position Methods

        /// <summary>
        /// Calculate the spacing required for the labels.
        /// </summary>
        private void GetPlotAreaSpacing()
        {
            // Provide small spacing on the sides of chart area
            plotAreaSpacing = new SKRect(1f, 1f, 1f, 1f);

            // Get plotting area position
            PlotAreaPosition = GetPlotAreaPosition();

            // Get list of segments
            segmentList = GetFunnelSegmentPositions();

            // If plotting area position is automatic
            if (Area.InnerPlotPosition.Auto)
            {
                // Set a position so that data labels fit
                // This method is called several time to adjust label position while 
                // funnel side angle is changed
                int iteration = 0;
                while (!FitPointLabels() && iteration < 5)
                {
                    iteration++;
                }
            }
            else
            {
                // Just get labels position
                GetLabelsPosition();
            }

        }

        /// <summary>
        /// Gets a rectangle in relative coordinates where the funnel will chart
        /// will be drawn.
        /// </summary>
        /// <returns>Plotting are of the chart in relative coordinates.</returns>
        private SKRect GetPlotAreaPosition()
        {
            // Get plotting area rectangle position
            SKRect plotAreaPosition = (Area.InnerPlotPosition.Auto) ?
                Area.Position.ToSKRect() : Area.PlotAreaPosition.ToSKRect();

            // NOTE: Fixes issue #4085
            // Do not allow decreasing of the plot area height more than 50%
            if (plotAreaSpacing.Top > plotAreaPosition.Height / 2f)
            {
                plotAreaSpacing.Top = plotAreaPosition.Height / 2f;
            }
            if (plotAreaSpacing.Height > plotAreaPosition.Height / 2f)
            {
                plotAreaSpacing.Size = new SKSize(plotAreaSpacing.Width, plotAreaPosition.Height / 2f);
            }

            // Decrease plotting are position using pre-calculated ratio
            plotAreaPosition.Left += plotAreaSpacing.Left;
            plotAreaPosition.Top += plotAreaSpacing.Top;
            plotAreaPosition.Right -= plotAreaSpacing.Left + plotAreaSpacing.Width;
            plotAreaPosition.Bottom -= plotAreaSpacing.Top + plotAreaSpacing.Height;

            // Apply vertical spacing on top and bottom to fit the 3D surfaces
            if (Area.Area3DStyle.Enable3D)
            {
                // Convert position to pixels
                SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(plotAreaPosition);

                // Funnel chart process only first series in the chart area
                // and cannot be combined with any other chart types.
                Series series = GetDataSeries();
                if (series != null)
                {
                    // Get 3D funnel rotation angle (from 10 to -10)
                    _rotation3D = GetFunnelRotation(series);
                }

                // Get top and bottom spacing
                float topSpacing = (float)Math.Abs((plotAreaPositionAbs.Width / 2f) * Math.Sin(_rotation3D / 180F * Math.PI));
                float bottomSpacing = (float)Math.Abs((plotAreaPositionAbs.Width / 2f) * Math.Sin(_rotation3D / 180F * Math.PI));

                // Adjust position
                if (isPyramid)
                {
                    // Only bottom spacing for the pyramid
                    plotAreaPositionAbs.Bottom -= bottomSpacing;
                }
                else
                {
                    // Add top/bottom spacing
                    plotAreaPositionAbs.Top += topSpacing;
                    plotAreaPositionAbs.Bottom -= topSpacing + bottomSpacing;
                }

                // Convert position back to relative coordinates
                plotAreaPosition = Graph.GetRelativeRectangle(plotAreaPositionAbs);
            }

            return plotAreaPosition;
        }

        #endregion // Position Methods

        #region Helper Methods

        /// <summary>
        /// Checks for minimum segment height.
        /// </summary>
        /// <param name="height">Current segment height.</param>
        /// <returns>Adjusted segment height.</returns>
        protected float CheckMinHeight(float height)
        {
            // When point gap is used do not allow to have the segment heigth to be zero.
            float minSize = Math.Min(2f, funnelSegmentGap / 2f);
            if (funnelSegmentGap > 0 &&
                height < minSize)
            {
                return minSize;
            }

            return height;
        }

        /// <summary>
        /// Gets minimum point height in pixels.
        /// </summary>
        /// <returns>Minimum point height in pixels.</returns>
        private void GetFunnelMinPointHeight(DataPointCustomProperties properties)
        {
            // Set default minimum point size
            _funnelMinPointHeight = 0f;

            // Get string value of the custom attribute
            string attrValue = properties[funnelPointMinHeight];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size

                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float pointHeight);
                if (parseSucceed)
                {
                    _funnelMinPointHeight = pointHeight;
                }

                if (!parseSucceed || _funnelMinPointHeight < 0f || _funnelMinPointHeight > 100f)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelMinimumPointHeightAttributeInvalid));
                }

                // Check if specified value is too big
                _funnelMinPointHeight = (float)(yValueTotal * _funnelMinPointHeight / 100f);

                // Get data statistic again using Min value
                GetDataPointValuesStatistic();
            }
        }

        /// <summary>
        /// Gets 3D funnel rotation angle.
        /// </summary>
        /// <returns>Rotation angle.</returns>
        private int GetFunnelRotation(DataPointCustomProperties properties)
        {
            // Set default gap size
            int angle = 5;

            // Get string value of the custom attribute
            string attrValue = properties[funnelRotationAngleAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size

                bool parseSucceed = int.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int a);
                if (parseSucceed)
                {
                    angle = a;
                }

                // Validate attribute value
                if (!parseSucceed || angle < -10 || angle > 10)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelAngleRangeInvalid));
                }
            }

            return angle;
        }

        /// <summary>
        /// Gets callout line color.
        /// </summary>
        /// <returns>Callout line color.</returns>
        private static SKColor GetCalloutLineColor(DataPointCustomProperties properties)
        {
            // Set default gap size
            SKColor color = SKColors.Black;

            // Get string value of the custom attribute
            string attrValue = properties[CustomPropertyName.CalloutLineColor];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to Color
                bool failed = false;
                ColorConverter colorConverter = new();
                try
                {
                    color = (SKColor)colorConverter.ConvertFromInvariantString(attrValue);
                }
                catch (ArgumentException)
                {
                    failed = true;
                }
                catch (NotSupportedException)
                {
                    failed = true;
                }

                // In case of an error try to convert using local settings
                if (failed)
                {
                    try
                    {
                        color = (SKColor)colorConverter.ConvertFromString(attrValue);
                    }
                    catch (ArgumentException)
                    {
                        throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, "CalloutLineColor"));
                    }
                }

            }

            return color;
        }

        /// <summary>
        /// Gets funnel neck size when shape of the funnel do not change.
        /// </summary>
        /// <returns>Funnel neck width and height.</returns>
        private SKSize GetFunnelNeckSize(DataPointCustomProperties properties)
        {
            // Set default gap size
            SKSize neckSize = new(5f, 5f);

            // Get string value of the custom attribute
            string attrValue = properties[CustomPropertyName.FunnelNeckWidth];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float w);
                if (parseSucceed)
                {
                    neckSize.Width = w;
                }

                // Validate attribute value
                if (!parseSucceed || neckSize.Width < 0 || neckSize.Width > 100)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelNeckWidthInvalid));
                }
            }

            // Get string value of the custom attribute
            attrValue = properties[CustomPropertyName.FunnelNeckHeight];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float h);
                if (parseSucceed)
                {
                    neckSize.Height = h;
                }


                if (!parseSucceed || neckSize.Height < 0 || neckSize.Height > 100)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelNeckHeightInvalid));
                }
            }

            // Make sure the neck size do not exceed the plotting area size
            if (neckSize.Height > PlotAreaPosition.Height / 2f)
            {
                neckSize.Height = PlotAreaPosition.Height / 2f;
            }
            if (neckSize.Width > PlotAreaPosition.Width / 2f)
            {
                neckSize.Width = PlotAreaPosition.Width / 2f;
            }

            // Convert from relative coordinates to pixels
            return Graph.GetAbsoluteSize(neckSize);
        }

        /// <summary>
        /// Gets gap between points in pixels.
        /// </summary>
        /// <returns>Gap between funnel points.</returns>
        private float GetFunnelPointGap(DataPointCustomProperties properties)
        {
            // Set default gap size
            float gapSize = 0f;

            // Get string value of the custom attribute
            string attrValue = properties[funnelPointGapAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float gs);
                if (parseSucceed)
                {
                    gapSize = gs;
                }
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, funnelPointGapAttributeName)));
                }

                // Make sure the total gap size for all points do not exceed the total height of the plotting area
                float maxGapSize = PlotAreaPosition.Height / (pointNumber - ((ShouldDrawFirstPoint()) ? 1 : 2));
                if (gapSize > maxGapSize)
                {
                    gapSize = maxGapSize;
                }
                if (gapSize < 0)
                {
                    gapSize = 0;
                }

                // Convert from relative coordinates to pixels
                gapSize = Graph.GetAbsoluteSize(new SKSize(gapSize, gapSize)).Height;
            }

            return gapSize;
        }

        /// <summary>
        /// Gets funnel drawing style.
        /// </summary>
        /// <returns>funnel drawing style.</returns>
        private FunnelStyle GetFunnelStyle(DataPointCustomProperties properties)
        {
            // Set default funnel drawing style
            FunnelStyle drawingStyle = FunnelStyle.YIsHeight;

            // Get string value of the custom attribute
            if (!isPyramid)
            {
                string attrValue = properties[CustomPropertyName.FunnelStyle];
                if (attrValue != null && attrValue.Length > 0)
                {
                    // Convert string to the labels style
                    try
                    {
                        drawingStyle = (FunnelStyle)Enum.Parse(typeof(FunnelStyle), attrValue, true);
                    }
                    catch
                    {
                        throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, "FunnelStyle"));
                    }
                }
            }
            return drawingStyle;
        }

        /// <summary>
        /// Gets outside labels placement.
        /// </summary>
        /// <returns>Outside labels placement.</returns>
        private FunnelLabelPlacement GetOutsideLabelPlacement(DataPointCustomProperties properties)
        {
            // Set default vertical alignment for the inside labels
            FunnelLabelPlacement placement = FunnelLabelPlacement.Right;

            // Get string value of the custom attribute
            string attrValue = properties[funnelOutsideLabelPlacementAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the labels placement
                try
                {
                    placement = (FunnelLabelPlacement)Enum.Parse(typeof(FunnelLabelPlacement), attrValue, true);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, funnelOutsideLabelPlacementAttributeName)));
                }
            }
            return placement;
        }

        /// <summary>
        /// Gets inside labels vertical alignment.
        /// </summary>
        /// <returns>Inside labels vertical alignment.</returns>
        private FunnelLabelVerticalAlignment GetInsideLabelAlignment(DataPointCustomProperties properties)
        {
            // Set default vertical alignment for the inside labels
            FunnelLabelVerticalAlignment alignment = FunnelLabelVerticalAlignment.Center;

            // Get string value of the custom attribute
            string attrValue = properties[funnelInsideLabelAlignmentAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the labels style
                try
                {
                    alignment = (FunnelLabelVerticalAlignment)Enum.Parse(typeof(FunnelLabelVerticalAlignment), attrValue, true);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, funnelInsideLabelAlignmentAttributeName)));
                }
            }
            return alignment;
        }

        /// <summary>
        /// Gets funnel 3D drawing style.
        /// </summary>
        /// <returns>funnel drawing style.</returns>
        private Funnel3DDrawingStyle GetFunnel3DDrawingStyle(DataPointCustomProperties properties)
        {
            // Set default funnel drawing style
            Funnel3DDrawingStyle drawingStyle = (isPyramid) ?
                Funnel3DDrawingStyle.SquareBase : Funnel3DDrawingStyle.CircularBase;

            // Get string value of the custom attribute
            string attrValue = properties[funnel3DDrawingStyleAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the labels style
                try
                {
                    drawingStyle = (Funnel3DDrawingStyle)Enum.Parse(typeof(Funnel3DDrawingStyle), attrValue, true);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, funnel3DDrawingStyleAttributeName)));
                }
            }

            return drawingStyle;
        }

        /// <summary>
        /// Get data point Y and X values statistics:
        ///   - Total of all Y values
        ///   - Total of all X values
        ///   - Maximum Y value
        /// Negative values are treated as positive.
        /// </summary>
        private void GetDataPointValuesStatistic()
        {
            // Get funnel chart type series
            Series series = GetDataSeries();
            if (series != null)
            {
                // Reset values
                yValueTotal = 0.0;
                _xValueTotal = 0.0;
                _yValueMax = 0.0;
                pointNumber = 0;

                // Get value type
                _valuePercentages = null;
                PyramidValueType valueType = GetPyramidValueType(series);
                if (valueType == PyramidValueType.Surface)
                {
                    // Calculate the total surface area
                    double triangleArea = 0.0;
                    int pointIndex = 0;
                    foreach (DataPoint point in series.Points)
                    {
                        // Ignore empty points
                        if (!point.IsEmpty)
                        {
                            // Get Y value
                            triangleArea += GetYValue(point, pointIndex);
                        }
                        ++pointIndex;
                    }

                    // Calculate the base
                    double triangleHeight = 100.0;
                    double triangleBase = (2 * triangleArea) / triangleHeight;

                    // Calculate the base to height ratio
                    double baseRatio = triangleBase / triangleHeight;

                    // Calcuate the height percentage for each value
                    double[] percentages = new double[series.Points.Count];
                    double sumArea = 0.0;
                    for (int loop = 0; loop < percentages.Length; loop++)
                    {
                        double yValue = GetYValue(series.Points[loop], loop);
                        sumArea += yValue;
                        percentages[loop] = Math.Sqrt((2 * sumArea) / baseRatio);
                    }
                    _valuePercentages = percentages;
                }

                // Loop through all ponts in the data series
                foreach (DataPoint point in series.Points)
                {
                    // Ignore empty points
                    if (!point.IsEmpty)
                    {
                        // Get Y value
                        double yValue = GetYValue(point, pointNumber);

                        // Get data point Y and X values statistics
                        yValueTotal += yValue;
                        _yValueMax = Math.Max(_yValueMax, yValue);
                        _xValueTotal += GetXValue(point);
                    }

                    ++pointNumber;
                }

            }
        }

        /// <summary>
        /// Gets funnel chart series that belongs to the current chart area.
        /// Method also checks that only one visible Funnel series exists in the chart area.
        /// </summary>
        /// <returns>Funnel chart type series.</returns>
        private Series GetDataSeries()
        {
            // Check if funnel series was already found
            if (_chartTypeSeries == null)
            {
                // Loop through all series
                Series funnelSeries = null;
                foreach (Series series in Common.DataManager.Series)
                {
                    // Check if series is visible and belong to the current chart area
                    if (series.IsVisible() &&
                        series.ChartArea == Area.Name)
                    {
                        // Check series chart type is Funnel
                        if (String.Compare(series.ChartTypeName, Name, true, System.Globalization.CultureInfo.CurrentCulture) == 0)
                        {
                            if (funnelSeries == null)
                            {
                                funnelSeries = series;
                            }
                        }
                        else if (!Common.ChartPicture.SuppressExceptions)
                        {
                            // Funnel chart can not be combined with other chart type
                            throw (new InvalidOperationException(SR.ExceptionFunnelCanNotCombine));
                        }
                    }
                }

                // Remember the chart type series
                _chartTypeSeries = funnelSeries;
            }

            return _chartTypeSeries;
        }

        /// <summary>
        /// Gets pyramid value type. Each point value may represent a "Linear" height of
        /// the segment or "Surface" of the segment.
        /// </summary>
        /// <returns>Pyramid value type.</returns>
        private PyramidValueType GetPyramidValueType(DataPointCustomProperties properties)
        {
            // Set default funnel drawing style
            PyramidValueType valueType = PyramidValueType.Linear;

            // Get string value of the custom attribute
            if (isPyramid)
            {
                string attrValue = properties[CustomPropertyName.PyramidValueType];
                if (attrValue != null && attrValue.Length > 0)
                {
                    // Convert string to the labels style
                    try
                    {
                        valueType = (PyramidValueType)Enum.Parse(typeof(PyramidValueType), attrValue, true);
                    }
                    catch
                    {
                        throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, "PyramidValueType"));
                    }
                }
            }
            return valueType;
        }

        #endregion // Helper Methods

        #region Y & X values related methods

        /// <summary>
        /// Helper function, which returns the Y value of the point.
        /// </summary>
        /// <param name="point">Point object.</param>
        /// <param name="pointIndex">Point index.</param>
        /// <returns>Y value of the point.</returns>
        virtual public double GetYValue(DataPoint point, int pointIndex)
        {
            double yValue = 0.0;
            if (!point.IsEmpty)
            {
                // Get Y value
                yValue = point.YValues[0];

                // Adjust point value
                if (_valuePercentages != null &&
                    _valuePercentages.Length > pointIndex)
                {
                    yValue = yValue / 100.0 * _valuePercentages[pointIndex];
                }

                if (Area.AxisY.IsLogarithmic)
                {
                    yValue = Math.Abs(Math.Log(yValue, Area.AxisY.LogarithmBase));
                }
                else
                {
                    yValue = Math.Abs(yValue);
                    if (yValue < _funnelMinPointHeight)
                    {
                        yValue = _funnelMinPointHeight;
                    }
                }
            }
            return yValue;
        }

        /// <summary>
        /// Helper function, which returns the X value of the point.
        /// </summary>
        /// <param name="point">Point object.</param>
        /// <returns>X value of the point.</returns>
        virtual public double GetXValue(DataPoint point)
        {
            if (Area.AxisX.IsLogarithmic)
            {
                return Math.Abs(Math.Log(point.XValue, Area.AxisX.LogarithmBase));
            }
            return Math.Abs(point.XValue);
        }

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
        virtual public double GetYValue(
            CommonElements common,
            ChartArea area,
            Series series,
            DataPoint point,
            int pointIndex,
            int yValueIndex)
        {
            return point.YValues[yValueIndex];
        }

        #endregion // Y & X values related methods

        #region SmartLabelStyle methods

        /// <summary>
        /// Adds markers position to the list. Used to check SmartLabelStyle overlapping.
        /// </summary>
        /// <param name="common">Common chart elements.</param>
        /// <param name="area">Chart area.</param>
        /// <param name="series">Series values to be used.</param>
        /// <param name="list">List to add to.</param>
        public void AddSmartLabelMarkerPositions(CommonElements common, ChartArea area, Series series, ArrayList list)
        {
            // Fast Line chart type do not support labels
        }

        #endregion

        #region IDisposable interface implementation
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            //Nothing to dispose at the base class. 
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// PyramidChart class overrides some of the functionality of FunnelChart class.
    /// Most of drawing and othere processing is done in the FunnelChart.
    /// </summary>
    internal class PyramidChart : FunnelChart
    {
        #region Fields and Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public PyramidChart()
        {
            // Renering of the pyramid chart type
            base.isPyramid = true;

            // Pyramid chart type uses square base by default
            base.round3DShape = false;

            // Pyramid properties names
            base.funnelLabelStyleAttributeName = CustomPropertyName.PyramidLabelStyle;
            base.funnelPointGapAttributeName = CustomPropertyName.PyramidPointGap;
            base.funnelRotationAngleAttributeName = CustomPropertyName.Pyramid3DRotationAngle;
            base.funnelPointMinHeight = CustomPropertyName.PyramidMinPointHeight;
            base.funnel3DDrawingStyleAttributeName = CustomPropertyName.Pyramid3DDrawingStyle;
            base.funnelInsideLabelAlignmentAttributeName = CustomPropertyName.PyramidInsideLabelAlignment;
            base.funnelOutsideLabelPlacementAttributeName = CustomPropertyName.PyramidOutsideLabelPlacement;
        }

        #endregion

        #region IChartType interface implementation

        /// <summary>
        /// Chart type name
        /// </summary>
        override public string Name { get { return ChartTypeNames.Pyramid; } }

        #endregion

        #region Methods

        /// <summary>
        /// Gets pyramid data point segment height and width.
        /// </summary>
        /// <param name="series">Chart type series.</param>
        /// <param name="pointIndex">Data point index in the series.</param>
        /// <param name="location">Segment top location. Bottom location if reversed drawing order.</param>
        /// <param name="height">Returns the height of the segment.</param>
        /// <param name="startWidth">Returns top width of the segment.</param>
        /// <param name="endWidth">Returns botom width of the segment.</param>
        protected override void GetPointWidthAndHeight(
            Series series,
            int pointIndex,
            float location,
            out float height,
            out float startWidth,
            out float endWidth)
        {
            SKPoint pointPositionAbs;

            // Get plotting area position in pixels
            SKRect plotAreaPositionAbs = Graph.GetAbsoluteRectangle(PlotAreaPosition);

            // Calculate total height of plotting area minus reserved space for the gaps
            float plotAreaHeightAbs = plotAreaPositionAbs.Height -
                funnelSegmentGap * (pointNumber - ((ShouldDrawFirstPoint()) ? 1 : 2));
            if (plotAreaHeightAbs < 0f)
            {
                plotAreaHeightAbs = 0f;
            }

            // Calculate segment height as a part of total Y values in series
            height = (float)(plotAreaHeightAbs * (GetYValue(series.Points[pointIndex], pointIndex) / yValueTotal));

            // Check for minimum segment height
            height = CheckMinHeight(height);

            // Get intersection point of the horizontal line at the start of the segment
            // with the left pre-defined wall of the funnel.
            SKPoint startIntersection = ChartGraphics.GetLinesIntersection(
                plotAreaPositionAbs.Left, location - height,
                plotAreaPositionAbs.Right, location - height,
                plotAreaPositionAbs.Left, plotAreaPositionAbs.Bottom,
                plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f, plotAreaPositionAbs.Top);
            if (startIntersection.X > (plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f))
            {
                startIntersection.X = plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f;
            }

            // Get intersection point of the horizontal line at the end of the segment
            // with the left pre-defined wall of the funnel.
            SKPoint endIntersection = ChartGraphics.GetLinesIntersection(
                plotAreaPositionAbs.Left, location,
                plotAreaPositionAbs.Right, location,
                plotAreaPositionAbs.Left, plotAreaPositionAbs.Bottom,
                plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f, plotAreaPositionAbs.Top);
            if (endIntersection.X > (plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f))
            {
                endIntersection.X = plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f;
            }

            // Get segment start and end width
            startWidth = Math.Abs(plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f -
                startIntersection.X) * 2f;
            endWidth = Math.Abs(plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f -
                endIntersection.X) * 2f;

            // Set point position for annotation anchoring
            pointPositionAbs = new SKPoint(
                plotAreaPositionAbs.Left + plotAreaPositionAbs.Width / 2f,
                location - height / 2f);

            // Set pre-calculated point position
            series.Points[pointIndex].positionRel = Graph.GetRelativePoint(pointPositionAbs);
        }

        #endregion // Methods
    }

    /// <summary>
    /// Helper data structure used to store information about single funnel segment.
    /// </summary>
    internal class FunnelSegmentInfo
    {
        #region Fields

        // Assosiated data point
        public DataPoint Point = null;

        // Data point index
        public int PointIndex = 0;

        // Segment top position
        public float Location = 0f;

        // Segment height
        public float Height = 0f;

        // Segment top width
        public float StartWidth = 0f;

        // Segment bottom width
        public float EndWidth = 0f;

        // Segment has nothing on the top
        public bool NothingOnTop = false;

        // Segment has nothing on the bottom
        public bool NothingOnBottom = false;

        #endregion // Fields
    }

    /// <summary>
    /// Helper data structure used to store information about funnel data point label.
    /// </summary>
    internal class FunnelPointLabelInfo
    {
        #region Fields

        // Assosiated data point
        public DataPoint Point = null;

        // Data point index
        public int PointIndex = 0;

        // Label text
        public string Text = string.Empty;

        // Data point label size
        public SKSize Size = SKSize.Empty;

        // Position of the data point label
        public SKRect Position = SKRect.Empty;

        // Label style
        public FunnelLabelStyle Style = FunnelLabelStyle.OutsideInColumn;

        // Inside label vertical alignment
        public FunnelLabelVerticalAlignment VerticalAlignment = FunnelLabelVerticalAlignment.Center;

        // Outside labels placement
        public FunnelLabelPlacement OutsidePlacement = FunnelLabelPlacement.Right;

        // Label callout first point
        public SKPoint CalloutPoint1 = SKPoint.Empty;

        // Label callout second point
        public SKPoint CalloutPoint2 = SKPoint.Empty;

        #endregion // Fields
    }
}
