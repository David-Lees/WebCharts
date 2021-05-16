using SkiaSharp;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.General;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.ChartTypes
{

    /// <summary>
    /// SplineAreaChart class extends the AreaChart class by 
    /// providing a different initial tension for the line.
    /// </summary>

    internal class SplineAreaChart : AreaChart
    {
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SplineAreaChart()
        {
            // Set default line tension
            base.lineTension = 0.5f;
        }

        #endregion

        #region IChartType interface implementation

        /// <summary>
        /// Chart type name
        /// </summary>
        public override string Name { get { return ChartTypeNames.SplineArea; } }

        /// <summary>
        /// Gets chart type image.
        /// </summary>
        /// <param name="registry">Chart types registry object.</param>
        /// <returns>Chart type image.</returns>
        override public SKImage GetImage(ChartTypeRegistry registry)
        {
            return (SKImage)registry.ResourceManager.GetObject(Name + "ChartType");
        }
        #endregion

        #region Default tension method

        /// <summary>
        /// Gets default line tension. For spline charts it's always 0.5.
        /// </summary>
        /// <returns>Line tension.</returns>
        override protected float GetDefaultTension()
        {
            return 0.5f;
        }

        /// <summary>
        /// Checks if line tension is supported by the chart type.
        /// </summary>
        /// <returns>True if line tension is supported.</returns>
        protected override bool IsLineTensionSupported()
        {
            return true;
        }

        #endregion
    }

    /// <summary>
    /// AreaChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Area and SplineArea charts. The 
    /// only difference of the SplineArea chart is the default 
    /// tension of the line.
    /// 
    /// SplineChart base class provides most of the functionality 
    /// like drawing lines, labels and markers.
    /// </summary>
    internal class AreaChart : SplineChart
    {
        #region Fields

        /// <summary>
        /// Fields used to fill area with gradient
        /// </summary>

        protected bool gradientFill = false;

        /// <summary>
        /// Coordinates of the area path
        /// </summary>
        protected SKPath areaPath = null;

        /// <summary>
        /// Reference to the current series object
        /// </summary>
        protected Series Series { get; set; }

        /// <summary>
        /// Horizontal axis position
        /// </summary>
        protected SKPoint axisPos;

        #endregion

        #region Constructor

        /// <summary>
        /// Area chart constructor
        /// </summary>
        public AreaChart()
        {
            drawOutsideLines = true;

            // Set default line tension
            base.lineTension = 0f;

            // Reset axis position
            axisPos = SKPoint.Empty;
        }

        #endregion

        #region Default tension method

        /// <summary>
        /// Gets default line tension.
        /// </summary>
        /// <returns>Line tension.</returns>
        override protected float GetDefaultTension()
        {
            return 0f;
        }

        #endregion

        #region IChartType interface implementation

        /// <summary>
        /// Chart type name
        /// </summary>
        public override string Name { get { return ChartTypeNames.Area; } }

        /// <summary>
        /// If the crossing value is auto Crossing value should be 
        /// automatically set to zero for some chart 
        /// types (Bar, column, area etc.)
        /// </summary>
        public override bool ZeroCrossing { get { return true; } }

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
        /// Gets chart type image.
        /// </summary>
        /// <param name="registry">Chart types registry object.</param>
        /// <returns>Chart type image.</returns>
        override public SKImage GetImage(ChartTypeRegistry registry)
        {
            return (SKImage)registry.ResourceManager.GetObject(Name + "ChartType");
        }

        #endregion

        #region Painting and Selection methods

        /// <summary>
        /// This method recalculates position of the end points of lines. This method 
        /// is used from Paint or Select method.
        /// </summary>
        /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
        /// <param name="graph">The Chart Graphics object.</param>
        /// <param name="common">The Common elements object.</param>
        /// <param name="area">Chart area for this chart.</param>
        /// <param name="seriesToDraw">Chart series to draw.</param>
        protected override void ProcessChartType(
            bool selection,
            ChartGraphics graph,
            CommonElements common,
            ChartArea area,
            Series seriesToDraw)
        {
            // Reset background gradient fill flag
            gradientFill = false;

            // Reset axis position
            axisPos = SKPoint.Empty;

            // Call base SplineChart class
            base.ProcessChartType(selection, graph, common, area, seriesToDraw);

            // Fill background gradient for the 2D chart. Feature is not supported in 3D.
            if (!area.Area3DStyle.Enable3D)
            {
                FillLastSeriesGradient(graph);
            }

        }

        /// <summary>
        /// This method is overriden to fill the area and draw border line.
        /// </summary>
        /// <param name="graph">Graphics object.</param>
        /// <param name="common">The Common elements object</param>
        /// <param name="point">Point to draw the line for.</param>
        /// <param name="series">Point series.</param>
        /// <param name="points">Array of oints coordinates.</param>
        /// <param name="pointIndex">Index of point to draw.</param>
        /// <param name="tension">Line tension</param>
        override protected void DrawLine(
            ChartGraphics graph,
            CommonElements common,
            DataPoint point,
            Series series,
            SKPoint[] points,
            int pointIndex,
            float tension)
        {
            // Start drawing from the second point
            if (pointIndex <= 0)
            {
                return;
            }

            // Check if its a beginning of a new series
            if (Series != null)
            {
                if (Series.Name != series.Name)
                {
                    // Fill gradient from the previous series
                    FillLastSeriesGradient(graph);
                    Series = series;
                }
            }
            else
            {
                Series = series;
            }

            // Calculate points position
            SKPoint point1 = points[pointIndex - 1];
            SKPoint point2 = points[pointIndex];
            point1.X = (float)Math.Round(point1.X);
            point1.Y = (float)Math.Round(point1.Y);
            point2.X = (float)Math.Round(point2.X);
            point2.Y = (float)Math.Round(point2.Y);
            if (axisPos == SKPoint.Empty)
            {
                axisPos.X = (float)VAxis.GetPosition(VAxis.Crossing);
                axisPos.Y = (float)VAxis.GetPosition(VAxis.Crossing);
                axisPos = graph.GetAbsolutePoint(axisPos);
                axisPos.X = (float)Math.Round(axisPos.X);
                axisPos.Y = (float)Math.Round(axisPos.Y);
            }

            // Point properties
            SKColor pointColor = point.Color;
            SKColor pointBorderColor = point.BorderColor;
            int pointBorderWidth = point.BorderWidth;
            ChartDashStyle pointBorderDashStyle = point.BorderDashStyle;

            // Create area brush
            SKPaint areaBrush = null;
            if (point.BackHatchStyle != ChartHatchStyle.None)
            {
                areaBrush = ChartGraphics.GetHatchBrush(point.BackHatchStyle, pointColor, point.BackSecondaryColor);
            }
            else if (point.BackGradientStyle != GradientStyle.None)
            {
                gradientFill = true;
                Series = point.series;
            }
            else if (point.BackImage.Length > 0 && point.BackImageWrapMode != ChartImageWrapMode.Unscaled && point.BackImageWrapMode != ChartImageWrapMode.Scaled)
            {
                areaBrush = graph.GetTextureBrush(point.BackImage, point.BackImageTransparentColor, point.BackImageWrapMode, point.Color);
            }
            else
            {
                areaBrush = new SKPaint() { Color = pointColor };
            }

            // Calculate data point area segment path
            SKPath path = new();
            path.MoveTo(point1.X, axisPos.Y);
            path.LineTo(point1.X, point1.Y);
            if (lineTension == 0)
            {
                path.MoveTo(points[pointIndex - 1]);
                path.LineTo(points[pointIndex]);
            }
            else
            {
                path = SkiaSharpExtensions.CreateSpline(path, points);
            }
            path.AddLine(point2.X, point2.Y, point2.X, axisPos.Y);

            // Draw shadow
            if (series.ShadowColor != SKColor.Empty && series.ShadowOffset != 0 && pointColor != SKColor.Empty && pointColor != SKColors.Transparent)
            {
                SKRegion shadowRegion = new(path);
                using SKPaint shadowBrush = new() { Color = (series.ShadowColor.Alpha != 255) ? series.ShadowColor : Color.FromArgb((byte)(pointColor.Alpha / 2), series.ShadowColor) };
                // Set offset transformation
                SKRegion clipRegion = null;
                SKRegion clipRegionOld = null;
                if (!graph.IsClipEmpty)
                {
                    clipRegionOld = new(graph.Clip);
                    clipRegion = graph.Clip;
                    clipRegion.Translate(series.ShadowOffset, series.ShadowOffset);
                    graph.Clip = clipRegion;
                }
                graph.TranslateTransform(series.ShadowOffset, series.ShadowOffset);

                // Draw top and bottom lines
                if (graph.SmoothingMode != SmoothingMode.None)
                {
                    shadowBrush.StrokeWidth = 1;

                    if (lineTension == 0)
                    {
                        graph.DrawLine(shadowBrush, points[pointIndex - 1], points[pointIndex]);
                    }
                    else
                    {
                        graph.DrawCurve(shadowBrush, points, pointIndex - 1, 1, lineTension);
                    }

                }

                // Fill shadow region
                graph.FillRegion(shadowBrush, shadowRegion);

                // Restore transformation matrix
                if (clipRegion != null && clipRegionOld != null)
                {
                    graph.Clip = clipRegionOld;
                }
            }

            // Draw area
            if (!gradientFill)
            {
                // Turn off anti aliasing and fill area
                SmoothingMode oldMode = graph.SmoothingMode;
                graph.SmoothingMode = SmoothingMode.None;

                // Fill area
                graph.FillPath(areaBrush, path);

                // Draw right side of the area (not filled by FillPath)

                // Restore smoothing mode
                graph.SmoothingMode = oldMode;

                // Draw top and bottom lines
                if (graph.SmoothingMode != SmoothingMode.None)
                {
                    areaBrush.StrokeWidth = 1;

                    if (lineTension == 0)
                    {
                        // Draw horizontal and vertical lines without anti-aliasing
                        if (!(points[pointIndex - 1].X == points[pointIndex].X ||
                            points[pointIndex - 1].Y == points[pointIndex].Y))
                        {
                            graph.DrawLine(areaBrush, points[pointIndex - 1], points[pointIndex]);
                        }
                    }
                    else
                    {
                        graph.DrawCurve(areaBrush, points, pointIndex - 1, 1, lineTension);
                    }
                }

            }

            if (areaBrush != null)
                areaBrush.Dispose();

            // Add first line
            if (areaPath == null)
            {
                areaPath = new SKPath();
                areaPath.AddLine(point1.X, axisPos.Y, point1.X, point1.Y);
            }

            // Add line to the gradient path
            if (lineTension == 0)
            {
                areaPath.AddLine(points[pointIndex - 1], points[pointIndex]);
            }
            else
            {
                areaPath = SkiaSharpExtensions.CreateSpline(areaPath, points);
            }

            // Draw area border line
            if (pointBorderWidth > 0 && pointBorderColor != SKColor.Empty)
            {
                using SKPaint pen = new() { Color = (pointBorderColor != SKColor.Empty) ? pointBorderColor : pointColor, StrokeWidth = pointBorderWidth };
                pen.PathEffect = ChartGraphics.GetPenStyle(pointBorderDashStyle, pointBorderWidth);

                // Set Rounded Cap
                pen.StrokeCap = SKStrokeCap.Round;

                if (lineTension == 0)
                {
                    graph.DrawLine(pen, points[pointIndex - 1], points[pointIndex]);
                }
                else
                {
                    graph.DrawCurve(pen, points, pointIndex - 1, 1, lineTension);
                }
            }

            //************************************************************
            // Hot Regions mode used for image maps, tool tips and 
            // hit test function
            //************************************************************
            if (common.ProcessModeRegions)
            {
                //**************************************************************
                //** Add area for the inside of the area
                //**************************************************************

                // Create grapics path object dor the curve
                SKPath mapAreaPath = new();
                mapAreaPath.AddLine(point1.X, axisPos.Y, point1.X, point1.Y);
                if (lineTension == 0)
                {
                    mapAreaPath.AddLine(points[pointIndex - 1], points[pointIndex]);
                }
                else
                {
                    mapAreaPath = SkiaSharpExtensions.CreateSpline(mapAreaPath, points);
                }
                mapAreaPath.AddLine(point2.X, point2.Y, point2.X, axisPos.Y);
                mapAreaPath.AddLine(point2.X, axisPos.Y, point1.X, axisPos.Y);
                float[] coord = new float[mapAreaPath.PointCount * 2];
                SKPoint[] areaPoints = mapAreaPath.Points;

                // Allocate array of floats
                SKPoint pointNew;
                for (int i = 0; i < mapAreaPath.PointCount; i++)
                {
                    pointNew = graph.GetRelativePoint(areaPoints[i]);
                    coord[2 * i] = pointNew.X;
                    coord[2 * i + 1] = pointNew.Y;
                }

                //************************************************************
                // Hot Regions mode used for image maps, tool tips and 
                // hit test function
                //************************************************************
                common.HotRegionsList.AddHotRegion(
                    mapAreaPath,
                    false,
                    coord,
                    point,
                    series.Name,
                    pointIndex);

                //**************************************************************
                //** Add area for the top line (with thickness)
                //**************************************************************
                if (pointBorderWidth > 1 && pointBorderDashStyle != ChartDashStyle.NotSet && pointBorderColor != SKColor.Empty)
                {
                    try
                    {
                        mapAreaPath.Dispose();
                        // Reset path
                        mapAreaPath = new SKPath();
                        if (lineTension == 0)
                        {
                            mapAreaPath.AddLine(points[pointIndex - 1], points[pointIndex]);
                        }
                        else
                        {
                            mapAreaPath = SkiaSharpExtensions.CreateSpline(mapAreaPath, points);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        // SKPath.Widen incorrectly throws OutOfMemoryException
                        // catching here and reacting by not widening
                    }
                    catch (ArgumentException)
                    {
                        // Do nothing
                    }

                    // Allocate array of floats
                    coord = new float[mapAreaPath.PointCount * 2];
                    SKPoint[] mapAreaPathPoints = mapAreaPath.Points;
                    for (int i = 0; i < mapAreaPathPoints.Length; i++)
                    {
                        pointNew = graph.GetRelativePoint(mapAreaPathPoints[i]);
                        coord[2 * i] = pointNew.X;
                        coord[2 * i + 1] = pointNew.Y;
                    }

                    //************************************************************
                    // Hot Regions mode used for image maps, tool tips and 
                    // hit test function
                    //************************************************************
                    common.HotRegionsList.AddHotRegion(
                        mapAreaPath,
                        false,
                        coord,
                        point,
                        series.Name,
                        pointIndex);

                }
                mapAreaPath.Dispose();
            }
        }

        /// <summary>
        /// Fills last series area with gradient.
        /// </summary>
        /// <param name="graph">The Chart Graphics object</param>
        private void FillLastSeriesGradient(ChartGraphics graph)
        {
            // Add last line in the path
            if (areaPath != null)
            {
                areaPath.AddLine(areaPath.GetLastPoint().X, areaPath.GetLastPoint().Y, areaPath.GetLastPoint().X, axisPos.Y);
            }

            // Fill whole area with gradient
            if (gradientFill && areaPath != null)
            {
                // Set clip region
                graph.SetClip(Area.PlotAreaPosition.ToSKRect());

                // Create brush
                using (SKPaint areaGradientBrush = ChartGraphics.GetGradientBrush(areaPath.GetBounds(), Series.Color, Series.BackSecondaryColor, Series.BackGradientStyle))
                {
                    // Fill area with gradient
                    graph.FillPath(areaGradientBrush, areaPath);
                    gradientFill = false;
                }

                // Reset clip region
                graph.ResetClip();
            }
            if (areaPath != null)
            {
                areaPath.Dispose();
                areaPath = null;
            }
        }

        /// <summary>
        /// Checks if line tension is supported by the chart type.
        /// </summary>
        /// <returns>True if line tension is supported.</returns>
        protected override bool IsLineTensionSupported()
        {
            return false;
        }

        #endregion

        #region 3D painting and selection methods

        /// <summary>
        /// Draws a 3D surface connecting the two specified points in 2D space.
        /// Used to draw Line based charts.
        /// </summary>
        /// <param name="area">Chart area reference.</param>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="prevDataPointEx">Previous data point object.</param>
        /// <param name="positionZ">Z position of the back side of the 3D surface.</param>
        /// <param name="depth">Depth of the 3D surface.</param>
        /// <param name="points">Array of points.</param>
        /// <param name="pointIndex">Index of point to draw.</param>
        /// <param name="pointLoopIndex">Index of points loop.</param>
        /// <param name="tension">Line tension.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
        /// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
        /// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
        /// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
        /// <param name="clippedSegment">Indicates that drawn segment is 3D clipped. Only top/bottom should be drawn.</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        protected override SKPath Draw3DSurface(
            ChartArea area,
            ChartGraphics graph,
            Matrix3D matrix,
            LightStyle lightStyle,
            DataPoint3D prevDataPointEx,
            float positionZ,
            float depth,
            ArrayList points,
            int pointIndex,
            int pointLoopIndex,
            float tension,
            DrawingOperationTypes operationType,
            float topDarkening,
            float bottomDarkening,
            SKPoint thirdPointPosition,
            SKPoint fourthPointPosition,
            bool clippedSegment)
        {
            // Create graphics path for selection
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            //****************************************************************
            //** Find line first and second points.
            //****************************************************************

            // Check if points are drawn from sides to center (do only once)
            if (centerPointIndex == int.MaxValue)
            {
                centerPointIndex = GetCenterPointIndex(points);
            }

            //************************************************************
            //** Find line first & second points
            //************************************************************
            DataPoint3D secondPoint = (DataPoint3D)points[pointIndex];
            int pointArrayIndex = pointIndex;
            DataPoint3D firstPoint = ChartGraphics.FindPointByIndex(
                points,
                secondPoint.index - 1,
                (multiSeries) ? secondPoint : null,
                ref pointArrayIndex);


            //****************************************************************
            //** Switch first and second points.
            //****************************************************************       
            if (firstPoint.index > secondPoint.index)
            {
                DataPoint3D tempPoint = firstPoint;
                firstPoint = secondPoint;
                secondPoint = tempPoint;
            }

            // Points can be drawn from sides to the center.
            // In this case can't use index in the list to find first point.
            // Use point series and real point index to find the first point.
            // Get required point index
            if (matrix.Perspective != 0 && centerPointIndex != int.MaxValue)
            {
                pointArrayIndex = pointIndex;
                if (pointIndex != (centerPointIndex + 1))
                {
                    firstPoint = ChartGraphics.FindPointByIndex(points, secondPoint.index - 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                }
                else
                {
                    if (!area.ReverseSeriesOrder)
                    {
                        secondPoint = ChartGraphics.FindPointByIndex(points, firstPoint.index + 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                    }
                    else
                    {
                        firstPoint = secondPoint;
                        secondPoint = ChartGraphics.FindPointByIndex(points, secondPoint.index - 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                    }
                }
            }

            // Check if points are not null
            if (firstPoint == null || secondPoint == null)
            {
                return resultPath;
            }


            //****************************************************************
            //** Check if reversed drawing order required
            //****************************************************************
            bool reversed = false;
            int indexOffset = 1;
            while ((pointIndex + indexOffset) < points.Count)
            {
                DataPoint3D p = (DataPoint3D)points[pointIndex + indexOffset];
                if (p.dataPoint.series.Name == firstPoint.dataPoint.series.Name)
                {
                    if (p.index == firstPoint.index)
                    {
                        reversed = true;
                    }
                    break;
                }

                ++indexOffset;
            }

            //****************************************************************
            //** Check line tension and draw spline area if non zero.
            //****************************************************************
            // Check tension
            if (tension != 0f)
            {
                // Get spline flatten path
                SKPath splineSurfacePath = graph.GetSplineFlattenPath(
                    area, positionZ,
                    firstPoint, secondPoint, points, tension, true, false, 0);

                // Reversed array of points if needed
                SKPoint[] splinePoints;
                reversed = (pointIndex < pointArrayIndex);
                if (reversed)
                {
                    splineSurfacePath.Reverse();
                }
                splinePoints = splineSurfacePath.Points;

                // Loop through all segment lines the spline consist off
                DataPoint3D dp1 = new();
                DataPoint3D dp2 = new();
                for (int pIndex = 1; pIndex < splinePoints.Length; pIndex++)
                {
                    // Calculate surface coordinates
                    if (!reversed)
                    {
                        dp1.dataPoint = firstPoint.dataPoint;
                        dp1.index = firstPoint.index;
                        dp1.xPosition = splinePoints[pIndex - 1].X;
                        dp1.yPosition = splinePoints[pIndex - 1].Y;

                        dp2.dataPoint = secondPoint.dataPoint;
                        dp2.index = secondPoint.index;
                        dp2.xPosition = splinePoints[pIndex].X;
                        dp2.yPosition = splinePoints[pIndex].Y;
                    }
                    else
                    {
                        dp2.dataPoint = firstPoint.dataPoint;
                        dp2.index = firstPoint.index;
                        dp2.xPosition = splinePoints[pIndex - 1].X;
                        dp2.yPosition = splinePoints[pIndex - 1].Y;

                        dp1.dataPoint = secondPoint.dataPoint;
                        dp1.index = secondPoint.index;
                        dp1.xPosition = splinePoints[pIndex].X;
                        dp1.yPosition = splinePoints[pIndex].Y;
                    }

                    // Get sefment type
                    LineSegmentType surfaceSegmentType = LineSegmentType.Middle;
                    if (pIndex == 1)
                    {
                        if (!reversed)
                            surfaceSegmentType = LineSegmentType.First;
                        else
                            surfaceSegmentType = LineSegmentType.Last;
                    }
                    else if (pIndex == splinePoints.Length - 1)
                    {
                        if (!reversed)
                            surfaceSegmentType = LineSegmentType.Last;
                        else
                            surfaceSegmentType = LineSegmentType.First;
                    }

                    // Draw each segment of the spline area
                    area.IterationCounter = 0;
                    SKPath segmentResultPath = Draw3DSurface(dp1, dp2, reversed,
                        area, graph, matrix, lightStyle, prevDataPointEx,
                        positionZ, depth, points, pointIndex, pointLoopIndex,
                        0f, operationType, surfaceSegmentType,
                        topDarkening, bottomDarkening,
                        new SKPoint(float.NaN, float.NaN),
                        new SKPoint(float.NaN, float.NaN),
                        clippedSegment,
                        true, true);

                    // Add selection path
                    if (resultPath != null && segmentResultPath != null && segmentResultPath.PointCount > 0)
                    {
                        resultPath.AddPath(segmentResultPath);
                    }
                }

                return resultPath;
            }

            // Area point is drawn as one segment
            return Draw3DSurface(firstPoint, secondPoint, reversed,
                area, graph, matrix, lightStyle, prevDataPointEx,
                positionZ, depth, points, pointIndex, pointLoopIndex,
                tension, operationType, LineSegmentType.Single,
                topDarkening, bottomDarkening,
                thirdPointPosition, fourthPointPosition,
                clippedSegment,
                true, true);
        }

        /// <summary>
        /// Draws a 3D surface connecting the two specified points in 2D space.
        /// Used to draw Line based charts.
        /// </summary>
        /// <param name="firstPoint">First data point.</param>
        /// <param name="secondPoint">Second data point.</param>
        /// <param name="reversed">Points are in reversed order.</param>
        /// <param name="area">Chart area reference.</param>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="prevDataPointEx">Previous data point object.</param>
        /// <param name="positionZ">Z position of the back side of the 3D surface.</param>
        /// <param name="depth">Depth of the 3D surface.</param>
        /// <param name="points">Array of points.</param>
        /// <param name="pointIndex">Index of point to draw.</param>
        /// <param name="pointLoopIndex">Index of points loop.</param>
        /// <param name="tension">Line tension.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <param name="surfaceSegmentType">Define surface segment type if it consists of several segments.</param>
        /// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
        /// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
        /// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
        /// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
        /// <param name="clippedSegment">Indicates that drawn segment is 3D clipped. Only top/bottom should be drawn.</param>
        /// <param name="clipOnTop">Indicates that top segment line should be clipped to the pkot area.</param>
        /// <param name="clipOnBottom">Indicates that bottom segment line should be clipped to the pkot area.</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        protected override SKPath Draw3DSurface(
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            bool reversed,
            ChartArea area,
            ChartGraphics graph,
            Matrix3D matrix,
            LightStyle lightStyle,
            DataPoint3D prevDataPointEx,
            float positionZ,
            float depth,
            ArrayList points,
            int pointIndex,
            int pointLoopIndex,
            float tension,
            DrawingOperationTypes operationType,
            LineSegmentType surfaceSegmentType,
            float topDarkening,
            float bottomDarkening,
            SKPoint thirdPointPosition,
            SKPoint fourthPointPosition,
            bool clippedSegment,
            bool clipOnTop,
            bool clipOnBottom)
        {
            // Create graphics path for selection
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            //**********************************************************************
            //** Check surface coordinates
            //**********************************************************************
            if (Math.Round(firstPoint.xPosition, 3) == Math.Round(secondPoint.xPosition, 3) &&
                Math.Round(firstPoint.yPosition, 3) == Math.Round(secondPoint.yPosition, 3))
            {
                return resultPath;
            }

            //****************************************************************
            //** Fint point with line properties
            //****************************************************************
            DataPoint3D pointAttr = secondPoint;
            if (prevDataPointEx.dataPoint.IsEmpty)
            {
                pointAttr = prevDataPointEx;
            }
            else if (firstPoint.index > secondPoint.index)
            {
                pointAttr = firstPoint;
            }

            //****************************************************************
            //** Adjust point visual properties.
            //****************************************************************
            SKColor color = (useBorderColor) ? pointAttr.dataPoint.BorderColor : pointAttr.dataPoint.Color;
            ChartDashStyle dashStyle = pointAttr.dataPoint.BorderDashStyle;
            if (pointAttr.dataPoint.IsEmpty && pointAttr.dataPoint.Color == SKColor.Empty)
            {
                color = SKColors.Gray;
            }
            if (pointAttr.dataPoint.IsEmpty && pointAttr.dataPoint.BorderDashStyle == ChartDashStyle.NotSet)
            {
                dashStyle = ChartDashStyle.Solid;
            }

            //****************************************************************
            //** Get axis position
            //****************************************************************
            float axisPosition = (float)Math.Round(VAxis.GetPosition(VAxis.Crossing), 3);


            //****************************************************************
            //** Detect visibility of the bounding rectangle.
            //****************************************************************
            float minX = (float)Math.Min(firstPoint.xPosition, secondPoint.xPosition);
            float minY = (float)Math.Min(firstPoint.yPosition, secondPoint.yPosition);
            minY = Math.Min(minY, axisPosition);
            float maxX = (float)Math.Max(firstPoint.xPosition, secondPoint.xPosition);
            float maxY = (float)Math.Max(firstPoint.yPosition, secondPoint.yPosition);
            maxY = Math.Max(maxY, axisPosition);
            SKRect position = new(minX, minY, maxX - minX, maxY - minY);
            SurfaceNames visibleSurfaces = graph.GetVisibleSurfaces(position, positionZ, depth, matrix);

            // Check if area point is drawn upside down.
            bool upSideDown = false;
            if (((decimal)firstPoint.yPosition) >= ((decimal)axisPosition) &&
                ((decimal)secondPoint.yPosition) >= ((decimal)axisPosition))
            {
                upSideDown = true;

                // Switch visibility between Top & Bottom surfaces
                bool topVisible = ((visibleSurfaces & SurfaceNames.Top) == SurfaceNames.Top);
                bool bottomVisible = ((visibleSurfaces & SurfaceNames.Bottom) == SurfaceNames.Bottom);
                visibleSurfaces ^= SurfaceNames.Bottom;
                visibleSurfaces ^= SurfaceNames.Top;
                if (topVisible)
                {
                    visibleSurfaces |= SurfaceNames.Bottom;
                }
                if (bottomVisible)
                {
                    visibleSurfaces |= SurfaceNames.Top;
                }
            }

            // Get visibility of the top surface (different from bounding rectangle)
            GetTopSurfaceVisibility(
                area,
                firstPoint,
                secondPoint,
                upSideDown,
                positionZ,
                depth,
                matrix,
                ref visibleSurfaces);

            //****************************************************************
            //** Calculate position of top/bootom points.
            //****************************************************************
            GetBottomPointsPosition(
                Common,
                area,
                axisPosition,
                ref firstPoint,
                ref secondPoint,
                thirdPointPosition,
                fourthPointPosition,
                out SKPoint thirdPoint,
                out SKPoint fourthPoint);

            // Check if point's position provided as parameter
            if (!float.IsNaN(thirdPointPosition.Y))
            {
                thirdPoint.Y = thirdPointPosition.Y;
            }
            if (!float.IsNaN(fourthPointPosition.Y))
            {
                fourthPoint.Y = fourthPointPosition.Y;
            }

            if (float.IsNaN(thirdPoint.X) ||
                float.IsNaN(thirdPoint.Y) ||
                float.IsNaN(fourthPoint.X) ||
                float.IsNaN(fourthPoint.Y))
            {
                return resultPath;
            }

            //****************************************************************
            //** Clip area first and second data points inside 
            //** the plotting area.
            //****************************************************************
            if (clipOnTop && ClipTopPoints(
                resultPath,
                ref firstPoint,
                ref secondPoint,
                reversed,
                area,
                graph,
                matrix,
                lightStyle,
                prevDataPointEx,
                positionZ,
                depth,
                points,
                pointIndex,
                pointLoopIndex,
                tension,
                operationType,
                surfaceSegmentType,
                topDarkening,
                bottomDarkening
                ))
            {
                return resultPath;
            }

            //****************************************************************
            //** Clip area third and fourth data points inside 
            //** the plotting area.
            //****************************************************************
            if (clipOnBottom && ClipBottomPoints(
                resultPath,
                ref firstPoint,
                ref secondPoint,
                ref thirdPoint,
                ref fourthPoint,
                reversed,
                area,
                graph,
                matrix,
                lightStyle,
                prevDataPointEx,
                positionZ,
                depth,
                points,
                pointIndex,
                pointLoopIndex,
                tension,
                operationType,
                surfaceSegmentType,
                topDarkening,
                bottomDarkening
                ))
            {
                return resultPath;
            }


            //****************************************************************
            //** Check if area points are on the different sides of the axis.
            //** In this case split area point into two points where the line
            //** intersects the axis line
            //****************************************************************
            if ((Math.Round((decimal)firstPoint.yPosition, 3) > (decimal)axisPosition + 0.001M && Math.Round((decimal)secondPoint.yPosition, 3) < (decimal)axisPosition - 0.001M) ||
                (Math.Round((decimal)firstPoint.yPosition, 3) < (decimal)axisPosition - 0.001M && Math.Round((decimal)secondPoint.yPosition, 3) > (decimal)axisPosition + 0.001M))
            {
                // Find intersection point
                DataPoint3D intersectionPoint = GetAxisIntersection(firstPoint, secondPoint, axisPosition);

                for (int segmentIndex = 0; segmentIndex <= 1; segmentIndex++)
                {
                    SKPath segmentPath = null;
                    if (segmentIndex == 0 && !reversed ||
                        segmentIndex == 1 && reversed)
                    {
                        // Draw first segment
                        intersectionPoint.dataPoint = secondPoint.dataPoint;
                        intersectionPoint.index = secondPoint.index;
                        segmentPath = Draw3DSurface(firstPoint, intersectionPoint, reversed,
                            area, graph, matrix, lightStyle, prevDataPointEx,
                            positionZ, depth, points, pointIndex, pointLoopIndex,
                            tension, operationType, surfaceSegmentType,
                            topDarkening, bottomDarkening,
                            new SKPoint(float.NaN, float.NaN),
                            new SKPoint(float.NaN, float.NaN),
                            clippedSegment,
                            clipOnTop, clipOnBottom);
                    }

                    if (segmentIndex == 1 && !reversed ||
                        segmentIndex == 0 && reversed)
                    {
                        // Draw second segment
                        intersectionPoint.dataPoint = firstPoint.dataPoint;
                        intersectionPoint.index = firstPoint.index;
                        segmentPath = Draw3DSurface(intersectionPoint, secondPoint, reversed,
                            area, graph, matrix, lightStyle, prevDataPointEx,
                            positionZ, depth, points, pointIndex, pointLoopIndex,
                            tension, operationType, surfaceSegmentType,
                            topDarkening, bottomDarkening,
                            new SKPoint(float.NaN, float.NaN),
                            new SKPoint(float.NaN, float.NaN),
                            clippedSegment,
                            clipOnTop, clipOnBottom);
                    }

                    // Add segment path
                    if (resultPath != null && segmentPath != null && segmentPath.PointCount > 0)
                    {
                        resultPath.AddPath(segmentPath);
                    }
                }

                return resultPath;
            }

            //**********************************************************************
            //** Check surface coordinates
            //**********************************************************************
            if (Math.Round(firstPoint.xPosition, 3) == Math.Round(secondPoint.xPosition, 3) &&
                Math.Round(firstPoint.yPosition, 3) == Math.Round(secondPoint.yPosition, 3))
            {
                return resultPath;
            }

            //****************************************************************
            //** Draw elements of area chart in 2 layers (back & front)
            //****************************************************************
            for (int elemLayer = 1; elemLayer <= 2; elemLayer++)
            {
                // Loop through all surfaces
                SurfaceNames[] surfacesOrder = new SurfaceNames[] { SurfaceNames.Back, SurfaceNames.Bottom, SurfaceNames.Top, SurfaceNames.Left, SurfaceNames.Right, SurfaceNames.Front };
                LineSegmentType lineSegmentType = LineSegmentType.Middle;
                foreach (SurfaceNames currentSurface in surfacesOrder)
                {
                    // Check if surface should be drawn
                    if (ChartGraphics.ShouldDrawLineChartSurface(area, area.ReverseSeriesOrder, currentSurface, visibleSurfaces, color,
                        points, firstPoint, secondPoint, multiSeries, ref lineSegmentType) != elemLayer)
                    {
                        continue;
                    }

                    // To solvce segments overlapping issues with semi-transparent colors ->
                    // Draw invisible surfaces in the first loop, visible in second
                    if (allPointsLoopsNumber == 2 && (operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement)
                    {
                        if (pointLoopIndex == 0 && (currentSurface == SurfaceNames.Front ||
                                elemLayer == 2 && (currentSurface == SurfaceNames.Left || currentSurface == SurfaceNames.Right)))
                        {
                            continue;
                        }
                        if (pointLoopIndex == 1 && (currentSurface == SurfaceNames.Back ||
                                currentSurface != SurfaceNames.Front))
                        {
                            if (elemLayer == 1)
                            {
                                continue;
                            }
                            else if (currentSurface != SurfaceNames.Left && currentSurface != SurfaceNames.Right)
                            {
                                continue;
                            }
                        }
                    }

                    // Draw only borders of the invisible elements on the back layer
                    SKColor surfaceColor = color;
                    SKColor surfaceBorderColor = pointAttr.dataPoint.BorderColor;
                    if (elemLayer == 1)
                    {
                        // Draw only if point color is semi-transparent
                        if (surfaceColor.Alpha == 255)
                        {
                            continue;
                        }

                        // Define drawing colors
                        surfaceColor = SKColors.Transparent;
                        if (surfaceBorderColor == SKColor.Empty)
                        {
                            // If border color is emty use color slightly darker than main back color
                            surfaceBorderColor = ChartGraphics.GetGradientColor(color, SKColors.Black, 0.2);
                        }
                    }

                    // Check if marker lines should be drawn
                    bool forceThinLines = showPointLines;
                    if (surfaceSegmentType == LineSegmentType.Middle)
                    {
                        forceThinLines = false;
                    }

                    // 3D clipped segment is drawn - draw only top & bottom
                    if (clippedSegment &&
                        currentSurface != SurfaceNames.Top &&
                        currentSurface != SurfaceNames.Bottom)
                    {
                        continue;
                    }

                    // Draw surfaces
                    SKPath surfacePath = null;
                    switch (currentSurface)
                    {
                        case (SurfaceNames.Top):
                            {
                                // Darken colors
                                SKColor topColor = (topDarkening == 0f) ? surfaceColor : ChartGraphics.GetGradientColor(surfaceColor, SKColors.Black, topDarkening);
                                SKColor topBorderColor = (topDarkening == 0f) ? surfaceBorderColor : ChartGraphics.GetGradientColor(surfaceBorderColor, SKColors.Black, topDarkening);

                                surfacePath = graph.Draw3DSurface(area, matrix, lightStyle, currentSurface, positionZ, depth,
                                    topColor, topBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle,
                                    firstPoint, secondPoint, points, pointIndex,
                                    0f, operationType, surfaceSegmentType,
                                    forceThinLines, false, area.ReverseSeriesOrder, multiSeries, 0, true);

                                break;
                            }
                        case (SurfaceNames.Bottom):
                            {
                                // Calculate coordinates
                                DataPoint3D dp1 = new()
                                {
                                    index = firstPoint.index,
                                    dataPoint = firstPoint.dataPoint,
                                    xPosition = firstPoint.xPosition,
                                    yPosition = thirdPoint.Y
                                };
                                DataPoint3D dp2 = new()
                                {
                                    index = secondPoint.index,
                                    dataPoint = secondPoint.dataPoint,
                                    xPosition = secondPoint.xPosition,
                                    yPosition = fourthPoint.Y
                                };

                                // Darken colors
                                SKColor bottomColor = (bottomDarkening == 0f) ? surfaceColor : ChartGraphics.GetGradientColor(surfaceColor, SKColors.Black, topDarkening);
                                SKColor bottomBorderColor = (bottomDarkening == 0f) ? surfaceBorderColor : ChartGraphics.GetGradientColor(surfaceBorderColor, SKColors.Black, topDarkening);

                                // Draw surface
                                surfacePath = graph.Draw3DSurface(area, matrix, lightStyle, currentSurface, positionZ, depth,
                                    bottomColor, bottomBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle,
                                    dp1, dp2, points, pointIndex,
                                    0f, operationType, surfaceSegmentType,
                                    forceThinLines, false, area.ReverseSeriesOrder, multiSeries, 0, true);

                                break;
                            }

                        case (SurfaceNames.Left):
                            {
                                if (surfaceSegmentType == LineSegmentType.Single ||
                                    (!area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.First) ||
                                    (area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.Last))
                                {

                                    // Calculate coordinates
                                    DataPoint3D leftMostPoint = (firstPoint.xPosition <= secondPoint.xPosition) ? firstPoint : secondPoint;
                                    DataPoint3D dp1 = new()
                                    {
                                        index = leftMostPoint.index,
                                        dataPoint = leftMostPoint.dataPoint,
                                        xPosition = leftMostPoint.xPosition,
                                        yPosition = (firstPoint.xPosition <= secondPoint.xPosition) ? thirdPoint.Y : fourthPoint.Y
                                    };
                                    DataPoint3D dp2 = new()
                                    {
                                        index = leftMostPoint.index,
                                        dataPoint = leftMostPoint.dataPoint,
                                        xPosition = leftMostPoint.xPosition
                                    };
                                    
                                    dp2.yPosition = leftMostPoint.yPosition;

                                    // Draw surface
                                    surfacePath = graph.Draw3DSurface(area, matrix, lightStyle, currentSurface, positionZ, depth,
                                        surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle,
                                        dp1, dp2, points, pointIndex,
                                        0f, operationType, LineSegmentType.Single, true, true, area.ReverseSeriesOrder, multiSeries, 0, true);

                                }
                                break;
                            }
                        case (SurfaceNames.Right):
                            {
                                if (surfaceSegmentType == LineSegmentType.Single ||
                                    (!area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.Last) ||
                                    (area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.First))

                                {
                                    // Calculate coordinates
                                    DataPoint3D rightMostPoint = (secondPoint.xPosition >= firstPoint.xPosition) ? secondPoint : firstPoint;
                                    DataPoint3D dp1 = new()
                                    {
                                        index = rightMostPoint.index,
                                        dataPoint = rightMostPoint.dataPoint,
                                        xPosition = rightMostPoint.xPosition,
                                        yPosition = (secondPoint.xPosition >= firstPoint.xPosition) ? fourthPoint.Y : thirdPoint.Y
                                    };
                                    DataPoint3D dp2 = new()
                                    {
                                        index = rightMostPoint.index,
                                        dataPoint = rightMostPoint.dataPoint,
                                        xPosition = rightMostPoint.xPosition,
                                        yPosition = rightMostPoint.yPosition
                                    };

                                    // Draw surface
                                    surfacePath = graph.Draw3DSurface(area, matrix, lightStyle, currentSurface, positionZ, depth,
                                        surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle,
                                        dp1, dp2, points, pointIndex,
                                        0f, operationType, LineSegmentType.Single, true, true, area.ReverseSeriesOrder, multiSeries, 0, true);

                                }

                                break;
                            }
                        case (SurfaceNames.Back):
                            {
                                // Calculate coordinates
                                DataPoint3D dp1 = new()
                                {
                                    index = firstPoint.index,
                                    dataPoint = firstPoint.dataPoint,
                                    xPosition = firstPoint.xPosition,
                                    yPosition = thirdPoint.Y
                                };
                                DataPoint3D dp2 = new()
                                {
                                    index = secondPoint.index,
                                    dataPoint = secondPoint.dataPoint,
                                    xPosition = secondPoint.xPosition,
                                    yPosition = fourthPoint.Y
                                };

                                // Border line is required on the data point boundary
                                SurfaceNames thinBorderSides = 0;
                                if (forceThinLines)
                                {
                                    if (surfaceSegmentType == LineSegmentType.Single)
                                        thinBorderSides = SurfaceNames.Left | SurfaceNames.Right;
                                    else if (surfaceSegmentType == LineSegmentType.First)
                                        thinBorderSides = SurfaceNames.Left;
                                    else if (surfaceSegmentType == LineSegmentType.Last)
                                        thinBorderSides = SurfaceNames.Right;
                                }


                                // Draw surface
                                surfacePath = graph.Draw3DPolygon(area, matrix, currentSurface, positionZ,
                                    surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth,
                                    firstPoint, secondPoint, dp2, dp1, operationType, lineSegmentType,
                                    thinBorderSides);
                                break;
                            }
                        case (SurfaceNames.Front):
                            {
                                // Calculate coordinates
                                DataPoint3D dp1 = new()
                                {
                                    index = firstPoint.index,
                                    dataPoint = firstPoint.dataPoint,
                                    xPosition = firstPoint.xPosition,
                                    yPosition = thirdPoint.Y
                                };

                                DataPoint3D dp2 = new()
                                {
                                    index = secondPoint.index,
                                    dataPoint = secondPoint.dataPoint,
                                    xPosition = secondPoint.xPosition,
                                    yPosition = fourthPoint.Y
                                };

                                // Change segment type for the reversed series order
                                if (area.ReverseSeriesOrder)
                                {
                                    if (lineSegmentType == LineSegmentType.First)
                                    {
                                        lineSegmentType = LineSegmentType.Last;
                                    }
                                    else if (lineSegmentType == LineSegmentType.Last)
                                    {
                                        lineSegmentType = LineSegmentType.First;
                                    }
                                }

                                if (surfaceSegmentType != LineSegmentType.Single && (surfaceSegmentType == LineSegmentType.Middle ||
                                        (surfaceSegmentType == LineSegmentType.First && lineSegmentType != LineSegmentType.First) ||
                                        (surfaceSegmentType == LineSegmentType.Last && lineSegmentType != LineSegmentType.Last)))
                                {
                                    lineSegmentType = LineSegmentType.Middle;
                                }

                                // Border line is required on the data point boundary
                                SurfaceNames thinBorderSides = 0;
                                if (forceThinLines)
                                {
                                    if (surfaceSegmentType == LineSegmentType.Single)
                                        thinBorderSides = SurfaceNames.Left | SurfaceNames.Right;
                                    else if (surfaceSegmentType == LineSegmentType.First)
                                        thinBorderSides = SurfaceNames.Left;
                                    else if (surfaceSegmentType == LineSegmentType.Last)
                                        thinBorderSides = SurfaceNames.Right;
                                }

                                // Draw surface
                                surfacePath = graph.Draw3DPolygon(area, matrix, currentSurface, positionZ + depth,
                                    surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth,
                                    firstPoint, secondPoint, dp2, dp1, operationType, lineSegmentType,
                                    thinBorderSides);

                                break;
                            }
                    }

                    // Add path of the fully visible surfaces to the result surface
                    if (elemLayer == 2 && resultPath != null && surfacePath != null && surfacePath.PointCount > 0)
                    {
                        resultPath.Close();
                        //resultPath.SetMarkers()
                        resultPath.AddPath(surfacePath);
                    }

                }
            }

            return resultPath;
        }


        /// <summary>
        /// Gets visibility of the top surface.
        /// </summary>
        /// <param name="area">Chart area object.</param>
        /// <param name="firstPoint">First data point of the line.</param>
        /// <param name="secondPoint">Second data point of the line.</param>
        /// <param name="upSideDown">Indicates that Y values of the data points are below axis line.</param>
        /// <param name="positionZ">Z coordinate of the back side of the cube.</param>
        /// <param name="depth">Cube depth.</param>
        /// <param name="matrix">Coordinate transformation matrix.</param>
        /// <param name="visibleSurfaces">Surface visibility reference. Initialized with bounary cube visibility.</param>
        protected virtual void GetTopSurfaceVisibility(
            ChartArea area,
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            bool upSideDown,
            float positionZ,
            float depth,
            Matrix3D matrix,
            ref SurfaceNames visibleSurfaces)
        {
            // If Top surface visibility in bounding rectangle - do not gurantee angled linde visibility
            if ((visibleSurfaces & SurfaceNames.Top) == SurfaceNames.Top)
            {
                visibleSurfaces ^= SurfaceNames.Top;
            }

            // Create top surface coordinates in 3D space
            Point3D[] cubePoints = new Point3D[3];
            if (!area.ReverseSeriesOrder)
            {
                if (!upSideDown && firstPoint.xPosition <= secondPoint.xPosition ||
                    upSideDown && firstPoint.xPosition >= secondPoint.xPosition)
                {
                    cubePoints[0] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ + depth);
                    cubePoints[1] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);
                    cubePoints[2] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
                }
                else
                {
                    cubePoints[0] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ + depth);
                    cubePoints[1] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
                    cubePoints[2] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);
                }
            }
            else
            {
                if (!upSideDown && secondPoint.xPosition <= firstPoint.xPosition ||
                    upSideDown && secondPoint.xPosition >= firstPoint.xPosition)
                {
                    cubePoints[0] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ + depth);
                    cubePoints[1] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
                    cubePoints[2] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);
                }
                else
                {
                    cubePoints[0] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ + depth);
                    cubePoints[1] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);
                    cubePoints[2] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
                }
            }

            // Tranform coordinates 
            matrix.TransformPoints(cubePoints);

            // Check the top side visibility
            if (ChartGraphics.IsSurfaceVisible(cubePoints[0], cubePoints[1], cubePoints[2]))
            {
                visibleSurfaces |= SurfaceNames.Top;
            }
        }

        /// <summary>
        /// Gets intersection point coordinates between point line and axis line.
        /// </summary>
        /// <param name="firstPoint">First data point.</param>
        /// <param name="secondPoint">Second data point.</param>
        /// <param name="axisPosition">Axis line position.</param>
        /// <returns>Intersection point coordinates.</returns>
        internal static DataPoint3D GetAxisIntersection(DataPoint3D firstPoint, DataPoint3D secondPoint, float axisPosition)
        {
            DataPoint3D intersectionPoint = new();
            intersectionPoint.yPosition = axisPosition;
            intersectionPoint.xPosition = (axisPosition - firstPoint.yPosition) *
                (secondPoint.xPosition - firstPoint.xPosition) /
                (secondPoint.yPosition - firstPoint.yPosition) +
                firstPoint.xPosition;
            return intersectionPoint;
        }

        /// <summary>
        /// Gets position ob the bottom points in area chart.
        /// </summary>
        /// <param name="common">Chart common elements.</param>
        /// <param name="area">Chart area the series belongs to.</param>
        /// <param name="axisPosition">Axis position.</param>
        /// <param name="firstPoint">First top point coordinates.</param>
        /// <param name="secondPoint">Second top point coordinates.</param>
        /// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
        /// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
        /// <param name="thirdPoint">Returns third bottom point coordinates.</param>
        /// <param name="fourthPoint">Returns fourth bottom point coordinates.</param>
        protected virtual void GetBottomPointsPosition(
            CommonElements common,
            ChartArea area,
            float axisPosition,
            ref DataPoint3D firstPoint,
            ref DataPoint3D secondPoint,
            SKPoint thirdPointPosition,
            SKPoint fourthPointPosition,
            out SKPoint thirdPoint,
            out SKPoint fourthPoint)
        {
            thirdPoint = new SKPoint((float)firstPoint.xPosition, axisPosition);
            fourthPoint = new SKPoint((float)secondPoint.xPosition, axisPosition);
        }

        /// <summary>
        /// Returns how many loops through all data points is required (1 or 2)
        /// </summary>
        /// <param name="selection">Selection indicator.</param>
        /// <param name="pointsArray">Points array list.</param>
        /// <returns>Number of loops (1 or 2).</returns>
        override protected int GetPointLoopNumber(bool selection, ArrayList pointsArray)
        {
            // Always one loop for selection
            if (selection)
            {
                return 1;
            }

            // Second loop will be required for semi-transparent colors
            int loopNumber = 1;
            foreach (object obj in pointsArray)
            {
                // Get point & series
                DataPoint3D pointEx = (DataPoint3D)obj;

                // Check properties
                if (pointEx.dataPoint.Color.Alpha != 255)
                {
                    loopNumber = 2;
                }
            }

            return loopNumber;
        }

        #endregion

        #region IDisposable overrides
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && areaPath != null)
            {
                areaPath.Dispose();
                areaPath = null;
            }
            base.Dispose(disposing);
        }
        #endregion

    }
}
