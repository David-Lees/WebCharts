// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Titles can be added to the chart by simply including 
//              those titles into the Titles collection, which is 
//              found in the root Chart object. The Title object 
//              incorporates several properties that can be used to 
//              position, dock, and control the appearance of any 
//              Title. Title positioning can be explicitly set, or 
//              you can specify that your title be docked. The 
//              charting control gives you full control over all of 
//              the appearance properties of your Titles, so you have 
//              the ability to set specific properties for such things 
//              as fonts, or colors, and even text effects. 
//              :
// NOTE: In early versions of the Chart control only 1 title was 
// exposed through the Title, TitleFont and TitleFontColor properties 
// in the root chart object. Due to the customer requests, support for 
// unlimited number of titles was added through the TitleCollection 
// exposed as a Titles property of the root chart object. Old 
// properties were deprecated and marked as non-browsable. 
//


using SkiaSharp;
using System;
using System.Globalization;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.General
{
    #region Title enumerations

    /// <summary>
    /// An enumeration of chart element docking styles.
    /// </summary>
    public enum Docking
    {
        /// <summary>
        /// Docked to the top.
        /// </summary>
        Top,

        /// <summary>
        /// Docked to the right.
        /// </summary>
        Right,

        /// <summary>
        /// Docked to the bottom.
        /// </summary>
        Bottom,

        /// <summary>
        /// Docked to the left.
        /// </summary>
        Left,
    };

    /// <summary>
    /// Text drawing styles.
    /// </summary>
    public enum TextStyle
    {
        /// <summary>
        /// Default text drawing style.
        /// </summary>
        Default,

        /// <summary>
        /// Shadow text.
        /// </summary>
        Shadow,

        /// <summary>
        /// Emboss text.
        /// </summary>
        Emboss,

        /// <summary>
        /// Embed text.
        /// </summary>
        Embed,

        /// <summary>
        /// Frame text.
        /// </summary>
        Frame
    }


    /// <summary>
    /// An enumeration of chart text orientation.
    /// </summary>
    public enum TextOrientation
    {
        /// <summary>
        /// Orientation is automatically determined based on the type of the 
        /// chart element it is used in.
        /// </summary>
        Auto,

        /// <summary>
        /// Horizontal text.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Text rotated 90 degrees and oriented from top to bottom.
        /// </summary>
        Rotated90,

        /// <summary>
        /// Text rotated 270 degrees and oriented from bottom to top.
        /// </summary>
        Rotated270,

        /// <summary>
        /// Text characters are not rotated and position one below the other.
        /// </summary>
        Stacked
    }

    #endregion

    /// <summary>
    /// The Title class provides properties which define content, visual 
    /// appearance and position of the single chart title. It also 
    /// contains methods responsible for calculating title position, 
    /// drawing and hit testing.
    /// </summary>
    [
    SRDescription("DescriptionAttributeTitle5"),
    ]
    public class Title : ChartNamedElement, IDisposable
    {
        #region Fields

        // Spacing between title text and the border in pixels
        internal int titleBorderSpacing = 4;


        //***********************************************************
        //** Private data members, which store properties values
        //***********************************************************

        // Title text
        private string _text = String.Empty;

        // Title drawing style
        private TextStyle _style = TextStyle.Default;

        // Title position
        private ElementPosition _position = null;

        // Background properties
        private bool _visible = true;
        private SKColor _backColor = SKColor.Empty;
        private ChartHatchStyle _backHatchStyle = ChartHatchStyle.None;
        private string _backImage = "";
        private ChartImageWrapMode _backImageWrapMode = ChartImageWrapMode.Tile;
        private SKColor _backImageTransparentColor = SKColor.Empty;
        private ChartImageAlignmentStyle _backImageAlignment = ChartImageAlignmentStyle.TopLeft;
        private GradientStyle _backGradientStyle = GradientStyle.None;
        private SKColor _backSecondaryColor = SKColor.Empty;
        private int _shadowOffset = 0;
        private SKColor _shadowColor = SKColor.Parse("00000080");

        // Border properties
        private SKColor _borderColor = SKColor.Empty;
        private int _borderWidth = 1;
        private ChartDashStyle _borderDashStyle = ChartDashStyle.Solid;

        // Font properties
        private FontCache _fontCache = new();
        private SKFont _font;
        private SKColor _foreColor = SKColors.Black;

        // Docking and Alignment properties
        private ContentAlignment _alignment = ContentAlignment.MiddleCenter;
        private Docking _docking = Docking.Top;
        private string _dockedToChartArea = Constants.NotSetValue;
        private bool _isDockedInsideChartArea = true;
        private int _dockingOffset = 0;

        // Interactive properties
        private string _toolTip = String.Empty;


        // Default text orientation
        private TextOrientation _textOrientation = TextOrientation.Auto;

        #endregion

        #region Constructors and Initialization

        /// <summary>
        /// Title constructor.
        /// </summary>
        public Title()
        {
            Initialize(string.Empty, Docking.Top, null, SKColors.Black);
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="text">Title text.</param>
        public Title(string text)
        {
            Initialize(text, Docking.Top, null, SKColors.Black);
        }

        /// <summary>
        /// Title constructor.
        /// </summary>
        /// <param name="text">Title text.</param>
        /// <param name="docking">Title docking.</param>
        public Title(string text, Docking docking)
        {
            Initialize(text, docking, null, SKColors.Black);
        }

        /// <summary>
        /// Title constructor.
        /// </summary>
        /// <param name="text">Title text.</param>
        /// <param name="docking">Title docking.</param>
        /// <param name="font">Title font.</param>
        /// <param name="color">Title color.</param>
        public Title(string text, Docking docking, SKFont font, SKColor color)
        {
            Initialize(text, docking, font, color);
        }

        /// <summary>
        /// Initialize title object.
        /// </summary>
        /// <param name="text">Title text.</param>
        /// <param name="docking">Title docking.</param>
        /// <param name="font">Title font.</param>
        /// <param name="color">Title color.</param>
        private void Initialize(string text, Docking docking, SKFont font, SKColor color)
        {
            // Initialize fields
            _position = new ElementPosition(this);
            _font = _fontCache.DefaultFont;
            _text = text;
            _docking = docking;
            _foreColor = color;
            if (font != null)
            {
                _font = font;
            }
        }

        #endregion

        #region	Properties

        /// <summary>
        /// Gets or sets the unique name of a ChartArea object.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),

        SRDescription("DescriptionAttributeTitle_Name"),

        ]
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the text orientation.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttribute_TextOrientation"),
        ]
        public TextOrientation TextOrientation
        {
            get
            {
                return _textOrientation;
            }
            set
            {
                _textOrientation = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets a flag that specifies whether the title is visible.
        /// </summary>
        /// <value>
        /// <b>True</b> if the title is visible; <b>false</b> otherwise.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTitle_Visible"),
        ]
        virtual public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the chart area name which the title is docked to inside or outside.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeTitle_DockToChartArea"),
        ]
        public string DockedToChartArea
        {
            get
            {
                return _dockedToChartArea;
            }
            set
            {
                if (value != _dockedToChartArea)
                {
                    if (value.Length == 0)
                    {
                        _dockedToChartArea = Constants.NotSetValue;
                    }
                    else
                    {
                        if (Chart != null && Chart.ChartAreas != null)
                        {
                            Chart.ChartAreas.VerifyNameReference(value);
                        }
                        _dockedToChartArea = value;
                    }
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a property which indicates whether the title is docked inside chart area. 
        /// DockedToChartArea property must be set first.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeTitle_DockInsideChartArea"),
        ]
        public bool IsDockedInsideChartArea
        {
            get
            {
                return _isDockedInsideChartArea;
            }
            set
            {
                if (value != _isDockedInsideChartArea)
                {
                    _isDockedInsideChartArea = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the positive or negative offset of the docked title position.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeTitle_DockOffset"),
        ]
        public int DockingOffset
        {
            get
            {
                return _dockingOffset;
            }
            set
            {
                if (value != _dockingOffset)
                {
                    if (value < -100 || value > 100)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionValueMustBeInRange(nameof(DockingOffset), (-100).ToString(CultureInfo.CurrentCulture), 100.ToString(CultureInfo.CurrentCulture)));
                    }
                    _dockingOffset = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the position of the title.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTitle_Position"),
        ]
        public ElementPosition Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                _position.Parent = this;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Determoines if this position should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializePosition()
        {
            return !Position.Auto;
        }


        /// <summary>
        /// Gets or sets the text of the title.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTitle_Text"),
        ]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value ?? string.Empty;
                Invalidate(false);
            }
        }


        /// <summary>
        /// Title drawing style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTextStyle"),
        ]
        public TextStyle TextStyle
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the background color of the title.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        public SKColor BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the border color of the title.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderColor"),
        ]
        public SKColor BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                _borderColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the border style of the title.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderDashStyle"),
        ]
        public ChartDashStyle BorderDashStyle
        {
            get
            {
                return _borderDashStyle;
            }
            set
            {
                _borderDashStyle = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the border width of the title.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderWidth"),
        ]
        public int BorderWidth
        {
            get
            {
                return _borderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionTitleBorderWidthIsNegative));
                }
                _borderWidth = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the background image.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImage"),
        ]
        public string BackImage
        {
            get
            {
                return _backImage;
            }
            set
            {
                _backImage = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the background image drawing mode.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageWrapMode"),
        ]
        public ChartImageWrapMode BackImageWrapMode
        {
            get
            {
                return _backImageWrapMode;
            }
            set
            {
                _backImageWrapMode = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the background image.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        ]
        public SKColor BackImageTransparentColor
        {
            get
            {
                return _backImageTransparentColor;
            }
            set
            {
                _backImageTransparentColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the background image alignment used by unscale drawing mode.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImageAlign"),
        ]
        public ChartImageAlignmentStyle BackImageAlignment
        {
            get
            {
                return _backImageAlignment;
            }
            set
            {
                _backImageAlignment = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the background gradient style.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle"),
        ]
        public GradientStyle BackGradientStyle
        {
            get
            {
                return _backGradientStyle;
            }
            set
            {
                _backGradientStyle = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the secondary background color.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of a background with 
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
        public SKColor BackSecondaryColor
        {
            get
            {
                return _backSecondaryColor;
            }
            set
            {
                _backSecondaryColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the background hatch style.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackHatchStyle"),
        ]
        public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return _backHatchStyle;
            }
            set
            {
                _backHatchStyle = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the title font.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTitle_Font"),

        ]
        public SKFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the title fore color.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTitle_Color"),
        ]
        public SKColor ForeColor
        {
            get
            {
                return _foreColor;
            }
            set
            {
                _foreColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets title alignment.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeTitle_Alignment"),
        ]
        public ContentAlignment Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the title docking style.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeTitle_Docking"),
        ]
        public Docking Docking
        {
            get
            {
                return _docking;
            }
            set
            {
                _docking = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the title shadow offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeShadowOffset"),
        ]
        public int ShadowOffset
        {
            get
            {
                return _shadowOffset;
            }
            set
            {
                _shadowOffset = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the title shadow color.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeShadowColor"),
        ]
        public SKColor ShadowColor
        {
            get
            {
                return _shadowColor;
            }
            set
            {
                _shadowColor = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        [
        SRCategory("CategoryAttributeToolTip"),
        SRDescription("DescriptionAttributeToolTip"),
        ]
        public string ToolTip
        {
            set
            {
                _toolTip = value;
            }
            get
            {
                return _toolTip;
            }
        }

        /// <summary>
		/// True if title background or border is visible
		/// </summary>
		internal bool BackGroundIsVisible
        {
            get
            {
                if (!(BackColor == SKColor.Empty) ||
                    BackImage.Length > 0 ||
                    (!(BorderColor == SKColor.Empty) && BorderDashStyle != ChartDashStyle.NotSet))
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if chart title is drawn vertically.
        /// Note: From the drawing perspective stacked text orientation is not vertical.
        /// </summary>
        /// <returns>True if text is vertical.</returns>
        private bool IsTextVertical
        {
            get
            {
                TextOrientation currentTextOrientation = GetTextOrientation();
                return currentTextOrientation == TextOrientation.Rotated90 || currentTextOrientation == TextOrientation.Rotated270;
            }
        }

        /// <summary>
        /// Returns title text orientation. If set to Auto automatically determines the
        /// orientation based on title docking.
        /// </summary>
        /// <returns>Current text orientation.</returns>
        private TextOrientation GetTextOrientation()
        {
            if (TextOrientation == TextOrientation.Auto)
            {
                // When chart title is docked to the left or right we automatically 
                // set vertical text with different rotation angles.
                if (Position.Auto)
                {
                    if (Docking == Docking.Left)
                    {
                        return TextOrientation.Rotated270;
                    }
                    else if (Docking == Docking.Right)
                    {
                        return TextOrientation.Rotated90;
                    }
                }
                return TextOrientation.Horizontal;
            }
            return TextOrientation;
        }

        /// <summary>
        /// Helper method that checks if title is visible.
        /// </summary>
        /// <returns>True if title is visible.</returns>
        internal bool IsVisible()
        {
            if (Visible)
            {

                // Check if title is docked to the chart area
                if (DockedToChartArea.Length > 0 &&
                    Chart != null)
                {
                    if (Chart.ChartAreas.IndexOf(DockedToChartArea) >= 0)
                    {
                        // Do not show title when it is docked to invisible chart area
                        ChartArea area = Chart.ChartAreas[DockedToChartArea];
                        if (!area.Visible)
                        {
                            return false;
                        }
                    }
                }


                return true;
            }
            return false;
        }

        /// <summary>
        /// Invalidate chart title when one of the properties is changed.
        /// </summary>
        /// <param name="invalidateTitleOnly">Indicates that only title area should be invalidated.</param>
        internal void Invalidate(bool invalidateTitleOnly)
        {
            if (Chart != null)
            {
                // Set dirty flag
                Chart.dirtyFlag = true;

                // Invalidate chart
                if (invalidateTitleOnly)
                {
                    // Calculate the position of the title
                    SKRect invalRect = new(); // TODO this does nothing;
                    if (Position.Width != 0 && Position.Height != 0)
                    {
                        // Convert relative coordinates to absolute coordinates
                        invalRect.Left = (int)(Position.X * (Common.ChartPicture.Width - 1) / 100F);
                        invalRect.Top = (int)(Position.Y * (Common.ChartPicture.Height - 1) / 100F);
                        invalRect.Right = invalRect.Left + (int)(Position.Width * (Common.ChartPicture.Width - 1) / 100F);
                        invalRect.Bottom = invalRect.Top + (int)(Position.Height * (Common.ChartPicture.Height - 1) / 100F);

                        // Inflate rectangle size using border size and shadow size
                        invalRect.Inflate(BorderWidth + ShadowOffset + 1, BorderWidth + ShadowOffset + 1);
                    }

                    // Invalidate title rectangle only
                    Chart.Invalidate(/*invalRect*/);
                }
                else
                {
                    Invalidate();
                }
            }
        }

        #endregion

        #region Painting and Selection Methods

        /// <summary>
        /// Paints title using chart graphics object.
        /// </summary>
        /// <param name="chartGraph">The graph provides drawing object to the display device. A Graphics object is associated with a specific device context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        internal void Paint(ChartGraphics chartGraph)
        {
            // check if title is visible
            if (!IsVisible())
            {
                return;
            }

            // Title text
            string titleText = Text;

            //***************************************************************
            //** Calculate title relative position
            //***************************************************************
            SKRect titlePosition = Position.ToSKRect();

            // Auto set the title position if width or height is not set for custom position
            if (!Position.Auto && Common != null && Common.ChartPicture != null)
            {
                if (titlePosition.Width == 0 || titlePosition.Height == 0)
                {
                    // Calculate text layout area
                    SKSize layoutArea = new(
                        (titlePosition.Width == 0) ? Common.ChartPicture.Width : titlePosition.Width,
                        (titlePosition.Height == 0) ? Common.ChartPicture.Height : titlePosition.Height);
                    if (IsTextVertical)
                    {
                        float tempValue = layoutArea.Width;
                        layoutArea.Width = layoutArea.Height;
                        layoutArea.Height = tempValue;
                    }

                    // Measure text size
                    layoutArea = chartGraph.GetAbsoluteSize(layoutArea);
                    SKSize titleSize = chartGraph.MeasureString(
                        "W" + titleText.Replace("\\n", "\n"),
                        Font,
                        layoutArea,
                        StringFormat.GenericDefault,
                        GetTextOrientation());

                    // Increase text size by 4 pixels
                    if (BackGroundIsVisible)
                    {
                        titleSize.Width += titleBorderSpacing;
                        titleSize.Height += titleBorderSpacing;
                    }

                    // Switch width and height for vertical text
                    if (IsTextVertical)
                    {
                        float tempValue = titleSize.Width;
                        titleSize.Width = titleSize.Height;
                        titleSize.Height = tempValue;
                    }

                    // Convert text size to relative coordinates
                    titleSize = chartGraph.GetRelativeSize(titleSize);

                    // Update custom position
                    if (titlePosition.Width == 0)
                    {
                        titlePosition.Size = new(titleSize.Width, titlePosition.Height);
                        if (Alignment == ContentAlignment.BottomRight ||
                            Alignment == ContentAlignment.MiddleRight ||
                            Alignment == ContentAlignment.TopRight)
                        {
                            titlePosition.Left -= titlePosition.Width;
                        }
                        else if (Alignment == ContentAlignment.BottomCenter ||
                            Alignment == ContentAlignment.MiddleCenter ||
                            Alignment == ContentAlignment.TopCenter)
                        {
                            titlePosition.Left -= titlePosition.Width / 2f;
                        }
                    }
                    if (titlePosition.Height == 0)
                    {
                        titlePosition.Bottom = titlePosition.Top + titleSize.Height;
                        if (Alignment == ContentAlignment.BottomRight ||
                            Alignment == ContentAlignment.BottomCenter ||
                            Alignment == ContentAlignment.BottomLeft)
                        {
                            titlePosition.Top -= titlePosition.Height;
                        }
                        else if (Alignment == ContentAlignment.MiddleCenter ||
                            Alignment == ContentAlignment.MiddleLeft ||
                            Alignment == ContentAlignment.MiddleRight)
                        {
                            titlePosition.Top -= titlePosition.Height / 2f;
                        }
                    }

                }
            }

            //***************************************************************
            //** Convert title position to absolute coordinates
            //***************************************************************
            SKRect absPosition = new(titlePosition.Left, titlePosition.Top, titlePosition.Right, titlePosition.Bottom);
            absPosition = chartGraph.GetAbsoluteRectangle(absPosition);

            //***************************************************************
            //** Draw title background, border and shadow
            //***************************************************************
            if (BackGroundIsVisible && Common.ProcessModePaint)
            {
                chartGraph.FillRectangleRel(titlePosition,
                    BackColor,
                    BackHatchStyle,
                    BackImage,
                    BackImageWrapMode,
                    BackImageTransparentColor,
                    BackImageAlignment,
                    BackGradientStyle,
                    BackSecondaryColor,
                    BorderColor,
                    BorderWidth,
                    BorderDashStyle,
                    ShadowColor,
                    ShadowOffset,
                    PenAlignment.Inset);
            }
            else
            {
                // Adjust text position to be only around the text itself
                SKSize titleArea = chartGraph.GetAbsoluteSize(titlePosition.Size);
                SKSize titleSize = chartGraph.MeasureString(
                    "W" + titleText.Replace("\\n", "\n"),
                    Font,
                    titleArea,
                    StringFormat.GenericDefault,
                    GetTextOrientation());

                // Convert text size to relative coordinates
                titleSize = chartGraph.GetRelativeSize(titleSize);

                // Adjust position depending on alignment
                SKRect exactTitleRect = new(
                    titlePosition.Left,
                    titlePosition.Top,
                    titlePosition.Left + titleSize.Width,
                    titlePosition.Top + titleSize.Height);
                if (Alignment == ContentAlignment.BottomCenter ||
                    Alignment == ContentAlignment.BottomLeft ||
                    Alignment == ContentAlignment.BottomRight)
                {
                    exactTitleRect.Top = titlePosition.Bottom - exactTitleRect.Height;
                }
                else if (Alignment == ContentAlignment.MiddleCenter ||
                    Alignment == ContentAlignment.MiddleLeft ||
                    Alignment == ContentAlignment.MiddleRight)
                {
                    exactTitleRect.Top = titlePosition.Top +
                        titlePosition.Height / 2f -
                        exactTitleRect.Height / 2f;
                }

                if (Alignment == ContentAlignment.BottomRight ||
                    Alignment == ContentAlignment.MiddleRight ||
                    Alignment == ContentAlignment.TopRight)
                {
                    exactTitleRect.Left = titlePosition.Right - exactTitleRect.Width;
                }
                else if (Alignment == ContentAlignment.BottomCenter ||
                    Alignment == ContentAlignment.MiddleCenter ||
                    Alignment == ContentAlignment.TopCenter)
                {
                    exactTitleRect.Left = titlePosition.Left +
                        titlePosition.Width / 2f -
                        exactTitleRect.Width / 2f;
                }

                // NOTE: This approach for text selection can not be used with
                // Flash animations because of the bug in Flash viewer. When the 
                // button shape is placed in the last frame the Alpha value of the
                // color is ignored.

                // NOTE: Feature tested again with Flash Player 7 and it seems to be 
                // working fine. Code below is commented to enable selection in flash
                // through transparent rectangle.
                // Fixes issue #4172.

                bool drawRect = true;

                // Draw transparent rectangle in the text position
                if (drawRect)
                {
                    chartGraph.FillRectangleRel(
                        exactTitleRect,
                        SKColors.White,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        BackImageTransparentColor,
                        BackImageAlignment,
                        GradientStyle.None,
                        BackSecondaryColor,
                        SKColors.Transparent,
                        0,
                        BorderDashStyle,
                        SKColors.Transparent,
                        0,
                        PenAlignment.Inset);
                }
            }

            if (Common.ProcessModePaint)
                Common.Chart.CallOnPrePaint(new ChartPaintEventArgs(this, chartGraph, Common, Position));

            //***************************************************************
            //** Add spacing between text and border
            //***************************************************************
            if (BackGroundIsVisible)
            {
                absPosition.Right -= titleBorderSpacing;
                absPosition.Bottom -= titleBorderSpacing;
                absPosition.Left += titleBorderSpacing / 2f;
                absPosition.Top += titleBorderSpacing / 2f;
            }

            //***************************************************************
            //** Create string format
            //***************************************************************
            using StringFormat format = new();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            if (Alignment == ContentAlignment.BottomCenter ||
                Alignment == ContentAlignment.BottomLeft ||
                Alignment == ContentAlignment.BottomRight)
            {
                format.LineAlignment = StringAlignment.Far;
            }
            else if (Alignment == ContentAlignment.TopCenter ||
                Alignment == ContentAlignment.TopLeft ||
                Alignment == ContentAlignment.TopRight)
            {
                format.LineAlignment = StringAlignment.Near;
            }

            if (Alignment == ContentAlignment.BottomLeft ||
                Alignment == ContentAlignment.MiddleLeft ||
                Alignment == ContentAlignment.TopLeft)
            {
                format.Alignment = StringAlignment.Near;
            }
            else if (Alignment == ContentAlignment.BottomRight ||
                Alignment == ContentAlignment.MiddleRight ||
                Alignment == ContentAlignment.TopRight)
            {
                format.Alignment = StringAlignment.Far;
            }

            //***************************************************************
            //** Draw text shadow for the default style when background is not drawn anf ShadowOffset is not null
            //***************************************************************
            SKColor textShadowColor = ChartGraphics.GetGradientColor(ForeColor, SKColors.Black, 0.8);
            int textShadowOffset = 1;
            TextStyle textStyle = TextStyle;
            if ((textStyle == TextStyle.Default || textStyle == TextStyle.Shadow) &&
                !BackGroundIsVisible &&
                ShadowOffset != 0)
            {
                // Draw shadowed text
                textStyle = TextStyle.Shadow;
                textShadowColor = ShadowColor;
                textShadowOffset = ShadowOffset;
            }

            if (textStyle == TextStyle.Shadow)
            {
                textShadowColor = (textShadowColor.Alpha != 255) ? textShadowColor : Color.FromArgb((byte)(textShadowColor.Alpha / 2), textShadowColor);
            }

            //***************************************************************
            //** Replace new line characters
            //***************************************************************
            titleText = titleText.Replace("\\n", "\n");

            //***************************************************************
            //** Define text angle depending on the docking
            //***************************************************************
            SKMatrix oldTransform = SKMatrix.Empty;
            if (IsTextVertical)
            {
                if (GetTextOrientation() == TextOrientation.Rotated270)
                {
                    // IMPORTANT !
                    // Right to Left flag has to be used because of bug with .net with multi line vertical text. As soon as .net bug is fixed this flag HAS TO be removed. Bug number 1870.
                    format.FormatFlags |= StringFormatFlags.DirectionVertical | StringFormatFlags.DirectionRightToLeft;

                    // Save old graphics transformation
                    oldTransform = chartGraph.Transform;

                    // Rotate tile 180 degrees at center
                    SKPoint center = SKPoint.Empty;

                    center.X = absPosition.MidX;
                    center.Y = absPosition.MidY;

                    // Create and set new transformation matrix
                    var newMatrix = chartGraph.Transform;
                    newMatrix = SKMatrix.CreateRotationDegrees(180, center.X, center.Y);
                    chartGraph.Transform = newMatrix;
                }
                else if (GetTextOrientation() == TextOrientation.Rotated90)
                {
                    // IMPORTANT !
                    // Right to Left flag has to be used because of bug with .net with multi line vertical text. As soon as .net bug is fixed this flag HAS TO be removed. Bug number 1870.
                    format.FormatFlags |= StringFormatFlags.DirectionVertical | StringFormatFlags.DirectionRightToLeft;
                }
            }
            try
            {
                chartGraph.IsTextClipped = !Position.Auto;
                DrawStringWithStyle(chartGraph, titleText, textStyle, Font, absPosition, ForeColor, textShadowColor, textShadowOffset, format, GetTextOrientation());
            }
            finally
            {
                chartGraph.IsTextClipped = false;
            }
            // Call Paint event
            if (Common.ProcessModePaint)
                Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, chartGraph, Common, Position));

            //***************************************************************
            //** Restore old transformation
            //***************************************************************
            if (oldTransform != SKMatrix.Empty)
            {
                chartGraph.Transform = oldTransform;
            }

            if (Common.ProcessModeRegions)
            {
                Common.HotRegionsList.AddHotRegion(titlePosition, ToolTip, null, null, null, this, ChartElementType.Title, null);
            }
        }

        /// <summary>
        /// Draws the string with style.
        /// </summary>
        /// <param name="chartGraph">The chart graph.</param>
        /// <param name="titleText">The title text.</param>
        /// <param name="textStyle">The text style.</param>
        /// <param name="font">The font.</param>
        /// <param name="absPosition">The abs position.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <param name="shadowColor">Color of the shadow.</param>
        /// <param name="shadowOffset">The shadow offset.</param>
        /// <param name="format">The format.</param>
        /// <param name="orientation">The orientation.</param>
        internal static void DrawStringWithStyle(
            ChartGraphics chartGraph,
            string titleText,
            TextStyle textStyle,
            SKFont font,
            SKRect absPosition,
            SKColor foreColor,
            SKColor shadowColor,
            int shadowOffset,
            StringFormat format,
            TextOrientation orientation
            )
        {
            //***************************************************************
            //** Draw title text
            //***************************************************************
            if (titleText.Length > 0)
            {
                if (textStyle == TextStyle.Default)
                {
                    using SKPaint brush = new() { Color = foreColor };
                    chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                }
                else if (textStyle == TextStyle.Frame)
                {
                    using SKPaint pen = new() { Color = foreColor, StrokeWidth = 1 };
                    chartGraph.DrawString(titleText, font, pen, absPosition, format, orientation);
                }
                else if (textStyle == TextStyle.Embed)
                {
                    // Draw shadow
                    SKRect shadowPosition = new(absPosition.Left, absPosition.Top, absPosition.Right, absPosition.Bottom);
                    shadowPosition.Left -= 1;
                    shadowPosition.Top -= 1;
                    using (SKPaint brush = new() { Color = shadowColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw highlighting
                    shadowPosition.Left += 2;
                    shadowPosition.Top += 2;
                    SKColor texthighlightColor = ChartGraphics.GetGradientColor(SKColors.White, foreColor, 0.3);
                    using (SKPaint brush = new() { Color = texthighlightColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    using (SKPaint brush = new() { Color = foreColor })
                    {
                        // Draw text
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }
                }
                else if (textStyle == TextStyle.Emboss)
                {
                    // Draw shadow
                    SKRect shadowPosition = new(absPosition.Left, absPosition.Top, absPosition.Right, absPosition.Bottom);
                    shadowPosition.Left += 1;
                    shadowPosition.Top += 1;
                    using (SKPaint brush = new() { Color = shadowColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw highlighting
                    shadowPosition.Left -= 2;
                    shadowPosition.Top -= 2;
                    SKColor texthighlightColor = ChartGraphics.GetGradientColor(SKColors.White, foreColor, 0.3);
                    using (SKPaint brush = new() { Color = texthighlightColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw text
                    using (SKPaint brush = new() { Color = foreColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }

                }
                else if (textStyle == TextStyle.Shadow)
                {
                    // Draw shadow
                    SKRect shadowPosition = new(absPosition.Left, absPosition.Top, absPosition.Right, absPosition.Bottom);
                    shadowPosition.Left += shadowOffset;
                    shadowPosition.Top += shadowOffset;
                    using (SKPaint brush = new() { Color = shadowColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw text
                    using (SKPaint brush = new() { Color = foreColor })
                    {
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }
                }
            }
        }

        #endregion

        #region Position Calculation Methods

        /// <summary>
        /// Recalculates title position.
        /// </summary>
        /// <param name="chartGraph">Chart graphics used.</param>
        /// <param name="chartAreasRectangle">Area where the title should be docked.</param>
        /// <param name="frameTitlePosition">Position of the title in the frame.</param>
        /// <param name="elementSpacing">Spacing size in percentage of the area.</param>
        internal void CalcTitlePosition(
            ChartGraphics chartGraph,
            ref SKRect chartAreasRectangle,
            ref SKRect frameTitlePosition,
            float elementSpacing)
        {
            // Special case for the first title docked to the top when the title frame is used
            if (!frameTitlePosition.IsEmpty &&
                Position.Auto &&
                Docking == Docking.Top &&
                DockedToChartArea == Constants.NotSetValue)
            {
                Position.SetPositionNoAuto(
                    frameTitlePosition.Left + elementSpacing,
                    frameTitlePosition.Top,
                    frameTitlePosition.Width - 2f * elementSpacing,
                    frameTitlePosition.Height);
                frameTitlePosition = SKRect.Empty;
                return;
            }

            // Get title size
            SKRect titlePosition = new();
            SKSize layoutArea = new(chartAreasRectangle.Width, chartAreasRectangle.Height);

            // Switch width and height for vertical text
            if (IsTextVertical)
            {
                float tempValue = layoutArea.Width;
                layoutArea.Width = layoutArea.Height;
                layoutArea.Height = tempValue;
            }

            // Meausure text size
            layoutArea.Width -= 2f * elementSpacing;
            layoutArea.Height -= 2f * elementSpacing;
            layoutArea = chartGraph.GetAbsoluteSize(layoutArea);
            SKSize titleSize = chartGraph.MeasureString(
                "W" + Text.Replace("\\n", "\n"),
                Font,
                layoutArea,
                StringFormat.GenericDefault,
                GetTextOrientation());

            // Increase text size by 4 pixels
            if (BackGroundIsVisible)
            {
                titleSize.Width += titleBorderSpacing;
                titleSize.Height += titleBorderSpacing;
            }

            // Switch width and height for vertical text
            if (IsTextVertical)
            {
                float tempValue = titleSize.Width;
                titleSize.Width = titleSize.Height;
                titleSize.Height = tempValue;
            }

            // Convert text size to relative coordinates
            titleSize = chartGraph.GetRelativeSize(titleSize);
            titlePosition.Size = new(titleSize.Width, titleSize.Height);
            if (float.IsNaN(titleSize.Height) || float.IsNaN(titleSize.Width))
            {
                return;
            }

            // Calculate title position
            if (Docking == Docking.Top)
            {
                titlePosition.Top = chartAreasRectangle.Top + elementSpacing;
                titlePosition.Left = chartAreasRectangle.Left + elementSpacing;
                titlePosition.Right = titlePosition.Left + chartAreasRectangle.Right - titlePosition.Left - elementSpacing;
                if (titlePosition.Width < 0)
                {
                    titlePosition.Right = titlePosition.Left;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Bottom -= titlePosition.Height + elementSpacing;
                chartAreasRectangle.Top = titlePosition.Bottom;
            }
            else if (Docking == Docking.Bottom)
            {
                titlePosition.Top = chartAreasRectangle.Bottom - titleSize.Height - elementSpacing;
                titlePosition.Left = chartAreasRectangle.Left + elementSpacing;
                titlePosition.Right = titlePosition.Left + chartAreasRectangle.Right - titlePosition.Left - elementSpacing;
                if (titlePosition.Width < 0)
                {
                    titlePosition.Right = titlePosition.Left;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Bottom -= titlePosition.Height + elementSpacing;
            }
            if (Docking == Docking.Left)
            {
                titlePosition.Left = chartAreasRectangle.Left + elementSpacing;
                titlePosition.Top = chartAreasRectangle.Top + elementSpacing;
                titlePosition.Bottom = titlePosition.Top + chartAreasRectangle.Bottom - titlePosition.Top - elementSpacing;
                if (titlePosition.Height < 0)
                {
                    titlePosition.Bottom = titlePosition.Top;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Right -= titlePosition.Width + elementSpacing;
                chartAreasRectangle.Left = titlePosition.Right;
            }
            if (Docking == Docking.Right)
            {
                titlePosition.Left = chartAreasRectangle.Right - titleSize.Width - elementSpacing;
                titlePosition.Top = chartAreasRectangle.Top + elementSpacing;
                titlePosition.Bottom = titlePosition.Top + chartAreasRectangle.Bottom - titlePosition.Top - elementSpacing;
                if (titlePosition.Height < 0)
                {
                    titlePosition.Bottom = titlePosition.Top;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Right -= titlePosition.Width + elementSpacing;
            }


            // Offset calculated docking position
            if (DockingOffset != 0)
            {
                if (Docking == Docking.Top || Docking == Docking.Bottom)
                {
                    titlePosition.Top += DockingOffset;
                    titlePosition.Bottom += DockingOffset;
                }
                else
                {
                    titlePosition.Left += DockingOffset;
                    titlePosition.Right += DockingOffset;
                }
            }

            Position.SetPositionNoAuto(titlePosition.Left, titlePosition.Top, titlePosition.Width, titlePosition.Height);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_fontCache != null)
                {
                    _fontCache.Dispose();
                    _fontCache = null;
                }
                if (_position != null)
                {
                    _position.Dispose();
                    _position = null;
                }
            }
        }


        #endregion
    }

    /// <summary>
    /// The TitleCollection class is a strongly typed collection of Title classes.
    /// Indexer of this collection can take the title index (integer) or unique 
    /// title name (string) as a parameter.
    /// </summary>
    [
        SRDescription("DescriptionAttributeTitles"),
    ]
    public class TitleCollection : ChartNamedElementCollection<Title>
    {

        #region Constructors

        /// <summary>
        /// TitleCollection constructor.
        /// </summary>
        /// <param name="parent">Parent chart element.</param>
        internal TitleCollection(IChartElement parent)
            : base(parent)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new Title with the specified name and adds it to the collection.
        /// </summary>
        /// <param name="name">The new chart area name.</param>
        /// <returns>New title</returns>
        public Title Add(string name)
        {
            Title title = new(name);
            Add(title);
            return title;
        }


        /// <summary>
        /// Recalculates title position in the collection for titles docked outside of chart area.
        /// </summary>
        /// <param name="chartPicture">Chart picture object.</param>
        /// <param name="chartGraph">Chart graphics used.</param>
        /// <param name="area">Area the title is docked to.</param>
        /// <param name="chartAreasRectangle">Area where the title should be positioned.</param>
        /// <param name="elementSpacing">Spacing size in percentage of the area.</param>
        internal static void CalcOutsideTitlePosition(
            ChartPicture chartPicture,
            ChartGraphics chartGraph,
            ChartArea area,
            ref SKRect chartAreasRectangle,
            float elementSpacing)
        {
            if (chartPicture != null)
            {
                // Get elemets spacing
                float areaSpacing = Math.Min((chartAreasRectangle.Height / 100F) * elementSpacing, (chartAreasRectangle.Width / 100F) * elementSpacing);

                // Loop through all titles
                foreach (Title title in chartPicture.Titles)
                {
                    // Check if title visible
                    if (!title.IsVisible())
                    {
                        continue;
                    }

                    // Check if all chart area names are valid
                    if (title.DockedToChartArea != Constants.NotSetValue && chartPicture.ChartAreas.IndexOf(title.DockedToChartArea) < 0)
                    {
                        throw (new ArgumentException(SR.ExceptionChartTitleDockedChartAreaIsMissing((string)title.DockedToChartArea)));
                    }

                    // Process only titles docked to specified area
                    if (title.IsDockedInsideChartArea == false &&
                        title.DockedToChartArea == area.Name &&
                        title.Position.Auto)
                    {
                        // Calculate title position
                        SKRect frameRect = SKRect.Empty;
                        SKRect prevChartAreasRectangle = chartAreasRectangle;
                        title.CalcTitlePosition(chartGraph,
                            ref chartAreasRectangle,
                            ref frameRect,
                            areaSpacing);

                        // Adjust title position
                        SKRect titlePosition = title.Position.ToSKRect();
                        if (title.Docking == Docking.Top)
                        {
                            titlePosition.Top -= areaSpacing;
                            if (!area.Position.Auto)
                            {
                                titlePosition.Top -= titlePosition.Height;
                                prevChartAreasRectangle.Top -= titlePosition.Height + areaSpacing;
                                prevChartAreasRectangle.Bottom += titlePosition.Height + areaSpacing;
                            }
                        }
                        else if (title.Docking == Docking.Bottom)
                        {
                            titlePosition.Top += areaSpacing;
                            if (!area.Position.Auto)
                            {
                                titlePosition.Top = prevChartAreasRectangle.Bottom + areaSpacing;
                                prevChartAreasRectangle.Bottom += titlePosition.Height + areaSpacing;
                            }
                        }
                        if (title.Docking == Docking.Left)
                        {
                            titlePosition.Left -= areaSpacing;
                            if (!area.Position.Auto)
                            {
                                titlePosition.Left -= titlePosition.Width;
                                prevChartAreasRectangle.Left -= titlePosition.Width + areaSpacing;
                                prevChartAreasRectangle.Right += titlePosition.Width + areaSpacing;
                            }
                        }
                        if (title.Docking == Docking.Right)
                        {
                            titlePosition.Left += areaSpacing;
                            if (!area.Position.Auto)
                            {
                                titlePosition.Left = prevChartAreasRectangle.Right + areaSpacing;
                                prevChartAreasRectangle.Right += titlePosition.Width + areaSpacing;
                            }
                        }

                        // Set title position without changing the 'Auto' flag
                        title.Position.SetPositionNoAuto(titlePosition.Left, titlePosition.Top, titlePosition.Width, titlePosition.Height);

                        // If custom position is used in the chart area reset the curent adjusted position
                        if (!area.Position.Auto)
                        {
                            chartAreasRectangle = prevChartAreasRectangle;
                        }

                    }
                }

            }
        }

        /// <summary>
        /// Recalculates all titles position inside chart area in the collection.
        /// </summary>
        /// <param name="chartPicture">Chart picture object.</param>
        /// <param name="chartGraph">Chart graphics used.</param>
        /// <param name="elementSpacing">Spacing size in percentage of the area.</param>
        internal static void CalcInsideTitlePosition(
            ChartPicture chartPicture,
            ChartGraphics chartGraph,
            float elementSpacing)
        {
            if (chartPicture != null)
            {
                // Check if all chart area names are valid
                foreach (Title title in chartPicture.Titles)
                {
                    // Check if title visible
                    if (!title.IsVisible())
                    {
                        continue;
                    }

                    if (title.DockedToChartArea != Constants.NotSetValue)
                    {
                        try
                        {
                            ChartArea area = chartPicture.ChartAreas[title.DockedToChartArea];
                        }
                        catch
                        {
                            throw (new ArgumentException(SR.ExceptionChartTitleDockedChartAreaIsMissing((string)title.DockedToChartArea)));
                        }
                    }
                }

                // Loop through all chart areas
                foreach (ChartArea area in chartPicture.ChartAreas)
                {

                    // Check if chart area is visible
                    if (area.Visible)

                    {
                        // Get area position
                        SKRect titlePlottingRectangle = area.PlotAreaPosition.ToSKRect();

                        // Get elemets spacing
                        float areaSpacing = Math.Min((titlePlottingRectangle.Height / 100F) * elementSpacing, (titlePlottingRectangle.Width / 100F) * elementSpacing);

                        // Loop through all titles
                        foreach (Title title in chartPicture.Titles)
                        {
                            if (title.IsDockedInsideChartArea == true &&
                                title.DockedToChartArea == area.Name &&
                                title.Position.Auto)
                            {
                                // Calculate title position
                                SKRect frameRect = SKRect.Empty;
                                title.CalcTitlePosition(chartGraph,
                                    ref titlePlottingRectangle,
                                    ref frameRect,
                                    areaSpacing);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Event handlers
        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            //If all the chart areas are removed and then the first one is added we don't want to dock the titles
            if (e.OldElement == null)
                return;

            foreach (Title title in this)
                if (title.DockedToChartArea == e.OldName)
                    title.DockedToChartArea = e.NewName;
        }
        #endregion


    }
}
