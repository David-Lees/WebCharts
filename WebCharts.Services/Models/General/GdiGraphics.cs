// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	GdiGraphics class is chart GDI+ rendering engine. It 
//              implements IChartRenderingEngine interface by mapping 
//              its methods to the drawing methods of GDI+. This 
//              rendering engine do not support animation.
//


using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using WebCharts.Services.Interfaces;

namespace WebCharts.Services.Models.General
{
    /// <summary>
    /// GdiGraphics class is chart GDI+ rendering engine.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gdi")]
    internal class GdiGraphics : IChartRenderingEngine
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public GdiGraphics()
        {
        }

        #endregion // Constructor

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
            _graphics.DrawLine(pt1, pt2, pen);
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
            _graphics.DrawLine(x1, y1, x2, y2, pen);
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
            int srcHeight
            )
        {
            _graphics.DrawImage(image, new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight), destRect);
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
            _graphics.DrawEllipse(pen, x, y, width, height);
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
            _graphics.DrawCurve(pen, points, offset, numberOfSegments, tension);
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
            _graphics.DrawRectangle(pen, x, y, width, height);
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
            _graphics.DrawPolygon(pen, points);
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
            _graphics.DrawString(s, font, brush, layoutRectangle);
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
            _graphics.DrawString(s, font, brush, point);
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
                SKCanvas image,
                Rectangle destRect,
                float srcX,
                float srcY,
                float srcWidth,
                float srcHeight
                )
        {
            _graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight);
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
            _graphics.DrawRectangle(pen, x, y, width, height);
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
            _graphics.DrawPath(pen, path);
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
            _graphics.DrawPie(pen, x, y, width, height, startAngle, sweepAngle);
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
            _graphics.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Draws the specified Image object at the specified location and with the specified size.
        /// </summary>
        /// <param name="image">Image object to draw.</param>
        /// <param name="rect">SKRect structure that specifies the location and size of the drawn image.</param>
        public void DrawImage(
            SKCanvas image,
            SKRect rect
            )
        {
            _graphics.DrawImage(image, rect);
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
            _graphics.DrawEllipse(pen, rect);
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
            _graphics.DrawLines(pen, points);
        }

        #endregion // Drawing Methods

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
            _graphics.FillEllipse(brush, rect);
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
            _graphics.FillPath(brush, path);
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
            _graphics.FillRegion(brush, region);
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
            _graphics.FillRectangle(brush, rect);
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
            _graphics.FillRectangle(brush, x, y, width, height);
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
            _graphics.FillPolygon(brush, points);
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
            _graphics.FillPie(brush, x, y, width, height, startAngle, sweepAngle);
        }

        #endregion // Filling Methods

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
            return _graphics.MeasureString(text, font, layoutArea);
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
            return _graphics.MeasureString(text, font);
        }

        /// <summary>
        /// Resets the clip region of this Graphics object to an infinite region.
        /// </summary>
        public void ResetClip()
        {
            _graphics.ResetClip();
        }

        /// <summary>
        /// Sets the clipping region of this Graphics object to the rectangle specified by a SKRect structure.
        /// </summary>
        /// <param name="rect">SKRect structure that represents the new clip region.</param>
        public void SetClip(
            SKRect rect
            )
        {
            _graphics.SetClip(rect);
        }

        /// <summary>
        /// Sets the clipping region of this Graphics object to the result of the 
        /// specified operation combining the current clip region and the 
        /// specified SKPath object.
        /// </summary>
        /// <param name="path">SKPath object to combine.</param>
        /// <param name="combineMode">Member of the CombineMode enumeration that specifies the combining operation to use.</param>
        public void SetClip(
            SKPath path,
            CombineMode combineMode
            )
        {
            _graphics.SetClip(path, combineMode);
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
            _graphics.TranslateTransform(dx, dy);
        }

        #endregion // Other Methods

        #region Properties

        /// <summary>
        /// Gets or sets the world transformation for this Graphics object.
        /// </summary>
        public SKMatrix Transform
        {
            get
            {
                return _graphics.Transform;
            }
            set
            {
                _graphics.Transform = value;
            }
        }

  

        /// <summary>
        /// Gets or sets a Region object that limits the drawing region of this Graphics object.
        /// </summary>
        public SKRegion Clip
        {
            get
            {
                return _graphics.Clip;
            }
            set
            {
                _graphics.Clip = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the clipping region of this Graphics object is empty.
        /// </summary>
        public bool IsClipEmpty
        {
            get
            {
                return _graphics.IsClipEmpty;
            }
        }

        /// <summary>
        /// Reference to the Graphics object
        /// </summary>
        public SKCanvas Graphics
        {
            get
            {
                return _graphics;
            }
            set
            {
                _graphics = value;
            }
        }

        #endregion // Properties

        #region Fields

        /// <summary>
        /// Graphics object
        /// </summary>
        SKCanvas _graphics = null;

        #endregion // Fields
    }
}
