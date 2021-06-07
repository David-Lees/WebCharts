// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	SkiaGraphics class is chart SkiaSharp rendering engine. It
//              implements IChartRenderingEngine interface by mapping
//              its methods to the drawing methods of SkiaSharp. This
//              rendering engine do not support animation.
//

using SkiaSharp;

namespace WebCharts.Services
{
    /// <summary>
    /// GdiGraphics class is chart GDI+ rendering engine.
    /// </summary>
    internal class SkiaGraphics : IChartRenderingEngine
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public SkiaGraphics()
        {
        }

        #endregion Constructors

        #region Drawing Methods

        /// <summary>
        /// Draws a line connecting two SKPoint structures.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
        /// <param name="pt1">SKPoint structure that represents the first point to connect.</param>
        /// <param name="pt2">SKPoint structure that represents the second point to connect.</param>
        public void DrawLine(
            SKPaint pen,
            SKPoint pt1,
            SKPoint pt2
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawLine(pt1, pt2, pen);
        }

        /// <summary>
        /// Draws a line connecting the two points specified by coordinate pairs.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
        /// <param name="x1">x-coordinate of the first point.</param>
        /// <param name="y1">y-coordinate of the first point.</param>
        /// <param name="x2">x-coordinate of the second point.</param>
        /// <param name="y2">y-coordinate of the second point.</param>
        public void DrawLine(
            SKPaint pen,
            float x1,
            float y1,
            float x2,
            float y2
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawLine(x1, y1, x2, y2, pen);
        }

        /// <summary>
        /// Draws a cardinal spline through a specified array of SKPoint structures
        /// using a specified tension. The drawing begins offset from
        /// the beginning of the array.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and height of the curve.</param>
        /// <param name="points">Array of SKPoint structures that define the spline.</param>
        /// <param name="offset">Offset from the first element in the array of the points parameter to the starting point in the curve.</param>
        /// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
        /// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve.</param>
        public void DrawCurve(
            SKPaint pen,
            SKPoint[] points,
            int offset,
            int numberOfSegments,
            float tension
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawPath(SkiaSharpExtensions.CreateSpline(points), pen);
        }

        /// <summary>
        /// Draws a polygon defined by an array of SKPoint structures.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the polygon.</param>
        /// <param name="points">Array of SKPoint structures that represent the vertices of the polygon.</param>
        public void DrawPolygon(
            SKPaint pen,
            SKPoint[] points
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawPoints(SKPointMode.Polygon, points, pen);
        }

        /// <summary>
        /// Draws the specified text string in the specified rectangle with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
        /// </summary>
        /// <param name="s">String to draw.</param>
        /// <param name="font">Font object that defines the text format of the string.</param>
        /// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
        /// <param name="layoutRectangle">SKRect structure that specifies the location of the drawn text.</param>
        /// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        public void DrawString(
            string s,
            SKFont font,
            SKPaint brush,
            SKRect layoutRectangle
            )
        {
            brush.IsAntialias = true;
            brush.Style = SKPaintStyle.StrokeAndFill;
            font.Hinting = SKFontHinting.Normal;            

            // Not sure why -1 is needed, but lines up better with it
            var p = new SKPoint(layoutRectangle.MidX, layoutRectangle.MidY - 1);

            var size = MeasureString(s, font);

            p.X -= size.Width / 2.0f;
            p.Y += size.Height / 2.0f;

            Graphics.DrawText(s, p.X, p.Y, font, brush);
        }

        /// <summary>
        /// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
        /// </summary>
        /// <param name="s">String to draw.</param>
        /// <param name="font">Font object that defines the text format of the string.</param>
        /// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
        /// <param name="point">SKPoint structure that specifies the upper-left corner of the drawn text.</param>
        /// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        public void DrawString(
            string s,
            SKFont font,
            SKPaint brush,
            SKPoint point)
        {
            point = new SKPoint(1, 1);
            brush.IsAntialias = true;
            brush.Style = SKPaintStyle.StrokeAndFill;
            font.Hinting = SKFontHinting.Normal;
            Graphics.DrawRect(new SKRect(point.X, point.Y, point.X + 10, point.Y + 10), new SKPaint() { Style = SKPaintStyle.Stroke, Color = SKColors.Red, StrokeWidth = 1 });
            Graphics.DrawText(s, point.X, point.Y, font, brush);
        }

        /// <summary>
        /// Draws the specified portion of the specified Image object at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image object to draw.</param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle.</param>
        /// <param name="srcX">x-coordinate of the upper-left corner of the portion of the source image to draw.</param>
        /// <param name="srcY">y-coordinate of the upper-left corner of the portion of the source image to draw.</param>
        /// <param name="srcWidth">Width of the portion of the source image to draw.</param>
        /// <param name="srcHeight">Height of the portion of the source image to draw.</param>
        public void DrawImage(
                SKImage image,
                SKRect destRect,
                float srcX,
                float srcY,
                float srcWidth,
                float srcHeight,
                SKPaint paint
                )
        { 
            Graphics.DrawImage(image, new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight), destRect, paint);
        }

        /// <summary>
        /// Draws the specified portion of the specified Image object at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image object to draw.</param>
        /// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle.</param>
        /// <param name="srcX">x-coordinate of the upper-left corner of the portion of the source image to draw.</param>
        /// <param name="srcY">y-coordinate of the upper-left corner of the portion of the source image to draw.</param>
        /// <param name="srcWidth">Width of the portion of the source image to draw.</param>
        /// <param name="srcHeight">Height of the portion of the source image to draw.</param>
        /// <param name="srcUnit">Member of the GraphicsUnit enumeration that specifies the units of measure used to determine the source rectangle.</param>
        /// <param name="imageAttr">ImageAttributes object that specifies recoloring and gamma information for the image object.</param>
        public void DrawImage(
            SKImage image,
            SKRect destRect,
            int srcX,
            int srcY,
            int srcWidth,
            int srcHeight,
            SKPaint paint
            )
        {
            Graphics.DrawImage(image, new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight), destRect, paint);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair: a width, and a height.
        /// </summary>
        /// <param name="pen">A Pen object that determines the color, width, and style of the rectangle.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width">The width of the rectangle to draw.</param>
        /// <param name="height">The height of the rectangle to draw.</param>
        public void DrawRectangle(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height
            )
        {
            Graphics.DrawRect(x, y, width, height, pen);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair: a width, and a height.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the rectangle.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width">Width of the rectangle to draw.</param>
        /// <param name="height">Height of the rectangle to draw.</param>
        public void DrawRectangle(
            SKPaint pen,
            int x,
            int y,
            int width,
            int height
            )
        {
            Graphics.DrawRect(x, y, width, height, pen);
        }

        /// <summary>
        /// Draws a SKPath object.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the path.</param>
        /// <param name="path">SKPath object to draw.</param>
        public void DrawPath(
            SKPaint pen,
            SKPath path
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawPath(path, pen);
        }

        /// <summary>
        /// Draws a pie shape defined by an ellipse specified by a coordinate pair: a width, a height and two radial lines.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the pie shape.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        public void DrawPie(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            pen.IsAntialias = true;
            DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Draws an arc representing a portion of an ellipse specified by a pair of coordinates: a width, and a height.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the arc.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
        /// <param name="width">Width of the rectangle that defines the ellipse.</param>
        /// <param name="height">Height of the rectangle that defines the ellipse.</param>
        /// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc.</param>
        /// <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to ending point of the arc.</param>
        public void DrawArc(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawArc(new SKRect(x, y, x + width, y + height), startAngle, sweepAngle, true, pen);
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding SKRect.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
        /// <param name="rect">SKRect structure that defines the boundaries of the ellipse.</param>
        public void DrawEllipse(
            SKPaint pen,
            SKRect rect
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawOval(rect, pen);
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle specified by
        /// a pair of coordinates: a height, and a width.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
        public void DrawEllipse(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawOval(new SKPoint(x, y), new SKSize(width, height), pen);
        }

        /// <summary>
        /// Draws a series of line segments that connect an array of SKPoint structures.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the line segments.</param>
        /// <param name="points">Array of SKPoint structures that represent the points to connect.</param>
        public void DrawLines(
            SKPaint pen,
            SKPoint[] points
            )
        {
            pen.IsAntialias = true;
            Graphics.DrawPoints(SKPointMode.Lines, points, pen);
        }

        #endregion Drawing Methods

        #region Filling Methods

        /// <summary>
        /// Fills the interior of an ellipse defined by a bounding rectangle
        /// specified by a SKRect structure.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="rect">SKRect structure that represents the bounding rectangle that defines the ellipse.</param>
        public void FillEllipse(
            SKPaint brush,
            SKRect rect
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawOval(rect, brush);
        }

        /// <summary>
        /// Fills the interior of a SKPath object.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="path">SKPath object that represents the path to fill.</param>
        public void FillPath(
            SKPaint brush,
            SKPath path
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawPath(path, brush);
        }

        /// <summary>
        /// Fills the interior of a Region object.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="region">Region object that represents the area to fill.</param>
        public void FillRegion(
            SKPaint brush,
            SKRegion region
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawRegion(region, brush);
        }

        /// <summary>
        /// Fills the interior of a rectangle specified by a SKRect structure.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="rect">SKRect structure that represents the rectangle to fill.</param>
        public void FillRectangle(
            SKPaint brush,
            SKRect rect
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawRect(rect, brush);
        }

        /// <summary>
        /// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the rectangle to fill.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the rectangle to fill.</param>
        /// <param name="width">Width of the rectangle to fill.</param>
        /// <param name="height">Height of the rectangle to fill.</param>
        public void FillRectangle(
            SKPaint brush,
            float x,
            float y,
            float width,
            float height
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawRect(x, y, width, height, brush);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by SKPoint structures .
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="points">Array of SKPoint structures that represent the vertices of the polygon to fill.</param>
        public void FillPolygon(
            SKPaint brush,
            SKPoint[] points
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawPoints(SKPointMode.Polygon, points, brush);
        }

        /// <summary>
        /// Fills the interior of a pie section defined by an ellipse
        /// specified by a pair of coordinates, a width, and a height
        /// and two radial lines.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
        /// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
        /// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the pie section.</param>
        /// <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to the second side of the pie section.</param>
        public void FillPie(
            SKPaint brush,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            brush.IsAntialias = true;
            Graphics.DrawArc(new SKRect(x, y, x + width, y + height), startAngle, sweepAngle, true, brush);
        }

        #endregion Filling Methods

        #region Other Methods

        /// <summary>
        /// Measures the specified string when drawn with the specified
        /// Font object and formatted with the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object defines the text format of the string.</param>
        /// <param name="layoutArea">SKSize structure that specifies the maximum layout area for the text.</param>
        /// <param name="stringFormat">StringFormat object that represents formatting information, such as line spacing, for the string.</param>
        /// <returns>This method returns a SKSize structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        public SKSize MeasureString(
            string text,
            SKFont font,
            SKSize layoutArea)
        {
            var p = new SKPaint() { Typeface = font.Typeface, TextSize = font.Size };
            var width = p.MeasureText(text);
            var height = p.TextSize;
            return new SKSize(width > layoutArea.Width ? layoutArea.Width : width, height > layoutArea.Height ? layoutArea.Height : height);
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified
        /// Font object and formatted with the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object defines the text format of the string.</param>
        /// <returns>This method returns a SKSize structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        public SKSize MeasureString(
            string text,
            SKFont font
            )
        {
            var p = new SKPaint() { Typeface = font.Typeface, TextSize = font.Size };
            var width = p.MeasureText(text);
            var height = p.TextSize;
            return new SKSize(width, height);
        }

        /// <summary>
        /// Resets the clip region of this Graphics object to an infinite region.
        /// </summary>
        public void ResetClip()
        {
            Graphics.Restore();
        }

        /// <summary>
        /// Sets the clipping region of this Graphics object to the rectangle specified by a SKRect structure.
        /// </summary>
        /// <param name="rect">SKRect structure that represents the new clip region.</param>
        public void SetClip(SKRect rect)
        {
            Graphics.Save();
            Graphics.ClipRect(rect);
        }

        /// <summary>
        /// Prepends the specified translation to the transformation matrix of this Graphics object.
        /// </summary>
        /// <param name="dx">x component of the translation.</param>
        /// <param name="dy">y component of the translation.</param>
        public void TranslateTransform(
            float dx,
            float dy
            )
        {
            Graphics.Translate(dx, dy);
        }

        #endregion Other Methods

        #region Properties

        /// <summary>
        /// Gets or sets the world transformation for this Graphics object.
        /// </summary>
        public SKMatrix Transform
        {
            get
            {
                return Graphics.TotalMatrix;
            }
            set
            {
                Graphics.SetMatrix(value);
            }
        }

        /// <summary>
        /// Gets or sets a Region object that limits the drawing region of this Graphics object.
        /// </summary>
        public SKRegion Clip
        {
            get
            {
                Graphics.GetLocalClipBounds(out var bounds);
                return new SKRegion(new SKRectI((int)bounds.Left, (int)bounds.Top, (int)bounds.Right, (int)bounds.Bottom));
            }
            set
            {
                Graphics.ClipRegion(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the clipping region of this Graphics object is empty.
        /// </summary>
        public bool IsClipEmpty
        {
            get
            {
                return Graphics.IsClipEmpty;
            }
        }

        /// <summary>
        /// Reference to the Graphics object
        /// </summary>
        public SKCanvas Graphics { get; set; } = null;

        #endregion Properties

        #region Fields

        /// <summary>
        /// Graphics object
        /// </summary>

        #endregion Fields
    }
}