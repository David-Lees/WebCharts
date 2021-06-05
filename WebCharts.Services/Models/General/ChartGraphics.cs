// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Chart graphic class is used for drawing Chart
//				elements as Rectangles, Pie slices, lines, areas
//				etc. This class is used in all classes where
//				drawing is necessary. The GDI+ graphic class is
//				used throw this class. Encapsulates a GDI+ chart
//				drawing functionality
//

using SkiaSharp;
using System;
using System.Linq;
using System.Text;

namespace WebCharts.Services
{
    /// <summary>
    /// The ChartGraphics class provides all chart drawing capabilities.
    /// It contains methods for drawing 2D primitives and also exposes
    /// all ChartGraphics3D class methods for 3D shapes. Only this
    /// class should be used for any drawing in the chart.
    /// </summary>
    public partial class ChartGraphics : ChartElement
    {
        public SmoothingMode SmoothingMode { get; set; }

        #region Fields

        // True if rendering into the metafile
        internal bool IsMetafile = false;

        // Indicates that smoothing is applied while drawing shadows
        internal bool softShadows = true;

        // Common Elements
        private readonly CommonElements _common;

        // Anti aliasing flags
        private AntiAliasingStyles _antiAliasing = AntiAliasingStyles.All;

        private int _height;

        private SKMatrix _myMatrix;

        // Reusable objects
        private SKPaint _pen;

        private SKPaint _solidBrush;
        // Private fields which represents picture size
        private int _width;
        #endregion Fields

        #region Lines Methods

        /// <summary>
        /// Draws a line connecting the two specified points using absolute coordinates.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="firstPoint">A Point that represents the first point to connect.</param>
        /// <param name="secondPoint">A Point that represents the second point to connect.</param>
        internal void DrawLineAbs(
            SKColor color,
            int width,
            ChartDashStyle style,
            SKPoint firstPoint,
            SKPoint secondPoint
            )
        {
            // Do not draw line if width is 0 or style not set
            if (width == 0 || style == ChartDashStyle.NotSet)
            {
                return;
            }

            // Set a line color
            if (_pen.Color != color)
            {
                _pen.Color = color;
            }

            // Set a line width
            if (_pen.StrokeWidth != width)
            {
                _pen.StrokeWidth = width;
            }

            // Set a line style
            if (_pen.PathEffect != GetPenStyle(style, width))
            {
                _pen.PathEffect = GetPenStyle(style, width);
            }

            // Remember SmoothingMode and turn off anti aliasing for
            // vertical or horizontal lines usinig 1 pixel dashed pen.
            // This prevents anialiasing from completly smoothing the
            // dashed line.
            SmoothingMode oldSmoothingMode = this.SmoothingMode;
            if (width <= 1 && style != ChartDashStyle.Solid)
            {
                if (firstPoint.X == secondPoint.X ||
                    firstPoint.Y == secondPoint.Y)
                {
                    this.SmoothingMode = SmoothingMode.None;
                }
            }

            // Draw a line
            DrawLine(_pen,
                (float)Math.Round(firstPoint.X),
                (float)Math.Round(firstPoint.Y),
                (float)Math.Round(secondPoint.X),
                (float)Math.Round(secondPoint.Y));

            // Return old smoothing mode
            this.SmoothingMode = oldSmoothingMode;
        }

        /// <summary>
        /// Draws a line with shadow connecting the two specified points.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="firstPoint">A Point that represents the first point to connect.</param>
        /// <param name="secondPoint">A Point that represents the second point to connect.</param>
        /// <param name="shadowColor">Shadow Color.</param>
        /// <param name="shadowOffset">Shadow Offset.</param>
        internal void DrawLineAbs(
            SKColor color,
            int width,
            ChartDashStyle style,
            SKPoint firstPoint,
            SKPoint secondPoint,
            SKColor shadowColor,
            int shadowOffset
            )
        {
            if (shadowOffset != 0)
            {
                // Shadow color
                SKColor shColor;

                // Make shadow semi transparent
                // if alpha value not used
                if (shadowColor.Alpha != 255)
                    shColor = shadowColor;
                else
                    shColor = new SKColor(shadowColor.Red, shadowColor.Green, shadowColor.Blue, (byte)(color.Alpha / 2));

                // Set shadow line position
                var firstShadow = new SKPoint(firstPoint.X + shadowOffset, firstPoint.Y + shadowOffset);
                var secondShadow = new SKPoint(secondPoint.X + shadowOffset, secondPoint.Y + shadowOffset);

                // Draw Shadow of Line
                DrawLineAbs(shColor, width, style, firstShadow, secondShadow);
            }

            // Draw Line
            DrawLineAbs(color, width, style, firstPoint, secondPoint);
        }

        /// <summary>
        /// Draws a line connecting the two specified points.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="firstSKPoint">A Point that represents the first point to connect.</param>
        /// <param name="secondSKPoint">A Point that represents the second point to connect.</param>
        internal void DrawLineRel(
            SKColor color,
            int width,
            ChartDashStyle style,
            SKPoint firstSKPoint,
            SKPoint secondSKPoint
            )
        {
            DrawLineAbs(
                color,
                width,
                style,
                GetAbsolutePoint(firstSKPoint),
                GetAbsolutePoint(secondSKPoint));
        }

        /// <summary>
        /// Draws a line with shadow connecting the two specified points.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="firstPoint">A Point that represents the first point to connect.</param>
        /// <param name="secondPoint">A Point that represents the second point to connect.</param>
        /// <param name="shadowColor">Shadow Color.</param>
        /// <param name="shadowOffset">Shadow Offset.</param>
        internal void DrawLineRel(
            SKColor color,
            int width,
            ChartDashStyle style,
            SKPoint firstPoint,
            SKPoint secondPoint,
            SKColor shadowColor,
            int shadowOffset
            )
        {
            DrawLineAbs(
                color,
                width,
                style,
                GetAbsolutePoint(firstPoint),
                GetAbsolutePoint(secondPoint),
                shadowColor,
                shadowOffset);
        }
        #endregion Lines Methods

        #region Pen and Brush Methods

        /// <summary>
        /// This method creates a gradient brush.
        /// </summary>
        /// <param name="rectangle">A rectangle which has to be filled with a gradient color.</param>
        /// <param name="firstColor">First color.</param>
        /// <param name="secondColor">Second color.</param>
        /// <param name="type ">Gradient type .</param>
        /// <returns>Gradient Brush</returns>
        internal static SKPaint GetGradientBrush(
            SKRect rectangle,
            SKColor firstColor,
            SKColor secondColor,
            GradientStyle type
            )
        {
            // Increse the brush rectangle by 1 pixel to ensure the fit
            rectangle.Inflate(1f, 1f);

            SKPaint gradientBrush = new();

            // Function which create gradient brush fires exception if
            // rectangle size is zero.
            if (rectangle.Height == 0 || rectangle.Width == 0)
            {
                gradientBrush.Color = SKColors.Black;
                return gradientBrush;
            }

            // Create a linear gradient brush
            if (type != GradientStyle.Center)
            {
                SKRect tempRect = new(0, rectangle.Top, rectangle.Width, rectangle.Top + rectangle.Height);

                // Resize and wrap gradient
                tempRect = new(tempRect.Left, tempRect.Top, tempRect.Right, tempRect.Top + tempRect.Height / 2F);

                var start = type == GradientStyle.DiagonalLeft ? new SKPoint(tempRect.Width, 0) : new SKPoint(0, 0);
                SKPoint end = type switch
                {
                    GradientStyle.LeftRight => new(tempRect.Width, 0),
                    GradientStyle.VerticalCenter => new(tempRect.Width, 0),
                    GradientStyle.TopBottom => new(0, tempRect.Height),
                    GradientStyle.HorizontalCenter => new(0, tempRect.Height),
                    GradientStyle.DiagonalLeft => new(0, tempRect.Height),
                    GradientStyle.DiagonalRight => new(tempRect.Width, tempRect.Height),
                    GradientStyle.None => throw new NotImplementedException(),
                    GradientStyle.Center => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };

                gradientBrush.Shader = SKShader.CreateLinearGradient(
                    start, end,
                    new SKColor[] { firstColor, secondColor },
                     SKShaderTileMode.Mirror);

                return gradientBrush;
            }

            // *******************************************
            // Gradient is not linear : From Center.
            // *******************************************

            // Create a gradient brush
            gradientBrush.Shader = SKShader.CreateRadialGradient(
                new SKPoint(rectangle.MidX, rectangle.MidY),
                Math.Max(rectangle.Width, rectangle.Height) / 2,
                new SKColor[] { firstColor, secondColor },
                SKShaderTileMode.Clamp);

            return gradientBrush;
        }

        /// <summary>
        /// Creates a Hatch Brush.
        /// </summary>
        /// <param name="hatchStyle">Chart Hatch style.</param>
        /// <param name="backColor">Back Color.</param>
        /// <param name="foreColor">Fore Color.</param>
        /// <returns>Brush</returns>
        internal static SKPaint GetHatchBrush(
            ChartHatchStyle hatchStyle,
            SKColor backColor,
            SKColor foreColor
            )
        {
            const float hatchWidth = 20;

            // create the path (diagonal hatch) with the center at 0,0
            var hatchPath = new SKPath();
            hatchPath.MoveTo(0, hatchWidth);
            hatchPath.LineTo(hatchWidth, 0);
            hatchPath.LineTo(0, -hatchWidth);
            hatchPath.LineTo(-hatchWidth, 0);
            hatchPath.LineTo(0, hatchWidth);

            // the size of the pattern
            var hatchMatrix = SKMatrix.CreateScale(hatchWidth * 2, hatchWidth * 2);
            // create the paint
            var hatchPaint = new SKPaint
            {
                PathEffect = SKPathEffect.Create2DPath(hatchMatrix, hatchPath),
                Color = foreColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            return hatchPaint;
        }

        internal static SKPathEffect GetPenStyle(ChartDashStyle style, float width)
        {
            // Convert to chart line styles. The custom style doesn’t exist.
            return style switch
            {
                ChartDashStyle.Dash => SKPathEffect.CreateDash(new[] { width * 5.0f, width * 5.0f }, 10),
                ChartDashStyle.DashDot => SKPathEffect.CreateDash(new[] { width * 5.0f, width * 5.0f, width, width }, width * 12),
                ChartDashStyle.DashDotDot => SKPathEffect.CreateDash(new[] { width * 5.0f, width * 5.0f, width, width, width, width }, width * 14),
                ChartDashStyle.Dot => SKPathEffect.CreateDash(new[] { width, width }, width * 2),
                _ => SKPathEffect.CreateDash(new[] { 1.0f, 0 }, 1),
            };
        }

        /// <summary>
        /// This method creates a gradient brush for pie. This gradient is one
        /// of the types used only with pie and doughnut.
        /// </summary>
        /// <param name="rectangle">A rectangle which has to be filled with a gradient color</param>
        /// <param name="firstColor">First color</param>
        /// <param name="secondColor">Second color</param>
        /// <returns>Gradient Brush</returns>
        internal static SKPaint GetPieGradientBrush(
            SKRect rectangle,
            SKColor firstColor,
            SKColor secondColor
            )
        {
            var brush = new SKPaint
            {
                Shader = SKShader.CreateRadialGradient(
                new SKPoint(rectangle.MidX, rectangle.MidY),
                Math.Max(rectangle.Width, rectangle.Height),
                new SKColor[] { firstColor, secondColor },
                SKShaderTileMode.Clamp)
            };

            return brush;
        }

        /// <summary>
        /// Creates a textured brush.
        /// </summary>
        /// <param name="name">Image file name or URL.</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="mode">Wrap mode.</param>
        /// <param name="backColor">Image background color.</param>
        /// <returns>Textured brush.</returns>
        internal SKPaint GetTextureBrush(
            string name,
            SKColor backImageTransparentColor,
            ChartImageWrapMode mode,
            SKColor backColor
            )
        {
            // Load a image
            var image = _common.ImageLoader.LoadImage(name);

            return new SKPaint()
            {
                Shader = SKShader.CreateImage(image),
                Style = SKPaintStyle.Fill,
            };
        }
        #endregion Pen and Brush Methods

        #region Markers

        /// <summary>
        /// Creates polygon for multi-corner star marker.
        /// </summary>
        /// <param name="rect">Marker rectangle.</param>
        /// <param name="numberOfCorners">Number of corners (4 and up).</param>
        /// <returns>Array of points.</returns>
        internal static SKPoint[] CreateStarPolygon(SKRect rect, int numberOfCorners)
        {
            int numberOfCornersX2;
            checked
            {
                numberOfCornersX2 = numberOfCorners * 2;
            }

            bool outside = true;
            SKPoint[] points = new SKPoint[numberOfCornersX2];
            SKPoint[] tempPoints = new SKPoint[1];
            // overflow check
            for (int pointIndex = 0; pointIndex < numberOfCornersX2; pointIndex++)
            {
                tempPoints[0] = new SKPoint(rect.Left + rect.Width / 2f, (outside) ? rect.Top : rect.Top + rect.Height / 4f);
                SKMatrix matrix = SKMatrix.CreateRotationDegrees(pointIndex * (360f / (numberOfCorners * 2f)), rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f);
                matrix.TransformPoints(tempPoints);
                points[pointIndex] = tempPoints[0];
                outside = !outside;
            }

            return points;
        }

        /// <summary>
        /// Draw marker using absolute coordinates of the center.
        /// </summary>
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
        /// <param name="forceAntiAlias">Always use anti aliasing when drawing the marker.</param>
        internal void DrawMarkerAbs(
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
            bool forceAntiAlias
            )
        {
            // Hide border when zero width specified
            if (markerBorderSize <= 0)
            {
                markerBorderColor = SKColors.Transparent;
            }

            // Draw image instead of standart markers
            if (markerImage.Length > 0)
            {
                // Get image
                SKImage image = _common.ImageLoader.LoadImage(markerImage);

                if (image != null)
                {
                    // Calculate image rectangle
                    SKRect rect = SKRect.Empty;
                    if (imageScaleRect == SKRect.Empty)
                    {
                        SKSize size = new();
                        ImageLoader.GetAdjustedImageSize(image, Graphics, ref size);
                        imageScaleRect.Size = new(size.Width, size.Height);
                    }

                    rect.Left = point.X - imageScaleRect.Width / 2F;
                    rect.Top = point.Y - imageScaleRect.Height / 2F;
                    rect.Size = new(imageScaleRect.Width, imageScaleRect.Height);

                    using SKPaint paint = new() { Style = SKPaintStyle.Fill };

                    // Draw image shadow
                    if (shadowSize != 0 && shadowColor != SKColor.Empty)
                    {
                        paint.ImageFilter = SKImageFilter.CreateDropShadow(shadowSize, shadowSize, shadowSize, shadowSize, shadowColor);
                    }

                    // Draw image
                    DrawImage(image,
                        new SKRect((int)rect.Left, (int)rect.Left, (int)rect.Width, (int)rect.Height),
                        0, 0, image.Width, image.Height, paint);
                }
            }

            // Draw standart marker using style, size and color
            else if (markerStyle != MarkerStyle.None && markerSize > 0 && markerColor != SKColor.Empty)
            {
                // Enable antialising
                SmoothingMode oldSmoothingMode = SmoothingMode;
                if (forceAntiAlias)
                {
                    SmoothingMode = SmoothingMode.AntiAlias;
                }

                // Create solid color brush
                using (SKPaint brush = new() { Color = markerColor, Style = SKPaintStyle.Fill })
                {
                    // Calculate marker rectangle
                    SKRect rect = SKRect.Empty;
                    rect.Left = point.X - markerSize / 2F;
                    rect.Top = point.Y - markerSize / 2F;
                    rect.Size = new(markerSize, markerSize);

                    // Draw marker depending on style
                    switch (markerStyle)
                    {
                        case (MarkerStyle.Star4):
                        case (MarkerStyle.Star5):
                        case (MarkerStyle.Star6):
                        case (MarkerStyle.Star10):
                            {
                                // Set number of corners
                                int cornerNumber = 4;
                                if (markerStyle == MarkerStyle.Star5)
                                {
                                    cornerNumber = 5;
                                }
                                else if (markerStyle == MarkerStyle.Star6)
                                {
                                    cornerNumber = 6;
                                }
                                else if (markerStyle == MarkerStyle.Star10)
                                {
                                    cornerNumber = 10;
                                }

                                // Get star polygon
                                SKPoint[] points = CreateStarPolygon(rect, cornerNumber);

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    SKMatrix translateMatrix = Transform;
                                    translateMatrix.Translate(shadowSize, shadowSize);
                                    SKMatrix oldMatrix = Transform;
                                    Transform = translateMatrix;

                                    using var p = new SKPaint()
                                    {
                                        Color = (shadowColor.Alpha != 255) ? shadowColor : Color.FromArgb(markerColor.Alpha / 2, shadowColor)
                                    };
                                    FillPolygon(p, points);

                                    Transform = oldMatrix;
                                }

                                // Draw star
                                FillPolygon(brush, points);
                                using var starPen = new SKPaint() { Style = SKPaintStyle.Stroke, Color = markerBorderColor, StrokeWidth = markerBorderSize };
                                DrawPolygon(starPen, points);
                                break;
                            }
                        case (MarkerStyle.Circle):
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    if (!softShadows)
                                    {
                                        using SKPaint shadowBrush = new() { Style = SKPaintStyle.Fill, Color = (shadowColor.Alpha != 255) ? shadowColor : Color.FromArgb(markerColor.Alpha / 2, shadowColor) };
                                        SKRect shadowRect = rect;
                                        shadowRect.Left += shadowSize;
                                        shadowRect.Top += shadowSize;
                                        FillEllipse(shadowBrush, shadowRect);
                                    }
                                    else
                                    {
                                        // Add circle to the graphics path
                                        using SKPath path = new();
                                        path.AddOval(new SKRect(rect.Left + shadowSize - 1, rect.Top + shadowSize - 1, rect.Width + 2, rect.Height + 2));

                                        // Create path brush
                                        using SKPaint shadowBrush = new();
                                        shadowBrush.Shader = SKShader.CreateRadialGradient(
                                            new SKPoint(point.X, point.Y),
                                            rect.Width / 2,
                                            new SKColor[] { shadowColor, SKColors.Transparent },
                                            SKShaderTileMode.Clamp
                                            );

                                        // Draw shadow
                                        FillPath(shadowBrush, path);
                                    }
                                }

                                FillEllipse(brush, rect);
                                DrawEllipse(new SKPaint() { Color = markerBorderColor, StrokeWidth = markerBorderSize, Style = SKPaintStyle.Stroke }, rect);
                                break;
                            }
                        case (MarkerStyle.Square):
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    FillRectangleShadowAbs(rect, shadowColor, shadowSize, shadowColor);
                                }

                                FillRectangle(brush, rect);
                                DrawRectangle(new SKPaint() { Color = markerBorderColor, StrokeWidth = markerBorderSize, Style = SKPaintStyle.Stroke }, (int)Math.Round(rect.Left, 0), (int)Math.Round(rect.Top, 0), (int)Math.Round(rect.Width, 0), (int)Math.Round(rect.Height, 0));
                                break;
                            }
                        case (MarkerStyle.Cross):
                            {
                                // Calculate cross line width and size
                                float crossLineWidth = (float)Math.Ceiling(markerSize / 4F);
                                float crossSize = markerSize;// * (float)Math.Sin(45f/180f*Math.PI)

                                // Calculate cross coordinates
                                SKPoint[] points = new SKPoint[12];
                                points[0].X = point.X - crossSize / 2F;
                                points[0].Y = point.Y + crossLineWidth / 2F;
                                points[1].X = point.X - crossSize / 2F;
                                points[1].Y = point.Y - crossLineWidth / 2F;

                                points[2].X = point.X - crossLineWidth / 2F;
                                points[2].Y = point.Y - crossLineWidth / 2F;
                                points[3].X = point.X - crossLineWidth / 2F;
                                points[3].Y = point.Y - crossSize / 2F;
                                points[4].X = point.X + crossLineWidth / 2F;
                                points[4].Y = point.Y - crossSize / 2F;

                                points[5].X = point.X + crossLineWidth / 2F;
                                points[5].Y = point.Y - crossLineWidth / 2F;
                                points[6].X = point.X + crossSize / 2F;
                                points[6].Y = point.Y - crossLineWidth / 2F;
                                points[7].X = point.X + crossSize / 2F;
                                points[7].Y = point.Y + crossLineWidth / 2F;

                                points[8].X = point.X + crossLineWidth / 2F;
                                points[8].Y = point.Y + crossLineWidth / 2F;
                                points[9].X = point.X + crossLineWidth / 2F;
                                points[9].Y = point.Y + crossSize / 2F;
                                points[10].X = point.X - crossLineWidth / 2F;
                                points[10].Y = point.Y + crossSize / 2F;
                                points[11].X = point.X - crossLineWidth / 2F;
                                points[11].Y = point.Y + crossLineWidth / 2F;

                                // Rotate cross coordinates 45 degrees
                                SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees(45, point.X, point.Y);
                                rotationMatrix.TransformPoints(points);

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    // Create translation matrix
                                    SKMatrix translateMatrix = Transform;
                                    translateMatrix.Translate(
                                        (softShadows) ? shadowSize + 1 : shadowSize,
                                        (softShadows) ? shadowSize + 1 : shadowSize);
                                    SKMatrix oldMatrix = Transform;
                                    Transform = translateMatrix;

                                    if (!softShadows)
                                    {
                                        using SKPaint softShadowBrush = new() { Style = SKPaintStyle.Fill, Color = (shadowColor.Alpha != 255) ? shadowColor : Color.FromArgb(markerColor.Alpha / 2, shadowColor) };
                                        FillPolygon(softShadowBrush, points);
                                    }
                                    else
                                    {
                                        // Add polygon to the graphics path
                                        using SKPath path = new();
                                        path.AddPoly(points);

                                        // Create path brush
                                        using SKPaint shadowBrush = new() { Style = SKPaintStyle.Fill, Color = SKColors.Transparent };
                                        // Define brush focus scale
                                        SKPoint focusScale = new(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                        if (focusScale.X < 0)
                                        {
                                            focusScale.X = 0;
                                        }
                                        if (focusScale.Y < 0)
                                        {
                                            focusScale.Y = 0;
                                        }

                                        // set drop shadow with position x/y of 2, and blur x/y of 4
                                        shadowBrush.ImageFilter = SKImageFilter.CreateDropShadowOnly(point.X, point.Y, focusScale.X, focusScale.Y, shadowColor);

                                        // Draw shadow
                                        FillPath(shadowBrush, path);
                                    }

                                    Transform = oldMatrix;
                                }

                                // Create translation matrix
                                SKMatrix translateMatrixShape = Transform;
                                SKMatrix oldMatrixShape = Transform;
                                Transform = translateMatrixShape;

                                FillPolygon(brush, points);
                                using var pp = new SKPaint() { Style = SKPaintStyle.Stroke, Color = markerBorderColor, StrokeWidth = markerBorderSize };
                                DrawPolygon(pp, points);

                                Transform = oldMatrixShape;

                                break;
                            }
                        case (MarkerStyle.Diamond):
                            {
                                SKPoint[] points = new SKPoint[4];
                                points[0].X = rect.Left;
                                points[0].Y = rect.Top + rect.Height / 2F;
                                points[1].X = rect.Left + rect.Width / 2F;
                                points[1].Y = rect.Top;
                                points[2].X = rect.Right;
                                points[2].Y = rect.Top + rect.Height / 2F;
                                points[3].X = rect.Left + rect.Width / 2F;
                                points[3].Y = rect.Bottom;

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    SKMatrix translateMatrix = Transform;
                                    translateMatrix.Translate((softShadows) ? 0 : shadowSize,
                                        (softShadows) ? 0 : shadowSize);
                                    SKMatrix oldMatrix = Transform;
                                    Transform = translateMatrix;

                                    if (!softShadows)
                                    {
                                        using SKPaint softShadowBrush = new() { Color = (shadowColor.Alpha != 255) ? shadowColor : Color.FromArgb(markerColor.Alpha / 2, shadowColor), Style = SKPaintStyle.Fill };
                                        FillPolygon(softShadowBrush, points);
                                    }
                                    else
                                    {
                                        // Calculate diamond size
                                        float diamondSize = markerSize * (float)Math.Sin(45f / 180f * Math.PI);

                                        // Calculate diamond rectangle position
                                        SKRect diamondRect = SKRect.Empty;
                                        diamondRect.Left = point.X - diamondSize / 2F;
                                        diamondRect.Top = point.Y - diamondSize / 2F - shadowSize;
                                        diamondRect.Size = new(diamondSize, diamondSize);

                                        // Set rotation matrix to 45
                                        translateMatrix = SKMatrix.CreateRotationDegrees(45, point.X, point.Y);
                                        Transform = translateMatrix;

                                        FillRectangleShadowAbs(diamondRect, shadowColor, shadowSize, shadowColor);
                                    }

                                    Transform = oldMatrix;
                                }

                                FillPolygon(brush, points);
                                DrawPolygon(new SKPaint() { Style = SKPaintStyle.Stroke, Color = markerBorderColor, StrokeWidth = markerBorderSize }, points);
                                break;
                            }
                        case (MarkerStyle.Triangle):
                            {
                                SKPoint[] points = new SKPoint[3];
                                points[0].X = rect.Left;
                                points[0].Y = rect.Bottom;
                                points[1].X = rect.Left + rect.Width / 2F;
                                points[1].Y = rect.Top;
                                points[2].X = rect.Right;
                                points[2].Y = rect.Bottom;

                                var pen = new SKPaint() { Style = SKPaintStyle.Stroke, Color = markerBorderColor, StrokeWidth = markerBorderSize };

                                // Draw image shadow
                                if (shadowSize != 0 && shadowColor != SKColor.Empty)
                                {
                                    if (!softShadows)
                                    {
                                        pen.ImageFilter = SKImageFilter.CreateDropShadow(shadowSize, shadowSize, 0, 0, shadowColor);
                                    }
                                    else
                                    {
                                        pen.ImageFilter = SKImageFilter.CreateDropShadow(shadowSize, shadowSize, shadowSize + 1, shadowSize + 1, shadowColor);
                                    }
                                }

                                FillPolygon(brush, points);
                                DrawPolygon(pen, points);
                                break;
                            }
                        default:
                            {
                                throw new InvalidOperationException(SR.ExceptionGraphicsMarkerStyleUnknown);
                            }
                    }
                }

                // Restore SmoothingMode
                if (forceAntiAlias)
                {
                    SmoothingMode = oldSmoothingMode;
                }
            }
        }

        /// <summary>
        /// Draw marker using relative coordinates of the center.
        /// </summary>
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
        internal void DrawMarkerRel(
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
            SKRect imageScaleRect
            )
        {
            DrawMarkerAbs(GetAbsolutePoint(point), markerStyle, markerSize, markerColor, markerBorderColor, markerBorderSize, markerImage, markerImageTransparentColor, shadowSize, shadowColor, imageScaleRect, false);
        }
        #endregion Markers

        #region String Methods

        /// <summary>
        /// Measures the specified text string when drawn with
        /// the specified Font object and formatted with the
        /// specified StringFormat object.
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The Font object used to determine the size of the text string. </param>
        /// <param name="layoutArea">A SKSize structure that specifies the layout rectangle for the text. </param>
        /// <param name="stringFormat">A StringFormat object that represents formatting information, such as line spacing, for the text string. </param>
        /// <param name="textOrientation">Text orientation.</param>
        /// <returns>A SKSize structure that represents the size of text as drawn with font.</returns>
        public SKSize MeasureStringRel(
            string text,
            SKFont font,
            SKSize layoutArea,
            StringFormat stringFormat,
            TextOrientation textOrientation)
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or
            // correctly handle text wrapping.
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }
            return MeasureStringRel(text, font, layoutArea, stringFormat);
        }

        /// <summary>
        /// Function returned stacked text by inserting new line characters between
        /// all characters in the original string.
        /// </summary>
        /// <param name="text">Original text.</param>
        /// <returns>Stacked text.</returns>
        internal static string GetStackedText(string text)
        {
            StringBuilder result = new();
            foreach (char ch in text)
            {
                result.Append(ch);
                if (ch != '\n')
                {
                    result.Append('\n');
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Draw label string.
        /// </summary>
        /// <param name="axis">Label axis.</param>
        /// <param name="labelRowIndex">Label text row index (0-10).</param>
        /// <param name="labelMark">Second row labels mark style.</param>
        /// <param name="markColor">Label mark line color.</param>
        /// <param name="text">Label text.</param>
        /// <param name="image">Label image name.</param>
        /// <param name="imageTransparentColor">Label image transparent color.</param>
        /// <param name="font">Text bont.</param>
        /// <param name="brush">Text brush.</param>
        /// <param name="position">Text position rectangle.</param>
        /// <param name="format">Label text format.</param>
        /// <param name="angle">Label text angle.</param>
        /// <param name="boundaryRect">Specifies the rectangle where the label text MUST be fitted.</param>
        /// <param name="label">Custom Label Item</param>
        /// <param name="truncatedLeft">Label is truncated on the left.</param>
        /// <param name="truncatedRight">Label is truncated on the right.</param>
        internal void DrawLabelStringRel(
            Axis axis,
            int labelRowIndex,
            LabelMarkStyle labelMark,
            SKColor markColor,
            string text,
            string image,
            SKColor imageTransparentColor,
            SKFont font,
            SKPaint brush,
            SKRect position,
            StringFormat format,
            int angle,
            SKRect boundaryRect,
            CustomLabel label,
            bool truncatedLeft,
            bool truncatedRight)
        {
            SKMatrix oldTransform;
            using (StringFormat drawingFormat = (StringFormat)format.Clone())
            {
                SKSize labelSize = SKSize.Empty;

                // Check that rectangle is not empty
                if (position.Width == 0 || position.Height == 0)
                {
                    return;
                }

                // Find absolute position
                SKRect absPosition = GetAbsoluteRectangle(position);

                // Make sure the rectangle is not empty
                if (absPosition.Width < 1f)
                {
                    absPosition.Right = absPosition.Left + 1f;
                }
                if (absPosition.Height < 1f)
                {
                    absPosition.Bottom = absPosition.Top + 1f;
                }

                CommonElements common = axis.Common;
                if (common.ProcessModeRegions)
                {
                    common.HotRegionsList.AddHotRegion(absPosition.Round(), label, ChartElementType.AxisLabels, false, true);
                }

                //********************************************************************
                //** Draw labels in the second row
                //********************************************************************
                if (labelRowIndex > 0)
                {
                    drawingFormat.LineAlignment = StringAlignment.Center;
                    drawingFormat.Alignment = StringAlignment.Center;
                    angle = 0;

                    if (axis.AxisPosition == AxisPosition.Left)
                    {
                        angle = -90;
                    }
                    else if (axis.AxisPosition == AxisPosition.Right)
                    {
                        angle = 90;
                    }
                    else if (axis.AxisPosition == AxisPosition.Top)
                    {
                    }
                    else if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                    }
                }

                //********************************************************************
                //** Calculate rotation point
                //********************************************************************
                SKPoint rotationPoint = SKPoint.Empty;
                if (axis.AxisPosition == AxisPosition.Left)
                {
                    rotationPoint.X = absPosition.Right;
                    rotationPoint.Y = absPosition.Top + absPosition.Height / 2F;
                }
                else if (axis.AxisPosition == AxisPosition.Right)
                {
                    rotationPoint.X = absPosition.Left;
                    rotationPoint.Y = absPosition.Top + absPosition.Height / 2F;
                }
                else if (axis.AxisPosition == AxisPosition.Top)
                {
                    rotationPoint.X = absPosition.Left + absPosition.Width / 2F;
                    rotationPoint.Y = absPosition.Bottom;
                }
                else if (axis.AxisPosition == AxisPosition.Bottom)
                {
                    rotationPoint.X = absPosition.Left + absPosition.Width / 2F;
                    rotationPoint.Y = absPosition.Top;
                }

                //********************************************************************
                //** Adjust rectangle for horisontal axis
                //********************************************************************
                if ((axis.AxisPosition == AxisPosition.Top || axis.AxisPosition == AxisPosition.Bottom) &&
                    angle != 0)
                {
                    // Get rectangle center
                    rotationPoint.X = absPosition.Left + absPosition.Width / 2F;
                    rotationPoint.Y = (axis.AxisPosition == AxisPosition.Top) ? absPosition.Bottom : absPosition.Top;

                    // Rotate rectangle 90 degrees
                    SKRect newRect = SKRect.Empty;
                    newRect.Left = absPosition.Left + absPosition.Width / 2F;
                    newRect.Top = absPosition.Top - absPosition.Width / 2F;
                    newRect.Size = new(absPosition.Height, absPosition.Width);

                    // Adjust values for bottom axis
                    if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                        if (angle < 0)
                        {
                            newRect.Left -= newRect.Width;
                        }

                        // Replace string alignment
                        drawingFormat.Alignment = StringAlignment.Near;
                        if (angle < 0)
                        {
                            drawingFormat.Alignment = StringAlignment.Far;
                        }
                        drawingFormat.LineAlignment = StringAlignment.Center;
                    }

                    // Adjust values for bottom axis
                    if (axis.AxisPosition == AxisPosition.Top)
                    {
                        newRect.Top += absPosition.Height;
                        if (angle > 0)
                        {
                            newRect.Left -= newRect.Width;
                        }

                        // Replace string alignment
                        drawingFormat.Alignment = StringAlignment.Far;
                        if (angle < 0)
                        {
                            drawingFormat.Alignment = StringAlignment.Near;
                        }
                        drawingFormat.LineAlignment = StringAlignment.Center;
                    }

                    // Set new label rect
                    absPosition = newRect;
                }

                //********************************************************************
                //** 90 degrees is a special case for vertical axes
                //********************************************************************
                if ((axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right) &&
                    (angle == 90 || angle == -90))
                {
                    // Get rectangle center
                    rotationPoint.X = absPosition.Left + absPosition.Width / 2F;
                    rotationPoint.Y = absPosition.Top + absPosition.Height / 2F;

                    // Rotate rectangle 90 degrees
                    SKRect newRect = SKRect.Empty;
                    newRect.Left = rotationPoint.X - absPosition.Height / 2F;
                    newRect.Top = rotationPoint.Y - absPosition.Width / 2F;
                    newRect.Size = new(absPosition.Height, absPosition.Width);
                    absPosition = newRect;

                    // Replace string alignment
                    StringAlignment align = drawingFormat.Alignment;
                    drawingFormat.Alignment = drawingFormat.LineAlignment;
                    drawingFormat.LineAlignment = align;
                    if (angle == 90)
                    {
                        if (drawingFormat.LineAlignment == StringAlignment.Far)
                            drawingFormat.LineAlignment = StringAlignment.Near;
                        else if (drawingFormat.LineAlignment == StringAlignment.Near)
                            drawingFormat.LineAlignment = StringAlignment.Far;
                    }
                    if (angle == -90)
                    {
                        if (drawingFormat.Alignment == StringAlignment.Far)
                            drawingFormat.Alignment = StringAlignment.Near;
                        else if (drawingFormat.Alignment == StringAlignment.Near)
                            drawingFormat.Alignment = StringAlignment.Far;
                    }
                }

                //********************************************************************
                //** Create a matrix and rotate it.
                //********************************************************************
                oldTransform = SKMatrix.Empty;
                if (angle != 0)
                {
                    _myMatrix = Transform = SKMatrix.CreateRotationDegrees(angle, rotationPoint.X, rotationPoint.Y);

                    // Old angle
                    oldTransform = Transform;

                    // Set Angle
                    Transform = _myMatrix;
                }

                //********************************************************************
                //** Measure string exact rectangle and adjust label bounding rectangle
                //********************************************************************
                SKRect labelRect = SKRect.Empty;
                float offsetY = 0f;
                float offsetX = 0f;

                // Measure text size
                labelSize = this.MeasureString(text.Replace("\\n", "\n"), font, absPosition.Size, drawingFormat);

                // Calculate text rectangle
                labelRect.Size = new(labelSize.Width, labelSize.Height);
                if (drawingFormat.Alignment == StringAlignment.Far)
                {
                    labelRect.Left = absPosition.Right - labelSize.Width;
                }
                else if (drawingFormat.Alignment == StringAlignment.Near)
                {
                    labelRect.Left = absPosition.Left;
                }
                else if (drawingFormat.Alignment == StringAlignment.Center)
                {
                    labelRect.Left = absPosition.Left + absPosition.Width / 2F - labelSize.Width / 2F;
                }

                if (drawingFormat.LineAlignment == StringAlignment.Far)
                {
                    labelRect.Top = absPosition.Bottom - labelSize.Height;
                }
                else if (drawingFormat.LineAlignment == StringAlignment.Near)
                {
                    labelRect.Top = absPosition.Top;
                }
                else if (drawingFormat.LineAlignment == StringAlignment.Center)
                {
                    labelRect.Top = absPosition.Top + absPosition.Height / 2F - labelSize.Height / 2F;
                }

                //If the angle is not vertical or horizontal
                if (angle != 0 && angle != 90 && angle != -90)
                {
                    // Adjust label rectangle so it will not overlap the plotting area
                    offsetY = (float)Math.Sin((90 - angle) / 180F * Math.PI) * labelRect.Height / 2F;
                    offsetX = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * labelRect.Height / 2F;

                    if (axis.AxisPosition == AxisPosition.Left)
                    {
                        _myMatrix.Translate(-offsetX, 0);
                    }
                    else if (axis.AxisPosition == AxisPosition.Right)
                    {
                        _myMatrix.Translate(offsetX, 0);
                    }
                    else if (axis.AxisPosition == AxisPosition.Top)
                    {
                        _myMatrix.Translate(0, -offsetY);
                    }
                    else if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                        _myMatrix.Translate(0, offsetY);
                    }

                    // Adjust label rectangle so it will be inside boundary
                    if (boundaryRect != SKRect.Empty)
                    {
                        SKPath p = new();
                        p.AddRect(labelRect);
                        p.Transform(_myMatrix);
                        SKRegion region = new(p);

                        // Extend boundary rectangle to the chart picture border
                        if (axis.AxisPosition == AxisPosition.Left)
                        {
                            boundaryRect.Right += boundaryRect.Left;
                            boundaryRect.Left = 0;
                        }
                        else if (axis.AxisPosition == AxisPosition.Right)
                        {
                            boundaryRect.Right = _common.Width;
                        }
                        else if (axis.AxisPosition == AxisPosition.Top)
                        {
                            boundaryRect.Bottom += boundaryRect.Top;
                            boundaryRect.Top = 0;
                        }
                        else if (axis.AxisPosition == AxisPosition.Bottom)
                        {
                            boundaryRect.Bottom = _common.Height;
                        }

                        // Exclude boundary rectangle from the label rectangle
                        var p2 = new SKPath();
                        p2.AddRect(GetAbsoluteRectangle(boundaryRect));
                        region.Op(p2, SKRegionOperation.Difference);

                        // If any part of the label was outside bounding rectangle
                        if (!region.IsEmpty)
                        {
                            Transform = oldTransform;
                            SKRect truncateRect = region.Bounds;

                            float sizeChange = truncateRect.Width / (float)Math.Cos(Math.Abs(angle) / 180F * Math.PI);
                            if (axis.AxisPosition == AxisPosition.Left)
                            {
                                sizeChange -= labelRect.Height * (float)Math.Tan(Math.Abs(angle) / 180F * Math.PI);
                                absPosition.Top = labelRect.Top;
                                absPosition.Left = labelRect.Left + sizeChange;
                                absPosition.Right = absPosition.Left + labelRect.Width - sizeChange;
                                absPosition.Bottom = absPosition.Top + labelRect.Height;
                            }
                            else if (axis.AxisPosition == AxisPosition.Right)
                            {
                                sizeChange -= labelRect.Height * (float)Math.Tan(Math.Abs(angle) / 180F * Math.PI);
                                absPosition.Top = labelRect.Top;
                                absPosition.Left = labelRect.Left;
                                absPosition.Right = absPosition.Left + labelRect.Width - sizeChange;
                                absPosition.Bottom = absPosition.Top + labelRect.Height;
                            }
                            else if (axis.AxisPosition == AxisPosition.Top)
                            {
                                absPosition.Top = labelRect.Top;
                                absPosition.Left = labelRect.Left;
                                absPosition.Right = absPosition.Left + labelRect.Width - sizeChange;
                                absPosition.Bottom = absPosition.Top + labelRect.Height;
                                if (angle > 0)
                                {
                                    absPosition.Left += sizeChange;
                                }
                            }
                            else if (axis.AxisPosition == AxisPosition.Bottom)
                            {
                                absPosition.Top = labelRect.Top;
                                absPosition.Left = labelRect.Left;
                                absPosition.Right = absPosition.Left + labelRect.Width - sizeChange;
                                absPosition.Bottom = absPosition.Top + labelRect.Height;
                                if (angle < 0)
                                {
                                    absPosition.Left += sizeChange;
                                }
                            }
                        }
                    }

                    // Update transformation matrix
                    Transform = _myMatrix;
                }

                //********************************************************************
                //** Reserve space on the left for the label iamge
                //********************************************************************
                SKRect absPositionWithoutImage = new(absPosition.Left, absPosition.Top, absPosition.Right, absPosition.Bottom);

                SKImage labelImage = null;
                SKSize imageAbsSize = new();

                if (image.Length > 0)
                {
                    labelImage = axis.Common.ImageLoader.LoadImage(label.Image);

                    if (labelImage != null)
                    {
                        ImageLoader.GetAdjustedImageSize(labelImage, Graphics, ref imageAbsSize);

                        // Adjust label position using image size
                        absPositionWithoutImage.Right -= imageAbsSize.Width;
                        absPositionWithoutImage.Left += imageAbsSize.Width;
                    }

                    if (absPositionWithoutImage.Width < 1f)
                    {
                        absPositionWithoutImage.Right = absPositionWithoutImage.Left + 1f;
                    }
                }

                //********************************************************************
                //** Draw tick marks for labels in second row
                //********************************************************************
                if (labelRowIndex > 0 && labelMark != LabelMarkStyle.None)
                {
                    // Make sure that me know the exact size of the text
                    labelSize = this.MeasureString(
                        text.Replace("\\n", "\n"),
                        font,
                        absPositionWithoutImage.Size,
                        drawingFormat);

                    // Adjust for label image
                    SKSize labelSizeWithImage = new(labelSize.Width, labelSize.Height);
                    if (labelImage != null)
                    {
                        labelSizeWithImage.Width += imageAbsSize.Width;
                    }

                    // Draw mark
                    DrawSecondRowLabelMark(
                        axis,
                        markColor,
                        absPosition,
                        labelSizeWithImage,
                        labelMark,
                        truncatedLeft,
                        truncatedRight,
                        oldTransform);
                }

                //********************************************************************
                //** Make sure that one line label will not disapear with LineLimit
                //** flag on.
                //********************************************************************
                if ((drawingFormat.FormatFlags & StringFormatFlags.LineLimit) != 0)
                {
                    // Measure string height out of one character
                    drawingFormat.FormatFlags ^= StringFormatFlags.LineLimit;
                    SKSize size = this.MeasureString("I", font, absPosition.Size, drawingFormat);

                    // If height of one characte is more than rectangle heigjt - remove LineLimit flag
                    if (size.Height < absPosition.Height)
                    {
                        drawingFormat.FormatFlags |= StringFormatFlags.LineLimit;
                    }
                }
                else
                {
                    // Set NoClip flag
                    if ((drawingFormat.FormatFlags & StringFormatFlags.NoClip) != 0)
                    {
                        drawingFormat.FormatFlags ^= StringFormatFlags.NoClip;
                    }

                    // Measure string height out of one character without clipping
                    SKSize size = this.MeasureString("I", font, absPosition.Size, drawingFormat);

                    // Clear NoClip flag
                    drawingFormat.FormatFlags ^= StringFormatFlags.NoClip;

                    // If height of one characte is more than rectangle heigt - set NoClip flag
                    if (size.Height > absPosition.Height)
                    {
                        float delta = size.Height - absPosition.Height;
                        absPosition.Top -= delta / 2f;
                        absPosition.Bottom += delta;
                    }
                }

                //********************************************************************
                //** Draw a string
                //********************************************************************

                DrawString(text.Replace("\\n", "\n"), font, brush,
                absPositionWithoutImage,
                drawingFormat, TextOrientation.Auto);

                // Add separate hot region for the label
                if (common.ProcessModeRegions)
                {
                    using SKPath path = new();
                    path.AddRect(labelRect);
                    path.Transform(Transform);
                    string url = string.Empty;
                    string mapAreaAttributes = string.Empty;
                    string postbackValue = string.Empty;
                    common.HotRegionsList.AddHotRegion(
                        this,
                        path,
                        false,
                        label.ToolTip,
                        url,
                        mapAreaAttributes,
                        postbackValue,
                        label,
                        ChartElementType.AxisLabels);
                }

                //********************************************************************
                //** Draw an image
                //********************************************************************
                if (labelImage != null)
                {
                    // Make sure we no the text size
                    if (labelSize.IsEmpty)
                    {
                        labelSize = this.MeasureString(
                            text.Replace("\\n", "\n"),
                            font,
                            absPositionWithoutImage.Size,
                            drawingFormat);
                    }

                    // Calculate image rectangle
                    SKRect imageRect = new(
                        absPosition.Left + (absPosition.Width - imageAbsSize.Width - labelSize.Width) / 2,
                        absPosition.Top + (absPosition.Height - imageAbsSize.Height) / 2,
                        imageAbsSize.Width,
                        imageAbsSize.Height);

                    if (drawingFormat.LineAlignment == StringAlignment.Center)
                    {
                        imageRect.Top = absPosition.Top + (absPosition.Height - imageAbsSize.Height) / 2;
                    }
                    else if (drawingFormat.LineAlignment == StringAlignment.Far)
                    {
                        imageRect.Top = absPosition.Bottom - (labelSize.Height + imageAbsSize.Height) / 2;
                    }
                    else if (drawingFormat.LineAlignment == StringAlignment.Near)
                    {
                        imageRect.Top = absPosition.Top + (labelSize.Height - imageAbsSize.Height) / 2;
                    }

                    if (drawingFormat.Alignment == StringAlignment.Center)
                    {
                        imageRect.Left = absPosition.Left + (absPosition.Width - imageAbsSize.Width - labelSize.Width) / 2;
                    }
                    else if (drawingFormat.Alignment == StringAlignment.Far)
                    {
                        imageRect.Left = absPosition.Right - imageAbsSize.Width - labelSize.Width;
                    }
                    else if (drawingFormat.Alignment == StringAlignment.Near)
                    {
                        imageRect.Left = absPosition.Left;
                    }

                    // Draw image
                    DrawImage(
                        labelImage,
                        new SKRect(MathF.Round(imageRect.Left), MathF.Round(imageRect.Top), MathF.Round(imageRect.Right), MathF.Round(imageRect.Bottom)),
                        0, 0, labelImage.Width, labelImage.Height, null);

                    // Add separate hot region for the label image
                    if (common.ProcessModeRegions)
                    {
                        using SKPath path = new();
                        path.AddRect(imageRect);
                        path.Transform(Transform);
                        string imageUrl = string.Empty;
                        string imageMapAreaAttributes = string.Empty;
                        string postbackValue = string.Empty;
                        common.HotRegionsList.AddHotRegion(
                            this,
                            path,
                            false,
                            string.Empty,
                            imageUrl,
                            imageMapAreaAttributes,
                            postbackValue,
                            label,
                            ChartElementType.AxisLabelImage);
                    }
                }
            }

            // Set Old Angle
            if (oldTransform != SKMatrix.Empty)
            {
                Transform = oldTransform;
            }
        }

        /// <summary>
        /// Draw a string and fills it's background
        /// </summary>
        /// <param name="common">The Common elements object.</param>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="angle">Text angle.</param>
        /// <param name="backPosition">Text background position.</param>
        /// <param name="backColor">Back Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="series">Series</param>
        /// <param name="point">Point</param>
        /// <param name="pointIndex">Point index in series</param>
        internal void DrawPointLabelStringRel(
            CommonElements common,
            string text,
            SKFont font,
            SKPaint brush,
            SKRect position,
            StringFormat format,
            int angle,
            SKRect backPosition,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            Series series,
            DataPoint point,
            int pointIndex)
        {
            // Draw background
            DrawPointLabelBackground(
                common,
                angle,
                SKPoint.Empty,
                backPosition,
                backColor,
                borderColor,
                borderWidth,
                borderDashStyle,
                series,
                point,
                pointIndex);

            point._lastLabelText = text;
            // Draw text
            DrawStringRel(text, font, brush, position, format, angle);
        }

        /// <summary>
        /// Draw a string and fills it's background
        /// </summary>
        /// <param name="common">The Common elements object.</param>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="angle">Text angle.</param>
        /// <param name="backPosition">Text background position.</param>
        /// <param name="backColor">Back Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="series">Series</param>
        /// <param name="point">Point</param>
        /// <param name="pointIndex">Point index in series</param>
        internal void DrawPointLabelStringRel(
            CommonElements common,
            string text,
            SKFont font,
            SKPaint brush,
            SKPoint position,
            StringFormat format,
            int angle,
            SKRect backPosition,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            Series series,
            DataPoint point,
            int pointIndex)
        {
            // Draw background
            DrawPointLabelBackground(
                common,
                angle,
                position,
                backPosition,
                backColor,
                borderColor,
                borderWidth,
                borderDashStyle,
                series,
                point,
                pointIndex);

            point._lastLabelText = text;
            // Draw text
            DrawStringRel(text, font, brush, position, format, angle);
        }

        /// <summary>
        /// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to draw.</param>
        /// <param name="font">Font object that defines the text format of the string.</param>
        /// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
        /// <param name="rect">Position of the drawn text in pixels.</param>
        /// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        /// <param name="textOrientation">Text orientation.</param>
        internal void DrawString(
            string text,
            SKFont font,
            SKPaint paint,
            SKRect rect,
            StringFormat format,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or
            // correctly handle text wrapping.
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }
            RenderingObject.DrawString(text, font, paint, rect/*, format, TextOrientation.Auto1*/);
        }

        /// <summary>
        /// Draw a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="absPosition">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="angle">Text angle.</param>
        internal void DrawStringAbs(
            string text,
            SKFont font,
            SKPaint brush,
            SKPoint absPosition,
            StringFormat format,
            int angle
            )
        {
            // Create a matrix and rotate it.
            _myMatrix = SKMatrix.CreateRotationDegrees(angle, absPosition.X, absPosition.Y);

            // Set Angle
            Transform = _myMatrix;

            // Draw a string
            this.DrawString(text, font, brush, absPosition);
        }

        /// <summary>
        /// Draw a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="angle">Text angle.</param>
        /// <param name="textOrientation">Text orientation.</param>
        internal void DrawStringRel(
            string text,
            SKFont font,
            SKPaint brush,
            SKPoint position,
            StringFormat format,
            int angle,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or
            // correctly handle text wrapping.
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }

            DrawStringRel(text, font, brush, position, format, angle);
        }

        /// <summary>
        /// Draw a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="textOrientation">Text orientation.</param>
        internal void DrawStringRel(
            string text,
            SKFont font,
            SKPaint brush,
            SKRect position,
            StringFormat format,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or
            // correctly handle text wrapping.
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }

            DrawStringRel(text, font, brush, position, format);
        }

        /// <summary>
        /// Draw a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="angle">Text angle.</param>
        internal void DrawStringRel(
            string text,
            SKFont font,
            SKPaint brush,
            SKPoint position,
            StringFormat format,
            int angle
            )
        {
            DrawStringAbs(
                text,
                font,
                brush,
                GetAbsolutePoint(position),
                format,
                angle);
        }

        /// <summary>
        /// Draws the specified text string at the specified location
        /// with the specified Brush object and font. The formatting
        /// properties in the specified StringFormat object are applied
        /// to the text.
        /// </summary>
        /// <param name="text">A string object that specifies the text to draw.</param>
        /// <param name="font">A Font object that specifies the font face and size with which to draw the text.</param>
        /// <param name="brush">A Brush object that determines the color and/or texture of the drawn text.</param>
        /// <param name="layoutRectangle">A SKRect structure that specifies the location of the drawn text.</param>
        /// <param name="format">A StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        internal void DrawStringRel(string text, SKFont font, SKPaint brush, SKRect layoutRectangle, StringFormat format)
        {
            SKRect rect;

            // Check that rectangle is not empty
            if (layoutRectangle.Width == 0 || layoutRectangle.Height == 0)
            {
                return;
            }

            // Get absolute coordinates
            rect = GetAbsoluteRectangle(layoutRectangle);

            var sm = SmoothingMode;

            // Draw text with anti-aliasing

            if ((AntiAliasing & AntiAliasingStyles.Text) == AntiAliasingStyles.Text)
            {
                SmoothingMode = SmoothingMode.AntiAlias;
            }
            else
            {
                SmoothingMode = SmoothingMode.HighSpeed;
            }
            
            this.DrawString(text, font, brush, rect, format, TextOrientation.Auto);
            SmoothingMode = sm;
        }

        /// <summary>
        /// Draws the specified text string at the specified location
        /// with the specified angle and with the specified Brush object and font. The
        /// formatting properties in the specified StringFormat object are applied
        /// to the text.
        /// </summary>
        /// <param name="text">A string object that specifies the text to draw.</param>
        /// <param name="font">A Font object that specifies the font face and size with which to draw the text.</param>
        /// <param name="brush">A Brush object that determines the color and/or texture of the drawn text.</param>
        /// <param name="layoutRectangle">A SKRect structure that specifies the location of the drawn text.</param>
        /// <param name="format">A StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        /// <param name="angle">A angle of the text</param>
        internal void DrawStringRel(
            string text,
            SKFont font,
            SKPaint brush,
            SKRect layoutRectangle,
            StringFormat format,
            int angle
            )
        {
            SKRect rect;
            SKSize size;
            SKPoint rotationCenter = SKPoint.Empty;

            // Check that rectangle is not empty
            if (layoutRectangle.Width == 0 || layoutRectangle.Height == 0)
            {
                return;
            }

            // Get absolute coordinates
            rect = GetAbsoluteRectangle(layoutRectangle);

            size = this.MeasureString(text, font, rect.Size, format);

            // Find the center of rotation
            if (format.Alignment == StringAlignment.Near)
            { // Near
                rotationCenter.X = rect.Left + size.Width / 2;
                rotationCenter.Y = (rect.Bottom + rect.Top) / 2;
            }
            else if (format.Alignment == StringAlignment.Far)
            { // Far
                rotationCenter.X = rect.Right - size.Width / 2;
                rotationCenter.Y = (rect.Bottom + rect.Top) / 2;
            }
            else
            { // Center
                rotationCenter.X = (rect.Left + rect.Right) / 2;
                rotationCenter.Y = (rect.Bottom + rect.Top) / 2;
            }

            RenderingObject.Graphics.Save();
            RenderingObject.Graphics.RotateDegrees(angle);
            RenderingObject.Graphics.DrawText(text, rotationCenter.X, rotationCenter.Y, font, brush);
            RenderingObject.Graphics.Restore();
        }

        /// <summary>
        /// This method is used by the axis title hot region generation code.
        /// It transforms the centered rectangle the same way as the Axis title text.
        /// </summary>
        /// <param name="center">Title center</param>
        /// <param name="size">Title text size</param>
        /// <param name="angle">Title rotation angle</param>
        /// <returns></returns>
        internal SKPath GetTranformedTextRectPath(SKPoint center, SKSize size, int angle)
        {
            // Text hot area is 10px greater than the size of text
            size.Width += 10;
            size.Height += 10;

            // Get the absolute center and create the centered rectangle points
            SKPoint absCenter = GetAbsolutePoint(center);
            SKPoint[] points = new SKPoint[] {
                new SKPoint(absCenter.X - size.Width / 2f, absCenter.Y - size.Height / 2f),
                new SKPoint(absCenter.X + size.Width / 2f, absCenter.Y - size.Height / 2f),
                new SKPoint(absCenter.X + size.Width / 2f, absCenter.Y + size.Height / 2f),
                new SKPoint(absCenter.X - size.Width / 2f, absCenter.Y + size.Height / 2f)};

            //Prepare the same tranformation matrix as used for the axis title
            SKMatrix matrix = SKMatrix.CreateRotationDegrees(angle, absCenter.X, absCenter.Y);
            //Tranform the rectangle points
            matrix.TransformPoints(points);

            //Return the path consisting of the rect points
            SKPath path = new();
            path.AddLines(points);
            path.Close();
            return path;
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified
        /// Font object and formatted with the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object defines the text format of the string.</param>
        /// <param name="layoutArea">SKSize structure that specifies the maximum layout area for the text.</param>
        /// <param name="stringFormat">StringFormat object that represents formatting information, such as line spacing, for the string.</param>
        /// <param name="textOrientation">Text orientation.</param>
        /// <returns>This method returns a SKSize structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        internal SKSize MeasureString(
            string text,
            SKFont font,
            SKSize layoutArea,
            StringFormat stringFormat
            )
        {
            return MeasureString(text, font, layoutArea, stringFormat, TextOrientation.Auto);
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified
        /// Font object and formatted with the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object defines the text format of the string.</param>
        /// <param name="layoutArea">SKSize structure that specifies the maximum layout area for the text.</param>
        /// <param name="stringFormat">StringFormat object that represents formatting information, such as line spacing, for the string.</param>
        /// <param name="textOrientation">Text orientation.</param>
        /// <returns>This method returns a SKSize structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        internal static SKSize MeasureString(
        string text,
        SKFont font,
        SKSize layoutArea,
        StringFormat stringFormat,
        TextOrientation textOrientation
        )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or
            // correctly handle text wrapping.
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }
            var p = new SKPaint() { Typeface = font.Typeface, TextSize = font.Size };
            var width = Math.Clamp(p.MeasureText(text), 0, layoutArea.Width);
            var height = Math.Clamp(p.TextSize, 0, layoutArea.Height);
            return new SKSize(width, height);
        }
        /// <summary>
        /// Measures the specified text string when drawn with
        /// the specified Font object and formatted with the
        /// specified StringFormat object.
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The Font object used to determine the size of the text string. </param>
        /// <returns>A SKSize structure that represents the size of text as drawn with font.</returns>
        internal SKSize MeasureStringAbs(string text, SKFont font)
        {
            // Measure string
            SKSize size = MeasureString(text, font);
            return new SKSize((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
        }

        /// <summary>
        /// Measures the specified text string when drawn with
        /// the specified Font object and formatted with the
        /// specified StringFormat object.
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The Font object used to determine the size of the text string. </param>
        /// <param name="layoutArea">A SKSize structure that specifies the layout rectangle for the text. </param>
        /// <param name="stringFormat">A StringFormat object that represents formatting information, such as line spacing, for the text string. </param>
        /// <returns>A SKSize structure that represents the size of text as drawn with font.</returns>
        internal SKSize MeasureStringAbs(string text, SKFont font, SKSize layoutArea, StringFormat stringFormat)
        {
            SKSize size = this.MeasureString(text, font, layoutArea, stringFormat);
            return new SKSize((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
        }

        /// <summary>
        /// Measures the specified text string when drawn with
        /// the specified Font object and formatted with the
        /// specified StringFormat object.
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The Font object used to determine the size of the text string. </param>
        /// <returns>A SKSize structure that represents the size of text as drawn with font.</returns>
        internal SKSize MeasureStringRel(string text, SKFont font)
        {
            SKSize newSize;

            // Measure string
            newSize = MeasureString(text, font);

            // Convert to relative Coordinates
            return GetRelativeSize(newSize);
        }

        /// <summary>
        /// Measures the specified text string when drawn with
        /// the specified Font object and formatted with the
        /// specified StringFormat object.
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The Font object used to determine the size of the text string. </param>
        /// <param name="layoutArea">A SKSize structure that specifies the layout rectangle for the text. </param>
        /// <param name="stringFormat">A StringFormat object that represents formatting information, such as line spacing, for the text string. </param>
        /// <returns>A SKSize structure that represents the size of text as drawn with font.</returns>
        internal SKSize MeasureStringRel(string text, SKFont font, SKSize layoutArea, StringFormat stringFormat)
        {
            SKSize size, newSize;

            // Get absolute coordinates
            size = GetAbsoluteSize(layoutArea);

            newSize = this.MeasureString(text, font, size, stringFormat);

            // Convert to relative Coordinates
            return GetRelativeSize(newSize);
        }

        /// <summary>
        /// Draw a string and fills it's background
        /// </summary>
        /// <param name="common">The Common elements object.</param>
        /// <param name="angle">Text angle.</param>
        /// <param name="textPosition">Text position.</param>
        /// <param name="backPosition">Text background position.</param>
        /// <param name="backColor">Back Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="series">Series</param>
        /// <param name="point">Point</param>
        /// <param name="pointIndex">Point index in series</param>
        private void DrawPointLabelBackground(
            CommonElements common,
            int angle,
            SKPoint textPosition,
            SKRect backPosition,
            SKColor backColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            Series series,
            DataPoint point,
            int pointIndex)
        {
            // Draw background
            if (!backPosition.IsEmpty)
            {
                SKRect backPositionAbs = Round(GetAbsoluteRectangle(backPosition));

                // Get rotation point
                SKPoint rotationPoint;
                if (textPosition.IsEmpty)
                {
                    rotationPoint = new SKPoint(backPositionAbs.Left + backPositionAbs.Width / 2f, backPositionAbs.Top + backPositionAbs.Height / 2f);
                }
                else
                {
                    rotationPoint = GetAbsolutePoint(textPosition);
                }

                // Create a matrix and rotate it.
                _myMatrix = SKMatrix.CreateRotationDegrees(angle, rotationPoint.X, rotationPoint.Y);

                // Set transformatino
                Transform = _myMatrix;

                // Check for empty colors
                if (backColor != SKColor.Empty ||
                    borderColor != SKColor.Empty)
                {
                    // Fill box around the label
                    using (SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = backColor })
                    {
                        this.FillRectangle(brush, backPositionAbs);
                    }

                    // deliant: Fix VSTS #156433	(2)	Data Label Border in core always shows when the style is set to NotSet
                    // Draw box border
                    if (borderWidth > 0 &&
                        borderColor != SKColor.Empty && borderDashStyle != ChartDashStyle.NotSet)
                    {
                        AntiAliasingStyles saveAntiAliasing = AntiAliasing;
                        try
                        {
                            AntiAliasing = AntiAliasingStyles.None;
                            using SKPaint pen = new() { Color = borderColor, StrokeWidth = borderWidth, Style = SKPaintStyle.Stroke };
                            pen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);
                            this.DrawRectangle(
                                pen,
                                backPositionAbs.Left,
                                backPositionAbs.Top,
                                backPositionAbs.Width,
                                backPositionAbs.Height);
                        }
                        finally
                        {
                            AntiAliasing = saveAntiAliasing;
                        }
                    }
                }
                else
                {
                    // Draw invisible rectangle to handle tooltips
                    using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = SKColors.Transparent };
                    FillRectangle(brush, backPositionAbs);
                }

                // Add point label hot region
                if (common != null &&
                    common.ProcessModeRegions)
                {
                    // Insert area
                    if (angle == 0)
                    {
                        common.HotRegionsList.AddHotRegion(
                            backPosition,
                            point,
                            series.Name,
                            pointIndex);
                    }
                    else
                    {
                        // Convert rectangle to the graphics path and apply rotation transformation
                        using SKPath path = new();
                        path.AddRect(backPositionAbs);
                        path.Transform(_myMatrix);

                        // Add hot region
                        common.HotRegionsList.AddHotRegion(
                            path,
                            false,
                            this,
                            point,
                            series.Name,
                            pointIndex);
                    }

                    // Set new hot region element type
                    if (common.HotRegionsList.List != null && common.HotRegionsList.List.Count > 0)
                    {
                        ((HotRegion)common.HotRegionsList.List[^1]).Type =
                            ChartElementType.DataPointLabel;
                    }
                }
            }
        }
        /// <summary>
        /// Draw box marks for the labels in second row
        /// </summary>
        /// <param name="axis">Axis object.</param>
        /// <param name="markColor">Label mark color.</param>
        /// <param name="absPosition">Absolute position of the text.</param>
        /// <param name="truncatedLeft">Label is truncated on the left.</param>
        /// <param name="truncatedRight">Label is truncated on the right.</param>
        /// <param name="originalTransform">Original transformation matrix.</param>
        private void DrawSecondRowLabelBoxMark(
            Axis axis,
            SKColor markColor,
            SKRect absPosition,
            bool truncatedLeft,
            bool truncatedRight,
            SKMatrix originalTransform)
        {
            // Remeber current and then reset original matrix
            SKMatrix curentMatrix = Transform;
            if (originalTransform != SKMatrix.Empty)
            {
                Transform = originalTransform;
            }

            // Calculate center of the text rectangle
            SKPoint centerNotRound = new(absPosition.Left + absPosition.Width / 2F, absPosition.Top + absPosition.Height / 2F);

            // Rotate rectangle 90 degrees
            if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
            {
                SKRect newRect = SKRect.Empty;
                newRect.Left = centerNotRound.X - absPosition.Height / 2F;
                newRect.Top = centerNotRound.Y - absPosition.Width / 2F;
                newRect.Bottom = newRect.Top + absPosition.Width;
                newRect.Right = newRect.Left + absPosition.Height;
                absPosition = newRect;
            }

            // Get axis position
            float axisPosRelative = (float)axis.GetAxisPosition(true);
            SKPoint axisPositionAbs = new(axisPosRelative, axisPosRelative);
            axisPositionAbs = GetAbsolutePoint(axisPositionAbs);

            // Round position to achieve crisp lines with antialiasing
            var absPositionRounded = absPosition.Round();

            // Make sure the right and bottom position is not shifted during rounding
            absPositionRounded.Size = new((int)Math.Round(absPosition.Right) - absPositionRounded.Left, (int)Math.Round(absPosition.Bottom) - absPositionRounded.Top);

            // Create pen
            SKPaint markPen = new()
            {
                Color = (markColor == SKColor.Empty) ? axis.MajorTickMark.LineColor : markColor,
                StrokeWidth = axis.MajorTickMark.LineWidth,
                Style = SKPaintStyle.Stroke
            };

            // Set pen style
            markPen.PathEffect = ChartGraphics.GetPenStyle(axis.MajorTickMark.LineDashStyle, axis.MajorTickMark.LineWidth);

            // Draw top/bottom lines
            if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
            {
                this.DrawLine(markPen, absPositionRounded.Left, absPositionRounded.Top, absPositionRounded.Left, absPositionRounded.Bottom);
                this.DrawLine(markPen, absPositionRounded.Right, absPositionRounded.Top, absPositionRounded.Right, absPositionRounded.Bottom);
            }
            else
            {
                this.DrawLine(markPen, absPositionRounded.Left, absPositionRounded.Top, absPositionRounded.Right, absPositionRounded.Top);
                this.DrawLine(markPen, absPositionRounded.Left, absPositionRounded.Bottom, absPositionRounded.Right, absPositionRounded.Bottom);
            }

            // Draw left line
            if (!truncatedLeft)
            {
                if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
                {
                    this.DrawLine(
                        markPen,
                        (axis.AxisPosition == AxisPosition.Left) ? absPositionRounded.Left : absPositionRounded.Right,
                        absPositionRounded.Bottom,
                        axisPositionAbs.X,
                        absPositionRounded.Bottom);
                }
                else
                {
                    this.DrawLine(
                        markPen,
                        absPositionRounded.Left,
                        (axis.AxisPosition == AxisPosition.Top) ? absPositionRounded.Top : absPositionRounded.Bottom,
                        absPositionRounded.Left,
                        axisPositionAbs.Y);
                }
            }

            // Draw right line
            if (!truncatedRight)
            {
                if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
                {
                    this.DrawLine(
                        markPen,
                        (axis.AxisPosition == AxisPosition.Left) ? absPositionRounded.Left : absPositionRounded.Right,
                        absPositionRounded.Top,
                        axisPositionAbs.X,
                        absPositionRounded.Top);
                }
                else
                {
                    this.DrawLine(
                        markPen,
                        absPositionRounded.Right,
                        (axis.AxisPosition == AxisPosition.Top) ? absPositionRounded.Top : absPositionRounded.Bottom,
                        absPositionRounded.Right,
                        axisPositionAbs.Y);
                }
            }

            // Dispose Pen
            if (markPen != null)
            {
                markPen.Dispose();
            }

            // Restore currentmatrix
            if (originalTransform != SKMatrix.Empty)
            {
                Transform = curentMatrix;
            }
        }

        /// <summary>
        /// Draw marks for the labels in second row
        /// </summary>
        /// <param name="axis">Axis object.</param>
        /// <param name="markColor">Label mark color.</param>
        /// <param name="absPosition">Absolute position of the text.</param>
        /// <param name="labelSize">Exact mesured size of the text.</param>
        /// <param name="labelMark">Label mark style to draw.</param>
        /// <param name="truncatedLeft">Label is truncated on the left.</param>
        /// <param name="truncatedRight">Label is truncated on the right.</param>
        /// <param name="oldTransform">Original transformation matrix.</param>
        private void DrawSecondRowLabelMark(
            Axis axis,
            SKColor markColor,
            SKRect absPosition,
            SKSize labelSize,
            LabelMarkStyle labelMark,
            bool truncatedLeft,
            bool truncatedRight,
            SKMatrix oldTransform)
        {
            // Do not draw marking line if width is 0 and style or color are not set
            if (axis.MajorTickMark.LineWidth == 0 ||
                axis.MajorTickMark.LineDashStyle == ChartDashStyle.NotSet ||
                axis.MajorTickMark.LineColor == SKColor.Empty)
            {
                return;
            }

            // Remember SmoothingMode and turn off anti aliasing for
            // vertical or horizontal lines of the label markers.
            SmoothingMode oldSmoothingMode = SmoothingMode;
            SmoothingMode = SmoothingMode.None;

            // Draw box marker
            if (labelMark == LabelMarkStyle.Box)
            {
                DrawSecondRowLabelBoxMark(
                    axis,
                    markColor,
                    absPosition,
                    truncatedLeft,
                    truncatedRight,
                    oldTransform);
            }
            else

            {
                // Calculate center of the text rectangle
                SKPoint center = new(
                    MathF.Round(absPosition.Left + absPosition.Width / 2F),
                    MathF.Round(absPosition.Top + absPosition.Height / 2F));

                // Round position to achieve crisp lines with antialiasing
                var absPositionRounded = absPosition.Round();

                // Make sure the right and bottom position is not shifted during rounding
                absPositionRounded.Size = new((int)Math.Round(absPosition.Right) - absPositionRounded.Left,
                    (int)Math.Round(absPosition.Bottom) - absPositionRounded.Top);

                // Arrays of points for the left and right marking lines
                SKPoint[] leftLine = new SKPoint[3];
                SKPoint[] rightLine = new SKPoint[3];

                // Calculate marking lines coordinates
                leftLine[0].X = absPositionRounded.Left;
                leftLine[0].Y = absPositionRounded.Bottom;
                leftLine[1].X = absPositionRounded.Left;
                leftLine[1].Y = center.Y;
                leftLine[2].X = (float)Math.Round((double)center.X - labelSize.Width / 2F - 1F);
                leftLine[2].Y = center.Y;

                rightLine[0].X = absPositionRounded.Right;
                rightLine[0].Y = absPositionRounded.Bottom;
                rightLine[1].X = absPositionRounded.Right;
                rightLine[1].Y = center.Y;
                rightLine[2].X = (float)Math.Round((double)center.X + labelSize.Width / 2F - 1F);
                rightLine[2].Y = center.Y;

                if (axis.AxisPosition == AxisPosition.Bottom)
                {
                    leftLine[0].Y = absPositionRounded.Top;
                    rightLine[0].Y = absPositionRounded.Top;
                }

                // Remove third point to draw only side marks
                if (labelMark == LabelMarkStyle.SideMark)
                {
                    leftLine[2] = leftLine[1];
                    rightLine[2] = rightLine[1];
                }

                if (truncatedLeft)
                {
                    leftLine[0] = leftLine[1];
                }
                if (truncatedRight)
                {
                    rightLine[0] = rightLine[1];
                }

                // Create pen
                SKPaint markPen = new()
                {
                    Color = (markColor == SKColor.Empty) ? axis.MajorTickMark.LineColor : markColor,
                    StrokeWidth = axis.MajorTickMark.LineWidth,
                    Style = SKPaintStyle.Stroke
                };

                // Set pen style
                markPen.PathEffect = GetPenStyle(axis.MajorTickMark.LineDashStyle, axis.MajorTickMark.LineWidth);

                // Draw marking lines
                this.DrawLines(markPen, leftLine);
                this.DrawLines(markPen, rightLine);

                // Dispose Pen
                if (markPen != null)
                {
                    markPen.Dispose();
                }
            }

            // Restore previous SmoothingMode
            SmoothingMode = oldSmoothingMode;
        }
        #endregion String Methods

        #region Rectangle Methods

        /// <summary>
        /// This method creates gradient color with brightness
        /// </summary>
        /// <param name="beginColor">Start color for gradient.</param>
        /// <param name="position">Position used between Start and end color.</param>
        /// <returns>Calculated Gradient color from gradient position</returns>
        internal static SKColor GetBrightGradientColor(SKColor beginColor, double position)
        {
            double brightness = 0.5;
            if (position < brightness)
            {
                return GetGradientColor(new SKColor(255, 255, 255, beginColor.Alpha), beginColor, 1 - brightness + position);
            }
            else if (-brightness + position < 1)
            {
                return GetGradientColor(beginColor, SKColors.Black, -brightness + position);
            }
            else
            {
                return new(0, 0, 0, beginColor.Alpha);
            }
        }

        /// <summary>
        /// Creates brush with specified properties.
        /// </summary>
        /// <param name="rect">Gradient rectangle</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <returns>New brush object.</returns>
        internal SKPaint CreateBrush(
            SKRect rect,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor
            )
        {
            SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = backColor };

            if (backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
            {
                brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor);
            }
            else if (backHatchStyle != ChartHatchStyle.None)
            {
                brush = GetHatchBrush(backHatchStyle, backColor, backSecondaryColor);
            }
            else if (backGradientStyle != GradientStyle.None)
            {
                // If a gradient type  is set create a brush with gradient
                brush = GetGradientBrush(rect, backColor, backSecondaryColor, backGradientStyle);
            }

            return brush;
        }

        /// <summary>
        /// Fills and/or draws border as circle or polygon.
        /// </summary>
        /// <param name="pen">Border pen.</param>
        /// <param name="brush">Border brush.</param>
        /// <param name="position">Circle position.</param>
        /// <param name="polygonSectorsNumber">Number of sectors for the polygon.</param>
        /// <param name="circle3D">Indicates that circle should be 3D..</param>
        internal void DrawCircleAbs(SKPaint pen, SKPaint brush, SKRect position, int polygonSectorsNumber, bool circle3D)
        {
            bool fill3DCircle = (circle3D && brush != null);

            // Draw 2D circle
            if (polygonSectorsNumber <= 2 && !fill3DCircle)
            {
                if (brush != null)
                {
                    FillEllipse(brush, position);
                }
                if (pen != null)
                {
                    DrawEllipse(pen, position);
                }
            }

            // Draw circle as polygon with specified number of sectors
            else
            {
                SKPoint firstPoint = new(position.Left + position.Width / 2f, position.Top);
                SKPoint centerPoint = new(position.Left + position.Width / 2f, position.Left + position.Height / 2f);
                float sectorSize = 0f;
                SKPoint prevPoint = SKPoint.Empty;
                float curentSector = 0f;

                using SKPath path = new();
                // Remember current smoothing mode
                SmoothingMode oldMode = SmoothingMode;
                if (fill3DCircle)
                {
                    SmoothingMode = SmoothingMode.None;
                }

                // Get sector size
                if (polygonSectorsNumber <= 2)
                {
                    // Circle sector size
                    sectorSize = 1f;
                }
                else
                {
                    // Polygon sector size
                    sectorSize = 360f / ((float)polygonSectorsNumber);
                }

                // Loop throug all sectors
                for (curentSector = 0f; curentSector < 360f; curentSector += sectorSize)
                {
                    // Create matrix
                    SKMatrix matrix = SkiaSharpExtensions.CreateRotationDegrees(curentSector, centerPoint);

                    // Get point and rotate it
                    SKPoint[] points = new SKPoint[] { firstPoint };
                    matrix.TransformPoints(points);

                    // Add point into the path
                    if (!prevPoint.IsEmpty)
                    {
                        path.AddLine(prevPoint, points[0]);

                        // Fill each segment separatly for the 3D look
                        if (fill3DCircle)
                        {
                            path.AddLine(points[0], centerPoint);
                            path.AddLine(centerPoint, prevPoint);
                            using (SKPaint sectorBrush = GetSector3DBrush(brush, curentSector, sectorSize))
                            {
                                this.FillPath(sectorBrush, path);
                            }
                            path.Reset();
                        }
                    }

                    // Remember last point
                    prevPoint = points[0];
                }

                path.Close();

                // Fill last segment for the 3D look
                if (!prevPoint.IsEmpty && fill3DCircle)
                {
                    path.AddLine(prevPoint, firstPoint);
                    path.AddLine(firstPoint, centerPoint);
                    path.AddLine(centerPoint, prevPoint);
                    using (var sectorBrush = GetSector3DBrush(brush, curentSector, sectorSize))
                    {
                        this.FillPath(sectorBrush, path);
                    }
                    path.Reset();
                }

                // Restore old mode
                if (fill3DCircle)
                {
                    SmoothingMode = oldMode;
                }

                if (brush != null && !circle3D)
                {
                    FillPath(brush, path);
                }
                if (pen != null)
                {
                    DrawPath(pen, path);
                }
            }
        }

        /// <summary>
        /// Fills graphics path with shadow using absolute coordinates.
        /// </summary>
        /// <param name="path">Graphics path to fill.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch Style</param>
        /// <param name="backImage">Image URL</param>
        /// <param name="backImageWrapMode">Image Mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
        /// <param name="backGradientStyle">Gradient AxisName</param>
        /// <param name="backSecondaryColor">End Gradient color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="penAlignment">Border is outside or inside rectangle</param>
        /// <param name="shadowOffset">Shadow offset.</param>
        /// <param name="shadowColor">Shadow color.</param>
        internal void DrawPathAbs(
            SKPath path,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            PenAlignment penAlignment,
            int shadowOffset,
            SKColor shadowColor)
        {
            // Draw patj shadow
            if (shadowOffset != 0 && shadowColor != SKColors.Transparent)
            {
                // Save graphics state and apply translate transformation
                TranslateTransform(shadowOffset, shadowOffset);

                if (backColor == SKColors.Transparent &&
                    backSecondaryColor == SKColor.Empty)
                {
                    this.DrawPathAbs(
                        path,
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        String.Empty,
                        ChartImageWrapMode.Scaled,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        shadowColor,
                        borderWidth,
                        borderDashStyle,
                        PenAlignment.Center);
                }
                else
                {
                    this.DrawPathAbs(
                        path,
                        shadowColor,
                        ChartHatchStyle.None,
                        String.Empty,
                        ChartImageWrapMode.Scaled,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        SKColors.Transparent,
                        0,
                        ChartDashStyle.NotSet,
                        PenAlignment.Center);
                }
            }

            // Draw path
            DrawPathAbs(
                path,
                backColor,
                backHatchStyle,
                backImage,
                backImageWrapMode,
                backImageTransparentColor,
                backImageAlign,
                backGradientStyle,
                backSecondaryColor,
                borderColor,
                borderWidth,
                borderDashStyle,
                penAlignment);
        }

        /// <summary>
        /// Fills graphics path using absolute coordinates.
        /// </summary>
        /// <param name="path">Graphics path to fill.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch Style</param>
        /// <param name="backImage">Image URL</param>
        /// <param name="backImageWrapMode">Image Mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
        /// <param name="backGradientStyle">Gradient AxisName</param>
        /// <param name="backSecondaryColor">End Gradient color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="penAlignment">Border is outside or inside rectangle</param>
        internal void DrawPathAbs(SKPath path,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            PenAlignment penAlignment)
        {
            SKPaint backBrush = null;

            // Color is empty
            if (backColor == SKColor.Empty)
                backColor = SKColors.White;

            if (backSecondaryColor == SKColor.Empty)
                backSecondaryColor = SKColors.White;

            if (borderColor == SKColor.Empty)
            {
                borderColor = SKColors.White;
                borderWidth = 0;
            }

            // Set pen properties
            _pen.Color = borderColor;
            _pen.StrokeWidth = borderWidth;
            // _pen.Alignment = penAlignment;
            _pen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);

            SKPaint brush;
            if (backGradientStyle == GradientStyle.None)
            {
                // Set solid brush color.
                _solidBrush.Color = backColor;
                brush = _solidBrush;
            }
            else
            {
                // If a gradient type  is set create a brush with gradient
                SKRect pathRect = path.GetBounds();
                pathRect.Inflate(new SKSize(2, 2));
                brush = GetGradientBrush(
                    pathRect,
                    backColor,
                    backSecondaryColor,
                    backGradientStyle);
            }

            if (backHatchStyle != ChartHatchStyle.None)
            {
                brush = GetHatchBrush(backHatchStyle, backColor, backSecondaryColor);
            }

            if (backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
            {
                backBrush = brush;
                brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor);
            }

            // For inset alignment resize fill rectangle
            SKRect fillRect = path.GetBounds();

            // Draw rectangle image
            if (backImage.Length > 0 && (backImageWrapMode == ChartImageWrapMode.Unscaled || backImageWrapMode == ChartImageWrapMode.Scaled))
            {
                // Load image
                SKImage image = _common.ImageLoader.LoadImage(backImage);

                // Prepare image properties (transparent color)
                if (backImageTransparentColor != SKColor.Empty)
                {
                    using var bmp = SKBitmap.FromImage(image);
                    // loop trough every pixel and set alpha if equal to transparent color
                    int width = bmp.Width;
                    int height = bmp.Height;
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            var color = bmp.GetPixel(col, row);

                            if (color.Red == backImageTransparentColor.Red && color.Green == backImageTransparentColor.Green && color.Blue == backImageTransparentColor.Blue)
                            {
                                bmp.SetPixel(col, row, color.WithAlpha(0x00));
                            }
                        }
                    }
                    using var bmp2 = new SKBitmap(bmp.Width, bmp.Height, bmp.ColorType, SKAlphaType.Premul);
                    bmp.CopyTo(bmp2);
                    image = SKImage.FromBitmap(bmp2);
                }

                // Draw scaled image
                SKRect imageRect = new();
                imageRect.Left = fillRect.Left;
                imageRect.Top = fillRect.Top;
                imageRect.Right = fillRect.Right;
                imageRect.Bottom = fillRect.Bottom;

                // Draw unscaled image using align property
                if (backImageWrapMode == ChartImageWrapMode.Unscaled)
                {
                    SKSize imageSize = new();

                    ImageLoader.GetAdjustedImageSize(image, Graphics, ref imageSize);

                    // Calculate image position
                    imageRect.Size = new(imageSize.Width, imageSize.Height);

                    // Adjust position with alignment property
                    if (imageRect.Width < fillRect.Width)
                    {
                        if (backImageAlign == ChartImageAlignmentStyle.BottomRight ||
                            backImageAlign == ChartImageAlignmentStyle.Right ||
                            backImageAlign == ChartImageAlignmentStyle.TopRight)
                        {
                            imageRect.Left = fillRect.Right - imageRect.Width;
                        }
                        else if (backImageAlign == ChartImageAlignmentStyle.Bottom ||
                            backImageAlign == ChartImageAlignmentStyle.Center ||
                            backImageAlign == ChartImageAlignmentStyle.Top)
                        {
                            imageRect.Left = fillRect.Left + (fillRect.Width - imageRect.Width) / 2;
                        }
                    }
                    if (imageRect.Height < fillRect.Height)
                    {
                        if (backImageAlign == ChartImageAlignmentStyle.BottomRight ||
                            backImageAlign == ChartImageAlignmentStyle.Bottom ||
                            backImageAlign == ChartImageAlignmentStyle.BottomLeft)
                        {
                            imageRect.Top = fillRect.Bottom - imageRect.Height;
                        }
                        else if (backImageAlign == ChartImageAlignmentStyle.Left ||
                            backImageAlign == ChartImageAlignmentStyle.Center ||
                            backImageAlign == ChartImageAlignmentStyle.Right)
                        {
                            imageRect.Top = fillRect.Top + (fillRect.Height - imageRect.Height) / 2;
                        }
                    }
                }

                // Fill background with brush
                this.FillPath(brush, path);

                // Draw image
                SKRegion oldClipRegion = Clip;
                Clip = new SKRegion(path);
                this.DrawImage(image,
                    new SKRect((int)Math.Round(imageRect.Left), (int)Math.Round(imageRect.Top), (int)Math.Round(imageRect.Width), (int)Math.Round(imageRect.Height)),
                    0, 0, image.Width, image.Height, null);
                Clip = oldClipRegion;
            }

            // Draw rectangle
            else
            {
                if (backBrush != null && backImageTransparentColor != SKColor.Empty)
                {
                    // Fill background with brush
                    this.FillPath(backBrush, path);
                }
                this.FillPath(brush, path);
            }

            // Draw border
            if (borderColor != SKColor.Empty && borderWidth > 0 && borderDashStyle != ChartDashStyle.NotSet)
            {
                DrawPath(_pen, path);
            }
        }

        /// <summary>
        /// Draws different shadows to create bar styles.
        /// </summary>
        /// <param name="barDrawingStyle">Bar drawing style.</param>
        /// <param name="isVertical">True if a vertical bar.</param>
        /// <param name="rect">Rectangle position.</param>
        internal void DrawRectangleBarStyle(BarDrawingStyle barDrawingStyle, bool isVertical, SKRect rect)
        {
            // Check if non-default bar drawing style is specified
            if (barDrawingStyle != BarDrawingStyle.Default && rect.Width > 0 && rect.Height > 0)
            {
                // Draw gradient(s)
                if (barDrawingStyle == BarDrawingStyle.Cylinder)
                {
                    // Calculate gradient position
                    SKRect gradientRect = rect;
                    if (isVertical)
                    {
                        gradientRect.Size = new(gradientRect.Width * 0.3f, gradientRect.Height);
                    }
                    else
                    {
                        gradientRect.Size = new(gradientRect.Width, gradientRect.Height * 0.3f);
                    }
                    if (gradientRect.Width > 0 && gradientRect.Height > 0)
                    {
                        this.FillRectangleAbs(
                            gradientRect,
                            SKColors.Transparent,
                            ChartHatchStyle.None,
                            string.Empty,
                            ChartImageWrapMode.Scaled,
                            SKColor.Empty,
                            ChartImageAlignmentStyle.Center,
                            (isVertical) ? GradientStyle.LeftRight : GradientStyle.TopBottom,
                            Color.FromArgb(120, SKColors.White),
                            SKColor.Empty,
                            0,
                            ChartDashStyle.NotSet,
                            PenAlignment.Inset);

                        if (isVertical)
                        {
                            gradientRect.Left += gradientRect.Width + 1f;
                            gradientRect.Right = rect.Right;
                        }
                        else
                        {
                            gradientRect.Top += gradientRect.Height + 1f;
                            gradientRect.Bottom = rect.Bottom - gradientRect.Top;
                        }

                        this.FillRectangleAbs(
                            gradientRect,
                            Color.FromArgb(120, SKColors.White),
                            ChartHatchStyle.None,
                            string.Empty,
                            ChartImageWrapMode.Scaled,
                            SKColor.Empty,
                            ChartImageAlignmentStyle.Center,
                            (isVertical) ? GradientStyle.LeftRight : GradientStyle.TopBottom,
                            Color.FromArgb(150, SKColors.Black),
                            SKColor.Empty,
                            0,
                            ChartDashStyle.NotSet,
                            PenAlignment.Inset);
                    }
                }
                else if (barDrawingStyle == BarDrawingStyle.Emboss)
                {
                    // Calculate width of shadows used to create the effect
                    float shadowSize = 3f;
                    if (rect.Width < 6f || rect.Height < 6f)
                    {
                        shadowSize = 1f;
                    }
                    else if (rect.Width < 15f || rect.Height < 15f)
                    {
                        shadowSize = 2f;
                    }

                    // Create and draw left/top path
                    using (SKPath path = new())
                    {
                        // Add shadow polygon to the path
                        SKPoint[] points = new SKPoint[] {
                                                               new SKPoint(rect.Left, rect.Bottom),
                                                               new SKPoint(rect.Left, rect.Top),
                                                               new SKPoint(rect.Right, rect.Top),
                                                               new SKPoint(rect.Right - shadowSize, rect.Top + shadowSize),
                                                               new SKPoint(rect.Left + shadowSize, rect.Top + shadowSize),
                                                               new SKPoint(rect.Left + shadowSize, rect.Bottom - shadowSize) };
                        path.AddPoly(points);

                        // Create brush
                        using SKPaint leftTopBrush = new() { Color = Color.FromArgb(100, SKColors.White), Style = SKPaintStyle.Fill };
                        // Fill shadow path on the left-bottom side of the bar
                        FillPath(leftTopBrush, path);
                    }

                    // Create and draw top/right path
                    using (SKPath path = new())
                    {
                        // Add shadow polygon to the path
                        SKPoint[] points = new SKPoint[] {
                                                               new SKPoint(rect.Right, rect.Top),
                                                               new SKPoint(rect.Right, rect.Bottom),
                                                               new SKPoint(rect.Left, rect.Bottom),
                                                               new SKPoint(rect.Left + shadowSize, rect.Bottom - shadowSize),
                                                               new SKPoint(rect.Right - shadowSize, rect.Bottom - shadowSize),
                                                               new SKPoint(rect.Right - shadowSize, rect.Top + shadowSize) };
                        path.AddPoly(points);

                        // Create brush
                        using SKPaint bottomRightBrush = new() { Color = Color.FromArgb(80, SKColors.Black), Style = SKPaintStyle.Fill };
                        // Fill shadow path on the left-bottom side of the bar
                        this.FillPath(bottomRightBrush, path);
                    }
                }
                else if (barDrawingStyle == BarDrawingStyle.LightToDark)
                {
                    // Calculate width of shadows used to create the effect
                    float shadowSize = 4f;
                    if (rect.Width < 6f || rect.Height < 6f)
                    {
                        shadowSize = 2f;
                    }
                    else if (rect.Width < 15f || rect.Height < 15f)
                    {
                        shadowSize = 3f;
                    }

                    // Calculate gradient position
                    SKRect gradientRect = rect;
                    gradientRect.Inflate(-shadowSize, -shadowSize);
                    if (isVertical)
                    {
                        gradientRect.Bottom = gradientRect.Top + (float)Math.Floor(gradientRect.Height / 3f);
                    }
                    else
                    {
                        gradientRect.Left = gradientRect.Right - (float)Math.Floor(gradientRect.Width / 3f);
                        gradientRect.Right = gradientRect.Left + (float)Math.Floor(gradientRect.Width / 3f);
                    }
                    if (gradientRect.Width > 0 && gradientRect.Height > 0)
                    {
                        this.FillRectangleAbs(
                            gradientRect,
                            (isVertical) ? Color.FromArgb(120, SKColors.White) : SKColors.Transparent,
                            ChartHatchStyle.None,
                            string.Empty,
                            ChartImageWrapMode.Scaled,
                            SKColor.Empty,
                            ChartImageAlignmentStyle.Center,
                            (isVertical) ? GradientStyle.TopBottom : GradientStyle.LeftRight,
                            (isVertical) ? SKColors.Transparent : Color.FromArgb(120, SKColors.White),
                            SKColor.Empty,
                            0,
                            ChartDashStyle.NotSet,
                            PenAlignment.Inset);

                        gradientRect = rect;
                        gradientRect.Inflate(-shadowSize, -shadowSize);
                        if (isVertical)
                        {
                            gradientRect.Top = gradientRect.Bottom - (float)Math.Floor(gradientRect.Height / 3f);
                            gradientRect.Bottom = gradientRect.Top + (float)Math.Floor(gradientRect.Height / 3f);
                        }
                        else
                        {
                            gradientRect.Right = gradientRect.Left + (float)Math.Floor(gradientRect.Width / 3f);
                        }

                        FillRectangleAbs(
                            gradientRect,
                            (!isVertical) ? Color.FromArgb(80, SKColors.Black) : SKColors.Transparent,
                            ChartHatchStyle.None,
                            string.Empty,
                            ChartImageWrapMode.Scaled,
                            SKColor.Empty,
                            ChartImageAlignmentStyle.Center,
                            (isVertical) ? GradientStyle.TopBottom : GradientStyle.LeftRight,
                            (!isVertical) ? SKColors.Transparent : Color.FromArgb(80, SKColors.Black),
                            SKColor.Empty,
                            0,
                            ChartDashStyle.NotSet,
                            PenAlignment.Inset);
                    }
                }
                else if (barDrawingStyle == BarDrawingStyle.Wedge)
                {
                    // Calculate wedge size to fit the rectangle
                    float size = (isVertical) ? rect.Width / 2f : rect.Height / 2f;
                    if (isVertical && 2f * size > rect.Height)
                    {
                        size = rect.Height / 2f;
                    }
                    if (!isVertical && 2f * size > rect.Width)
                    {
                        size = rect.Width / 2f;
                    }

                    // Draw left/bottom shadow
                    SKRect gradientRect = rect;
                    using (SKPath path = new())
                    {
                        if (isVertical)
                        {
                            path.AddLine(gradientRect.Left + gradientRect.Width / 2f, gradientRect.Top + size, gradientRect.Left + gradientRect.Width / 2f, gradientRect.Bottom - size);
                            path.AddLine(gradientRect.Left + gradientRect.Width / 2f, gradientRect.Bottom - size, gradientRect.Right, gradientRect.Bottom);
                            path.AddLine(gradientRect.Right, gradientRect.Bottom, gradientRect.Right, gradientRect.Top);
                        }
                        else
                        {
                            path.AddLine(gradientRect.Left + size, gradientRect.Top + gradientRect.Height / 2f, gradientRect.Right - size, gradientRect.Top + gradientRect.Height / 2f);
                            path.AddLine(gradientRect.Right - size, gradientRect.Top + gradientRect.Height / 2f, gradientRect.Right, gradientRect.Bottom);
                            path.AddLine(gradientRect.Right, gradientRect.Bottom, gradientRect.Left, gradientRect.Bottom);
                        }
                        path.Close();

                        // Create brush and fill path
                        using SKPaint brush = new() { Color = Color.FromArgb(90, SKColors.Black), Style = SKPaintStyle.Fill };
                        FillPath(brush, path);
                    }

                    // Draw top/right triangle
                    using (SKPath path = new())
                    {
                        if (isVertical)
                        {
                            path.AddLine(gradientRect.Left, gradientRect.Top, gradientRect.Left + gradientRect.Width / 2f, gradientRect.Top + size);
                            path.AddLine(gradientRect.Left + gradientRect.Width / 2f, gradientRect.Top + size, gradientRect.Right, gradientRect.Top);
                        }
                        else
                        {
                            path.AddLine(gradientRect.Right, gradientRect.Top, gradientRect.Right - size, gradientRect.Top + gradientRect.Height / 2f);
                            path.AddLine(gradientRect.Right - size, gradientRect.Top + gradientRect.Height / 2f, gradientRect.Right, gradientRect.Bottom);
                        }

                        // Create brush and fill path
                        using SKPaint brush = new() { Color = Color.FromArgb(50, SKColors.Black), Style = SKPaintStyle.Fill };
                        // Fill shadow path on the left-bottom side of the bar
                        this.FillPath(brush, path);

                        // Draw Lines
                        using (SKPaint penDark = new() { Color = Color.FromArgb(20, SKColors.Black), StrokeWidth = 1, Style = SKPaintStyle.Stroke })
                        {
                            this.DrawPath(penDark, path);
                            if (isVertical)
                            {
                                this.DrawLine(
                                    penDark,
                                    rect.Left + rect.Width / 2f,
                                    rect.Top + size,
                                    rect.Left + rect.Width / 2f,
                                    rect.Bottom - size);
                            }
                            else
                            {
                                this.DrawLine(
                                    penDark,
                                    rect.Left + size,
                                    rect.Top + rect.Height / 2f,
                                    rect.Left + size,
                                    rect.Bottom - rect.Height / 2f);
                            }
                        }

                        // Draw Lines
                        using SKPaint pen = new() { Color = Color.FromArgb(40, SKColors.White), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                        this.DrawPath(pen, path);
                        if (isVertical)
                        {
                            this.DrawLine(
                                pen,
                                rect.Left + rect.Width / 2f,
                                rect.Top + size,
                                rect.Left + rect.Width / 2f,
                                rect.Bottom - size);
                        }
                        else
                        {
                            this.DrawLine(
                                pen,
                                rect.Left + size,
                                rect.Top + rect.Height / 2f,
                                rect.Left + size,
                                rect.Bottom - rect.Height / 2f);
                        }
                    }

                    // Draw bottom/left triangle
                    using (SKPath path = new())
                    {
                        if (isVertical)
                        {
                            path.AddLine(gradientRect.Left, gradientRect.Bottom, gradientRect.Left + gradientRect.Width / 2f, gradientRect.Bottom - size);
                            path.AddLine(gradientRect.Left + gradientRect.Width / 2f, gradientRect.Bottom - size, gradientRect.Right, gradientRect.Bottom);
                        }
                        else
                        {
                            path.AddLine(gradientRect.Left, gradientRect.Top, gradientRect.Left + size, gradientRect.Top + gradientRect.Height / 2f);
                            path.AddLine(gradientRect.Left + size, gradientRect.Top + gradientRect.Height / 2f, gradientRect.Left, gradientRect.Bottom);
                        }

                        // Create brush
                        using SKPaint brush = new() { Color = Color.FromArgb(50, SKColors.Black), Style = SKPaintStyle.Fill };
                        // Fill shadow path on the left-bottom side of the bar
                        FillPath(brush, path);

                        // Draw edges
                        using SKPaint penDark = new() { Color = Color.FromArgb(20, SKColors.Black), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                        DrawPath(penDark, path);
                        using SKPaint pen = new() { Color = Color.FromArgb(40, SKColors.White), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
                        DrawPath(pen, path);
                    }
                }
            }
        }

        /// <summary>
        /// Draw Rectangle using absolute coordinates.
        /// </summary>
        /// <param name="rect">Size of rectangle</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch Style</param>
        /// <param name="backImage">Image URL</param>
        /// <param name="backImageWrapMode">Image Mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
        /// <param name="backGradientStyle">Gradient AxisName</param>
        /// <param name="backSecondaryColor">End Gradient color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="penAlignment">Border is outside or inside rectangle</param>
        internal void FillRectangleAbs(SKRect rect,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            PenAlignment penAlignment)
        {
            SKPaint brush = null;
            SKPaint backBrush = null;

            // Turn off Antialias
            SmoothingMode oldMode = SmoothingMode;
            SmoothingMode = SmoothingMode.None;

            // Color is empty
            if (backColor == SKColor.Empty)
                backColor = SKColors.White;

            if (backSecondaryColor == SKColor.Empty)
                backSecondaryColor = SKColors.White;

            if (borderColor == SKColor.Empty)
            {
                borderColor = SKColors.White;
                borderWidth = 0;
            }

            // Set a border line color
            _pen.Color = borderColor;

            // Set a border line width
            _pen.StrokeWidth = borderWidth;

            // Set pen alignment
            //_pen.Alignment = penAlignment;

            // Set a border line style
            _pen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);

            if (backGradientStyle == GradientStyle.None)
            {
                // Set a bar color.
                _solidBrush.Color = backColor;
                brush = _solidBrush;
            }
            else
            {
                // If a gradient type  is set create a brush with gradient
                brush = GetGradientBrush(rect, backColor, backSecondaryColor, backGradientStyle);
            }

            if (backHatchStyle != ChartHatchStyle.None)
            {
                brush = GetHatchBrush(backHatchStyle, backColor, backSecondaryColor);
            }

            if (backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
            {
                backBrush = brush;
                brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor);
            }

            // For inset alignment resize fill rectangle
            SKRect fillRect;

            // The fill rectangle is same
            fillRect = new SKRect(rect.Left + borderWidth, rect.Top + borderWidth, rect.Width - borderWidth * 2, rect.Height - borderWidth * 2);

            // FillRectangle and DrawRectangle works differently with SKRect.
            fillRect.Right += 1;
            fillRect.Bottom += 1;

            // Draw rectangle image
            if (backImage.Length > 0 && (backImageWrapMode == ChartImageWrapMode.Unscaled || backImageWrapMode == ChartImageWrapMode.Scaled))
            {
                // Load image
                SKImage image = _common.ImageLoader.LoadImage(backImage);

                // Prepare image properties (transparent color)
                if (backImageTransparentColor != SKColor.Empty)
                {
                    using var bmp = SKBitmap.FromImage(image);
                    // loop trough every pixel and set alpha if equal to transparent color
                    int width = bmp.Width;
                    int height = bmp.Height;
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            var color = bmp.GetPixel(col, row);

                            if (color.Red == backImageTransparentColor.Red && color.Green == backImageTransparentColor.Green && color.Blue == backImageTransparentColor.Blue)
                            {
                                bmp.SetPixel(col, row, color.WithAlpha(0x00));
                            }
                        }
                    }
                    using var bmp2 = new SKBitmap(bmp.Width, bmp.Height, bmp.ColorType, SKAlphaType.Premul);
                    bmp.CopyTo(bmp2);
                    image = SKImage.FromBitmap(bmp2);
                }

                // Draw scaled image
                SKRect imageRect = new();
                imageRect.Left = fillRect.Left;
                imageRect.Top = fillRect.Top;
                imageRect.Right = fillRect.Right;
                imageRect.Bottom = fillRect.Bottom;

                // Draw unscaled image using align property
                if (backImageWrapMode == ChartImageWrapMode.Unscaled)
                {
                    SKSize imageAbsSize = new();

                    ImageLoader.GetAdjustedImageSize(image, Graphics, ref imageAbsSize);

                    // Calculate image position
                    imageRect.Size = new(imageAbsSize.Width, imageAbsSize.Height);

                    // Adjust position with alignment property
                    if (imageRect.Width < fillRect.Width)
                    {
                        if (backImageAlign == ChartImageAlignmentStyle.BottomRight ||
                            backImageAlign == ChartImageAlignmentStyle.Right ||
                            backImageAlign == ChartImageAlignmentStyle.TopRight)
                        {
                            imageRect.Left = fillRect.Right - imageRect.Width;
                        }
                        else if (backImageAlign == ChartImageAlignmentStyle.Bottom ||
                            backImageAlign == ChartImageAlignmentStyle.Center ||
                            backImageAlign == ChartImageAlignmentStyle.Top)
                        {
                            imageRect.Left = fillRect.Left + (fillRect.Width - imageRect.Width) / 2;
                        }
                    }
                    if (imageRect.Height < fillRect.Height)
                    {
                        if (backImageAlign == ChartImageAlignmentStyle.BottomRight ||
                            backImageAlign == ChartImageAlignmentStyle.Bottom ||
                            backImageAlign == ChartImageAlignmentStyle.BottomLeft)
                        {
                            imageRect.Top = fillRect.Bottom - imageRect.Height;
                        }
                        else if (backImageAlign == ChartImageAlignmentStyle.Left ||
                            backImageAlign == ChartImageAlignmentStyle.Center ||
                            backImageAlign == ChartImageAlignmentStyle.Right)
                        {
                            imageRect.Top = fillRect.Top + (fillRect.Height - imageRect.Height) / 2;
                        }
                    }
                }

                // Fill background with brush
                this.FillRectangle(brush, rect.Left, rect.Top, rect.Width + 1, rect.Height + 1);

                // Draw image
                DrawImage(image,
                    new SKRect((int)Math.Round(imageRect.Left), (int)Math.Round(imageRect.Top), (int)Math.Round(imageRect.Right), (int)Math.Round(imageRect.Bottom)),
                    0, 0, image.Width, image.Height, null);
            }
            // Draw rectangle
            else
            {
                if (backBrush != null && backImageTransparentColor != SKColor.Empty)
                {
                    // Fill background with brush
                    this.FillRectangle(backBrush, rect.Left, rect.Top, rect.Width + 1, rect.Height + 1);
                }
                this.FillRectangle(brush, rect.Left, rect.Top, rect.Width + 1, rect.Height + 1);
            }

            // Set pen alignment
            if (borderDashStyle != ChartDashStyle.NotSet)
            {
                if (borderWidth > 1)
                    this.DrawRectangle(_pen, rect.Left, rect.Top, rect.Width + 1, rect.Height + 1);
                else if (borderWidth == 1)
                    this.DrawRectangle(_pen, rect.Left, rect.Top, rect.Width, rect.Height);
            }

            // Dispose Image and Gradient
            if (backGradientStyle != GradientStyle.None)
            {
                brush.Dispose();
            }
            if (backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
            {
                brush.Dispose();
            }
            if (backHatchStyle != ChartHatchStyle.None)
            {
                brush.Dispose();
            }

            // Set Old Smoothing Mode
            SmoothingMode = oldMode;
        }

        /// <summary>
        /// Draw a bar with shadow.
        /// </summary>
        /// <param name="rectF">Size of rectangle</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="penAlignment">Pen Alignment</param>
        /// <param name="barDrawingStyle">Bar drawing style.</param>
        /// <param name="isVertical">True if a vertical bar.</param>
        internal void FillRectangleRel(SKRect rectF,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            SKColor shadowColor,
            int shadowOffset,
            PenAlignment penAlignment,
            BarDrawingStyle barDrawingStyle,
            bool isVertical)
        {
            FillRectangleRel(
                rectF,
                backColor,
                backHatchStyle,
                backImage,
                backImageWrapMode,
                backImageTransparentColor,
                backImageAlign,
                backGradientStyle,
                backSecondaryColor,
                borderColor,
                borderWidth,
                borderDashStyle,
                shadowColor,
                shadowOffset,
                penAlignment,
                false,
                0,
                false,
                barDrawingStyle,
                isVertical);
        }

        /// <summary>
        /// Draw a bar with shadow.
        /// </summary>
        /// <param name="rectF">Size of rectangle</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="penAlignment">Pen Alignment</param>
        internal void FillRectangleRel(SKRect rectF,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            SKColor shadowColor,
            int shadowOffset,
            PenAlignment penAlignment)
        {
            FillRectangleRel(
                rectF,
                backColor,
                backHatchStyle,
                backImage,
                backImageWrapMode,
                backImageTransparentColor,
                backImageAlign,
                backGradientStyle,
                backSecondaryColor,
                borderColor,
                borderWidth,
                borderDashStyle,
                shadowColor,
                shadowOffset,
                penAlignment,
                false,
                0,
                false,
                BarDrawingStyle.Default,
                true);
        }

        /// <summary>
        /// Draws rectangle or circle (inside rectangle) with shadow.
        /// </summary>
        /// <param name="rectF">Size of rectangle</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="penAlignment">Pen Alignment</param>
        /// <param name="circular">Draw circular shape inside the rectangle.</param>
        /// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
        /// <param name="circle3D">3D Circle must be drawn.</param>
        internal void FillRectangleRel(SKRect rectF,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            SKColor shadowColor,
            int shadowOffset,
            PenAlignment penAlignment,
            bool circular,
            int circularSectorsCount,
            bool circle3D)
        {
            FillRectangleRel(
                rectF,
                backColor,
                backHatchStyle,
                backImage,
                backImageWrapMode,
                backImageTransparentColor,
                backImageAlign,
                backGradientStyle,
                backSecondaryColor,
                borderColor,
                borderWidth,
                borderDashStyle,
                shadowColor,
                shadowOffset,
                penAlignment,
                circular,
                circularSectorsCount,
                circle3D,
                BarDrawingStyle.Default,
                true);
        }

        /// <summary>
        /// Draws rectangle or circle (inside rectangle) with shadow.
        /// </summary>
        /// <param name="rectF">Size of rectangle</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="penAlignment">Pen Alignment</param>
        /// <param name="circular">Draw circular shape inside the rectangle.</param>
        /// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
        /// <param name="circle3D">3D Circle must be drawn.</param>
        /// <param name="barDrawingStyle">Bar drawing style.</param>
        /// <param name="isVertical">True if a vertical bar.</param>
        internal void FillRectangleRel(SKRect rectF,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            SKColor shadowColor,
            int shadowOffset,
            PenAlignment penAlignment,
            bool circular,
            int circularSectorsCount,
            bool circle3D,
            BarDrawingStyle barDrawingStyle,
            bool isVertical)
        {
            SKPaint brush = null;
            SKPaint backBrush = null;

            // Remember SmoothingMode and turn off anti aliasing
            SmoothingMode oldSmoothingMode = SmoothingMode;
            if (!circular)
            {
                SmoothingMode = SmoothingMode.Default;
            }

            // Color is empty
            if (backColor == SKColor.Empty)
            {
                backColor = SKColors.White;
            }

            if (backSecondaryColor == SKColor.Empty)
            {
                backSecondaryColor = SKColors.White;
            }

            if (borderColor == SKColor.Empty || borderDashStyle == ChartDashStyle.NotSet)
            {
                borderWidth = 0;
            }

            // Get absolute coordinates
            SKRect rect = GetAbsoluteRectangle(rectF);

            // Rectangle width and height can not be very small value
            if (rect.Width < 1.0F && rect.Width > 0.0F)
            {
                rect.Right = rect.Left + 1.0F;
            }

            if (rect.Height < 1.0F && rect.Height > 0.0F)
            {
                rect.Bottom = rect.Top + 1.0F;
            }

            // Round the values
            rect = Round(rect);

            // SkiaSharp do not support inset pen styles - use same rectangle
            SKRect fillRect;

            // For inset alignment resize fill rectangle
            if (penAlignment == PenAlignment.Inset && borderWidth > 0)
            {
                // The fill rectangle is resized because of border size.
                fillRect = new SKRect(
                    rect.Left + borderWidth,
                    rect.Top + borderWidth,
                    rect.Right - borderWidth,
                    rect.Bottom - borderWidth);
            }
            else
            {
                // The fill rectangle is same
                fillRect = rect;
            }

            if (backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
            {
                backBrush = brush;
                brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor);
            }
            else if (backHatchStyle != ChartHatchStyle.None)
            {
                brush = GetHatchBrush(backHatchStyle, backColor, backSecondaryColor);
            }
            else if (backGradientStyle != GradientStyle.None)
            {
                // If a gradient type  is set create a brush with gradient
                brush = GetGradientBrush(rect, backColor, backSecondaryColor, backGradientStyle);
            }
            else
            {
                // Set a bar color.
                if (backColor == SKColor.Empty || backColor == SKColors.Transparent)
                {
                    brush = null;
                }
                else
                {
                    brush = new SKPaint() { Color = backColor, Style = SKPaintStyle.Fill };
                }
            }

            // Draw shadow
            FillRectangleShadowAbs(rect, shadowColor, shadowOffset, backColor, circular, circularSectorsCount);

            // Draw rectangle image
            if (backImage.Length > 0 && (backImageWrapMode == ChartImageWrapMode.Unscaled || backImageWrapMode == ChartImageWrapMode.Scaled))
            {
                // Load image
                SKImage image = _common.ImageLoader.LoadImage(backImage);

                // Prepare image properties (transparent color)
                if (backImageTransparentColor != SKColor.Empty)
                {
                    using var bmp = SKBitmap.FromImage(image);
                    // loop trough every pixel and set alpha if equal to transparent color
                    int width = bmp.Width;
                    int height = bmp.Height;
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            var color = bmp.GetPixel(col, row);

                            if (color.Red == backImageTransparentColor.Red && color.Green == backImageTransparentColor.Green && color.Blue == backImageTransparentColor.Blue)
                            {
                                bmp.SetPixel(col, row, color.WithAlpha(0x00));
                            }
                        }
                    }
                    using var bmp2 = new SKBitmap(bmp.Width, bmp.Height, bmp.ColorType, SKAlphaType.Premul);
                    bmp.CopyTo(bmp2);
                    image = SKImage.FromBitmap(bmp2);
                }

                // Draw scaled image
                SKRect imageRect = new(fillRect.Left, fillRect.Top, fillRect.Right, fillRect.Bottom);

                SKSize imageAbsSize = new();

                // Calculate unscaled image position
                if (backImageWrapMode == ChartImageWrapMode.Unscaled)
                {
                    ImageLoader.GetAdjustedImageSize(image, Graphics, ref imageAbsSize);

                    // Calculate image position
                    imageRect.Size = new SKSize(Math.Min(fillRect.Width, imageAbsSize.Width), Math.Min(fillRect.Height, imageAbsSize.Height));

                    // Adjust position with alignment property
                    if (imageRect.Width < fillRect.Width)
                    {
                        if (backImageAlign == ChartImageAlignmentStyle.BottomRight ||
                            backImageAlign == ChartImageAlignmentStyle.Right ||
                            backImageAlign == ChartImageAlignmentStyle.TopRight)
                        {
                            imageRect.Left = fillRect.Right - imageRect.Width;
                        }
                        else if (backImageAlign == ChartImageAlignmentStyle.Bottom ||
                            backImageAlign == ChartImageAlignmentStyle.Center ||
                            backImageAlign == ChartImageAlignmentStyle.Top)
                        {
                            imageRect.Left = fillRect.Left + (fillRect.Width - imageRect.Width) / 2;
                        }
                    }
                    if (imageRect.Height < fillRect.Height)
                    {
                        if (backImageAlign == ChartImageAlignmentStyle.BottomRight ||
                            backImageAlign == ChartImageAlignmentStyle.Bottom ||
                            backImageAlign == ChartImageAlignmentStyle.BottomLeft)
                        {
                            imageRect.Top = fillRect.Bottom - imageRect.Height;
                        }
                        else if (backImageAlign == ChartImageAlignmentStyle.Left ||
                            backImageAlign == ChartImageAlignmentStyle.Center ||
                            backImageAlign == ChartImageAlignmentStyle.Right)
                        {
                            imageRect.Top = fillRect.Top + (fillRect.Height - imageRect.Height) / 2;
                        }
                    }
                }

                // Fill background with brush
                if (brush != null)
                {
                    if (circular)
                        this.DrawCircleAbs(null, brush, fillRect, circularSectorsCount, circle3D);
                    else
                        this.FillRectangle(brush, fillRect);
                }

                // Draw image
                this.DrawImage(image,
                    new SKRect((int)Math.Round(imageRect.Left), (int)Math.Round(imageRect.Top), (int)Math.Round(imageRect.Right), (int)Math.Round(imageRect.Bottom)),
                    0, 0,
                    (backImageWrapMode == ChartImageWrapMode.Unscaled) ? imageRect.Width * image.Width / imageAbsSize.Width : image.Width,
                    (backImageWrapMode == ChartImageWrapMode.Unscaled) ? imageRect.Height * image.Height / imageAbsSize.Height : image.Height,
                    null);
            }
            // Draw rectangle
            else
            {
                if (backBrush != null && backImageTransparentColor != SKColor.Empty)
                {
                    // Fill background with brush
                    if (circular)
                        this.DrawCircleAbs(null, backBrush, fillRect, circularSectorsCount, circle3D);
                    else
                        this.FillRectangle(backBrush, fillRect);
                }

                if (brush != null)
                {
                    if (circular)
                        this.DrawCircleAbs(null, brush, fillRect, circularSectorsCount, circle3D);
                    else
                        this.FillRectangle(brush, fillRect);
                }
            }

            // Draw different bar style
            DrawRectangleBarStyle(barDrawingStyle, isVertical, fillRect);

            // Draw border
            if (borderWidth > 0 && borderDashStyle != ChartDashStyle.NotSet)
            {
                // Set a border line color
                if (_pen.Color != borderColor)
                {
                    _pen.Color = borderColor;
                }

                // Set a border line width
                if (_pen.StrokeWidth != borderWidth)
                {
                    _pen.StrokeWidth = borderWidth;
                }

                // Set a border line style
                if (_pen.PathEffect != GetPenStyle(borderDashStyle, borderWidth))
                {
                    _pen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);
                }

                // Draw border
                if (circular)
                {
                    DrawCircleAbs(_pen, null, rect, circularSectorsCount, false);
                }
                else
                {
                    // Draw rectangle
                    this.DrawRectangle(_pen, rect.Left, rect.Top, rect.Width, rect.Height);
                }
            }

            // Dispose Image and Gradient
            if (brush != null)
            {
                brush.Dispose();
            }

            // Return old smoothing mode
            SmoothingMode = oldSmoothingMode;
        }

        /// <summary>
        /// Draw Shadow for a bar
        /// </summary>
        /// <param name="rect">Bar rectangle</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="backColor">Back Color</param>
        internal void FillRectangleShadowAbs(
            SKRect rect,
            SKColor shadowColor,
            float shadowOffset,
            SKColor backColor)
        {
            FillRectangleShadowAbs(
                rect,
                shadowColor,
                shadowOffset,
                backColor,
                false,
                0);
        }

        /// <summary>
        /// Draw Shadow for a bar
        /// </summary>
        /// <param name="rect">Bar rectangle</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="backColor">Back Color</param>
        /// <param name="circular">Draw circular shape inside the rectangle.</param>
        /// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
        internal void FillRectangleShadowAbs(
            SKRect rect,
            SKColor shadowColor,
            float shadowOffset,
            SKColor backColor,
            bool circular,
            int circularSectorsCount)
        {
            // Do not draw shadoe for empty rectangle
            if (rect.Height == 0 || rect.Width == 0 || shadowOffset == 0)
            {
                return;
            }

            // Do not draw  shadow if color is IsEmpty or offset is 0
            if (shadowOffset == 0 || shadowColor == SKColor.Empty)
            {
                return;
            }

            // Draw usual or "soft" shadows
            if (!softShadows || circularSectorsCount > 2)
            {
                SKRect absolute;
                SKRect offset = SKRect.Empty;

                absolute = Round(rect);

                // Change shadow color
                using SKPaint shadowBrush = new() { Style = SKPaintStyle.Fill, Color = (shadowColor.Alpha != 255) ? shadowColor : Color.FromArgb(backColor.Alpha / 2, shadowColor) };
                // Shadow Position
                offset.Left = absolute.Left + shadowOffset;
                offset.Top = absolute.Top + shadowOffset;
                offset.Size = new(absolute.Width, absolute.Height);

                // Draw rectangle
                if (circular)
                    DrawCircleAbs(null, shadowBrush, offset, circularSectorsCount, false);
                else
                    FillRectangle(shadowBrush, offset);
            }
            else
            {
                SKRect absolute;
                SKRect offset = SKRect.Empty;

                absolute = Round(rect);

                // Shadow Position
                offset.Left = absolute.Left + shadowOffset - 1;
                offset.Top = absolute.Top + shadowOffset - 1;
                offset.Size = new(absolute.Width + 2, absolute.Height + 2);

                // Calculate rounded rect radius
                float radius = shadowOffset * 0.7f;
                radius = Math.Max(radius, 2f);
                radius = Math.Min(radius, offset.Width / 4f);
                radius = Math.Min(radius, offset.Height / 4f);
                radius = (float)Math.Ceiling(radius);
                if (circular)
                {
                    radius = offset.Width / 2f;
                }

                // Create rounded rectangle path
                SKPath path = new();
                if (circular && offset.Width != offset.Height)
                {
                    float radiusX = offset.Width / 2f;
                    float radiusY = offset.Height / 2f;
                    path.AddLine(offset.Left + radiusX, offset.Top, offset.Right - radiusX, offset.Top);
                    path.AddArc(new SKRect(offset.Right - 2f * radiusX, offset.Top, 2f * radiusX, 2f * radiusY), 270, 90);
                    path.AddLine(offset.Right, offset.Top + radiusY, offset.Right, offset.Bottom - radiusY);
                    path.AddArc(new SKRect(offset.Right - 2f * radiusX, offset.Bottom - 2f * radiusY, 2f * radiusX, 2f * radiusY), 0, 90);
                    path.AddLine(offset.Right - radiusX, offset.Bottom, offset.Left + radiusX, offset.Bottom);
                    path.AddArc(new SKRect(offset.Left, offset.Bottom - 2f * radiusY, 2f * radiusX, 2f * radiusY), 90, 90);
                    path.AddLine(offset.Left, offset.Bottom - radiusY, offset.Left, offset.Top + radiusY);
                    path.AddArc(new SKRect(offset.Left, offset.Top, 2f * radiusX, 2f * radiusY), 180, 90);
                }
                else
                {
                    path.AddLine(offset.Left + radius, offset.Top, offset.Right - radius, offset.Top);
                    path.AddArc(new SKRect(offset.Right - 2f * radius, offset.Top, 2f * radius, 2f * radius), 270, 90);
                    path.AddLine(offset.Right, offset.Top + radius, offset.Right, offset.Bottom - radius);
                    path.AddArc(new SKRect(offset.Right - 2f * radius, offset.Bottom - 2f * radius, 2f * radius, 2f * radius), 0, 90);
                    path.AddLine(offset.Right - radius, offset.Bottom, offset.Left + radius, offset.Bottom);
                    path.AddArc(new SKRect(offset.Left, offset.Bottom - 2f * radius, 2f * radius, 2f * radius), 90, 90);
                    path.AddLine(offset.Left, offset.Bottom - radius, offset.Left, offset.Top + radius);
                    path.AddArc(new SKRect(offset.Left, offset.Top, 2f * radius, 2f * radius), 180, 90);
                }

                SKPaint shadowBrush = new();
                shadowBrush.ImageFilter = SKImageFilter.CreateDropShadowOnly(
                    shadowOffset, shadowOffset,
                    1 - 2f * shadowOffset / offset.Width,
                    1 - 2f * shadowOffset / offset.Height,
                    shadowColor);

                // Draw rectangle
                this.FillPath(shadowBrush, path);
            }
        }

        /// <summary>
        /// Gets the path of the polygon which represent the circular area.
        /// </summary>
        /// <param name="position">Circle position.</param>
        /// <param name="polygonSectorsNumber">Number of sectors for the polygon.</param>
        /// <returns>Graphics path of the polygon circle.</returns>
        internal SKPath GetPolygonCirclePath(SKRect position, int polygonSectorsNumber)
        {
            SKPoint firstPoint = new(position.Left + position.Width / 2f, position.Top);
            SKPoint centerPoint = new(position.Left + position.Width / 2f, position.Top + position.Height / 2f);
            float sectorSize = 0f;
            SKPath path = new();
            SKPoint prevPoint = SKPoint.Empty;
            float curentSector = 0f;

            // Get sector size
            if (polygonSectorsNumber <= 2)
            {
                // Circle sector size
                sectorSize = 1f;
            }
            else
            {
                // Polygon sector size
                sectorSize = 360f / ((float)polygonSectorsNumber);
            }

            // Loop throug all sectors
            for (curentSector = 0f; curentSector < 360f; curentSector += sectorSize)
            {
                // Create matrix
                SKMatrix matrix = SkiaSharpExtensions.CreateRotationDegrees(curentSector, centerPoint);

                // Get point and rotate it
                SKPoint[] points = new SKPoint[] { firstPoint };
                matrix.TransformPoints(points);

                // Add point into the path
                if (!prevPoint.IsEmpty)
                {
                    path.AddLine(prevPoint, points[0]);
                }

                // Remember last point
                prevPoint = points[0];
            }

            path.Close();

            return path;
        }
        /// <summary>
        /// Creates 3D sector brush.
        /// </summary>
        /// <param name="brush">Original brush.</param>
        /// <param name="curentSector">Sector position.</param>
        /// <param name="sectorSize">Sector size.</param>
        /// <returns>3D brush.</returns>
        internal SKPaint GetSector3DBrush(SKPaint brush, float curentSector, float sectorSize)
        {
            // Get color from the brush
            SKColor brushColor = brush.Color;

            // Adjust sector angle
            curentSector -= sectorSize / 2f;

            // Make adjustment for polygon circle with 5 segments
            // to avoid the issue that bottom segment is too dark
            if (sectorSize == 72f && curentSector == 180f)
            {
                curentSector *= 0.8f;
            }

            // No angles more than 180
            if (curentSector > 180)
            {
                curentSector = 360f - curentSector;
            }
            curentSector /= 180F;

            // Get brush
            brushColor = GetBrightGradientColor(brushColor, curentSector);

            // Get brush
            return new SKPaint() { Style = SKPaintStyle.Fill, Color = brushColor };
        }
        #endregion Rectangle Methods

        #region Coordinates converter

        /// <summary>
        /// This method takes a SKPoint object and converts its relative coordinates
        /// to absolute coordinates.
        /// </summary>
        /// <param name="point">SKPoint object in relative coordinates.</param>
        /// <returns>SKPoint object in absolute coordinates.</returns>
        public SKPoint GetAbsolutePoint(SKPoint point)
        {
            SKPoint absolute = SKPoint.Empty;

            // Convert relative coordinates to absolute coordinates
            absolute.X = point.X * (_width - 1) / 100F;
            absolute.Y = point.Y * (_height - 1) / 100F;

            // Return Absolute coordinates
            return absolute;
        }

        /// <summary>
        /// This method takes a SKRect structure and converts its relative coordinates
        /// to absolute coordinates.
        /// </summary>
        /// <param name="rectangle">SKRect object in relative coordinates.</param>
        /// <returns>SKRect object in absolute coordinates.</returns>
        public SKRect GetAbsoluteRectangle(SKRect rectangle)
        {
            // Check arguments
            if (rectangle == SKRect.Empty)
                throw new ArgumentNullException(nameof(rectangle));

            SKRect absolute = SKRect.Empty;

            // Convert relative coordinates to absolute coordinates
            absolute.Left = rectangle.Left * (_width - 1) / 100F;
            absolute.Top = rectangle.Top * (_height - 1) / 100F;
            absolute.Right = absolute.Left + rectangle.Width * (_width - 1) / 100F;
            absolute.Bottom = absolute.Top + rectangle.Height * (_height - 1) / 100F;

            // Return Absolute coordinates
            return absolute;
        }

        /// <summary>
        /// This method takes a SKSize object that uses relative coordinates
        /// and returns a SKSize object that uses absolute coordinates.
        /// </summary>
        /// <param name="size">SKSize object in relative coordinates.</param>
        /// <returns>SKSize object in absolute coordinates.</returns>
        public SKSize GetAbsoluteSize(SKSize size)
        {
            // Check arguments
            if (size == SKSize.Empty)
                throw new ArgumentNullException(nameof(size));

            SKSize absolute = SKSize.Empty;

            // Convert relative coordinates to absolute coordinates
            absolute.Width = size.Width * (_width - 1) / 100F;
            absolute.Height = size.Height * (_height - 1) / 100F;

            // Return Absolute coordinates
            return absolute;
        }

        /// <summary>
        /// This method takes a SKPoint object that is using absolute coordinates
        /// and returns a SKPoint object that uses relative coordinates.
        /// </summary>
        /// <param name="point">SKPoint object in absolute coordinates.</param>
        /// <returns>SKPoint object in relative coordinates.</returns>
        public SKPoint GetRelativePoint(SKPoint point)
        {
            // Check arguments
            if (point == SKPoint.Empty)
                throw new ArgumentNullException(nameof(point));

            SKPoint relative = SKPoint.Empty;

            // Convert absolute coordinates to relative coordinates
            relative.X = point.X * 100F / (_width - 1);
            relative.Y = point.Y * 100F / (_height - 1);

            // Return Relative coordinates
            return relative;
        }

        /// <summary>
        /// This method takes a SKRect structure that is using absolute coordinates
        /// and returns a SKRect object that uses relative coordinates.
        /// </summary>
        /// <param name="rectangle">SKRect structure in absolute coordinates.</param>
        /// <returns>SKRect structure in relative coordinates.</returns>
        public SKRect GetRelativeRectangle(SKRect rectangle)
        {
            // Check arguments
            if (rectangle == SKRect.Empty)
                throw new ArgumentNullException(nameof(rectangle));

            SKRect relative = SKRect.Empty;

            // Convert absolute coordinates to relative coordinates
            relative.Left = rectangle.Left * 100F / (_width - 1);
            relative.Top = rectangle.Top * 100F / (_height - 1);
            relative.Right = relative.Left + rectangle.Width * 100F / (_width - 1);
            relative.Bottom = relative.Top + rectangle.Height * 100F / (_height - 1);

            // Return Relative coordinates
            return relative;
        }
        /// <summary>
        /// This method takes a SKSize object that uses absolute coordinates
        /// and returns a SKSize object that uses relative coordinates.
        /// </summary>
        /// <param name="size">SKSize object in absolute coordinates.</param>
        /// <returns>SKSize object in relative coordinates.</returns>
        public SKSize GetRelativeSize(SKSize size)
        {
            // Check arguments
            if (size == SKSize.Empty)
                throw new ArgumentNullException(nameof(size));

            SKSize relative = SKSize.Empty;

            // Convert absolute coordinates to relative coordinates
            relative.Width = size.Width * 100F / (_width - 1);
            relative.Height = size.Height * 100F / (_height - 1);

            // Return relative coordinates
            return relative;
        }
        #endregion Coordinates converter

        #region Border drawing helper methods

        /// <summary>
        /// Helper function which creates a rounded rectangle path.
        /// </summary>
        /// <param name="rect">Rectangle coordinates.</param>
        /// <param name="cornerRadius">Array of 4 corners radius.</param>
        /// <returns>Graphics path object.</returns>
        internal static SKPath CreateRoundedRectPath(SKRect rect, float[] cornerRadius)
        {
            // Create rounded rectangle path
            SKPath path = new();
            path.AddLine(rect.Left + cornerRadius[0], rect.Top, rect.Right - cornerRadius[1], rect.Top);
            path.AddArc(new SKRect(rect.Right - 2f * cornerRadius[1], rect.Top, rect.Right, rect.Top + 2f * cornerRadius[2]), 270, 90);
            path.AddLine(rect.Right, rect.Top + cornerRadius[2], rect.Right, rect.Bottom - cornerRadius[3]);
            path.AddArc(new SKRect(rect.Right - 2f * cornerRadius[4], rect.Bottom - 2f * cornerRadius[3], 2f * cornerRadius[4], rect.Bottom), 0, 90);
            path.AddLine(rect.Right - cornerRadius[4], rect.Bottom, rect.Left + cornerRadius[5], rect.Bottom);
            path.AddArc(new SKRect(rect.Left, rect.Bottom - 2f * cornerRadius[6], rect.Left + 2f * cornerRadius[5], rect.Bottom), 90, 90);
            path.AddLine(rect.Left, rect.Bottom - cornerRadius[6], rect.Left, rect.Top + cornerRadius[7]);
            path.AddArc(new SKRect(rect.Left, rect.Top, rect.Left + 2f * cornerRadius[0], rect.Left + 2f * cornerRadius[7]), 180, 90);

            return path;
        }

        /// <summary>
        /// Draws 3D border in absolute coordinates.
        /// </summary>
        /// <param name="borderSkin">Border skin object.</param>
        /// <param name="absRect">Rectangle of the border (pixel coordinates).</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        internal void Draw3DBorderAbs(
            BorderSkin borderSkin,
            SKRect absRect,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle)
        {
            // Check input parameters
            if (_common == null || borderSkin.SkinStyle == BorderSkinStyle.None || absRect.Width == 0 || absRect.Height == 0)
            {
                return;
            }

            // Find required border interface
            IBorderType borderTypeInterface = _common.BorderTypeRegistry.GetBorderType(borderSkin.SkinStyle.ToString());
            if (borderTypeInterface != null)
            {
                borderTypeInterface.Resolution = 96;
                // Draw border
                borderTypeInterface.DrawBorder(this, borderSkin, absRect, backColor, backHatchStyle, backImage, backImageWrapMode,
                    backImageTransparentColor, backImageAlign, backGradientStyle, backSecondaryColor,
                    borderColor, borderWidth, borderDashStyle);
            }
        }

        /// <summary>
        /// Draws 3D border in absolute coordinates.
        /// </summary>
        /// <param name="borderSkin">Border skin object.</param>
        /// <param name="rect">Rectangle of the border (pixel coordinates).</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type </param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        internal void Draw3DBorderRel(
            BorderSkin borderSkin,
            SKRect rect,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            ChartImageAlignmentStyle backImageAlign,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle)
        {
            Draw3DBorderAbs(borderSkin, GetAbsoluteRectangle(rect), backColor, backHatchStyle,
                backImage, backImageWrapMode, backImageTransparentColor, backImageAlign, backGradientStyle,
                backSecondaryColor, borderColor, borderWidth, borderDashStyle);
        }

        /// <summary>
        /// Helper function which draws a shadow of the rounded rect.
        /// </summary>
        /// <param name="rect">Rectangle coordinates.</param>
        /// <param name="cornerRadius">Array of 4 corners radius.</param>
        /// <param name="radius">Rounding radius.</param>
        /// <param name="centerColor">Center color.</param>
        /// <param name="surroundColor">Surrounding color.</param>
        /// <param name="shadowScale">Shadow scale value.</param>
        internal void DrawRoundedRectShadowAbs(SKRect rect, float[] cornerRadius, float radius, SKColor centerColor, SKColor surroundColor, float shadowScale)
        {
            // Create rounded rectangle path
            SKPath path = CreateRoundedRectPath(rect, cornerRadius);

            // Create gradient brush
            SKPaint shadowBrush = new();
            shadowBrush.ImageFilter = SKImageFilter.CreateDropShadowOnly(
                rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f,
                radius, radius,
                centerColor);

            // Draw rounded rectangle
            FillPath(shadowBrush, path);

            if (path != null)
            {
                path.Dispose();
            }
        }
        #endregion Border drawing helper methods

        #region Pie Method

        /// <summary>
        /// Helper function that retrieves pie drawing style.
        /// </summary>
        /// <param name="point">Data point to get the drawing style for.</param>
        /// <returns>pie drawing style.</returns>
        internal static PieDrawingStyle GetPieDrawingStyle(DataPoint point)
        {
            // Get column drawing style
            PieDrawingStyle pieDrawingStyle = PieDrawingStyle.Default;
            string styleName = point[CustomPropertyName.PieDrawingStyle];
            if (styleName != null)
            {
                if (string.Compare(styleName, "Default", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pieDrawingStyle = PieDrawingStyle.Default;
                }
                else if (string.Compare(styleName, "SoftEdge", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pieDrawingStyle = PieDrawingStyle.SoftEdge;
                }
                else if (string.Compare(styleName, "Concave", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pieDrawingStyle = PieDrawingStyle.Concave;
                }
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(styleName, "PieDrawingStyle")));
                }
            }
            return pieDrawingStyle;
        }

        /// <summary>
        /// Draws a pie defined by an ellipse specified by a Rectangle structure and two radial lines.
        /// </summary>
        /// <param name="rect">Rectangle structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        /// <param name="backColor">Fill color</param>
        /// <param name="backHatchStyle">Fill Hatch Style</param>
        /// <param name="backImage">Fill texture</param>
        /// <param name="backImageWrapMode">Texture image mode</param>
        /// <param name="backImageTransparentColor">Texture transparent color</param>
        /// <param name="backGradientStyle">Fill Gradient type </param>
        /// <param name="backSecondaryColor">Fill Gradient Second Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
        /// <param name="shadow">True if shadow is active</param>
        /// <param name="doughnut">True if Doughnut is drawn instead of pie</param>
        /// <param name="doughnutRadius">Internal radius of the doughnut</param>
        /// <param name="pieDrawingStyle">Pie drawing style.</param>
        internal void DrawPieRel(
            SKRect rect,
            float startAngle,
            float sweepAngle,
            SKColor backColor,
            ChartHatchStyle backHatchStyle,
            string backImage,
            ChartImageWrapMode backImageWrapMode,
            SKColor backImageTransparentColor,
            GradientStyle backGradientStyle,
            SKColor backSecondaryColor,
            SKColor borderColor,
            int borderWidth,
            ChartDashStyle borderDashStyle,
            bool shadow,
            bool doughnut,
            float doughnutRadius,
            PieDrawingStyle pieDrawingStyle
            )
        {
            SKPaint borderPen = null;   // Pen
            SKPaint fillBrush;        // Brush

            // Get absolute rectangle
            SKRect absRect = GetAbsoluteRectangle(rect);

            if (doughnutRadius == 100.0)
            {
                doughnut = false;
            }

            if (doughnutRadius == 0.0)
            {
                return;
            }

            // Create Brush
            if (backHatchStyle != ChartHatchStyle.None)
            {
                // Create Hatch Brush
                fillBrush = GetHatchBrush(backHatchStyle, backColor, backSecondaryColor);
            }
            else if (backGradientStyle != GradientStyle.None)
            {
                // Create gradient brush
                if (backGradientStyle == GradientStyle.Center)
                {
                    fillBrush = GetPieGradientBrush(absRect, backColor, backSecondaryColor);
                }
                else
                {
                    using SKPath path = new();
                    path.AddPie(absRect.Left, absRect.Top, absRect.Width, absRect.Height, startAngle, sweepAngle);
                    fillBrush = GetGradientBrush(path.GetBounds(), backColor, backSecondaryColor, backGradientStyle);
                }
            }
            else if (backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
            {
                // Create textured brush
                fillBrush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor);
            }
            else
            {
                // Create solid brush
                fillBrush = new() { Style = SKPaintStyle.Fill, Color = backColor };
            }

            // Create border Pen
            borderPen = new() { Color = borderColor, Style = SKPaintStyle.Stroke, StrokeWidth = borderWidth };

            // Set a border line style
            borderPen.PathEffect = GetPenStyle(borderDashStyle, borderWidth);

            // Use rounded line joins
            borderPen.StrokeJoin = SKStrokeJoin.Round;

            // Draw Doughnut
            if (doughnut)
            {
                using SKPath path = new();

                path.AddArc(new SKRect(
                    absRect.Left + absRect.Width * doughnutRadius / 200 - 1,
                    absRect.Top + absRect.Height * doughnutRadius / 200 - 1,
                    absRect.Left + absRect.Width * doughnutRadius / 200 - 1 + absRect.Width - absRect.Width * doughnutRadius / 100 + 2,
                    absRect.Top + absRect.Height * doughnutRadius / 200 - 1 + absRect.Height - absRect.Height * doughnutRadius / 100 + 2),
                    startAngle, sweepAngle);
                path.AddArc(new SKRect(absRect.Left, absRect.Top, absRect.Right, absRect.Bottom), startAngle + sweepAngle, -sweepAngle);

                path.Close();

                this.FillPath(fillBrush, path);

                // Draw Pie gradien effects
                DrawPieGradientEffects(pieDrawingStyle, absRect, startAngle, sweepAngle, doughnutRadius);

                // Draw Doughnut Border
                if (!shadow &&
                    borderWidth > 0 &&
                    borderDashStyle != ChartDashStyle.NotSet)
                {
                    this.DrawPath(borderPen, path);
                }
            }
            else // Draw Pie
            {
                // Draw Soft shadow for pie slice
                if (shadow && softShadows)
                {
                    DrawPieSoftShadow(startAngle, sweepAngle, absRect, backColor);
                }
                else
                {
                    // Fill Pie for normal shadow or colored pie slice
                    this.FillPie(fillBrush, absRect.Left, absRect.Top, absRect.Width, absRect.Height, startAngle, sweepAngle);

                    // Draw Pie gradien effects
                    DrawPieGradientEffects(pieDrawingStyle, absRect, startAngle, sweepAngle, -1f);
                }

                // Draw Pie Border
                if (!shadow &&
                    borderWidth > 0 &&
                    borderDashStyle != ChartDashStyle.NotSet)
                {
                    this.DrawPie(borderPen, absRect.Left, absRect.Top, absRect.Width, absRect.Height, startAngle, sweepAngle);
                }
            }

            // Dispose graphics objects
            if (borderPen != null)
            {
                borderPen.Dispose();
            }

            if (fillBrush != null)
            {
                fillBrush.Dispose();
            }
        }

        private void DrawPieGradientEffects(
            PieDrawingStyle pieDrawingStyle,
            SKRect position,
            float startAngle,
            float sweepAngle,
            float doughnutRadius)
        {
            if (pieDrawingStyle == PieDrawingStyle.Concave)
            {
                // Calculate the size of the shadow. Note: For Doughnut chart shadow is drawn
                // twice on the outside and inside radius.
                float minSize = Math.Min(position.Width, position.Height);
                float shadowSize = minSize * 0.05f;

                // Create brush path
                SKRect gradientPath = position;
                gradientPath.Inflate(-shadowSize, -shadowSize);
                using SKPath brushPath = new();
                brushPath.AddOval(gradientPath);

                // Create shadow path
                using SKPath path = new();
                if (doughnutRadius < 0f)
                {
                    path.AddArc(gradientPath.Round(), startAngle, sweepAngle);
                }
                else
                {
                    path.AddArc(new SKRect(
                        gradientPath.Left + position.Width * doughnutRadius / 200 - 1 - shadowSize,
                        gradientPath.Top + position.Height * doughnutRadius / 200 - 1 - shadowSize,
                        gradientPath.Width - position.Width * doughnutRadius / 100 + 2 + 2f * shadowSize,
                        gradientPath.Height - position.Height * doughnutRadius / 100 + 2 + 2f * shadowSize),
                        startAngle,
                        sweepAngle);
                    path.AddArc(new SKRect(gradientPath.Left, gradientPath.Top, gradientPath.Width, gradientPath.Height), startAngle + sweepAngle, -sweepAngle);
                }

                // Create linear gradient brush
                gradientPath.Inflate(1f, 1f);
                using SKPaint brush = new() { Style = SKPaintStyle.Fill };
                brush.Shader = SKShader.CreateLinearGradient(
                    gradientPath.Location, gradientPath.Location + gradientPath.Size,
                    new SKColor[] { SKColors.Black, SKColors.Transparent, SKColors.White },
                     SKShaderTileMode.Clamp
                    );

                // Fill shadow
                this.FillPath(brush, path);
            }
            else if (pieDrawingStyle == PieDrawingStyle.SoftEdge)
            {
                // Calculate the size of the shadow. Note: For Doughnut chart shadow is drawn
                // twice on the outside and inside radius.
                float minSize = Math.Min(position.Width, position.Height);
                float shadowSize = minSize / 10f;
                if (doughnutRadius > 0f)
                {
                    shadowSize = (minSize * doughnutRadius / 100f) / 8f;
                }

                // Create brush path
                using SKPath brushPath = new();
                brushPath.AddOval(position);

                // Create shadow path
                using (SKPath path = new())
                {
                    path.AddArc(new SKRect(position.Left + shadowSize, position.Top + shadowSize, position.Width - shadowSize * 2f, position.Height - shadowSize * 2f), startAngle, sweepAngle);
                    path.AddArc(new SKRect(position.Left, position.Top, position.Width, position.Height), startAngle + sweepAngle, -sweepAngle);
                    path.Close();

                    // Create shadow brush
                    using SKPaint brush = new() { Style = SKPaintStyle.Fill };
                    brush.Shader = SKShader.CreateLinearGradient(
                        position.Location, position.Location + position.Size,
                        new SKColor[] { Color.FromArgb(100, SKColors.Black), SKColors.Transparent, Color.FromArgb(100, SKColors.Black) },
                        SKShaderTileMode.Clamp
                        );

                    // Fill shadow
                    this.FillPath(brush, path);
                }

                // Draw inner shadow for the doughnut chart
                if (doughnutRadius > 0f)
                {
                    // Create brush path
                    using SKPath brushInsidePath = new();
                    SKRect innerPosition = position;
                    innerPosition.Inflate(-position.Width * doughnutRadius / 200f + shadowSize, -position.Height * doughnutRadius / 200f + shadowSize);
                    brushInsidePath.AddOval(innerPosition);

                    // Create shadow path
                    using SKPath path = new();
                    path.AddArc(new SKRect(innerPosition.Left + shadowSize, innerPosition.Top + shadowSize, innerPosition.Width - 2f * shadowSize, innerPosition.Height - 2f * shadowSize), startAngle, sweepAngle);
                    path.AddArc(new SKRect(innerPosition.Left, innerPosition.Top, innerPosition.Width, innerPosition.Height), startAngle + sweepAngle, -sweepAngle);
                    path.Close();

                    // Create shadow brush
                    using SKPaint brushInner = new() { Style = SKPaintStyle.Fill };
                    brushInner.Shader = SKShader.CreateLinearGradient(
                         innerPosition.Location, innerPosition.Location + innerPosition.Size,
                        new SKColor[] { Color.FromArgb(100, SKColors.Black), SKColors.Transparent, Color.FromArgb(100, SKColors.Black) },
                        SKShaderTileMode.Clamp
                        );

                    // Fill shadow
                    FillPath(brushInner, path);
                }
            }
        }

        /// <summary>
        /// The soft shadow of the pie
        /// </summary>
        /// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        /// <param name="absRect">Rectangle of the pie in absolute coordinates</param>
        /// <param name="backColor">Fill color</param>
        private void DrawPieSoftShadow(float startAngle, float sweepAngle, SKRect absRect, SKColor backColor)
        {
            SKPath path = new();

            path.AddOval(new SKRect(absRect.Left, absRect.Top, absRect.Left + absRect.Width, absRect.Top + absRect.Height));

            SKPaint brush = new()
            {
                Style = SKPaintStyle.Fill,
                Shader = SKShader.CreateLinearGradient(
                    path.Points.First(),
                    path.Points.Last(),
                    new SKColor[] {
                                Color.FromArgb( 0, backColor ),
                                Color.FromArgb( backColor.Alpha, backColor ),
                                Color.FromArgb( backColor.Alpha, backColor )},
                    new float[] { 0f, 0.05f, 1.0f },
                    SKShaderTileMode.Clamp)
            };

            FillPie(brush, absRect.Left, absRect.Top, absRect.Width, absRect.Height, startAngle, sweepAngle);
        }

        #endregion Pie Method

        #region Arrow Methods

        /// <summary>
        /// Draw Arrow.
        /// </summary>
        /// <param name="position">Position of the arrow</param>
        /// <param name="orientation">Orientation of the arrow - left, right, top, bottom </param>
        /// <param name="type">Arrow style: Triangle, Sharp Triangle, Lines</param>
        /// <param name="color">Color of the arrow</param>
        /// <param name="lineWidth">Line width</param>
        /// <param name="lineDashStyle">Line Dash style</param>
        /// <param name="shift">Distance from the chart area</param>
        /// <param name="size">Arrow size</param>
        internal void DrawArrowRel(SKPoint position, ArrowOrientation orientation, AxisArrowStyle type, SKColor color, int lineWidth, ChartDashStyle lineDashStyle, double shift, double size)
        {
            // Check if arrow should be drawn
            if (type == AxisArrowStyle.None)
            {
                return;
            }

            // Set a color
            using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = color };
            SKPoint endPoint = SKPoint.Empty; // End point of axis line
            SKPoint[] points; // arrow points
            SKPoint absolutePosition; // Absolute position of axis

            absolutePosition = GetAbsolutePoint(position);

            // Arrow type is triangle
            if (type == AxisArrowStyle.Triangle || type == AxisArrowStyle.SharpTriangle)
            {
                points = GetArrowShape(absolutePosition, orientation, shift, size, type, ref endPoint);

                endPoint = GetRelativePoint(endPoint);

                // Draw center line
                DrawLineRel(color, lineWidth, lineDashStyle, position, endPoint);

                // Draw arrow
                FillPolygon(brush, points);
            }
            // Arrow type is 'Lines'
            else if (type == AxisArrowStyle.Lines)
            {
                points = GetArrowShape(absolutePosition, orientation, shift, size, type, ref endPoint);

                points[0] = GetRelativePoint(points[0]);
                points[1] = GetRelativePoint(points[1]);
                points[2] = GetRelativePoint(points[2]);

                endPoint = GetRelativePoint(endPoint);

                // Draw arrow
                DrawLineRel(color, lineWidth, lineDashStyle, position, endPoint);
                DrawLineRel(color, lineWidth, lineDashStyle, points[0], points[2]);
                DrawLineRel(color, lineWidth, lineDashStyle, points[1], points[2]);
            }
        }

        /// <summary>
        /// This function calculates points for polygon, which represents
        /// shape of an arrow. There are four different orientations
        /// of arrow and three arrow types.
        /// </summary>
        /// <param name="position">Arrow position</param>
        /// <param name="orientation">Arrow orientation ( Left, Right, Top, Bottom )</param>
        /// <param name="shift">Distance from chart area to the arrow</param>
        /// <param name="size">Arrow size</param>
        /// <param name="type">Arrow style.</param>
        /// <param name="endPoint">End point of the axis and the beginning of arrow</param>
        /// <returns>Polygon points</returns>
        private SKPoint[] GetArrowShape(SKPoint position, ArrowOrientation orientation, double shift, double size, AxisArrowStyle type, ref SKPoint endPoint)
        {
            SKPoint[] points = new SKPoint[3]; // Polygon points
            double sharp; // Size for sharp triangle

            // Four different orientations for AxisArrowStyle
            switch (orientation)
            {
                // Top orientation
                case ArrowOrientation.Top:
                    // Get absolute size for arrow
                    // Arrow size has to have the same shape when width and height
                    // are changed. When the picture is resized, width of the chart
                    // picture is used only for arrow size.
                    size = GetAbsoluteSize(new SKSize((float)size, (float)size)).Width;
                    shift = GetAbsoluteSize(new SKSize((float)shift, (float)shift)).Height;

                    // Size for sharp and regular triangle
                    if (type == AxisArrowStyle.SharpTriangle)
                        sharp = size * 4;
                    else
                        sharp = size * 2;

                    points[0].X = position.X - (float)size;
                    points[0].Y = position.Y - (float)shift;
                    points[1].X = position.X + (float)size;
                    points[1].Y = position.Y - (float)shift;
                    points[2].X = position.X;
                    points[2].Y = position.Y - (float)shift - (float)sharp;
                    // End of the axis line
                    endPoint.X = position.X;
                    if (type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle)
                        endPoint.Y = points[1].Y;
                    else
                        endPoint.Y = points[2].Y;

                    break;
                // Bottom orientation
                case ArrowOrientation.Bottom:
                    // Get absolute size for arrow
                    // Arrow size has to have the same shape when width and height
                    // are changed. When the picture is resized, width of the chart
                    // picture is used only for arrow size.
                    size = GetAbsoluteSize(new SKSize((float)size, (float)size)).Width;
                    shift = GetAbsoluteSize(new SKSize((float)shift, (float)shift)).Height;

                    // Size for sharp and regular triangle
                    if (type == AxisArrowStyle.SharpTriangle)
                        sharp = size * 4;
                    else
                        sharp = size * 2;

                    points[0].X = position.X - (float)size;
                    points[0].Y = position.Y + (float)shift;
                    points[1].X = position.X + (float)size;
                    points[1].Y = position.Y + (float)shift;
                    points[2].X = position.X;
                    points[2].Y = position.Y + (float)shift + (float)sharp;
                    // End of the axis line
                    endPoint.X = position.X;
                    if (type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle)
                        endPoint.Y = points[1].Y;
                    else
                        endPoint.Y = points[2].Y;
                    break;
                // Left orientation
                case ArrowOrientation.Left:
                    // Get absolute size for arrow
                    size = GetAbsoluteSize(new SKSize((float)size, (float)size)).Width;
                    shift = GetAbsoluteSize(new SKSize((float)shift, (float)shift)).Width;

                    // Size for sharp and regular triangle
                    if (type == AxisArrowStyle.SharpTriangle)
                        sharp = size * 4;
                    else
                        sharp = size * 2;

                    points[0].Y = position.Y - (float)size;
                    points[0].X = position.X - (float)shift;
                    points[1].Y = position.Y + (float)size;
                    points[1].X = position.X - (float)shift;
                    points[2].Y = position.Y;
                    points[2].X = position.X - (float)shift - (float)sharp;
                    // End of the axis line
                    endPoint.Y = position.Y;
                    if (type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle)
                        endPoint.X = points[1].X;
                    else
                        endPoint.X = points[2].X;
                    break;
                // Right orientation
                case ArrowOrientation.Right:
                    // Get absolute size for arrow
                    size = GetAbsoluteSize(new SKSize((float)size, (float)size)).Width;
                    shift = GetAbsoluteSize(new SKSize((float)shift, (float)shift)).Width;

                    // Size for sharp and regular triangle
                    if (type == AxisArrowStyle.SharpTriangle)
                        sharp = size * 4;
                    else
                        sharp = size * 2;

                    points[0].Y = position.Y - (float)size;
                    points[0].X = position.X + (float)shift;
                    points[1].Y = position.Y + (float)size;
                    points[1].X = position.X + (float)shift;
                    points[2].Y = position.Y;
                    points[2].X = position.X + (float)shift + (float)sharp;
                    // End of the axis line
                    endPoint.Y = position.Y;
                    if (type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle)
                        endPoint.X = points[1].X;
                    else
                        endPoint.X = points[2].X;
                    break;
            }

            return points;
        }

        #endregion Arrow Methods

        #region Other methods and properties

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="common">Common elements class</param>
        internal ChartGraphics(CommonElements common)
        {
            // Set Common elements
            _common = common;
            base.Common = common;
            // Create a pen object
            _pen = new() { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

            // Create a brush object
            _solidBrush = new() { Color = SKColors.Black, Style = SKPaintStyle.Fill };
        }

        /// <summary>
        /// Chart Graphics Anti alias mode
        /// </summary>
        internal AntiAliasingStyles AntiAliasing
        {
            get
            {
                return _antiAliasing;
            }
            set
            {
                _antiAliasing = value;

                // Graphics mode not set
                if (Graphics == null)
                    return;

                // Convert Chart's anti alias enumeration to GDI+ SmoothingMode
                if ((_antiAliasing & AntiAliasingStyles.Graphics) == AntiAliasingStyles.Graphics)
                {
                    SmoothingMode = SmoothingMode.AntiAlias;
                }
                else
                {
                    SmoothingMode = SmoothingMode.None;
                }
            }
        }

        /// <summary>
        /// Gets reusable pen.
        /// </summary>
        internal SKPaint Pen
        {
            get { return _pen; }
        }

        /// <summary>
        /// This method takes a given axis value for a specified axis and returns the relative pixel value.
        /// </summary>
        /// <param name="chartAreaName">Chart area name.</param>
        /// <param name="axis">An AxisName enum value that identifies the relevant axis.</param>
        /// <param name="axisValue">The axis value that needs to be converted to a relative pixel value.</param>
        /// <returns>The converted axis value, in relative pixel coordinates.</returns>
        public double GetPositionFromAxis(string chartAreaName, AxisName axis, double axisValue)
        {
            if (axis == AxisName.X)
                return _common.ChartPicture.ChartAreas[chartAreaName].AxisX.GetLinearPosition(axisValue);

            if (axis == AxisName.X2)
                return _common.ChartPicture.ChartAreas[chartAreaName].AxisX2.GetLinearPosition(axisValue);

            if (axis == AxisName.Y)
                return _common.ChartPicture.ChartAreas[chartAreaName].AxisY.GetLinearPosition(axisValue);

            if (axis == AxisName.Y2)
                return _common.ChartPicture.ChartAreas[chartAreaName].AxisY2.GetLinearPosition(axisValue);

            return 0;
        }

        /// <summary>
        /// Helper function that retrieves bar drawing style.
        /// </summary>
        /// <param name="point">Data point to get the drawing style for.</param>
        /// <returns>Bar drawing style.</returns>
        internal static BarDrawingStyle GetBarDrawingStyle(DataPoint point)
        {
            // Get column drawing style
            BarDrawingStyle barDrawingStyle = BarDrawingStyle.Default;
            string styleName = point[CustomPropertyName.DrawingStyle];
            if (styleName != null)
            {
                if (string.Compare(styleName, "Default", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    barDrawingStyle = BarDrawingStyle.Default;
                }
                else if (string.Compare(styleName, "Cylinder", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    barDrawingStyle = BarDrawingStyle.Cylinder;
                }
                else if (string.Compare(styleName, "Emboss", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    barDrawingStyle = BarDrawingStyle.Emboss;
                }
                else if (string.Compare(styleName, "LightToDark", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    barDrawingStyle = BarDrawingStyle.LightToDark;
                }
                else if (string.Compare(styleName, "Wedge", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    barDrawingStyle = BarDrawingStyle.Wedge;
                }
                else
                {
                    throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(styleName, "DrawingStyle"));
                }
            }
            return barDrawingStyle;
        }

        /// <summary>
        /// Find rounding coordinates for a rectangle
        /// </summary>
        /// <param name="rect">Rectangle which has to be rounded</param>
        /// <returns>Rounded rectangle</returns>
        internal static SKRect Round(SKRect rect)
        {
            float left = (float)Math.Round(rect.Left);
            float right = (float)Math.Round(rect.Right);
            float top = (float)Math.Round(rect.Top);
            float bottom = (float)Math.Round(rect.Bottom);

            return new SKRect(left, top, right - left, bottom - top);
        }
        /// <summary>
        /// Sets the clipping region of this Graphics object
        /// to the rectangle specified by a SKRect structure.
        /// </summary>
        /// <param name="region">Region rectangle</param>
        internal void SetClip(SKRect region)
        {
            SetClipAbs(GetAbsoluteRectangle(region));
        }

        /// <summary>
        /// Set picture size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        internal void SetPictureSize(int width, int height)
        {
            _width = width;
            _height = height;
        }
        #endregion Other methods and properties

        #region Color manipulation methods

        /// <summary>
        /// Returns the gradient color from a gradient position.
        /// </summary>
        /// <param name="beginColor">The color from the gradient beginning</param>
        /// <param name="endColor">The color from the gradient end.</param>
        /// <param name="relativePosition">The relative position.</param>
        /// <returns>Result color.</returns>s
        static internal SKColor GetGradientColor(SKColor beginColor, SKColor endColor, double relativePosition)
        {
            // Check if position is valid
            if (relativePosition < 0 || relativePosition > 1 || double.IsNaN(relativePosition))
            {
                return beginColor;
            }

            // Extracts Begin color
            int nBRed = beginColor.Red;
            int nBGreen = beginColor.Green;
            int nBBlue = beginColor.Blue;

            // Extracts End color
            int nERed = endColor.Red;
            int nEGreen = endColor.Green;
            int nEBlue = endColor.Blue;

            // Gradient positions for Red, Green and Blue colors
            double dRRed = nBRed + (nERed - nBRed) * relativePosition;
            double dRGreen = nBGreen + (nEGreen - nBGreen) * relativePosition;
            double dRBlue = nBBlue + (nEBlue - nBBlue) * relativePosition;

            // Make sure colors are in range from 0 to 255
            if (dRRed > 255.0)
                dRRed = 255.0;
            if (dRRed < 0.0)
                dRRed = 0.0;
            if (dRGreen > 255.0)
                dRGreen = 255.0;
            if (dRGreen < 0.0)
                dRGreen = 0.0;
            if (dRBlue > 255.0)
                dRBlue = 255.0;
            if (dRBlue < 0.0)
                dRBlue = 0.0;

            // Return a gradient color position
            return new((byte)dRRed, (byte)dRGreen, (byte)dRBlue, beginColor.Alpha);
        }

        #endregion Color manipulation methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free up managed resources
                if (_pen != null)
                {
                    _pen.Dispose();
                    _pen = null;
                }
                if (_solidBrush != null)
                {
                    _solidBrush.Dispose();
                    _solidBrush = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}