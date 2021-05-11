// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Line annotation class.
//


using SkiaSharp;
using System;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Annotation
{
    /// <summary>
    /// <b>LineAnnotation</b> is a class that represents a line annotation.
    /// </summary>
    [
        SRDescription("DescriptionAttributeLineAnnotation_LineAnnotation"),
    ]
    public class LineAnnotation : Annotation
    {
        #region Fields

        // Indicates that an infinitive line should be drawn through 2 specified points.
        private bool _isInfinitive = false;

        // Line start/end caps
        private LineAnchorCapStyle _startCap = LineAnchorCapStyle.None;
        private LineAnchorCapStyle _endCap = LineAnchorCapStyle.None;

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public LineAnnotation()
            : base()
        {
            this.anchorAlignment = ContentAlignment.TopLeft;
        }

        #endregion

        #region Properties

        #region Line Visual Attributes

        /// <summary>
        /// Gets or sets a flag that indicates if an infinitive line should be drawn.
        /// </summary>
        /// <value>
        /// <b>True</b> if a line should be drawn infinitively through 2 points provided, <b>false</b> otherwise.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeDrawInfinitive"),
        ]
        virtual public bool IsInfinitive
        {
            get
            {
                return _isInfinitive;
            }
            set
            {
                _isInfinitive = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a cap style used at the start of an annotation line.
        /// <seealso cref="EndCap"/>
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value, used for a cap style used at the start of an annotation line.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeStartCap3"),
        ]
        virtual public LineAnchorCapStyle StartCap
        {
            get
            {
                return _startCap;
            }
            set
            {
                _startCap = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a cap style used at the end of an annotation line.
        /// <seealso cref="StartCap"/>
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value, used for a cap style used at the end of an annotation line.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeStartCap3"),
        ]
        virtual public LineAnchorCapStyle EndCap
        {
            get
            {
                return _endCap;
            }
            set
            {
                _endCap = value;
                Invalidate();
            }
        }

        #endregion

        #region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
        /// <value>
        /// A <see cref="ContentAlignment"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public ContentAlignment Alignment
        {
            get
            {
                return base.Alignment;
            }
            set
            {
                base.Alignment = value;
            }
        }

        /// <summary>
        /// Gets or sets an annotation's text style.
        /// <seealso cref="Font"/>
        /// 	<seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="TextStyle"/> value used to draw an annotation's text.
        /// </value>
        public override TextStyle TextStyle
        {
            get
            {
                return base.TextStyle;
            }
            set
            {
                base.TextStyle = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// <seealso cref="Font"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeForeColor"),
        ]
        override public SKColor ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// <seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="Font"/> object.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public SKFont Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
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
        /// Not applicable to this annotation type.
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
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
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
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
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
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

        #endregion

        #region Position

        /// <summary>
        /// Gets or sets a flag that specifies whether the size of an annotation is always 
        /// defined in relative chart coordinates.
        /// <seealso cref="Annotation.Width"/>
        /// <seealso cref="Annotation.Height"/>
        /// </summary>
        /// <value>
        /// <b>True</b> if an annotation's <see cref="Annotation.Width"/> and <see cref="Annotation.Height"/> are always 
        /// in chart relative coordinates, <b>false</b> otherwise.
        /// </value>
        /// <remarks>
        /// An annotation's width and height may be set in relative chart or axes coordinates. 
        /// By default, relative chart coordinates are used.
        /// <para>
        /// To use axes coordinates for size set the <b>IsSizeAlwaysRelative</b> property to 
        /// <b>false</b> and either anchor the annotation to a data point or set the 
        /// <see cref="Annotation.AxisX"/> or <see cref="Annotation.AxisY"/> properties.
        /// </para>
        /// </remarks>
        [
        SRCategory("CategoryAttributePosition"),
        SRDescription("DescriptionAttributeSizeAlwaysRelative3"),
        ]
        override public bool IsSizeAlwaysRelative
        {
            get
            {
                return base.IsSizeAlwaysRelative;
            }
            set
            {
                base.IsSizeAlwaysRelative = value;
            }
        }

        #endregion // Position

        #region Anchor

        /// <summary>
        /// Gets or sets an annotation position's alignment to the anchor point.
        /// <seealso cref="Annotation.AnchorX"/>
        /// <seealso cref="Annotation.AnchorY"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="Annotation.AnchorOffsetX"/>
        /// <seealso cref="Annotation.AnchorOffsetY"/>
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

        #endregion // Anchoring

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
                return "Line";
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
                return SelectionPointsStyle.TwoPoints;
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Adjusts the two coordinates of the line.
        /// </summary>
        /// <param name="point1">First line coordinate.</param>
        /// <param name="point2">Second line coordinate.</param>
        /// <param name="selectionRect">Selection rectangle.</param>
        virtual internal void AdjustLineCoordinates(ref SKPoint point1, ref SKPoint point2, ref SKRect selectionRect)
        {
            // Adjust line points to draw infinitive line
            if (IsInfinitive)
            {
                if (Math.Round(point1.X, 3) == Math.Round(point2.X, 3))
                {
                    point1.Y = (point1.Y < point2.Y) ? 0f : 100f;
                    point2.Y = (point1.Y < point2.Y) ? 100f : 0f;
                }
                else if (Math.Round(point1.Y, 3) == Math.Round(point2.Y, 3))
                {
                    point1.X = (point1.X < point2.X) ? 0f : 100f;
                    point2.X = (point1.X < point2.X) ? 100f : 0f;
                }
                else
                {
                    // Calculate intersection point of the line with two bounaries Y = 0 and Y = 100
                    SKPoint intersectionPoint1 = SKPoint.Empty;
                    intersectionPoint1.Y = 0f;
                    intersectionPoint1.X = (0f - point1.Y) *
                        (point2.X - point1.X) /
                        (point2.Y - point1.Y) +
                        point1.X;
                    SKPoint intersectionPoint2 = SKPoint.Empty;
                    intersectionPoint2.Y = 100f;
                    intersectionPoint2.X = (100f - point1.Y) *
                        (point2.X - point1.X) /
                        (point2.Y - point1.Y) +
                        point1.X;

                    // Select point closect to the intersection
                    point1 = (point1.Y < point2.Y) ? intersectionPoint1 : intersectionPoint2;
                    point2 = (point1.Y < point2.Y) ? intersectionPoint2 : intersectionPoint1;
                }
            }
        }

        /// <summary>
        /// Paints an annotation object on the specified graphics.
        /// </summary>
        /// <param name="graphics">
        /// A <see cref="ChartGraphics"/> object, used to paint an annotation object.
        /// </param>
        /// <param name="chart">
        /// Reference to the <see cref="Chart"/> owner control.
        /// </param>
        override internal void Paint(Chart chart, ChartGraphics graphics)
        {
            // Get annotation position in relative coordinates
            SKPoint firstPoint = SKPoint.Empty;
            SKPoint anchorPoint = SKPoint.Empty;
            SKSize size = SKSize.Empty;
            GetRelativePosition(out firstPoint, out size, out anchorPoint);
            SKPoint secondPoint = new SKPoint(firstPoint.X + size.Width, firstPoint.Y + size.Height);

            // Create selection rectangle
            SKRect selectionRect = new SKRect(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Adjust coordinates
            AdjustLineCoordinates(ref firstPoint, ref secondPoint, ref selectionRect);

            // Check if text position is valid
            if (float.IsNaN(firstPoint.X) ||
                float.IsNaN(firstPoint.Y) ||
                float.IsNaN(secondPoint.X) ||
                float.IsNaN(secondPoint.Y))
            {
                return;
            }

            // Set line caps
            bool capChanged = false;
            LineCap oldStartCap = LineCap.Flat;
            LineCap oldEndCap = LineCap.Flat;
            if (this._startCap != LineAnchorCapStyle.None ||
                this._endCap != LineAnchorCapStyle.None)
            {
                capChanged = true;
                oldStartCap = graphics.Pen.StartCap;
                oldEndCap = graphics.Pen.EndCap;

                // Apply anchor cap settings
                if (this._startCap == LineAnchorCapStyle.Arrow)
                {
                    // Adjust arrow size for small line width
                    if (this.LineWidth < 4)
                    {
                        int adjustment = 3 - this.LineWidth;
                        graphics.Pen.StartCap = LineCap.Custom;
                        graphics.Pen.CustomStartCap = new AdjustableArrowCap(
                            this.LineWidth + adjustment,
                            this.LineWidth + adjustment,
                            true);
                    }
                    else
                    {
                        graphics.Pen.StartCap = LineCap.ArrowAnchor;
                    }
                }
                else if (this._startCap == LineAnchorCapStyle.Diamond)
                {
                    graphics.Pen.StartCap = LineCap.DiamondAnchor;
                }
                else if (this._startCap == LineAnchorCapStyle.Round)
                {
                    graphics.Pen.StartCap = LineCap.RoundAnchor;
                }
                else if (this._startCap == LineAnchorCapStyle.Square)
                {
                    graphics.Pen.StartCap = LineCap.SquareAnchor;
                }
                if (this._endCap == LineAnchorCapStyle.Arrow)
                {
                    // Adjust arrow size for small line width
                    if (this.LineWidth < 4)
                    {
                        int adjustment = 3 - this.LineWidth;
                        graphics.Pen.EndCap = LineCap.Custom;
                        graphics.Pen.CustomEndCap = new AdjustableArrowCap(
                            this.LineWidth + adjustment,
                            this.LineWidth + adjustment,
                            true);
                    }
                    else
                    {
                        graphics.Pen.EndCap = LineCap.ArrowAnchor;
                    }
                }
                else if (this._endCap == LineAnchorCapStyle.Diamond)
                {
                    graphics.Pen.EndCap = LineCap.DiamondAnchor;
                }
                else if (this._endCap == LineAnchorCapStyle.Round)
                {
                    graphics.Pen.EndCap = LineCap.RoundAnchor;
                }
                else if (this._endCap == LineAnchorCapStyle.Square)
                {
                    graphics.Pen.EndCap = LineCap.SquareAnchor;
                }
            }

            if (this.Common.ProcessModePaint)
            {
                // Draw line
                graphics.DrawLineRel(
                    this.LineColor,
                    this.LineWidth,
                    this.LineDashStyle,
                    firstPoint,
                    secondPoint,
                    this.ShadowColor,
                    this.ShadowOffset);
            }

            // Restore line caps
            if (capChanged)
            {
                graphics.Pen.StartCap = oldStartCap;
                graphics.Pen.EndCap = oldEndCap;
            }

            // Paint selection handles
            PaintSelectionHandles(graphics, selectionRect, null);
        }

        #endregion
    }

    /// <summary>
    /// <b>VerticalLineAnnotation</b> is a class that represents a vertical line annotation.
    /// </summary>
    [
        SRDescription("DescriptionAttributeVerticalLineAnnotation_VerticalLineAnnotation"),
    ]
    public class VerticalLineAnnotation : LineAnnotation
    {
        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public VerticalLineAnnotation()
            : base()
        {
        }

        #endregion

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
                return "VerticalLine";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adjusts the two coordinates of the line.
        /// </summary>
        /// <param name="point1">First line coordinate.</param>
        /// <param name="point2">Second line coordinate.</param>
        /// <param name="selectionRect">Selection rectangle.</param>
        override internal void AdjustLineCoordinates(ref SKPoint point1, ref SKPoint point2, ref SKRect selectionRect)
        {
            // Make line vertical
            point2.X = point1.X;
            selectionRect.Right = selectionRect.Left;

            // Call base class
            base.AdjustLineCoordinates(ref point1, ref point2, ref selectionRect);
        }

        #region Content Size

        /// <summary>
        /// Gets text annotation content size based on the text and font.
        /// </summary>
        /// <returns>Annotation content position.</returns>
        override internal SKRect GetContentPosition()
        {
            return new SKRect(float.NaN, float.NaN, 0f, float.NaN);
        }

        #endregion // Content Size

        #endregion
    }

    /// <summary>
    /// <b>HorizontalLineAnnotation</b> is a class that represents a horizontal line annotation.
    /// </summary>
    [
        SRDescription("DescriptionAttributeHorizontalLineAnnotation_HorizontalLineAnnotation"),
    ]
    public class HorizontalLineAnnotation : LineAnnotation
    {
        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public HorizontalLineAnnotation()
            : base()
        {
        }

        #endregion

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
                return "HorizontalLine";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adjusts the two coordinates of the line.
        /// </summary>
        /// <param name="point1">First line coordinate.</param>
        /// <param name="point2">Second line coordinate.</param>
        /// <param name="selectionRect">Selection rectangle.</param>
        override internal void AdjustLineCoordinates(ref SKPoint point1, ref SKPoint point2, ref SKRect selectionRect)
        {
            // Make line horizontal
            point2.Y = point1.Y;
            selectionRect.Bottom = selectionRect.Top;

            // Call base class
            base.AdjustLineCoordinates(ref point1, ref point2, ref selectionRect);
        }

        #region Content Size

        /// <summary>
        /// Gets text annotation content size based on the text and font.
        /// </summary>
        /// <returns>Annotation content position.</returns>
        override internal SKRect GetContentPosition()
        {
            return new SKRect(float.NaN, float.NaN, float.NaN, 0f);
        }

        #endregion // Content Size

        #endregion
    }
}
