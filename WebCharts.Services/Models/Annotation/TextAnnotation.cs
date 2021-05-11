// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Text annotation class.
//

using SkiaSharp;
using System;
using System.Security;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Annotation
{

    /// <summary>
    /// <b>TextAnnotation</b> is a class that represents a text annotation.
    /// </summary>
    /// <remarks>
    /// Note that other annotations do display inner text (e.g. rectangle, 
    /// ellipse annotations.).
    /// </remarks>
    [
        SRDescription("DescriptionAttributeTextAnnotation_TextAnnotation"),
    ]
    public class TextAnnotation : Annotation
    {
        // Annotation text
        private string _text = "";

        // Indicates multiline text
        private bool _isMultiline = false;

        // Current content size
        internal SKSize contentSize = SKSize.Empty;

        // Indicates that annotion is an ellipse
        internal bool isEllipse = false;

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public TextAnnotation()
            : base()
        {
        }

        #endregion

        #region Properties

        #region Text Visual Attributes

        /// <summary>
        /// Annotation's text.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeText"),
        ]
        virtual public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Invalidate();

                // Reset content size to empty
                contentSize = SKSize.Empty;
            }
        }

        /// <summary>
        /// Indicates whether the annotation text is multiline.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeMultiline"),
        ]
        virtual public bool IsMultiline
        {
            get
            {
                return _isMultiline;
            }
            set
            {
                _isMultiline = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the font of an annotation's text.
        /// <seealso cref="Annotation.ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="Font"/> object used for an annotation's text.
        /// </value>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTextFont4"),
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

                // Reset content size to empty
                contentSize = SKSize.Empty;
            }
        }

        #endregion

        #region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
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
        /// Not applicable to this annotation type.
        /// </summary>
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
        /// Not applicable to this annotation type.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
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
        SRDescription("DescriptionAttributeTextAnnotation_AnnotationType"),
        ]
        public override string AnnotationType
        {
            get
            {
                return "Text";
            }
        }

        /// <summary>
        /// Annotation selection points style.
        /// </summary>
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

        #endregion

        #endregion

        #region Methods

        #region Painting

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
            SKPoint secondPoint = new(firstPoint.X + size.Width, firstPoint.Y + size.Height);

            // Create selection rectangle
            SKRect selectionRect = new(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Get text position
            SKRect textPosition = new(selectionRect.Left, selectionRect.Top, selectionRect.Right, selectionRect.Bottom);
            if (textPosition.Width < 0 || textPosition.Height < 0)
            {
                textPosition = textPosition.Standardized;
            }

            // Check if text position is valid
            if (textPosition.IsEmpty ||
                float.IsNaN(textPosition.Left) ||
                float.IsNaN(textPosition.Top) ||
                float.IsNaN(textPosition.Right) ||
                float.IsNaN(textPosition.Bottom))
            {
                return;
            }

            if (this.Common.ProcessModePaint)
            {
                DrawText(graphics, textPosition, false, false);
            }

            if (this.Common.ProcessModeRegions)
            {
                // Add hot region
                if (isEllipse)
                {
                    using (SKPath ellipsePath = new())
                    {
                        ellipsePath.AddOval(textPosition);
                        this.Common.HotRegionsList.AddHotRegion(
                            graphics,
                            ellipsePath,
                            true,
                            ReplaceKeywords(this.ToolTip),
                            String.Empty,
                            String.Empty,
                            String.Empty,
                            this,
                            ChartElementType.Annotation);
                    }
                }
                else
                {
                    this.Common.HotRegionsList.AddHotRegion(
                        textPosition,
                        ReplaceKeywords(this.ToolTip),
                        String.Empty,
                        String.Empty,
                        String.Empty,
                        this,
                        ChartElementType.Annotation,
                        String.Empty);
                }
            }

            // Paint selection handles
            PaintSelectionHandles(graphics, selectionRect, null);
        }

        /// <summary>
        /// Draws text in specified rectangle.
        /// </summary>
        /// <param name="graphics">Chart graphics.</param>
        /// <param name="textPosition">Text position.</param>
        /// <param name="noSpacingForCenteredText">True if text allowed to be outside of position when centered.</param>
        /// <param name="getTextPosition">True if position text must be returned by the method.</param>
        /// <returns>Text actual position if required.</returns>
        internal SKRect DrawText(ChartGraphics graphics, SKRect textPosition, bool noSpacingForCenteredText, bool getTextPosition)
        {
            SKRect textActualPosition = SKRect.Empty;

            //***************************************************************
            //** Adjust text position uing text spacing
            //***************************************************************
            bool annotationRelative = false;
            SKRect textSpacing = GetTextSpacing(out annotationRelative);
            float spacingScaleX = 1f;
            float spacingScaleY = 1f;
            if (annotationRelative)
            {
                if (textPosition.Width > 25f)
                {
                    spacingScaleX = textPosition.Width / 50f;
                    spacingScaleX = Math.Max(1f, spacingScaleX);
                }
                if (textPosition.Height > 25f)
                {
                    spacingScaleY = textPosition.Height / 50f;
                    spacingScaleY = Math.Max(1f, spacingScaleY);
                }
            }

            SKRect textPositionWithSpacing = new SKRect(textPosition.Location, textPosition.Size);
            textPositionWithSpacing.Width -= (textSpacing.Width + textSpacing.X) * spacingScaleX;
            textPositionWithSpacing.X += textSpacing.X * spacingScaleX;
            textPositionWithSpacing.Height -= (textSpacing.Height + textSpacing.Y) * spacingScaleY;
            textPositionWithSpacing.Y += textSpacing.Y * spacingScaleY;

            //***************************************************************
            //** Replace new line characters
            //***************************************************************
            string titleText = this.ReplaceKeywords(this.Text.Replace("\\n", "\n"));

            //***************************************************************
            //** Check if centered text require spacing.
            //** Use only half of the spacing required.
            //** Apply only for 1 line of text.
            //***************************************************************
            if (noSpacingForCenteredText &&
                titleText.IndexOf('\n') == -1)
            {
                if (this.Alignment == ContentAlignment.MiddleCenter ||
                    this.Alignment == ContentAlignment.MiddleLeft ||
                    this.Alignment == ContentAlignment.MiddleRight)
                {
                    textPositionWithSpacing.Y = textPosition.Y;
                    textPositionWithSpacing.Height = textPosition.Height;
                    textPositionWithSpacing.Height -= textSpacing.Height / 2f + textSpacing.Y / 2f;
                    textPositionWithSpacing.Y += textSpacing.Y / 2f;
                }
                if (this.Alignment == ContentAlignment.BottomCenter ||
                    this.Alignment == ContentAlignment.MiddleCenter ||
                    this.Alignment == ContentAlignment.TopCenter)
                {
                    textPositionWithSpacing.X = textPosition.X;
                    textPositionWithSpacing.Width = textPosition.Width;
                    textPositionWithSpacing.Width -= textSpacing.Width / 2f + textSpacing.X / 2f;
                    textPositionWithSpacing.X += textSpacing.X / 2f;
                }
            }

            // Draw text
            using (Brush textBrush = new SolidBrush(this.ForeColor))
            {
                using (StringFormat format = new StringFormat(StringFormat.GenericTypographic))
                {
                    //***************************************************************
                    //** Set text format
                    //***************************************************************
                    format.FormatFlags = format.FormatFlags ^ StringFormatFlags.LineLimit;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    if (this.Alignment == ContentAlignment.BottomRight ||
                        this.Alignment == ContentAlignment.MiddleRight ||
                        this.Alignment == ContentAlignment.TopRight)
                    {
                        format.Alignment = StringAlignment.Far;
                    }
                    if (this.Alignment == ContentAlignment.BottomCenter ||
                        this.Alignment == ContentAlignment.MiddleCenter ||
                        this.Alignment == ContentAlignment.TopCenter)
                    {
                        format.Alignment = StringAlignment.Center;
                    }
                    if (this.Alignment == ContentAlignment.BottomCenter ||
                        this.Alignment == ContentAlignment.BottomLeft ||
                        this.Alignment == ContentAlignment.BottomRight)
                    {
                        format.LineAlignment = StringAlignment.Far;
                    }
                    if (this.Alignment == ContentAlignment.MiddleCenter ||
                        this.Alignment == ContentAlignment.MiddleLeft ||
                        this.Alignment == ContentAlignment.MiddleRight)
                    {
                        format.LineAlignment = StringAlignment.Center;
                    }

                    //***************************************************************
                    //** Set shadow color and offset
                    //***************************************************************
                    SKColor textShadowColor = ChartGraphics.GetGradientColor(ForeColor, SKColors.Black, 0.8);
                    int textShadowOffset = 1;
                    TextStyle textStyle = this.TextStyle;
                    if (textStyle == TextStyle.Shadow &&
                        ShadowOffset != 0)
                    {
                        // Draw shadowed text
                        textShadowColor = ShadowColor;
                        textShadowOffset = ShadowOffset;
                    }

                    if (textStyle == TextStyle.Shadow)
                    {
                        textShadowColor = (textShadowColor.Alpha != 255) ? textShadowColor : new(textShadowColor.Red, textShadowColor.Green, textShadowColor.Blue, (byte)(textShadowColor.Alpha / 2));
                    }

                    //***************************************************************
                    //** Get text actual position
                    //***************************************************************
                    if (getTextPosition)
                    {
                        // Measure text size
                        SKSize textSize = graphics.MeasureStringRel(
                            this.ReplaceKeywords(_text.Replace("\\n", "\n")),
                            this.Font,
                            textPositionWithSpacing.Size,
                            format);

                        // Get text position
                        textActualPosition = new SKRect(textPositionWithSpacing.Location, textSize);
                        if (this.Alignment == ContentAlignment.BottomRight ||
                            this.Alignment == ContentAlignment.MiddleRight ||
                            this.Alignment == ContentAlignment.TopRight)
                        {
                            textActualPosition.X += textPositionWithSpacing.Width - textSize.Width;
                        }
                        if (this.Alignment == ContentAlignment.BottomCenter ||
                            this.Alignment == ContentAlignment.MiddleCenter ||
                            this.Alignment == ContentAlignment.TopCenter)
                        {
                            textActualPosition.X += (textPositionWithSpacing.Width - textSize.Width) / 2f;
                        }
                        if (this.Alignment == ContentAlignment.BottomCenter ||
                            this.Alignment == ContentAlignment.BottomLeft ||
                            this.Alignment == ContentAlignment.BottomRight)
                        {
                            textActualPosition.Y += textPositionWithSpacing.Height - textSize.Height;
                        }
                        if (this.Alignment == ContentAlignment.MiddleCenter ||
                            this.Alignment == ContentAlignment.MiddleLeft ||
                            this.Alignment == ContentAlignment.MiddleRight)
                        {
                            textActualPosition.Y += (textPositionWithSpacing.Height - textSize.Height) / 2f;
                        }

                        // Do not allow text to go outside annotation position
                        textActualPosition.Intersect(textPositionWithSpacing);
                    }

                    SKRect absPosition = graphics.GetAbsoluteRectangle(textPositionWithSpacing);
                    Title.DrawStringWithStyle(
                            graphics,
                            titleText,
                            this.TextStyle,
                            this.Font,
                            absPosition,
                            this.ForeColor,
                            textShadowColor,
                            textShadowOffset,
                            format,
                            TextOrientation.Auto
                      );
                }
            }

            return textActualPosition;
        }

        #endregion // Painting

        #region Content Size

        /// <summary>
        /// Gets text annotation content size based on the text and font.
        /// </summary>
        /// <returns>Annotation content position.</returns>
        override internal SKRect GetContentPosition()
        {
            // Return pre calculated value
            if (!contentSize.IsEmpty)
            {
                return new SKRect(float.NaN, float.NaN, contentSize.Width, contentSize.Height);
            }

            // Create temporary bitmap based chart graphics if chart was not 
            // rendered yet and the graphics was not created.
            // NOTE: Fix for issue #3978.
            Graphics graphics = null;
            System.Drawing.Image graphicsImage = null;
            ChartGraphics tempChartGraph = null;
            if (GetGraphics() == null && this.Common != null)
            {
                graphicsImage = new System.Drawing.Bitmap(Common.ChartPicture.Width, Common.ChartPicture.Height);
                graphics = Graphics.FromImage(graphicsImage);
                tempChartGraph = new ChartGraphics(Common);
                tempChartGraph.Graphics = graphics;
                tempChartGraph.SetPictureSize(Common.ChartPicture.Width, Common.ChartPicture.Height);
                this.Common.graph = tempChartGraph;
            }

            // Calculate content size
            SKRect result = SKRect.Empty;
            if (GetGraphics() != null && this.Text.Trim().Length > 0)
            {
                // Measure text using current font and slightly increase it
                contentSize = GetGraphics().MeasureString(
                     "W" + this.ReplaceKeywords(this.Text.Replace("\\n", "\n")),
                     this.Font,
                     new SKSize(2000, 2000),
                     StringFormat.GenericTypographic);

                contentSize.Height *= 1.04f;

                // Convert to relative coordinates
                contentSize = GetGraphics().GetRelativeSize(contentSize);

                // Add spacing
                bool annotationRelative = false;
                SKRect textSpacing = GetTextSpacing(out annotationRelative);
                float spacingScaleX = 1f;
                float spacingScaleY = 1f;
                if (annotationRelative)
                {
                    if (contentSize.Width > 25f)
                    {
                        spacingScaleX = contentSize.Width / 25f;
                        spacingScaleX = Math.Max(1f, spacingScaleX);
                    }
                    if (contentSize.Height > 25f)
                    {
                        spacingScaleY = contentSize.Height / 25f;
                        spacingScaleY = Math.Max(1f, spacingScaleY);
                    }
                }

                contentSize.Width += (textSpacing.X + textSpacing.Width) * spacingScaleX;
                contentSize.Height += (textSpacing.Y + textSpacing.Height) * spacingScaleY;

                result = new SKRect(float.NaN, float.NaN, contentSize.Width, contentSize.Height);
            }

            // Dispose temporary chart graphics
            if (tempChartGraph != null)
            {
                tempChartGraph.Dispose();
                graphics.Dispose();
                graphicsImage.Dispose();
                this.Common.graph = null;
            }

            return result;
        }

        /// <summary>
        /// Gets text spacing on four different sides in relative coordinates.
        /// </summary>
        /// <param name="annotationRelative">Indicates that spacing is in annotation relative coordinates.</param>
        /// <returns>Rectangle with text spacing values.</returns>
        internal virtual SKRect GetTextSpacing(out bool annotationRelative)
        {
            annotationRelative = false;
            SKRect rect = new SKRect(3f, 3f, 3f, 3f);
            if (GetGraphics() != null)
            {
                rect = GetGraphics().GetRelativeRectangle(rect);
            }
            return rect;
        }

        #endregion

        #region Placement Methods

        /// <summary>
        /// Ends user placement of an annotation.
        /// </summary>
        /// <remarks>
        /// Ends an annotation placement operation previously started by a 
        /// <see cref="Annotation.BeginPlacement"/> method call.
        /// <para>
        /// Calling this method is not required, since placement will automatically
        /// end when an end user enters all required points. However, it is useful when an annotation 
        /// placement operation needs to be aborted for some reason.
        /// </para>
        /// </remarks>
        override public void EndPlacement()
        {
            // Check if text editing is allowed
            // Maybe changed later in the EndPlacement method.
            bool allowTextEditing = this.AllowTextEditing;

            // Call base class
            base.EndPlacement();

            // Begin text editing
            if (this.Chart != null)
            {
                this.Chart.Annotations.lastClickedAnnotation = this;
                if (allowTextEditing)
                {
                    BeginTextEditing();
                }
            }
        }

        #endregion // Placement Methods

        #endregion	// Methods
    }

    /// <summary>
    /// The <b>AnnotationSmartLabelStyle</b> class is used to store an annotation's smart 
    /// labels properties.
    /// <seealso cref="Annotation.SmartLabelStyle"/>
    /// </summary>
    /// <remarks>
    /// This class is derived from the <b>SmartLabelStyle</b> class
    /// used for <b>Series</b> objects.
    /// </remarks>
    [
        SRDescription("DescriptionAttributeAnnotationSmartLabelsStyle_AnnotationSmartLabelsStyle"),
    ]
    public class AnnotationSmartLabelStyle : SmartLabelStyle
    {
        #region Constructors and initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public AnnotationSmartLabelStyle()
        {
            this.chartElement = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="chartElement">
        /// Chart element this style belongs to.
        /// </param>
        public AnnotationSmartLabelStyle(Object chartElement) : base(chartElement)
        {
        }

        #endregion

        #region Non Applicable Appearance Attributes (set as Non-Browsable)


        /// <summary>
        /// Callout style of the repositioned smart labels.
        /// </summary>
        /// <remarks>
        /// This method is for internal use and is hidden at design time and runtime.
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeCalloutStyle3"),
        ]
        override public LabelCalloutStyle CalloutStyle
        {
            get
            {
                return base.CalloutStyle;
            }
            set
            {
                base.CalloutStyle = value;
            }
        }

        /// <summary>
        /// Label callout line color.
        /// </summary>
        /// <remarks>
        /// This method is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCalloutLineColor"),
        ]
        override public SKColor CalloutLineColor
        {
            get
            {
                return base.CalloutLineColor;
            }
            set
            {
                base.CalloutLineColor = value;
            }
        }

        /// <summary>
        /// Label callout line style.
        /// </summary>
        /// <remarks>
        /// This method is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineDashStyle"),
        ]
        override public ChartDashStyle CalloutLineDashStyle
        {
            get
            {
                return base.CalloutLineDashStyle;
            }
            set
            {
                base.CalloutLineDashStyle = value;
            }
        }

        /// <summary>
        /// Label callout back color. Applies to the Box style only.
        /// </summary>
        /// <remarks>
        /// This method is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCalloutBackColor"),
        ]
        override public SKColor CalloutBackColor
        {
            get
            {
                return base.CalloutBackColor;
            }
            set
            {
                base.CalloutBackColor = value;
            }
        }

        /// <summary>
        /// Label callout line width.
        /// </summary>
        /// <remarks>
        /// This method is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineWidth"),
        ]
        override public int CalloutLineWidth
        {
            get
            {
                return base.CalloutLineWidth;
            }
            set
            {
                base.CalloutLineWidth = value;
            }
        }

        /// <summary>
        /// Label callout line anchor cap.
        /// </summary>
        /// <remarks>
        /// This method is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCalloutLineAnchorCapStyle"),
        ]
        override public LineAnchorCapStyle CalloutLineAnchorCapStyle
        {
            get
            {
                return base.CalloutLineAnchorCapStyle;
            }
            set
            {
                base.CalloutLineAnchorCapStyle = value;
            }
        }

        #endregion
    }
}
