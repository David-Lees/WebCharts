// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	LegendCell and LegendCellColumn classes allow to
//              create highly customize legends. Please refer to
//              Chart documentation which contains images and
//              samples describing this functionality.
//

using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace WebCharts.Services
{
    #region Enumerations

    /// <summary>
    /// An enumeration of legend cell types.
    /// </summary>
    public enum LegendCellType
    {
        /// <summary>
        /// Legend cell contains text.
        /// </summary>
        Text,

        /// <summary>
        /// Legend cell contains series symbol.
        /// </summary>
        SeriesSymbol,

        /// <summary>
        /// Legend cell contains image.
        /// </summary>
        Image
    }

    /// <summary>
    /// An enumeration of legend cell column types.
    /// </summary>
    public enum LegendCellColumnType
    {
        /// <summary>
        /// Legend column contains text.
        /// </summary>
        Text,

        /// <summary>
        /// Legend column contains series symbol.
        /// </summary>
        SeriesSymbol
    }

    #endregion Enumerations

    /// <summary>
    /// The LegendCellColumn class represents a cell column in a legend,
    /// used to extend the functionality of the default legend. It contains
    /// visual appearance properties, legend header settings and also determine
    /// how and in which order cells are formed for each of the legend items.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLegendCellColumn_LegendCellColumn"),
    ]
    public class LegendCellColumn : ChartNamedElement
    {
        #region Fields

        // Legend column type
        private LegendCellColumnType _columnType;

        // Legend column text
        private string _text;

        // Legend column text color
        private SKColor _foreColor = SKColor.Empty;

        // Legend column back color
        private SKColor _backColor = SKColor.Empty;

        // Font cache
        private FontCache _fontCache = new();

        // Legend column text font
        private SKFont _font = null;

        // Legend column series symbol size
        private SKSize _seriesSymbolSize = new(200, 70);

        // Legend column content allignment
        private ContentAlignment _alignment;

        // Legend column margins
        private Margins _margins = new(0, 0, 15, 15);

        // Legend column header text
        private string _headerText;

        // Legend column/cell content allignment
        private StringAlignment _headerAlignment = StringAlignment.Center;

        // Legend column header text color
        private SKColor _headerForeColor = SKColors.Black;

        // Legend column header text back color
        private SKColor _headerBackColor = SKColor.Empty;

        // Legend column header text font
        private SKFont _headerFont = null;

        // Minimum column width
        private int _minimumCellWidth = -1;

        // Maximum column width
        private int _maximumCellWidth = -1;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// LegendCellColumn constructor.
        /// </summary>
        public LegendCellColumn()
            : this(string.Empty, LegendCellColumnType.Text, KeywordName.LegendText, ContentAlignment.MiddleCenter)
        {
            _headerFont = _fontCache.DefaultBoldFont;
        }

        /// <summary>
        /// LegendCellColumn constructor.
        /// </summary>
        /// <param name="headerText">Column header text.</param>
        /// <param name="columnType">Column type.</param>
        /// <param name="text">Column cell text.</param>
        public LegendCellColumn(string headerText, LegendCellColumnType columnType, string text) : this(headerText, columnType, text, ContentAlignment.MiddleCenter)
        {
        }

        /// <summary>
        /// Legend column object constructor.
        /// </summary>
        /// <param name="headerText">Column header text.</param>
        /// <param name="columnType">Column type.</param>
        /// <param name="text">Column cell text .</param>
        /// <param name="alignment">Column cell content alignment.</param>
        public LegendCellColumn(string headerText, LegendCellColumnType columnType, string text, ContentAlignment alignment)
        {
            _headerText = headerText;
            _columnType = columnType;
            _text = text;
            _alignment = alignment;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets name of legend column.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeLegendCellColumn_Name"),
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
        /// Gets legend this column belongs too.
        /// </summary>
        public virtual Legend Legend
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
        /// Gets or sets legend column type.  This is only applicable to items that are automatically generated for the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeLegendCellColumn_ColumnType"),
        ]
        public virtual LegendCellColumnType ColumnType
        {
            get
            {
                return _columnType;
            }
            set
            {
                _columnType = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets legend column text.  This is only applicable to items that are automatically generated for the series.
        /// Set the ColumnType property to text to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeLegendCellColumn_Text"),
        ]
        public virtual string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text color of the legend column.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeForeColor"),
        ]
        public virtual SKColor ForeColor
        {
            get
            {
                return _foreColor;
            }
            set
            {
                _foreColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of the legend column.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        public virtual SKColor BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the font of the legend column text.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeLegendCellColumn_Font"),
        ]
        public virtual SKFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the series symbol size of the legend column
        /// for the items automatically generated for the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeLegendCellColumn_SeriesSymbolSize"),
        ]
        public virtual SKSize SeriesSymbolSize
        {
            get
            {
                return _seriesSymbolSize;
            }
            set
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw (new ArgumentException(SR.ExceptionSeriesSymbolSizeIsNegative, nameof(value)));
                }
                _seriesSymbolSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the content alignment of the legend column.
        /// This is only applicable to items that are automatically generated for the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeLegendCellColumn_Alignment"),
        ]
        public virtual ContentAlignment Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the margins of the legend column (as a percentage of legend font size).
        /// This is only applicable to items that are automatically generated for the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeSeriesItems"),
        SRDescription("DescriptionAttributeLegendCellColumn_Margins"),
        ]
        public virtual Margins Margins
        {
            get
            {
                return _margins;
            }
            set
            {
                _margins = value;
                Invalidate();

                // Set common elements of the new margins class
                if (Legend != null)
                {
                    _margins.Common = Legend.Common;
                }
            }
        }

        /// <summary>
        /// Returns true if property should be serialized.  This is for internal use only.
        /// </summary>
        public bool ShouldSerializeMargins()
        {
            if (_margins.Top == 0 &&
                _margins.Bottom == 0 &&
                _margins.Left == 15 &&
                _margins.Right == 15)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets or sets the legend column header text.
        /// </summary>
        [
        SRCategory("CategoryAttributeHeader"),
        SRDescription("DescriptionAttributeLegendCellColumn_HeaderText"),
        ]
        public virtual string HeaderText
        {
            get
            {
                return _headerText;
            }
            set
            {
                _headerText = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the legend column header text.
        /// </summary>
        [
        SRCategory("CategoryAttributeHeader"),
        SRDescription("DescriptionAttributeLegendCellColumn_HeaderColor"),
        ]
        public virtual SKColor HeaderForeColor
        {
            get
            {
                return _headerForeColor;
            }
            set
            {
                _headerForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of the legend column header.
        /// </summary>
        [
        SRCategory("CategoryAttributeHeader"),
        SRDescription("DescriptionAttributeHeaderBackColor"),
        ]
        public virtual SKColor HeaderBackColor
        {
            get
            {
                return _headerBackColor;
            }
            set
            {
                _headerBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the font of the legend column header.
        /// </summary>
        [
        SRCategory("CategoryAttributeHeader"),
        SRDescription("DescriptionAttributeLegendCellColumn_HeaderFont"),
        ]
        public virtual SKFont HeaderFont
        {
            get
            {
                return _headerFont;
            }
            set
            {
                _headerFont = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the horizontal text alignment of the legend column header.
        /// </summary>
        [
        SRCategory("CategoryAttributeHeader"),
        SRDescription("DescriptionAttributeLegendCellColumn_HeaderTextAlignment"),
        ]
        public StringAlignment HeaderAlignment
        {
            get
            {
                return _headerAlignment;
            }
            set
            {
                if (value != _headerAlignment)
                {
                    _headerAlignment = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum width (as a percentage of legend font size) of legend column. Set this property to -1 for automatic calculation.
        /// </summary>
        [
        SRCategory("CategoryAttributeSize"),
        SRDescription("DescriptionAttributeLegendCellColumn_MinimumWidth"),
        ]
        public virtual int MinimumWidth
        {
            get
            {
                return _minimumCellWidth;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentException(SR.ExceptionMinimumCellWidthIsWrong, nameof(value));
                }

                _minimumCellWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum width (as a percentage of legend font size) of legend column.  Set this property to -1 for automatic calculation.
        /// </summary>
        [
        SRCategory("CategoryAttributeSize"),
        SRDescription("DescriptionAttributeLegendCellColumn_MaximumWidth"),
        ]
        public virtual int MaximumWidth
        {
            get
            {
                return _maximumCellWidth;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentException(SR.ExceptionMaximumCellWidthIsWrong, nameof(value));
                }
                _maximumCellWidth = value;
                Invalidate();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a new LegendCell object and copies all properties from the
        /// current column into the newly created one.
        /// </summary>
        /// <returns>A new copy of the LegendCell</returns>
        internal LegendCell CreateNewCell()
        {
            LegendCell newCell = new();
            newCell.CellType = (ColumnType == LegendCellColumnType.SeriesSymbol) ? LegendCellType.SeriesSymbol : LegendCellType.Text;
            newCell.Text = Text;
            newCell.SeriesSymbolSize = SeriesSymbolSize;
            newCell.Alignment = Alignment;
            newCell.Margins = new Margins(Margins.Top, Margins.Bottom, Margins.Left, Margins.Right);
            return newCell;
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _fontCache != null)
            {
                _fontCache.Dispose();
                _fontCache = null;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// The LegendCell class represents a single cell in the chart legend.
    /// Legend contains several legend items.  Each item contains several
    /// cells which form the vertical columns. This class provides properties
    /// which determine content of the cell and its visual appearance. It
    /// also contains method which determine the size of the cell and draw
    /// cell in the chart.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLegendCell_LegendCell"),
    ]
    public class LegendCell : ChartNamedElement
    {
        #region Fields

        // Legend cell type
        private LegendCellType _cellType = LegendCellType.Text;

        // Legend cell text
        private string _text = string.Empty;

        // Legend cell text color
        private SKColor _foreColor = SKColor.Empty;

        // Legend cell back color
        private SKColor _backColor = SKColor.Empty;

        // Font cache
        private FontCache _fontCache = new();

        // Legend cell text font
        private SKFont _font = null;

        // Legend cell image name
        private string _image = string.Empty;

        // Legend cell image transparent color
        private SKColor _imageTransparentColor = SKColor.Empty;

        // Legend cell image size
        private SKSize _imageSize = SKSize.Empty;

        // Legend cell series symbol size
        private SKSize _seriesSymbolSize = new(200, 70);

        // Legend cell content allignment
        private ContentAlignment _alignment = ContentAlignment.MiddleCenter;

        // Numer of cells this cell uses to show it's content
        private int _cellSpan = 1;

        // Legend cell margins
        private Margins _margins = new(0, 0, 15, 15);

        // Cell row index
        private int _rowIndex = -1;

        // Position where cell is drawn in pixel coordinates.
        // Exncludes margins and space required for separators
        internal SKRect cellPosition = SKRect.Empty;

        // Position where cell is drawn in pixel coordinates.
        // Includes margins and space required for separators
        internal SKRect cellPositionWithMargins = SKRect.Empty;

        // Last cached cell size.
        private SKSize _cachedCellSize = SKSize.Empty;

        // Font reduced value used to calculate last cached cell size
        private int _cachedCellSKSizeontReducedBy = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// LegendCell constructor.
        /// </summary>
        public LegendCell()
        {
            Intitialize(LegendCellType.Text, string.Empty, ContentAlignment.MiddleCenter);
        }

        /// <summary>
        /// LegendCell constructor.
        /// </summary>
        /// <param name="text">Cell text or image name, depending on the type.</param>
        public LegendCell(string text)
        {
            Intitialize(LegendCellType.Text, text, ContentAlignment.MiddleCenter);
        }

        /// <summary>
        /// LegendCell constructor.
        /// </summary>
        /// <param name="cellType">Cell type.</param>
        /// <param name="text">Cell text or image name, depending on the type.</param>
        public LegendCell(LegendCellType cellType, string text)
        {
            Intitialize(cellType, text, ContentAlignment.MiddleCenter);
        }

        /// <summary>
        /// LegendCell constructor.
        /// </summary>
        /// <param name="cellType">Cell type.</param>
        /// <param name="text">Cell text or image name, depending on the type.</param>
        /// <param name="alignment">Cell content alignment.</param>
        public LegendCell(LegendCellType cellType, string text, ContentAlignment alignment)
        {
            Intitialize(cellType, text, alignment);
        }

        /// <summary>
        /// Initializes newly created object.
        /// </summary>
        /// <param name="cellType">Cell type.</param>
        /// <param name="text">Cell text or image name depending on the type.</param>
        /// <param name="alignment">Cell content alignment.</param>
        private void Intitialize(LegendCellType cellType, string text, ContentAlignment alignment)
        {
            _cellType = cellType;
            if (_cellType == LegendCellType.Image)
            {
                _image = text;
            }
            else
            {
                _text = text;
            }
            _alignment = alignment;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the legend cell.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeLegendCell_Name"),
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
        /// Gets or sets the type of the legend cell.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendCell_CellType"),
        ]
        public virtual LegendCellType CellType
        {
            get
            {
                return _cellType;
            }
            set
            {
                _cellType = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets legend this column/cell belongs too.
        /// </summary>
        public virtual Legend Legend
        {
            get
            {
                LegendItem item = LegendItem;
                if (item != null)
                    return item.Legend;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets legend item this cell belongs too.
        /// </summary>
        public virtual LegendItem LegendItem
        {
            get
            {
                if (Parent != null)
                    return Parent.Parent as LegendItem;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets the text of the legend cell. Set CellType to text to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendCell_Text"),
        ]
        public virtual string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text color of the legend cell. Set CellType to text to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeForeColor"),
        ]
        public virtual SKColor ForeColor
        {
            get
            {
                return _foreColor;
            }
            set
            {
                _foreColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of the legend cell.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        public virtual SKColor BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the font of the legend cell. Set CellType to text to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendCell_Font"),
        ]
        public virtual SKFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the URL of the image of the legend cell. Set CellType to image to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLegendCell_Image"),
        ]
        public virtual string Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the image.  Set CellType to image to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        ]
        public virtual SKColor ImageTransparentColor
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
        /// Gets or sets the image size (as a percentage of legend font size) of the legend cell.
        /// Set CellType to Image to use this property.
        /// </summary>
        /// <remarks>
        /// If property is set to Size.IsEmpty, the original image pixels size is used.
        /// </remarks>
        [
        SRCategory("CategoryAttributeLayout"),
        SRDescription("DescriptionAttributeLegendCell_ImageSize"),
        ]
        public virtual SKSize ImageSize
        {
            get
            {
                return _imageSize;
            }
            set
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw (new ArgumentException(SR.ExceptionLegendCellImageSizeIsNegative, nameof(value)));
                }
                _imageSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the series symbol size (as a percentage of legend font size) of the legend cell.
        /// Set CellType to SeriesSymbol to use this property.
        /// </summary>
        [
        SRCategory("CategoryAttributeLayout"),
        SRDescription("DescriptionAttributeLegendCell_SeriesSymbolSize"),
        ]
        public virtual SKSize SeriesSymbolSize
        {
            get
            {
                return _seriesSymbolSize;
            }
            set
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw (new ArgumentException(SR.ExceptionLegendCellSeriesSymbolSizeIsNegative, nameof(value)));
                }
                _seriesSymbolSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the content alignment of the legend cell.
        /// </summary>
        [
        SRCategory("CategoryAttributeLayout"),
        SRDescription("DescriptionAttributeLegendCell_Alignment"),
        ]
        public virtual ContentAlignment Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the number of horizontal cells used to draw the content.
        /// </summary>
        [
        SRCategory("CategoryAttributeLayout"),
        SRDescription("DescriptionAttributeLegendCell_CellSpan"),
        ]
        public virtual int CellSpan
        {
            get
            {
                return _cellSpan;
            }
            set
            {
                if (value < 1)
                {
                    throw (new ArgumentException(SR.ExceptionLegendCellSpanIsLessThenOne, nameof(value)));
                }
                _cellSpan = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the legend cell margins (as a percentage of legend font size).
        /// </summary>
        [
        SRCategory("CategoryAttributeLayout"),
        SRDescription("DescriptionAttributeLegendCell_Margins"),
        ]
        public virtual Margins Margins
        {
            get
            {
                return _margins;
            }
            set
            {
                _margins = value;
                Invalidate();

                // Set common elements of the new margins class
                if (Legend != null)
                {
                    _margins.Common = Common;
                }
            }
        }

        /// <summary>
        /// Returns true if property should be serialized.  This method is for internal use only.
        /// </summary>
        internal bool ShouldSerializeMargins()
        {
            if (_margins.Top == 0 &&
                _margins.Bottom == 0 &&
                _margins.Left == 15 &&
                _margins.Right == 15)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets or sets the tooltip of the legend cell.
        /// </summary>
        [
        SRCategory("CategoryAttributeMapArea"),
        SRDescription("DescriptionAttributeToolTip"),
        ]
        public virtual string ToolTip { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Resets cached cell values.
        /// </summary>
        internal void ResetCache()
        {
            _cachedCellSize = SKSize.Empty;
            _cachedCellSKSizeontReducedBy = 0;
        }

        /// <summary>
        /// Sets cell position in relative coordinates.
        /// </summary>
        /// <param name="rowIndex">Cell row index.</param>
        /// <param name="position">Cell position.</param>
        /// <param name="singleWCharacterSize">Size of the 'W' character used to calculate elements.</param>
        internal void SetCellPosition(
            int rowIndex,
            SKRect position,
            SKSize singleWCharacterSize)
        {
            // Set cell position
            cellPosition = position;
            cellPositionWithMargins = position;
            _rowIndex = rowIndex;

            // Adjust cell position by specified margin
            cellPosition.Left += (int)(Margins.Left * singleWCharacterSize.Width / 100f);
            cellPosition.Top += (int)(Margins.Top * singleWCharacterSize.Height / 100f);
            cellPosition.Right -= (int)(Margins.Left * singleWCharacterSize.Width / 100f)
                + (int)(Margins.Right * singleWCharacterSize.Width / 100f);
            cellPosition.Bottom -= (int)(Margins.Top * singleWCharacterSize.Height / 100f)
                + (int)(Margins.Bottom * singleWCharacterSize.Height / 100f);

            // Adjust cell position by space required for the separatorType
            if (LegendItem != null &&
                LegendItem.SeparatorType != LegendSeparatorStyle.None)
            {
                cellPosition.Bottom -= Legend.GetSeparatorSize(LegendItem.SeparatorType).Height;
            }
        }

        /// <summary>
        /// Measures legend cell size in chart relative coordinates.
        /// </summary>
        /// <param name="graph">
        /// Chart graphics.
        /// </param>
        /// <param name="fontSizeReducedBy">
        /// A positive or negative integer value that determines the how standard cell font size
        /// should be adjusted. As a result smaller or larger font can be used.
        /// </param>
        /// <param name="legendAutoFont">
        /// Auto fit font used in the legend.
        /// </param>
        /// <param name="singleWCharacterSize">
        /// Size of the 'W' character used to calculate elements.
        /// </param>
        /// <returns>Legend cell size.</returns>
        internal SKSize MeasureCell(
            ChartGraphics graph,
            int fontSizeReducedBy,
            SKFont legendAutoFont,
            SKSize singleWCharacterSize)
        {
            // Check if cached size may be reused
            if (_cachedCellSKSizeontReducedBy == fontSizeReducedBy &&
                !_cachedCellSize.IsEmpty)
            {
                return _cachedCellSize;
            }

            // Get cell font
            SKSize cellSize = SKSize.Empty;
            SKFont cellFont = GetCellFont(legendAutoFont, fontSizeReducedBy, out bool disposeFont);

            // Measure cell content size based on the type
            if (CellType == LegendCellType.SeriesSymbol)
            {
                cellSize.Width = (int)(Math.Abs(SeriesSymbolSize.Width) * singleWCharacterSize.Width / 100f);
                cellSize.Height = (int)(Math.Abs(SeriesSymbolSize.Height) * singleWCharacterSize.Height / 100f);
            }
            else if (CellType == LegendCellType.Image)
            {
                if (ImageSize.IsEmpty && Image.Length > 0)
                {
                    SKSize imageSize = new();

                    // Use original image size
                    if (Common.ImageLoader.GetAdjustedImageSize(Image, graph.Graphics, ref imageSize))
                    {
                        cellSize.Width = (int)imageSize.Width;
                        cellSize.Height = (int)imageSize.Height;
                    }
                }
                else
                {
                    cellSize.Width = (int)(Math.Abs(ImageSize.Width) * singleWCharacterSize.Width / 100f);
                    cellSize.Height = (int)(Math.Abs(ImageSize.Height) * singleWCharacterSize.Height / 100f);
                }
            }
            else if (CellType == LegendCellType.Text)
            {
                // Get current cell text taking in consideration keywords
                // and automatic text wrapping.
                string cellText = GetCellText();

                // Measure text size.
                // Note that extra "I" character added to add more horizontal spacing
                cellSize = graph.MeasureStringAbs(cellText + "I", cellFont);
            }
            else
            {
                throw (new InvalidOperationException(SR.ExceptionLegendCellTypeUnknown(CellType.ToString())));
            }

            // Add cell margins
            cellSize.Width += (int)((Margins.Left + Margins.Right) * singleWCharacterSize.Width / 100f);
            cellSize.Height += (int)((Margins.Top + Margins.Bottom) * singleWCharacterSize.Height / 100f);

            // Add space required for the separatorType
            if (LegendItem != null &&
                LegendItem.SeparatorType != LegendSeparatorStyle.None)
            {
                cellSize.Height += Legend.GetSeparatorSize(LegendItem.SeparatorType).Height;
            }

            // Dispose created font object
            if (disposeFont)
            {
                cellFont.Dispose();
                cellFont = null;
            }

            // Save calculated size
            _cachedCellSize = cellSize;
            _cachedCellSKSizeontReducedBy = fontSizeReducedBy;

            return cellSize;
        }

        /// <summary>
        /// Gets cell background color.
        /// </summary>
        /// <returns></returns>
        private SKColor GetCellBackColor()
        {
            SKColor resultColor = BackColor;
            if (BackColor == SKColor.Empty && Legend != null)
            {
                // Try getting back color from the associated column
                if (LegendItem != null)
                {
                    // Get index of this cell
                    int cellIndex = LegendItem.Cells.IndexOf(this);
                    // Check if associated column exsists
                    if (cellIndex >= 0 && cellIndex < Legend.CellColumns.Count &&
                            Legend.CellColumns[cellIndex].BackColor != SKColor.Empty)
                    {
                        resultColor = Legend.CellColumns[cellIndex].BackColor;
                    }
                }

                // Get font from the legend isInterlaced
                if (resultColor == SKColor.Empty &&
                    Legend.InterlacedRows &&
                    _rowIndex % 2 != 0)
                {
                    if (Legend.InterlacedRowsColor == SKColor.Empty)
                    {
                        // Automatically determine background color
                        // If isInterlaced strips color is not set - use darker color of the area
                        if (Legend.BackColor == SKColor.Empty)
                        {
                            resultColor = SKColors.LightGray;
                        }
                        else if (Legend.BackColor == SKColors.Transparent)
                        {
                            if (Chart.BackColor != SKColors.Transparent &&
                                Chart.BackColor != SKColors.Black)
                            {
                                resultColor = ChartGraphics.GetGradientColor(Chart.BackColor, SKColors.Black, 0.2);
                            }
                            else
                            {
                                resultColor = SKColors.LightGray;
                            }
                        }
                        else
                        {
                            resultColor = ChartGraphics.GetGradientColor(Legend.BackColor, SKColors.Black, 0.2);
                        }
                    }
                    else
                    {
                        resultColor = Legend.InterlacedRowsColor;
                    }
                }
            }
            return resultColor;
        }

        /// <summary>
        /// Gets default cell font. Font can be specified in the cell, column or in the legend.
        /// </summary>
        /// <param name="legendAutoFont">Auto fit font used in the legend.</param>
        /// <param name="fontSizeReducedBy">Number of points legend auto-font reduced by.</param>
        /// <param name="disposeFont">Returns a flag if result font object should be disposed.</param>
        /// <returns>Default cell font.</returns>
        private SKFont GetCellFont(SKFont legendAutoFont, int fontSizeReducedBy, out bool disposeFont)
        {
            SKFont cellFont = Font;
            disposeFont = false;

            // Check if font is not set in the cell and legend object reference is valid
            if (cellFont == null &&
                Legend != null)
            {
                // Try getting font from the associated column
                if (LegendItem != null)
                {
                    // Get index of this cell
                    int cellIndex = LegendItem.Cells.IndexOf(this);
                    if (cellIndex >= 0 && cellIndex < Legend.CellColumns.Count &&
                            Legend.CellColumns[cellIndex].Font != null)
                    {
                        cellFont = Legend.CellColumns[cellIndex].Font;
                    }
                }

                // Get font from the legend
                if (cellFont == null)
                {
                    cellFont = legendAutoFont;

                    // No further processing required.
                    // Font is already reduced.
                    return cellFont;
                }
            }

            // Check if font size should be adjusted
            if (cellFont != null && fontSizeReducedBy != 0)
            {
                // New font is created anf it must be disposed
                disposeFont = true;

                // Calculate new font size
                int newFontSize = (int)Math.Round(cellFont.Size - fontSizeReducedBy);
                if (newFontSize < 1)
                {
                    // Font can't be less than size 1
                    newFontSize = 1;
                }

                // Create new font
                cellFont = new(cellFont.Typeface, newFontSize);
            }

            return cellFont;
        }

        /// <summary>
        /// Helper function that returns cell tooltip.
        /// </summary>
        /// <remarks>
        /// Tooltip can be set in the cell or in the legend item. Cell
        /// tooltip always has a higher priority.
        /// </remarks>
        /// <returns>Returns cell text.</returns>
        private string GetCellToolTip()
        {
            // Check if tooltip is set in the cell (highest priority)
            if (ToolTip.Length > 0)
            {
                return ToolTip;
            }

            // Check if tooltip is set in associated legend item
            if (LegendItem != null)
            {
                return LegendItem.ToolTip;
            }

            return string.Empty;
        }

        /// <summary>
        /// Helper function that returns cell url.
        /// </summary>
        /// <remarks>
        /// Url can be set in the cell or in the legend item. Cell
        /// tooltip always has a higher priority.
        /// </remarks>
        /// <returns>Returns cell text.</returns>
        private string GetCellUrl()
        {
            return string.Empty;
        }

        /// <summary>
        /// Helper function that returns cell url.
        /// </summary>
        /// <remarks>
        /// Url can be set in the cell or in the legend item. Cell
        /// tooltip always has a higher priority.
        /// </remarks>
        /// <returns>Returns cell text.</returns>
        private string GetCellMapAreaAttributes()
        {
            return string.Empty;
        }

        /// <summary>
        /// Helper function that returns cell url.
        /// </summary>
        /// <remarks>
        /// Url can be set in the cell or in the legend item. Cell
        /// tooltip always has a higher priority.
        /// </remarks>
        /// <returns>Returns cell text.</returns>
        private string GetCellPostBackValue()
        {
            return string.Empty;
        }

        /// <summary>
        /// Helper function that returns the exact text presented in the cell.
        /// </summary>
        /// <remarks>
        /// This method replaces the "\n" substring with the new line character
        /// and automatically wrap text if required.
        /// </remarks>
        /// <returns>Returns cell text.</returns>
        private string GetCellText()
        {
            // Replace all "\n" strings with the new line character
            string resultString = Text.Replace("\\n", "\n");

            // Replace the KeywordName.LegendText keyword with legend item Name property
            if (LegendItem != null)
            {
                resultString = resultString.Replace(KeywordName.LegendText, LegendItem.Name);
            }
            else
            {
                resultString = resultString.Replace(KeywordName.LegendText, "");
            }

            // Check if text width exceeds recomended character length
            if (Legend != null)
            {
                int recomendedTextLength = Legend.TextWrapThreshold;

                if (recomendedTextLength > 0 &&
                    resultString.Length > recomendedTextLength)
                {
                    // Iterate through all text characters
                    int lineLength = 0;
                    for (int charIndex = 0; charIndex < resultString.Length; charIndex++)
                    {
                        // Reset line length when new line character is found
                        if (resultString[charIndex] == '\n')
                        {
                            lineLength = 0;
                            continue;
                        }

                        // Increase line length counter
                        ++lineLength;

                        // Check if current character is a white space and
                        // current line length exceeds the recomended values.
                        if (char.IsWhiteSpace(resultString, charIndex) &&
                            lineLength >= recomendedTextLength)
                        {
                            // Insert new line character in the string
                            lineLength = 0;
                            resultString = resultString.Substring(0, charIndex) + "\n" +
                                resultString[(charIndex + 1)..].TrimStart();
                        }
                    }
                }
            }

            return resultString;
        }

        /// <summary>
        /// Helper function that returns cell text color.
        /// </summary>
        /// <returns>Cell text color.</returns>
        private SKColor GetCellForeColor()
        {
            // Check if cell text color defined in the cell
            if (ForeColor != SKColor.Empty)
            {
                return ForeColor;
            }

            // Check if color from the Column or legend should be used
            if (Legend != null)
            {
                // Try getting font from the associated column
                if (LegendItem != null)
                {
                    // Get index of this cell
                    int cellIndex = LegendItem.Cells.IndexOf(this);
                    // Check if associated column exsists
                    if (cellIndex >= 0 && cellIndex < Legend.CellColumns.Count &&
                            Legend.CellColumns[cellIndex].ForeColor != SKColor.Empty)
                    {
                        return Legend.CellColumns[cellIndex].ForeColor;
                    }
                }

                // Use legend text color
                return Legend.ForeColor;
            }

            return SKColors.Black;
        }

        #endregion Methods

        #region Cell Painting Methods

        /// <summary>
        /// Paints content of the legend cell.
        /// </summary>
        /// <param name="chartGraph">Chart graphics to draw content on.</param>
        /// <param name="fontSizeReducedBy">Number that determines how much the cell font should be reduced by.</param>
        /// <param name="legendAutoFont">Auto-fit font used in the legend.</param>
        /// <param name="singleWCharacterSize">Size of the 'W' character in auto-fit font.</param>
        internal void Paint(
            ChartGraphics chartGraph,
            int fontSizeReducedBy,
            SKFont legendAutoFont,
            SKSize singleWCharacterSize)
        {
            // Check cell size before painting
            if (cellPosition.Width <= 0 || cellPosition.Height <= 0)
            {
                return;
            }

            // Chart elements painting mode
            if (Common.ProcessModePaint)
            {
                // Check if cell background should be painted
                SKColor cellBackColor = GetCellBackColor();
                SKRect rectRelative = chartGraph.GetRelativeRectangle(cellPositionWithMargins);
                if (cellBackColor != SKColor.Empty)
                {
                    chartGraph.FillRectangleRel(
                        rectRelative,
                        cellBackColor,
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

                // Fire an event for custom cell back drawing
                Chart.CallOnPrePaint(new ChartPaintEventArgs(this, chartGraph, Common, new ElementPosition(rectRelative.Left, rectRelative.Top, rectRelative.Width, rectRelative.Height)));

                // Check legend cell type
                switch (CellType)
                {
                    case (LegendCellType.Text):
                        this.PaintCellText(chartGraph, fontSizeReducedBy, legendAutoFont);
                        break;

                    case (LegendCellType.Image):
                        PaintCellImage(chartGraph, singleWCharacterSize);
                        break;

                    case (LegendCellType.SeriesSymbol):
                        this.PaintCellSeriesSymbol(chartGraph, singleWCharacterSize);
                        break;

                    default:
                        throw (new InvalidOperationException(SR.ExceptionLegendCellTypeUnknown(CellType.ToString())));
                }

                // Fire an event for custom cell drawing
                Chart.CallOnPostPaint(new ChartPaintEventArgs(this, chartGraph, Common, new ElementPosition(rectRelative.Left, rectRelative.Top, rectRelative.Width, rectRelative.Height)));
            }
#if DEBUG
            // Draw bounding rectangle for debug purpose
            //				SKRect absRectangle = this.cellPosition;
            //				chartGraph.DrawRectangle(Pens.Red, absRectangle.X, absRectangle.Y, absRectangle.Width, absRectangle.Height);
#endif // DEBUG

            // Legend cell selection mode
            if (Common.ProcessModeRegions)
            {
                // Add hot region.
                // Note that legend cell is passed as sub-object of legend item
                Common.HotRegionsList.AddHotRegion(
                    chartGraph.GetRelativeRectangle(cellPositionWithMargins),
                    GetCellToolTip(),
                    GetCellUrl(),
                    GetCellMapAreaAttributes(),
                    GetCellPostBackValue(),
                    LegendItem,
                    this,
                    ChartElementType.LegendItem,
                    LegendItem.SeriesName);
            }
        }

        /// <summary>
        /// Draw legend cell text.
        /// </summary>
        /// <param name="chartGraph">Chart graphics to draw the text on.</param>
        /// <param name="fontSizeReducedBy">Number that determines how much the cell font should be reduced by.</param>
        /// <param name="legendAutoFont">Auto-fit font used in the legend.</param>
        private void PaintCellText(
            ChartGraphics chartGraph,
            int fontSizeReducedBy,
            SKFont legendAutoFont)
        {
            // Get cell font
            SKFont cellFont = this.GetCellFont(legendAutoFont, fontSizeReducedBy, out bool disposeFont);

            // Create font brush
            using (SKPaint fontBrush = new() { Color = GetCellForeColor(), Style = SKPaintStyle.Fill })
            {
                // Create cell text format
                using StringFormat format = new(StringFormat.GenericDefault);
                format.FormatFlags = StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.Alignment = StringAlignment.Center;
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

                // Measure string height out of one character
                SKSize charSize = chartGraph.MeasureStringAbs(GetCellText(), cellFont, new SKSize(10000f, 10000f), format);

                // If height of one characte is more than rectangle heigjt - remove LineLimit flag
                if (charSize.Height > cellPosition.Height && (format.FormatFlags & StringFormatFlags.LineLimit) != 0)
                {
                    format.FormatFlags ^= StringFormatFlags.LineLimit;
                }
                else if (charSize.Height < cellPosition.Height && (format.FormatFlags & StringFormatFlags.LineLimit) == 0)
                {
                    format.FormatFlags |= StringFormatFlags.LineLimit;
                }

                // Draw text
                chartGraph.DrawStringRel(
                    GetCellText(),
                    cellFont,
                    fontBrush,
                    chartGraph.GetRelativeRectangle(cellPosition),
                    format);
            }

            // Dispose created cell font object
            if (disposeFont)
            {
                cellFont.Dispose();
            }
        }

        /// <summary>
        /// Paints cell image.
        /// </summary>
        /// <param name="chartGraph">Graphics used to draw cell image.</param>
        /// <param name="singleWCharacterSize">Size of the 'W' character in auto-fit font.</param>
        private void PaintCellImage(
            ChartGraphics chartGraph,
            SKSize singleWCharacterSize)
        {
            if (Image.Length > 0)
            {
                // Get image size in relative coordinates
                SKRect imagePosition = SKRect.Empty;
                SKImage image = Common.ImageLoader.LoadImage(Image);

                SKSize imageSize = new();

                ImageLoader.GetAdjustedImageSize(image, chartGraph.Graphics, ref imageSize);

                imagePosition.Size = new((int)imageSize.Width, (int)imageSize.Height);

                // Calculate cell position
                SKRect imageCellPosition = cellPosition;
                imageCellPosition.Size = new(imagePosition.Width, imagePosition.Height);
                if (!ImageSize.IsEmpty)
                {
                    // Adjust cell size using image symbol size specified
                    if (ImageSize.Width > 0)
                    {
                        int newWidth = (int)(ImageSize.Width * singleWCharacterSize.Width / 100f);
                        if (newWidth > cellPosition.Width)
                        {
                            newWidth = (int)cellPosition.Width;
                        }
                        imageCellPosition.Size = new(newWidth, imageCellPosition.Height);
                    }
                    if (ImageSize.Height > 0)
                    {
                        int newHeight = (int)(ImageSize.Height * singleWCharacterSize.Height / 100f);
                        if (newHeight > cellPosition.Height)
                        {
                            newHeight = (int)cellPosition.Height;
                        }
                        imageCellPosition.Size = new(imageCellPosition.Width, newHeight);
                    }
                }

                // Make sure image size fits into the cell drawing rectangle
                float scaleValue = 1f;
                if (imagePosition.Height > imageCellPosition.Height)
                {
                    scaleValue = imagePosition.Height / imageCellPosition.Height;
                }
                if (imagePosition.Width > imageCellPosition.Width)
                {
                    scaleValue = Math.Max(scaleValue, imagePosition.Width / imageCellPosition.Width);
                }

                // Scale image size
                imagePosition.Size = new((int)(imagePosition.Width / scaleValue), (int)(imagePosition.Height / scaleValue));

                // Get image location
                imagePosition.Left = (int)((cellPosition.Left + cellPosition.Width / 2f) - imagePosition.Width / 2f);
                imagePosition.Top = (int)((cellPosition.Top + cellPosition.Height / 2f) - imagePosition.Height / 2f);

                // Adjust image location based on the cell content alignment
                if (Alignment == ContentAlignment.BottomLeft ||
                    Alignment == ContentAlignment.MiddleLeft ||
                    Alignment == ContentAlignment.TopLeft)
                {
                    imagePosition.Left = cellPosition.Left;
                }
                else if (Alignment == ContentAlignment.BottomRight ||
                    Alignment == ContentAlignment.MiddleRight ||
                    Alignment == ContentAlignment.TopRight)
                {
                    imagePosition.Left = cellPosition.Right - imagePosition.Width;
                }

                if (Alignment == ContentAlignment.BottomCenter ||
                    Alignment == ContentAlignment.BottomLeft ||
                    Alignment == ContentAlignment.BottomRight)
                {
                    imagePosition.Top = cellPosition.Bottom - imagePosition.Height;
                }
                else if (Alignment == ContentAlignment.TopCenter ||
                    Alignment == ContentAlignment.TopLeft ||
                    Alignment == ContentAlignment.TopRight)
                {
                    imagePosition.Top = cellPosition.Top;
                }

                // Draw image
                chartGraph.DrawImage(
                    image,
                    imagePosition,
                    0,
                    0,
                    image.Width,
                    image.Height,
                    new SKPaint() { Style = SKPaintStyle.Fill });
            }
        }

        /// <summary>
        /// Paint a series symbol in the cell.
        /// </summary>
        /// <param name="chartGraph">Chart graphics</param>
        /// <param name="singleWCharacterSize">Size of the 'W' character in auto-fit font.</param>
        private void PaintCellSeriesSymbol(
            ChartGraphics chartGraph,
            SKSize singleWCharacterSize)
        {
            //Cache legend item
            LegendItem legendItem = LegendItem;

            // Calculate cell position
            SKRect seriesMarkerPosition = cellPosition;

            // Adjust cell size using image symbol size specified
            if (SeriesSymbolSize.Width >= 0)
            {
                int newWidth = (int)(SeriesSymbolSize.Width * singleWCharacterSize.Width / 100f);
                if (newWidth > cellPosition.Width)
                {
                    newWidth = (int)cellPosition.Width;
                }
                seriesMarkerPosition.Size = new(newWidth, seriesMarkerPosition.Height);
            }
            if (SeriesSymbolSize.Height >= 0)
            {
                int newHeight = (int)(SeriesSymbolSize.Height * singleWCharacterSize.Height / 100f);
                if (newHeight > cellPosition.Height)
                {
                    newHeight = (int)cellPosition.Height;
                }
                seriesMarkerPosition.Size = new(seriesMarkerPosition.Width, newHeight);
            }

            // Check for empty size
            if (seriesMarkerPosition.Height <= 0 || seriesMarkerPosition.Width <= 0)
            {
                return;
            }

            // Get symbol location
            seriesMarkerPosition.Left = (int)((cellPosition.Left + cellPosition.Width / 2f) - seriesMarkerPosition.Width / 2f);
            seriesMarkerPosition.Top = (int)((cellPosition.Top + cellPosition.Height / 2f) - seriesMarkerPosition.Height / 2f);

            // Adjust image location based on the cell content alignment
            if (Alignment == ContentAlignment.BottomLeft ||
                Alignment == ContentAlignment.MiddleLeft ||
                Alignment == ContentAlignment.TopLeft)
            {
                seriesMarkerPosition.Left = cellPosition.Left;
            }
            else if (Alignment == ContentAlignment.BottomRight ||
                Alignment == ContentAlignment.MiddleRight ||
                Alignment == ContentAlignment.TopRight)
            {
                seriesMarkerPosition.Left = cellPosition.Right - seriesMarkerPosition.Width;
            }

            if (Alignment == ContentAlignment.BottomCenter ||
                Alignment == ContentAlignment.BottomLeft ||
                Alignment == ContentAlignment.BottomRight)
            {
                seriesMarkerPosition.Top = cellPosition.Bottom - seriesMarkerPosition.Height;
            }
            else if (Alignment == ContentAlignment.TopCenter ||
                Alignment == ContentAlignment.TopLeft ||
                Alignment == ContentAlignment.TopRight)
            {
                seriesMarkerPosition.Top = cellPosition.Top;
            }

            // Draw legend item image
            if (legendItem.Image.Length > 0)
            {
                // Get image size
                SKRect imageScale = SKRect.Empty;
                SKImage image = Common.ImageLoader.LoadImage(legendItem.Image);

                if (image != null)
                {
                    SKSize imageSize = new();

                    ImageLoader.GetAdjustedImageSize(image, chartGraph.Graphics, ref imageSize);

                    imageScale.Size = new((int)imageSize.Width, (int)imageSize.Height);

                    // Make sure image size fits into the drawing rectangle
                    float scaleValue = 1f;
                    if (imageScale.Height > seriesMarkerPosition.Height)
                    {
                        scaleValue = imageScale.Height / seriesMarkerPosition.Height;
                    }
                    if (imageScale.Width > seriesMarkerPosition.Width)
                    {
                        scaleValue = Math.Max(scaleValue, imageScale.Width / seriesMarkerPosition.Width);
                    }

                    // Scale image size
                    imageScale.Size = new((int)(imageScale.Width / scaleValue), (int)(imageScale.Height / scaleValue));

                    imageScale.Left = (int)((seriesMarkerPosition.Left + seriesMarkerPosition.Width / 2f) - imageScale.Width / 2f);
                    imageScale.Top = (int)((seriesMarkerPosition.Top + seriesMarkerPosition.Height / 2f) - imageScale.Height / 2f);

                    // Draw image
                    chartGraph.DrawImage(
                        image,
                        imageScale,
                        0,
                        0,
                        image.Width,
                        image.Height,
                        new SKPaint() { Style = SKPaintStyle.Fill });
                }
            }
            else
            {
                int maxShadowOffset = 3;
                int maxBorderWidth = 3;

                if (legendItem.ImageStyle == LegendImageStyle.Rectangle)
                {
                    int maxBorderWidthRect = 2;

                    // Draw series rectangle
                    chartGraph.FillRectangleRel(
                        chartGraph.GetRelativeRectangle(seriesMarkerPosition),
                        legendItem.Color,
                        legendItem.BackHatchStyle,
                        legendItem.Image,
                        legendItem.backImageWrapMode,
                        legendItem.BackImageTransparentColor,
                        legendItem.backImageAlign,
                        legendItem.backGradientStyle,
                        legendItem.backSecondaryColor,
                        legendItem.borderColor,
                        (legendItem.BorderWidth > maxBorderWidthRect) ? maxBorderWidthRect : legendItem.BorderWidth,
                        legendItem.BorderDashStyle,
                        legendItem.ShadowColor,
                        (legendItem.ShadowOffset > maxShadowOffset) ? maxShadowOffset : legendItem.ShadowOffset,
                        PenAlignment.Inset);
                }
                if (legendItem.ImageStyle == LegendImageStyle.Line)
                {
                    // Prepare line coordinates
                    SKPoint point1 = new();
                    point1.X = seriesMarkerPosition.Left;
                    point1.Y = (int)(seriesMarkerPosition.Top + seriesMarkerPosition.Height / 2F);
                    SKPoint point2 = new();
                    point2.Y = point1.Y;
                    point2.X = seriesMarkerPosition.Right;

                    // Disable antialiasing
                    SmoothingMode oldMode = chartGraph.SmoothingMode;
                    chartGraph.SmoothingMode = SmoothingMode.None;

                    // Draw line
                    chartGraph.DrawLineRel(
                        legendItem.Color,
                        (legendItem.borderWidth > maxBorderWidth) ? maxBorderWidth : legendItem.borderWidth,
                        legendItem.borderDashStyle,
                        chartGraph.GetRelativePoint(point1),
                        chartGraph.GetRelativePoint(point2),
                        legendItem.shadowColor,
                        (legendItem.shadowOffset > maxShadowOffset) ? maxShadowOffset : legendItem.shadowOffset);

                    // Restore antialiasing mode
                    chartGraph.SmoothingMode = oldMode;
                }

                // Draw symbol (for line also)
                if (legendItem.ImageStyle == LegendImageStyle.Marker ||
                    legendItem.ImageStyle == LegendImageStyle.Line)
                {
                    MarkerStyle markerStyle = legendItem.markerStyle;
                    if (legendItem.style == LegendImageStyle.Marker)
                    {
                        markerStyle = (legendItem.markerStyle == MarkerStyle.None) ?
                            MarkerStyle.Circle : legendItem.markerStyle;
                    }

                    if (markerStyle != MarkerStyle.None ||
                        legendItem.markerImage.Length > 0)
                    {
                        // Calculate marker size
                        int markerSize = (int)Math.Min(seriesMarkerPosition.Width, seriesMarkerPosition.Height);
                        markerSize = (int)Math.Min(legendItem.markerSize, (legendItem.style == LegendImageStyle.Line) ? 2f * (markerSize / 3f) : markerSize);

                        // Reduce marker size to fit border
                        int markerBorderWidth = (legendItem.MarkerBorderWidth > maxBorderWidth) ? maxBorderWidth : legendItem.MarkerBorderWidth;
                        if (markerBorderWidth > 0)
                        {
                            markerSize -= markerBorderWidth;
                            if (markerSize < 1)
                            {
                                markerSize = 1;
                            }
                        }

                        // Draw marker
                        SKPoint point = new();
                        point.X = (int)(seriesMarkerPosition.Left + seriesMarkerPosition.Width / 2f);
                        point.Y = (int)(seriesMarkerPosition.Top + seriesMarkerPosition.Height / 2f);

                        // Calculate image scale
                        SKRect imageScale = SKRect.Empty;
                        if (legendItem.markerImage.Length > 0)
                        {
                            // Get image size
                            SKImage image = Common.ImageLoader.LoadImage(legendItem.markerImage);

                            SKSize imageSize = new();

                            ImageLoader.GetAdjustedImageSize(image, chartGraph.Graphics, ref imageSize);
                            imageScale.Size = new((int)imageSize.Width, (int)imageSize.Height);

                            // Make sure image size fits into the drawing rectangle
                            float scaleValue = 1f;
                            if (imageScale.Height > seriesMarkerPosition.Height)
                            {
                                scaleValue = imageScale.Height / seriesMarkerPosition.Height;
                            }
                            if (imageScale.Width > seriesMarkerPosition.Width)
                            {
                                scaleValue = Math.Max(scaleValue, imageScale.Width / seriesMarkerPosition.Width);
                            }

                            // Scale image size
                            imageScale.Size = new((int)(imageScale.Width / scaleValue), (int)(imageScale.Height / scaleValue));
                        }

                        // Adjust marker position so that it always drawn on pixel
                        // boundary.
                        SKPoint SKPoint = new(point.X, point.Y);
                        if ((markerSize % 2) != 0.0)
                        {
                            SKPoint.X -= 0.5f;
                            SKPoint.Y -= 0.5f;
                        }

                        // Draw marker if it's not image
                        chartGraph.DrawMarkerRel(
                            chartGraph.GetRelativePoint(SKPoint),
                            markerStyle,
                            markerSize,
                            (legendItem.markerColor == SKColor.Empty) ? legendItem.Color : legendItem.markerColor,
                            (legendItem.markerBorderColor == SKColor.Empty) ? legendItem.borderColor : legendItem.markerBorderColor,
                            markerBorderWidth,
                            legendItem.markerImage,
                            legendItem.markerImageTransparentColor,
                            (legendItem.shadowOffset > maxShadowOffset) ? maxShadowOffset : legendItem.shadowOffset,
                            legendItem.shadowColor,
                            imageScale);
                    }
                }
            }
        }

        #endregion Cell Painting Methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _fontCache != null)
            {
                _fontCache.Dispose();
                _fontCache = null;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// The Margins class represents the margins for various chart elements.
    /// </summary>
    [
    SRDescription("DescriptionAttributeMargins_Margins"),
    ]
    public class Margins
    {
        #region Fields

        // Top margin
        private int _top = 0;

        // Bottom margin
        private int _bottom = 0;

        // Left margin
        private int _left = 0;

        // Right margin
        private int _right = 0;

        // Reference to common chart elements which allows to invalidate
        // chart when one of the properties is changed.
        internal CommonElements Common = null;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Margins constructor.
        /// </summary>
        public Margins()
        {
        }

        /// <summary>
        /// Margins constructor.
        /// </summary>
        /// <param name="top">Top margin.</param>
        /// <param name="bottom">Bottom margin.</param>
        /// <param name="left">Left margin.</param>
        /// <param name="right">Right margin.</param>
        public Margins(int top, int bottom, int left, int right)
        {
            _top = top;
            _bottom = bottom;
            _left = left;
            _right = right;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the top margin.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeMargins_Top"),
        ]
        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentException(SR.ExceptionMarginTopIsNegative, nameof(value)));
                }
                _top = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the bottom margin.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeMargins_Bottom"),
        ]
        public int Bottom
        {
            get
            {
                return _bottom;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentException(SR.ExceptionMarginBottomIsNegative, nameof(value)));
                }
                _bottom = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeMargins_Left"),
        ]
        public int Left
        {
            get
            {
                return _left;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentException(SR.ExceptionMarginLeftIsNegative, nameof(value)));
                }
                _left = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeMargins_Right"),
        ]
        public int Right
        {
            get
            {
                return _right;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentException(SR.ExceptionMarginRightIsNegative, nameof(value)));
                }
                _right = value;
                Invalidate();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Convert margins object to string.
        /// </summary>
        /// <returns>A string that represents the margins object.</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:D}, {1:D}, {2:D}, {3:D}",
                Top,
                Bottom,
                Left,
                Right);
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object.
        /// </summary>
        /// <param name="obj">
        /// The Object to compare with the current Object.
        /// </param>
        /// <returns>
        /// True if the specified Object is equal to the current Object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Margins margins && Top == margins.Top &&
                    Bottom == margins.Bottom &&
                    Left == margins.Left &&
                    Right == margins.Right)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets object hash code.
        /// </summary>
        /// <returns>Margins object hash value.</returns>
         public override int GetHashCode()
        {
            return Top.GetHashCode() + Bottom.GetHashCode() + Left.GetHashCode() + Right.GetHashCode();
        }

        /// <summary>
        /// Checks if there is no margin.
        /// </summary>
        /// <returns>
        /// <b>True</b> if all margins values are zeros.
        /// </returns>
        public bool IsEmpty()
        {
            return (Top == 0 && Bottom == 0 && Left == 0 && Right == 0);
        }

        /// <summary>
        /// Converts Margins class to SKRect class.
        /// </summary>
        /// <returns>A SKRect class that contains the values of the margins.</returns>
        public SKRect ToSKRect()
        {
            return new SKRect(Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Invalidates chart.
        /// </summary>
        private void Invalidate()
        {
            if (Common != null && Common.Chart != null)
            {
                ChartService.Invalidate();
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// <b>LegendCellCollection</b> is a strongly typed collection of LegendCell objects.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLegendCellCollection_LegendCellCollection"),
    ]
    public class LegendCellCollection : ChartNamedElementCollection<LegendCell>
    {
        #region Constructors

        /// <summary>
        /// LegendCellCollection constructor.
        /// </summary>
        /// <remarks>
        /// This constructor is for internal use and should not be part of documentation.
        /// </remarks>
        /// <param name="parent">Legend item this collection belongs to.</param>
        internal LegendCellCollection(LegendItem parent) : base(parent)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds a cell to the end of the collection.
        /// </summary>
        /// <param name="cellType">
        /// A <see cref="LegendCellType"/> value representing the cell type.
        /// </param>
        /// <param name="text">
        /// A <b>string</b> value representing cell text or image name depending
        /// on the <b>cellType</b> parameter.
        /// </param>
        /// <param name="alignment">
        /// A <see cref="ContentAlignment"/> value representing cell content alignment.
        /// </param>
        /// <returns>
        /// Index of the newly added object.
        /// </returns>
        public int Add(LegendCellType cellType, string text, ContentAlignment alignment)
        {
            Add(new LegendCell(cellType, text, alignment));
            return Count - 1;
        }

        /// <summary>
        /// Inserts a cell into the collection.
        /// </summary>
        /// <param name="index">
        /// Index to insert the object at.
        /// </param>
        /// <param name="cellType">
        /// A <see cref="LegendCellType"/> value representing the cell type.
        /// </param>
        /// <param name="text">
        /// A <b>string</b> value representing cell text or image name depending
        /// on the <b>cellType</b> parameter.
        /// </param>
        /// <param name="alignment">
        /// A <see cref="ContentAlignment"/> value representing cell content alignment.
        /// </param>
        public void Insert(int index, LegendCellType cellType, string text, ContentAlignment alignment)
        {
            Insert(index, new LegendCell(cellType, text, alignment));
        }

        #endregion Methods
    }

    /// <summary>
    /// The <b>LegendCellColumnCollection</b> class is a strongly typed collection
    /// of LegendCellColumn objects.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLegendCellColumnCollection_LegendCellColumnCollection"),
    ]
    public class LegendCellColumnCollection : ChartNamedElementCollection<LegendCellColumn>
    {
        #region Construction and Initialization

        /// <summary>
        /// LegendCellColumnCollection constructor.
        /// </summary>
        /// <remarks>
        /// This constructor is for internal use and should not be part of documentation.
        /// </remarks>
        /// <param name="legend">
        /// Chart legend which this collection belongs to.
        /// </param>
        internal LegendCellColumnCollection(Legend legend)
            : base(legend)
        {
        }

        #endregion Construction and Initialization

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Free managed resources
                foreach (LegendCellColumn item in this)
                {
                    item.Dispose();
                }
                ClearItems();
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}