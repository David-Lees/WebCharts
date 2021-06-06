// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Callout annotation classes.
//

using SkiaSharp;
using System;

namespace WebCharts.Services
{
    #region Enumerations

    /// <summary>
    /// Annotation callout style.
    /// <seealso cref="CalloutAnnotation.CalloutStyle"/>
    /// </summary>
    [
    SRDescription("DescriptionAttributeCalloutStyle_CalloutStyle"),
    ]
    public enum CalloutStyle
    {
        /// <summary>
        /// Callout text is underlined and a line is pointing to the anchor point.
        /// </summary>
        SimpleLine,

        /// <summary>
        /// Border is drawn around text and a line is pointing to the anchor point.
        /// </summary>
        Borderline,

        /// <summary>
        /// Callout text is inside the cloud and smaller clouds are pointing to the anchor point.
        /// </summary>
        Cloud,

        /// <summary>
        /// Rectangle is drawn around the callout text, which is connected with the anchor point.
        /// </summary>
        Rectangle,

        /// <summary>
        /// Rounded rectangle is drawn around the callout text, which is connected with the anchor point.
        /// </summary>
        RoundedRectangle,

        /// <summary>
        /// Ellipse is drawn around the callout text, which is connected with the anchor point.
        /// </summary>
        Ellipse,

        /// <summary>
        /// Perspective rectangle is drawn around the callout text, which is connected with the anchor point.
        /// </summary>
        Perspective,
    }

    #endregion Enumerations

    /// <summary>
    /// <b>CalloutAnnotation</b> is a class class that represents a callout annotation.
    /// </summary>
    /// <remarks>
    /// Callout annotation is the only annotation that draws a connection between the
    /// annotation position and anchor point. It can display text and automatically
    /// calculate the required size. Different <see cref="CalloutStyle"/> are supported.
    /// </remarks>
    [
        SRDescription("DescriptionAttributeCalloutAnnotation_CalloutAnnotation"),
    ]
    public class CalloutAnnotation : TextAnnotation
    {
        #region Fields

        // Callout anchor type
        private LineAnchorCapStyle _calloutAnchorCap = LineAnchorCapStyle.Arrow;

        // Callout drawing style
        private CalloutStyle _calloutStyle = CalloutStyle.Rectangle;

        // Cloud shape path
        private static SKPath _cloudPath = null;

        // Cloud shape outline path
        private static SKPath _cloudOutlinePath = null;

        // Cloud shape boundary rectangle
        private static SKRect _cloudBounds = SKRect.Empty;

        #endregion Fields

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public CalloutAnnotation()
            : base()
        {
            // Changing default values of properties
            anchorOffsetX = 3.0;
            anchorOffsetY = 3.0;
            anchorAlignment = ContentAlignment.BottomLeft;
        }

        #endregion Construction and Initialization

        #region Properties

        #region	Callout properties

        /// <summary>
        /// Gets or sets the annotation callout style.
        /// </summary>
        /// <value>
        /// <see cref="CalloutStyle"/> of the annotation.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCalloutAnnotation_CalloutStyle"),
        ]
        virtual public CalloutStyle CalloutStyle
        {
            get
            {
                return _calloutStyle;
            }
            set
            {
                _calloutStyle = value;
                ResetCurrentRelativePosition();

                // Reset content size to empty
                contentSize = SKSize.Empty;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the anchor cap style of a callout line.
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value used as the anchor cap of a callout line.
        /// </value>
        /// <remarks>
        /// This property sets the anchor cap of the line connecting an annotation to
        /// its anchor point. It only applies when SimpleLine or BorderLine
        /// are used.
        /// </remarks>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCalloutAnnotation_CalloutAnchorCap"),
        ]
        virtual public LineAnchorCapStyle CalloutAnchorCap
        {
            get
            {
                return _calloutAnchorCap;
            }
            set
            {
                _calloutAnchorCap = value;
                Invalidate();
            }
        }

        #endregion Properties

        #region Applicable Annotation Appearance Attributes (set as Browsable)

        /// <summary>
        /// Gets or sets the color of an annotation line.
        /// <seealso cref="LineWidth"/>
        /// <seealso cref="LineDashStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used to draw an annotation line.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineColor"),
        ]
        override public SKColor LineColor
        {
            get
            {
                return base.LineColor;
            }
            set
            {
                base.LineColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of an annotation line.
        /// <seealso cref="LineColor"/>
        /// <seealso cref="LineDashStyle"/>
        /// </summary>
        /// <value>
        /// An integer value defining the width of an annotation line in pixels.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineWidth"),
        ]
        override public int LineWidth
        {
            get
            {
                return base.LineWidth;
            }
            set
            {
                base.LineWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the style of an annotation line.
        /// <seealso cref="LineWidth"/>
        /// <seealso cref="LineColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartDashStyle"/> value used to draw an annotation line.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineDashStyle"),
        ]
        override public ChartDashStyle LineDashStyle
        {
            get
            {
                return base.LineDashStyle;
            }
            set
            {
                base.LineDashStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the background of an annotation.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        override public SKColor BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background hatch style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackHatchStyle"),
        ]
        override public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return base.BackHatchStyle;
            }
            set
            {
                base.BackHatchStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the background gradient style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle"),
        ]
        override public GradientStyle BackGradientStyle
        {
            get
            {
                return base.BackGradientStyle;
            }
            set
            {
                base.BackGradientStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the secondary background color of an annotation.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of an annotation background with
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        ]
        override public SKColor BackSecondaryColor
        {
            get
            {
                return base.BackSecondaryColor;
            }
            set
            {
                base.BackSecondaryColor = value;
            }
        }

        #endregion Applicable Annotation Appearance Attributes (set as Browsable)

        #region Anchor

        /// <summary>
        /// Gets or sets the x-coordinate offset between the positions of an annotation and its anchor point.
        /// <seealso cref="AnchorOffsetY"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="Annotation.AnchorX"/>
        /// <seealso cref="AnchorAlignment"/>
        /// </summary>
        /// <value>
        /// A double value that represents the x-coordinate offset between the positions of an annotation and its anchor point.
        /// </value>
        /// <remarks>
        /// The annotation must be anchored using the <see cref="Annotation.AnchorDataPoint"/> or
        /// <see cref="Annotation.AnchorX"/> properties, and its <see cref="Annotation.X"/> property must be set
        /// to <b>Double.NaN</b>.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAnchor"),
        SRDescription("DescriptionAttributeCalloutAnnotation_AnchorOffsetX"),
        ]
        override public double AnchorOffsetX
        {
            get
            {
                return base.AnchorOffsetX;
            }
            set
            {
                base.AnchorOffsetX = value;
            }
        }

        /// <summary>
        /// Gets or sets the y-coordinate offset between the positions of an annotation and its anchor point.
        /// <seealso cref="Annotation.AnchorOffsetX"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="Annotation.AnchorY"/>
        /// <seealso cref="Annotation.AnchorAlignment"/>
        /// </summary>
        /// <value>
        /// A double value that represents the y-coordinate offset between the positions of an annotation and its anchor point.
        /// </value>
        /// <remarks>
        /// Annotation must be anchored using <see cref="Annotation.AnchorDataPoint"/> or
        /// <see cref="Annotation.AnchorY"/> properties and its <see cref="Annotation.Y"/> property must be set
        /// to <b>Double.NaN</b>.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAnchor"),
        SRDescription("DescriptionAttributeCalloutAnnotation_AnchorOffsetY"),
        ]
        override public double AnchorOffsetY
        {
            get
            {
                return base.AnchorOffsetY;
            }
            set
            {
                base.AnchorOffsetY = value;
            }
        }

        /// <summary>
        /// Gets or sets an annotation position's alignment to the anchor point.
        /// <seealso cref="Annotation.AnchorX"/>
        /// <seealso cref="Annotation.AnchorY"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="AnchorOffsetX"/>
        /// <seealso cref="AnchorOffsetY"/>
        /// </summary>
        /// <value>
        /// A <see cref="ContentAlignment"/> value that represents the annotation's alignment to
        /// the anchor point.
        /// </value>
        /// <remarks>
        /// The annotation must be anchored using either <see cref="Annotation.AnchorDataPoint"/>, or the <see cref="Annotation.AnchorX"/>
        /// and <see cref="Annotation.AnchorY"/> properties. Its <see cref="Annotation.X"/> and <see cref="Annotation.Y"/>
        /// properties must be set to <b>Double.NaN</b>.
        /// </remarks>
		[
        SRCategory("CategoryAttributeAnchor"),
        SRDescription("DescriptionAttributeAnchorAlignment"),
        ]
        override public ContentAlignment AnchorAlignment
        {
            get
            {
                return base.AnchorAlignment;
            }
            set
            {
                base.AnchorAlignment = value;
            }
        }

        #endregion Anchor

        #region Other

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type
        /// (e.g. Line, Rectangle, Ellipse).
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAnnotationType"),
        ]
        public override string AnnotationType
        {
            get
            {
                return "Callout";
            }
        }

        /// <summary>
        /// Gets or sets annotation selection points style.
        /// </summary>
        /// <value>
        /// A <see cref="SelectionPointsStyle"/> value that represents annotation
        /// selection style.
        /// </value>
        /// <remarks>
        /// This property is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeSelectionPointsStyle"),
        ]
        override internal SelectionPointsStyle SelectionPointsStyle
        {
            get
            {
                return SelectionPointsStyle.Rectangle;
            }
        }

        #endregion Other

        #endregion

        #region Methods

        #region Text Spacing

        /// <summary>
        /// Gets text spacing on four different sides in relative coordinates.
        /// </summary>
        /// <param name="annotationRelative">Indicates that spacing is in annotation relative coordinates.</param>
        /// <returns>Rectangle with text spacing values.</returns>
        internal override SKRect GetTextSpacing(out bool annotationRelative)
        {
            SKRect spacing = base.GetTextSpacing(out annotationRelative);
            if (_calloutStyle == CalloutStyle.Cloud ||
                _calloutStyle == CalloutStyle.Ellipse)
            {
                spacing = new SKRect(4f, 4f, 4f, 4f);
                annotationRelative = true;
            }
            else if (_calloutStyle == CalloutStyle.RoundedRectangle)
            {
                spacing = new SKRect(1f, 1f, 1f, 1f);
                annotationRelative = true;
            }

            return spacing;
        }

        #endregion // Text Spacing

        #region Painting

        /// <summary>
        /// Paints annotation object on specified graphics.
        /// </summary>
        /// <param name="graphics">
        /// A <see cref="ChartGraphics"/> used to paint annotation object.
        /// </param>
        /// <param name="chart">
        /// Reference to the <see cref="ChartService"/> control.
        /// </param>
        override internal void Paint(ChartService chart, ChartGraphics graphics)
        {
            // Get annotation position in relative coordinates
            GetRelativePosition(out SKPoint firstPoint, out SKSize size, out SKPoint anchorPoint);
            SKPoint secondPoint = new(firstPoint.X + size.Width, firstPoint.Y + size.Height);

            // Create selection rectangle
            SKRect selectionRect = new(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Adjust negative rectangle width and height
            SKRect rectanglePosition = selectionRect;
            if (rectanglePosition.Width < 0)
            {
                var w = rectanglePosition.Width;
                rectanglePosition.Left = rectanglePosition.Right;
                rectanglePosition.Right -= w;
            }
            if (rectanglePosition.Height < 0)
            {
                var h = rectanglePosition.Height;
                rectanglePosition.Top = rectanglePosition.Bottom;
                rectanglePosition.Bottom -= h;
            }

            // Check if position is valid
            if (float.IsNaN(rectanglePosition.Left) ||
                float.IsNaN(rectanglePosition.Top) ||
                float.IsNaN(rectanglePosition.Right) ||
                float.IsNaN(rectanglePosition.Bottom))
            {
                return;
            }

            // Paint different style of callouts
            SKPath hotRegionPathAbs = null;
            if (Common.ProcessModePaint)
            {
                switch (_calloutStyle)
                {
                    case (CalloutStyle.SimpleLine):
                        hotRegionPathAbs = DrawRectangleLineCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint,
                            false);
                        break;

                    case (CalloutStyle.Borderline):
                        hotRegionPathAbs = DrawRectangleLineCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint,
                            true);
                        break;

                    case (CalloutStyle.Perspective):
                        hotRegionPathAbs = DrawPerspectiveCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint);
                        break;

                    case (CalloutStyle.Cloud):
                        hotRegionPathAbs = DrawCloudCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint);
                        break;

                    case (CalloutStyle.Rectangle):
                        hotRegionPathAbs = DrawRectangleCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint);
                        break;

                    case (CalloutStyle.Ellipse):
                        hotRegionPathAbs = DrawRoundedRectCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint,
                            true);
                        break;

                    case (CalloutStyle.RoundedRectangle):
                        hotRegionPathAbs = DrawRoundedRectCallout(
                            graphics,
                            rectanglePosition,
                            anchorPoint,
                            false);
                        break;
                }
            }

            //Clean up
            if (hotRegionPathAbs != null)
                hotRegionPathAbs.Dispose();

            // Paint selection handles
            PaintSelectionHandles(graphics, selectionRect, null);
        }

        /// <summary>
        /// Draws Rounded rectangle or Ellipse style callout.
        /// </summary>
        /// <param name="graphics">Chart graphics.</param>
        /// <param name="rectanglePosition">Position of annotation objet.</param>
        /// <param name="anchorPoint">Anchor location.</param>
        /// <param name="isEllipse">True if ellipse shape should be used.</param>
        /// <returns>Hot region of the callout.</returns>
        private SKPath DrawRoundedRectCallout(
            ChartGraphics graphics,
            SKRect rectanglePosition,
            SKPoint anchorPoint,
            bool isEllipse)
        {
            // Get absolute position
            SKRect rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

            // NOTE: Fix for issue #6692.
            // Do not draw the callout if size is not set. This may happen if callou text is set to empty string.
            if (rectanglePositionAbs.Width <= 0 || rectanglePositionAbs.Height <= 0)
            {
                return null;
            }

            // Create ellipse path
            SKPath ellipsePath = new();
            if (isEllipse)
            {
                // Add ellipse shape
                ellipsePath.AddOval(rectanglePositionAbs);
            }
            else
            {
                // Add rounded rectangle shape
                float radius = Math.Min(rectanglePositionAbs.Width, rectanglePositionAbs.Height);
                radius /= 5f;
                ellipsePath = CreateRoundedRectPath(rectanglePositionAbs, radius);
            }

            // Draw perspective polygons from anchoring point
            if (!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y) && !rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
            {
                // Get absolute anchor point
                SKPoint anchorPointAbs = graphics.GetAbsolutePoint(new SKPoint(anchorPoint.X, anchorPoint.Y));

                // Flatten ellipse path
                //ellipsePath.Flatten();

                // Find point in the path closest to the anchor point
                SKPoint[] points = ellipsePath.Points;
                int closestPointIndex = 0;
                int index = 0;
                float currentDistance = float.MaxValue;
                foreach (SKPoint point in points)
                {
                    float deltaX = point.X - anchorPointAbs.X;
                    float deltaY = point.Y - anchorPointAbs.Y;
                    float distance = deltaX * deltaX + deltaY * deltaY;
                    if (distance < currentDistance)
                    {
                        currentDistance = distance;
                        closestPointIndex = index;
                    }
                    ++index;
                }

                // Change point to the anchor location
                points[closestPointIndex] = anchorPointAbs;

                // Recreate ellipse path
                ellipsePath.Reset();
                ellipsePath.AddPoly(points);
                ellipsePath.Close();
            }

            // Draw ellipse
            graphics.DrawPathAbs(
                ellipsePath,
                BackColor,
                BackHatchStyle,
                string.Empty,
                ChartImageWrapMode.Scaled,
                SKColor.Empty,
                ChartImageAlignmentStyle.Center,
                BackGradientStyle,
                BackSecondaryColor,
                LineColor,
                LineWidth,
                LineDashStyle,
                PenAlignment.Center,
                ShadowOffset,
                ShadowColor);

            // Draw text
            DrawText(graphics, rectanglePosition, true, false);

            return ellipsePath;
        }

        /// <summary>
        /// Draws Rectangle style callout.
        /// </summary>
        /// <param name="graphics">Chart graphics.</param>
        /// <param name="rectanglePosition">Position of annotation objet.</param>
        /// <param name="anchorPoint">Anchor location.</param>
        /// <returns>Hot region of the callout.</returns>
        private SKPath DrawRectangleCallout(
            ChartGraphics graphics,
            SKRect rectanglePosition,
            SKPoint anchorPoint)
        {
            // Create path for the rectangle connected with anchor point.
            SKPath hotRegion = null;
            bool anchorVisible = false;
            if (!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
            {
                // Get relative size of a pixel
                SKSize pixelSize = graphics.GetRelativeSize(new SKSize(1f, 1f));

                // Increase annotation position rectangle by 1 pixel
                SKRect inflatedPosition = new(rectanglePosition.Left, rectanglePosition.Top, rectanglePosition.Right, rectanglePosition.Bottom);
                inflatedPosition.Inflate(pixelSize);

                // Check if point is inside annotation position
                if (!inflatedPosition.Contains(anchorPoint.X, anchorPoint.Y))
                {
                    anchorVisible = true;

                    // Get absolute position
                    SKRect rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

                    // Get absolute anchor point
                    SKPoint anchorPointAbs = graphics.GetAbsolutePoint(new SKPoint(anchorPoint.X, anchorPoint.Y));

                    // Calculate anchor pointer thicness
                    float size = Math.Min(rectanglePositionAbs.Width, rectanglePositionAbs.Height);
                    size /= 4f;

                    // Create shape points
                    SKPoint[] points = new SKPoint[7];
                    if (anchorPoint.X < rectanglePosition.Left &&
                        anchorPoint.Y > rectanglePosition.Bottom)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[2] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[3] = new SKPoint(rectanglePositionAbs.Left + size, rectanglePositionAbs.Bottom);
                        points[4] = anchorPointAbs;
                        points[5] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom - size);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom - size);
                    }
                    else if (anchorPoint.X >= rectanglePosition.Left &&
                        anchorPoint.X <= rectanglePosition.Right &&
                        anchorPoint.Y > rectanglePosition.Bottom)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[2] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[3] = new SKPoint(rectanglePositionAbs.Left + rectanglePositionAbs.Width / 2f + size, rectanglePositionAbs.Bottom);
                        points[4] = anchorPointAbs;
                        points[5] = new SKPoint(rectanglePositionAbs.Left + rectanglePositionAbs.Width / 2f - size, rectanglePositionAbs.Bottom);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                    }
                    else if (anchorPoint.X > rectanglePosition.Right &&
                        anchorPoint.Y > rectanglePosition.Bottom)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[2] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom - size);
                        points[3] = anchorPointAbs;
                        points[4] = new SKPoint(rectanglePositionAbs.Right - size, rectanglePositionAbs.Bottom);
                        points[5] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                    }
                    else if (anchorPoint.X > rectanglePosition.Right &&
                        anchorPoint.Y <= rectanglePosition.Bottom &&
                        anchorPoint.Y >= rectanglePosition.Top)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[2] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top + rectanglePositionAbs.Height / 2f - size);
                        points[3] = anchorPointAbs;
                        points[4] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top + rectanglePositionAbs.Height / 2f + size);
                        points[5] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                    }
                    else if (anchorPoint.X > rectanglePosition.Right &&
                        anchorPoint.Y < rectanglePosition.Top)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Right - size, rectanglePositionAbs.Top);
                        points[2] = anchorPointAbs;
                        points[3] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top + size);
                        points[4] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[5] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                    }
                    else if (anchorPoint.X >= rectanglePosition.Left &&
                        anchorPoint.X <= rectanglePosition.Right &&
                        anchorPoint.Y < rectanglePosition.Top)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Left + rectanglePositionAbs.Width / 2f - size, rectanglePositionAbs.Top);
                        points[2] = anchorPointAbs;
                        points[3] = new SKPoint(rectanglePositionAbs.Left + rectanglePositionAbs.Width / 2f + size, rectanglePositionAbs.Top);
                        points[4] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[5] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                    }
                    else if (anchorPoint.X < rectanglePosition.Left &&
                        anchorPoint.Y < rectanglePosition.Top)
                    {
                        points[0] = anchorPointAbs;
                        points[1] = new SKPoint(rectanglePositionAbs.Left + size, rectanglePositionAbs.Top);
                        points[2] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[3] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[4] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                        points[5] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top + size);
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top + size);
                    }
                    else if (anchorPoint.X < rectanglePosition.Left &&
                        anchorPoint.Y >= rectanglePosition.Top &&
                        anchorPoint.Y <= rectanglePosition.Bottom)
                    {
                        points[0] = rectanglePositionAbs.Location;
                        points[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                        points[2] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                        points[3] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                        points[4] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top + rectanglePositionAbs.Height / 2f + size);
                        points[5] = anchorPointAbs;
                        points[6] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top + rectanglePositionAbs.Height / 2f - size);
                    }

                    // Create graphics path of the callout
                    hotRegion = new SKPath();
                    hotRegion.AddPoly(points);
                    hotRegion.Close();

                    // Draw callout
                    graphics.DrawPathAbs(
                        hotRegion,
                        BackColor,
                        BackHatchStyle,
                        string.Empty,
                        ChartImageWrapMode.Scaled,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        BackGradientStyle,
                        BackSecondaryColor,
                        LineColor,
                        LineWidth,
                        LineDashStyle,
                        PenAlignment.Center,
                        ShadowOffset,
                        ShadowColor);
                }
            }

            // Draw rectangle if anchor is not visible
            if (!anchorVisible)
            {
                graphics.FillRectangleRel(
                    rectanglePosition,
                    BackColor,
                    BackHatchStyle,
                    string.Empty,
                    ChartImageWrapMode.Scaled,
                    SKColor.Empty,
                    ChartImageAlignmentStyle.Center,
                    BackGradientStyle,
                    BackSecondaryColor,
                    LineColor,
                    LineWidth,
                    LineDashStyle,
                    ShadowColor,
                    ShadowOffset,
                    PenAlignment.Center);

                // Get hot region
                hotRegion = new SKPath();
                hotRegion.AddRect(graphics.GetAbsoluteRectangle(rectanglePosition));
            }

            // Draw text
            DrawText(graphics, rectanglePosition, false, false);

            return hotRegion;
        }

        /// <summary>
        /// Draws Perspective style callout.
        /// </summary>
        /// <param name="graphics">Chart graphics.</param>
        /// <param name="rectanglePosition">Position of annotation objet.</param>
        /// <param name="anchorPoint">Anchor location.</param>
        /// <returns>Hot region of the cloud.</returns>
        private SKPath DrawCloudCallout(
            ChartGraphics graphics,
            SKRect rectanglePosition,
            SKPoint anchorPoint)
        {
            // Get absolute position
            SKRect rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

            // Draw perspective polygons from anchoring point
            if (!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y) && !rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
            {
                // Get center point of the cloud
                SKPoint cloudCenterAbs = graphics.GetAbsolutePoint(
                    new SKPoint(
                    rectanglePosition.Left + rectanglePosition.Width / 2f,
                    rectanglePosition.Top + rectanglePosition.Height / 2f));

                // Calculate absolute ellipse size and position
                SKSize ellipseSize = graphics.GetAbsoluteSize(
                    new SKSize(rectanglePosition.Width, rectanglePosition.Height));
                ellipseSize.Width /= 10f;
                ellipseSize.Height /= 10f;
                SKPoint anchorPointAbs = graphics.GetAbsolutePoint(
                    new SKPoint(anchorPoint.X, anchorPoint.Y));
                SKPoint ellipseLocation = anchorPointAbs;

                // Get distance between anchor point and center of the cloud
                float dxAbs = anchorPointAbs.X - cloudCenterAbs.X;
                float dyAbs = anchorPointAbs.Y - cloudCenterAbs.Y;

                SKPoint point = SKPoint.Empty;
                if (anchorPoint.Y < rectanglePosition.Top)
                {
                    point = GetIntersectionY(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Top);
                    if (point.X < rectanglePositionAbs.Left)
                    {
                        point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Left);
                    }
                    else if (point.X > rectanglePositionAbs.Right)
                    {
                        point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Right);
                    }
                }
                else if (anchorPoint.Y > rectanglePosition.Bottom)
                {
                    point = GetIntersectionY(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Bottom);
                    if (point.X < rectanglePositionAbs.Left)
                    {
                        point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Left);
                    }
                    else if (point.X > rectanglePositionAbs.Right)
                    {
                        point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Right);
                    }
                }
                else
                {
                    if (anchorPoint.X < rectanglePosition.Left)
                    {
                        point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Left);
                    }
                    else
                    {
                        point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Right);
                    }
                }

                SKSize size = new(Math.Abs(cloudCenterAbs.X - point.X), Math.Abs(cloudCenterAbs.Y - point.Y));
                if (dxAbs > 0)
                    dxAbs -= size.Width;
                else
                    dxAbs += size.Width;

                if (dyAbs > 0)
                    dyAbs -= size.Height;
                else
                    dyAbs += size.Height;

                // Draw 3 smaller ellipses from anchor point to the cloud
                for (int index = 0; index < 3; index++)
                {
                    using SKPath path = new();
                    // Create ellipse path
                    path.AddOval(new SKRect(
                        ellipseLocation.X - ellipseSize.Width / 2f,
                        ellipseLocation.Y - ellipseSize.Height / 2f,
                        ellipseLocation.X + ellipseSize.Width,
                        ellipseLocation.Y + ellipseSize.Height));

                    // Draw ellipse
                    graphics.DrawPathAbs(
                        path,
                        BackColor,
                        BackHatchStyle,
                        string.Empty,
                        ChartImageWrapMode.Scaled,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        BackGradientStyle,
                        BackSecondaryColor,
                        LineColor,
                        1, // this.LineWidth,	NOTE: Cloud supports only 1 pixel border
                        LineDashStyle,
                        PenAlignment.Center,
                        ShadowOffset,
                        ShadowColor);

                    // Adjust ellipse size
                    ellipseSize.Width *= 1.5f;
                    ellipseSize.Height *= 1.5f;

                    // Adjust next ellipse position
                    ellipseLocation.X -= dxAbs / 3f + (index * (dxAbs / 10f));
                    ellipseLocation.Y -= dyAbs / 3f + (index * (dyAbs / 10f));
                }
            }

            // Draw cloud
            SKPath pathCloud = GetCloudPath(rectanglePositionAbs);
            graphics.DrawPathAbs(
                pathCloud,
                BackColor,
                BackHatchStyle,
                String.Empty,
                ChartImageWrapMode.Scaled,
                SKColor.Empty,
                ChartImageAlignmentStyle.Center,
                BackGradientStyle,
                BackSecondaryColor,
                LineColor,
                1, // this.LineWidth,	NOTE: Cloud supports only 1 pixel border
                LineDashStyle,
                PenAlignment.Center,
                ShadowOffset,
                ShadowColor);

            // Draw cloud outline (Do not draw in SVG or Flash Animation)

            using SKPath pathCloudOutline = GetCloudOutlinePath(rectanglePositionAbs);
            graphics.DrawPathAbs(
                pathCloudOutline,
                BackColor,
                BackHatchStyle,
                string.Empty,
                ChartImageWrapMode.Scaled,
                SKColor.Empty,
                ChartImageAlignmentStyle.Center,
                BackGradientStyle,
                BackSecondaryColor,
                LineColor,
                1, // this.LineWidth,	NOTE: Cloud supports only 1 pixel border
                LineDashStyle,
                PenAlignment.Center);

            // Draw text
            DrawText(graphics, rectanglePosition, true, false);

            return pathCloud;
        }

        /// <summary>
        /// Draws Perspective style callout.
        /// </summary>
        /// <param name="graphics">Chart graphics.</param>
        /// <param name="rectanglePosition">Position of annotation objet.</param>
        /// <param name="anchorPoint">Anchor location.</param>
        /// <returns>Hot region of the cloud.</returns>
        private SKPath DrawPerspectiveCallout(
            ChartGraphics graphics,
            SKRect rectanglePosition,
            SKPoint anchorPoint)
        {
            // Draw rectangle
            graphics.FillRectangleRel(
                rectanglePosition,
                BackColor,
                BackHatchStyle,
                String.Empty,
                ChartImageWrapMode.Scaled,
                SKColor.Empty,
                ChartImageAlignmentStyle.Center,
                BackGradientStyle,
                BackSecondaryColor,
                LineColor,
                LineWidth,
                LineDashStyle,
                ShadowColor,
                0,  // Shadow is never drawn
                PenAlignment.Center);

            // Create hot region path
            SKPath hotRegion = new();
            hotRegion.AddRect(graphics.GetAbsoluteRectangle(rectanglePosition));

            // Draw text
            DrawText(graphics, rectanglePosition, false, false);

            // Draw perspective polygons from anchoring point
            if (!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y) && !rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
            {
                SKColor[] perspectivePathColors = new SKColor[2];
                SKColor color = BackColor;
                perspectivePathColors[0] = ChartGraphics.GetBrightGradientColor(color, 0.6);
                perspectivePathColors[1] = ChartGraphics.GetBrightGradientColor(color, 0.8);
                SKPath[] perspectivePaths = new SKPath[2];
                using (perspectivePaths[0] = new SKPath())
                {
                    using (perspectivePaths[1] = new SKPath())
                    {
                        // Convert coordinates to absolute
                        SKRect rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);
                        SKPoint anchorPointAbs = graphics.GetAbsolutePoint(anchorPoint);

                        // Create paths of perspective
                        if (anchorPoint.Y < rectanglePosition.Top)
                        {
                            SKPoint[] points1 = new SKPoint[3];
                            points1[0] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top);
                            points1[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                            points1[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                            perspectivePaths[0].AddPoly(points1);
                            if (anchorPoint.X < rectanglePosition.Left)
                            {
                                SKPoint[] points2 = new SKPoint[3];
                                points2[0] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                                points2[1] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top);
                                points2[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                                perspectivePaths[1].AddPoly(points2);
                            }
                            else if (anchorPoint.X > rectanglePosition.Right)
                            {
                                SKPoint[] points2 = new SKPoint[3];
                                points2[0] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                                points2[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                                points2[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                                perspectivePaths[1].AddPoly(points2);
                            }
                        }
                        else if (anchorPoint.Y > rectanglePosition.Bottom)
                        {
                            SKPoint[] points1 = new SKPoint[3];
                            points1[0] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                            points1[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                            points1[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                            perspectivePaths[0].AddPoly(points1);
                            if (anchorPoint.X < rectanglePosition.Left)
                            {
                                SKPoint[] points2 = new SKPoint[3];
                                points2[0] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                                points2[1] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top);
                                points2[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                                perspectivePaths[1].AddPoly(points2);
                            }
                            else if (anchorPoint.X > rectanglePosition.Right)
                            {
                                SKPoint[] points2 = new SKPoint[3];
                                points2[0] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                                points2[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                                points2[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                                perspectivePaths[1].AddPoly(points2);
                            }
                        }
                        else
                        {
                            if (anchorPoint.X < rectanglePosition.Left)
                            {
                                SKPoint[] points2 = new SKPoint[3];
                                points2[0] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Bottom);
                                points2[1] = new SKPoint(rectanglePositionAbs.Left, rectanglePositionAbs.Top);
                                points2[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                                perspectivePaths[1].AddPoly(points2);
                            }
                            else if (anchorPoint.X > rectanglePosition.Right)
                            {
                                SKPoint[] points2 = new SKPoint[3];
                                points2[0] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
                                points2[1] = new SKPoint(rectanglePositionAbs.Right, rectanglePositionAbs.Top);
                                points2[2] = new SKPoint(anchorPointAbs.X, anchorPointAbs.Y);
                                perspectivePaths[1].AddPoly(points2);
                            }
                        }

                        // Draw paths if non-empty
                        int index = 0;
                        foreach (SKPath path in perspectivePaths)
                        {
                            if (path.PointCount > 0)
                            {
                                path.Close();
                                graphics.DrawPathAbs(
                                    path,
                                    perspectivePathColors[index],
                                    BackHatchStyle,
                                    String.Empty,
                                    ChartImageWrapMode.Scaled,
                                    SKColor.Empty,
                                    ChartImageAlignmentStyle.Center,
                                    BackGradientStyle,
                                    BackSecondaryColor,
                                    LineColor,
                                    LineWidth,
                                    LineDashStyle,
                                    PenAlignment.Center);

                                // Add area to hot region path
                                hotRegion.AddPath(path);
                            }
                            ++index;
                        }
                    }
                }
            }

            return hotRegion;
        }

        /// <summary>
        /// Draws SimpleLine or BorderLine style callout.
        /// </summary>
        /// <param name="graphics">Chart graphics.</param>
        /// <param name="rectanglePosition">Position of annotation objet.</param>
        /// <param name="anchorPoint">Anchor location.</param>
        /// <param name="drawRectangle">If true draws BorderLine style, otherwise SimpleLine.</param>
        /// <returns>Hot region of the cloud.</returns>
        private SKPath DrawRectangleLineCallout(
            ChartGraphics graphics,
            SKRect rectanglePosition,
            SKPoint anchorPoint,
            bool drawRectangle)
        {
            // Rectangle mode
            if (drawRectangle)
            {
                // Draw rectangle
                graphics.FillRectangleRel(
                    rectanglePosition,
                    BackColor,
                    BackHatchStyle,
                    String.Empty,
                    ChartImageWrapMode.Scaled,
                    SKColor.Empty,
                    ChartImageAlignmentStyle.Center,
                    BackGradientStyle,
                    BackSecondaryColor,
                    LineColor,
                    LineWidth,
                    LineDashStyle,
                    ShadowColor,
                    ShadowOffset,
                    PenAlignment.Center);

                // Draw text
                DrawText(graphics, rectanglePosition, false, false);
            }
            else
            {
                // Draw text
                rectanglePosition = DrawText(graphics, rectanglePosition, false, true);
                SKSize pixelSize = graphics.GetRelativeSize(new SKSize(2f, 2f));
                rectanglePosition.Inflate(pixelSize);
            }

            // Create hot region path
            SKPath hotRegion = new();
            hotRegion.AddRect(graphics.GetAbsoluteRectangle(rectanglePosition));

            // Define position of text underlying line
            SKPoint textLinePoint1 = new(rectanglePosition.Left, rectanglePosition.Bottom);
            SKPoint textLinePoint2 = new(rectanglePosition.Right, rectanglePosition.Bottom);

            // Draw line to the anchor point
            if (!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
            {
                // Check if point is inside annotation position
                if (!rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
                {
                    SKPoint lineSecondPoint = SKPoint.Empty;
                    if (anchorPoint.X < rectanglePosition.Left)
                    {
                        lineSecondPoint.X = rectanglePosition.Left;
                    }
                    else if (anchorPoint.X > rectanglePosition.Right)
                    {
                        lineSecondPoint.X = rectanglePosition.Right;
                    }
                    else
                    {
                        lineSecondPoint.X = rectanglePosition.Left + rectanglePosition.Width / 2f;
                    }

                    if (anchorPoint.Y < rectanglePosition.Top)
                    {
                        lineSecondPoint.Y = rectanglePosition.Top;
                    }
                    else if (anchorPoint.Y > rectanglePosition.Bottom)
                    {
                        lineSecondPoint.Y = rectanglePosition.Bottom;
                    }
                    else
                    {
                        lineSecondPoint.Y = rectanglePosition.Top + rectanglePosition.Height / 2f;
                    }

                    //TODO: fix this
#if false
                    // Set line caps
                    bool capChanged = false;
                    SKStrokeCap oldStartCap = SKStrokeCap.Butt;
                    if (CalloutAnchorCap != LineAnchorCapStyle.None)
                    {
                        // Save old pen
                        capChanged = true;
                        oldStartCap = graphics.Pen.StrokeCap;

                        // Apply anchor cap settings
                        if (CalloutAnchorCap == LineAnchorCapStyle.Arrow)
                        {
                            // Adjust arrow size for small line width
                            if (LineWidth < 4)
                            {
                                int adjustment = 3 - LineWidth;
                                graphics.Pen.StrokeCap = LineCap.Custom;
                                graphics.Pen.CustomStartCap = new AdjustableArrowCap(
                                    LineWidth + adjustment,
                                    LineWidth + adjustment,
                                    true);
                            }
                            else
                            {
                                graphics.Pen.StartCap = LineCap.ArrowAnchor;
                            }
                        }
                        else if (CalloutAnchorCap == LineAnchorCapStyle.Diamond)
                        {
                            graphics.Pen.StartCap = LineCap.DiamondAnchor;
                        }
                        else if (CalloutAnchorCap == LineAnchorCapStyle.Round)
                        {
                            graphics.Pen.StartCap = LineCap.RoundAnchor;
                        }
                        else if (CalloutAnchorCap == LineAnchorCapStyle.Square)
                        {
                            graphics.Pen.StartCap = LineCap.SquareAnchor;
                        }
                    }
#endif
                    // Draw callout line
                    graphics.DrawLineAbs(
                        LineColor,
                        LineWidth,
                        LineDashStyle,
                        graphics.GetAbsolutePoint(anchorPoint),
                        graphics.GetAbsolutePoint(lineSecondPoint),
                        ShadowColor,
                        ShadowOffset);

                    // Create hot region path
                    using (SKPath linePath = new())
                    {
                        linePath.AddLine(
                            graphics.GetAbsolutePoint(anchorPoint),
                            graphics.GetAbsolutePoint(lineSecondPoint));

                        //linePath.Widen(new Pen(SKColors.Black, LineWidth + 2));
                        //hotRegion.SetMarkers();
                        hotRegion.AddPath(linePath);
                    }

                    //// Restore line caps
                    //if (capChanged)
                    //{
                    //    graphics.Pen.StartCap = oldStartCap;
                    //}

                    // Adjust text underlying line position
                    if (anchorPoint.Y < rectanglePosition.Top)
                    {
                        textLinePoint1.Y = rectanglePosition.Top;
                        textLinePoint2.Y = rectanglePosition.Top;
                    }
                    else if (anchorPoint.Y > rectanglePosition.Top &&
                        anchorPoint.Y < rectanglePosition.Bottom)
                    {
                        textLinePoint1.Y = rectanglePosition.Top;
                        textLinePoint2.Y = rectanglePosition.Bottom;
                        if (anchorPoint.X < rectanglePosition.Left)
                        {
                            textLinePoint1.X = rectanglePosition.Left;
                            textLinePoint2.X = rectanglePosition.Left;
                        }
                        else
                        {
                            textLinePoint1.X = rectanglePosition.Right;
                            textLinePoint2.X = rectanglePosition.Right;
                        }
                    }
                }

                // Draw text underlying line
                if (!drawRectangle)
                {
                    graphics.DrawLineAbs(
                        LineColor,
                        LineWidth,
                        LineDashStyle,
                        graphics.GetAbsolutePoint(textLinePoint1),
                        graphics.GetAbsolutePoint(textLinePoint2),
                        ShadowColor,
                        ShadowOffset);

                    // Create hot region path
                    using SKPath linePath = new();
                    linePath.AddLine(
                        graphics.GetAbsolutePoint(textLinePoint1),
                        graphics.GetAbsolutePoint(textLinePoint2));

                    //linePath.Widen(new Pen(SKColors.Black, LineWidth + 2));
                    //hotRegion.SetMarkers();
                    hotRegion.AddPath(linePath);
                }
            }

            return hotRegion;
        }

        #endregion // Painting

        #region Anchor Methods

        /// <summary>
        /// Checks if annotation draw anything in the anchor position (except selection handle)
        /// </summary>
        /// <returns>True if annotation "connects" itself and anchor point visually.</returns>
        override internal bool IsAnchorDrawn()
        {
            return true;
        }

        #endregion // Anchor Methods

        #region Helper methods

        /// <summary>
        /// Gets cloud callout outline graphics path.
        /// </summary>
        /// <param name="position">Absolute position of the callout cloud.</param>
        /// <returns>Cloud outline path.</returns>
        private static SKPath GetCloudOutlinePath(SKRect position)
        {
            if (_cloudOutlinePath == null)
            {
                GetCloudPath(position);
            }

            // Translate and sacle original path to fit specified position
            SKPath resultPath = _cloudOutlinePath;
            SKMatrix matrix = new();
            matrix.Translate(-_cloudBounds.Left, -_cloudBounds.Top);
            resultPath.Transform(matrix);
            matrix = new();
            matrix.Translate(position.Left, position.Top);
            matrix.ScaleX = position.Width / _cloudBounds.Width;
            matrix.ScaleY = position.Height / _cloudBounds.Height;
            resultPath.Transform(matrix);

            return resultPath;
        }

        /// <summary>
        /// Gets cloud callout graphics path.
        /// </summary>
        /// <param name="position">Absolute position of the callout cloud.</param>
        /// <returns>Cloud path.</returns>
        private static SKPath GetCloudPath(SKRect position)
        {
            // Check if cloud path was already created
            if (_cloudPath == null)
            {
                // Create cloud path
                _cloudPath = new SKPath();

                _cloudPath.AddBezier(1689.5f, 1998.6f, 1581.8f, 2009.4f, 1500f, 2098.1f, 1500f, 2204f);

                _cloudPath.AddBezier(1500f, 2204f, 1499.9f, 2277.2f, 1539.8f, 2345.1f, 1604.4f, 2382.1f);

                _cloudPath.AddBezier(1603.3f, 2379.7f, 1566.6f, 2417.8f, 1546.2f, 2468.1f, 1546.2f, 2520.1f);
                _cloudPath.AddBezier(1546.2f, 2520.1f, 1546.2f, 2633.7f, 1641.1f, 2725.7f, 1758.1f, 2725.7f);
                _cloudPath.AddBezier(1758.1f, 2725.7f, 1766.3f, 2725.6f, 1774.6f, 2725.2f, 1782.8f, 2724.2f);

                _cloudPath.AddBezier(1781.7f, 2725.6f, 1848.5f, 2839.4f, 1972.8f, 2909.7f, 2107.3f, 2909.7f);
                _cloudPath.AddBezier(2107.3f, 2909.7f, 2175.4f, 2909.7f, 2242.3f, 2891.6f, 2300.6f, 2857.4f);

                _cloudPath.AddBezier(2300f, 2857.6f, 2360.9f, 2946.5f, 2463.3f, 2999.7f, 2572.9f, 2999.7f);
                _cloudPath.AddBezier(2572.9f, 2999.7f, 2717.5f, 2999.7f, 2845.2f, 2907.4f, 2887.1f, 2772.5f);

                _cloudPath.AddBezier(2887.4f, 2774.3f, 2932.1f, 2801.4f, 2983.6f, 2815.7f, 3036.3f, 2815.7f);
                _cloudPath.AddBezier(3036.3f, 2815.7f, 3190.7f, 2815.7f, 3316.3f, 2694.8f, 3317.5f, 2544.8f);

                _cloudPath.AddBezier(3317f, 2544.1f, 3479.2f, 2521.5f, 3599.7f, 2386.5f, 3599.7f, 2227.2f);
                _cloudPath.AddBezier(3599.7f, 2227.2f, 3599.7f, 2156.7f, 3575.7f, 2088.1f, 3531.6f, 2032.2f);

                _cloudPath.AddBezier(3530.9f, 2032f, 3544.7f, 2000.6f, 3551.9f, 1966.7f, 3551.9f, 1932.5f);
                _cloudPath.AddBezier(3551.9f, 1932.5f, 3551.9f, 1818.6f, 3473.5f, 1718.8f, 3360.7f, 1688.8f);

                _cloudPath.AddBezier(3361.6f, 1688.3f, 3341.4f, 1579.3f, 3243.5f, 1500f, 3129.3f, 1500f);
                _cloudPath.AddBezier(3129.3f, 1500f, 3059.8f, 1499.9f, 2994f, 1529.6f, 2949.1f, 1580.9f);

                _cloudPath.AddBezier(2949.5f, 1581.3f, 2909.4f, 1530f, 2847f, 1500f, 2780.8f, 1500f);
                _cloudPath.AddBezier(2780.8f, 1500f, 2700.4f, 1499.9f, 2626.8f, 1544.2f, 2590.9f, 1614.2f);

                _cloudPath.AddBezier(2591.7f, 1617.6f, 2543.2f, 1571.1f, 2477.9f, 1545.1f, 2409.8f, 1545.1f);
                _cloudPath.AddBezier(2409.8f, 1545.1f, 2313.9f, 1545.1f, 2225.9f, 1596.6f, 2180.8f, 1679f);

                _cloudPath.AddBezier(2180.1f, 1680.7f, 2129.7f, 1652f, 2072.4f, 1636.9f, 2014.1f, 1636.9f);
                _cloudPath.AddBezier(2014.1f, 1636.9f, 1832.8f, 1636.9f, 1685.9f, 1779.8f, 1685.9f, 1956f);
                _cloudPath.AddBezier(1685.9f, 1956f, 1685.8f, 1970.4f, 1686.9f, 1984.8f, 1688.8f, 1999f);

                _cloudPath.Close();

                // Create cloud outline path
                _cloudOutlinePath = new SKPath();

                _cloudOutlinePath.AddBezier(1604.4f, 2382.1f, 1636.8f, 2400.6f, 1673.6f, 2410.3f, 1711.2f, 2410.3f);
                _cloudOutlinePath.AddBezier(1711.2f, 2410.3f, 1716.6f, 2410.3f, 1722.2f, 2410.2f, 1727.6f, 2409.8f);

                _cloudOutlinePath.AddBezier(1782.8f, 2724.2f, 1801.3f, 2722.2f, 1819.4f, 2717.7f, 1836.7f, 2711f);

                _cloudOutlinePath.AddBezier(2267.6f, 2797.2f, 2276.1f, 2818.4f, 2287f, 2838.7f, 2300f, 2857.6f);

                _cloudOutlinePath.AddBezier(2887.1f, 2772.5f, 2893.8f, 2750.9f, 2898.1f, 2728.7f, 2900f, 2706.3f);

                // NOTE: This cloud segment overlaps text too much. Removed for now!
                //cloudOutlinePath.StartFigure();
                //cloudOutlinePath.AddBezier(3317.5f, 2544.8f, 3317.5f, 2544f, 3317.6f, 2543.3f, 3317.6f, 2542.6f);
                //cloudOutlinePath.AddBezier(3317.6f, 2542.6f, 3317.6f, 2438.1f, 3256.1f, 2342.8f, 3159.5f, 2297f);

                _cloudOutlinePath.AddBezier(3460.5f, 2124.9f, 3491f, 2099.7f, 3515f, 2067.8f, 3530.9f, 2032f);

                _cloudOutlinePath.AddBezier(3365.3f, 1732.2f, 3365.3f, 1731.1f, 3365.4f, 1730.1f, 3365.4f, 1729f);
                _cloudOutlinePath.AddBezier(3365.4f, 1729f, 3365.4f, 1715.3f, 3364.1f, 1701.7f, 3361.6f, 1688.3f);

                _cloudOutlinePath.AddBezier(2949.1f, 1580.9f, 2934.4f, 1597.8f, 2922.3f, 1616.6f, 2913.1f, 1636.9f);

                _cloudOutlinePath.AddBezier(2590.9f, 1614.2f, 2583.1f, 1629.6f, 2577.2f, 1645.8f, 2573.4f, 1662.5f);

                _cloudOutlinePath.AddBezier(2243.3f, 1727.5f, 2224.2f, 1709.4f, 2203f, 1693.8f, 2180.1f, 1680.7f);

                _cloudOutlinePath.AddBezier(1688.8f, 1999f, 1691.1f, 2015.7f, 1694.8f, 2032.2f, 1699.9f, 2048.3f);

                _cloudOutlinePath.Close();

                // Get cloud path bounds
                _cloudPath.GetBounds(out var b);
                _cloudBounds = b;
            }

            // Translate and sacle original path to fit specified position
            SKPath resultPath = _cloudPath;
            SKMatrix matrix = new();
            matrix.TransX = -_cloudBounds.Left;
            matrix.TransY = -_cloudBounds.Top;
            resultPath.Transform(matrix);
            matrix = new();
            matrix.TransX = position.Left;
            matrix.TransY = position.Top;
            matrix.ScaleX = position.Width / _cloudBounds.Width;
            matrix.ScaleY = position.Height / _cloudBounds.Height;
            resultPath.Transform(matrix);

            return resultPath;
        }

        /// <summary>
        /// Gets intersection point coordinates between point line and and horizontal
        /// line specified by Y coordinate.
        /// </summary>
        /// <param name="firstPoint">First data point.</param>
        /// <param name="secondPoint">Second data point.</param>
        /// <param name="pointY">Y coordinate.</param>
        /// <returns>Intersection point coordinates.</returns>
        internal static SKPoint GetIntersectionY(SKPoint firstPoint, SKPoint secondPoint, float pointY)
        {
            SKPoint intersectionPoint = new();
            intersectionPoint.Y = pointY;
            intersectionPoint.X = (pointY - firstPoint.Y) *
                (secondPoint.X - firstPoint.X) /
                (secondPoint.Y - firstPoint.Y) +
                firstPoint.X;
            return intersectionPoint;
        }

        /// <summary>
        /// Gets intersection point coordinates between point line and and vertical
        /// line specified by X coordinate.
        /// </summary>
        /// <param name="firstPoint">First data point.</param>
        /// <param name="secondPoint">Second data point.</param>
        /// <param name="pointX">X coordinate.</param>
        /// <returns>Intersection point coordinates.</returns>
        internal static SKPoint GetIntersectionX(SKPoint firstPoint, SKPoint secondPoint, float pointX)
        {
            SKPoint intersectionPoint = new();
            intersectionPoint.X = pointX;
            intersectionPoint.Y = (pointX - firstPoint.X) *
                (secondPoint.Y - firstPoint.Y) /
                (secondPoint.X - firstPoint.X) +
                firstPoint.Y;
            return intersectionPoint;
        }

        /// <summary>
        /// Adds a horizontal or vertical line into the path as multiple segments.
        /// </summary>
        /// <param name="path">Graphics path.</param>
        /// <param name="x1">First point X coordinate.</param>
        /// <param name="y1">First point Y coordinate.</param>
        /// <param name="x2">Second point X coordinate.</param>
        /// <param name="y2">Second point Y coordinate.</param>
        /// <param name="segments">Number of segments to add.</param>
        private void PathAddLineAsSegments(SKPath path, float x1, float y1, float x2, float y2, int segments)
        {
            if (x1 == x2)
            {
                float distance = (y2 - y1) / segments;
                for (int index = 0; index < segments; index++)
                {
                    path.AddLine(x1, y1, x1, y1 + distance);
                    y1 += distance;
                }
            }
            else if (y1 == y2)
            {
                float distance = (x2 - x1) / segments;
                for (int index = 0; index < segments; index++)
                {
                    path.AddLine(x1, y1, x1 + distance, y1);
                    x1 += distance;
                }
            }
            else
            {
                throw (new InvalidOperationException(SR.ExceptionAnnotationPathAddLineAsSegmentsInvalid));
            }
        }

        /// <summary>
        /// Helper function which creates a rounded rectangle path.
        /// Extra points are added on the sides to allow anchor connection.
        /// </summary>
        /// <param name="rect">Rectangle coordinates.</param>
        /// <param name="cornerRadius">Corner radius.</param>
        /// <returns>Graphics path object.</returns>
        private SKPath CreateRoundedRectPath(SKRect rect, float cornerRadius)
        {
            // Create rounded rectangle path
            SKPath path = new();
            int segments = 10;
            PathAddLineAsSegments(path, rect.Left + cornerRadius, rect.Top, rect.Right - cornerRadius, rect.Top, segments);

            path.AddArc(new(rect.Right - 2f * cornerRadius, rect.Top, 2f * cornerRadius, 2f * cornerRadius), 270, 90);

            PathAddLineAsSegments(path, rect.Right, rect.Top + cornerRadius, rect.Right, rect.Bottom - cornerRadius, segments);

            path.AddArc(new(rect.Right - 2f * cornerRadius, rect.Bottom - 2f * cornerRadius, 2f * cornerRadius, 2f * cornerRadius), 0, 90);

            PathAddLineAsSegments(path, rect.Right - cornerRadius, rect.Bottom, rect.Left + cornerRadius, rect.Bottom, segments);

            path.AddArc(new SKRect(rect.Left, rect.Bottom - 2f * cornerRadius, 2f * cornerRadius, 2f * cornerRadius), 90, 90);

            PathAddLineAsSegments(path, rect.Left, rect.Bottom - cornerRadius, rect.Left, rect.Top + cornerRadius, segments);

            path.AddArc(new SKRect(rect.Left, rect.Top, 2f * cornerRadius, 2f * cornerRadius), 180, 90);

            return path;
        }

        #endregion // Helper methods

        #endregion
    }
}