// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	ChartRenderingEngine class provides a common interface 
//              to the graphics rendering and animation engines. 
//              Internally it uses SvgChartGraphics, FlashGraphics or 
//              GdiGraphics classes depending on the ActiveRenderingType 
//              property settings.
//              ValueA, PointA, RectangleA and ColorA classes are
//              used to store data about animated values like colors
//              position or rectangles. They store starting value/time, 
//              end value/time, repeat flags and other settings. These 
//              clases are used with animation engines.
//


using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using WebCharts.Services.Interfaces;
using WebCharts.Services.Models.DataManager;

namespace WebCharts.Services.Models.General
{
    #region Enumerations

    /// <summary>
    /// Specify Rendering AxisName
    /// </summary>
    internal enum RenderingType
    {
        /// <summary>
        /// GDI+ AxisName
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gdi")]
        Gdi,

        /// <summary>
        /// SVG AxisName
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Svg")]
        Svg,
    }

    #endregion // Enumerations

    /// <summary>
    /// The ChartGraphics class provides a common interface to the 
    /// graphics rendering.
    /// </summary>
    public partial class ChartGraphics
    {
        #region Fields

        // Current rendering type
        private RenderingType _activeRenderingType = RenderingType.Gdi;

        // GDI+ rendering engine
        private GdiGraphics _gdiGraphics = new GdiGraphics();

        // Document title used for SVG rendering
        //private string documentTitle = string.Empty;

        // True if text should be clipped
        internal bool IsTextClipped = false;

        #endregion // Fields

        #region Drawing Methods

        /// <summary>
        /// Draws a line connecting two SKPoint structures.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
        /// <param name="pt1">SKPoint structure that represents the first point to connect.</param>
        /// <param name="pt2">SKPoint structure that represents the second point to connect.</param>
        internal void DrawLine(
            SKPaint pen,
            SKPoint pt1,
            SKPoint pt2
            )
        {
            RenderingObject.DrawLine(pen, pt1, pt2);
        }

        /// <summary>
        /// Draws a line connecting the two points specified by coordinate pairs.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
        /// <param name="x1">x-coordinate of the first point.</param>
        /// <param name="y1">y-coordinate of the first point.</param>
        /// <param name="x2">x-coordinate of the second point.</param>
        /// <param name="y2">y-coordinate of the second point.</param>
        internal void DrawLine(
            SKPaint pen,
            float x1,
            float y1,
            float x2,
            float y2
            )
        {
            RenderingObject.DrawLine(pen, x1, y1, x2, y2);
        }

   

        /// <summary>
        /// Draws an ellipse defined by a bounding rectangle specified by 
        /// a pair of coordinates, a height, and a width.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
        internal void DrawEllipse(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height
            )
        {
            RenderingObject.DrawEllipse(pen, x, y, width, height);
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
        internal void DrawCurve(
            SKPaint pen,
            SKPoint[] points,
            int offset,
            int numberOfSegments,
            float tension
            )
        {
            ChartGraphics chartGraphics = this as ChartGraphics;
            if (chartGraphics == null || !chartGraphics.IsMetafile)
            {
                RenderingObject.DrawCurve(pen, points, offset, numberOfSegments, tension);
            }
            else
            {
                // Special handling required for the metafiles. We cannot pass large array of
                // points because they will be persisted inside EMF file and cause exponential 
                // increase in emf file size. Draw curve method uses additional 2, 3 or 4 points
                // depending on which segement is drawn.
                SKPoint[] pointsExact = null;
                if (offset == 0 && numberOfSegments == points.Length - 1)
                {
                    // In case the array contains the minimum required number of points
                    // to draw segments - just call the curve drawing method
                    RenderingObject.DrawCurve(pen, points, offset, numberOfSegments, tension);
                }
                else
                {
                    if (offset == 0 && numberOfSegments < points.Length - 1)
                    {
                        // Segment is at the beginning of the array with more points following
                        pointsExact = new SKPoint[numberOfSegments + 2];
                        for (int index = 0; index < numberOfSegments + 2; index++)
                        {
                            pointsExact[index] = points[index];
                        }
                    }
                    else if (offset > 0 && (offset + numberOfSegments) == points.Length - 1)
                    {
                        // Segment is at the end of the array with more points prior to it
                        pointsExact = new SKPoint[numberOfSegments + 2];
                        for (int index = 0; index < numberOfSegments + 2; index++)
                        {
                            pointsExact[index] = points[offset + index - 1];
                        }
                        offset = 1;
                    }
                    else if (offset > 0 && (offset + numberOfSegments) < points.Length - 1)
                    {
                        // Segment in the middle of the array with points prior and following it
                        pointsExact = new SKPoint[numberOfSegments + 3];
                        for (int index = 0; index < numberOfSegments + 3; index++)
                        {
                            pointsExact[index] = points[offset + index - 1];
                        }
                        offset = 1;
                    }

                    // Render the curve using minimum number of required points in the array 
                    RenderingObject.DrawCurve(pen, pointsExact, offset, numberOfSegments, tension);
                }
            }
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the rectangle.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width">Width of the rectangle to draw.</param>
        /// <param name="height">Height of the rectangle to draw.</param>
        internal void DrawRectangle(
            SKPaint pen,
            int x,
            int y,
            int width,
            int height
            )
        {
            RenderingObject.DrawRectangle(pen, x, y, width, height);
        }

        /// <summary>
        /// Draws a polygon defined by an array of SKPoint structures.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the polygon.</param>
        /// <param name="points">Array of SKPoint structures that represent the vertices of the polygon.</param>
        internal void DrawPolygon(
            SKPaint pen,
            SKPoint[] points
            )
        {
            RenderingObject.DrawPolygon(pen, points);
        }

        /// <summary>
        /// Draws the specified text string in the specified rectangle with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
        /// </summary>
        /// <param name="s">String to draw.</param>
        /// <param name="font">Font object that defines the text format of the string.</param>
        /// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
        /// <param name="layoutRectangle">SKRect structure that specifies the location of the drawn text.</param>
        internal void DrawString(
            string s,
            SKFont font,
            SKPaint brush,
            SKRect layoutRectangle
            )
        {
            RenderingObject.DrawString(s, font, brush, layoutRectangle);
        }

        /// <summary>
        /// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
        /// </summary>
        /// <param name="s">String to draw.</param>
        /// <param name="font">Font object that defines the text format of the string.</param>
        /// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
        /// <param name="point">SKPoint structure that specifies the upper-left corner of the drawn text.</param>
        /// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        internal void DrawString(
            string s,
            SKFont font,
            SKPaint brush,
            SKPoint point
            )
        {
            RenderingObject.DrawString(s, font, brush, point);
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
        /// <param name="paint"></param>
        internal void DrawImage(SKImage image, SKRect destRect, int srcX, int srcY, int srcWidth, int srcHeight, SKPaint paint)
        {
            RenderingObject.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, paint);
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
        internal void DrawImage(
            SKImage image,
            SKRect destRect,
            float srcX,
            float srcY,
            float srcWidth,
            float srcHeight,
            SKPaint paint
            )
        {
            RenderingObject.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, paint);
        }

        /// <summary>
        /// Draws a rectangle specified by a coordinate pair, a width, and a height.
        /// </summary>
        /// <param name="pen">A Pen object that determines the color, width, and style of the rectangle.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
        /// <param name="width">The width of the rectangle to draw.</param>
        /// <param name="height">The height of the rectangle to draw.</param>
        internal void DrawRectangle(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height
            )
        {
            RenderingObject.DrawRectangle(pen, x, y, width, height);
        }

        /// <summary>
        /// Draws a SKPath object.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the path.</param>
        /// <param name="path">SKPath object to draw.</param>
        internal void DrawPath(
            SKPaint pen,
            SKPath path
            )
        {
            // Check if path is empty
            if (path == null ||
                path.PointCount == 0)
            {
                return;
            }

            RenderingObject.DrawPath(pen, path);
        }

        /// <summary>
        /// Draws a pie shape defined by an ellipse specified by a coordinate pair, a width, and a height and two radial lines.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the pie shape.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
        /// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
        internal void DrawPie(
            SKPaint pen,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            RenderingObject.DrawPie(pen, x, y, width, height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Draws an ellipse defined by a bounding SKRect.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
        /// <param name="rect">SKRect structure that defines the boundaries of the ellipse.</param>
        internal void DrawEllipse(
            SKPaint pen,
            SKRect rect
            )
        {
            RenderingObject.DrawEllipse(pen, rect);
        }

        /// <summary>
        /// Draws a series of line segments that connect an array of SKPoint structures.
        /// </summary>
        /// <param name="pen">Pen object that determines the color, width, and style of the line segments.</param>
        /// <param name="points">Array of SKPoint structures that represent the points to connect.</param>
        internal void DrawLines(
            SKPaint pen,
            SKPoint[] points
            )
        {
            RenderingObject.DrawLines(pen, points);
        }

        #endregion // Drawing Methods

        #region Filling Methods

        /// <summary>
        /// Fills the interior of an ellipse defined by a bounding rectangle 
        /// specified by a SKRect structure.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="rect">SKRect structure that represents the bounding rectangle that defines the ellipse.</param>
        internal void FillEllipse(
            SKPaint brush,
            SKRect rect
            )
        {
            RenderingObject.FillEllipse(brush, rect);
        }

        /// <summary>
        /// Fills the interior of a SKPath object.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="path">SKPath object that represents the path to fill.</param>
        internal void FillPath(
            SKPaint brush,
            SKPath path
            )
        {
            // Check if path is empty
            if (path == null ||
                path.PointCount == 0)
            {
                return;
            }

            RenderingObject.FillPath(brush, path);
        }

        /// <summary>
        /// Fills the interior of a Region object.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="region">Region object that represents the area to fill.</param>
        internal void FillRegion(
            SKPaint brush,
            SKRegion region
            )
        {
            RenderingObject.FillRegion(brush, region);
        }

        /// <summary>
        /// Fills the interior of a rectangle specified by a SKRect structure.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="rect">SKRect structure that represents the rectangle to fill.</param>
        internal void FillRectangle(
            SKPaint brush,
            SKRect rect
            )
        {
            RenderingObject.FillRectangle(brush, rect);
        }

        /// <summary>
        /// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="x">x-coordinate of the upper-left corner of the rectangle to fill.</param>
        /// <param name="y">y-coordinate of the upper-left corner of the rectangle to fill.</param>
        /// <param name="width">Width of the rectangle to fill.</param>
        /// <param name="height">Height of the rectangle to fill.</param>
        internal void FillRectangle(
            SKPaint brush,
            float x,
            float y,
            float width,
            float height
            )
        {
            RenderingObject.FillRectangle(brush, x, y, width, height);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by an array of points specified by SKPoint structures .
        /// </summary>
        /// <param name="brush">Brush object that determines the characteristics of the fill.</param>
        /// <param name="points">Array of SKPoint structures that represent the vertices of the polygon to fill.</param>
        internal void FillPolygon(
            SKPaint brush,
            SKPoint[] points
            )
        {
            RenderingObject.FillPolygon(brush, points);
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
        internal void FillPie(
            SKPaint brush,
            float x,
            float y,
            float width,
            float height,
            float startAngle,
            float sweepAngle
            )
        {
            RenderingObject.FillPie(brush, x, y, width, height, startAngle, sweepAngle);
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
        /// <returns>This method returns a SKSize structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        internal SKSize MeasureString(
            string text,
            SKFont font,
            SKSize layoutArea
            )
        {
            return RenderingObject.MeasureString(text, font, layoutArea);
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified 
        /// Font object and formatted with the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object defines the text format of the string.</param>
        /// <returns>This method returns a SKSize structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        internal SKSize MeasureString(
            string text,
            SKFont font
            )
        {
            return RenderingObject.MeasureString(text, font);
        }

        ///// <summary>
        ///// Saves the current state of this Graphics object and identifies the saved state with a GraphicsState object.
        ///// </summary>
        ///// <returns>This method returns a GraphicsState object that represents the saved state of this Graphics object.</returns>
        //internal GraphicsState Save()
        //{
        //	return RenderingObject.Save();
        //}

        ///// <summary>
        ///// Restores the state of this Graphics object to the state represented by a GraphicsState object.
        ///// </summary>
        ///// <param name="gstate">GraphicsState object that represents the state to which to restore this Graphics object.</param>
        //internal void Restore(
        //	GraphicsState gstate
        //	)
        //{
        //	RenderingObject.Restore( gstate );
        //}

        /// <summary>
        /// Resets the clip region of this Graphics object to an infinite region.
        /// </summary>
        internal void ResetClip()
        {
            RenderingObject.ResetClip();
        }

        /// <summary>
        /// Sets the clipping region of this Graphics object to the rectangle specified by a SKRect structure.
        /// </summary>
        /// <param name="rect">SKRect structure that represents the new clip region.</param>
        internal void SetClipAbs(SKRect rect)
        {
            RenderingObject.SetClip(rect);
        }

        /// <summary>
        /// Prepends the specified translation to the transformation matrix of this Graphics object.
        /// </summary>
        /// <param name="dx">x component of the translation.</param>
        /// <param name="dy">y component of the translation.</param>
        internal void TranslateTransform(
            float dx,
            float dy
            )
        {
            RenderingObject.TranslateTransform(dx, dy);
        }

        #endregion // Other Methods

        #region Properties

        /// <summary>
        /// Gets current rendering object.
        /// </summary>
        internal IChartRenderingEngine RenderingObject
        {
            get
            {
                return _gdiGraphics;
            }
        }

        /// <summary>
        /// Gets the active rendering type.
        /// </summary>
        internal RenderingType ActiveRenderingType
        {
            get
            {
                return _activeRenderingType;
            }
        }

        ///// <summary>
        ///// Gets or sets the rendering mode for text associated with this Graphics object.
        ///// </summary>
        //internal TextRenderingHint TextRenderingHint 
        //{
        //	get
        //	{
        //		return RenderingObject.TextRenderingHint;
        //	}
        //	set
        //	{
        //		RenderingObject.TextRenderingHint = value;
        //	}
        //}

        /// <summary>
        /// Gets or sets the world transformation for this Graphics object.
        /// </summary>
        internal SKMatrix Transform
        {
            get
            {
                return RenderingObject.Transform;
            }
            set
            {
                RenderingObject.Transform = value;
            }
        }

        ///// <summary>
        ///// Gets or sets the rendering quality for this Graphics object.
        ///// </summary>
        //internal SmoothingMode SmoothingMode 
        //{
        //	get
        //	{
        //		return RenderingObject.SmoothingMode;
        //	}
        //	set
        //	{
        //		RenderingObject.SmoothingMode = value;
        //	}
        //}

        /// <summary>
        /// Gets or sets a Region object that limits the drawing region of this Graphics object.
        /// </summary>
        internal SKRegion Clip
        {
            get
            {
                return RenderingObject.Clip;
            }
            set
            {
                RenderingObject.Clip = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the clipping region of this Graphics object is empty.
        /// </summary>
        internal bool IsClipEmpty
        {
            get
            {
                return RenderingObject.IsClipEmpty;
            }
        }

        /// <summary>
        /// Gets or sets the reference to the Graphics object.
        /// </summary>
        public SKCanvas Graphics
        {
            get
            {
                return RenderingObject.Graphics;
            }
            set
            {
                RenderingObject.Graphics = value;
            }
        }

        #endregion // Properties
    }
}
