// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Image annotation classes.
//



using SkiaSharp;
using System;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.Annotations
{
    /// <summary>
    /// <b>ImageAnnotation</b> is a class that represents an image annotation.
    /// </summary>
    [
        SRDescription("DescriptionAttributeImageAnnotation_ImageAnnotation"),
    ]
    public class ImageAnnotation : Annotation
    {
        #region Fields

        // Annotation image name
        private string _imageName = String.Empty;

        // Image wrapping mode
        private ChartImageWrapMode _imageWrapMode = ChartImageWrapMode.Scaled;

        // Image transparent color
        private SKColor _imageTransparentColor = SKColor.Empty;

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public ImageAnnotation()
            : base()
        {
        }

        #endregion

        #region Properties

        #region Image properties

        /// <summary>
        /// Gets or sets the name of an annotation's image. 
        /// <seealso cref="ImageTransparentColor"/>
        /// </summary>
        /// <value>
        /// A string value representing the name of an annotation's image.
        /// </value>
        /// <remarks>
        /// The name can be a file name, URL for the web control or a name from 
        /// the <see cref="NamedImagesCollection"/> class.	
        /// </remarks>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeImageAnnotation_Image"),
        ]
        public virtual string Image
        {
            get
            {
                return _imageName;
            }
            set
            {
                _imageName = value;
                Invalidate();
            }
        }


        /// <summary>
        /// Gets or sets the drawing mode of the image.
        /// </summary>
        /// <value>
        /// A <see cref="ChartImageWrapMode"/> value that defines the drawing mode of the image. 
        /// </value>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeImageWrapMode"),
        ]
        public ChartImageWrapMode ImageWrapMode
        {
            get
            {
                return _imageWrapMode;
            }
            set
            {
                _imageWrapMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the image.
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value which will be replaced with a transparent color while drawing the image.
        /// </value>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        ]
        public SKColor ImageTransparentColor
        {
            get
            {
                return _imageTransparentColor;
            }
            set
            {
                _imageTransparentColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an annotation's content alignment.
        /// </summary>
        /// <value>
        /// A <see cref="ContentAlignment"/> value that represents the content alignment.
        /// </value>
        /// <remarks>
        /// This property is used to align text for <see cref="TextAnnotation"/>, <see cref="RectangleAnnotation"/>,  
        /// <see cref="EllipseAnnotation"/> and <see cref="CalloutAnnotation"/> objects, and to align 
        /// a non-scaled image inside an <see cref="ImageAnnotation"/> object.
        /// </remarks>
		[
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeImageAnnotation_Alignment"),
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
                Invalidate();
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

        #endregion // Image properties

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
                return "Image";
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

        #endregion

        #region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

        /// <summary>
        /// Not applicable to this type of annotation.
        /// <seealso cref="Font"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
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
        /// Not applicable to this type of annotation.
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


        #endregion

        #endregion

        #region Methods

        #region Painting

        /// <summary>
        /// Paints the annotation object on the specified graphics.
        /// </summary>
        /// <param name="graphics">
        /// A <see cref="ChartGraphics"/> object, used to paint the annotation object.
        /// </param>
        /// <param name="chart">
        /// Reference to the <see cref="ChartService"/> owner control.
        /// </param>
        override internal void Paint(ChartService chart, ChartGraphics graphics)
        {
            // Get annotation position in relative coordinates
            GetRelativePosition(out SKPoint firstPoint, out SKSize size, out _);
            SKPoint secondPoint = new(firstPoint.X + size.Width, firstPoint.Y + size.Height);

            // Create selection rectangle
            SKRect selectionRect = new(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Get position
            SKRect rectanglePosition = new(selectionRect.Left, selectionRect.Top, selectionRect.Right, selectionRect.Bottom);
            if (rectanglePosition.Width < 0)
            {
                rectanglePosition.Left = rectanglePosition.Right;
                rectanglePosition.Right = -rectanglePosition.Width;
            }
            if (rectanglePosition.Height < 0)
            {
                rectanglePosition.Top = rectanglePosition.Bottom;
                rectanglePosition.Bottom = -rectanglePosition.Height;
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
                // Draw "empty" image at design time
                if (_imageName.Length == 0)
                {
                    graphics.FillRectangleRel(
                        rectanglePosition,
                        BackColor,
                        BackHatchStyle,
                        _imageName,
                        _imageWrapMode,
                        _imageTransparentColor,
                        GetImageAlignment(Alignment),
                        BackGradientStyle,
                        BackSecondaryColor,
                        LineColor,
                        LineWidth,
                        LineDashStyle,
                        ShadowColor,
                        ShadowOffset,
                        PenAlignment.Center);

                    // Draw text
                    using SKPaint textBrush = new() { Style = SKPaintStyle.Fill, Color = ForeColor };
                    using StringFormat format = new(StringFormat.GenericTypographic);
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    format.FormatFlags = StringFormatFlags.LineLimit;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    graphics.DrawStringRel(
                        "(no image)",
                        Font,
                        textBrush,
                        rectanglePosition,
                        format);
                }
                else
                {
                    // Draw image
                    graphics.FillRectangleRel(
                        rectanglePosition,
                        SKColors.Transparent,
                        BackHatchStyle,
                        _imageName,
                        _imageWrapMode,
                        _imageTransparentColor,
                        GetImageAlignment(Alignment),
                        BackGradientStyle,
                        SKColors.Transparent,
                        SKColors.Transparent,
                        0,
                        LineDashStyle,
                        ShadowColor,
                        ShadowOffset,
                        PenAlignment.Center);
                }
            }

            if (Common.ProcessModeRegions)
            {
                // Add hot region
                Common.HotRegionsList.AddHotRegion(
                    rectanglePosition,
                    ReplaceKeywords(ToolTip),
                    String.Empty,
                    String.Empty,
                    String.Empty,
                    this,
                    ChartElementType.Annotation,
                    String.Empty);
            }

            // Paint selection handles
            PaintSelectionHandles(graphics, selectionRect, null);
        }

        /// <summary>
        /// Coverts ContentAlignment enumeration to ChartImageAlignmentStyle enumeration.
        /// </summary>
        /// <param name="alignment">Content alignment.</param>
        /// <returns>Image content alignment.</returns>
        private static ChartImageAlignmentStyle GetImageAlignment(ContentAlignment alignment)
        {
            if (alignment == ContentAlignment.TopLeft)
            {
                return ChartImageAlignmentStyle.TopLeft;
            }
            else if (alignment == ContentAlignment.TopCenter)
            {
                return ChartImageAlignmentStyle.Top;
            }
            else if (alignment == ContentAlignment.TopRight)
            {
                return ChartImageAlignmentStyle.TopRight;
            }
            else if (alignment == ContentAlignment.MiddleRight)
            {
                return ChartImageAlignmentStyle.Right;
            }
            else if (alignment == ContentAlignment.BottomRight)
            {
                return ChartImageAlignmentStyle.BottomRight;
            }
            else if (alignment == ContentAlignment.BottomCenter)
            {
                return ChartImageAlignmentStyle.Bottom;
            }
            else if (alignment == ContentAlignment.BottomLeft)
            {
                return ChartImageAlignmentStyle.BottomLeft;
            }
            else if (alignment == ContentAlignment.MiddleLeft)
            {
                return ChartImageAlignmentStyle.Left;
            }
            return ChartImageAlignmentStyle.Center;
        }

        #endregion // Painting

        #region Content Size

        /// <summary>
        /// Gets text annotation content size based on the text and font.
        /// </summary>
        /// <returns>Annotation content position.</returns>
        override internal SKRect GetContentPosition()
        {
            // Check image size
            if (Image.Length > 0)
            {
                // Try loading image and getting its size
                try
                {
                    if (Chart != null)
                    {
                        ImageLoader imageLoader = Common.ImageLoader;

                        if (imageLoader != null)
                        {
                            ChartGraphics chartGraphics = GetGraphics();

                            if (chartGraphics != null)
                            {
                                SKSize absSize = new();

                                if (imageLoader.GetAdjustedImageSize(Image, chartGraphics.Graphics, ref absSize))
                                {
                                    SKSize imageSize = chartGraphics.GetRelativeSize(absSize);
                                    return new SKRect(float.NaN, float.NaN, imageSize.Width, imageSize.Height);
                                }
                            }
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // ArgumentException is thrown by LoadImage in certain situations when it can't load the image
                }
            }

            return new SKRect(float.NaN, float.NaN, float.NaN, float.NaN);
        }

        #endregion

        #endregion
    }
}
