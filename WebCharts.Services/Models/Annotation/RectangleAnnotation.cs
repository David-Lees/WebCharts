// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Rectangle, Ellipse and 3DBorder annotation classes.
//

using SkiaSharp;
using System;

namespace WebCharts.Services
{
    /// <summary>
    /// <b>RectangleAnnotation</b> is a class that represents a rectangle annotation.
    /// </summary>
    /// <remarks>
    /// A rectangle annotation can also display text inside the rectangle.
    /// </remarks>
    [
            SRDescription("DescriptionAttributeRectangleAnnotation_RectangleAnnotation"),
        ]
    public class RectangleAnnotation : TextAnnotation
    {
        #region Fields

        // Indicates that annotion rectangle should be drawn
        internal bool isRectVisible = true;

        #endregion Fields

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public RectangleAnnotation()
            : base()
        {
        }

        #endregion Construction and Initialization

        #region Properties

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
                return "Rectangle";
            }
        }

        /// <summary>
        /// Gets or sets an annotation's selection points style.
        /// </summary>
        /// <value>
        /// A <see cref="SelectionPointsStyle"/> value that represents the annotation
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Paints an annotation object on the specified graphics.
        /// </summary>
        /// <param name="graphics">
        /// A <see cref="ChartGraphics"/> object, used to paint an annotation object.
        /// </param>
        /// <param name="chart">
        /// Reference to the <see cref="ChartService"/> control.
        /// </param>
        override internal void Paint(ChartService chart, ChartGraphics graphics)
        {
            // Get annotation position in relative coordinates
            GetRelativePosition(out SKPoint firstPoint, out _, out _);
            SKPoint secondPoint = new(firstPoint.X + SKSize.Empty.Width, firstPoint.Y + SKSize.Empty.Height);

            // Create selection rectangle
            SKRect selectionRect = new(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Get text position
            SKRect rectanglePosition = new(selectionRect.Left, selectionRect.Top, selectionRect.Right, selectionRect.Bottom);
            if (rectanglePosition.Width < 0 || rectanglePosition.Height < 0)
            {
                rectanglePosition = rectanglePosition.Standardized;
            }

            // Check if position is valid
            if (float.IsNaN(rectanglePosition.Left) ||
                float.IsNaN(rectanglePosition.Top) ||
                float.IsNaN(rectanglePosition.Right) ||
                float.IsNaN(rectanglePosition.Bottom))
            {
                return;
            }

            if (isRectVisible &&
                Common.ProcessModePaint)
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
                    PenAlignment.Center,
                    isEllipse,
                    1,
                    false);
            }

            // Call base class to paint text, selection handles and process hot regions
            base.Paint(chart, graphics);
        }

        #endregion Methods
    }

    /// <summary>
    /// <b>EllipseAnnotation</b> is a class that represents an ellipse annotation.
    /// </summary>
    /// <remarks>
    /// An ellipse annotation can also display text inside the ellipse.
    /// </remarks>
    [
        SRDescription("DescriptionAttributeEllipseAnnotation_EllipseAnnotation"),
    ]
    public class EllipseAnnotation : RectangleAnnotation
    {
        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public EllipseAnnotation()
            : base()
        {
            isEllipse = true;
        }

        #endregion Construction and Initialization

        #region Properties

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
                return "Ellipse";
            }
        }

        #endregion Properties
    }

    /// <summary>
    /// <b>Border3DAnnotation</b> is a class that represents an annotation with a 3D border.
    /// </summary>
    /// <remarks>
    /// A Border3D annotation can also display inner text.
    /// </remarks>
    [
        SRDescription("DescriptionAttributeBorder3DAnnotation_Border3DAnnotation"),
    ]
    public class Border3DAnnotation : RectangleAnnotation
    {
        #region Fields

        // 3D border properties
        private BorderSkin _borderSkin;

        #endregion Fields

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public Border3DAnnotation()
            : base()
        {
            isRectVisible = false;
            _borderSkin = new BorderSkin(this)
            {
                PageColor = SKColors.Transparent,
                SkinStyle = BorderSkinStyle.Raised
            };

            // Change default appearance styles
            lineColor = SKColor.Empty;
        }

        #endregion Construction and Initialization

        #region Properties

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
                return "Border3D";
            }
        }

        /// <summary>
        /// Gets or sets the skin style of the 3D border.
        /// </summary>
        /// <value>
        /// A <see cref="BorderSkin"/>
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin"),
        ]
        public BorderSkin BorderSkin
        {
            get
            {
                return _borderSkin;
            }
            set
            {
                _borderSkin = value;
                Invalidate();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Paints the annotation object on the specified graphics.
        /// </summary>
        /// <param name="graphics">
        /// A <see cref="ChartGraphics"/>
        /// </param>
        /// <param name="chart">
        /// Reference to the <see cref="ChartService"/> control that owns the annotation.
        /// </param>
        override internal void Paint(ChartService chart, ChartGraphics graphics)
        {
            // Get annotation position in relative coordinates
            GetRelativePosition(out SKPoint firstPoint, out SKSize size, out _);
            SKPoint secondPoint = new(firstPoint.X + size.Width, firstPoint.Y + size.Height);

            // Create selection rectangle
            SKRect selectionRect = new(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Get text position
            SKRect rectanglePosition = new(selectionRect.Left, selectionRect.Top, selectionRect.Right, selectionRect.Bottom);
            if (rectanglePosition.Width < 0 || rectanglePosition.Height < 0)
            {
                rectanglePosition = rectanglePosition.Standardized;
            }

            // Check if position is valid
            if (float.IsNaN(rectanglePosition.Left) ||
                float.IsNaN(rectanglePosition.Top) ||
                float.IsNaN(rectanglePosition.Right) ||
                float.IsNaN(rectanglePosition.Bottom))
            {
                return;
            }

            if (Common.ProcessModePaint)
            {
                // Do not draw border if size is less that 10 pixels
                SKRect absRectanglePosition = graphics.GetAbsoluteRectangle(rectanglePosition);
                if (absRectanglePosition.Width > 30f &&
                    absRectanglePosition.Height > 30f)
                {
                    // Draw rectangle
                    graphics.Draw3DBorderRel(
                        _borderSkin,
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
                        LineDashStyle);
                }
            }

            // Call base class to paint text, selection handles and process hot regions
            base.Paint(chart, graphics);
        }

        /// <summary>
        /// Gets text spacing on four different sides in relative coordinates.
        /// </summary>
        /// <param name="annotationRelative">Indicates that spacing is in annotation relative coordinates.</param>
        /// <returns>Rectangle with text spacing values.</returns>
        internal override SKRect GetTextSpacing(out bool annotationRelative)
        {
            annotationRelative = false;
            SKRect rect = new(3f, 3f, 3f, 3f);
            if (GetGraphics() != null)
            {
                rect = GetGraphics().GetRelativeRectangle(rect);
            }

            if (_borderSkin.SkinStyle != BorderSkinStyle.None &&
                GetGraphics() != null &&
                Chart != null &&
                Chart.chartPicture != null &&
                Common != null)
            {
                IBorderType border3D = Common.BorderTypeRegistry.GetBorderType(_borderSkin.SkinStyle.ToString());
                if (border3D != null)
                {
                    // Adjust are position to the border size
                    SKRect rectangle = new(0f, 0f, 100f, 100f);
                    border3D.AdjustAreasPosition(GetGraphics(), ref rectangle);
                    rect = new SKRect(
                        rectangle.Left + 1,
                        rectangle.Top + 1,
                        100f - rectangle.Right + 2,
                        100f - rectangle.Bottom + 2);
                }
            }

            return rect;
        }

        #endregion Methods
    }
}