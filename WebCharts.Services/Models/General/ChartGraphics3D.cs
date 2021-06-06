// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	ChartGraphics3D class is 3D chart rendering engine.
//              All chart 3D shapes are drawn in specific order so
//              that correct Z order of all shapes is achieved. 3D
//              graphics engine do not support shapes intersection.
//              3D shapes are transformed into one or more 2D shapes
//              and then drawn with 2D chart graphics engine.
//

using SkiaSharp;
using System;
using System.Collections;

namespace WebCharts.Services
{
    /// <summary>
    /// The ChartGraphics class is 3D chart rendering engine. All chart
    /// 3D shapes are drawn in specific order so that correct Z order
    /// of all shapes is achieved. 3D graphics engine do not support
    /// shapes intersection. 3D shapes are transformed into one or
    /// more 2D shapes and then drawn with 2D chart graphics engine.
    /// </summary>
    public partial class ChartGraphics
    {
        #region Fields

        /// <summary>
        /// Helper field used to store the index of cylinder left/bottom side coordinate.
        /// </summary>
        private int _oppLeftBottomPoint = -1;

        /// <summary>
        /// Helper field used to store the index of cylinder right/top side coordinate.
        /// </summary>
        private int _oppRigthTopPoint = -1;

        /// <summary>
        /// Point of the front line from the previous line segment.
        /// </summary>
        internal SKPoint frontLinePoint1 = SKPoint.Empty;

        /// <summary>
        /// Point of the front line from the previous line segment.
        /// </summary>
        internal SKPoint frontLinePoint2 = SKPoint.Empty;

        /// <summary>
        /// Previous line segment pen.
        /// </summary>
        internal SKPaint frontLinePen = null;

        #endregion Fields

        #region 3D Line drawing methods

        /// <summary>
        /// Draws grid line in 3D space (on two area scene walls)
        /// </summary>
        /// <param name="area">Chart area.</param>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="point1">First line point.</param>
        /// <param name="point2">Second line point.</param>
        /// <param name="horizontal">Indicates that grid line is horizontal</param>
        /// <param name="common">Common Elements</param>
        /// <param name="obj">Selected object</param>
        internal void Draw3DGridLine(
            ChartArea area,
            SKColor color,
            int width,
            ChartDashStyle style,
            SKPoint point1,
            SKPoint point2,
            bool horizontal,
            CommonElements common,
            object obj
            )
        {
            float zPositon = area.IsMainSceneWallOnFront() ? area.areaSceneDepth : 0f;

            ChartElementType chartElementType = obj is StripLine ? ChartElementType.StripLines : ChartElementType.Gridlines;

            // Draw strip line on the back/front wall
            Draw3DLine(
                area.matrix3D,
                color, width, style,
                new Point3D(point1.X, point1.Y, zPositon),
                new Point3D(point2.X, point2.Y, zPositon),
                common,
                obj,
                chartElementType
                );

            if (horizontal)
            {
                // Draw strip line on the side wall (left or right)
                if (area.IsSideSceneWallOnLeft())
                {
                    point1.X = Math.Min(point1.X, point2.X);
                }
                else
                {
                    point1.X = Math.Max(point1.X, point2.X);
                }

                Draw3DLine(
                    area.matrix3D,
                    color, width, style,
                    new Point3D(point1.X, point1.Y, 0f),
                    new Point3D(point1.X, point1.Y, area.areaSceneDepth),
                    common,
                    obj,
                    chartElementType
                    );
            }
            else if (area.IsBottomSceneWallVisible())
            {
                // Draw strip line on the bottom wall (if visible)
                point1.Y = Math.Max(point1.Y, point2.Y);

                Draw3DLine(
                    area.matrix3D,
                    color, width, style,
                    new Point3D(point1.X, point1.Y, 0f),
                    new Point3D(point1.X, point1.Y, area.areaSceneDepth),
                    common,
                    obj,
                    chartElementType
                    );
            }
        }

        /// <summary>
        /// Draws a line connecting the two specified points.
        /// </summary>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="firstPoint">A Point that represents the first point to connect.</param>
        /// <param name="secondPoint">A Point that represents the second point to connect.</param>
        /// <param name="common">Common elements</param>
        /// <param name="obj">Selected object</param>
        /// <param name="type">Selected chart element</param>
        internal void Draw3DLine(
            Matrix3D matrix,
            SKColor color,
            int width,
            ChartDashStyle style,
            Point3D firstPoint,
            Point3D secondPoint,
            CommonElements common,
            object obj,
            ChartElementType type
            )
        {
            // Transform coordinates
            Point3D[] points = new Point3D[] { firstPoint, secondPoint };
            matrix.TransformPoints(points);

            // Selection mode
            if (common.ProcessModeRegions && type != ChartElementType.Nothing)
            {
                using SKPath path = new();

                if (Math.Abs(points[0].X - points[1].X) > Math.Abs(points[0].Y - points[1].Y))
                {
                    path.AddLine(points[0].X, points[0].Y - 1, points[1].X, points[1].Y - 1);
                    path.AddLine(points[1].X, points[1].Y + 1, points[0].X, points[0].Y + 1);
                    path.Close();
                }
                else
                {
                    path.AddLine(points[0].X - 1, points[0].Y, points[1].X - 1, points[1].Y);
                    path.AddLine(points[1].X + 1, points[1].Y, points[0].X + 1, points[0].Y);
                    path.Close();
                }
                common.HotRegionsList.AddHotRegion(path, true, type, obj);
            }

            if (common.ProcessModePaint)
            {
                // Draw 2D line in 3D space
                DrawLineRel(color, width, style, points[0].SKPoint, points[1].SKPoint);
            }
        }

        #endregion 3D Line drawing methods

        #region 3D Pie Drawing methods and enumerations

        /// <summary>
        /// This method draw and fill four point polygons which
        /// represents sides of a pie slice.
        /// </summary>
        /// <param name="area">Chart Area</param>
        /// <param name="inclination">X angle rotation</param>
        /// <param name="startAngle">Start Angle of a pie slice</param>
        /// <param name="sweepAngle">Sweep angle of a pie slice</param>
        /// <param name="points">Significant points of a pie slice</param>
        /// <param name="brush">Brush used for fill</param>
        /// <param name="pen">Pen used for drawing</param>
        /// <param name="doughnut">Chart AxisName is Doughnut</param>
        internal void FillPieSides(
            ChartArea area,
            float inclination,
            float startAngle,
            float sweepAngle,
            SKPoint[] points,
            SKPaint brush,
            SKPaint pen,
            bool doughnut
            )
        {
            // Create a graphics path
            SKPath path;

            // Significant Points for Side polygons
            SKPoint topCenter = points[(int)PiePoints.TopCenter];
            SKPoint bottomCenter = points[(int)PiePoints.BottomCenter];
            SKPoint topStart = points[(int)PiePoints.TopStart];
            SKPoint bottomStart = points[(int)PiePoints.BottomStart];
            SKPoint topEnd = points[(int)PiePoints.TopEnd];
            SKPoint bottomEnd = points[(int)PiePoints.BottomEnd];

            // For Doughnut
            SKPoint topDoughnutStart = SKPoint.Empty;
            SKPoint bottomDoughnutStart = SKPoint.Empty;
            SKPoint topDoughnutEnd = SKPoint.Empty;
            SKPoint bottomDoughnutEnd = SKPoint.Empty;

            if (doughnut)
            {
                // For Doughnut
                topDoughnutStart = points[(int)PiePoints.DoughnutTopStart];
                bottomDoughnutStart = points[(int)PiePoints.DoughnutBottomStart];
                topDoughnutEnd = points[(int)PiePoints.DoughnutTopEnd];
                bottomDoughnutEnd = points[(int)PiePoints.DoughnutBottomEnd];
            }

            bool startSide = false;
            bool endSide = false;
            float endAngle = startAngle + sweepAngle;

            // If X angle is negative different side of pie slice is visible.
            if (inclination > 0)
            {
                // Enable start or/and the end side
                if (startAngle > -90 && startAngle < 90 || startAngle > 270 && startAngle < 450)
                {
                    startSide = true;
                }
                if (endAngle >= -180 && endAngle < -90 || endAngle > 90 && endAngle < 270 || endAngle > 450 && endAngle <= 540)
                {
                    endSide = true;
                }
            }
            else
            {
                // Enable start or/and the end side
                if (startAngle >= -180 && startAngle < -90 || startAngle > 90 && startAngle < 270 || startAngle > 450 && startAngle <= 540)
                {
                    startSide = true;
                }
                if (endAngle > -90 && endAngle < 90 || endAngle > 270 && endAngle < 450)
                {
                    endSide = true;
                }
            }

            if (startSide)
            {
                // *****************************************
                // Draw Start Angle side
                // *****************************************
                using (path = new SKPath())
                {
                    if (doughnut)
                    {
                        // Add Line between The Doughnut Arc and Arc
                        path.AddLine(topDoughnutStart, topStart);

                        // Add Line between The Top Start and Bottom Start
                        path.AddLine(topStart, bottomStart);

                        // Add Line between The Bottom Start and Doughnut Start
                        path.AddLine(bottomStart, bottomDoughnutStart);

                        // Add Line between The Bottom Doughnut Start and The Top Doughnut Start
                        path.AddLine(bottomDoughnutStart, topDoughnutStart);
                    }
                    else
                    {
                        // Add Line between The Center and Arc
                        path.AddLine(topCenter, topStart);

                        // Add Line between The Top Start and Bottom Start
                        path.AddLine(topStart, bottomStart);

                        // Add Line between The Bottom Start and The Center Bottom
                        path.AddLine(bottomStart, bottomCenter);

                        // Add Line between The Bottom Center and The Top Center
                        path.AddLine(bottomCenter, topCenter);
                    }
                    // Get surface colors
                    area.matrix3D.GetLight(
                        brush.Color, 
                        out SKColor frontLightColor, 
                        out SKColor backLightColor, 
                        out SKColor leftLightColor, 
                        out SKColor rightLightColor, 
                        out SKColor topLightColor, 
                        out SKColor bottomLightColor);

                    SKColor lightColor;
                    if (area.Area3DStyle.Inclination < 0)
                    {
                        lightColor = bottomLightColor;
                    }
                    else
                    {
                        lightColor = topLightColor;
                    }

                    // Draw Path
                    using (SKPaint lightBrush = new() { Style = SKPaintStyle.Fill, Color = lightColor })
                    {
                        FillPath(lightBrush, path);
                    }

                    DrawSKPath(pen, path);
                }
            }

            if (endSide)
            {
                // *****************************************
                // Draw End Angle side
                // *****************************************
                using (path = new SKPath())
                {
                    if (doughnut)
                    {
                        // Add Line between The Doughnut Arc and Arc
                        path.AddLine(topDoughnutEnd, topEnd);

                        // Add Line between The Top End and Bottom End
                        path.AddLine(topEnd, bottomEnd);

                        // Add Line between The Bottom End and Doughnut End
                        path.AddLine(bottomEnd, bottomDoughnutEnd);

                        // Add Line between The Bottom Doughnut End and The Top Doughnut End
                        path.AddLine(bottomDoughnutEnd, topDoughnutEnd);
                    }
                    else
                    {
                        // Add Line between The Center and Arc
                        path.AddLine(topCenter, topEnd);

                        // Add Line between The Top End and Bottom End
                        path.AddLine(topEnd, bottomEnd);

                        // Add Line between The Bottom End and The Center Bottom
                        path.AddLine(bottomEnd, bottomCenter);

                        // Add Line between The Bottom Center and The Top Center
                        path.AddLine(bottomCenter, topCenter);
                    }

                    // Get surface colors
                    area.matrix3D.GetLight(
                        brush.Color, 
                        out SKColor frontLightColor, 
                        out SKColor backLightColor,
                        out SKColor leftLightColor,
                        out SKColor rightLightColor, 
                        out SKColor topLightColor, 
                        out SKColor bottomLightColor);

                    SKColor lightColor;
                    if (area.Area3DStyle.Inclination < 0)
                    {
                        lightColor = bottomLightColor;
                    }
                    else
                    {
                        lightColor = topLightColor;
                    }

                    // Draw Path
                    using (SKPaint lightBrush = new() { Color = lightColor, Style = SKPaintStyle.Fill })
                    {
                        FillPath(lightBrush, path);
                    }

                    DrawSKPath(pen, path);
                }
            }
        }

        /// <summary>
        /// This method Draw and fill pie curves.
        /// </summary>
        /// <param name="area">Chart area used for drawing</param>
        /// <param name="point">Data Point</param>
        /// <param name="brush">Graphic Brush used for drawing</param>
        /// <param name="pen">Graphic Pen used for drawing</param>
        /// <param name="topFirstRectPoint">Rotated bounded rectangle points</param>
        /// <param name="topSecondRectPoint">Rotated bounded rectangle points</param>
        /// <param name="bottomFirstRectPoint">Rotated bounded rectangle points</param>
        /// <param name="bottomSecondRectPoint">Rotated bounded rectangle points</param>
        /// <param name="topFirstPoint">Significant pie points</param>
        /// <param name="topSecondPoint">Significant pie points</param>
        /// <param name="bottomFirstPoint">Significant pie points</param>
        /// <param name="bottomSecondPoint">Significant pie points</param>
        /// <param name="startAngle">Start pie angle</param>
        /// <param name="sweepAngle">End pie angle</param>
        /// <param name="pointIndex">Data Point Index</param>
        internal void FillPieCurve(
            ChartArea area,
            DataPoint point,
            SKPaint brush,
            SKPaint pen,
            SKPoint topFirstRectPoint,
            SKPoint topSecondRectPoint,
            SKPoint bottomFirstRectPoint,
            SKPoint bottomSecondRectPoint,
            SKPoint topFirstPoint,
            SKPoint topSecondPoint,
            SKPoint bottomFirstPoint,
            SKPoint bottomSecondPoint,
            float startAngle,
            float sweepAngle,
            int pointIndex
            )
        {
            // Common Elements
            CommonElements common = area.Common;

            // Create a graphics path
            using SKPath path = new();
            // It is enough to transform only two points from
            // rectangle. This code will create SKRect from
            // top left and bottom right points.
            SKRect pieTopRectangle = new(topFirstRectPoint.X, topFirstRectPoint.Y, topSecondRectPoint.X, topSecondRectPoint.Y);
            SKRect pieBottomRectangle = new(bottomFirstRectPoint.X, bottomFirstRectPoint.Y, bottomSecondRectPoint.X, bottomSecondRectPoint.Y);

            // Angle correction algorithm. After rotation AddArc method should used
            // different transformed angles. This method transforms angles.
            double angleCorrection = pieTopRectangle.Height / pieTopRectangle.Width;

            float endAngle;
            endAngle = AngleCorrection(startAngle + sweepAngle, angleCorrection);
            startAngle = AngleCorrection(startAngle, angleCorrection);

            sweepAngle = endAngle - startAngle;

            // Add Line between first points
            path.MoveTo(topFirstPoint);
            path.LineTo(bottomFirstPoint);

            if (pieBottomRectangle.Height <= 0)
            {
                // If x angle is 0 this arc will be line in projection.
                path.MoveTo(bottomFirstPoint.X, bottomFirstPoint.Y);
                path.LineTo(bottomSecondPoint.X, bottomSecondPoint.Y);
            }
            else
            {
                // Add Arc
                path.AddArc(new SKRect(pieBottomRectangle.Left, pieBottomRectangle.Top, pieBottomRectangle.Right, pieBottomRectangle.Bottom), startAngle, sweepAngle);
            }

            // Add Line between second points
            path.MoveTo(bottomSecondPoint);
            path.LineTo(topSecondPoint);

            if (pieTopRectangle.Height <= 0)
            {
                // If x angle is 0 this arc will be line in projection.
                path.MoveTo(topFirstPoint.X, topFirstPoint.Y);
                path.LineTo(topSecondPoint.X, topSecondPoint.Y);
            }
            else
            {
                path.AddArc(new SKRect(pieTopRectangle.Left, pieTopRectangle.Top, pieTopRectangle.Right, pieTopRectangle.Bottom), startAngle + sweepAngle, -sweepAngle);
            }

            if (common.ProcessModePaint)
            {
                // Drawing Mode
                FillPath(brush, path);

                if (point.BorderColor != SKColor.Empty &&
                    point.BorderWidth > 0 &&
                    point.BorderDashStyle != ChartDashStyle.NotSet)
                {
                    DrawSKPath(pen, path);
                }
            }
            if (common.ProcessModeRegions)
            {
                // Check if processing collected data point
                if (point.IsCustomPropertySet("_COLLECTED_DATA_POINT"))
                {
                    // Add point to the map area
                    common.HotRegionsList.AddHotRegion(
                        this,
                        path,
                        false,
                        point.ReplaceKeywords(point.ToolTip),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        point,
                        ChartElementType.DataPoint);

                    return;
                }

                common.HotRegionsList.AddHotRegion(
                    path,
                    false,
                    this,
                    point,
                    point.series.Name,
                    pointIndex);
            }
        }

        /// <summary>
        /// This method draws projection of 3D pie slice.
        /// </summary>
        /// <param name="area">Chart area used for drawing</param>
        /// <param name="point">Data point which creates this pie slice</param>
        /// <param name="brush">Graphic Brush used for drawing</param>
        /// <param name="pen">Graphic Pen used for drawing</param>
        /// <param name="firstRectPoint">The first point of transformed bounding rectangle</param>
        /// <param name="firstPoint">The first arc point of pie slice</param>
        /// <param name="secondRectPoint">The second point of transformed bounding rectangle</param>
        /// <param name="secondPoint">The second arc point of pie slice</param>
        /// <param name="center">The center point of pie slice</param>
        /// <param name="startAngle">Start angle of pie slice</param>
        /// <param name="sweepAngle">The end angle of pie slice</param>
        /// <param name="fill">Fill pie slice with brush</param>
        /// <param name="pointIndex"></param>
        internal void FillPieSlice(ChartArea area, DataPoint point, SKPaint brush, SKPaint pen, SKPoint firstRectPoint, SKPoint firstPoint, SKPoint secondRectPoint, SKPoint secondPoint, SKPoint center, float startAngle, float sweepAngle, bool fill, int pointIndex)
        {
            // Common elements
            CommonElements common = area.Common;

            // Create a graphics path
            using SKPath path = new();

            // It is enough to transform only two points from
            // rectangle. This code will create SKRect from
            // top left and bottom right points.
            SKRect pieRectangle = new();
            pieRectangle.Left = firstRectPoint.X;
            pieRectangle.Top = firstRectPoint.Y;
            pieRectangle.Bottom = pieRectangle.Top + secondRectPoint.Y - firstRectPoint.Y;
            pieRectangle.Right = pieRectangle.Left + secondRectPoint.X - firstRectPoint.X;

            // Angle correction algorithm. After rotation AddArc method should used
            // different transformed angles. This method transforms angles.
            double angleCorrection = pieRectangle.Height / pieRectangle.Width;

            float endAngle;
            endAngle = AngleCorrection(startAngle + sweepAngle, angleCorrection);
            startAngle = AngleCorrection(startAngle, angleCorrection);

            sweepAngle = endAngle - startAngle;

            // Add Line between The Center and Arc
            path.AddLine(center, firstPoint);

            // Add Arc
            if (pieRectangle.Height > 0)
            {
                // If x angle is 0 this arc will be line in projection.
                path.AddArc(new SKRect(pieRectangle.Left, pieRectangle.Top, pieRectangle.Right, pieRectangle.Bottom), startAngle, sweepAngle);
            }

            // Add Line between the end of the arc and the centre.
            path.AddLine(secondPoint, center);

            if (common.ProcessModePaint)
            {
                // Get surface colors
                area.matrix3D.GetLight(brush.Color, out SKColor frontLightColor, out SKColor backLightColor, out SKColor leftLightColor, out SKColor rightLightColor, out SKColor topLightColor, out SKColor bottomLightColor);

                SKPaint newPen = pen.Clone();

                if (area.Area3DStyle.LightStyle == LightStyle.Realistic && point.BorderColor == SKColor.Empty)
                {
                    newPen.Color = frontLightColor;
                }

                // Drawing Mode
                if (fill)
                {
                    using SKPaint lightBrush = new() { Color = frontLightColor, Style = SKPaintStyle.Fill };
                    FillPath(lightBrush, path);
                }

                if (point.BorderColor != SKColor.Empty &&
                    point.BorderWidth > 0 &&
                    point.BorderDashStyle != ChartDashStyle.NotSet)
                {
                    DrawSKPath(newPen, path);
                }
            }

            if (common.ProcessModeRegions && fill)
            {
                // Check if processing collected data point
                if (point.IsCustomPropertySet("_COLLECTED_DATA_POINT"))
                {
                    // Add point to the map area
                    common.HotRegionsList.AddHotRegion(
                        this,
                        path,
                        false,
                        point.ReplaceKeywords(point.ToolTip),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        point,
                        ChartElementType.DataPoint);

                    return;
                }

                common.HotRegionsList.AddHotRegion(path, false, this, point, point.series.Name, pointIndex);
            }
        }

        /// <summary>
        /// This method draws projection of 3D pie slice.
        /// </summary>
        /// <param name="area">Chart area used for drawing</param>
        /// <param name="point">Data point which creates this Doughnut slice</param>
        /// <param name="brush">Graphic Brush used for drawing</param>
        /// <param name="pen">Graphic Pen used for drawing</param>
        /// <param name="firstRectPoint">The first point of transformed bounding rectangle</param>
        /// <param name="firstPoint">The first arc point of Doughnut slice</param>
        /// <param name="secondRectPoint">The second point of transformed bounding rectangle</param>
        /// <param name="secondPoint">The second arc point of Doughnut slice</param>
        /// <param name="threePoint">The three point of Doughnut slice</param>
        /// <param name="fourPoint">The four point of Doughnut slice</param>
        /// <param name="startAngle">Start angle of Doughnut slice</param>
        /// <param name="sweepAngle">The end angle of Doughnut slice</param>
        /// <param name="fill">Fill Doughnut slice with brush</param>
        /// <param name="doughnutRadius">Radius for doughnut chart</param>
        /// <param name="pointIndex">Data Point Index</param>
        internal void FillDoughnutSlice(ChartArea area, DataPoint point, SKPaint brush, SKPaint pen, SKPoint firstRectPoint, SKPoint firstPoint, SKPoint secondRectPoint, SKPoint secondPoint, SKPoint threePoint, SKPoint fourPoint, float startAngle, float sweepAngle, bool fill, float doughnutRadius, int pointIndex)
        {
            // Common Elements
            CommonElements common = area.Common;

            doughnutRadius = 1F - doughnutRadius / 100F;

            // Create a graphics path
            using SKPath path = new();

            // It is enough to transform only two points from
            // rectangle. This code will create SKRect from
            // top left and bottom right points.
            SKRect pieRectangle = new();
            pieRectangle.Left = firstRectPoint.X;
            pieRectangle.Top = firstRectPoint.Y;
            pieRectangle.Bottom = secondRectPoint.Y;
            pieRectangle.Right = secondRectPoint.X;

            SKRect pieDoughnutRectangle = new();
            pieDoughnutRectangle.Left = pieRectangle.Left + pieRectangle.Width * (1F - doughnutRadius) / 2F;
            pieDoughnutRectangle.Top = pieRectangle.Top + pieRectangle.Height * (1F - doughnutRadius) / 2F;
            pieDoughnutRectangle.Size = new(pieRectangle.Width * doughnutRadius, pieRectangle.Height * doughnutRadius);

            // Angle correction algorithm. After rotation AddArc method should used
            // different transformed angles. This method transforms angles.
            double angleCorrection = pieRectangle.Height / pieRectangle.Width;

            float endAngle;
            endAngle = AngleCorrection(startAngle + sweepAngle, angleCorrection);
            startAngle = AngleCorrection(startAngle, angleCorrection);

            sweepAngle = endAngle - startAngle;

            // Add Line between The Doughnut Arc and Arc
            path.AddLine(fourPoint, firstPoint);

            // Add Arc
            if (pieRectangle.Height > 0)
            {
                // If x angle is 0 this arc will be line in projection.
                path.AddArc(pieRectangle, startAngle, sweepAngle);
            }

            // Add Line between the end of the arc and The Doughnut Arc.
            path.AddLine(secondPoint, threePoint);

            // Add Doughnut Arc
            if (pieDoughnutRectangle.Height > 0)
            {
                path.AddArc(pieDoughnutRectangle, startAngle + sweepAngle, -sweepAngle);
            }

            if (common.ProcessModePaint)
            {
                // Get surface colors
                area.matrix3D.GetLight(brush.Color, out SKColor frontLightColor, out SKColor backLightColor,
                    out SKColor leftLightColor, out SKColor rightLightColor, out SKColor topLightColor, out SKColor bottomLightColor);

                var newPen = pen.Clone();

                if (area.Area3DStyle.LightStyle == LightStyle.Realistic && point.BorderColor == SKColor.Empty)
                {
                    newPen.Color = frontLightColor;
                }

                // Drawing Mode
                if (fill)
                {
                    using SKPaint lightBrush = new() { Color = frontLightColor, Style = SKPaintStyle.Fill };
                    FillPath(lightBrush, path);
                }

                if (point.BorderColor != SKColor.Empty &&
                    point.BorderWidth > 0 &&
                    point.BorderDashStyle != ChartDashStyle.NotSet)
                {
                    DrawSKPath(newPen, path);
                }
            }

            if (common.ProcessModeRegions && fill)
            {
                // Check if processing collected data point
                if (point.IsCustomPropertySet("_COLLECTED_DATA_POINT"))
                {
                    // Add point to the map area
                    common.HotRegionsList.AddHotRegion(
                        this,
                        path,
                        false,
                        point.ReplaceKeywords(point.ToolTip),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        point,
                        ChartElementType.DataPoint);

                    return;
                }

                // Add points to the map area
                common.HotRegionsList.AddHotRegion(
                    path,
                    false,
                    this,
                    point,
                    point.series.Name,
                    pointIndex);
            }
        }

        /// <summary>
        /// Draw Graphics Path. This method is introduced because of
        /// bug in DrawPath method when Pen Width is bigger then 1.
        /// </summary>
        /// <param name="pen">Pen</param>
        /// <param name="path">Graphics Path</param>
        private void DrawSKPath(SKPaint pen, SKPath path)
        {
            // Normal case. Very fast Drawing.
            if (pen.StrokeWidth < 2)
            {
                DrawPath(pen, path);
            }
            else
            {
                // Set Pen cap
                pen.StrokeCap = SKStrokeCap.Round;

                SKPoint[] pathPoints;

                pathPoints = path.Points;

                // Draw any segment as a line.
                for (int point = 0; point < path.Points.Length - 1; point++)
                {
                    SKPoint[] points;

                    points = new SKPoint[2];
                    points[0] = pathPoints[point];
                    points[1] = pathPoints[point + 1];

                    DrawLine(pen, points[0], points[1]);
                }
            }
        }

        /// <summary>
        /// Angle correction algorithm. After rotation different
        /// transformed angle should be used. This method transforms angles.
        /// </summary>
        /// <param name="angle">Not transformed angle</param>
        /// <param name="correction">Correction of bounding rectangle (change between width and height)</param>
        /// <returns>Transformed angle</returns>
        private static float AngleCorrection(float angle, double correction)
        {
            // Make all angles to be between -90 and 90.
            if (angle > -90 && angle < 90)
            {
                angle = (float)(Math.Atan(Math.Tan((angle) * Math.PI / 180) * correction) * 180 / Math.PI);
            }
            else if (angle > -270 && angle < -90)
            {
                angle += 180;
                angle = (float)(Math.Atan(Math.Tan(angle * Math.PI / 180) * correction) * 180 / Math.PI);
                angle -= 180;
            }
            else if (angle > 90 && angle < 270)
            {
                angle -= 180;
                angle = (float)(Math.Atan(Math.Tan(angle * Math.PI / 180) * correction) * 180 / Math.PI);
                angle += 180;
            }
            else if (angle > 270 && angle < 450)
            {
                angle -= 360;
                angle = (float)(Math.Atan(Math.Tan((angle) * Math.PI / 180) * correction) * 180 / Math.PI);
                angle += 360;
            }
            else if (angle > 450)
            {
                angle -= 540;
                angle = (float)(Math.Atan(Math.Tan((angle) * Math.PI / 180) * correction) * 180 / Math.PI);
                angle += 540;
            }
            return angle;
        }

        #endregion 3D Pie Drawing methods and enumerations

        #region 3D Surface drawing methods (used in Line charts)

        /// <summary>
        /// Draws a 3D polygon defined by 4 points in 2D space.
        /// </summary>
        /// <param name="area">Chart area reference.</param>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="surfaceName">Name of the surface to draw.</param>
        /// <param name="positionZ">Z position of the back side of the 3D surface.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="firstPoint">First point.</param>
        /// <param name="secondPoint">Second point.</param>
        /// <param name="thirdPoint">Third point.</param>
        /// <param name="fourthPoint">Fourth point.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <param name="lineSegmentType">AxisName of line segment. Used for step lines and splines.</param>
        /// <param name="thinBorders">Thin border will be drawn on specified sides.</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        internal SKPath Draw3DPolygon(
            ChartArea area,
            Matrix3D matrix,
            SurfaceNames surfaceName,
            float positionZ,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            DataPoint3D thirdPoint,
            DataPoint3D fourthPoint,
            DrawingOperationTypes operationType,
            LineSegmentType lineSegmentType,
            SurfaceNames thinBorders)
        {
            // Create graphics path for selection
            bool drawElements = ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement);
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            //**********************************************************************
            //** Prepare, transform polygon coordinates
            //**********************************************************************

            // Define 4 points polygon
            Point3D[] points3D = new Point3D[4];
            points3D[0] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);
            points3D[1] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
            points3D[2] = new Point3D((float)thirdPoint.xPosition, (float)thirdPoint.yPosition, positionZ);
            points3D[3] = new Point3D((float)fourthPoint.xPosition, (float)fourthPoint.yPosition, positionZ);

            // Transform coordinates
            matrix.TransformPoints(points3D);

            // Get absolute coordinates and create array of SKPoint
            SKPoint[] polygonPoints = new SKPoint[4];
            polygonPoints[0] = GetAbsolutePoint(points3D[0].SKPoint);
            polygonPoints[1] = GetAbsolutePoint(points3D[1].SKPoint);
            polygonPoints[2] = GetAbsolutePoint(points3D[2].SKPoint);
            polygonPoints[3] = GetAbsolutePoint(points3D[3].SKPoint);

            //**********************************************************************
            //** Define drawing colors
            //**********************************************************************
            bool topIsVisible = IsSurfaceVisible(points3D[0], points3D[1], points3D[2]);
            SKColor polygonColor = matrix.GetPolygonLight(points3D, backColor, topIsVisible, area.Area3DStyle.Rotation, surfaceName, area.ReverseSeriesOrder);
            SKColor surfaceBorderColor = borderColor;
            if (surfaceBorderColor == SKColor.Empty)
            {
                // If border color is emty use color slightly darker than main back color
                surfaceBorderColor = ChartGraphics.GetGradientColor(backColor, SKColors.Black, 0.2);
            }

            //**********************************************************************
            //** Draw elements if required.
            //**********************************************************************
            SKPaint thickBorderPen = null;
            if (drawElements)
            {
                // Remember SmoothingMode and turn off anti aliasing
                SmoothingMode oldSmoothingMode = SmoothingMode;
                SmoothingMode = SmoothingMode.Default;

                // Draw the polygon
                using (SKPaint brush = new() { Color = polygonColor, Style = SKPaintStyle.Fill })
                {
                    FillPolygon(brush, polygonPoints);
                }

                // Return old smoothing mode
                SmoothingMode = oldSmoothingMode;

                // Draw thin polygon border of darker color around the whole polygon
                if (thinBorders != 0)
                {
                    SKPaint thinLinePen = new() { Color = surfaceBorderColor, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                    if ((thinBorders & SurfaceNames.Left) != 0)
                        DrawLine(thinLinePen, polygonPoints[3], polygonPoints[0]);
                    if ((thinBorders & SurfaceNames.Right) != 0)
                        DrawLine(thinLinePen, polygonPoints[1], polygonPoints[2]);
                    if ((thinBorders & SurfaceNames.Top) != 0)
                        DrawLine(thinLinePen, polygonPoints[0], polygonPoints[1]);
                    if ((thinBorders & SurfaceNames.Bottom) != 0)
                        DrawLine(thinLinePen, polygonPoints[2], polygonPoints[3]);
                }
                else if (polygonColor.Alpha == 255)
                {
                    DrawPolygon(new SKPaint() { Color = polygonColor, StrokeWidth = 1, Style = SKPaintStyle.Stroke }, polygonPoints);
                }

                // Create thick border line pen
                thickBorderPen = new() { Color = surfaceBorderColor, StrokeWidth = borderWidth, Style = SKPaintStyle.Stroke };
                thickBorderPen.StrokeCap = SKStrokeCap.Round;

                // Draw thick Top & Bottom lines
                DrawLine(thickBorderPen, polygonPoints[0], polygonPoints[1]);
                DrawLine(thickBorderPen, polygonPoints[2], polygonPoints[3]);

                // Draw thick Right & Left lines on first & last segments of the line
                if (lineSegmentType == LineSegmentType.First)
                {
                    DrawLine(thickBorderPen, polygonPoints[3], polygonPoints[0]);
                }
                else if (lineSegmentType == LineSegmentType.Last)
                {
                    DrawLine(thickBorderPen, polygonPoints[1], polygonPoints[2]);
                }
            }

            //**********************************************************************
            //** Redraw front line of the previuos line segment.
            //**********************************************************************
            if (area.Area3DStyle.Perspective == 0)
            {
                if (frontLinePoint1 != SKPoint.Empty && frontLinePen != null)
                {
                    if ((frontLinePoint1.X == polygonPoints[0].X &&
                        frontLinePoint1.Y == polygonPoints[0].Y ||
                        frontLinePoint2.X == polygonPoints[1].X &&
                        frontLinePoint2.Y == polygonPoints[1].Y) ||

                        (frontLinePoint1.X == polygonPoints[1].X &&
                        frontLinePoint1.Y == polygonPoints[1].Y ||
                        frontLinePoint2.X == polygonPoints[0].X &&
                        frontLinePoint2.Y == polygonPoints[0].Y) ||

                        (frontLinePoint1.X == polygonPoints[3].X &&
                        frontLinePoint1.Y == polygonPoints[3].Y ||
                        frontLinePoint2.X == polygonPoints[2].X &&
                        frontLinePoint2.Y == polygonPoints[2].Y) ||

                        (frontLinePoint1.X == polygonPoints[2].X &&
                        frontLinePoint1.Y == polygonPoints[2].Y ||
                        frontLinePoint2.X == polygonPoints[3].X &&
                        frontLinePoint2.Y == polygonPoints[3].Y))
                    {
                        // Do not draw the line if it will be overlapped with current
                    }
                    else
                    {
                        // Draw line !!!!
                        DrawLine(
                            frontLinePen,
                            (float)Math.Round(frontLinePoint1.X),
                            (float)Math.Round(frontLinePoint1.Y),
                            (float)Math.Round(frontLinePoint2.X),
                            (float)Math.Round(frontLinePoint2.Y));
                    }

                    // Reset line properties
                    frontLinePen = null;
                    frontLinePoint1 = SKPoint.Empty;
                    frontLinePoint2 = SKPoint.Empty;
                }

                //**********************************************************************
                //** Check if front line should be redrawn whith the next segment.
                //**********************************************************************
                if (drawElements)
                {
                    // Add top line
                    frontLinePen = thickBorderPen;
                    frontLinePoint1 = polygonPoints[0];
                    frontLinePoint2 = polygonPoints[1];
                }
            }

            // Calculate path for selection
            if (resultPath != null)
            {
                // Add polygon to the path
                resultPath.AddPoly(polygonPoints);
            }

            return resultPath;
        }

        /// <summary>
        /// Helper method which returns the splines flatten path.
        /// </summary>
        /// <param name="area">Chart area reference.</param>
        /// <param name="positionZ">Z position of the back side of the 3D surface.</param>
        /// <param name="firstPoint">First point.</param>
        /// <param name="secondPoint">Second point.</param>
        /// <param name="points">Array of points.</param>
        /// <param name="tension">Line tension.</param>
        /// <param name="flatten">Flatten result path.</param>
        /// <param name="translateCoordinates">Indicates that points coordinates should be translated.</param>
        /// <param name="yValueIndex">Index of the Y value to use.</param>
        /// <returns>Spline path.</returns>
        internal SKPath GetSplineFlattenPath(
            ChartArea area,
            float positionZ,
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            ArrayList points,
            float tension,
            bool flatten,
            bool translateCoordinates,
            int yValueIndex)
        {
            // Find first spline point index
            int firtsSplinePointIndex = (firstPoint.index < secondPoint.index) ? firstPoint.index : secondPoint.index;
            --firtsSplinePointIndex;
            if (firtsSplinePointIndex >= (points.Count - 2))
            {
                --firtsSplinePointIndex;
            }
            if (firtsSplinePointIndex < 1)
            {
                firtsSplinePointIndex = 1;
            }

            // Find four points which are required to draw the spline
            int pointArrayIndex = int.MinValue;
            DataPoint3D[] splineDataPoints = new DataPoint3D[4];
            splineDataPoints[0] = FindPointByIndex(points, firtsSplinePointIndex, null, ref pointArrayIndex);
            splineDataPoints[1] = FindPointByIndex(points, firtsSplinePointIndex + 1, null, ref pointArrayIndex);
            splineDataPoints[2] = FindPointByIndex(points, firtsSplinePointIndex + 2, null, ref pointArrayIndex);
            splineDataPoints[3] = FindPointByIndex(points, firtsSplinePointIndex + 3, null, ref pointArrayIndex);

            // Get offset of spline segment in array
            int splineSegmentOffset = 0;
            while (splineSegmentOffset < 4)
            {
                if (splineDataPoints[splineSegmentOffset].index == firstPoint.index ||
                    splineDataPoints[splineSegmentOffset].index == secondPoint.index)
                {
                    break;
                }
                ++splineSegmentOffset;
            }

            // Get number of found points
            int nonNullPoints = 2;
            if (splineDataPoints[2] != null)
                ++nonNullPoints;
            if (splineDataPoints[3] != null)
                ++nonNullPoints;

            // Get coordinates and create array of SKPoint for the front spline
            SKPoint[] polygonPointsFront = new SKPoint[nonNullPoints];
            if (yValueIndex == 0)
            {
                polygonPointsFront[0] = new SKPoint((float)splineDataPoints[0].xPosition, (float)splineDataPoints[0].yPosition);
                polygonPointsFront[1] = new SKPoint((float)splineDataPoints[1].xPosition, (float)splineDataPoints[1].yPosition);
                if (nonNullPoints > 2)
                    polygonPointsFront[2] = new SKPoint((float)splineDataPoints[2].xPosition, (float)splineDataPoints[2].yPosition);
                if (nonNullPoints > 3)
                    polygonPointsFront[3] = new SKPoint((float)splineDataPoints[3].xPosition, (float)splineDataPoints[3].yPosition);
            }
            else
            {
                // Set active vertical axis
                Axis vAxis = (firstPoint.dataPoint.series.YAxisType == AxisType.Primary) ? area.AxisY : area.AxisY2;

                float secondYValue = (float)vAxis.GetPosition(splineDataPoints[0].dataPoint.YValues[yValueIndex]);
                polygonPointsFront[0] = new SKPoint((float)splineDataPoints[0].xPosition, secondYValue);
                secondYValue = (float)vAxis.GetPosition(splineDataPoints[1].dataPoint.YValues[yValueIndex]);
                polygonPointsFront[1] = new SKPoint((float)splineDataPoints[1].xPosition, secondYValue);
                if (nonNullPoints > 2)
                {
                    secondYValue = (float)vAxis.GetPosition(splineDataPoints[2].dataPoint.YValues[yValueIndex]);
                    polygonPointsFront[2] = new SKPoint((float)splineDataPoints[2].xPosition, secondYValue);
                }
                if (nonNullPoints > 3)
                {
                    secondYValue = (float)vAxis.GetPosition(splineDataPoints[3].dataPoint.YValues[yValueIndex]);
                    polygonPointsFront[3] = new SKPoint((float)splineDataPoints[3].xPosition, secondYValue);
                }
            }

            // Translate points coordinates in 3D space and get absolute coordinate
            if (translateCoordinates)
            {
                // Prepare array of points
                Point3D[] points3D = new Point3D[nonNullPoints];
                for (int index = 0; index < nonNullPoints; index++)
                {
                    points3D[index] = new Point3D(polygonPointsFront[index].X, polygonPointsFront[index].Y, positionZ);
                }

                // Make coordinates transformation
                area.matrix3D.TransformPoints(points3D);

                // Get absolute values
                for (int index = 0; index < nonNullPoints; index++)
                {
                    polygonPointsFront[index] = GetAbsolutePoint(points3D[index].SKPoint);
                }
            }

            // Create graphics path for the front spline surface and flatten it.
            SKPath splineSurfacePath = new();
            splineSurfacePath.AddPath(SkiaSharpExtensions.CreateSpline(polygonPointsFront));

            // IsReversed points order
            if (firstPoint.index > secondPoint.index)
            {
                splineSurfacePath.Reverse();
            }

            return splineSurfacePath;
        }

        /// <summary>
        /// Draws a 3D spline surface connecting the two specified points in 2D space.
        /// Used to draw Spline based charts.
        /// </summary>
        /// <param name="area">Chart area reference.</param>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="surfaceName">Name of the surface to draw.</param>
        /// <param name="positionZ">Z position of the back side of the 3D surface.</param>
        /// <param name="depth">Depth of the 3D surface.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="firstPoint">First point.</param>
        /// <param name="secondPoint">Second point.</param>
        /// <param name="points">Array of points.</param>
        /// <param name="pointIndex">Index of point to draw.</param>
        /// <param name="tension">Line tension.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <param name="forceThinBorder">Thin border will be drawn on all segments.</param>
        /// <param name="forceThickBorder">Thick border will be drawn on all segments.</param>
        /// <param name="reversedSeriesOrder">Series are drawn in reversed order.</param>
        /// <param name="multiSeries">Multiple series are drawn at the same time.</param>
        /// <param name="yValueIndex">Index of the Y value to use.</param>
        /// <param name="clipInsideArea">Surface should be clipped inside plotting area.</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        internal SKPath Draw3DSplineSurface(
            ChartArea area,
            Matrix3D matrix,
            LightStyle lightStyle,
            SurfaceNames surfaceName,
            float positionZ,
            float depth,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            ArrayList points,
            int pointIndex,
            float tension,
            DrawingOperationTypes operationType,
            bool forceThinBorder,
            bool forceThickBorder,
            bool reversedSeriesOrder,
            bool multiSeries,
            int yValueIndex,
            bool clipInsideArea)
        {
            // If zero tension is specified - draw a Line Surface
            if (tension == 0f)
            {
                return Draw3DSurface(
                    area,
                    matrix,
                    lightStyle,
                    surfaceName,
                    positionZ,
                    depth,
                    backColor,
                    borderColor,
                    borderWidth,
                    borderDashStyle,
                    firstPoint,
                    secondPoint,
                    points,
                    pointIndex,
                    tension,
                    operationType,
                    LineSegmentType.Single,
                    forceThinBorder,
                    forceThickBorder,
                    reversedSeriesOrder,
                    multiSeries,
                    yValueIndex,
                    clipInsideArea);
            }

            // Create graphics path for selection
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            // Get spline flatten path
            SKPath splineSurfacePath = GetSplineFlattenPath(
                area, positionZ,
                firstPoint, secondPoint, points, tension, true, false, yValueIndex);

            // Check if reversed drawing order required
            bool reversed = false;
            if ((pointIndex + 1) < points.Count)
            {
                DataPoint3D p = (DataPoint3D)points[pointIndex + 1];
                if (p.index == firstPoint.index)
                {
                    reversed = true;
                }
            }

            if (reversed)
            {
                splineSurfacePath.Reverse();
            }

            // Loop through all segment lines the spline consists off
            SKPoint[] splinePathPoints = splineSurfacePath.Points;
            DataPoint3D dp1 = new();
            DataPoint3D dp2 = new();
            for (int pIndex = 1; pIndex < splinePathPoints.Length; pIndex++)
            {
                bool forceSegmentThinBorder = false;
                bool forceSegmentThickBorder = false;

                // Calculate surface coordinates
                if (!reversed)
                {
                    dp1.index = firstPoint.index;
                    dp1.dataPoint = firstPoint.dataPoint;
                    dp1.xPosition = splinePathPoints[pIndex - 1].X;
                    dp1.yPosition = splinePathPoints[pIndex - 1].Y;

                    dp2.index = secondPoint.index;
                    dp2.index = secondPoint.index;
                    dp2.xPosition = splinePathPoints[pIndex].X;
                    dp2.yPosition = splinePathPoints[pIndex].Y;
                }
                else
                {
                    dp2.index = firstPoint.index;
                    dp2.dataPoint = firstPoint.dataPoint;
                    dp2.xPosition = splinePathPoints[pIndex - 1].X;
                    dp2.yPosition = splinePathPoints[pIndex - 1].Y;

                    dp1.index = secondPoint.index;
                    dp1.dataPoint = secondPoint.dataPoint;
                    dp1.xPosition = splinePathPoints[pIndex].X;
                    dp1.yPosition = splinePathPoints[pIndex].Y;
                }

                // Get sefment type
                LineSegmentType lineSegmentType = LineSegmentType.Middle;
                if (pIndex == 1)
                {
                    if (!reversed)
                        lineSegmentType = LineSegmentType.First;
                    else
                        lineSegmentType = LineSegmentType.Last;

                    forceSegmentThinBorder = forceThinBorder;
                    forceSegmentThickBorder = forceThickBorder;
                }
                else if (pIndex == splinePathPoints.Length - 1)
                {
                    if (!reversed)
                        lineSegmentType = LineSegmentType.Last;
                    else
                        lineSegmentType = LineSegmentType.First;

                    forceSegmentThinBorder = forceThinBorder;
                    forceSegmentThickBorder = forceThickBorder;
                }

                // Draw flat surface
                SKPath segmentResultPath = Draw3DSurface(
                    area,
                    matrix,
                    lightStyle,
                    surfaceName,
                    positionZ,
                    depth,
                    backColor,
                    borderColor,
                    borderWidth,
                    borderDashStyle,
                    dp1,
                    dp2,
                    points,
                    pointIndex,
                    0f,
                    operationType,
                    lineSegmentType,
                    forceSegmentThinBorder,
                    forceSegmentThickBorder,
                    reversedSeriesOrder,
                    multiSeries,
                    yValueIndex,
                    clipInsideArea);

                // Add selection path
                if (resultPath != null && segmentResultPath != null && segmentResultPath.PointCount > 0)
                {
                    resultPath.AddPath(segmentResultPath);
                }
            }

            return resultPath;
        }

        /// <summary>
        /// Draws a 3D surface connecting the two specified points in 2D space.
        /// Used to draw Line based charts.
        /// </summary>
        /// <param name="area">Chart area reference.</param>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="surfaceName">Name of the surface to draw.</param>
        /// <param name="positionZ">Z position of the back side of the 3D surface.</param>
        /// <param name="depth">Depth of the 3D surface.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="firstPoint">First point.</param>
        /// <param name="secondPoint">Second point.</param>
        /// <param name="points">Array of points.</param>
        /// <param name="pointIndex">Index of point to draw.</param>
        /// <param name="tension">Line tension.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <param name="lineSegmentType">AxisName of line segment. Used for step lines and splines.</param>
        /// <param name="forceThinBorder">Thin border will be drawn on all segments.</param>
        /// <param name="forceThickBorder">Thick border will be drawn on all segments.</param>
        /// <param name="reversedSeriesOrder">Series are drawn in reversed order.</param>
        /// <param name="multiSeries">Multiple series are drawn at the same time.</param>
        /// <param name="yValueIndex">Index of the Y value to use.</param>
        /// <param name="clipInsideArea">Surface should be clipped inside plotting area.</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        internal SKPath Draw3DSurface(
            ChartArea area,
            Matrix3D matrix,
            LightStyle lightStyle,
            SurfaceNames surfaceName,
            float positionZ,
            float depth,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            ArrayList points,
            int pointIndex,
            float tension,
            DrawingOperationTypes operationType,
            LineSegmentType lineSegmentType,
            bool forceThinBorder,
            bool forceThickBorder,
            bool reversedSeriesOrder,
            bool multiSeries,
            int yValueIndex,
            bool clipInsideArea)
        {
            // If non-zero tension is specified - draw a Spline Surface
            if (tension != 0f)
            {
                return Draw3DSplineSurface(
                    area,
                    matrix,
                    lightStyle,
                    surfaceName,
                    positionZ,
                    depth,
                    backColor,
                    borderColor,
                    borderWidth,
                    borderDashStyle,
                    firstPoint,
                    secondPoint,
                    points,
                    pointIndex,
                    tension,
                    operationType,
                    forceThinBorder,
                    forceThickBorder,
                    reversedSeriesOrder,
                    multiSeries,
                    yValueIndex,
                    clipInsideArea);
            }

            //**********************************************************************
            //** Create graphics path for selection
            //**********************************************************************
            bool drawElements = ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement);
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            //**********************************************************************
            //** Check surface coordinates
            //**********************************************************************
            if ((decimal)firstPoint.xPosition == (decimal)secondPoint.xPosition &&
                (decimal)firstPoint.yPosition == (decimal)secondPoint.yPosition)
            {
                return resultPath;
            }

            //**********************************************************************
            //** Clip surface
            //**********************************************************************

            // Check if line between the first and second points intersects with
            // plotting area top or bottom boundary
            if (clipInsideArea)
            {
                //****************************************************************
                //** Round plot are position and point coordinates
                //****************************************************************
                int decimals = 3;
                decimal plotAreaPositionX = Math.Round((decimal)area.PlotAreaPosition.X, decimals);
                decimal plotAreaPositionY = Math.Round((decimal)area.PlotAreaPosition.Y, decimals);
                decimal plotAreaPositionRight = Math.Round((decimal)area.PlotAreaPosition.Right, decimals);
                decimal plotAreaPositionBottom = Math.Round((decimal)area.PlotAreaPosition.Bottom, decimals);

                // Make area a little bit bigger
                plotAreaPositionX -= 0.001M;
                plotAreaPositionY -= 0.001M;
                plotAreaPositionRight += 0.001M;
                plotAreaPositionBottom += 0.001M;

                // Chech data points X values
                if ((decimal)firstPoint.xPosition < plotAreaPositionX ||
                    (decimal)firstPoint.xPosition > plotAreaPositionRight ||
                    (decimal)secondPoint.xPosition < plotAreaPositionX ||
                    (decimal)secondPoint.xPosition > plotAreaPositionRight)
                {
                    // Check if surface completly out of the plot area
                    if ((decimal)firstPoint.xPosition < plotAreaPositionX &&
                        (decimal)secondPoint.xPosition < plotAreaPositionX)
                    {
                        return resultPath;
                    }
                    // Check if surface completly out of the plot area
                    if ((decimal)firstPoint.xPosition > plotAreaPositionRight &&
                        (decimal)secondPoint.xPosition > plotAreaPositionRight)
                    {
                        return resultPath;
                    }

                    // Only part of the surface is outside - fix X value and adjust Y value
                    if ((decimal)firstPoint.xPosition < plotAreaPositionX)
                    {
                        firstPoint.yPosition = ((double)plotAreaPositionX - secondPoint.xPosition) /
                            (firstPoint.xPosition - secondPoint.xPosition) *
                            (firstPoint.yPosition - secondPoint.yPosition) +
                            secondPoint.yPosition;
                        firstPoint.xPosition = (double)plotAreaPositionX;
                    }
                    else if ((decimal)firstPoint.xPosition > plotAreaPositionRight)
                    {
                        firstPoint.yPosition = ((double)plotAreaPositionRight - secondPoint.xPosition) /
                            (firstPoint.xPosition - secondPoint.xPosition) *
                            (firstPoint.yPosition - secondPoint.yPosition) +
                            secondPoint.yPosition;
                        firstPoint.xPosition = (double)plotAreaPositionRight;
                    }
                    if ((decimal)secondPoint.xPosition < plotAreaPositionX)
                    {
                        secondPoint.yPosition = ((double)plotAreaPositionX - secondPoint.xPosition) /
                            (firstPoint.xPosition - secondPoint.xPosition) *
                            (firstPoint.yPosition - secondPoint.yPosition) +
                            secondPoint.yPosition;
                        secondPoint.xPosition = (double)plotAreaPositionX;
                    }
                    else if ((decimal)secondPoint.xPosition > plotAreaPositionRight)
                    {
                        secondPoint.yPosition = ((double)plotAreaPositionRight - secondPoint.xPosition) /
                            (firstPoint.xPosition - secondPoint.xPosition) *
                            (firstPoint.yPosition - secondPoint.yPosition) +
                            secondPoint.yPosition;
                        secondPoint.xPosition = (double)plotAreaPositionRight;
                    }
                }

                // Chech data points Y values
                if ((decimal)firstPoint.yPosition < plotAreaPositionY ||
                    (decimal)firstPoint.yPosition > plotAreaPositionBottom ||
                    (decimal)secondPoint.yPosition < plotAreaPositionY ||
                    (decimal)secondPoint.yPosition > plotAreaPositionBottom)
                {
                    // Remember previous y positions
                    double prevFirstPointY = firstPoint.yPosition;
                    double prevSecondPointY = secondPoint.yPosition;

                    // Check if whole line is outside plotting region
                    bool surfaceCompletlyOutside = false;
                    if ((decimal)firstPoint.yPosition < plotAreaPositionY &&
                        (decimal)secondPoint.yPosition < plotAreaPositionY)
                    {
                        surfaceCompletlyOutside = true;
                        firstPoint.yPosition = (double)plotAreaPositionY;
                        secondPoint.yPosition = (double)plotAreaPositionY;
                    }
                    if ((decimal)firstPoint.yPosition > plotAreaPositionBottom &&
                        (decimal)secondPoint.yPosition > plotAreaPositionBottom)
                    {
                        surfaceCompletlyOutside = true;
                        firstPoint.yPosition = (double)plotAreaPositionBottom;
                        secondPoint.yPosition = (double)plotAreaPositionBottom;
                    }

                    // Calculate color used to draw "cut" surfaces
                    SKColor cutSurfaceBackColor = ChartGraphics.GetGradientColor(backColor, SKColors.Black, 0.5);
                    SKColor cutSurfaceBorderColor = ChartGraphics.GetGradientColor(borderColor, SKColors.Black, 0.5);

                    // Draw just one surface
                    if (surfaceCompletlyOutside)
                    {
                        resultPath = this.Draw3DSurface(
                            area, matrix, lightStyle, surfaceName, positionZ, depth,
                            cutSurfaceBackColor, cutSurfaceBorderColor, borderWidth, borderDashStyle,
                            firstPoint, secondPoint,
                            points, pointIndex, tension, operationType, lineSegmentType,
                            forceThinBorder, forceThickBorder, reversedSeriesOrder,
                            multiSeries, yValueIndex, clipInsideArea);

                        // Restore previous y positions
                        firstPoint.yPosition = prevFirstPointY;
                        secondPoint.yPosition = prevSecondPointY;

                        return resultPath;
                    }

                    // Get intersection point
                    DataPoint3D intersectionPoint = new();
                    intersectionPoint.yPosition = (double)plotAreaPositionY;
                    if ((decimal)firstPoint.yPosition > plotAreaPositionBottom ||
                        (decimal)secondPoint.yPosition > plotAreaPositionBottom)
                    {
                        intersectionPoint.yPosition = (double)plotAreaPositionBottom;
                    }
                    intersectionPoint.xPosition = (intersectionPoint.yPosition - secondPoint.yPosition) *
                        (firstPoint.xPosition - secondPoint.xPosition) /
                        (firstPoint.yPosition - secondPoint.yPosition) +
                        secondPoint.xPosition;

                    // Check if there are 2 intersection points (3 segments)
                    int segmentNumber = 2;
                    DataPoint3D intersectionPoint2 = null;
                    if (((decimal)firstPoint.yPosition < plotAreaPositionY &&
                        (decimal)secondPoint.yPosition > plotAreaPositionBottom) ||
                        ((decimal)firstPoint.yPosition > plotAreaPositionBottom &&
                        (decimal)secondPoint.yPosition < plotAreaPositionY))
                    {
                        segmentNumber = 3;
                        intersectionPoint2 = new DataPoint3D();
                        if ((decimal)intersectionPoint.yPosition == plotAreaPositionY)
                        {
                            intersectionPoint2.yPosition = (double)plotAreaPositionBottom;
                        }
                        else
                        {
                            intersectionPoint2.yPosition = (double)plotAreaPositionY;
                        }
                        intersectionPoint2.xPosition = (intersectionPoint2.yPosition - secondPoint.yPosition) *
                            (firstPoint.xPosition - secondPoint.xPosition) /
                            (firstPoint.yPosition - secondPoint.yPosition) +
                            secondPoint.xPosition;

                        // Switch intersection points
                        if ((decimal)firstPoint.yPosition > plotAreaPositionBottom)
                        {
                            DataPoint3D tempPoint = new();
                            tempPoint.xPosition = intersectionPoint.xPosition;
                            tempPoint.yPosition = intersectionPoint.yPosition;
                            intersectionPoint.xPosition = intersectionPoint2.xPosition;
                            intersectionPoint.yPosition = intersectionPoint2.yPosition;
                            intersectionPoint2.xPosition = tempPoint.xPosition;
                            intersectionPoint2.yPosition = tempPoint.yPosition;
                        }
                    }

                    // Adjust points Y values
                    bool firstSegmentVisible = true;
                    if ((decimal)firstPoint.yPosition < plotAreaPositionY)
                    {
                        firstSegmentVisible = false;
                        firstPoint.yPosition = (double)plotAreaPositionY;
                    }
                    else if ((decimal)firstPoint.yPosition > plotAreaPositionBottom)
                    {
                        firstSegmentVisible = false;
                        firstPoint.yPosition = (double)plotAreaPositionBottom;
                    }
                    if ((decimal)secondPoint.yPosition < plotAreaPositionY)
                    {
                        secondPoint.yPosition = (double)plotAreaPositionY;
                    }
                    else if ((decimal)secondPoint.yPosition > plotAreaPositionBottom)
                    {
                        secondPoint.yPosition = (double)plotAreaPositionBottom;
                    }

                    // Check if reversed drawing order required
                    bool reversed = false;
                    if ((pointIndex + 1) < points.Count)
                    {
                        DataPoint3D p = (DataPoint3D)points[pointIndex + 1];
                        if (p.index == firstPoint.index)
                        {
                            reversed = true;
                        }
                    }

                    // Draw surfaces in 2 or 3 segments
                    for (int segmentIndex = 0; segmentIndex < 3; segmentIndex++)
                    {
                        SKPath segmentPath = null;
                        if (segmentIndex == 0 && !reversed ||
                            segmentIndex == 2 && reversed)
                        {
                            // Draw first segment
                            if (intersectionPoint2 == null)
                            {
                                intersectionPoint2 = intersectionPoint;
                            }
                            intersectionPoint2.dataPoint = secondPoint.dataPoint;
                            intersectionPoint2.index = secondPoint.index;

                            segmentPath = this.Draw3DSurface(
                                area, matrix, lightStyle, surfaceName, positionZ, depth,
                                (firstSegmentVisible && segmentNumber != 3) ? backColor : cutSurfaceBackColor,
                                (firstSegmentVisible && segmentNumber != 3) ? borderColor : cutSurfaceBorderColor,
                                borderWidth, borderDashStyle,
                                firstPoint, intersectionPoint2,
                                points, pointIndex, tension, operationType, lineSegmentType,
                                forceThinBorder, forceThickBorder, reversedSeriesOrder,
                                multiSeries, yValueIndex, clipInsideArea);
                        }

                        if (segmentIndex == 1 && intersectionPoint2 != null && segmentNumber == 3)
                        {
                            // Draw middle segment
                            intersectionPoint2.dataPoint = secondPoint.dataPoint;
                            intersectionPoint2.index = secondPoint.index;

                            segmentPath = Draw3DSurface(
                                area, matrix, lightStyle, surfaceName, positionZ, depth,
                                backColor,
                                borderColor,
                                borderWidth, borderDashStyle,
                                intersectionPoint, intersectionPoint2,
                                points, pointIndex, tension, operationType, lineSegmentType,
                                forceThinBorder, forceThickBorder, reversedSeriesOrder,
                                multiSeries, yValueIndex, clipInsideArea);
                        }

                        if (segmentIndex == 2 && !reversed ||
                            segmentIndex == 0 && reversed)
                        {
                            // Draw second segment
                            intersectionPoint.dataPoint = firstPoint.dataPoint;
                            intersectionPoint.index = firstPoint.index;

                            segmentPath = this.Draw3DSurface(
                                area, matrix, lightStyle, surfaceName, positionZ, depth,
                                (!firstSegmentVisible && segmentNumber != 3) ? backColor : cutSurfaceBackColor,
                                (!firstSegmentVisible && segmentNumber != 3) ? borderColor : cutSurfaceBorderColor,
                                borderWidth, borderDashStyle,
                                intersectionPoint, secondPoint,
                                points, pointIndex, tension, operationType, lineSegmentType,
                                forceThinBorder, forceThickBorder, reversedSeriesOrder,
                                multiSeries, yValueIndex, clipInsideArea);
                        }

                        // Add segment path
                        if (resultPath != null && segmentPath != null && segmentPath.PointCount > 0)
                        {
                            //resultPath.SetMarkers()
                            resultPath.AddPath(segmentPath);
                        }
                    }

                    // Restore previous y positions
                    firstPoint.yPosition = prevFirstPointY;
                    secondPoint.yPosition = prevSecondPointY;

                    return resultPath;
                }
            }

            //**********************************************************************
            //** Prepare, transform polygon coordinates
            //**********************************************************************

            // Define 4 points polygon
            Point3D[] points3D = new Point3D[4];
            points3D[0] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ + depth);
            points3D[1] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ + depth);
            points3D[2] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
            points3D[3] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);

            // Transform coordinates
            matrix.TransformPoints(points3D);

            // Get absolute coordinates and create array of SKPoint
            SKPoint[] polygonPoints = new SKPoint[4];
            polygonPoints[0] = GetAbsolutePoint(points3D[0].SKPoint);
            polygonPoints[1] = GetAbsolutePoint(points3D[1].SKPoint);
            polygonPoints[2] = GetAbsolutePoint(points3D[2].SKPoint);
            polygonPoints[3] = GetAbsolutePoint(points3D[3].SKPoint);

            //**********************************************************************
            //** Define drawing colors
            //**********************************************************************
            bool topIsVisible = IsSurfaceVisible(points3D[0], points3D[1], points3D[2]);
            SKColor polygonColor = matrix.GetPolygonLight(points3D, backColor, topIsVisible, area.Area3DStyle.Rotation, surfaceName, area.ReverseSeriesOrder);
            SKColor surfaceBorderColor = borderColor;
            if (surfaceBorderColor == SKColor.Empty)
            {
                // If border color is emty use color slightly darker than main back color
                surfaceBorderColor = ChartGraphics.GetGradientColor(backColor, SKColors.Black, 0.2);
            }

            //**********************************************************************
            //** Draw elements if required.
            //**********************************************************************
            SKPaint thinBorderPen = new() { Color = surfaceBorderColor, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
            if (drawElements)
            {
                // Draw the polygon
                if (backColor != SKColors.Transparent)
                {
                    // Remember SmoothingMode and turn off anti aliasing
                    SmoothingMode oldSmoothingMode = SmoothingMode;
                    SmoothingMode = SmoothingMode.Default;

                    // Draw the polygon
                    using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = polygonColor };
                    FillPolygon(brush, polygonPoints);

                    // Return old smoothing mode
                    SmoothingMode = oldSmoothingMode;
                }

                // Draw thin polygon border of darker color
                if (forceThinBorder || forceThickBorder)
                {
                    if (forceThickBorder)
                    {
                        SKPaint linePen = new() { Style = SKPaintStyle.Stroke, Color = surfaceBorderColor, StrokeWidth = borderWidth };
                        linePen.StrokeCap = SKStrokeCap.Round;

                        DrawLine(linePen, polygonPoints[0], polygonPoints[1]);
                        DrawLine(linePen, polygonPoints[2], polygonPoints[3]);
                        DrawLine(linePen, polygonPoints[3], polygonPoints[0]);
                        DrawLine(linePen, polygonPoints[1], polygonPoints[2]);
                    }
                    else
                    {
                        // Front & Back lines
                        DrawLine(thinBorderPen, polygonPoints[0], polygonPoints[1]);
                        DrawLine(thinBorderPen, polygonPoints[2], polygonPoints[3]);
                        if (lineSegmentType == LineSegmentType.First)
                        {
                            // Left line
                            DrawLine(thinBorderPen, polygonPoints[3], polygonPoints[0]);
                        }
                        else if (lineSegmentType == LineSegmentType.Last)
                        {
                            // Right Line
                            DrawLine(thinBorderPen, polygonPoints[1], polygonPoints[2]);
                        }
                        else
                        {
                            // Left & Right lines
                            DrawLine(thinBorderPen, polygonPoints[3], polygonPoints[0]);
                            DrawLine(thinBorderPen, polygonPoints[1], polygonPoints[2]);
                        }
                    }
                }
                else
                {
                    // Draw thin polygon border of same color (solves anti-aliasing issues)
                    if (polygonColor.Alpha == 255)
                    {
                        DrawPolygon(new SKPaint() { Color = polygonColor, StrokeWidth = 1, Style = SKPaintStyle.Stroke }, polygonPoints);
                    }

                    // Draw thin Front & Back lines
                    DrawLine(thinBorderPen, polygonPoints[0], polygonPoints[1]);
                    DrawLine(thinBorderPen, polygonPoints[2], polygonPoints[3]);
                }
            }

            //**********************************************************************
            //** Draw thick border line on visible sides
            //**********************************************************************
            SKPaint thickBorderPen = null;
            if (borderWidth > 1 && !forceThickBorder)
            {
                // Create thick border line pen
                thickBorderPen = new() { Color = surfaceBorderColor, StrokeWidth = borderWidth, Style = SKPaintStyle.Stroke };
                thickBorderPen.StrokeCap = SKStrokeCap.Round;

                //****************************************************************
                //** Switch first and second points.
                //****************************************************************
                if (firstPoint.index > secondPoint.index)
                {
                    DataPoint3D tempPoint = firstPoint;
                    firstPoint = secondPoint;
                    secondPoint = tempPoint;
                }

                //**********************************************************************
                //** Check if there are visible (non-empty) lines to the left & right
                //** of the current line.
                //**********************************************************************

                // Get visibility of bounding rectangle
                float minX = Math.Min(points3D[0].X, points3D[1].X);
                float minY = Math.Min(points3D[0].Y, points3D[1].Y);
                float maxX = Math.Max(points3D[0].X, points3D[1].X);
                float maxY = Math.Max(points3D[0].Y, points3D[1].Y);
                SKRect position = new(minX, minY, maxX - minX, maxY - minY);
                SurfaceNames visibleSurfaces = GetVisibleSurfaces(position, positionZ, depth, matrix);

                // Check left line visibility
                bool thickBorderOnLeft = false;
                bool thickBorderOnRight = false;

                if (lineSegmentType != LineSegmentType.Middle)
                {
                    LineSegmentType tempLineSegmentType = LineSegmentType.Single;

                    // Check left line visibility
                    thickBorderOnLeft = (ChartGraphics.ShouldDrawLineChartSurface(
                        area,
                        reversedSeriesOrder,
                        SurfaceNames.Left,
                        visibleSurfaces,
                        polygonColor,
                        points,
                        firstPoint,
                        secondPoint,
                        multiSeries,
                        ref tempLineSegmentType) == 2);

                    // Check right line visibility
                    thickBorderOnRight = (ChartGraphics.ShouldDrawLineChartSurface(
                        area,
                        reversedSeriesOrder,
                        SurfaceNames.Right,
                        visibleSurfaces,
                        polygonColor,
                        points,
                        firstPoint,
                        secondPoint,
                        multiSeries,
                        ref tempLineSegmentType) == 2);
                }

                // Switch left & right border if series is reversed
                if (reversedSeriesOrder)
                {
                    bool tempVal = thickBorderOnLeft;
                    thickBorderOnLeft = thickBorderOnRight;
                    thickBorderOnRight = tempVal;
                }

                // Draw thick border for single segment lines only
                // or for the first & last segment
                if (lineSegmentType != LineSegmentType.First && lineSegmentType != LineSegmentType.Single)
                {
                    thickBorderOnLeft = false;
                }
                if (lineSegmentType != LineSegmentType.Last && lineSegmentType != LineSegmentType.Single)
                {
                    thickBorderOnRight = false;
                }

                //**********************************************************************
                //** Draw border on the front side of line surface (only when visible)
                //**********************************************************************
                if (matrix.Perspective != 0 ||
                    (matrix.AngleX != 90 && matrix.AngleX != -90 &&
                    matrix.AngleY != 90 && matrix.AngleY != -90 &&
                    matrix.AngleY != 180 && matrix.AngleY != -180))
                {
                    // Draw thick line on the front side of the line surface
                    if (drawElements)
                    {
                        DrawLine(
                            thickBorderPen,
                            (float)Math.Round(polygonPoints[0].X),
                            (float)Math.Round(polygonPoints[0].Y),
                            (float)Math.Round(polygonPoints[1].X),
                            (float)Math.Round(polygonPoints[1].Y));
                    }

                    // Calculate path for selection
                    if (resultPath != null)
                    {
                        // Add front line to the path
                        resultPath.AddLine(
                            (float)Math.Round(polygonPoints[0].X),
                            (float)Math.Round(polygonPoints[0].Y),
                            (float)Math.Round(polygonPoints[1].X),
                            (float)Math.Round(polygonPoints[1].Y));
                    }
                }

                //**********************************************************************
                //** Draw border on the left side of line surface (only when visible)
                //**********************************************************************

                // Use flat end for Right & Left border
                thickBorderPen.StrokeCap = SKStrokeCap.Butt;

                // Draw border on the left side
                if ((matrix.Perspective != 0 || (matrix.AngleX != 90 && matrix.AngleX != -90)) && thickBorderOnLeft)
                {
                    if (drawElements)
                    {
                        DrawLine(
                            thickBorderPen,
                            (float)Math.Round(polygonPoints[3].X),
                            (float)Math.Round(polygonPoints[3].Y),
                            (float)Math.Round(polygonPoints[0].X),
                            (float)Math.Round(polygonPoints[0].Y));
                    }

                    // Calculate path for selection
                    if (resultPath != null)
                    {
                        // Add left line to the path
                        resultPath.AddLine(
                            (float)Math.Round(polygonPoints[3].X),
                            (float)Math.Round(polygonPoints[3].Y),
                            (float)Math.Round(polygonPoints[0].X),
                            (float)Math.Round(polygonPoints[0].Y));
                    }
                }

                //**********************************************************************
                //** Draw border on the right side of the line surface
                //**********************************************************************
                if ((matrix.Perspective != 0 || (matrix.AngleX != 90 && matrix.AngleX != -90)) && thickBorderOnRight)
                {
                    if (drawElements)
                    {
                        DrawLine(
                            thickBorderPen,
                            (float)Math.Round(polygonPoints[1].X),
                            (float)Math.Round(polygonPoints[1].Y),
                            (float)Math.Round(polygonPoints[2].X),
                            (float)Math.Round(polygonPoints[2].Y));
                    }

                    // Calculate path for selection
                    if (resultPath != null)
                    {
                        // Add right line to the path
                        resultPath.AddLine(
                            (float)Math.Round(polygonPoints[1].X),
                            (float)Math.Round(polygonPoints[1].Y),
                            (float)Math.Round(polygonPoints[2].X),
                            (float)Math.Round(polygonPoints[2].Y));
                    }
                }
            }

            //**********************************************************************
            // Redraw front line of the previuos line segment.
            // Solves 3D visibility problem between wide border line and line surface.
            //**********************************************************************
            if (area.Area3DStyle.Perspective == 0)
            {
                if (frontLinePoint1 != SKPoint.Empty && frontLinePen != null)
                {
                    // Draw line
                    DrawLine(
                        frontLinePen,
                        (float)Math.Round(frontLinePoint1.X),
                        (float)Math.Round(frontLinePoint1.Y),
                        (float)Math.Round(frontLinePoint2.X),
                        (float)Math.Round(frontLinePoint2.Y));

                    // Reset line properties
                    frontLinePen = null;
                    frontLinePoint1 = SKPoint.Empty;
                    frontLinePoint2 = SKPoint.Empty;
                }

                //**********************************************************************
                //** Check if front line should be redrawn whith the next segment.
                //**********************************************************************
                if (drawElements)
                {
                    frontLinePen = (borderWidth > 1) ? thickBorderPen : thinBorderPen;
                    frontLinePoint1 = polygonPoints[0];
                    frontLinePoint2 = polygonPoints[1];
                }
            }

            //**********************************************************************
            //** Calculate path for selection
            //**********************************************************************
            if (resultPath != null)
            {
                // Add polygon to the path
                resultPath.AddPoly(polygonPoints);
            }

            return resultPath;
        }

        /// <summary>
        /// Helper method, which indicates if area chart surface should be drawn or not.
        /// </summary>
        /// <param name="area">Chart area object.</param>
        /// <param name="reversedSeriesOrder">Series are drawn in reversed order.</param>
        /// <param name="surfaceName">Surface name.</param>
        /// <param name="boundaryRectVisibleSurfaces">Visible surfaces of the bounding rectangle.</param>
        /// <param name="color">Point back color.</param>
        /// <param name="points">Array of all points.</param>
        /// <param name="firstPoint">First point.</param>
        /// <param name="secondPoint">Second point.</param>
        /// <param name="multiSeries">Indicates that multiple series are painted at the same time (stacked or side-by-side).</param>
        /// <param name="lineSegmentType">Returns line segment type.</param>
        /// <returns>Function retrns 0, 1 or 2. 0 - Do not draw surface, 1 - draw on the back, 2 - draw in front.</returns>
        static internal int ShouldDrawLineChartSurface(
            ChartArea area,
            bool reversedSeriesOrder,
            SurfaceNames surfaceName,
            SurfaceNames boundaryRectVisibleSurfaces,
            SKColor color,
            ArrayList points,
            DataPoint3D firstPoint,
            DataPoint3D secondPoint,
            bool multiSeries,
            ref LineSegmentType lineSegmentType)
        {
            int result = 0;
            Series series = firstPoint.dataPoint.series;

            // Set active horizontal/vertical axis
            Axis hAxis = (series.XAxisType == AxisType.Primary) ? area.AxisX : area.AxisX2;
            double hAxisMin = hAxis.ViewMinimum;
            double hAxisMax = hAxis.ViewMaximum;

            //****************************************************************
            //** Check if data point and it's neigbours have non-transparent
            //** colors.
            //****************************************************************

            // Check if point main color has transparency
            bool transparent = color.Alpha != 255;

            // Check if points on the left and right side exsit and are transparent
            bool leftPointVisible = false;
            bool rightPointVisible = false;
            if (surfaceName == SurfaceNames.Left)
            {
                int pointArrayIndex = int.MinValue;
                DataPoint3D leftPoint;
                // Find Left point
                DataPoint3D leftPointAttr;
                if (!reversedSeriesOrder)
                {
                    leftPoint = FindPointByIndex(points, Math.Min(firstPoint.index, secondPoint.index) - 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                    leftPointAttr = FindPointByIndex(points, Math.Min(firstPoint.index, secondPoint.index), (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                }
                else
                {
                    leftPoint = FindPointByIndex(points, Math.Max(firstPoint.index, secondPoint.index) + 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                    leftPointAttr = leftPoint;
                }
                if (leftPoint != null)
                {
                    if (leftPointAttr.dataPoint.IsEmpty)
                    {
                        if (leftPointAttr.dataPoint.series.EmptyPointStyle.Color == color ||
                            leftPointAttr.dataPoint.series.EmptyPointStyle.Color.Alpha == 255)
                        {
                            leftPointVisible = true;
                        }
                    }
                    else
                    {
                        if (leftPointAttr.dataPoint.Color == color ||
                            leftPointAttr.dataPoint.Color.Alpha == 255)
                        {
                            leftPointVisible = true;
                        }
                    }

                    // Check if found point is outside the scaleView
                    double xValue = (leftPoint.indexedSeries) ? leftPoint.index : leftPoint.dataPoint.XValue;
                    if (xValue > hAxisMax || xValue < hAxisMin)
                    {
                        DataPoint3D currentPoint;
                        if (reversedSeriesOrder)
                        {
                            currentPoint = (firstPoint.index > secondPoint.index) ? firstPoint : secondPoint;
                        }
                        else
                        {
                            currentPoint = (firstPoint.index < secondPoint.index) ? firstPoint : secondPoint;
                        }
                        double currentXValue = (currentPoint.indexedSeries) ? currentPoint.index : currentPoint.dataPoint.XValue;
                        if (currentXValue > hAxisMax || currentXValue < hAxisMin)
                        {
                            leftPointVisible = false;
                        }
                    }
                }
            }

            // Find Right point
            if (surfaceName == SurfaceNames.Right)
            {
                DataPoint3D rightPoint = null, rightPointAttr = null;
                int pointArrayIndex = int.MinValue;
                if (!reversedSeriesOrder)
                {
                    rightPoint = ChartGraphics.FindPointByIndex(points, Math.Max(firstPoint.index, secondPoint.index) + 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                    rightPointAttr = rightPoint;
                }
                else
                {
                    rightPoint = ChartGraphics.FindPointByIndex(points, Math.Min(firstPoint.index, secondPoint.index) - 1, (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                    rightPointAttr = ChartGraphics.FindPointByIndex(points, Math.Min(firstPoint.index, secondPoint.index), (multiSeries) ? secondPoint : null, ref pointArrayIndex);
                }
                if (rightPoint != null)
                {
                    if (rightPointAttr.dataPoint.IsEmpty)
                    {
                        if (rightPointAttr.dataPoint.series.EmptyPointStyle.Color == color ||
                            rightPointAttr.dataPoint.series.EmptyPointStyle.Color.Alpha == 255)
                        {
                            rightPointVisible = true;
                        }
                    }
                    else
                    {
                        if (rightPointAttr.dataPoint.Color == color ||
                            rightPointAttr.dataPoint.Color.Alpha == 255)
                        {
                            rightPointVisible = true;
                        }
                    }

                    // Check if found point is outside the scaleView
                    double xValue = (rightPoint.indexedSeries) ? rightPoint.index : rightPoint.dataPoint.XValue;
                    if (xValue > hAxisMax || xValue < hAxisMin)
                    {
                        DataPoint3D currentPoint = null;
                        if (reversedSeriesOrder)
                        {
                            currentPoint = (firstPoint.index > secondPoint.index) ? firstPoint : secondPoint;
                        }
                        else
                        {
                            currentPoint = (firstPoint.index < secondPoint.index) ? firstPoint : secondPoint;
                        }
                        double currentXValue = (currentPoint.indexedSeries) ? currentPoint.index : currentPoint.dataPoint.XValue;
                        if (currentXValue > hAxisMax || currentXValue < hAxisMin)
                        {
                            rightPointVisible = false;
                        }
                    }
                }
            }

            //****************************************************************
            //** Get line segment
            //****************************************************************
            if (surfaceName == SurfaceNames.Left && !leftPointVisible)
            {
                if (lineSegmentType == LineSegmentType.Middle)
                {
                    lineSegmentType = LineSegmentType.First;
                }
                else if (lineSegmentType == LineSegmentType.Last)
                {
                    lineSegmentType = LineSegmentType.Single;
                }
            }
            if (surfaceName == SurfaceNames.Right && !rightPointVisible)
            {
                if (lineSegmentType == LineSegmentType.Middle)
                {
                    lineSegmentType = LineSegmentType.Last;
                }
                else if (lineSegmentType == LineSegmentType.First)
                {
                    lineSegmentType = LineSegmentType.Single;
                }
            }

            //****************************************************************
            //** Check surfaces visibility
            //****************************************************************
            if (surfaceName == SurfaceNames.Top)
            {
                result = ((boundaryRectVisibleSurfaces & SurfaceNames.Top) == SurfaceNames.Top) ? 2 : 1;
            }
            if (surfaceName == SurfaceNames.Bottom)
            {
                result = ((boundaryRectVisibleSurfaces & SurfaceNames.Bottom) == SurfaceNames.Bottom) ? 2 : 1;
                // Draw invisible bottom surface only if chart is transparent
                if (result == 1 && !transparent)
                {
                    result = 0;
                }
            }
            if (surfaceName == SurfaceNames.Front)
            {
                result = ((boundaryRectVisibleSurfaces & SurfaceNames.Front) == SurfaceNames.Front) ? 2 : 1;
                // Draw invisible front surface only if chart is transparent
                if (result == 1 && !transparent)
                {
                    result = 0;
                }
            }
            if (surfaceName == SurfaceNames.Back)
            {
                result = ((boundaryRectVisibleSurfaces & SurfaceNames.Back) == SurfaceNames.Back) ? 2 : 1;
                // Draw invisible back surface only if chart is transparent
                if (result == 1 && !transparent)
                {
                    result = 0;
                }
            }
            if (surfaceName == SurfaceNames.Left)
            {
                result = ((boundaryRectVisibleSurfaces & SurfaceNames.Left) == SurfaceNames.Left) ? 2 : 1;
                // Draw invisible left surface only if point to the left is transparent
                if (leftPointVisible)
                {
                    result = 0;
                }
            }
            if (surfaceName == SurfaceNames.Right)
            {
                result = ((boundaryRectVisibleSurfaces & SurfaceNames.Right) == SurfaceNames.Right) ? 2 : 1;
                // Draw invisible right surface only if point to the right is transparent
                if (rightPointVisible)
                {
                    result = 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Helper method which finds point in the list by it's real index.
        /// </summary>
        /// <param name="points">List of points.</param>
        /// <param name="index">Required index.</param>
        /// <param name="neighborDataPoint">Neighbor point of the same series.</param>
        /// <param name="neighborPointIndex">Neighbor point index in the array list.</param>
        /// <returns>Data point found.</returns>
        internal static DataPoint3D FindPointByIndex(ArrayList points, int index, DataPoint3D neighborDataPoint, ref int neighborPointIndex)
        {
            // Try to look around the neighbor point index
            if (neighborPointIndex != int.MinValue)
            {
                // Try getting the next point
                if (neighborPointIndex < (points.Count - 2))
                {
                    DataPoint3D point = (DataPoint3D)points[neighborPointIndex + 1];

                    // Check required point index for the first point
                    if (point.index == index &&
                        (neighborDataPoint == null || String.Compare(neighborDataPoint.dataPoint.series.Name, point.dataPoint.series.Name, StringComparison.Ordinal) == 0))
                    {
                        ++neighborPointIndex;
                        return point;
                    }
                }

                // Try getting the prev point
                if (neighborPointIndex > 0)
                {
                    DataPoint3D point = (DataPoint3D)points[neighborPointIndex - 1];

                    // Check required point index for the first point
                    if (point.index == index &&
                        (neighborDataPoint == null || String.Compare(neighborDataPoint.dataPoint.series.Name, point.dataPoint.series.Name, StringComparison.Ordinal) == 0))
                    {
                        --neighborPointIndex;
                        return point;
                    }
                }
            }

            // Loop through all points
            neighborPointIndex = 0;
            foreach (DataPoint3D point3D in points)
            {
                // Check required point index for the first point
                if (point3D.index == index)
                {
                    // Check if point belongs to the same series
                    if (neighborDataPoint != null && String.Compare(neighborDataPoint.dataPoint.series.Name, point3D.dataPoint.series.Name, StringComparison.Ordinal) != 0)
                    {
                        ++neighborPointIndex;
                        continue;
                    }

                    // Point found
                    return point3D;
                }

                ++neighborPointIndex;
            }

            // Data point was not found
            return null;
        }

        #endregion 3D Surface drawing methods (used in Line charts)

        #region 3D Rectangle drawing methods

        /// <summary>
        /// Function is used to calculate the coordinates of the 2D rectangle in 3D space
        /// and either draw it or/and calculate the bounding path for selection.
        /// </summary>
        /// <param name="position">Position of 2D rectangle.</param>
        /// <param name="positionZ">Z position of the back side of the 3D rectangle.</param>
        /// <param name="depth">Depth of the 3D rectangle.</param>
        /// <param name="matrix">Coordinate transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        internal SKPath Fill3DRectangle(
            SKRect position,
            float positionZ,
            float depth,
            Matrix3D matrix,
            LightStyle lightStyle,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            DrawingOperationTypes operationType)
        {
            return Fill3DRectangle(
                position,
                positionZ,
                depth,
                matrix,
                lightStyle,
                backColor,
                0f,
                0f,
                borderColor,
                borderWidth,
                borderDashStyle,
                BarDrawingStyle.Default,
                false,
                operationType);
        }

        /// <summary>
        /// Function is used to calculate the coordinates of the 2D rectangle in 3D space
        /// and either draw it or/and calculate the bounding path for selection.
        /// </summary>
        /// <param name="position">Position of 2D rectangle.</param>
        /// <param name="positionZ">Z position of the back side of the 3D rectangle.</param>
        /// <param name="depth">Depth of the 3D rectangle.</param>
        /// <param name="matrix">Coordinate transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="topRightDarkening">Top (or right in bar chart) darkening effect.</param>
        /// <param name="bottomLeftDarkening">Bottom (or left in bar chart) darkening effect.</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="barDrawingStyle">Bar drawing style.</param>
        /// <param name="veticalOrientation">Defines if bar is vertical or horizontal.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        internal SKPath Fill3DRectangle(
            SKRect position,
            float positionZ,
            float depth,
            Matrix3D matrix,
            LightStyle lightStyle,
            SKColor backColor,
            float topRightDarkening,
            float bottomLeftDarkening,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            BarDrawingStyle barDrawingStyle,
            bool veticalOrientation,
            DrawingOperationTypes operationType)
        {
            // Check if special drawing is required
            if (barDrawingStyle == BarDrawingStyle.Cylinder)
            {
                // Draw as 3D cylinder
                return Fill3DRectangleAsCylinder(
                    position,
                    positionZ,
                    depth,
                    matrix,
                    lightStyle,
                    backColor,
                    topRightDarkening,
                    bottomLeftDarkening,
                    borderColor,
                    borderWidth,
                    borderDashStyle,
                    veticalOrientation,
                    operationType);
            }

            // Declare variables
            Point3D[] cubePoints = new Point3D[8];
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            // Front Side
            cubePoints[0] = new Point3D(position.Left, position.Top, positionZ + depth);
            cubePoints[1] = new Point3D(position.Left, position.Bottom, positionZ + depth);
            cubePoints[2] = new Point3D(position.Right, position.Bottom, positionZ + depth);
            cubePoints[3] = new Point3D(position.Right, position.Top, positionZ + depth);

            // Back Side
            cubePoints[4] = new Point3D(position.Left, position.Top, positionZ);
            cubePoints[5] = new Point3D(position.Left, position.Bottom, positionZ);
            cubePoints[6] = new Point3D(position.Right, position.Bottom, positionZ);
            cubePoints[7] = new Point3D(position.Right, position.Top, positionZ);

            // Tranform cube coordinates
            matrix.TransformPoints(cubePoints);

            // For lightStyle style Non, Border color always exist.
            if (lightStyle == LightStyle.None &&
                (borderWidth == 0 || borderDashStyle == ChartDashStyle.NotSet || borderColor == SKColor.Empty))
            {
                borderColor = GetGradientColor(backColor, SKColors.Black, 0.5);
            }

            // Get surface colors
            matrix.GetLight(backColor, out SKColor frontLightColor, out SKColor backLightColor, out SKColor leftLightColor, out SKColor rightLightColor, out SKColor topLightColor, out SKColor bottomLightColor);

            // Darken colors by specified values
            if (topRightDarkening != 0f)
            {
                if (veticalOrientation)
                {
                    topLightColor = GetGradientColor(topLightColor, SKColors.Black, topRightDarkening);
                }
                else
                {
                    rightLightColor = GetGradientColor(rightLightColor, SKColors.Black, topRightDarkening);
                }
            }
            if (bottomLeftDarkening != 0f)
            {
                if (veticalOrientation)
                {
                    bottomLightColor = GetGradientColor(bottomLightColor, SKColors.Black, bottomLeftDarkening);
                }
                else
                {
                    leftLightColor = GetGradientColor(leftLightColor, SKColors.Black, bottomLeftDarkening);
                }
            }

            // Check visible surfaces
            SurfaceNames visibleSurfaces = GetVisibleSurfacesWithPerspective(position, positionZ, depth, matrix);

            // Draw all invisible surfaces first (if semi-transparent color is used)
            for (int drawVisible = 0; drawVisible <= 1; drawVisible++)
            {
                // Do not draw invisible surfaces for solid colors
                if (drawVisible == 0 && backColor.Alpha == 255)
                {
                    continue;
                }

                // Check visibility of all surfaces and draw them
                for (int surfaceIndex = (int)SurfaceNames.Front; surfaceIndex <= (int)SurfaceNames.Bottom; surfaceIndex *= 2)
                {
                    SurfaceNames currentSurface = (SurfaceNames)surfaceIndex;

                    // If width, height or depth of the cube (3DRectangle) is zero graphical path
                    // should contain only one surface with 4 points.
                    if (depth == 0.0 && currentSurface != SurfaceNames.Front)
                    {
                        continue;
                    }
                    if (position.Width == 0.0 && currentSurface != SurfaceNames.Left && currentSurface != SurfaceNames.Right)
                    {
                        continue;
                    }
                    if (position.Height == 0.0 && currentSurface != SurfaceNames.Top && currentSurface != SurfaceNames.Bottom)
                    {
                        continue;
                    }

                    // Check if surface is visible or semi-transparent color is used
                    bool isVisible = (visibleSurfaces & currentSurface) != 0;
                    if (isVisible && drawVisible == 1 ||
                        !isVisible && drawVisible == 0)
                    {
                        // Fill surface coordinates and color
                        SKPoint[] pointsSurface = new SKPoint[4];
                        SKColor surfaceColor = backColor;

                        switch (currentSurface)
                        {
                            case (SurfaceNames.Front):
                                surfaceColor = frontLightColor;
                                pointsSurface[0] = new SKPoint(cubePoints[0].X, cubePoints[0].Y);
                                pointsSurface[1] = new SKPoint(cubePoints[1].X, cubePoints[1].Y);
                                pointsSurface[2] = new SKPoint(cubePoints[2].X, cubePoints[2].Y);
                                pointsSurface[3] = new SKPoint(cubePoints[3].X, cubePoints[3].Y);
                                break;

                            case (SurfaceNames.Back):
                                surfaceColor = backLightColor;
                                pointsSurface[0] = new SKPoint(cubePoints[4].X, cubePoints[4].Y);
                                pointsSurface[1] = new SKPoint(cubePoints[5].X, cubePoints[5].Y);
                                pointsSurface[2] = new SKPoint(cubePoints[6].X, cubePoints[6].Y);
                                pointsSurface[3] = new SKPoint(cubePoints[7].X, cubePoints[7].Y);
                                break;

                            case (SurfaceNames.Left):
                                surfaceColor = leftLightColor;
                                pointsSurface[0] = new SKPoint(cubePoints[0].X, cubePoints[0].Y);
                                pointsSurface[1] = new SKPoint(cubePoints[1].X, cubePoints[1].Y);
                                pointsSurface[2] = new SKPoint(cubePoints[5].X, cubePoints[5].Y);
                                pointsSurface[3] = new SKPoint(cubePoints[4].X, cubePoints[4].Y);
                                break;

                            case (SurfaceNames.Right):
                                surfaceColor = rightLightColor;
                                pointsSurface[0] = new SKPoint(cubePoints[3].X, cubePoints[3].Y);
                                pointsSurface[1] = new SKPoint(cubePoints[2].X, cubePoints[2].Y);
                                pointsSurface[2] = new SKPoint(cubePoints[6].X, cubePoints[6].Y);
                                pointsSurface[3] = new SKPoint(cubePoints[7].X, cubePoints[7].Y);
                                break;

                            case (SurfaceNames.Top):
                                surfaceColor = topLightColor;
                                pointsSurface[0] = new SKPoint(cubePoints[0].X, cubePoints[0].Y);
                                pointsSurface[1] = new SKPoint(cubePoints[3].X, cubePoints[3].Y);
                                pointsSurface[2] = new SKPoint(cubePoints[7].X, cubePoints[7].Y);
                                pointsSurface[3] = new SKPoint(cubePoints[4].X, cubePoints[4].Y);
                                break;

                            case (SurfaceNames.Bottom):
                                surfaceColor = bottomLightColor;
                                pointsSurface[0] = new SKPoint(cubePoints[1].X, cubePoints[1].Y);
                                pointsSurface[1] = new SKPoint(cubePoints[2].X, cubePoints[2].Y);
                                pointsSurface[2] = new SKPoint(cubePoints[6].X, cubePoints[6].Y);
                                pointsSurface[3] = new SKPoint(cubePoints[5].X, cubePoints[5].Y);
                                break;
                        }

                        // Covert coordinates to absolute
                        for (int pointIndex = 0; pointIndex < pointsSurface.Length; pointIndex++)
                        {
                            pointsSurface[pointIndex] = GetAbsolutePoint(pointsSurface[pointIndex]);
                        }

                        // Draw surface
                        if ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement)
                        {
                            // Draw only completly visible surfaces
                            if ((visibleSurfaces & currentSurface) != 0)
                            {
                                using (SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = surfaceColor })
                                {
                                    FillPolygon(brush, pointsSurface);
                                }

                                // Check if any additional drawing should be done
                                if (currentSurface == SurfaceNames.Front &&
                                    barDrawingStyle != BarDrawingStyle.Default &&
                                    barDrawingStyle != BarDrawingStyle.Cylinder)
                                {
                                    DrawBarStyleGradients(matrix, barDrawingStyle, position, positionZ, depth, veticalOrientation);
                                }
                            }

                            // Draw surface border
                            using SKPaint pen = new() { Style = SKPaintStyle.Stroke, Color = borderColor, StrokeWidth = borderWidth };
                            pen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);
                            if (lightStyle != LightStyle.None &&
                                (borderWidth == 0 || borderDashStyle == ChartDashStyle.NotSet || borderColor == SKColor.Empty))
                            {
                                // Draw line of the same color inside the bar
                                pen.Color = surfaceColor;
                                pen.StrokeWidth = 1;
                            }

                            pen.StrokeCap = SKStrokeCap.Round;
                            DrawLine(pen, pointsSurface[0], pointsSurface[1]);
                            DrawLine(pen, pointsSurface[1], pointsSurface[2]);
                            DrawLine(pen, pointsSurface[2], pointsSurface[3]);
                            DrawLine(pen, pointsSurface[3], pointsSurface[0]);
                        }

                        // Add surface coordinate to the path
                        if ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath && (visibleSurfaces & currentSurface) != 0)
                        {
                            // resultPath.SetMarkers()
                            resultPath.AddPoly(pointsSurface);
                        }
                    }
                }
            }

            return resultPath;
        }

        /// <summary>
        /// Draws special bar style effect on the front surface of the bar.
        /// </summary>
        /// <param name="matrix">Drawing matrix.</param>
        /// <param name="barDrawingStyle">Bar drawing style.</param>
        /// <param name="position">Position in relative coordinates</param>
        /// <param name="positionZ">Z position.</param>
        /// <param name="depth">Depth.</param>
        /// <param name="isVertical">Defines if bar is vertical or horizontal.</param>
		private void DrawBarStyleGradients(
            Matrix3D matrix,
            BarDrawingStyle barDrawingStyle,
            SKRect position,
            float positionZ,
            float depth,
            bool isVertical)
        {
            if (barDrawingStyle == BarDrawingStyle.Wedge)
            {
                // Calculate wedge size to fit the rectangle
                SKRect positionAbs = GetAbsoluteRectangle(position);
                float size = (isVertical) ? positionAbs.Width / 2f : positionAbs.Height / 2f;
                if (isVertical && 2f * size > positionAbs.Height)
                {
                    size = positionAbs.Height / 2f;
                }
                if (!isVertical && 2f * size > positionAbs.Width)
                {
                    size = positionAbs.Width / 2f;
                }
                SKSize sizeRel = GetRelativeSize(new SKSize(size, size));

                // Make 3D convertion of the key points
                Point3D[] gradientPoints = new Point3D[6];
                gradientPoints[0] = new Point3D(position.Left, position.Top, positionZ + depth);
                gradientPoints[1] = new Point3D(position.Left, position.Bottom, positionZ + depth);
                gradientPoints[2] = new Point3D(position.Right, position.Bottom, positionZ + depth);
                gradientPoints[3] = new Point3D(position.Right, position.Top, positionZ + depth);
                if (isVertical)
                {
                    gradientPoints[4] = new Point3D(position.Left + position.Width / 2f, position.Top + sizeRel.Height, positionZ + depth);
                    gradientPoints[5] = new Point3D(position.Left + position.Width / 2f, position.Bottom - sizeRel.Height, positionZ + depth);
                }
                else
                {
                    gradientPoints[4] = new Point3D(position.Left + sizeRel.Width, position.Top + position.Height / 2f, positionZ + depth);
                    gradientPoints[5] = new Point3D(position.Right - sizeRel.Width, position.Top + position.Height / 2f, positionZ + depth);
                }

                // Tranform cube coordinates
                matrix.TransformPoints(gradientPoints);

                // Convert points to absolute
                SKPoint[] gradientPointsAbs = new SKPoint[6];
                for (int index = 0; index < gradientPoints.Length; index++)
                {
                    gradientPointsAbs[index] = GetAbsolutePoint(gradientPoints[index].SKPoint);
                }

                // Draw left/bottom shadow
                using (SKPath path3 = new())
                {
                    if (isVertical)
                    {
                        path3.AddLine(gradientPointsAbs[4], gradientPointsAbs[5]);
                        path3.AddLine(gradientPointsAbs[5], gradientPointsAbs[2]);
                        path3.AddLine(gradientPointsAbs[2], gradientPointsAbs[3]);
                    }
                    else
                    {
                        path3.AddLine(gradientPointsAbs[4], gradientPointsAbs[5]);
                        path3.AddLine(gradientPointsAbs[5], gradientPointsAbs[2]);
                        path3.AddLine(gradientPointsAbs[2], gradientPointsAbs[1]);
                    }
                    path3.Close();

                    // Create brush and fill path
                    using SKPaint brush3 = new() { Style = SKPaintStyle.Fill, Color = Color.FromArgb(90, SKColors.Black) };
                    FillPath(brush3, path3);
                }

                // Draw top/right triangle
                using SKPath path = new();

                if (isVertical)
                {
                    path.AddLine(gradientPointsAbs[0], gradientPointsAbs[4]);
                    path.AddLine(gradientPointsAbs[4], gradientPointsAbs[3]);
                }
                else
                {
                    path.AddLine(gradientPointsAbs[3], gradientPointsAbs[5]);
                    path.AddLine(gradientPointsAbs[5], gradientPointsAbs[2]);
                }

                // Create brush and fill path
                using SKPaint brush = new() { Color = Color.FromArgb(50, SKColors.Black), Style = SKPaintStyle.Fill };
                // Fill shadow path on the left-bottom side of the bar
                FillPath(brush, path);

                // Draw Lines
                using SKPaint penDark = new() { Color = Color.FromArgb(20, SKColors.Black), StrokeWidth = 1, Style = SKPaintStyle.Stroke };

                DrawPath(penDark, path);
                DrawLine(
                    penDark,
                    gradientPointsAbs[4],
                    gradientPointsAbs[5]);

                // Draw Lines
                using SKPaint pen = new() { Color = Color.FromArgb(40, SKColors.White), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                DrawPath(pen, path);
                DrawLine(
                    pen,
                    gradientPointsAbs[4],
                    gradientPointsAbs[5]);

                // Draw bottom/left triangle
                using SKPath path2 = new();

                if (isVertical)
                {
                    path2.AddLine(gradientPointsAbs[1], gradientPointsAbs[5]);
                    path2.AddLine(gradientPointsAbs[5], gradientPointsAbs[2]);
                }
                else
                {
                    path2.AddLine(gradientPointsAbs[0], gradientPointsAbs[4]);
                    path2.AddLine(gradientPointsAbs[4], gradientPointsAbs[1]);
                }

                // Create brush
                using SKPaint brush2 = new() { Color = Color.FromArgb(50, SKColors.Black), Style = SKPaintStyle.Fill };

                // Fill shadow path on the left-bottom side of the bar
                FillPath(brush2, path2);

                // Draw edges
                using SKPaint penDark2 = new() { Color = Color.FromArgb(20, SKColors.Black), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                DrawPath(penDark2, path);
                using SKPaint pen2 = new() { Color = Color.FromArgb(40, SKColors.Black), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                DrawPath(pen2, path);
            }
            else if (barDrawingStyle == BarDrawingStyle.LightToDark)
            {
                // Calculate width of shadows used to create the effect
                SKRect positionAbs = GetAbsoluteRectangle(position);
                float shadowSizeAbs = 5f;
                if (positionAbs.Width < 6f || positionAbs.Height < 6f)
                {
                    shadowSizeAbs = 2f;
                }
                else if (positionAbs.Width < 15f || positionAbs.Height < 15f)
                {
                    shadowSizeAbs = 3f;
                }
                SKSize shadowSizeRel = GetRelativeSize(new SKSize(shadowSizeAbs, shadowSizeAbs));

                // Calculate gradient position
                SKRect gradientRect = position;
                gradientRect.Inflate(-shadowSizeRel.Width, -shadowSizeRel.Height);
                if (isVertical)
                {
                    gradientRect.Bottom = gradientRect.Top + (float)Math.Floor(gradientRect.Height / 3f);
                }
                else
                {
                    gradientRect.Left = gradientRect.Right - (float)Math.Floor(gradientRect.Width / 3f);
                    gradientRect.Right = gradientRect.Left + (float)Math.Floor(gradientRect.Width / 3f);
                }

                // Top gradient
                Point3D[] gradientPoints = new Point3D[4];
                gradientPoints[0] = new Point3D(gradientRect.Left, gradientRect.Top, positionZ + depth);
                gradientPoints[1] = new Point3D(gradientRect.Left, gradientRect.Bottom, positionZ + depth);
                gradientPoints[2] = new Point3D(gradientRect.Right, gradientRect.Bottom, positionZ + depth);
                gradientPoints[3] = new Point3D(gradientRect.Right, gradientRect.Top, positionZ + depth);

                // Tranform cube coordinates
                matrix.TransformPoints(gradientPoints);

                // Convert points to absolute
                SKPoint[] gradientPointsAbs = new SKPoint[4];
                for (int index = 0; index < gradientPoints.Length; index++)
                {
                    gradientPointsAbs[index] = GetAbsolutePoint(gradientPoints[index].SKPoint);
                }

                // Create and draw top path
                using (SKPath path = new())
                {
                    path.AddPoly(gradientPointsAbs);
                    SKRect bounds = path.GetBounds();
                    bounds.Right += 1f;
                    bounds.Bottom += 1f;

                    // Create brush
                    if (bounds.Width > 0f && bounds.Height > 0f)
                    {
                        using SKPaint topBrush = new()
                        {
                            Style = SKPaintStyle.Fill,
                            Shader = SKShader.CreateLinearGradient(
                                bounds.Location,
                                isVertical ? new SKPoint(bounds.Left, bounds.Height) : new SKPoint(bounds.Right, bounds.Top),
                                new SKColor[]
                                {
                                    (!isVertical) ? SKColors.Transparent : Color.FromArgb(120, SKColors.White),
                                    (!isVertical) ? Color.FromArgb(120, SKColors.White) : SKColors.Transparent,
                                },
                                SKShaderTileMode.Clamp)
                        };

                        // Fill shadow path on the top side of the bar
                        FillPath(topBrush, path);
                    }
                }

                // Calculate gradient position for the bottom gradient
                gradientRect = position;
                gradientRect.Inflate(-shadowSizeRel.Width, -shadowSizeRel.Height);
                if (isVertical)
                {
                    gradientRect.Top = gradientRect.Bottom - (float)Math.Floor(gradientRect.Height / 3f);
                    gradientRect.Bottom = gradientRect.Top + (float)Math.Floor(gradientRect.Height / 3f);
                }
                else
                {
                    gradientRect.Right = gradientRect.Left + (float)Math.Floor(gradientRect.Width / 3f);
                }

                // Top gradient
                gradientPoints = new Point3D[4];
                gradientPoints[0] = new Point3D(gradientRect.Left, gradientRect.Top, positionZ + depth);
                gradientPoints[1] = new Point3D(gradientRect.Left, gradientRect.Bottom, positionZ + depth);
                gradientPoints[2] = new Point3D(gradientRect.Right, gradientRect.Bottom, positionZ + depth);
                gradientPoints[3] = new Point3D(gradientRect.Right, gradientRect.Top, positionZ + depth);

                // Tranform cube coordinates
                matrix.TransformPoints(gradientPoints);

                // Convert points to absolute
                gradientPointsAbs = new SKPoint[4];
                for (int index = 0; index < gradientPoints.Length; index++)
                {
                    gradientPointsAbs[index] = GetAbsolutePoint(gradientPoints[index].SKPoint);
                }

                // Create and draw top path
                using (SKPath path = new())
                {
                    path.AddPoly(gradientPointsAbs);
                    SKRect bounds = path.GetBounds();
                    bounds.Right += 1f;
                    bounds.Bottom += 1f;

                    // Create brush
                    if (bounds.Width > 0f && bounds.Height > 0f)
                    {
                        using SKPaint topBrush = new()
                        {
                            Shader = SKShader.CreateLinearGradient(
                                bounds.Location,
                                isVertical ? new SKPoint(bounds.Left, bounds.Bottom) : new SKPoint(bounds.Right, bounds.Top),
                                new SKColor[]
                                {
                                    (isVertical) ? SKColors.Transparent : Color.FromArgb(80, SKColors.Black),
                                    (isVertical) ? Color.FromArgb(80, SKColors.Black) : SKColors.Transparent
                                },
                                SKShaderTileMode.Clamp),
                            Style = SKPaintStyle.Fill
                        };

                        // Fill shadow path on the top side of the bar
                        FillPath(topBrush, path);
                    }
                }
            }
            else if (barDrawingStyle == BarDrawingStyle.Emboss)
            {
                // Calculate width of shadows used to create the effect
                SKRect positionAbs = GetAbsoluteRectangle(position);
                float shadowSizeAbs = 4f;
                if (positionAbs.Width < 6f || positionAbs.Height < 6f)
                {
                    shadowSizeAbs = 2f;
                }
                else if (positionAbs.Width < 15f || positionAbs.Height < 15f)
                {
                    shadowSizeAbs = 3f;
                }
                SKSize shadowSizeRel = GetRelativeSize(new SKSize(shadowSizeAbs, shadowSizeAbs));

                // Left/top Side
                Point3D[] gradientPoints = new Point3D[6];
                gradientPoints[0] = new Point3D(position.Left, position.Bottom, positionZ + depth);
                gradientPoints[1] = new Point3D(position.Left, position.Top, positionZ + depth);
                gradientPoints[2] = new Point3D(position.Right, position.Top, positionZ + depth);
                gradientPoints[3] = new Point3D(position.Right - shadowSizeRel.Width, position.Top + shadowSizeRel.Height, positionZ + depth);
                gradientPoints[4] = new Point3D(position.Left + shadowSizeRel.Width, position.Top + shadowSizeRel.Height, positionZ + depth);
                gradientPoints[5] = new Point3D(position.Left + shadowSizeRel.Width, position.Bottom - shadowSizeRel.Height, positionZ + depth);

                // Tranform cube coordinates
                matrix.TransformPoints(gradientPoints);

                // Convert points to absolute
                SKPoint[] gradientPointsAbs = new SKPoint[6];
                for (int index = 0; index < gradientPoints.Length; index++)
                {
                    gradientPointsAbs[index] = GetAbsolutePoint(gradientPoints[index].SKPoint);
                }

                // Create and draw left/top path
                using SKPath path = new();
                path.AddPoly(gradientPointsAbs);

                // Create brush
                using SKPaint leftTopBrush = new() { Color = Color.FromArgb(100, SKColors.White), Style = SKPaintStyle.Fill };
                // Fill shadow path on the left-bottom side of the bar
                FillPath(leftTopBrush, path);

                // Right/bottom Side
                gradientPoints[0] = new Point3D(position.Right, position.Top, positionZ + depth);
                gradientPoints[1] = new Point3D(position.Right, position.Bottom, positionZ + depth);
                gradientPoints[2] = new Point3D(position.Left, position.Bottom, positionZ + depth);
                gradientPoints[3] = new Point3D(position.Left + shadowSizeRel.Width, position.Bottom - shadowSizeRel.Height, positionZ + depth);
                gradientPoints[4] = new Point3D(position.Right - shadowSizeRel.Width, position.Bottom - shadowSizeRel.Height, positionZ + depth);
                gradientPoints[5] = new Point3D(position.Right - shadowSizeRel.Width, position.Top + shadowSizeRel.Height, positionZ + depth);

                // Tranform cube coordinates
                matrix.TransformPoints(gradientPoints);

                // Convert points to absolute
                for (int index = 0; index < gradientPoints.Length; index++)
                {
                    gradientPointsAbs[index] = GetAbsolutePoint(gradientPoints[index].SKPoint);
                }

                // Create and draw left/top path
                using SKPath path2 = new();

                path2.AddPoly(gradientPointsAbs);

                // Create brush
                using SKPaint bottomRightBrush = new() { Color = Color.FromArgb(80, SKColors.Black), Style = SKPaintStyle.Fill };

                // Fill shadow path on the left-bottom side of the bar
                FillPath(bottomRightBrush, path2);
            }
        }

        #endregion 3D Rectangle drawing methods

        #region 3D markers drawing methods

        /// <summary>
        /// Draw marker using absolute coordinates of the center.
        /// </summary>
        /// <param name="matrix">Coordinates transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="positionZ">Z position of the 3D marker center.</param>
        /// <param name="point">Coordinates of the center.</param>
        /// <param name="markerStyle">Marker style.</param>
        /// <param name="markerSize">Marker size.</param>
        /// <param name="markerColor">Marker color.</param>
        /// <param name="markerBorderColor">Marker border color.</param>
        /// <param name="markerBorderSize">Marker border size.</param>
        /// <param name="markerImage">Marker image name.</param>
        /// <param name="markerImageTransparentColor">Marker image transparent color.</param>
        /// <param name="shadowSize">Marker shadow size.</param>
        /// <param name="shadowColor">Marker shadow color.</param>
        /// <param name="imageScaleRect">Rectangle to which marker image should be scaled.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to ElementPath, otherwise Null.</returns>
        internal SKPath DrawMarker3D(
            Matrix3D matrix,
            LightStyle lightStyle,
            float positionZ,
            SKPoint point,
            MarkerStyle markerStyle,
            int markerSize,
            SKColor markerColor,
            SKColor markerBorderColor,
            int markerBorderSize,
            string markerImage,
            SKColor markerImageTransparentColor,
            int shadowSize,
            SKColor shadowColor,
            SKRect imageScaleRect,
            DrawingOperationTypes operationType)
        {
            ChartGraphics graph = this;
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            //************************************************************
            //** Transform marker position in 3D space
            //************************************************************
            // Get projection coordinates
            Point3D[] marker3DPosition = new Point3D[1];
            marker3DPosition[0] = new Point3D(point.X, point.Y, positionZ);

            // Transform coordinates of the marker center
            matrix.TransformPoints(marker3DPosition);
            SKPoint markerRotatedPosition = marker3DPosition[0].SKPoint;

            // Translate to absolute coordinates
            markerRotatedPosition = graph.GetAbsolutePoint(markerRotatedPosition);

            //************************************************************
            //** For those markers that do not have a 3D version - draw the same as in 2D
            //************************************************************
            if (markerImage.Length > 0 ||
                !(markerStyle == MarkerStyle.Circle ||
                markerStyle == MarkerStyle.Square))
            {
                // Call 2D version of the method
                if ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement)
                {
                    graph.DrawMarkerAbs(markerRotatedPosition, markerStyle, markerSize, markerColor, markerBorderColor, markerBorderSize, markerImage, markerImageTransparentColor, shadowSize, shadowColor, imageScaleRect, false);
                }

                // Prepare marker path
                if ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                {
                    SKRect rect = SKRect.Empty;
                    rect.Left = markerRotatedPosition.X - ((float)markerSize) / 2F;
                    rect.Top = markerRotatedPosition.Y - ((float)markerSize) / 2F;
                    rect.Right = rect.Left + markerSize;
                    rect.Bottom = rect.Top + markerSize;
                    resultPath.AddRect(rect);
                }

                return resultPath;
            }

            //************************************************************
            //** Draw marker
            //************************************************************
            // Check if marker properties are set
            if (markerStyle != MarkerStyle.None && markerSize > 0 && markerColor != SKColor.Empty)
            {
                // Create solid color brush
                using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = markerColor };
                // Calculate marker rectangle
                SKRect rect = SKRect.Empty;
                rect.Left = markerRotatedPosition.X - markerSize / 2F;
                rect.Top = markerRotatedPosition.Y - markerSize / 2F;
                rect.Size = new SKSize(markerSize, markerSize);

                // Calculate relative marker size
                SKSize markerRelativeSize = graph.GetRelativeSize(new SKSize(markerSize, markerSize));

                // Draw marker depending on style
                switch (markerStyle)
                {
                    case (MarkerStyle.Circle):
                        {
                            if ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement)
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    if (!graph.softShadows)
                                    {
                                        using SKPaint shadowBrush = new() { Style = SKPaintStyle.Fill, Color = (shadowColor.Alpha != 255) ? shadowColor : Color.FromArgb(markerColor.Alpha / 2, shadowColor) };
                                        SKRect shadowRect = rect;
                                        shadowRect.Left += shadowSize;
                                        shadowRect.Top += shadowSize;
                                        graph.FillEllipse(shadowBrush, shadowRect);
                                    }
                                    else
                                    {
                                        // Add circle to the graphics path
                                        using SKPath path = new();
                                        path.AddOval(new SKRect(rect.Left + shadowSize - 1, rect.Top + shadowSize - 1, rect.Width + 2, rect.Height + 2));

                                        // Create path brush
                                        using SKPaint shadowBrush = new();
                                        shadowBrush.Shader = SKShader.CreateRadialGradient(
                                            new SKPoint(markerRotatedPosition.X, markerRotatedPosition.Y),
                                            rect.Width + 2, new SKColor[] { shadowColor, SKColors.Transparent },
                                            SKShaderTileMode.Clamp);

                                        // Draw shadow
                                        graph.FillPath(shadowBrush, path);
                                    }
                                }

                                // Create path gradient brush
                                using SKPath brushPath = new();
                                SKRect rectLightCenter = new(rect.Left, rect.Top, rect.Right, rect.Bottom);
                                rectLightCenter.Inflate(rectLightCenter.Width / 4f, rectLightCenter.Height / 4f);
                                brushPath.AddOval(rectLightCenter);
                                using SKPaint circleBrush = new() { Style = SKPaintStyle.Fill };
                                // Calculate the center point of the gradient
                                Point3D[] centerPoint = new Point3D[] { new Point3D(point.X, point.Y, positionZ + markerRelativeSize.Width) };
                                matrix.TransformPoints(centerPoint);
                                centerPoint[0].SKPoint = graph.GetAbsolutePoint(centerPoint[0].SKPoint);
                                circleBrush.Shader = SKShader.CreateRadialGradient(
                                        new SKPoint(centerPoint[0].SKPoint.X, centerPoint[0].SKPoint.Y),
                                        rect.Width + 2, new SKColor[] { GetGradientColor(markerColor, SKColors.White, 0.85), markerColor },
                                        SKShaderTileMode.Clamp);

                                // Draw circle (sphere)
                                graph.FillEllipse(circleBrush, rect);
                                graph.DrawEllipse(new SKPaint() { Color = markerBorderColor, StrokeWidth = markerBorderSize, Style = SKPaintStyle.Stroke }, rect);
                            }

                            // Prepare marker path
                            if ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                            {
                                resultPath.AddOval(rect);
                            }

                            break;
                        }
                    case (MarkerStyle.Square):
                        {
                            // Calculate marker non-rotated rectangle
                            SKRect rectNonRotated = SKRect.Empty;
                            rectNonRotated.Left = point.X - (markerRelativeSize.Width) / 2F;
                            rectNonRotated.Top = point.Y - (markerRelativeSize.Height) / 2F;
                            rectNonRotated.Size = new(markerRelativeSize.Width, markerRelativeSize.Height);

                            // Draw 3D bar
                            resultPath = Fill3DRectangle(
                                rectNonRotated,
                                positionZ - markerRelativeSize.Width / 2f,
                                markerRelativeSize.Width,
                                matrix,
                                lightStyle,
                                markerColor,
                                markerBorderColor,
                                markerBorderSize,
                                ChartDashStyle.Solid,
                                operationType);

                            break;
                        }
                    default:
                        {
                            throw (new InvalidOperationException(SR.ExceptionGraphics3DMarkerStyleUnknown));
                        }
                }
            }

            return resultPath;
        }

        #endregion 3D markers drawing methods

        #region 3D cube surface visibility methods

        /// <summary>
        /// Returns visible surfaces of the 3D cube.
        /// </summary>
        /// <param name="position">2D rectangle coordinates.</param>
        /// <param name="positionZ">Z coordinate of the back side of the cube.</param>
        /// <param name="depth">Cube depth.</param>
        /// <param name="matrix">Coordinate transformation matrix.</param>
        /// <returns>Visible surfaces.</returns>
        internal SurfaceNames GetVisibleSurfaces(
            SKRect position,
            float positionZ,
            float depth,
            Matrix3D matrix
            )
        {
            // Check if perspective is used
            if (matrix.Perspective != 0)
            {
                // More sofisticated algorithm must be used for visibility detection.
                return GetVisibleSurfacesWithPerspective(position, positionZ, depth, matrix);
            }

            // Front surface is always visible
            SurfaceNames result = SurfaceNames.Front;

            // Left and Right surfaces depend on the Y axis angle
            if (matrix.AngleY > 0)
            {
                result |= SurfaceNames.Right;
            }
            else if (matrix.AngleY < 0)
            {
                result |= SurfaceNames.Left;
            }

            // Top and Bottom surfaces depend on the X axis angle
            if (matrix.AngleX > 0)
            {
                result |= SurfaceNames.Top;
            }
            else if (matrix.AngleX < 0)
            {
                result |= SurfaceNames.Bottom;
            }

            return result;
        }

        /// <summary>
        /// Returns visible surfaces of the 3D cube.
        /// This method takes in consideration the perspective.
        /// </summary>
        /// <param name="position">2D rectangle coordinates.</param>
        /// <param name="positionZ">Z coordinate of the back side of the cube.</param>
        /// <param name="depth">Cube depth.</param>
        /// <param name="matrix">Coordinate transformation matrix.</param>
        /// <returns>Visible surfaces.</returns>
        internal SurfaceNames GetVisibleSurfacesWithPerspective(
            SKRect position,
            float positionZ,
            float depth,
            Matrix3D matrix)
        {
            // Create cube coordinates in 3D space
            Point3D[] cubePoints = new Point3D[8];

            // Front Side
            cubePoints[0] = new Point3D(position.Left, position.Top, positionZ + depth);
            cubePoints[1] = new Point3D(position.Left, position.Bottom, positionZ + depth);
            cubePoints[2] = new Point3D(position.Right, position.Bottom, positionZ + depth);
            cubePoints[3] = new Point3D(position.Right, position.Top, positionZ + depth);

            // Back Side
            cubePoints[4] = new Point3D(position.Left, position.Top, positionZ);
            cubePoints[5] = new Point3D(position.Left, position.Bottom, positionZ);
            cubePoints[6] = new Point3D(position.Right, position.Bottom, positionZ);
            cubePoints[7] = new Point3D(position.Right, position.Top, positionZ);

            // Tranform coordinates
            matrix.TransformPoints(cubePoints);

            // Detect surfaces visibility
            return GetVisibleSurfacesWithPerspective(cubePoints);
        }

        /// <summary>
        /// Returns visible surfaces of the 3D cube.
        /// This method takes in consideration the perspective.
        /// </summary>
        /// <param name="cubePoints">Array of 8 points which define the cube.</param>
        /// <returns>Visible surfaces.</returns>
        internal SurfaceNames GetVisibleSurfacesWithPerspective(Point3D[] cubePoints)
        {
            // Check imput array size
            if (cubePoints.Length != 8)
            {
                throw new ArgumentException(SR.ExceptionGraphics3DCoordinatesInvalid, nameof(cubePoints));
            }

            // Detect surfaces visibility
            SurfaceNames result = 0;

            // Check the front side
            if (IsSurfaceVisible(cubePoints[0], cubePoints[3], cubePoints[2]))
            {
                result |= SurfaceNames.Front;
            }
            // Check the back side
            if (IsSurfaceVisible(cubePoints[4], cubePoints[5], cubePoints[6]))
            {
                result |= SurfaceNames.Back;
            }

            // Check the left side
            if (IsSurfaceVisible(cubePoints[0], cubePoints[1], cubePoints[5]))
            {
                result |= SurfaceNames.Left;
            }

            // Check the right side
            if (IsSurfaceVisible(cubePoints[3], cubePoints[7], cubePoints[6]))
            {
                result |= SurfaceNames.Right;
            }

            // Check the top side
            if (IsSurfaceVisible(cubePoints[4], cubePoints[7], cubePoints[3]))
            {
                result |= SurfaceNames.Top;
            }

            // Check the bottom side
            if (IsSurfaceVisible(cubePoints[1], cubePoints[2], cubePoints[6]))
            {
                result |= SurfaceNames.Bottom;
            }

            return result;
        }

        /// <summary>
        /// Checks surface visibility using 3 points and clockwise points index rotation.
        /// </summary>
        /// <param name="first">First point.</param>
        /// <param name="second">Second point.</param>
        /// <param name="tree">Third point.</param>
        /// <returns>True if surface is visible</returns>
        internal static bool IsSurfaceVisible(Point3D first, Point3D second, Point3D tree)
        {
            // Check if points are oriented clocwise in 2D projection.
            // If points are clockwise the surface is visible.
            float a = (first.Y - second.Y) / (first.X - second.X);
            float b = first.Y - a * first.X;
            if (first.X == second.X)
            {
                if (first.Y > second.Y)
                {
                    if (tree.X > first.X)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (tree.X > first.X)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (first.X < second.X)
            {
                if (tree.Y < a * tree.X + b)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (tree.Y <= a * tree.X + b)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion 3D cube surface visibility methods

        #region Line intersection helper method

        /// <summary>
        /// Gets intersection point of two lines
        /// </summary>
        /// <param name="x1">First X value of first line.</param>
        /// <param name="y1">First Y value of first line.</param>
        /// <param name="x2">Second X value of first line.</param>
        /// <param name="y2">Second Y value of first line.</param>
        /// <param name="x3">First X value of second line.</param>
        /// <param name="y3">First Y value of second line.</param>
        /// <param name="x4">Second X value of second line.</param>
        /// <param name="y4">Second Y value of second line.</param>
        /// <returns>Intersection coordinates.</returns>
        internal static SKPoint GetLinesIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            SKPoint result = SKPoint.Empty;

            // Special case for horizontal & vertical lines
            if (x1 == x2 && y3 == y4)
            {
                result.X = x1;
                result.Y = y3;
                return result;
            }
            else if (y1 == y2 && x3 == x4)
            {
                result.X = x3;
                result.Y = y1;
                return result;
            }
            else if (x1 == x2)
            {
                result.X = x1;
                result.Y = (result.X - x3) * (y4 - y3);
                result.Y /= x4 - x3;
                result.Y += y3;
                return result;
            }
            else if (x3 == x4)
            {
                result.X = x3;
                result.Y = (result.X - x1) * (y2 - y1);
                result.Y /= x2 - x1;
                result.Y += y1;
                return result;
            }

            // Calculate line eqaution
            float a1 = (y1 - y2) / (x1 - x2);
            float b1 = y1 - a1 * x1;
            float a2 = (y3 - y4) / (x3 - x4);
            float b2 = y3 - a2 * x3;

            // Calculate intersection point
            result.X = (b2 - b1) / (a1 - a2);
            result.Y = a1 * result.X + b1;

            return result;
        }

        #endregion Line intersection helper method

        #region 3D Cylinder drawing methods

        /// <summary>
        /// Function is used to calculate the coordinates of the 2D rectangle in 3D space
        /// and either draw it or/and calculate the bounding path for selection.
        /// </summary>
        /// <param name="position">Position of 2D rectangle.</param>
        /// <param name="positionZ">Z position of the back side of the 3D rectangle.</param>
        /// <param name="depth">Depth of the 3D rectangle.</param>
        /// <param name="matrix">Coordinate transformation matrix.</param>
        /// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="topRightDarkening">Top (or right in bar chart) darkening effect.</param>
        /// <param name="bottomLeftDarkening">Bottom (or left in bar chart) darkening effect.</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="veticalOrientation">Defines if bar is vertical or horizontal.</param>
        /// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
        /// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
        internal SKPath Fill3DRectangleAsCylinder(
            SKRect position,
            float positionZ,
            float depth,
            Matrix3D matrix,
            LightStyle lightStyle,
            SKColor backColor,
            float topRightDarkening,
            float bottomLeftDarkening,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            bool veticalOrientation,
            DrawingOperationTypes operationType)
        {
            Point3D[] cubePoints = new Point3D[8];
            SKPath resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
                ? new SKPath() : null;

            //*******************************************************
            //** Define coordinates to draw the cylinder
            //*******************************************************
            if (veticalOrientation)
            {
                cubePoints[0] = new Point3D(position.Left, position.Top, positionZ + depth / 2f);
                cubePoints[1] = new Point3D(position.Left, position.Bottom, positionZ + depth / 2f);
                cubePoints[2] = new Point3D(position.Right, position.Bottom, positionZ + depth / 2f);
                cubePoints[3] = new Point3D(position.Right, position.Top, positionZ + depth / 2f);

                float middleXValue = position.Left + position.Width / 2f;
                cubePoints[4] = new Point3D(middleXValue, position.Top, positionZ + depth);
                cubePoints[5] = new Point3D(middleXValue, position.Bottom, positionZ + depth);
                cubePoints[6] = new Point3D(middleXValue, position.Bottom, positionZ);
                cubePoints[7] = new Point3D(middleXValue, position.Top, positionZ);
            }
            else
            {
                cubePoints[0] = new Point3D(position.Right, position.Top, positionZ + depth / 2f);
                cubePoints[1] = new Point3D(position.Left, position.Top, positionZ + depth / 2f);
                cubePoints[2] = new Point3D(position.Left, position.Bottom, positionZ + depth / 2f);
                cubePoints[3] = new Point3D(position.Right, position.Bottom, positionZ + depth / 2f);

                float middleYValue = position.Top + position.Height / 2f;
                cubePoints[4] = new Point3D(position.Right, middleYValue, positionZ + depth);
                cubePoints[5] = new Point3D(position.Left, middleYValue, positionZ + depth);
                cubePoints[6] = new Point3D(position.Left, middleYValue, positionZ);
                cubePoints[7] = new Point3D(position.Right, middleYValue, positionZ);
            }

            // Tranform cylinder coordinates
            matrix.TransformPoints(cubePoints);

            // Covert coordinates to absolute
            for (int pointIndex = 0; pointIndex < cubePoints.Length; pointIndex++)
            {
                cubePoints[pointIndex].SKPoint = GetAbsolutePoint(cubePoints[pointIndex].SKPoint);
            }

            //*******************************************************
            //** Get cylinder colors.
            //*******************************************************
            if (lightStyle == LightStyle.None &&
                (borderWidth == 0 || borderDashStyle == ChartDashStyle.NotSet || borderColor == SKColor.Empty))
            {
                borderColor = GetGradientColor(backColor, SKColors.Black, 0.5);
            }

            // Get surface colors
            matrix.GetLight(backColor, out SKColor frontLightColor, out SKColor backLightColor, out SKColor leftLightColor, out SKColor rightLightColor, out SKColor topLightColor, out SKColor bottomLightColor);

            // Darken colors by specified values
            if (topRightDarkening != 0f)
            {
                if (veticalOrientation)
                {
                    topLightColor = GetGradientColor(topLightColor, SKColors.Black, topRightDarkening);
                }
                else
                {
                    rightLightColor = GetGradientColor(rightLightColor, SKColors.Black, topRightDarkening);
                }
            }
            if (bottomLeftDarkening != 0f)
            {
                if (veticalOrientation)
                {
                    bottomLightColor = GetGradientColor(bottomLightColor, SKColors.Black, bottomLeftDarkening);
                }
                else
                {
                    leftLightColor = GetGradientColor(leftLightColor, SKColors.Black, bottomLeftDarkening);
                }
            }

            //*******************************************************
            //** Check visible surfaces
            //*******************************************************
            SurfaceNames visibleSurfaces = GetVisibleSurfacesWithPerspective(position, positionZ, depth, matrix);

            // Front surface is always visible in cylinder
            if ((visibleSurfaces & SurfaceNames.Front) != SurfaceNames.Front)
            {
                visibleSurfaces |= SurfaceNames.Front;
            }

            //*******************************************************
            //** Create flattened paths for the sides of the
            //** cylinder (top,bottom/left,rigth)
            //*******************************************************
            SKPoint[] sidePoints = new SKPoint[4];
            sidePoints[0] = cubePoints[6].SKPoint;
            sidePoints[1] = cubePoints[1].SKPoint;
            sidePoints[2] = cubePoints[5].SKPoint;
            sidePoints[3] = cubePoints[2].SKPoint;
            SKPath bottomLeftSide = new();
            bottomLeftSide.AddPath(SkiaSharpExtensions.CreateSpline(sidePoints));
            bottomLeftSide.Close();
            sidePoints[0] = cubePoints[7].SKPoint;
            sidePoints[1] = cubePoints[0].SKPoint;
            sidePoints[2] = cubePoints[4].SKPoint;
            sidePoints[3] = cubePoints[3].SKPoint;
            SKPath topRigthSide = new();
            topRigthSide.AddPath(SkiaSharpExtensions.CreateSpline(sidePoints));
            topRigthSide.Close();

            //*******************************************************
            //** Find cylinder angle
            //*******************************************************
            float cylinderAngle = 90f;
            if (cubePoints[5].SKPoint.Y != cubePoints[4].SKPoint.Y)
            {
                cylinderAngle = (float)Math.Atan(
                    (cubePoints[4].SKPoint.X - cubePoints[5].SKPoint.X) /
                    (cubePoints[5].SKPoint.Y - cubePoints[4].SKPoint.Y));
                cylinderAngle = (float)Math.Round(cylinderAngle * 180f / (float)Math.PI);
            }

            //*******************************************************
            //** Draw all invisible surfaces first (if semi-transparent color is used)
            //*******************************************************
            for (int drawVisible = 0; drawVisible <= 1; drawVisible++)
            {
                // Do not draw invisible surfaces for solid colors
                if (drawVisible == 0 && backColor.Alpha == 255)
                {
                    continue;
                }

                // Check visibility of all surfaces and draw them
                for (int surfaceIndex = (int)SurfaceNames.Front; surfaceIndex <= (int)SurfaceNames.Bottom; surfaceIndex *= 2)
                {
                    SurfaceNames currentSurface = (SurfaceNames)surfaceIndex;

                    // Check if surface is visible or semi-transparent color is used
                    bool isVisible = (visibleSurfaces & currentSurface) != 0;
                    if (isVisible && drawVisible == 1 ||
                        !isVisible && drawVisible == 0)
                    {
                        // Fill surface coordinates and color
                        SKPath pathToDraw = null;
                        SKColor surfaceColor = backColor;

                        // Declare a special brush for the front surface
                        SKPaint frontSurfaceBrush = null;

                        switch (currentSurface)
                        {
                            case (SurfaceNames.Front):
                                {
                                    // Set front surface color
                                    surfaceColor = backColor;

                                    // Add ellipse segment of the cylinder on top/rigth (reversed)
                                    pathToDraw = new SKPath();
                                    SKPoint leftSideLinePoint = SKPoint.Empty;
                                    SKPoint rightSideLinePoint = SKPoint.Empty;
                                    AddEllipseSegment(
                                        pathToDraw,
                                        topRigthSide,
                                        bottomLeftSide,
                                        (matrix.Perspective == 0) && veticalOrientation,
                                        cylinderAngle,
                                        out leftSideLinePoint,
                                        out rightSideLinePoint);
                                    pathToDraw.Reverse();

                                    // Add ellipse segment of the cylinder on bottom/left
                                    AddEllipseSegment(
                                        pathToDraw,
                                        bottomLeftSide,
                                        topRigthSide,
                                        (matrix.Perspective == 0) && veticalOrientation,
                                        cylinderAngle,
                                        out SKPoint leftOppSideLinePoint,
                                        out SKPoint rightOppSideLinePoint);
                                    pathToDraw.Close();

                                    // Reset indexes of opposite side points
                                    _oppLeftBottomPoint = -1;
                                    _oppRigthTopPoint = -1;

                                    // Create gradient brush for the front surface
                                    if (lightStyle != LightStyle.None)
                                    {
                                        SKRect boundsRect = pathToDraw.GetBounds();
                                        if (boundsRect.Height > 0 && boundsRect.Width > 0)
                                        {
                                            SKColor lightColor = GetGradientColor(backColor, SKColors.White, 0.3);
                                            SKColor darkColor = GetGradientColor(backColor, SKColors.Black, 0.3);

                                            // Create gradient
                                            if (!leftSideLinePoint.IsEmpty &&
                                                !rightSideLinePoint.IsEmpty &&
                                                !leftOppSideLinePoint.IsEmpty &&
                                                !rightOppSideLinePoint.IsEmpty)
                                            {
                                                SKPoint boundsRectMiddlePoint = SKPoint.Empty;
                                                boundsRectMiddlePoint.X = boundsRect.Left + boundsRect.Width / 2f;
                                                boundsRectMiddlePoint.Y = boundsRect.Top + boundsRect.Height / 2f;

                                                SKPoint centralLinePoint = SKPoint.Empty;
                                                double centralLineAngle = ((cylinderAngle) * Math.PI / 180f);
                                                if (cylinderAngle == 0 || cylinderAngle == 180 || cylinderAngle == -180)
                                                {
                                                    centralLinePoint.X = boundsRectMiddlePoint.X + 100f;
                                                    centralLinePoint.Y = boundsRectMiddlePoint.Y;
                                                }
                                                else if (cylinderAngle == 90 || cylinderAngle == -90)
                                                {
                                                    centralLinePoint.X = boundsRectMiddlePoint.X;
                                                    centralLinePoint.Y = boundsRectMiddlePoint.Y + 100f;
                                                }
                                                else if (cylinderAngle > -45 && cylinderAngle < 45)
                                                {
                                                    centralLinePoint.X = boundsRectMiddlePoint.X + 100f;
                                                    centralLinePoint.Y = (float)(Math.Tan(centralLineAngle) * centralLinePoint.X);
                                                    centralLinePoint.Y += (float)(boundsRectMiddlePoint.Y - Math.Tan(centralLineAngle) * boundsRectMiddlePoint.X);
                                                }
                                                else
                                                {
                                                    centralLinePoint.Y = boundsRectMiddlePoint.Y + 100f;
                                                    centralLinePoint.X = (float)(centralLinePoint.Y - (boundsRectMiddlePoint.Y - Math.Tan(centralLineAngle) * boundsRectMiddlePoint.X));
                                                    centralLinePoint.X /= (float)(Math.Tan(centralLineAngle));
                                                }

                                                SKPoint middlePoint1 = GetLinesIntersection(
                                                    boundsRectMiddlePoint.X, boundsRectMiddlePoint.Y,
                                                    centralLinePoint.X, centralLinePoint.Y,
                                                    leftSideLinePoint.X, leftSideLinePoint.Y,
                                                    leftOppSideLinePoint.X, leftOppSideLinePoint.Y);

                                                SKPoint middlePoint2 = GetLinesIntersection(
                                                    boundsRectMiddlePoint.X, boundsRectMiddlePoint.Y,
                                                    centralLinePoint.X, centralLinePoint.Y,
                                                    rightSideLinePoint.X, rightSideLinePoint.Y,
                                                    rightOppSideLinePoint.X, rightOppSideLinePoint.Y);

                                                // Gradient points can not have same coordinates
                                                if (middlePoint1.X != middlePoint2.X || middlePoint1.Y != middlePoint2.Y)
                                                {
                                                    frontSurfaceBrush.Shader = SKShader.CreateLinearGradient(
                                                        middlePoint1, middlePoint2,
                                                        new SKColor[] { darkColor, darkColor, lightColor, darkColor, darkColor },
                                                        new float[] { 0.0f, 0.0f, 0.5f, 1.0f, 1.0f },
                                                        SKShaderTileMode.Clamp);
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }
                            case SurfaceNames.Top:
                                if (veticalOrientation)
                                {
                                    surfaceColor = topLightColor;
                                    pathToDraw = topRigthSide;
                                }
                                break;

                            case SurfaceNames.Bottom:
                                if (veticalOrientation)
                                {
                                    surfaceColor = bottomLightColor;
                                    pathToDraw = bottomLeftSide;
                                }
                                break;

                            case (SurfaceNames.Right):
                                if (!veticalOrientation)
                                {
                                    surfaceColor = rightLightColor;
                                    pathToDraw = topRigthSide;
                                }
                                break;

                            case (SurfaceNames.Left):
                                if (!veticalOrientation)
                                {
                                    surfaceColor = leftLightColor;
                                    pathToDraw = bottomLeftSide;
                                }
                                break;
                        }

                        //*******************************************************
                        //** Draw surface
                        //*******************************************************
                        if (pathToDraw != null)
                        {
                            if ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement)
                            {
                                // Draw only completly visible surfaces
                                if ((visibleSurfaces & currentSurface) != 0)
                                {
                                    using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = surfaceColor };
                                    FillPath(frontSurfaceBrush ?? brush, pathToDraw);
                                }

                                // Draw surface border
                                using SKPaint pen = new() { Color = borderColor, StrokeWidth = borderWidth, Style = SKPaintStyle.Stroke };
                                pen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);
                                if (lightStyle != LightStyle.None &&
                                    (borderWidth == 0 || borderDashStyle == ChartDashStyle.NotSet || borderColor == SKColor.Empty))
                                {
                                    // Draw line of the darker color inside the cylinder
                                    pen.Color = frontSurfaceBrush == null ? surfaceColor : GetGradientColor(backColor, SKColors.Black, 0.3);
                                    pen.StrokeWidth = 1;
                                }

                                pen.StrokeCap = SKStrokeCap.Round;
                                pen.StrokeJoin = SKStrokeJoin.Bevel;
                                DrawPath(pen, pathToDraw);
                            }

                            // Add surface coordinate to the path
                            // Only if surface is completly visible
                            if ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath && (visibleSurfaces & currentSurface) != 0 && pathToDraw != null && pathToDraw.PointCount > 0)
                            {
                                resultPath.AddPath(pathToDraw);
                                //resultPath.SetMarkers()
                            }
                        }
                    }
                }
            }

            return resultPath;
        }

        /// <summary>
        /// Adds segment of the ellipse to form the front surface of the cylinder
        /// </summary>
        internal void AddEllipseSegment(
            SKPath resultPath,
            SKPath ellipseFlattenPath,
            SKPath oppositeEllipseFlattenPath,
            bool veticalOrientation,
            float cylinderAngle,
            out SKPoint leftSideLinePoint,
            out SKPoint rightSideLinePoint)
        {
            // Initialize return values
            leftSideLinePoint = SKPoint.Empty;
            rightSideLinePoint = SKPoint.Empty;

            // Check if input path is empty
            if (ellipseFlattenPath.PointCount == 0)
            {
                return;
            }

            // Find the index the left/bottom most and right/top most point in flatten array of ellipse points
            int leftBottomPoint = 0;
            int rigthTopPoint = 0;
            SKPoint[] ellipsePoints = ellipseFlattenPath.Points;

            if (veticalOrientation)
            {
                for (int pointIndex = 1; pointIndex < ellipsePoints.Length; pointIndex++)
                {
                    if (ellipsePoints[leftBottomPoint].X > ellipsePoints[pointIndex].X)
                    {
                        leftBottomPoint = pointIndex;
                    }
                    if (ellipsePoints[rigthTopPoint].X < ellipsePoints[pointIndex].X)
                    {
                        rigthTopPoint = pointIndex;
                    }
                }
            }
            else
            {
                bool doneFlag = false;
                leftBottomPoint = -1;
                rigthTopPoint = -1;

                if (_oppLeftBottomPoint != -1 && _oppRigthTopPoint != -1)
                {
                    // Get index from previously calculated values
                    leftBottomPoint = _oppLeftBottomPoint;
                    rigthTopPoint = _oppRigthTopPoint;
                }
                else
                {
                    // Loop through first ellipse points
                    SKPoint[] oppositeEllipsePoints = oppositeEllipseFlattenPath.Points;
                    for (int pointIndex = 0; !doneFlag && pointIndex < ellipsePoints.Length; pointIndex++)
                    {
                        // Loop through opposite ellipse points
                        for (int pointOppositeIndex = 0; !doneFlag && pointOppositeIndex < oppositeEllipsePoints.Length; pointOppositeIndex++)
                        {
                            bool closeToVertical = false;
                            bool pointsOnLeft = false;
                            bool pointsOnRight = false;

                            //if(cylinderAngle == 0 || cylinderAngle == 180 || cylinderAngle == -180)
                            if (cylinderAngle > -30 && cylinderAngle < 30)
                            {
                                closeToVertical = true;
                            }

                            if (closeToVertical)
                            {
                                if (oppositeEllipsePoints[pointOppositeIndex].Y == ellipsePoints[pointIndex].Y)
                                {
                                    continue;
                                }

                                float linePointX = oppositeEllipsePoints[pointOppositeIndex].X - ellipsePoints[pointIndex].X;
                                linePointX /= oppositeEllipsePoints[pointOppositeIndex].Y - ellipsePoints[pointIndex].Y;

                                // Check if this line has any points to the right/left
                                for (int innerPointIndex = 0; innerPointIndex < ellipsePoints.Length; innerPointIndex++)
                                {
                                    // Skip points used to define line function
                                    if (innerPointIndex == pointIndex)
                                    {
                                        continue;
                                    }

                                    float x = linePointX;
                                    x *= ellipsePoints[innerPointIndex].Y - ellipsePoints[pointIndex].Y;
                                    x += ellipsePoints[pointIndex].X;

                                    if (x > ellipsePoints[innerPointIndex].X)
                                    {
                                        pointsOnLeft = true;
                                    }
                                    if (x < ellipsePoints[innerPointIndex].X)
                                    {
                                        pointsOnRight = true;
                                    }
                                    if (pointsOnLeft && pointsOnRight)
                                    {
                                        break;
                                    }
                                }

                                if (!pointsOnLeft || !pointsOnRight)
                                {
                                    for (int innerPointIndex = 0; innerPointIndex < oppositeEllipsePoints.Length; innerPointIndex++)
                                    {
                                        // Skip points used to define line function
                                        if (innerPointIndex == pointOppositeIndex)
                                        {
                                            continue;
                                        }

                                        float x = linePointX;
                                        x *= oppositeEllipsePoints[innerPointIndex].Y - ellipsePoints[pointIndex].Y;
                                        x += ellipsePoints[pointIndex].X;

                                        if (x > oppositeEllipsePoints[innerPointIndex].X)
                                        {
                                            pointsOnLeft = true;
                                        }
                                        if (x < oppositeEllipsePoints[innerPointIndex].X)
                                        {
                                            pointsOnRight = true;
                                        }
                                        if (pointsOnLeft && pointsOnRight)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (oppositeEllipsePoints[pointOppositeIndex].X == ellipsePoints[pointIndex].X)
                                {
                                    continue;
                                }

                                float linePointY = oppositeEllipsePoints[pointOppositeIndex].Y - ellipsePoints[pointIndex].Y;
                                linePointY /= oppositeEllipsePoints[pointOppositeIndex].X - ellipsePoints[pointIndex].X;

                                // Check if this line has any points to the right/left
                                for (int innerPointIndex = 0; innerPointIndex < ellipsePoints.Length; innerPointIndex++)
                                {
                                    // Skip points used to define line function
                                    if (innerPointIndex == pointIndex)
                                    {
                                        continue;
                                    }

                                    float y = linePointY;
                                    y *= ellipsePoints[innerPointIndex].X - ellipsePoints[pointIndex].X;
                                    y += ellipsePoints[pointIndex].Y;

                                    if (y > ellipsePoints[innerPointIndex].Y)
                                    {
                                        pointsOnLeft = true;
                                    }
                                    if (y < ellipsePoints[innerPointIndex].Y)
                                    {
                                        pointsOnRight = true;
                                    }
                                    if (pointsOnLeft && pointsOnRight)
                                    {
                                        break;
                                    }
                                }

                                if (!pointsOnLeft || !pointsOnRight)
                                {
                                    for (int innerPointIndex = 0; innerPointIndex < oppositeEllipsePoints.Length; innerPointIndex++)
                                    {
                                        // Skip points used to define line function
                                        if (innerPointIndex == pointOppositeIndex)
                                        {
                                            continue;
                                        }

                                        float y = linePointY;
                                        y *= oppositeEllipsePoints[innerPointIndex].X - ellipsePoints[pointIndex].X;
                                        y += ellipsePoints[pointIndex].Y;

                                        if (y > oppositeEllipsePoints[innerPointIndex].Y)
                                        {
                                            pointsOnLeft = true;
                                        }
                                        if (y < oppositeEllipsePoints[innerPointIndex].Y)
                                        {
                                            pointsOnRight = true;
                                        }
                                        if (pointsOnLeft && pointsOnRight)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!pointsOnLeft && leftBottomPoint == -1)
                            {
                                leftBottomPoint = pointIndex;
                                _oppLeftBottomPoint = pointOppositeIndex;
                            }
                            if (!pointsOnRight && rigthTopPoint == -1)
                            {
                                rigthTopPoint = pointIndex;
                                _oppRigthTopPoint = pointOppositeIndex;
                            }

                            if (leftBottomPoint >= 0 && rigthTopPoint >= 0)
                            {
                                doneFlag = true;

                                if (closeToVertical && ellipsePoints[leftBottomPoint].Y > oppositeEllipsePoints[_oppLeftBottomPoint].Y)
                                {
                                    int temp = leftBottomPoint;
                                    leftBottomPoint = rigthTopPoint;
                                    rigthTopPoint = temp;

                                    temp = _oppLeftBottomPoint;
                                    _oppLeftBottomPoint = _oppRigthTopPoint;
                                    _oppRigthTopPoint = temp;
                                }
                            }
                        }
                    }
                }
            }

            // Point indexes were not found
            if (leftBottomPoint == rigthTopPoint ||
                rigthTopPoint == -1 ||
                leftBottomPoint == -1)
            {
                return;
            }

            // Set left\right line coordinates
            leftSideLinePoint = ellipsePoints[leftBottomPoint];
            rightSideLinePoint = ellipsePoints[rigthTopPoint];

            // Add required ellipse segment to the result path
            for (int pointIndex = leftBottomPoint + 1; pointIndex != rigthTopPoint + 1; pointIndex++)
            {
                if (pointIndex > ellipsePoints.Length - 1)
                {
                    resultPath.AddLine(ellipsePoints[^1], ellipsePoints[0]);
                    pointIndex = 0;
                    continue;
                }
                resultPath.AddLine(ellipsePoints[pointIndex - 1], ellipsePoints[pointIndex]);
            }
        }

        #endregion 3D Cylinder drawing methods
    }

    /// <summary>
    /// The Point3D class represents point coordinates in 3D space.
    /// </summary>
    public class Point3D
    {
        #region Fields

        // Point X and Y coordinates
        private SKPoint _coordXY = new(0f, 0f);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the X coordinate of the point.
        /// </summary>
        [
        SRDescription("DescriptionAttributePoint3D_X")
        ]
        public float X
        {
            get
            {
                return _coordXY.X;
            }
            set
            {
                _coordXY.X = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the point.
        /// </summary>
        [
        SRDescription("DescriptionAttributePoint3D_Y")
        ]
        public float Y
        {
            get
            {
                return _coordXY.Y;
            }
            set
            {
                _coordXY.Y = value;
            }
        }

        /// <summary>
        /// Gets or sets the Z coordinate of the point.
        /// </summary>
        [
        SRDescription("DescriptionAttributePoint3D_Z")
        ]
        public float Z { get; set; } = 0;

        /// <summary>
        /// Gets or sets a SKPoint structure, which stores the X and Y coordinates of a 3D point.
        /// </summary>
        [
        SRDescription("DescriptionAttributePoint3D_SKPoint")
        ]
        public SKPoint SKPoint
        {
            get
            {
                return _coordXY;
            }
            set
            {
                _coordXY = new SKPoint(value.X, value.Y);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Public constructor.
        /// </summary>
        public Point3D(float x, float y, float z)
        {
            _coordXY = new SKPoint(x, y);
            Z = z;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        public Point3D()
        {
        }

        #endregion Constructors
    }
}