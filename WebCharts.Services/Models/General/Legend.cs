// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Chart Legend consist of default and custom legend
//              items. Default items are automatically added based
//              on the data series and custom items are added by
//              the user. Each item usually consist of 2 cells;
//              series color marker and series name. Legend item
//              cells form vertical columns in the legend.
//              Please refer to the Chart documentation which
//              contains images and samples describing legend features.
//  :
//  NOTE: In early versions of the Chart control only 1 legend was
//  exposed through the Legend property of the root chart object.
//  Due to the customer requests, support for unlimited number of
//  legends was added through the LegendCollection exposed as a
//  Legends property in the root chart object. Old propertys was
//  deprecated and marked as non-browsable.
//

using SkiaSharp;
using System;
using System.Collections.Generic;

namespace WebCharts.Services
{
    #region Legend enumerations

    /// <summary>
    /// An enumeration that specifies a style for a legend item's symbol.
    /// </summary>
    public enum LegendImageStyle
    {
        /// <summary>
        /// The symbol will be a rectangle.
        /// </summary>
        Rectangle,

        /// <summary>
        /// The symbol will be a line.
        /// </summary>
        Line,

        /// <summary>
        /// The symbol will be a marker.
        /// </summary>
        Marker
    }

    /// <summary>
    /// An enumeration of legend item orderings.
    /// </summary>
    public enum LegendItemOrder
    {
        /// <summary>
        /// Items will be added into the legend in an order automatically determined by the chart.
        /// </summary>
        Auto,

        /// <summary>
        /// Items will be added into the legend in the same order as the chart series.
        /// </summary>
        SameAsSeriesOrder,

        /// <summary>
        /// Items will be added into the legend in the same order as the chart series.
        /// </summary>
        ReversedSeriesOrder
    };

    /// <summary>
    /// An enumeration of legend separator styles.
    /// </summary>
    public enum LegendSeparatorStyle
    {
        /// <summary>
        /// No separator will be shown.
        /// </summary>
        None,

        /// <summary>
        /// Single solid line separator.
        /// </summary>
		Line,

        /// <summary>
        /// Single solid thick line separator.
        /// </summary>
		ThickLine,

        /// <summary>
        /// Double solid line separator.
        /// </summary>
        DoubleLine,

        /// <summary>
        /// Single dash line separator.
        /// </summary>
        DashLine,

        /// <summary>
        /// Single dot line separator.
        /// </summary>
        DotLine,

        /// <summary>
        /// Gradient solid line separator.
        /// </summary>
        GradientLine,

        /// <summary>
        /// Thick gradient solid line separator.
        /// </summary>
        ThickGradientLine,
    }
    /// <summary>
    /// An enumeration of legend styles.
    /// </summary>
    public enum LegendStyle
    {
        /// <summary>
        /// One column, many rows.
        /// </summary>
        Column,

        /// <summary>
        /// One row, many columns.
        /// </summary>
        Row,

        /// <summary>
        /// Many column, many rows.
        /// </summary>
        Table
    };

    /// <summary>
    /// An enumeration of legend table styles.
    /// </summary>
    public enum LegendTableStyle
    {
        /// <summary>
        /// The legend table style is automatically determined by the chart.
        /// </summary>
        Auto,

        /// <summary>
        /// The legend items will be fit horizontally within the legend.
        /// It is preferred to use this style when the docking is set to top or bottom.
        /// </summary>
        Wide,

        /// <summary>
        /// The legend items will be fit vertically within the legend.
        /// It is preferred to use this style when docking is set to left or right.
        /// </summary>
        Tall
    };

    #endregion Legend enumerations

    /// <summary>
    /// The legend class represents a single chart legend. It contains visual
    /// appearance, position and content properties. This class is also
    /// responsible for drawing and positioning of the legend.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLegend_Legend"),
    ]
    public class Legend : ChartNamedElement
    {
        #region Fields

        // Font calculated by auto fitting
        internal SKFont autofitFont = null;

        // Collection of custom and series legend items
        internal LegendItemsCollection legendItems = null;

        // Pixel size of the 'W' character
        internal SKSize singleWCharacterSize = SKSize.Empty;

        // Size of the dots (pixels) that will drawn on the bottom of the legend when it is truncated
        private readonly int _truncatedDotsSize = 3;

        // Value used to calculate auto-fit font size from the legend Font.
        private int _autoFitFontSizeAdjustment = 0;

        // Minimum font size that can be used by the legend auto-fitting algorithm
        private int _autoFitMinFontSize = 7;

        private SKColor _backColor = SKColor.Empty;

        private GradientStyle _backGradientStyle = GradientStyle.None;

        private ChartHatchStyle _backHatchStyle = ChartHatchStyle.None;

        private string _backImage = "";

        private ChartImageAlignmentStyle _backImageAlignment = ChartImageAlignmentStyle.TopLeft;

        private SKColor _backImageTransparentColor = SKColor.Empty;

        private ChartImageWrapMode _backImageWrapMode = ChartImageWrapMode.Tile;

        private SKColor _backSecondaryColor = SKColor.Empty;

        private SKColor _borderColor = SKColor.Empty;

        private ChartDashStyle _borderDashStyle = ChartDashStyle.Solid;

        private int _borderWidth = 1;

        // Legend column collection
        private LegendCellColumnCollection _cellColumns = null;

        // Legend item heigts
        private int[,] _cellHeights = null;

        private LegendItemsCollection _customLegends = null;

        private string _dockedToChartArea = Constants.NotSetValue;

        private bool _enabled = true;

        private SKFont _font = null;

        private FontCache _fontCache = new();

        private SKColor _foreColor = SKColors.Black;

        // Legend header position in pixel coordinates.
        private SKRect _headerPosition = SKRect.Empty;

        // Legend header visual separator
        private LegendSeparatorStyle _headerSeparator = LegendSeparatorStyle.None;

        // Legend header visual separator color
        private SKColor _headerSeparatorColor = SKColors.Black;

        // Horizontal space left after fitting legend items
        private int _horizontalSpaceLeft = 0;

        // Indicate that legend rows should be drawn with isInterlaced background color.
        private bool _interlacedRows = false;

        // Legend isInterlaced rows color
        private SKColor _interlacedRowsColor = SKColor.Empty;

        private bool _isDockedInsideChartArea = true;

        // Indicates that all items in the legend should be equally spaced
        private bool _isEquallySpacedItems = false;

        private bool _isTextAutoFit = true;

        //***********************************************************
        //** Private data members
        //***********************************************************
        // Number of rows and columns
        private int _itemColumns = 0;

        // Legend table columns visual separator
        private LegendSeparatorStyle _itemColumnSeparator = LegendSeparatorStyle.None;

        // Legend table columns visual separator color
        private SKColor _itemColumnSeparatorColor = SKColors.Black;

        // Legend table column spacing calculated as a percentage of the font
        private int _itemColumnSpacing = 50;

        // Legend table column spacing calculated in relative coordinates
        private int _itemColumnSpacingRel = 0;

        private StringAlignment _legendAlignment = StringAlignment.Near;

        private Docking _legendDocking = Docking.Right;

        // Indicates that legend items automatically added based on the exsisting
        // series in reversed order.
        private LegendItemOrder _legendItemOrder = LegendItemOrder.Auto;

        // Legend items area position in pixels
        private SKRect _legendItemsAreaPosition = SKRect.Empty;

        // Indicates that not all legend items were able to fit the legend
        private bool _legendItemsTruncated = false;

        private LegendStyle _legendStyle = LegendStyle.Table;

        private LegendTableStyle _legendTableStyle = LegendTableStyle.Auto;

        // Adjustment point used for legend animation
        private float _maximumLegendAutoSize = 50f;

        // Maximum number of cells in the legend item
        private int _numberOfCells = -1;

        // Number of items from the collection that should be processed
        private int _numberOfLegendItemsToProcess = -1;

        // Number of rows per each legend table column
        private int[] _numberOfRowsPerColumn = null;

        // Legend offsets
        private SKSize _offset = SKSize.Empty;

        //***********************************************************
        //** Private data members, which store properties values
        //***********************************************************
        private ElementPosition _position = null;
        private SKColor _shadowColor = Color.FromArgb(128, 0, 0, 0);
        private int _shadowOffset = 0;
        // Sub-columns sizes calculated during the fitting process
        private int[,] _subColumnSizes = null;

        // Text length after which the legend item text will be wrapped on the next whitespace.
        private int _textWrapThreshold = 25;
        // Legend title text
        private string _title = string.Empty;

        // Legend title alignment
        private StringAlignment _titleAlignment = StringAlignment.Center;

        // Legend title back color
        private SKColor _titleBackColor = SKColor.Empty;

        // Legend title font
        private SKFont _titleFont = null;

        // Legend title color
        private SKColor _titleForeColor = SKColors.Black;
        // Legend title position in pixelcoordinates.
        // Note that legend title always docked to the top of the legend.
        private SKRect _titlePosition = SKRect.Empty;

        // Legend title visual separator
        private LegendSeparatorStyle _titleSeparator = LegendSeparatorStyle.None;

        // Legend title visual separator color
        private SKColor _titleSeparatorColor = SKColors.Black;
        // Vertical space left after fitting legend items
        private int _verticalSpaceLeft = 0;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Legend constructor
        /// </summary>
        public Legend()
        {
            _position = new ElementPosition(this);
            // Initialize custom items collection
            _customLegends = new LegendItemsCollection(this);
            legendItems = new LegendItemsCollection(this);
            _cellColumns = new LegendCellColumnCollection(this);
            _font = _fontCache.DefaultFont;
            _titleFont = _fontCache.DefaultBoldFont;
        }

        /// <summary>
        /// Legend constructor
        /// </summary>
        /// <param name="name">The legend name.</param>
        public Legend(string name) : base(name)
        {
            _position = new ElementPosition(this);
            // Initialize custom items collection
            _customLegends = new LegendItemsCollection(this);
            legendItems = new LegendItemsCollection(this);
            _cellColumns = new LegendCellColumnCollection(this);
            _font = _fontCache.DefaultFont;
            _titleFont = _fontCache.DefaultBoldFont;
        }

        #endregion Constructors

        #region Legend position & size methods

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeLegend_Alignment"),
        ]
        public StringAlignment Alignment
        {
            get
            {
                return _legendAlignment;
            }
            set
            {
                _legendAlignment = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the minimum font size that can be used by the legend text's auto-fitting algorithm.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_AutoFitMinFontSize"),
        ]
        public int AutoFitMinFontSize
        {
            get
            {
                return _autoFitMinFontSize;
            }
            set
            {
                // Font size cannot be less than 5
                if (value < 5)
                {
                    throw (new InvalidOperationException(SR.ExceptionLegendAutoFitMinFontSizeInvalid));
                }

                _autoFitMinFontSize = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the legend background color.
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
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets background gradient style of the legend.
        /// </summary>
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
        /// Gets or sets the legend background image.
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
        /// Gets or sets the background image alignment used for the unscaled drawing mode.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImageAlign")
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
        /// Gets or sets the legend background image drawing mode.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageWrapMode")
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
        /// Gets or sets the secondary background color.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of background with
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
        /// Gets or sets the legend border color.
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
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the legend border style.
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
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the legend border width.
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
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionLegendBorderWidthIsNegative);
                }
                _borderWidth = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets a collection of legend columns.
        /// </summary>
        [
        SRCategory("CategoryAttributeCellColumns"),
        SRDescription("DescriptionAttributeLegend_CellColumns"),
        ]
        public LegendCellColumnCollection CellColumns
        {
            get
            {
                return _cellColumns;
            }
        }

        /// <summary>
        /// Gets the custom legend items.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_CustomItems"),
        ]
        public LegendItemsCollection CustomItems
        {
            get
            {
                return _customLegends;
            }
        }

        /// <summary>
        /// Gets or sets the name of the chart area where the legend
        /// should be docked.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeLegend_DockToChartArea"),
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
                    if (String.IsNullOrEmpty(value))
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
        /// Gets or sets the property that specifies where the legend docks.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeLegend_Docking"),
        ]
        public Docking Docking
        {
            get
            {
                return _legendDocking;
            }
            set
            {
                _legendDocking = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the legend is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_Enabled"),
        ]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the font of the legend text.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_Font"),
        ]
        public SKFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                IsTextAutoFit = false;

                _font = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the color of the legend text.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendFontColor"),
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
        /// Gets the legend header separator style.
        /// </summary>
        [
        SRCategory("CategoryAttributeCellColumns"),
        SRDescription("DescriptionAttributeLegend_HeaderSeparator"),
        ]
        public LegendSeparatorStyle HeaderSeparator
        {
            get
            {
                return _headerSeparator;
            }
            set
            {
                if (value != _headerSeparator)
                {
                    _headerSeparator = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the legend header separator.
        /// </summary>
        [
        SRCategory("CategoryAttributeCellColumns"),
        SRDescription("DescriptionAttributeLegend_HeaderSeparatorColor"),
        ]
        public SKColor HeaderSeparatorColor
        {
            get
            {
                return _headerSeparatorColor;
            }
            set
            {
                if (value != _headerSeparatorColor)
                {
                    _headerSeparatorColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the chart area name inside which the legend is drawn.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_InsideChartArea"),
        ]
        public string InsideChartArea
        {
            get
            {
                if (Common != null &&
                    Common.Chart != null &&
                    Common.Chart.serializing)
                {
                    return "NotSet";
                }
                return DockedToChartArea;
            }
            set
            {
                if (value.Length == 0)
                {
                    DockedToChartArea = Constants.NotSetValue;
                }
                else
                {
                    DockedToChartArea = value;
                }
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether
        /// legend rows should be drawn with interlaced background color.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_InterlacedRows"),
        ]
        public bool InterlacedRows
        {
            get
            {
                return _interlacedRows;
            }
            set
            {
                if (value != _interlacedRows)
                {
                    _interlacedRows = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the legend interlaced row's background color. Only applicable if interlaced rows are used.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_InterlacedRowsColor"),
        ]
        public SKColor InterlacedRowsColor
        {
            get
            {
                return _interlacedRowsColor;
            }
            set
            {
                if (value != _interlacedRowsColor)
                {
                    _interlacedRowsColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a property which indicates whether
        /// the legend is docked inside the chart area.
        /// This property is only available when DockedToChartArea is set.
        /// </summary>
        [
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeLegend_DockInsideChartArea"),
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
        /// Gets or sets a property which indicates whether
        /// all legend items are equally spaced.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_EquallySpacedItems"),
        ]
        public bool IsEquallySpacedItems
        {
            get
            {
                return _isEquallySpacedItems;
            }
            set
            {
                _isEquallySpacedItems = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if legend text is automatically sized.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_AutoFitText"),
        ]
        public bool IsTextAutoFit
        {
            get
            {
                return _isTextAutoFit;
            }
            set
            {
                _isTextAutoFit = value;

                if (_isTextAutoFit)
                {
                    // Reset the font size to "8"
                    // Use current font family name ans style if possible.
                    if (_font != null)
                    {
                        _font = _fontCache.GetFont(_font.Typeface.FamilyName, 8, _font.Typeface.FontStyle);
                    }
                    else
                    {
                        _font = _fontCache.DefaultFont;
                    }
                }

                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the separator style of the legend table columns.
        /// </summary>
        [
        SRCategory("CategoryAttributeCellColumns"),
        SRDescription("DescriptionAttributeLegend_ItemColumnSeparator"),
        ]
        public LegendSeparatorStyle ItemColumnSeparator
        {
            get
            {
                return _itemColumnSeparator;
            }
            set
            {
                if (value != _itemColumnSeparator)
                {
                    _itemColumnSeparator = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the separator of the legend table columns.
        /// </summary>
        [
        SRCategory("CategoryAttributeCellColumns"),
        SRDescription("DescriptionAttributeLegend_ItemColumnSeparatorColor"),
        ]
        public SKColor ItemColumnSeparatorColor
        {
            get
            {
                return _itemColumnSeparatorColor;
            }
            set
            {
                if (value != _itemColumnSeparatorColor)
                {
                    _itemColumnSeparatorColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the legend table column spacing, as a percentage of the legend text font.
        /// </summary>
        [
        SRCategory("CategoryAttributeCellColumns"),
        SRDescription("DescriptionAttributeLegend_ItemColumnSpacing"),
        ]
        public int ItemColumnSpacing
        {
            get
            {
                return _itemColumnSpacing;
            }
            set
            {
                if (value != _itemColumnSpacing)
                {
                    if (value < 0)
                    {
                        throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionLegendColumnSpacingInvalid));
                    }
                    _itemColumnSpacing = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a property that specifies the order that legend items are shown. This property only affects
        /// legend items automatically added for the chart series and has no effect on custom legend items.
        /// </summary>
        /// <remarks>
        /// When the <b>LegendItemOrder</b> property is set to <b>Auto</b>, the legend will automatically be reversed
        /// if StackedColumn, StackedColumn100, StackedArea or StackedArea100 chart types are used.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_Reversed"),
        ]
        public LegendItemOrder LegendItemOrder
        {
            get
            {
                return _legendItemOrder;
            }
            set
            {
                if (value != _legendItemOrder)
                {
                    _legendItemOrder = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the legend style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_LegendStyle"),
        ]
        public LegendStyle LegendStyle
        {
            get
            {
                return _legendStyle;
            }
            set
            {
                _legendStyle = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the maximum size (in percentage) of the legend used in the automatic layout algorithm.
        /// </summary>
        /// <remarks>
        /// If the legend is docked to the left or right, this property determines the maximum width of the legend, measured as a percentage.
        /// If the legend is docked to the top or bottom, this property determines the maximum height of the legend, measured as a percentage.
        /// </remarks>
		[
        SRCategory("CategoryAttributeDocking"),
        SRDescription("DescriptionAttributeLegend_MaxAutoSize"),
        ]
        public float MaximumAutoSize
        {
            get
            {
                return _maximumLegendAutoSize;
            }
            set
            {
                if (value < 0f || value > 100f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionLegendMaximumAutoSizeInvalid);
                }
                _maximumLegendAutoSize = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the name of the legend.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeLegend_Name"),
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
        /// Gets or sets the position of the legend.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_Position"),
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
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the color of a legend's shadow.
        /// <seealso cref="ShadowOffset"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used to draw a legend's shadow.
        /// </value>
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
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the offset between the legend and its shadow.
        /// <seealso cref="ShadowColor"/>
        /// </summary>
        /// <value>
        /// An integer value that represents the offset between the legend and its shadow.
        /// </value>
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
        /// Gets the legend table style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_TableStyle"),
        ]
        public LegendTableStyle TableStyle
        {
            get
            {
                return _legendTableStyle;
            }
            set
            {
                _legendTableStyle = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets a property that defines the preferred number of characters in a line of the legend text.
        /// </summary>
        /// <remarks>
        /// When legend text exceeds the value defined in the <b>TextWrapThreshold</b> property, it will be
        /// automatically wrapped on the next whitespace. Text will not be wrapped if there is no whitespace
        /// characters in the text. Set this property to zero to disable the feature.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegend_TextWrapThreshold"),
        ]
        public int TextWrapThreshold
        {
            get
            {
                return _textWrapThreshold;
            }
            set
            {
                if (value != _textWrapThreshold)
                {
                    if (value < 0)
                    {
                        throw (new ArgumentException(SR.ExceptionTextThresholdIsNegative, nameof(value)));
                    }
                    _textWrapThreshold = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the title text of the legend.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeLegend_Title"),
        ]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the legend title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeLegend_TitleAlignment"),
        ]
        public StringAlignment TitleAlignment
        {
            get
            {
                return _titleAlignment;
            }
            set
            {
                if (value != _titleAlignment)
                {
                    _titleAlignment = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of the legend title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeTitleBackColor"),
        ]
        public SKColor TitleBackColor
        {
            get
            {
                return _titleBackColor;
            }
            set
            {
                if (value != _titleBackColor)
                {
                    _titleBackColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the font of the legend title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeTitleFont"),
        ]
        public SKFont TitleFont
        {
            get
            {
                return _titleFont;
            }
            set
            {
                if (value != _titleFont)
                {
                    _titleFont = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text color of the legend title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeLegend_TitleColor"),
        ]
        public SKColor TitleForeColor
        {
            get
            {
                return _titleForeColor;
            }
            set
            {
                if (value != _titleForeColor)
                {
                    _titleForeColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the separator style of the legend title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeLegend_TitleSeparator"),
        ]
        public LegendSeparatorStyle TitleSeparator
        {
            get
            {
                return _titleSeparator;
            }
            set
            {
                if (value != _titleSeparator)
                {
                    _titleSeparator = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the separator color of the legend title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeLegend_TitleSeparatorColor"),
        ]
        public SKColor TitleSeparatorColor
        {
            get
            {
                return _titleSeparatorColor;
            }
            set
            {
                if (value != _titleSeparatorColor)
                {
                    _titleSeparatorColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Recalculates legend position.
        /// </summary>
        /// <param name="chartGraph">Chart graphics used.</param>
        /// <param name="chartAreasRectangle">Area where the legend should be positioned.</param>
        /// <param name="elementSpacing">Spacing size as a percentage of the area.</param>
        internal void CalcLegendPosition(
            ChartGraphics chartGraph,
            ref SKRect chartAreasRectangle,
            float elementSpacing)
        {
            SKRect legendPosition = new();

            // Get optimal legend size
            SKSize maxSize = new(chartAreasRectangle.Width - 2 * elementSpacing, chartAreasRectangle.Height - 2 * elementSpacing);
            if (DockedToChartArea == Constants.NotSetValue)
            {
                // Note: 'maxLegendSize' parameter is ignored. New legend property
                // 'maximumLegendAutoSize' is used instead.
                if (Docking == Docking.Top || Docking == Docking.Bottom)
                {
                    maxSize.Height = (maxSize.Height / 100F) * _maximumLegendAutoSize;
                }
                else
                {
                    maxSize.Width = (maxSize.Width / 100F) * _maximumLegendAutoSize;
                }
            }

            if (maxSize.Width <= 0 || maxSize.Height <= 0)
            {
                return;
            }

            SKSize legendSize = GetOptimalSize(chartGraph, maxSize);
            legendPosition.Size = new(legendSize.Width, legendSize.Height);
            if (float.IsNaN(legendSize.Height) || float.IsNaN(legendSize.Width))
            {
                return;
            }

            // Calculate legend position
            if (Docking == Docking.Top)
            {
                legendPosition.Top = chartAreasRectangle.Top + elementSpacing;
                if (Alignment == StringAlignment.Near)
                {
                    legendPosition.Left = chartAreasRectangle.Left + elementSpacing;
                }
                else if (Alignment == StringAlignment.Far)
                {
                    legendPosition.Left = chartAreasRectangle.Right - legendSize.Width - elementSpacing;
                }
                else if (Alignment == StringAlignment.Center)
                {
                    legendPosition.Top = chartAreasRectangle.Left + (chartAreasRectangle.Width - legendSize.Width) / 2F;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Bottom -= legendPosition.Height + elementSpacing;
                chartAreasRectangle.Top = legendPosition.Bottom;
            }
            else if (Docking == Docking.Bottom)
            {
                legendPosition.Top = chartAreasRectangle.Bottom - legendSize.Height - elementSpacing;
                if (Alignment == StringAlignment.Near)
                {
                    legendPosition.Left = chartAreasRectangle.Left + elementSpacing;
                }
                else if (Alignment == StringAlignment.Far)
                {
                    legendPosition.Left = chartAreasRectangle.Right - legendSize.Width - elementSpacing;
                }
                else if (Alignment == StringAlignment.Center)
                {
                    legendPosition.Left = chartAreasRectangle.Left + (chartAreasRectangle.Width - legendSize.Width) / 2F;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Bottom -= legendPosition.Height + elementSpacing;
            }
            if (Docking == Docking.Left)
            {
                legendPosition.Left = chartAreasRectangle.Left + elementSpacing;
                if (Alignment == StringAlignment.Near)
                {
                    legendPosition.Top = chartAreasRectangle.Top + elementSpacing;
                }
                else if (Alignment == StringAlignment.Far)
                {
                    legendPosition.Top = chartAreasRectangle.Bottom - legendSize.Height - elementSpacing;
                }
                else if (Alignment == StringAlignment.Center)
                {
                    legendPosition.Top = chartAreasRectangle.Top + (chartAreasRectangle.Height - legendSize.Height) / 2F;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Bottom -= legendPosition.Width + elementSpacing;
                chartAreasRectangle.Left = legendPosition.Right;
            }
            if (Docking == Docking.Right)
            {
                legendPosition.Left = chartAreasRectangle.Right - legendSize.Width - elementSpacing;
                if (Alignment == StringAlignment.Near)
                {
                    legendPosition.Top = chartAreasRectangle.Top + elementSpacing;
                }
                else if (Alignment == StringAlignment.Far)
                {
                    legendPosition.Top = chartAreasRectangle.Bottom - legendSize.Height - elementSpacing;
                }
                else if (Alignment == StringAlignment.Center)
                {
                    legendPosition.Top = chartAreasRectangle.Top + (chartAreasRectangle.Height - legendSize.Height) / 2F;
                }

                // Adjust position of the chart area(s)
                chartAreasRectangle.Right -= legendPosition.Width + elementSpacing;
            }

            Position.SetPositionNoAuto(legendPosition.Left, legendPosition.Top, legendPosition.Width, legendPosition.Height);
        }

        /// <summary>
        /// Gets legend separator size in pixels
        /// </summary>
        /// <param name="separatorType">Separator type.</param>
        /// <returns>Separator size in relative coordinates.</returns>
        internal SKSize GetSeparatorSize(LegendSeparatorStyle separatorType)
        {
            SKSize size;
            if (separatorType == LegendSeparatorStyle.None)
            {
                size = SKSize.Empty;
            }
            else if (separatorType == LegendSeparatorStyle.Line)
            {
                size = new(1, 1);
            }
            else if (separatorType == LegendSeparatorStyle.DashLine)
            {
                size = new(1, 1);
            }
            else if (separatorType == LegendSeparatorStyle.DotLine)
            {
                size = new(1, 1);
            }
            else if (separatorType == LegendSeparatorStyle.ThickLine)
            {
                size = new(2, 2);
            }
            else if (separatorType == LegendSeparatorStyle.DoubleLine)
            {
                size = new(3, 3);
            }
            else if (separatorType == LegendSeparatorStyle.GradientLine)
            {
                size = new(1, 1);
            }
            else if (separatorType == LegendSeparatorStyle.ThickGradientLine)
            {
                size = new(2, 2);
            }
            else
            {
                throw (new InvalidOperationException(SR.ExceptionLegendSeparatorTypeUnknown(separatorType.ToString())));
            }

            // For the vertical part of the separator always add additiobal spacing
            size.Width += _itemColumnSpacingRel;

            return size;
        }

        /// <summary>
        /// Invalidate chart legend when one of the properties is changed
        /// </summary>
        /// <param name="invalidateLegendOnly">Indicates that only legend area should be invalidated.</param>
        internal void Invalidate(bool invalidateLegendOnly)
        {
            // TODO: this does nothing
            if (Chart != null && !Chart.disableInvalidates)
            {
                if (invalidateLegendOnly)
                {
                    // Calculate the position of the legend
                    SKRect invalRect = new();
                    if (Position.Width != 0 && Position.Height != 0)
                    {
                        // Convert relative coordinates to absolute coordinates
                        invalRect.Left = (int)(Position.X * (Common.ChartPicture.Width - 1) / 100F);
                        invalRect.Top = (int)(Position.Y * (Common.ChartPicture.Height - 1) / 100F);
                        invalRect.Size = new(
                            (int)(Position.Width * (Common.ChartPicture.Width - 1) / 100F),
                            (int)(Position.Height * (Common.ChartPicture.Height - 1) / 100F));

                        // Inflate rectangle size using border size and shadow size
                        invalRect.Inflate(BorderWidth + ShadowOffset + 1, BorderWidth + ShadowOffset + 1);
                    }

                    // Invalidate legend rectangle only
                    Chart.dirtyFlag = true;
                    ChartService.Invalidate();
                }
                else
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Helper method that checks if legend is enabled.
        /// </summary>
        /// <returns>True if legend is enabled.</returns>
        internal bool IsEnabled()
        {
                // Check if legend is docked to the chart area
                    // Do not show legend when it is docked to invisible chart area
            if (Enabled)
            {
                if (DockedToChartArea.Length > 0 &&
                    Common != null &&
                    Common.ChartPicture != null && Common.ChartPicture.ChartAreas.IndexOf(DockedToChartArea) >= 0)
                {
                    ChartArea area = Common.ChartPicture.ChartAreas[DockedToChartArea];
                    if (!area.Visible)
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Paints legend using chart graphics object.
        /// </summary>
        /// <param name="chartGraph">The graph provides drawing object to the display device. A Graphics object is associated with a specific device context.</param>
        internal void Paint(ChartGraphics chartGraph)
        {
            // Reset some values
            _offset = SKSize.Empty;
            _itemColumns = 0;
            _horizontalSpaceLeft = 0;
            _verticalSpaceLeft = 0;
            _subColumnSizes = null;
            _numberOfRowsPerColumn = null;
            _cellHeights = null;
            autofitFont = null;
            _autoFitFontSizeAdjustment = 0;
            _numberOfCells = -1;
            _numberOfLegendItemsToProcess = -1;

            // Do nothing if legend disabled
            if (!IsEnabled() ||
                (MaximumAutoSize == 0f && Position.Auto))
            {
                return;
            }

            // Add all series legend into items collection and then add custom legend items
            FillLegendItemsCollection();

            // Clear all legend item cells information
            foreach (LegendItem legendItem in legendItems)
            {
                foreach (LegendCell cell in legendItem.Cells)
                {
                    cell.ResetCache();
                }
            }

            // Call a notification event, so that legend items collection can be modified by user
            Common.Chart.CallOnCustomizeLegend(legendItems, Name);

            // Check if legend is empty
            if (legendItems.Count == 0)
            {
                return;
            }

            //***********************************************************
            //** RecalculateAxesScale legend information
            //***********************************************************
            RecalcLegendInfo(chartGraph);

            //***********************************************************
            //** Paint legend
            //***********************************************************

            // Call BackPaint event
            if (Common.ProcessModePaint)
            {
                // Draw legend background, border and shadow
                chartGraph.FillRectangleRel(
                    chartGraph.GetRelativeRectangle(chartGraph.GetAbsoluteRectangle(Position.ToSKRect()).Round()),
                    BackColor,
                    BackHatchStyle,
                    BackImage,
                    BackImageWrapMode,
                    BackImageTransparentColor,
                    BackImageAlignment,
                    BackGradientStyle,
                    BackSecondaryColor,
                    BorderColor,
                    GetBorderSize(),
                    BorderDashStyle,
                    ShadowColor,
                    ShadowOffset,
                    PenAlignment.Inset);

                Common.Chart.CallOnPrePaint(new ChartPaintEventArgs(this, chartGraph, Common, Position));
            }

            if (Common.ProcessModeRegions)
            {
                SelectLegendBackground();
            }

            //***********************************************************
            //** Draw legend header
            //***********************************************************

            DrawLegendHeader(chartGraph);

            //***********************************************************
            //** Draw legend title
            //***********************************************************

            DrawLegendTitle(chartGraph);

            // Add legend title hot region
            if (Common.ProcessModeRegions && !_titlePosition.IsEmpty)
            {
                Common.HotRegionsList.AddHotRegion(chartGraph.GetRelativeRectangle(_titlePosition), this, ChartElementType.LegendTitle, true);
            }

            //***********************************************************
            //** Draw legend items
            //***********************************************************
            if (_numberOfLegendItemsToProcess < 0)
            {
                _numberOfLegendItemsToProcess = legendItems.Count;
            }
            for (int itemIndex = 0; itemIndex < _numberOfLegendItemsToProcess; itemIndex++)
            {
                LegendItem legendItem = legendItems[itemIndex];

                // Iterate through legend item cells
                for (int cellIndex = 0; cellIndex < legendItem.Cells.Count; cellIndex++)
                {
                    // Get legend cell
                    LegendCell legendCell = legendItem.Cells[cellIndex];

                    // Paint cell
                    legendCell.Paint(
                        chartGraph,
                        _autoFitFontSizeAdjustment,
                        autofitFont,
                        singleWCharacterSize);
                }

                // Paint legend item separator
                if (legendItem.SeparatorType != LegendSeparatorStyle.None &&
                    legendItem.Cells.Count > 0)
                {
                    // Calculate separator position
                    SKRect separatorPosition = SKRect.Empty;
                    separatorPosition.Left = legendItem.Cells[0].cellPosition.Left;

                    // Find right most cell position excluding ovelapped cells that have negative size
                    int right = 0;
                    for (int index = legendItem.Cells.Count - 1; index >= 0; index--)
                    {
                        right = (int)legendItem.Cells[index].cellPosition.Right;
                        if (right > 0)
                        {
                            break;
                        }
                    }
                    separatorPosition.Top = legendItem.Cells[0].cellPosition.Bottom;
                    separatorPosition.Size = new(right - separatorPosition.Left, GetSeparatorSize(legendItem.SeparatorType).Height);
                    separatorPosition.Intersect(_legendItemsAreaPosition);

                    // Draw separator
                    this.DrawSeparator(
                        chartGraph,
                        legendItem.SeparatorType,
                        legendItem.SeparatorColor,
                        true,
                        separatorPosition);
                }
            }

            //***********************************************************
            //** If legend items are in multiple columns draw vertical
            //** separator
            //***********************************************************
            if (ItemColumnSeparator != LegendSeparatorStyle.None)
            {
                var separatorRect = (chartGraph.GetAbsoluteRectangle(Position.ToSKRect())).Round();
                separatorRect.Top += GetBorderSize() + _titlePosition.Height;
                separatorRect.Bottom -= 2 * GetBorderSize() + _titlePosition.Height;
                separatorRect.Left += GetBorderSize() + _offset.Width;
                separatorRect.Right = separatorRect.Left + GetSeparatorSize(ItemColumnSeparator).Width;
                if (_horizontalSpaceLeft > 0)
                {
                    separatorRect.Left += _horizontalSpaceLeft / 2.0F;
                }

                // Check position
                if (separatorRect.Width > 0 && separatorRect.Height > 0)
                {
                    // Iterate through all columns
                    for (int columnIndex = 0; columnIndex < _itemColumns; columnIndex++)
                    {
                        // Iterate through all sub-columns
                        int cellCount = GetNumberOfCells();
                        for (int subColumnIndex = 0; subColumnIndex < cellCount; subColumnIndex++)
                        {
                            separatorRect.Left += _subColumnSizes[columnIndex, subColumnIndex];
                        }

                        // Draw separator if not the last column
                        if (columnIndex < _itemColumns - 1)
                        {
                            this.DrawSeparator(chartGraph, ItemColumnSeparator, ItemColumnSeparatorColor, false, separatorRect);
                        }

                        // Add separator width
                        separatorRect.Left += separatorRect.Width;
                    }
                }
            }

            //***********************************************************
            //** Draw special indicator on the bottom of the legend if
            //** it was truncated.
            //***********************************************************
            if (_legendItemsTruncated &&
                _legendItemsAreaPosition.Height > _truncatedDotsSize / 2)
            {
                // Calculate dots step (no more than 10 pixel)
                int markerCount = 3;
                int step = (int)(_legendItemsAreaPosition.Width / 3 / markerCount);
                step = Math.Min(step, 10);

                // Calculate start point
                SKPoint point = new(
                    _legendItemsAreaPosition.Left + _legendItemsAreaPosition.Width / 2 - step * (float)Math.Floor(markerCount / 2f),
                    _legendItemsAreaPosition.Bottom + (_truncatedDotsSize + _offset.Height) / 2);

                // Draw several dots at the bottom of the legend
                for (int index = 0; index < markerCount; index++)
                {
                    chartGraph.DrawMarkerRel(
                        chartGraph.GetRelativePoint(point),
                        MarkerStyle.Circle,
                        _truncatedDotsSize,
                        ForeColor,
                        SKColor.Empty,
                        0,
                        string.Empty,
                        SKColor.Empty,
                        0,
                        SKColor.Empty,
                        SKRect.Empty);

                    // Shift to the right
                    point.X += step;
                }
            }

            // Call Paint event
            if (Common.ProcessModePaint)
            {
                Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, chartGraph, Common, Position));
            }

            // Remove temporary cells from legend items
            foreach (LegendItem legendItem in legendItems)
            {
                if (legendItem.clearTempCells)
                {
                    legendItem.clearTempCells = false;
                    legendItem.Cells.Clear();
                }
            }
        }

        /// <summary>
        /// Checks if legend background is selected.
        /// </summary>
        internal void SelectLegendBackground()
        {
            Common.HotRegionsList.AddHotRegion(Position.ToSKRect(), this, ChartElementType.LegendArea, true);
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
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Free managed resources
                if (_fontCache != null)
                {
                    _fontCache.Dispose();
                    _fontCache = null;
                }
                if (legendItems != null)
                {
                    legendItems.Dispose();
                    legendItems = null;
                }
                if (_cellColumns != null)
                {
                    _cellColumns.Dispose();
                    _cellColumns = null;
                }
                if (_customLegends != null)
                {
                    _customLegends.Dispose();
                    _customLegends = null;
                }
                if (_position != null)
                {
                    _position.Dispose();
                    _position = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Checks if specified legend will fit the specified size.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="legendItemsAreaSize">Area that legend items must fit.</param>
        /// <param name="numberOfItemsToCheck">Number of items that should be fitted.</param>
        /// <param name="fontSizeReducedBy">Number of points the standard legend font is reduced by auto-fitting algorithm.</param>
        /// <param name="numberOfColumns">Legend column number.</param>
        /// <param name="numberOfRowsPerColumn">Array of number of rows per column.</param>
        /// <param name="subColumnSizes">Returns array of sub-column size.</param>
        /// <param name="cellHeights">Returns array of cell heights.</param>
        /// <param name="horizontalSpaceLeft">Returns horizontal space left.</param>
        /// <param name="verticalSpaceLeft">Returns vertical space left.</param>
        /// <returns>True if items fit.</returns>
        private bool CheckLegendItemsFit(
            ChartGraphics graph,
            SKSize legendItemsAreaSize,
            int numberOfItemsToCheck,
            int fontSizeReducedBy,
            int numberOfColumns,
            int[] numberOfRowsPerColumn,
            out int[,] subColumnSizes,
            out int[,] cellHeights,
            out int horizontalSpaceLeft,
            out int verticalSpaceLeft)
        {
            bool fitFlag = true;

            // Initialize output values
            horizontalSpaceLeft = 0;
            verticalSpaceLeft = 0;

            // Use current legend item count if number of items to check is not specified
            if (numberOfItemsToCheck < 0)
            {
                numberOfItemsToCheck = legendItems.Count;
            }

            // Calculate how many sub-columns (cells) this legend has
            int numberOfSubColumns = GetNumberOfCells();

            // Each column may have its own number of rows. Calculate the maximum number of rows.
            int maxNumberOfRows = GetMaximumNumberOfRows(numberOfRowsPerColumn);

            // Create multidimensional arrays that will be holding the widths and heightsof all
            // individual cells. First dimension will be the legend column index, second dimension
            // is row index and the third is sub-column (cell) index.
            int[,,] cellWidths = new int[numberOfColumns, maxNumberOfRows, numberOfSubColumns];
            cellHeights = new int[numberOfColumns, maxNumberOfRows];

            //*************************************************************************
            //** Measure legend font single character
            //*************************************************************************
            singleWCharacterSize = graph.MeasureStringAbs("W", autofitFont ?? Font);
            SKSize doubleCharacterSize = graph.MeasureStringAbs("WW", autofitFont ?? Font);
            singleWCharacterSize.Width = doubleCharacterSize.Width - singleWCharacterSize.Width;

            //*************************************************************************
            //** Iterate through all legend items and measure each individual cell
            //*************************************************************************
            int currentColumn = 0;
            int currentRow = 0;
            for (int legendItemIndex = 0; legendItemIndex < numberOfItemsToCheck; legendItemIndex++)
            {
                LegendItem legendItem = legendItems[legendItemIndex];

                // Iterate through legend item cells
                int numberOfCellsToSkip = 0;
                for (int cellIndex = 0; cellIndex < legendItem.Cells.Count; cellIndex++)
                {
                    // Get legend cell
                    LegendCell legendCell = legendItem.Cells[cellIndex];

                    // Get assocated legend column object (may be NULL)
                    LegendCellColumn legendColumn = null;
                    if (cellIndex < CellColumns.Count)
                    {
                        legendColumn = CellColumns[cellIndex];
                    }

                    // Check if current cell should be skipped becuse it's overlapped
                    // by the previous sell that uses CellSpan.
                    if (numberOfCellsToSkip > 0)
                    {
                        // Put size (-1) for the cells that follow a cell using ColumnSpan
                        cellWidths[currentColumn, currentRow, cellIndex] = -1;
                        --numberOfCellsToSkip;
                        continue;
                    }

                    // Check if current cell uses CellSpan
                    if (legendCell.CellSpan > 1)
                    {
                        numberOfCellsToSkip = legendCell.CellSpan - 1;
                    }

                    // Measure cell and store the value in the array
                    SKSize cellSize = legendCell.MeasureCell(
                        graph,
                        fontSizeReducedBy,
                        autofitFont ?? Font,
                        singleWCharacterSize);

                    // Check for column maximum/minimum cell width restrictions
                    if (legendColumn != null)
                    {
                        if (legendColumn.MinimumWidth >= 0)
                        {
                            cellSize.Width = (int)Math.Max(cellSize.Width, legendColumn.MinimumWidth * singleWCharacterSize.Width / 100f);
                        }
                        if (legendColumn.MaximumWidth >= 0)
                        {
                            cellSize.Width = (int)Math.Min(cellSize.Width, legendColumn.MaximumWidth * singleWCharacterSize.Width / 100f);
                        }
                    }

                    // Store cell size in arrays
                    cellWidths[currentColumn, currentRow, cellIndex] = (int)cellSize.Width;
                    if (cellIndex == 0)
                    {
                        cellHeights[currentColumn, currentRow] = (int)cellSize.Height;
                    }
                    else
                    {
                        cellHeights[currentColumn, currentRow] =
                            (int)Math.Max(cellHeights[currentColumn, currentRow], cellSize.Height);
                    }
                }

                // Advance to the next row/column. Break if number of legend items exceed
                // number of availabale rows/columns.
                ++currentRow;
                if (currentRow >= numberOfRowsPerColumn[currentColumn])
                {
                    ++currentColumn;
                    currentRow = 0;
                    if (currentColumn >= numberOfColumns)
                    {
                        // Check if we were able to fit all the items
                        if (legendItemIndex < numberOfItemsToCheck - 1)
                        {
                            fitFlag = false;
                        }
                        break;
                    }
                }
            }

            //*************************************************************************
            //** For each sub-column get the maximum cell width
            //*************************************************************************
            subColumnSizes = new int[numberOfColumns, numberOfSubColumns];
            bool secondIterationRequired = false;
            for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
            {
                for (int currentSubColumn = 0; currentSubColumn < numberOfSubColumns; currentSubColumn++)
                {
                    int width = 0;
                    for (currentRow = 0; currentRow < numberOfRowsPerColumn[currentColumn]; currentRow++)
                    {
                        // Get current cell size
                        int cellWidth = cellWidths[currentColumn, currentRow, currentSubColumn];

                        // Skip overlapped cells and cells that use ColumnSpan during the
                        // first iteration. Their size will be determined during the
                        // second iteration.
                        if (cellWidth < 0)
                        {
                            secondIterationRequired = true;
                            continue;
                        }
                        if (currentSubColumn + 1 < numberOfSubColumns)
                        {
                            int nextCellWidth = cellWidths[currentColumn, currentRow, currentSubColumn + 1];
                            if (nextCellWidth < 0)
                            {
                                continue;
                            }
                        }

                        // Get maximum width
                        width = Math.Max(width, cellWidth);
                    }

                    // Store maximum width in the array
                    subColumnSizes[currentColumn, currentSubColumn] = width;
                }
            }

            //*************************************************************************
            //** If leagend header text is used check if it fits into the currenly
            //** calculated sub-column sizes.
            //*************************************************************************

            for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
            {
                for (int currentSubColumn = 0; currentSubColumn < numberOfSubColumns; currentSubColumn++)
                {
                    if (currentSubColumn < CellColumns.Count)
                    {
                        LegendCellColumn legendColumn = CellColumns[currentSubColumn];
                        if (legendColumn.HeaderText.Length > 0)
                        {
                            // Note that extra "I" character added to add more horizontal spacing
                            SKSize headerTextSize = graph.MeasureStringAbs(legendColumn.HeaderText + "I", legendColumn.HeaderFont);
                            if (headerTextSize.Width > subColumnSizes[currentColumn, currentSubColumn])
                            {
                                // Set new width
                                subColumnSizes[currentColumn, currentSubColumn] = (int)headerTextSize.Width;

                                // Check for column maximum/minimum cell width restrictions
                                if (legendColumn.MinimumWidth >= 0)
                                {
                                    subColumnSizes[currentColumn, currentSubColumn] =
                                        (int)Math.Max(subColumnSizes[currentColumn, currentSubColumn], legendColumn.MinimumWidth * singleWCharacterSize.Width / 100f);
                                }
                                if (legendColumn.MaximumWidth >= 0)
                                {
                                    subColumnSizes[currentColumn, currentSubColumn] =
                                        (int)Math.Min(subColumnSizes[currentColumn, currentSubColumn], legendColumn.MaximumWidth * singleWCharacterSize.Width / 100f);
                                }
                            }
                        }
                    }
                }
            }

            //*************************************************************************
            //** Adjust width of the cells to fit cell content displayed across
            //** several cells (CellSpanning).
            //*************************************************************************
            if (secondIterationRequired)
            {
                for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
                {
                    for (int currentSubColumn = 0; currentSubColumn < numberOfSubColumns; currentSubColumn++)
                    {
                        for (currentRow = 0; currentRow < numberOfRowsPerColumn[currentColumn]; currentRow++)
                        {
                            // Get current cell size
                            int cellWidth = cellWidths[currentColumn, currentRow, currentSubColumn];

                            // Second iteration used to adjust width of the cells that are used to
                            // draw content across several horizontal cells (CellSpanning)
                            // Check if current cell will be spanned to the next ones
                            int cellSpan = 0;
                            while (currentSubColumn + cellSpan + 1 < numberOfSubColumns)
                            {
                                int nextCellWidth = cellWidths[currentColumn, currentRow, currentSubColumn + cellSpan + 1];
                                if (nextCellWidth >= 0)
                                {
                                    break;
                                }
                                ++cellSpan;
                            }

                            // Cell span was detected
                            if (cellSpan > 0)
                            {
                                // Calculate total width of current cell and all overlapped cells
                                int spanWidth = 0;
                                for (int index = 0; index <= cellSpan; index++)
                                {
                                    spanWidth += subColumnSizes[currentColumn, currentSubColumn + index];
                                }

                                // Check if current cell fits into the cell span
                                if (cellWidth > spanWidth)
                                {
                                    // Adjust last span cell width to fit all curent cell content
                                    subColumnSizes[currentColumn, currentSubColumn + cellSpan] += cellWidth - spanWidth;
                                }
                            }
                        }
                    }
                }
            }

            //*************************************************************************
            //** Check if equally spaced legend columns are used
            //*************************************************************************
            if (IsEquallySpacedItems)
            {
                // Makre sure that same sub-colimn width are used in all columns
                for (int currentSubColumn = 0; currentSubColumn < numberOfSubColumns; currentSubColumn++)
                {
                    int width = 0;
                    for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
                    {
                        width = Math.Max(width, subColumnSizes[currentColumn, currentSubColumn]);
                    }

                    // Set new sub-column width for each column
                    for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
                    {
                        subColumnSizes[currentColumn, currentSubColumn] = width;
                    }
                }
            }

            //*************************************************************************
            //** Calculate total width and height occupied by all cells
            //*************************************************************************
            int totalWidth = 0;
            int totalTableColumnSpacingWidth = 0;
            for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
            {
                // Add up all sub-columns
                for (int currentSubColumn = 0; currentSubColumn < numberOfSubColumns; currentSubColumn++)
                {
                    totalWidth += subColumnSizes[currentColumn, currentSubColumn];
                }

                // Add spacer between columns
                if (currentColumn < numberOfColumns - 1)
                {
                    totalTableColumnSpacingWidth += (int)GetSeparatorSize(ItemColumnSeparator).Width;
                }
            }

            int totalHeight = 0;
            for (currentColumn = 0; currentColumn < numberOfColumns; currentColumn++)
            {
                int columnHeight = 0;
                for (currentRow = 0; currentRow < numberOfRowsPerColumn[currentColumn]; currentRow++)
                {
                    columnHeight += cellHeights[currentColumn, currentRow];
                }

                totalHeight = Math.Max(totalHeight, columnHeight);
            }

            //*************************************************************************
            //** Check if everything fits
            //*************************************************************************
            horizontalSpaceLeft = (int)legendItemsAreaSize.Width - totalWidth - totalTableColumnSpacingWidth;
            if (horizontalSpaceLeft < 0)
            {
                fitFlag = false;
            }

            verticalSpaceLeft = (int)legendItemsAreaSize.Height - totalHeight;
            if (verticalSpaceLeft < 0)
            {
                fitFlag = false;
            }

            return fitFlag;
        }

        /// <summary>
        /// Draw Legend header.
        /// </summary>
        /// <param name="chartGraph">Chart graphics to draw the header on.</param>
        private void DrawLegendHeader(ChartGraphics chartGraph)
        {
            // Check if header should be drawn
            if (!_headerPosition.IsEmpty &&
                _headerPosition.Width > 0 &&
                _headerPosition.Height > 0)
            {
                float prevRightLocation = -1.0F;
                bool redrawLegendBorder = false;

                // Get Legend position
                SKRect legendPosition = chartGraph.GetAbsoluteRectangle(Position.ToSKRect()).Round();
                legendPosition.Top += /*this.offset.Height + */GetBorderSize();
                legendPosition.Bottom -= 2 * (_offset.Height + GetBorderSize());
                legendPosition.Left += GetBorderSize();
                legendPosition.Right -= 2 * GetBorderSize();
                if (GetBorderSize() > 0)
                {
                    legendPosition.Size = new(legendPosition.Height + 1, legendPosition.Width + 1);
                }

                // Check if at least 1 column header has non-empty background color
                bool headerBackFill = false;
                for (int subColumnIndex = 0; subColumnIndex < CellColumns.Count; subColumnIndex++)
                {
                    LegendCellColumn legendColumn = CellColumns[subColumnIndex];
                    if (legendColumn.HeaderBackColor != SKColor.Empty)
                    {
                        headerBackFill = true;
                    }
                }

                // Iterate through all columns
                for (int columnIndex = 0; columnIndex < _itemColumns; columnIndex++)
                {
                    int columnStart = 0;
                    int columnWidth = 0;

                    // Iterate through all sub-columns
                    int numberOfSubColumns = _subColumnSizes.GetLength(1);
                    for (int subColumnIndex = 0; subColumnIndex < numberOfSubColumns; subColumnIndex++)
                    {
                        // Calculate position of the header
                        SKRect rect = _headerPosition;
                        if (_horizontalSpaceLeft > 0)
                        {
                            rect.Left += (int)(_horizontalSpaceLeft / 2f);
                        }
                        if (prevRightLocation != -1)
                        {
                            rect.Left = prevRightLocation;
                        }
                        rect.Size = new(_subColumnSizes[columnIndex, subColumnIndex], rect.Height);
                        prevRightLocation = rect.Right;

                        // Remember column start position and update width
                        if (subColumnIndex == 0)
                        {
                            columnStart = (int)rect.Left;
                        }
                        columnWidth += (int)rect.Width;

                        // Make sure header position do not go outside of the legend
                        rect.Intersect(legendPosition);
                        if (rect.Width > 0 && rect.Height > 0)
                        {
                            // Define fill rectangle
                            SKRect fillRect = rect;

                            // Make sure header fill riches legend top border
                            if (_titlePosition.Height <= 0)
                            {
                                fillRect.Top -= _offset.Height;
                                fillRect.Bottom += _offset.Height;
                            }

                            // Stretch header fill rectangle and separators when vertical
                            // separator are used or if there is 1 column with header background
                            if ((_itemColumns == 1 && headerBackFill) ||
                                ItemColumnSeparator != LegendSeparatorStyle.None)
                            {
                                // For the first cell in the first column stretch filling
                                // to the left side of the legend
                                if (columnIndex == 0 && subColumnIndex == 0)
                                {
                                    int newX = (int)legendPosition.Left;
                                    columnWidth += columnStart - newX;
                                    columnStart = newX;
                                    fillRect.Right += fillRect.Left - legendPosition.Left;
                                    fillRect.Left = newX;
                                }

                                // For the last cell in the last column stretch filling
                                // to the right side of the legend
                                if (columnIndex == (_itemColumns - 1) &&
                                    subColumnIndex == (numberOfSubColumns - 1))
                                {
                                    columnWidth += (int)(legendPosition.Right - fillRect.Right + 1);
                                    fillRect.Right += legendPosition.Right - fillRect.Right + 1;
                                }

                                // For the first cell of any column except the first one
                                // make sure we also fill the item column spacing
                                if (columnIndex != 0 && subColumnIndex == 0)
                                {
                                    columnWidth += _itemColumnSpacingRel / 2;
                                    columnStart -= _itemColumnSpacingRel / 2;
                                    fillRect.Right += _itemColumnSpacingRel / 2.0f;
                                    fillRect.Left -= _itemColumnSpacingRel / 2.0f;
                                }

                                // For the last cell in all columns except the last one
                                // make sure we also fill the item column spacing
                                if (columnIndex != (_itemColumns - 1) &&
                                    subColumnIndex == (numberOfSubColumns - 1))
                                {
                                    columnWidth += _itemColumnSpacingRel / 2;
                                    fillRect.Right += _itemColumnSpacingRel / 2.0f;
                                }
                            }

                            if (subColumnIndex < CellColumns.Count)
                            {
                                // Draw header background
                                LegendCellColumn legendColumn = CellColumns[subColumnIndex];
                                if (legendColumn.HeaderBackColor != SKColor.Empty)
                                {
                                    redrawLegendBorder = true;

                                    // Fill title background
                                    if (fillRect.Right > legendPosition.Right)
                                    {
                                        fillRect.Right -= (legendPosition.Right - fillRect.Right);
                                    }
                                    if (fillRect.Left < legendPosition.Left)
                                    {
                                        fillRect.Left += legendPosition.Left - fillRect.Left;
                                        fillRect.Right -= legendPosition.Left - fillRect.Left;
                                    }
                                    fillRect.Intersect(legendPosition);
                                    chartGraph.FillRectangleRel(
                                        chartGraph.GetRelativeRectangle(fillRect),
                                        legendColumn.HeaderBackColor,
                                        ChartHatchStyle.None,
                                        string.Empty,
                                        ChartImageWrapMode.Tile,
                                        SKColor.Empty,
                                        ChartImageAlignmentStyle.Center,
                                        GradientStyle.None,
                                        SKColor.Empty,
                                        SKColor.Empty,
                                        0,
                                        ChartDashStyle.NotSet,
                                        SKColor.Empty,
                                        0,
                                        PenAlignment.Inset);
                                }

                                // Draw header text
                                using SKPaint textBrush = new() { Color = legendColumn.HeaderForeColor, Style = SKPaintStyle.Fill };
                                // Set text alignment
                                using StringFormat format = new();
                                format.Alignment = legendColumn.HeaderAlignment;
                                format.LineAlignment = StringAlignment.Center;
                                format.FormatFlags = StringFormatFlags.LineLimit;
                                format.Trimming = StringTrimming.EllipsisCharacter;

                                // Draw string using relative coordinates
                                chartGraph.DrawStringRel(
                                    legendColumn.HeaderText,
                                    legendColumn.HeaderFont,
                                    textBrush,
                                    chartGraph.GetRelativeRectangle(rect),
                                    format);
                            }
                        }
                    }

                    // Draw header separator for each column
                    SKRect separatorRect = _headerPosition;
                    separatorRect.Left = columnStart;
                    separatorRect.Right = separatorRect.Left + columnWidth;
                    if (HeaderSeparator == LegendSeparatorStyle.Line || HeaderSeparator == LegendSeparatorStyle.DoubleLine)
                    {
                        // NOTE: For some reason a line with a single pen width is drawn 1 pixel longer than
                        // any other line. Reduce width to solve the issue.
                        legendPosition.Right -= 1;
                    }
                    separatorRect.Intersect(legendPosition);
                    this.DrawSeparator(chartGraph, HeaderSeparator, HeaderSeparatorColor, true, separatorRect);

                    // Add spacing between columns
                    prevRightLocation += GetSeparatorSize(ItemColumnSeparator).Width;
                }

                // Draw legend border to solve any issues with header background overlapping
                if (redrawLegendBorder)
                {
                    chartGraph.FillRectangleRel(
                        chartGraph.GetRelativeRectangle(chartGraph.GetAbsoluteRectangle(Position.ToSKRect()).Round()),
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        BorderColor,
                        GetBorderSize(),
                        BorderDashStyle,
                        SKColor.Empty,
                        0,
                        PenAlignment.Inset);
                }

                // Add legend header hot region
                if (Common.ProcessModeRegions && !_headerPosition.IsEmpty)
                {
                    Common.HotRegionsList.AddHotRegion(chartGraph.GetRelativeRectangle(_headerPosition), this, ChartElementType.LegendHeader, true);
                }
            }
        }

        /// <summary>
        /// Draw Legend title.
        /// </summary>
        /// <param name="chartGraph">Chart graphics to draw the title on.</param>
        private void DrawLegendTitle(ChartGraphics chartGraph)
        {
            // Check if title text is specified and position recalculated
            if (Title.Length > 0 &&
                !_titlePosition.IsEmpty)
            {
                // Get Legend position
                SKRect legendPosition = chartGraph.GetAbsoluteRectangle(Position.ToSKRect()).Round();
                legendPosition.Top += GetBorderSize();
                legendPosition.Bottom -= 2 * GetBorderSize();
                legendPosition.Top += GetBorderSize();
                legendPosition.Right -= 2 * GetBorderSize();
                if (GetBorderSize() > 0)
                {
                    legendPosition.Size = new(legendPosition.Width + 1, legendPosition.Height + 1);
                }

                // Draw title background
                if (TitleBackColor != SKColor.Empty)
                {
                    // Fill title background
                    SKRect fillRect = _titlePosition;
                    fillRect.Intersect(legendPosition);
                    chartGraph.FillRectangleRel(
                        chartGraph.GetRelativeRectangle(fillRect),
                        TitleBackColor,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        SKColor.Empty,
                        0,
                        ChartDashStyle.NotSet,
                        SKColor.Empty,
                        0,
                        PenAlignment.Inset);
                }

                // Draw title text
                using (SKPaint textBrush = new() { Style = SKPaintStyle.Fill, Color = TitleForeColor })
                {
                    // Set text alignment
                    StringFormat format = new();
                    format.Alignment = TitleAlignment;
                    //format.LineAlignment = StringAlignment.Center;

                    // Shift text rectangle by the top offset amount
                    SKRect rect = _titlePosition;
                    rect.Top += _offset.Height;
                    rect.Left += _offset.Width;
                    rect.Left += GetBorderSize();
                    rect.Right -= GetBorderSize() * 2 + _offset.Width;

                    // Draw string using relative coordinates
                    rect.Intersect(legendPosition);
                    chartGraph.DrawStringRel(
                        Title.Replace("\\n", "\n"),
                        TitleFont,
                        textBrush,
                        chartGraph.GetRelativeRectangle(rect),
                        format);
                }

                // Draw title separator
                SKRect separatorPosition = _titlePosition;
                //if (TitleSeparator == LegendSeparatorStyle.Line || TitleSeparator == LegendSeparatorStyle.DoubleLine)
                //{
                //    // NOTE: For some reason a line with a single pen width is drawn 1 pixel longer than
                //    // any other line. Reduce width to solve the issue.
                //    legendPosition.Width -= 1;
                //}
                separatorPosition.Intersect(legendPosition);
                this.DrawSeparator(chartGraph, TitleSeparator, TitleSeparatorColor, true, separatorPosition);

                // Draw legend border to solve any issues with title background overlapping
                if (TitleBackColor != SKColor.Empty ||
                    TitleSeparator != LegendSeparatorStyle.None)
                {
                    chartGraph.FillRectangleRel(
                        chartGraph.GetRelativeRectangle(chartGraph.GetAbsoluteRectangle(Position.ToSKRect()).Round()),
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        BorderColor,
                        GetBorderSize(),
                        BorderDashStyle,
                        SKColor.Empty,
                        0,
                        PenAlignment.Inset);
                }
            }
        }

        /// <summary>
        /// Draws specified legend separator.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="separatorType">Separator type.</param>
        /// <param name="color">Separator color.</param>
        /// <param name="horizontal">Flag that determines if separator is vertical or horizontal.</param>
        /// <param name="position">Separator position.</param>
        private void DrawSeparator(
            ChartGraphics chartGraph,
            LegendSeparatorStyle separatorType,
            SKColor color,
            bool horizontal,
            SKRect position)
        {
            // Temporary disable antialiasing
            SmoothingMode oldSmoothingMode = chartGraph.SmoothingMode;
            chartGraph.SmoothingMode = SmoothingMode.None;

            // Get line position in absolute coordinates
            SKRect rect = position;
            if (!horizontal)
            {
                rect.Left += (int)(_itemColumnSpacingRel / 2f);
                rect.Right -= _itemColumnSpacingRel;
            }
            if (separatorType == LegendSeparatorStyle.Line)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Left, rect.Bottom - 1),
                        new SKPoint(rect.Right, rect.Bottom - 1));
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Right - 1, rect.Top),
                        new SKPoint(rect.Right - 1, rect.Bottom));
                }
            }
            else if (separatorType == LegendSeparatorStyle.DashLine)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Dash,
                        new SKPoint(rect.Left, rect.Bottom - 1),
                        new SKPoint(rect.Right, rect.Bottom - 1));
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Dash,
                        new SKPoint(rect.Right - 1, rect.Top),
                        new SKPoint(rect.Right - 1, rect.Bottom));
                }
            }
            else if (separatorType == LegendSeparatorStyle.DotLine)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Dot,
                        new SKPoint(rect.Left, rect.Bottom - 1),
                        new SKPoint(rect.Right, rect.Bottom - 1));
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Dot,
                        new SKPoint(rect.Right - 1, rect.Top),
                        new SKPoint(rect.Right - 1, rect.Bottom));
                }
            }
            else if (separatorType == LegendSeparatorStyle.ThickLine)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.DrawLineAbs(
                        color,
                        2,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Left, rect.Bottom - 1f),
                        new SKPoint(rect.Right, rect.Bottom - 1f));
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.DrawLineAbs(
                        color,
                        2,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Right - 1f, rect.Top),
                        new SKPoint(rect.Right - 1f, rect.Bottom));
                }
            }
            else if (separatorType == LegendSeparatorStyle.DoubleLine)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Left, rect.Bottom - 3),
                        new SKPoint(rect.Right, rect.Bottom - 3));
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Left, rect.Bottom - 1),
                        new SKPoint(rect.Right, rect.Bottom - 1));
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Right - 3, rect.Top),
                        new SKPoint(rect.Right - 3, rect.Bottom));
                    chartGraph.DrawLineAbs(
                        color,
                        1,
                        ChartDashStyle.Solid,
                        new SKPoint(rect.Right - 1, rect.Top),
                        new SKPoint(rect.Right - 1, rect.Bottom));
                }
            }
            else if (separatorType == LegendSeparatorStyle.GradientLine)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.FillRectangleAbs(
                        new SKRect(rect.Left, rect.Bottom - 1f, rect.Width, 0f),
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.VerticalCenter,
                        color,
                        SKColor.Empty,
                        0,
                        ChartDashStyle.NotSet,
                        PenAlignment.Inset);
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.FillRectangleAbs(
                        new SKRect(rect.Right - 1f, rect.Top, 0f, rect.Height),
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.HorizontalCenter,
                        color,
                        SKColor.Empty,
                        0,
                        ChartDashStyle.NotSet,
                        PenAlignment.Inset);
                }
            }
            else if (separatorType == LegendSeparatorStyle.ThickGradientLine)
            {
                if (horizontal)
                {
                    // Draw horizontal line separator
                    chartGraph.FillRectangleAbs(
                        new SKRect(rect.Left, rect.Bottom - 2f, rect.Width, 1f),
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.VerticalCenter,
                        color,
                        SKColor.Empty,
                        0,
                        ChartDashStyle.NotSet,
                        PenAlignment.Inset);
                }
                else
                {
                    // Draw vertical line separator
                    chartGraph.FillRectangleAbs(
                        new SKRect(rect.Right - 2f, rect.Top, 1f, rect.Height),
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.HorizontalCenter,
                        color,
                        SKColor.Empty,
                        0,
                        ChartDashStyle.NotSet,
                        PenAlignment.Inset);
                }
            }

            // Restore smoothing
            chartGraph.SmoothingMode = oldSmoothingMode;
        }

        /// <summary>
        /// Add all series legend into items collection and then
        /// add custom legend items.
        /// </summary>
        private void FillLegendItemsCollection()
        {
            // Clear all items
            legendItems.Clear();

            // Check that there is no invalid legend names in the series
            foreach (Series series in Common.DataManager.Series)
            {
                if (Common.ChartPicture.Legends.IndexOf(series.Legend) < 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionLegendReferencedInSeriesNotFound(series.Name, series.Legend)));
                }
            }

            // Flag which indicates that series requires legend items to be reversed
            bool seriesWithReversedLegendItemsPresent = false;

            // Add legend items based on the exsisting chart series
            foreach (Series series in Common.DataManager.Series)
            {
                // Check if series uses this legend
                // VSTS issue #140694 fix: support of series.Legend = "Default";
                if (Common.ChartPicture.Legends[series.Legend] != this)
                {
                    continue;
                }

                // Make sure series is assigned to the chart area
                if (series.ChartArea.Length > 0)
                {
                    // Check if chart area name is valid
                    bool areaNameFound = false;
                    foreach (ChartArea area in Common.ChartPicture.ChartAreas)
                    {
                        if (area.Name == series.ChartArea)
                        {
                            areaNameFound = true;
                            break;
                        }
                    }

                    // Check if series is visible and valid chart area name was used
                    if (series.IsVisible() && areaNameFound)
                    {
                        // Check if we should add all data points into the legend
                        IChartType chartType = Common.ChartTypeRegistry.GetChartType(series.ChartTypeName);

                        // Check if series legend items should be reversed
                        if (LegendItemOrder == LegendItemOrder.Auto &&
                            (series.ChartType == SeriesChartType.StackedArea ||
                                series.ChartType == SeriesChartType.StackedArea100 ||
                                series.ChartType == SeriesChartType.Pyramid ||
                                series.ChartType == SeriesChartType.StackedColumn ||
                                series.ChartType == SeriesChartType.StackedColumn100))
                        {
                            seriesWithReversedLegendItemsPresent = true;
                        }
                        // Add item(s) based on series points label and fore color
                        if (chartType.DataPointsInLegend)
                        {
                            // Check if data points have X values set
                            bool xValuesSet = false;
                            foreach (DataPoint point in series.Points)
                            {
                                if (point.XValue != 0.0)
                                {
                                    xValuesSet = true;
                                    break;
                                }
                            }

                            // Add legend items for each point
                            int index = 0;
                            foreach (DataPoint point in series.Points)
                            {
                                // Do not show empty data points in the legend
                                if (point.IsEmpty)
                                {
                                    ++index;
                                    continue;
                                }

                                // Point should not be shown in the legend
                                if (!point.IsVisibleInLegend)
                                {
                                    ++index;
                                    continue;
                                }

                                // Create new legend item
                                LegendItem item = new(point.Label, point.Color, "");

                                // Check if series is drawn in 3D chart area
                                bool area3D = Common.Chart.ChartAreas[series.ChartArea].Area3DStyle.Enable3D;

                                // Set legend item appearance properties
                                item.SetAttributes(Common, series);
                                item.SetAttributes(point, area3D);

                                // Set chart image map properties
                                item.ToolTip = point.ReplaceKeywords(point.LegendToolTip);
                                item.Name = point.ReplaceKeywords(point.LegendText);

                                item.SeriesPointIndex = index++;
                                if (item.Name.Length == 0)
                                {
                                    item.Name = point.ReplaceKeywords((point.Label.Length > 0) ? point.Label : point.AxisLabel);
                                }

                                // If legend item name is not defined - try using the X value
                                if (item.Name.Length == 0 && xValuesSet)
                                {
                                    item.Name = ValueConverter.FormatValue(
                                        series.Chart,
                                        this,
                                        Tag,
                                        point.XValue,
                                        "", // Do not use point label format! For Y values only! point.LabelFormat,
                                        point.series.XValueType,
                                        ChartElementType.LegendItem);
                                }

                                // If legend item name is not defined - use index
                                if (item.Name.Length == 0)
                                {
                                    item.Name = "Point " + index;
                                }

                                // Add legend item cells based on predefined columns
                                item.AddAutomaticCells(this);
                                foreach (LegendCell cell in item.Cells)
                                {
                                    if (cell.Text.Length > 0)
                                    {
                                        // #LEGENDTEXT - series name
                                        cell.Text = cell.Text.Replace(KeywordName.LegendText, item.Name);

                                        // Process rest of the keywords
                                        cell.Text = point.ReplaceKeywords(cell.Text);
                                        cell.ToolTip = point.ReplaceKeywords(cell.ToolTip);
                                    }
                                }

                                legendItems.Add(item);
                            }
                        }

                        // Add item based on series name and fore color
                        else
                        {
                            // Point should not be shown in the legend
                            if (!series.IsVisibleInLegend)
                            {
                                continue;
                            }

                            // Create legend item
                            LegendItem item = new(series.Name, series.Color, "");
                            item.SetAttributes(Common, series);

                            item.ToolTip = series.ReplaceKeywords(series.LegendToolTip);

                            if (series.LegendText.Length > 0)
                            {
                                item.Name = series.ReplaceKeywords(series.LegendText);
                            }

                            // Add legend item cells based on predefined columns
                            item.AddAutomaticCells(this);
                            foreach (LegendCell cell in item.Cells)
                            {
                                if (cell.Text.Length > 0)
                                {
                                    // #LEGENDTEXT - series name
                                    cell.Text = cell.Text.Replace(KeywordName.LegendText, item.Name);

                                    // Process rest of the keywords
                                    cell.Text = series.ReplaceKeywords(cell.Text);
                                    cell.ToolTip = series.ReplaceKeywords(cell.ToolTip);
                                }
                            }

                            legendItems.Add(item);
                        }
                    }
                }
            }

            // Check if series legend items should be reversed
            if (LegendItemOrder == LegendItemOrder.SameAsSeriesOrder ||
                (LegendItemOrder == LegendItemOrder.Auto && seriesWithReversedLegendItemsPresent))
            {
                // Reversed series generated legend items
                legendItems.Reverse();
            }

            // Add custom items
            foreach (LegendItem item in _customLegends)
            {
                if (item.Enabled)
                {
                    legendItems.Add(item);
                }
            }


            // Add legend item cells based on predefined columns
            foreach (LegendItem item in legendItems)
            {
                item.AddAutomaticCells(this);
            }
        }

        /// <summary>
        /// Get visible legend border size.
        /// </summary>
        /// <returns>Visible legend border size.</returns>
        private int GetBorderSize()
        {
            if (BorderWidth > 0 &&
                BorderDashStyle != ChartDashStyle.NotSet &&
                BorderColor != SKColor.Empty &&
                BorderColor != SKColors.Transparent)
            {
                return BorderWidth;
            }
            return 0;
        }

        /// <summary>
        /// Gets single cell position in relative coordinates.
        /// </summary>
        /// <param name="columnIndex">Cell column index.</param>
        /// <param name="rowIndex">Cell row index.</param>
        /// <param name="cellIndex">Index of the cell in the legend item.</param>
        /// <param name="itemHalfSpacing">Half legend item spacing in relative coordinates.</param>
        /// <returns></returns>
        private SKRect GetCellPosition(
            int columnIndex,
            int rowIndex,
            int cellIndex,
            SKSize itemHalfSpacing)
        {
            SKRect cellPosition = _legendItemsAreaPosition;

            //*****************************************************************
            //** Get cell Top location
            //*****************************************************************
            for (int index = 0; index < rowIndex; index++)
            {
                cellPosition.Top += _cellHeights[columnIndex, index];
            }
            if (itemHalfSpacing.Height > 0)
            {
                cellPosition.Top += itemHalfSpacing.Height * rowIndex * 2 + itemHalfSpacing.Height;
            }

            //*****************************************************************
            //** Get cell Left location
            //*****************************************************************

            // Add extar space left after auto fitting
            if (_horizontalSpaceLeft > 0)
            {
                cellPosition.Left += itemHalfSpacing.Width;
            }

            // Calculate how many sub-columns (cells) this legend has
            int numberOfSubColumns = GetNumberOfCells();

            // Iterate through all prev. columns
            for (int index = 0; index < columnIndex; index++)
            {
                // Add width of previous columns
                for (int subColumnIndex = 0; subColumnIndex < numberOfSubColumns; subColumnIndex++)
                {
                    cellPosition.Left += _subColumnSizes[index, subColumnIndex];
                }

                // Add width of separator for the previous columns
                cellPosition.Left += GetSeparatorSize(ItemColumnSeparator).Width;
            }
            // Add width of current column cells
            for (int subColumnIndex = 0; subColumnIndex < cellIndex; subColumnIndex++)
            {
                cellPosition.Left += _subColumnSizes[columnIndex, subColumnIndex];
            }

            //*****************************************************************
            //** Get cell Height
            //*****************************************************************
            cellPosition.Bottom = cellPosition.Top + _cellHeights[columnIndex, rowIndex];

            //*****************************************************************
            //** Get cell Width
            //*****************************************************************
            cellPosition.Right = cellPosition.Left + _subColumnSizes[columnIndex, cellIndex];

            return cellPosition;
        }

        /// <summary>
        /// Gets column height.
        /// </summary>
        /// <param name="columnIndex">Index of the column to get the height for.</param>
        /// <returns>Column height in relative coordinates.</returns>
        private int GetColumnHeight(int columnIndex)
        {
            // Calculate current column height
            int columnHeight = 0;
            for (int rowIndex = 0; rowIndex < _numberOfRowsPerColumn[columnIndex]; rowIndex++)
            {
                columnHeight += _cellHeights[columnIndex, rowIndex];
            }

            return columnHeight;
        }

        /// <summary>
        /// Gets legend header size in relative coordinates.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="legendColumn">Legend column to get the header for.</param>
        /// <returns>Legend yitle size.</returns>
        private SKSize GetHeaderSize(ChartGraphics chartGraph, LegendCellColumn legendColumn)
        {
            SKSize headerSize = SKSize.Empty;
            if (legendColumn.HeaderText.Length > 0)
            {
                // Measure title text size
                headerSize = chartGraph.MeasureStringAbs(
                    legendColumn.HeaderText.Replace("\\n", "\n") + "I",
                    legendColumn.HeaderFont);

                // Add text spacing
                headerSize.Height += _offset.Height;
                headerSize.Width += _offset.Width;

                // Add space required for the title separator
                headerSize.Height += GetSeparatorSize(HeaderSeparator).Height;
            }

            return headerSize;
        }

        /// <summary>
        /// Helper method which returns current legend table style.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <returns>Legend table style.</returns>
        private LegendTableStyle GetLegendTableStyle(ChartGraphics chartGraph)
        {
            LegendTableStyle style = TableStyle;
            if (TableStyle == LegendTableStyle.Auto)
            {
                if (Position.Auto)
                {
                    // If legend is automatically positioned, use docking
                    // do determine preffered table style
                    if (Docking == Docking.Left ||
                        Docking == Docking.Right)
                    {
                        return LegendTableStyle.Tall;
                    }
                    else
                    {
                        return LegendTableStyle.Wide;
                    }
                }
                else
                {
                    // If legend is custom positioned, use legend width and heiht
                    // to determine the best table layout.
                    SKSize legendPixelSize = chartGraph.GetAbsoluteRectangle(Position.ToSKRect()).Size;
                    if (legendPixelSize.Width < legendPixelSize.Height)
                    {
                        return LegendTableStyle.Tall;
                    }
                    else
                    {
                        return LegendTableStyle.Wide;
                    }
                }
            }

            return style;
        }

        /// <summary>
        /// Gets maximum number of rows in all columns.
        /// </summary>
        /// <returns>Maximum number of rows.</returns>
        private int GetMaximumNumberOfRows()
        {
            return GetMaximumNumberOfRows(_numberOfRowsPerColumn);
        }

        /// <summary>
        /// Gets maximum number of rows in all columns.
        /// </summary>
        /// <param name="rowsPerColumn">Array that stores number of rows per column.</param>
        /// <returns>Maximum number of rows.</returns>
        private static int GetMaximumNumberOfRows(int[] rowsPerColumn)
        {
            // Find column with maximum number of rows
            int maxNumberOfColumns = 0;
            if (rowsPerColumn != null)
            {
                for (int columnIndex = 0; columnIndex < rowsPerColumn.Length; columnIndex++)
                {
                    maxNumberOfColumns = Math.Max(maxNumberOfColumns, rowsPerColumn[columnIndex]);
                }
            }
            return maxNumberOfColumns;
        }

        /// <summary>
        /// Gets maximum number of legend cells defined as Column objects
        /// or Cells in the custom legend items.
        /// </summary>
        /// <returns>Maximum number of cells.</returns>
        private int GetNumberOfCells()
        {
            // Calculate cell number if it was not previously cached
            if (_numberOfCells < 0)
            {
                // Initialize with number of defined columns
                _numberOfCells = CellColumns.Count;

                // Check if number of cells in legend items exceed number of defined columns
                foreach (LegendItem legendItem in legendItems)
                {
                    _numberOfCells = Math.Max(_numberOfCells, legendItem.Cells.Count);
                }
            }
            return _numberOfCells;
        }

        /// <summary>
        /// Get number of columns and rows that can be fit in specified size.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="legendSize">Legend size.</param>
        /// <param name="numberOfItemsToCheck">Number of legend items to check.</param>
        /// <param name="numberOfRowsPerColumn">Array with number of rows per each column.</param>
        /// <param name="columnNumber">Returns number of columns.</param>
        private void GetNumberOfRowsAndColumns(
            ChartGraphics chartGraph,
            SKSize legendSize,
            int numberOfItemsToCheck,
            out int[] numberOfRowsPerColumn,
            out int columnNumber)
        {
            GetNumberOfRowsAndColumns(
                chartGraph,
                legendSize,
                numberOfItemsToCheck,
                out numberOfRowsPerColumn,
                out columnNumber,
                out int _,
                out int _);
        }

        /// <summary>
        /// Get number of columns and rows that can be fit in specified size.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="legendSize">Legend size.</param>
        /// <param name="numberOfItemsToCheck">Legend items number to check.</param>
        /// <param name="numberOfRowsPerColumn">Array with number of rows per each column.</param>
        /// <param name="columnNumber">Returns number of columns.</param>
        /// <param name="horSpaceLeft">Returns horizontal spacing left.</param>
        /// <param name="vertSpaceLeft">Returns vertical spacing left.</param>
        private void GetNumberOfRowsAndColumns(
            ChartGraphics chartGraph,
            SKSize legendSize,
            int numberOfItemsToCheck,
            out int[] numberOfRowsPerColumn,
            out int columnNumber,
            out int horSpaceLeft,
            out int vertSpaceLeft)
        {
            // Initialize output parameters
            numberOfRowsPerColumn = null;
            columnNumber = 1;

            // If number of items to check is nor set use total number of items in the collection
            if (numberOfItemsToCheck < 0)
            {
                numberOfItemsToCheck = legendItems.Count;
            }

            // Check legend style
            if (LegendStyle == LegendStyle.Column || numberOfItemsToCheck <= 1)
            {
                columnNumber = 1;
                numberOfRowsPerColumn = new int[] { numberOfItemsToCheck };
            }
            else if (LegendStyle == LegendStyle.Row)
            {
                columnNumber = numberOfItemsToCheck;
                numberOfRowsPerColumn = new int[columnNumber];
                for (int index = 0; index < columnNumber; index++)
                {
                    numberOfRowsPerColumn[index] = 1;
                }
            }
            else if (LegendStyle == LegendStyle.Table)
            {
                // Start with 1 column and 1 row
                columnNumber = 1;
                numberOfRowsPerColumn = new int[] { 1 };

                // Get legend table style and adjust number of columns and rows accordinly
                LegendTableStyle tableStyle = GetLegendTableStyle(chartGraph);

                //*********************************************************************************
                //** Tall table layout
                //*********************************************************************************
                if (tableStyle == LegendTableStyle.Tall)
                {
                    // Iterate from second item trying to add them and check if their fit
                    bool exitLoop = false;
                    int legendItemIndex;
                    for (legendItemIndex = 1; !exitLoop && legendItemIndex < numberOfItemsToCheck; legendItemIndex++)
                    {
                        // Try to increase number of rows in the current column
                        ++numberOfRowsPerColumn[columnNumber - 1];

                        // Check if legend items fit into the legend area
                        bool autoFitDone = CheckLegendItemsFit(
                            chartGraph,
                            legendSize,
                            legendItemIndex + 1,
                            _autoFitFontSizeAdjustment,
                            columnNumber,
                            numberOfRowsPerColumn,
                            out _subColumnSizes,
                            out _cellHeights,
                            out horSpaceLeft,
                            out vertSpaceLeft);

                        // Check if we fit or if we have just one column that do not fit
                        // horizontally but still have vertical space.
                        if (autoFitDone ||
                            ((columnNumber == 1 || horSpaceLeft < 0) && vertSpaceLeft > 0))
                        {
                            // Continue adding rows to the current column
                            continue;
                        }
                        else
                        {
                            // Reduce number of rows in the current column
                            if (numberOfRowsPerColumn[columnNumber - 1] > 1)
                            {
                                --numberOfRowsPerColumn[columnNumber - 1];
                            }

                            // Get half of average column width
                            int averageColumnWidth = 0;
                            if (horSpaceLeft > 0)
                            {
                                averageColumnWidth = (int)Math.Round((double)(legendSize.Width - horSpaceLeft) / columnNumber) / 2;
                            }

                            // Check if number of columns can be increased
                            if (columnNumber < 50 && horSpaceLeft >= averageColumnWidth)
                            {
                                // Add new column
                                ++columnNumber;

                                // Resize array that stores number of rows per column
                                int[] tempArray = numberOfRowsPerColumn;
                                numberOfRowsPerColumn = new int[columnNumber];
                                for (int index = 0; index < tempArray.Length; index++)
                                {
                                    numberOfRowsPerColumn[index] = tempArray[index];
                                }
                                numberOfRowsPerColumn[columnNumber - 1] = 1;

                                // If last legend item is moved into a new column
                                // call the auto fitting method before leaving the loop
                                if (legendItemIndex == numberOfItemsToCheck - 1)
                                {
                                    CheckLegendItemsFit(
                                        chartGraph,
                                        legendSize,
                                        legendItemIndex + 1,
                                        _autoFitFontSizeAdjustment,
                                        columnNumber,
                                        numberOfRowsPerColumn,
                                        out _subColumnSizes,
                                        out _cellHeights,
                                        out horSpaceLeft,
                                        out vertSpaceLeft);
                                }
                            }
                            else
                            {
                                exitLoop = true;
                            }
                        }
                    }

                    // Check if we end up with legend with multiple columns
                    // where last column has sinificantly lower height of all rows
                    if (columnNumber > 1)
                    {
                        // Try reducing number of rows in the "tall" calumns and move them
                        // into the last column.
                        bool done = false;
                        while (!done)
                        {
                            // By default no more iterations required
                            done = true;

                            // Find maximum column height not taking the last row in consideration
                            int maxColumnHeight = -1;
                            for (int columnIndex = 0; columnIndex < columnNumber; columnIndex++)
                            {
                                // Calculate current column height not taking the last row in consideration
                                int columnHeight = 0;
                                for (int rowIndex = 0; rowIndex < _numberOfRowsPerColumn[columnIndex] - 1; rowIndex++)
                                {
                                    columnHeight += _cellHeights[columnIndex, rowIndex];
                                }

                                // Find maximum height
                                maxColumnHeight = Math.Max(maxColumnHeight, columnHeight);
                            }

                            // Calculate total height of items in the last row
                            int totalHieghtOfItemInLastRow = 0;
                            for (int columnIndex = 0; columnIndex < (columnNumber - 1); columnIndex++)
                            {
                                if (_numberOfRowsPerColumn[columnIndex] > 1)
                                {
                                    totalHieghtOfItemInLastRow += _cellHeights[columnIndex, _numberOfRowsPerColumn[columnIndex] - 1];
                                }
                            }

                            // Check if rows are available for removal
                            if (totalHieghtOfItemInLastRow > 0)
                            {
                                // Get last column height
                                int lastColumnHeight = GetColumnHeight(columnNumber - 1);

                                // Check if all items in the last row can vertically fit in last column
                                if ((lastColumnHeight + totalHieghtOfItemInLastRow) <= maxColumnHeight)
                                {
                                    // Reduce number of rows in all columns except last
                                    int itemsToAdd = 0;
                                    for (int columnIndex = 0; columnIndex < (columnNumber - 1); columnIndex++)
                                    {
                                        if (_numberOfRowsPerColumn[columnIndex] > 1)
                                        {
                                            --_numberOfRowsPerColumn[columnIndex];
                                            ++itemsToAdd;
                                        }
                                    }

                                    // Add rows to last column
                                    if (itemsToAdd > 0)
                                    {
                                        // Add roes into the last column
                                        _numberOfRowsPerColumn[columnNumber - 1] += itemsToAdd;

                                        // Check if legend items fit into the legend area
                                        _ = CheckLegendItemsFit(
                                            chartGraph,
                                            legendSize,
                                            legendItemIndex + 1,
                                            _autoFitFontSizeAdjustment,
                                            columnNumber,
                                            numberOfRowsPerColumn,
                                            out _subColumnSizes,
                                            out _cellHeights,
                                            out _,
                                            out _);

                                        // Try doing one more time
                                        done = false;
                                    }
                                }
                            }
                        }
                    }
                }
                //*********************************************************************************
                //** Wide table layout
                //*********************************************************************************
                else if (tableStyle == LegendTableStyle.Wide)
                {
                    // Iterate from second item trying to add them and check if they fit
                    bool exitLoop = false;
                    int legendItemIndex = 1;
                    for (legendItemIndex = 1; !exitLoop && legendItemIndex < numberOfItemsToCheck; legendItemIndex++)
                    {
                        // Try to increase number of columns
                        ++columnNumber;

                        // Resize array that stores number of rows per column
                        int[] tempArray = numberOfRowsPerColumn;
                        numberOfRowsPerColumn = new int[columnNumber];
                        for (int index = 0; index < tempArray.Length; index++)
                        {
                            numberOfRowsPerColumn[index] = tempArray[index];
                        }
                        numberOfRowsPerColumn[columnNumber - 1] = 1;

                        // Check if legend items fit into the legend area
                        bool autoFitDone = CheckLegendItemsFit(
                            chartGraph,
                            legendSize,
                            legendItemIndex + 1,
                            _autoFitFontSizeAdjustment,
                            columnNumber,
                            numberOfRowsPerColumn,
                            out _subColumnSizes,
                            out _cellHeights,
                            out horSpaceLeft,
                            out vertSpaceLeft);

                        // Check if we fit or if we have just one row that do not fit
                        // vertically but still have horizontal space.
                        if (autoFitDone ||
                            ((GetMaximumNumberOfRows(numberOfRowsPerColumn) == 1 || vertSpaceLeft < 0) && horSpaceLeft > 0))
                        {
                            // Continue adding columns
                            continue;
                        }
                        else
                        {
                            // Remove columns and increase number of rows
                            bool columnFitting = true;
                            while (columnFitting)
                            {
                                columnFitting = false;

                                // If we can't fit current number of columns reduce current column number
                                int rowsToAdd = 0;
                                if (columnNumber > 1)
                                {
                                    rowsToAdd = numberOfRowsPerColumn[columnNumber - 1];
                                    --columnNumber;

                                    // Resize array that stores number of rows per column
                                    tempArray = numberOfRowsPerColumn;
                                    numberOfRowsPerColumn = new int[columnNumber];
                                    for (int index = 0; index < columnNumber; index++)
                                    {
                                        numberOfRowsPerColumn[index] = tempArray[index];
                                    }
                                }

                                // We may need to add more than 1 row
                                for (int indexRowToAdd = 0; indexRowToAdd < rowsToAdd; indexRowToAdd++)
                                {
                                    // Find first column with smallest height
                                    int smallestColumnIndex = -1;
                                    int columnMinHeight = int.MaxValue;
                                    for (int columnIndex = 0; columnIndex < columnNumber; columnIndex++)
                                    {
                                        int columnHeight = GetColumnHeight(columnIndex);
                                        int nextColumnFirstItemHeight = 0;
                                        if (columnIndex < columnNumber - 1)
                                        {
                                            nextColumnFirstItemHeight = _cellHeights[columnIndex + 1, 0];
                                        }
                                        if (columnHeight < columnMinHeight &&
                                            (columnHeight + nextColumnFirstItemHeight) < legendSize.Height)
                                        {
                                            // Remember column index and height
                                            columnMinHeight = columnHeight;
                                            smallestColumnIndex = columnIndex;
                                        }
                                    }

                                    // No more items can fit
                                    if (smallestColumnIndex < 0)
                                    {
                                        // Check if legend items fit into the legend area
                                        autoFitDone = CheckLegendItemsFit(
                                            chartGraph,
                                            legendSize,
                                            legendItemIndex + 1,
                                            _autoFitFontSizeAdjustment,
                                            columnNumber,
                                            numberOfRowsPerColumn,
                                            out _subColumnSizes,
                                            out _cellHeights,
                                            out horSpaceLeft,
                                            out _);

                                        exitLoop = true;
                                        break;
                                    }

                                    // Add new row to the smallest column
                                    ++numberOfRowsPerColumn[smallestColumnIndex];

                                    // Check if next column will be removed if it contains only 1 row
                                    if (smallestColumnIndex < (columnNumber - 1) && numberOfRowsPerColumn[smallestColumnIndex + 1] == 1)
                                    {
                                        // Shift number of rows per column
                                        tempArray = numberOfRowsPerColumn;
                                        for (int index = smallestColumnIndex + 1; index < tempArray.Length - 1; index++)
                                        {
                                            numberOfRowsPerColumn[index] = tempArray[index + 1];
                                        }
                                        numberOfRowsPerColumn[columnNumber - 1] = 1;
                                    }

                                    // Check if legend items fit into the legend area
                                    autoFitDone = CheckLegendItemsFit(
                                        chartGraph,
                                        legendSize,
                                        legendItemIndex + 1,
                                        _autoFitFontSizeAdjustment,
                                        columnNumber,
                                        numberOfRowsPerColumn,
                                        out _subColumnSizes,
                                        out _cellHeights,
                                        out horSpaceLeft,
                                        out vertSpaceLeft);
                                }

                                // If there is more than 1 column and items do not fit
                                // horizontally - reduce number of columns.
                                if (!autoFitDone &&
                                    horSpaceLeft < 0f &&
                                    columnNumber > 1)
                                {
                                    columnFitting = true;
                                }
                            }
                        }
                    }
                }
            }

            // Check if items fit and how much empty space left
            CheckLegendItemsFit(
                chartGraph,
                legendSize,
                -1,
                _autoFitFontSizeAdjustment,
                columnNumber,
                numberOfRowsPerColumn,
                out _subColumnSizes,
                out _cellHeights,
                out horSpaceLeft,
                out vertSpaceLeft);
        }

        /// <summary>
        /// Calculates the optimal size of the legend.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="maxSizeRel">Max size for the legend.</param>
        /// <returns>Legend optimal size.</returns>
        private SKSize GetOptimalSize(ChartGraphics chartGraph, SKSize maxSizeRel)
        {
            // Reset some values
            _offset = SKSize.Empty;
            _itemColumns = 0;
            _horizontalSpaceLeft = 0;
            _verticalSpaceLeft = 0;
            _subColumnSizes = null;
            _numberOfRowsPerColumn = null;
            _cellHeights = null;
            autofitFont = null;
            _autoFitFontSizeAdjustment = 0;
            _numberOfCells = -1;
            _numberOfLegendItemsToProcess = -1;
            SKSize optimalSize = SKSize.Empty;

            // Convert to pixels
            SKSize maxSizeAbs = chartGraph.GetAbsoluteSize(maxSizeRel);
            SKSize maxSize = new((int)maxSizeAbs.Width, (int)maxSizeAbs.Height);

            // Clear all legend item cells cached information
            foreach (LegendItem legendItem in legendItems)
            {
                foreach (LegendCell cell in legendItem.Cells)
                {
                    cell.ResetCache();
                }
            }

            // Check if legend is enabled
            if (IsEnabled())
            {
                // Add all series legend into items collection and then add custom legend items
                FillLegendItemsCollection();

                // Call a notification event, so that legend items collection can be modified by user
                Common.Chart.CallOnCustomizeLegend(legendItems, Name);

                // Check if there are any items in the legend
                if (legendItems.Count > 0)
                {
                    //***********************************************************
                    //** Use size of the "W" character in current font to
                    //** calculate legend spacing
                    //***********************************************************
                    singleWCharacterSize = chartGraph.MeasureStringAbs("W", Font);
                    SKSize doubleCharacterSize = chartGraph.MeasureStringAbs("WW", Font);
                    singleWCharacterSize.Width = doubleCharacterSize.Width - singleWCharacterSize.Width;

                    // Calculate left, top offset and column spacing
                    _offset.Width = (int)Math.Ceiling(singleWCharacterSize.Width / 2f);
                    _offset.Height = (int)Math.Ceiling(singleWCharacterSize.Width / 3f);
                    _itemColumnSpacingRel = (int)(singleWCharacterSize.Width * (_itemColumnSpacing / 100f));
                    if (_itemColumnSpacingRel % 2 == 1)
                    {
                        _itemColumnSpacingRel += 1;
                    }

                    //***********************************************************
                    //** Add size required for the legend tile
                    //***********************************************************

                    SKSize titleSize = SKSize.Empty;
                    if (Title.Length > 0)
                    {
                        titleSize = GetTitleSize(chartGraph, maxSize);
                    }

                    //***********************************************************
                    //** Add size required for the legend header
                    //***********************************************************

                    SKSize highestHeader = SKSize.Empty;
                    foreach (LegendCellColumn legendColumn in CellColumns)
                    {
                        if (legendColumn.HeaderText.Length > 0)
                        {
                            // Measure header text size
                            SKSize headerSize = GetHeaderSize(chartGraph, legendColumn);

                            // Get header with maximum height
                            highestHeader.Height = Math.Max(highestHeader.Height, headerSize.Height);
                        }
                    }

                    //***********************************************************
                    //** Calculate size available for legend items
                    //***********************************************************
                    SKSize legenItemsMaxSize = maxSize;
                    legenItemsMaxSize.Width -= 2 * (_offset.Width + GetBorderSize());
                    legenItemsMaxSize.Height -= 2 * (_offset.Height + GetBorderSize());
                    legenItemsMaxSize.Height -= titleSize.Height;
                    legenItemsMaxSize.Height -= highestHeader.Height;

                    //***********************************************************
                    //** Calculate number of rows and columns depending on
                    //** the legend style
                    //***********************************************************
                    _autoFitFontSizeAdjustment = 0;

                    autofitFont = Font;
                    int vertSpaceLeft = 0;
                    int horizSpaceLeft = 0;
                    bool reduceFont = IsTextAutoFit;
                    bool autoFit = false;
                    do
                    {
                        // Get number of columns and rows that we can fit in the legend
                        GetNumberOfRowsAndColumns(
                            chartGraph,
                            legenItemsMaxSize,
                            -1,
                            out _numberOfRowsPerColumn,
                            out _itemColumns,
                            out horizSpaceLeft,
                            out vertSpaceLeft);

                        // Calculate total number of items fit and make sure we fit all of them
                        int itemsFit = 0;
                        for (int index = 0; index < _itemColumns; index++)
                        {
                            itemsFit += _numberOfRowsPerColumn[index];
                        }
                        autoFit = (horizSpaceLeft >= 0 && vertSpaceLeft >= 0 && itemsFit >= legendItems.Count);

                        // Check if items fit
                        if (reduceFont && !autoFit)
                        {
                            if ((Font.Size - _autoFitFontSizeAdjustment) > _autoFitMinFontSize)
                            {
                                // Reduce font size by one
                                ++_autoFitFontSizeAdjustment;

                                // Calculate new font size
                                int newFontSize = (int)Math.Round(Font.Size - _autoFitFontSizeAdjustment);
                                if (newFontSize < 1)
                                {
                                    // Font can't be less than size 1
                                    newFontSize = 1;
                                }

                                // Create new font
                                autofitFont = Common.ChartPicture.FontCache.GetFont(
                                    Font.Typeface.FamilyName,
                                    newFontSize,
                                    Font.Typeface.FontStyle);
                            }
                            else
                            {
                                reduceFont = false;
                            }
                        }
                    } while (reduceFont && !autoFit);

                    // Slightly reduce used space
                    horizSpaceLeft -= Math.Min(4, horizSpaceLeft);
                    vertSpaceLeft -= Math.Min(2, vertSpaceLeft);

                    //***********************************************************
                    //** Calculate legend size
                    //***********************************************************
                    optimalSize.Width = (legenItemsMaxSize.Width - horizSpaceLeft);
                    optimalSize.Width = Math.Max(optimalSize.Width, titleSize.Width);
                    optimalSize.Width += 2 * (_offset.Width + GetBorderSize());

                    optimalSize.Height = (legenItemsMaxSize.Height - vertSpaceLeft) + titleSize.Height + highestHeader.Height;
                    optimalSize.Height += 2 * (_offset.Height + GetBorderSize());

                    // Adjust legend items area height by size required to show
                    // visually (dots) that legend is truncated
                    if (horizSpaceLeft < 0 || vertSpaceLeft < 0)
                    {
                        optimalSize.Height += _truncatedDotsSize;
                    }

                    //***********************************************************
                    //** Make sure legend size do not exceed max. value
                    //***********************************************************
                    if (optimalSize.Width > maxSize.Width)
                    {
                        optimalSize.Width = maxSize.Width;
                    }
                    if (optimalSize.Height > maxSize.Height)
                    {
                        optimalSize.Height = maxSize.Height;
                    }
                    if (optimalSize.Width < 0)
                    {
                        optimalSize.Width = 0;
                    }
                    if (optimalSize.Height < 0)
                    {
                        optimalSize.Height = 0;
                    }
                }
            }

            // Convert result size from pixel to relative coordinates
            return chartGraph.GetRelativeSize(optimalSize);
        }

        /// <summary>
        /// Gets legend title size in relative coordinates.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="titleMaxSize">Maximum possible legend title size.</param>
        /// <returns>Legend yitle size.</returns>
        private SKSize GetTitleSize(ChartGraphics chartGraph, SKSize titleMaxSize)
        {
            SKSize titleSize = SKSize.Empty;
            if (Title.Length > 0)
            {
                // Adjust available space
                titleMaxSize.Width -= GetBorderSize() * 2 + _offset.Width;

                // Measure title text size
                titleSize = chartGraph.MeasureStringAbs(
                    Title.Replace("\\n", "\n"),
                    TitleFont,
                    titleMaxSize,
                    StringFormat.GenericTypographic);

                // Add text spacing
                titleSize.Height += _offset.Height;
                titleSize.Width += _offset.Width;

                // Add space required for the title separator
                titleSize.Height += GetSeparatorSize(TitleSeparator).Height;
            }

            return titleSize;
        }

        /// <summary>
        /// Recalculates legend information:
        ///   - legend items collection
        ///   - maximum text rectangle
        /// </summary>
        /// <param name="chartGraph">Reference to the chart graphics.</param>
        private void RecalcLegendInfo(ChartGraphics chartGraph)
        {
            // Reset some values
            SKRect legendPositionRel = _position.ToSKRect();
            SKRect legendPosition = chartGraph.GetAbsoluteRectangle(legendPositionRel).Round();

            //***********************************************************
            //** Use size of the "W" characters in current font to
            //** calculate legend spacing
            //***********************************************************
            singleWCharacterSize = chartGraph.MeasureStringAbs("W", Font);
            SKSize doubleCharacterSize = chartGraph.MeasureStringAbs("WW", Font);
            singleWCharacterSize.Width = doubleCharacterSize.Width - singleWCharacterSize.Width;

            // Calculate left, top offset and column spacing
            _offset.Width = (int)Math.Ceiling(singleWCharacterSize.Width / 2f);
            _offset.Height = (int)Math.Ceiling(singleWCharacterSize.Width / 3f);

            // Calculate item column spacing and make sure it is dividable by 2
            _itemColumnSpacingRel = (int)(singleWCharacterSize.Width * (_itemColumnSpacing / 100f));
            if (_itemColumnSpacingRel % 2 == 1)
            {
                _itemColumnSpacingRel += 1;
            }

            //***********************************************************
            //** Calculate how much space required for the title.
            //***********************************************************
            _titlePosition = SKRect.Empty;
            if (Title.Length > 0)
            {
                // Measure title text size
                SKSize titleSize = this.GetTitleSize(chartGraph, legendPosition.Size);

                // Set legend title position
                _titlePosition = new(
                    legendPosition.Location.X,
                    legendPosition.Location.Y,
                    legendPosition.Width,
                    Math.Min(legendPosition.Height, titleSize.Height));

                // Adjust legend items position height
                legendPosition.Bottom -= _titlePosition.Height;

                // Increase title top location by border height
                _titlePosition.Top += GetBorderSize();
            }

            //***********************************************************
            //** Calculate how much space required for the header.
            //***********************************************************
            _headerPosition = SKRect.Empty;

            // Find the largest (height only) header
            SKSize highestHeader = SKSize.Empty;
            foreach (LegendCellColumn legendColumn in CellColumns)
            {
                if (legendColumn.HeaderText.Length > 0)
                {
                    // Measure header text size
                    SKSize headerSize = GetHeaderSize(chartGraph, legendColumn);

                    // Get header with maximum height
                    highestHeader.Height = Math.Max(highestHeader.Height, headerSize.Height);
                }
            }

            // Check if any headers where found
            if (!highestHeader.IsEmpty)
            {
                // Set legend header position
                _headerPosition = new(
                    legendPosition.Location.X + GetBorderSize() + _offset.Width,
                    legendPosition.Location.Y + _titlePosition.Height,
                    legendPosition.Width - (GetBorderSize() + _offset.Width) * 2,
                    Math.Min(legendPosition.Height - _titlePosition.Height, highestHeader.Height));
                _headerPosition.Bottom = _headerPosition.Top + Math.Max(_headerPosition.Height, 0);

                // Adjust legend items position height
                legendPosition.Bottom -= _headerPosition.Height;

                // Increase header top location by border height
                _headerPosition.Top += GetBorderSize();
            }

            //***********************************************************
            //** Calculate size available for all legend items
            //***********************************************************
            _legendItemsAreaPosition = new(
                legendPosition.Left + _offset.Width + GetBorderSize(),
                legendPosition.Top + _offset.Height + GetBorderSize() + _titlePosition.Height + _headerPosition.Height,
                legendPosition.Width - 2 * (_offset.Width + GetBorderSize()),
                legendPosition.Height - 2 * (_offset.Height + GetBorderSize()));

            //***********************************************************
            //** Calculate number of rows and columns depending on
            //** the legend style
            //***********************************************************
            this.GetNumberOfRowsAndColumns(
                chartGraph,
                _legendItemsAreaPosition.Size,
                -1,
                out _numberOfRowsPerColumn,
                out _itemColumns,
                out _horizontalSpaceLeft,
                out _verticalSpaceLeft);

            //***********************************************************
            //** Try to fit all legend item cells reducing the font size
            //***********************************************************

            // Reset auto-fit font adjustment value and truncated legend flag
            _autoFitFontSizeAdjustment = 0;
            _legendItemsTruncated = false;

            // Check if legend items fit into the legend area
            bool autoFitDone = (_horizontalSpaceLeft >= 0 && _verticalSpaceLeft >= 0);

            // Calculate total number of items fit and make sure we fit all of them
            _numberOfLegendItemsToProcess = legendItems.Count;
            int itemsFit = 0;
            for (int index = 0; index < _itemColumns; index++)
            {
                itemsFit += _numberOfRowsPerColumn[index];
            }
            if (itemsFit < _numberOfLegendItemsToProcess)
            {
                autoFitDone = false;
            }

            // If items do not fit try reducing font or number of legend items
            autofitFont = Font;
            if (!autoFitDone)
            {
                do
                {
                    // Check if legend item font size can be reduced
                    if (IsTextAutoFit &&
                        (Font.Size - _autoFitFontSizeAdjustment) > _autoFitMinFontSize)
                    {
                        // Reduce font size by one
                        ++_autoFitFontSizeAdjustment;

                        // Calculate new font size
                        int newFontSize = (int)Math.Round(Font.Size - _autoFitFontSizeAdjustment);
                        if (newFontSize < 1)
                        {
                            // Font can't be less than size 1
                            newFontSize = 1;
                        }

                        // Create new font
                        autofitFont = Common.ChartPicture.FontCache.GetFont(
                            Font.Typeface.FamilyName,
                            newFontSize,
                            Font.Typeface.FontStyle);

                        // Calculate number of rows and columns
                        this.GetNumberOfRowsAndColumns(
                            chartGraph,
                            _legendItemsAreaPosition.Size,
                            -1,
                            out _numberOfRowsPerColumn,
                            out _itemColumns,
                            out _horizontalSpaceLeft,
                            out _verticalSpaceLeft);

                        autoFitDone = (_horizontalSpaceLeft >= 0 && _verticalSpaceLeft >= 0);

                        // Calculate total number of items fit and make sure we fit all of them
                        itemsFit = 0;
                        for (int index = 0; index < _itemColumns; index++)
                        {
                            itemsFit += _numberOfRowsPerColumn[index];
                        }
                        if (itemsFit < _numberOfLegendItemsToProcess)
                        {
                            autoFitDone = false;
                        }
                    }
                    else
                    {
                        // If font size cannot be reduced start removing legend items
                        if (_numberOfLegendItemsToProcess > 2)
                        {
                            // Handle case of 1 column that do not fit horizontally
                            if (_itemColumns == 1 && (_horizontalSpaceLeft < 0 && _verticalSpaceLeft >= 0))
                            {
                                autoFitDone = true;
                                _numberOfLegendItemsToProcess =
                                    Math.Min(_numberOfLegendItemsToProcess, _numberOfRowsPerColumn[0]);
                            }
                            // Handle case of 1 row that do not fit vertically
                            else if (GetMaximumNumberOfRows() == 1 && (_verticalSpaceLeft < 0 && _horizontalSpaceLeft >= 0))
                            {
                                autoFitDone = true;
                                _numberOfLegendItemsToProcess =
                                    Math.Min(_numberOfLegendItemsToProcess, _itemColumns);
                            }
                            else
                            {
                                // Adjust legend items area height by size required to show
                                // visually (dots) that legend is truncated
                                if (!_legendItemsTruncated)
                                {
                                    _legendItemsAreaPosition.Bottom -= _truncatedDotsSize;
                                }

                                // Remove last legend item
                                _legendItemsTruncated = true;
                                --_numberOfLegendItemsToProcess;

                                // RecalculateAxesScale number of rows and columns
                                this.GetNumberOfRowsAndColumns(
                                    chartGraph,
                                    _legendItemsAreaPosition.Size,
                                    _numberOfLegendItemsToProcess,
                                    out _numberOfRowsPerColumn,
                                    out _itemColumns);
                            }

                            // Make sure we show truncated legend symbols when not all items shown
                            if (autoFitDone &&
                                !_legendItemsTruncated &&
                                _numberOfLegendItemsToProcess < legendItems.Count)
                            {
                                // Adjust legend items area height by size required to show
                                // visually (dots) that legend is truncated
                                _legendItemsAreaPosition.Bottom -= _truncatedDotsSize;

                                // Legend is truncated
                                _legendItemsTruncated = true;
                            }
                        }
                        else
                        {
                            autoFitDone = true;
                        }

                        // Check if legend items fit into the legend area
                        if (!autoFitDone)
                        {
                            autoFitDone = this.CheckLegendItemsFit(
                                chartGraph,
                                _legendItemsAreaPosition.Size,
                                _numberOfLegendItemsToProcess,
                                _autoFitFontSizeAdjustment,
                                _itemColumns,
                                _numberOfRowsPerColumn,
                                out _subColumnSizes,
                                out _cellHeights,
                                out _horizontalSpaceLeft,
                                out _verticalSpaceLeft);
                        }
                    }
                } while (!autoFitDone);
            }

            //***********************************************************
            //** Calculate position of all cells
            //***********************************************************

            // Calculate item vertical spacing in relative coordinates but rounded on pixel boundary
            SKSize itemHalfSpacing = SKSize.Empty;
            if (_verticalSpaceLeft > 0)
            {
                itemHalfSpacing.Height = _verticalSpaceLeft / GetMaximumNumberOfRows() / 2.0f;
            }
            if (_horizontalSpaceLeft > 0)
            {
                itemHalfSpacing.Width = _horizontalSpaceLeft / 2.0f;
            }

            // Iterate through all legend items
            int currentColumn = 0;
            int currentRow = 0;
            if (_numberOfLegendItemsToProcess < 0)
            {
                _numberOfLegendItemsToProcess = legendItems.Count;
            }
            for (int legendItemIndex = 0; legendItemIndex < _numberOfLegendItemsToProcess; legendItemIndex++)
            {
                LegendItem legendItem = legendItems[legendItemIndex];

                // Iterate through legend item cells
                for (int cellIndex = 0; cellIndex < legendItem.Cells.Count; cellIndex++)
                {
                    // Get legend cell
                    LegendCell legendCell = legendItem.Cells[cellIndex];

                    // Calculate cell position
                    SKRect cellPosition = GetCellPosition(currentColumn, currentRow, cellIndex, itemHalfSpacing);

                    // Check if current cell spans through more than 1 cell
                    int overlappedCellsNumber = 0;
                    if (legendCell.CellSpan > 1)
                    {
                        for (int spanIndex = 1; spanIndex < legendCell.CellSpan && (cellIndex + spanIndex) < legendItem.Cells.Count; spanIndex++)
                        {
                            // Calculate overlapped cell position
                            SKRect overlappedCellPosition = GetCellPosition(currentColumn, currentRow, cellIndex + spanIndex, itemHalfSpacing);

                            // Adjust current cell right position
                            if (cellPosition.Right < overlappedCellPosition.Right)
                            {
                                cellPosition.Right += overlappedCellPosition.Right - cellPosition.Right;
                            }

                            // Increase number of overlapped cells
                            ++overlappedCellsNumber;

                            // Set empty size for the overlapped cells
                            LegendCell overlappedLegendCell = legendItem.Cells[cellIndex + spanIndex];
                            overlappedLegendCell.SetCellPosition(
                                currentRow,
                                SKRect.Empty,
                                singleWCharacterSize);
                        }
                    }

                    // Make sure cell is drawn inside the legend
                    cellPosition.Intersect(_legendItemsAreaPosition);

                    // Set cell object position
                    legendCell.SetCellPosition(
                        currentRow,
                        cellPosition,
                        singleWCharacterSize);

                    // Skip overlapped cells
                    cellIndex += overlappedCellsNumber;
                }

                // Advance to the next row/column. Break if number of legend items exceed
                // number of availabale rows/columns.
                ++currentRow;
                if (currentRow >= _numberOfRowsPerColumn[currentColumn])
                {
                    ++currentColumn;
                    currentRow = 0;
                    if (currentColumn >= _itemColumns)
                    {
                        break;
                    }
                }
            }
        }
        #endregion Legend position & size methods

        #region Legend Items Fitting Methods
        #endregion Legend Items Fitting Methods

        #region Legend items collection filling methods
        #endregion Legend items collection filling methods

        #region Legend painting methods
        #endregion Legend painting methods

        #region	Legend properties
        #endregion

        #region Legend Title Properties
        #endregion // Legend Title Properties

        #region Legent Title and Header Helper methods
        #endregion // Legent Title Helper methods

        #region Helper methods
        #endregion

        #region IDisposable Members
        #endregion
    }

    /// <summary>
    /// The LegendCollection class is a strongly typed collection of legends.
    /// </summary>
    [
        SRDescription("DescriptionAttributeLegendCollection_LegendCollection"),
    ]
    public class LegendCollection : ChartNamedElementCollection<Legend>
    {
        #region Constructors

        /// <summary>
        /// LegendCollection constructor.
        /// </summary>
        /// <param name="chartPicture">Chart picture object.</param>
        internal LegendCollection(ChartPicture chartPicture)
            : base(chartPicture)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default legend name.
        /// </summary>
        internal string DefaultNameReference
        {
            get { return Count > 0 ? this[0].Name : String.Empty; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new Legend with the specified name and adds it to the collection.
        /// </summary>
        /// <param name="name">The new chart area name.</param>
        /// <returns>New legend</returns>
        public Legend Add(string name)
        {
            Legend legend = new(name);
            Add(legend);
            return legend;
        }

        /// <summary>
        /// Recalculates legend position inside chart area in the collection.
        /// </summary>
        /// <param name="chartGraph">Chart graphics used.</param>
        /// <param name="elementSpacing">Spacing size as a percentage of the area.</param>
        internal void CalcInsideLegendPosition(
            ChartGraphics chartGraph,
            float elementSpacing)
        {
            if (Common != null && Common.ChartPicture != null)
            {
                // Check if all chart area names are valid
                foreach (Legend legend in this)
                {
                    if (legend.DockedToChartArea != Constants.NotSetValue)
                    {
                        try
                        {
                            ChartArea area = Common.ChartPicture.ChartAreas[legend.DockedToChartArea];
                        }
                        catch
                        {
                            throw (new ArgumentException(SR.ExceptionLegendDockedChartAreaIsMissing((string)legend.DockedToChartArea)));
                        }
                    }
                }

                // Loop through all chart areas
                foreach (ChartArea area in Common.ChartPicture.ChartAreas)
                {
                    // Check if chart area is visible
                    if (area.Visible)

                    {
                        // Get area position
                        SKRect legendPlottingRectangle = area.PlotAreaPosition.ToSKRect();

                        // Get elemets spacing
                        float areaSpacing = Math.Min((legendPlottingRectangle.Height / 100F) * elementSpacing, (legendPlottingRectangle.Width / 100F) * elementSpacing);

                        // Loop through all legends
                        foreach (Legend legend in this)
                        {
                            if (legend.IsEnabled() &&
                                legend.IsDockedInsideChartArea == true &&
                                legend.DockedToChartArea == area.Name &&
                                legend.Position.Auto)
                            {
                                // Calculate legend position
                                legend.CalcLegendPosition(chartGraph,
                                    ref legendPlottingRectangle,
                                    areaSpacing);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
		/// Recalculates legend position in the collection.
		/// </summary>
		/// <param name="chartGraph">Chart graphics used.</param>
		/// <param name="chartAreasRectangle">Area where the legend should be positioned.</param>
		/// <param name="elementSpacing">Spacing size as a percentage of the area.</param>
		internal void CalcLegendPosition(
            ChartGraphics chartGraph,
            ref SKRect chartAreasRectangle,
            float elementSpacing)
        {
            // Loop through all legends
            foreach (Legend legend in this)
            {
                // Calculate position of the legends docked to the chart picture
                if (legend.IsEnabled() &&
                    legend.DockedToChartArea == Constants.NotSetValue &&
                    legend.Position.Auto)
                {
                    legend.CalcLegendPosition(chartGraph, ref chartAreasRectangle, elementSpacing);
                }
            }
        }

        /// <summary>
        /// Recalculates legend position in the collection for legends docked outside of chart area.
        /// </summary>
        /// <param name="chartGraph">Chart graphics used.</param>
        /// <param name="area">Area the legend is docked to.</param>
        /// <param name="chartAreasRectangle">Area where the legend should be positioned.</param>
        /// <param name="elementSpacing">Spacing size as a percentage of the area.</param>
        internal void CalcOutsideLegendPosition(
            ChartGraphics chartGraph,
            ChartArea area,
            ref SKRect chartAreasRectangle,
            float elementSpacing)
        {
            if (Common != null && Common.ChartPicture != null)
            {
                // Get elemets spacing
                float areaSpacing = Math.Min((chartAreasRectangle.Height / 100F) * elementSpacing, (chartAreasRectangle.Width / 100F) * elementSpacing);

                // Loop through all legends
                foreach (Legend legend in this)
                {
                    // Check if all chart area names are valid
                    if (legend.DockedToChartArea != Constants.NotSetValue && Chart.ChartAreas.IndexOf(legend.DockedToChartArea) < 0)
                    {
                        throw (new ArgumentException(SR.ExceptionLegendDockedChartAreaIsMissing((string)legend.DockedToChartArea)));
                    }

                    // Process only legends docked to specified area
                    if (legend.IsEnabled() &&
                        legend.IsDockedInsideChartArea == false &&
                        legend.DockedToChartArea == area.Name &&
                        legend.Position.Auto)
                    {
                        // Calculate legend position
                        legend.CalcLegendPosition(chartGraph,
                            ref chartAreasRectangle,
                            areaSpacing);

                        // Adjust legend position
                        SKRect legendPosition = legend.Position.ToSKRect();
                        if (legend.Docking == Docking.Top)
                        {
                            legendPosition.Top -= areaSpacing;
                            if (!area.Position.Auto)
                            {
                                legendPosition.Top -= legendPosition.Height;
                            }
                        }
                        else if (legend.Docking == Docking.Bottom)
                        {
                            legendPosition.Top += areaSpacing;
                            if (!area.Position.Auto)
                            {
                                legendPosition.Top = area.Position.Bottom + areaSpacing;
                            }
                        }
                        if (legend.Docking == Docking.Left)
                        {
                            legendPosition.Left -= areaSpacing;
                            if (!area.Position.Auto)
                            {
                                legendPosition.Left -= legendPosition.Width;
                            }
                        }
                        if (legend.Docking == Docking.Right)
                        {
                            legendPosition.Left += areaSpacing;
                            if (!area.Position.Auto)
                            {
                                legendPosition.Left = area.Position.Right + areaSpacing;
                            }
                        }

                        legend.Position.SetPositionNoAuto(legendPosition.Left, legendPosition.Left, legendPosition.Width, legendPosition.Height);
                    }
                }
            }
        }
        #endregion

        #region Event handlers

        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            //If all the chart areas are removed and then the first one is added we don't want to dock the legends
            if (e.OldElement == null)
                return;

            foreach (Legend legend in this)
                if (legend.DockedToChartArea == e.OldName)
                    legend.DockedToChartArea = e.NewName;
        }

        #endregion
    }

    /// <summary>
    /// The LegendItem class represents a single item (row) in the legend.
    /// It contains properties which describe visual appearance and
    /// content of the legend item.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLegendItem_LegendItem"),
    ]
    public class LegendItem : ChartNamedElement
    {
        #region Fields

        internal GradientStyle backGradientStyle = GradientStyle.None;

        internal ChartHatchStyle backHatchStyle = ChartHatchStyle.None;

        internal ChartImageAlignmentStyle backImageAlign = ChartImageAlignmentStyle.TopLeft;

        internal SKColor backImageTransparentColor = SKColor.Empty;

        internal ChartImageWrapMode backImageWrapMode = ChartImageWrapMode.Tile;

        internal SKColor backSecondaryColor = SKColor.Empty;

        internal SKColor borderColor = SKColors.Black;

        internal ChartDashStyle borderDashStyle = ChartDashStyle.Solid;

        internal int borderWidth = 1;

        // Indicates that temporary cells where added and thet have to be removed
        internal bool clearTempCells = false;

        internal SKColor markerBorderColor = SKColor.Empty;

        internal SKColor markerColor = SKColor.Empty;

        internal string markerImage = "";

        internal SKColor markerImageTransparentColor = SKColor.Empty;

        internal int markerSize = 5;

        // Marker properties
        internal MarkerStyle markerStyle = MarkerStyle.None;

        internal SKColor shadowColor = Services.Color.FromArgb(128, 0, 0, 0);

        internal int shadowOffset = 0;

        // Additional appearance properties
        internal LegendImageStyle style = LegendImageStyle.Rectangle;

        // Collection of legend item cells
        private LegendCellCollection _cells = null;

        // Private data members, which store properties values
        private SKColor _color = SKColor.Empty;

        // True if legend item is enabled.
        private bool _enabled = true;

        private string _image = "";

        // Series marker border width
        private int _markerBorderWidth = 1;

        // Legend item visual separator color
        private SKColor _separatorColor = SKColors.Black;

        // Legend item visual separator
        private LegendSeparatorStyle _separatorType = LegendSeparatorStyle.None;

        /// <summary>
        /// LegendItem constructor
        /// </summary>
        public LegendItem()
        {
            // Create collection of legend item cells
            _cells = new LegendCellCollection(this);
        }

        /// <summary>
        /// LegendItem constructor
        /// </summary>
        /// <param name="name">Item name.</param>
        /// <param name="color">Item color.</param>
        /// <param name="image">Item image.</param>
        public LegendItem(string name, SKColor color, string image) : base(name)
        {
            _color = color;
            _image = image;

            // Create collection of legend item cells
            _cells = new LegendCellCollection(this);
        }

        /// <summary>
        /// Gets or sets background gradient style of the legend item.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle"),
        ]
        public GradientStyle BackGradientStyle
        {
            get
            {
                return backGradientStyle;
            }
            set
            {
                backGradientStyle = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the background hatch style of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackHatchStyle"),
        ]
        public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return backHatchStyle;
            }
            set
            {
                backHatchStyle = value;
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
                return backImageTransparentColor;
            }
            set
            {
                backImageTransparentColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the secondary background color.
        /// <seealso cref="Color"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of background with
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="Color"/> when <see cref="BackHatchStyle"/> or
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
                return backSecondaryColor;
            }
            set
            {
                if (value != SKColor.Empty && (value.Alpha != 255 || value == SKColors.Transparent))
                {
                    throw (new ArgumentException(SR.ExceptionBackSecondaryColorIsTransparent));
                }

                backSecondaryColor = value;

                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the border color of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderColor"),
        ]
        public SKColor BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the legend item border style.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderDashStyle"),
                ]
        public ChartDashStyle BorderDashStyle
        {
            get
            {
                return borderDashStyle;
            }
            set
            {
                borderDashStyle = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the border width of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderWidth"),
                ]
        public int BorderWidth
        {
            get
            {
                return borderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionBorderWidthIsZero));
                }
                borderWidth = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// The LegendCellCollection class is a collection of legend item cells.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Cells"),
        ]
        public LegendCellCollection Cells
        {
            get
            {
                return _cells;
            }
        }

        /// <summary>
        /// Gets or sets the color of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Color"),
        ]
        public SKColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the Legend item is enabled.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Enabled"),
        ]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets a string value that represents a URL to an image file, which will be used for the legend item's symbol.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Image"),
        ]
        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the picture style of the legend item image.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Style"),
        ]
        public LegendImageStyle ImageStyle
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets the Legend object which the item belongs to.
        /// </summary>
        public Legend Legend
        {
            get
            {
                if (Parent != null)
                    return Parent.Parent as Legend;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets the marker border color of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeMarkerBorderColor"),
        ]
        public SKColor MarkerBorderColor
        {
            get
            {
                return markerBorderColor;
            }
            set
            {
                markerBorderColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the marker border width of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeMarkerBorderWidth"),
                ]
        public int MarkerBorderWidth
        {
            get
            {
                return _markerBorderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionLegendMarkerBorderWidthIsNegative);
                }
                _markerBorderWidth = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the marker color of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeLegendItem_MarkerColor"),
        ]
        public SKColor MarkerColor
        {
            get
            {
                return markerColor;
            }
            set
            {
                markerColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the marker image of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeMarkerImage"),
        ]
        public string MarkerImage
        {
            get
            {
                return markerImage;
            }
            set
            {
                markerImage = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the marker image.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        ]
        public SKColor MarkerImageTransparentColor
        {
            get
            {
                return markerImageTransparentColor;
            }
            set
            {
                markerImageTransparentColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the marker size of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeLegendItem_MarkerSize"),
        ]
        public int MarkerSize
        {
            get
            {
                return markerSize;
            }
            set
            {
                markerSize = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the marker style of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeLegendItem_MarkerStyle"),
        ]
        public MarkerStyle MarkerStyle
        {
            get
            {
                return markerStyle;
            }
            set
            {
                markerStyle = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the name of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Name"),
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
        /// Gets or sets the separator color of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_SeparatorColor"),
        ]
        public SKColor SeparatorColor
        {
            get
            {
                return _separatorColor;
            }
            set
            {
                if (value != _separatorColor)
                {
                    _separatorColor = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the separator style of the legend item.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendItem_Separator"),
        ]
        public LegendSeparatorStyle SeparatorType
        {
            get
            {
                return _separatorType;
            }
            set
            {
                if (value != _separatorType)
                {
                    _separatorType = value;
                    Invalidate(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets the series name of the legend item..
        /// </summary>
        [
        SRDescription("DescriptionAttributeLegendItem_SeriesName"),
        ]
        public string SeriesName { get; set; } = "";

        /// <summary>
        /// Gets or sets the index of the legend item's associated DataPoint object.
        /// </summary>
        [
        SRDescription("DescriptionAttributeLegendItem_SeriesPointIndex"),
        ]
        public int SeriesPointIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the color of a legend item's shadow.
        /// <seealso cref="ShadowOffset"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used to draw a legend item's shadow.
        /// </value>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeShadowColor"),
        ]
        public SKColor ShadowColor
        {
            get
            {
                return shadowColor;
            }
            set
            {
                shadowColor = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the offset between the legend item and its shadow.
        /// <seealso cref="ShadowColor"/>
        /// </summary>
        /// <value>
        /// An integer value that represents the offset between the legend item and its shadow.
        /// </value>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeShadowOffset"),
        ]
        public int ShadowOffset
        {
            get
            {
                return shadowOffset;
            }
            set
            {
                shadowOffset = value;
                Invalidate(false);
            }
        }

        // Chart image map properties
        internal string ToolTip { get; set; } = "";
        #endregion

        #region Constructors
        #endregion

        #region	Legend item properties
        #endregion

        #region Helper methods

        /// <summary>
		/// Helper method adds default legend item cells based on the columns
		/// specified. If columns collection is empty we assume the presence of
		/// two columns: series marker and legend item text.
		/// </summary>
		/// <param name="legend">Legend this item belongs to.</param>
		internal void AddAutomaticCells(Legend legend)
        {
            // Check if cells defined
            if (Cells.Count == 0)
            {
                // Check if legend item was generated for the series
                if (SeriesName.Length > 0)
                {
                    // If legend do not have any columns set add a series marker
                    // and legend text cells
                    if (legend.CellColumns.Count == 0)
                    {
                        Cells.Add(LegendCellType.SeriesSymbol, string.Empty, ContentAlignment.MiddleCenter);
                        Cells.Add(LegendCellType.Text, KeywordName.LegendText, ContentAlignment.MiddleLeft);
                    }
                    else
                    {
                        // Add cell for each of the columns
                        foreach (LegendCellColumn legendColumn in legend.CellColumns)
                        {
                            Cells.Add(legendColumn.CreateNewCell());
                        }
                    }
                }
                else
                {
                    // Add Marker plus text for everything else
                    clearTempCells = true;
                    Cells.Add(LegendCellType.SeriesSymbol, string.Empty, ContentAlignment.MiddleCenter);
                    Cells.Add(LegendCellType.Text, KeywordName.LegendText, ContentAlignment.MiddleLeft);
                }
            }
        }

        /// <summary>
        /// Sets legend item properties from the series
        /// </summary>
        /// <param name="series">Series object.</param>
        /// <param name="common">Common elements object.</param>
        internal void SetAttributes(CommonElements common, Series series)
        {
            // Get legend item picture style
            IChartType chartType = common.ChartTypeRegistry.GetChartType(series.ChartTypeName);
            style = chartType.GetLegendImageStyle(series);

            // Set series name
            SeriesName = series.Name;

            // Get shadow properties
            shadowOffset = series.ShadowOffset;
            shadowColor = series.ShadowColor;

            // Check if series is drawn in 3D chart area
            bool area3D = common.Chart.ChartAreas[series.ChartArea].Area3DStyle.Enable3D;

            // Get other properties
            SetAttributes((DataPointCustomProperties)series, area3D);
        }

        /// <summary>
        /// Sets legend item properties from the DataPointCustomProperties object.
        /// </summary>
        /// <param name="properties">DataPointCustomProperties object.</param>
        /// <param name="area3D">Element belongs to the 3D area.</param>
        internal void SetAttributes(DataPointCustomProperties properties, bool area3D)
        {
            borderColor = properties.BorderColor;
            borderWidth = properties.BorderWidth;
            borderDashStyle = properties.BorderDashStyle;
            markerStyle = properties.MarkerStyle;
            markerSize = properties.MarkerSize;
            markerImage = properties.MarkerImage;
            markerImageTransparentColor = properties.MarkerImageTransparentColor;
            markerColor = properties.MarkerColor;
            markerBorderColor = properties.MarkerBorderColor;

            _markerBorderWidth = properties.MarkerBorderWidth;

            float dpi = 96;

            if (Common != null)
                dpi = 96;

            int maxBorderWidth = (int)Math.Round((2 * dpi) / 96);

            if (_markerBorderWidth > maxBorderWidth)
            {
                _markerBorderWidth = maxBorderWidth;
            }

            if (properties.MarkerBorderWidth <= 0)
            {
                markerBorderColor = SKColors.Transparent;
            }

            // Improve readability of the line series marker by using at least 2 pixel wide lines
            if (style == LegendImageStyle.Line &&
                borderWidth <= (int)Math.Round(dpi / 96))
            {
                borderWidth = maxBorderWidth;
            }

            if (!area3D)
            {
                backGradientStyle = properties.BackGradientStyle;
                backSecondaryColor = properties.BackSecondaryColor;
                backImageTransparentColor = properties.BackImageTransparentColor;
                backImageWrapMode = properties.BackImageWrapMode;
                backImageAlign = properties.BackImageAlignment;
                backHatchStyle = properties.BackHatchStyle;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _cells != null)
            {
                _cells.Dispose();
                _cells = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Invalidate chart (or just legend )when collection is changed
        /// </summary>
        private void Invalidate(bool invalidateLegendOnly)
        {
            if (Legend != null)
            {
                // Invalidate control
                Legend.Invalidate(invalidateLegendOnly);
            }
        }

        #endregion

        #region IDisposable Members
        #endregion
    }

    /// <summary>
    /// The LegendItemsCollection class is a strongly typed collection of legend items.
    /// </summary>
    [
        SRDescription("DescriptionAttributeCustomLabelsCollection_CustomLabelsCollection"),
    ]
    public class LegendItemsCollection : ChartElementCollection<LegendItem>
    {
        #region Constructors

        /// <summary>
        /// LegendItemsCollection constructor
		/// </summary>
        internal LegendItemsCollection(Legend legend)
            : base(legend)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a legend item into the collection.
        /// </summary>
        /// <param name="color">Legend item color.</param>
        /// <param name="text">Legend item text.</param>
        /// <returns>Index of newly added item.</returns>
        public int Add(SKColor color, string text)
        {
            LegendItem item = new(text, color, "");
            Add(item);
            return Count - 1;
        }

        /// <summary>
        /// Adds a legend item into the collection.
        /// </summary>
        /// <param name="image">Legend item image.</param>
        /// <param name="text">Legend item text.</param>
        /// <returns>Index of newly added item.</returns>
        public int Add(string image, string text)
        {
            LegendItem item = new(text, SKColor.Empty, image);
            Add(item);
            return Count - 1;
        }

        /// <summary>
        /// Insert a legend item into the collection.
        /// </summary>
        /// <param name="index">Index to insert at.</param>
        /// <param name="color">Legend item color.</param>
        /// <param name="text">Legend item text.</param>
        /// <returns>Index of newly added item.</returns>
        public void Insert(int index, SKColor color, string text)
        {
            LegendItem item = new(text, color, "");
            Insert(index, item);
        }
        /// <summary>
        /// Insert one legend item into the collection.
        /// </summary>
        /// <param name="index">Index to insert at.</param>
        /// <param name="image">Legend item image.</param>
        /// <param name="text">Legend item text.</param>
        /// <returns>Index of newly added item.</returns>
        public void Insert(int index, string image, string text)
        {
            LegendItem item = new(text, SKColor.Empty, image);
            Insert(index, item);
        }

        /// <summary>
        /// Reverses the order of items in the collection.
        /// </summary>
        public void Reverse()
        {
            List<LegendItem> list = Items as List<LegendItem>;
            list.Reverse();
            Invalidate();
        }

        #endregion
    }
}