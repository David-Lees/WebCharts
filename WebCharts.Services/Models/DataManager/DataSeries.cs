// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Chart series collection class and series properties class.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace WebCharts.Services
{
    #region Series enumerations

    /// <summary>
    /// Chart axis type (Primary or Secondary).
    /// </summary>
    public enum AxisType
    {
        /// <summary>
        /// Primary axis. For X axis - bottom, for Y axis - left.
        /// </summary>
        Primary,

        /// <summary>
        /// Secondary axis. For X axis - top, for Y axis - right.
        /// </summary>
        Secondary
    };

    /// <summary>
    /// Sorting order (Ascending or Descending).
    /// </summary>
    public enum PointSortOrder
    {
        /// <summary>
        /// Ascending sorting order
        /// </summary>
        Ascending,

        /// <summary>
        /// Descending sorting order
        /// </summary>
        Descending
    }

    #endregion Series enumerations

    /// <summary>
    /// Data series collection
    /// </summary>
    [
        SRDescription("DescriptionAttributeSeriesCollection_SeriesCollection"),
    ]
    public class SeriesCollection : ChartNamedElementCollection<Series>
    {
        #region Constructors

        /// <summary>
        /// Data series collection object constructor.
        /// </summary>
        internal SeriesCollection(DataManager dataManager)
            : base(dataManager)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Creates a new Series with the specified name and adds it to the collection.
        /// </summary>
        /// <param name="name">The new chart area name.</param>
        /// <returns>New series</returns>
        public Series Add(string name)
        {
            Series series = new(name);
            Add(series);
            return series;
        }

        /// <summary>
        /// Fixes the name references of the item.
        /// </summary>
        /// <param name="item">Item to verify and fix.</param>
        internal override void FixNameReferences(Series item)
        {
            if (item != null && Chart != null)
            {
                if (String.IsNullOrEmpty(item.ChartArea) && Chart.ChartAreas != null)
                {
                    item.ChartArea = Chart.ChartAreas.DefaultNameReference;
                }
                if (String.IsNullOrEmpty(item.Legend) && Chart.Legends != null)
                {
                    item.Legend = Chart.Legends.DefaultNameReference;
                }
            }
        }

        #endregion Methods

        #region Event handlers

        /// <summary>
        /// Updates the Series' references to ChartAreas.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Charting.NameReferenceChangedEventArgs"/> instance containing the event data.</param>
        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            foreach (Series series in this)
                if (series.ChartArea == e.OldName)
                    series.ChartArea = e.NewName;
        }

        /// <summary>
        /// Updates the Series' references to Legends.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Charting.NameReferenceChangedEventArgs"/> instance containing the event data.</param>
        internal void LegendNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            foreach (Series series in this)
                if (series.Legend == e.OldName)
                    series.Legend = e.NewName;
        }

        #endregion Event handlers
    }

    /// <summary>
    /// The class stores the data points and the default series properties.
    /// </summary>
    [
    SRDescription("DescriptionAttributeSeries_Series"),
    ]
    public class Series : DataPointCustomProperties
    {
        #region Fields

        // Private data members, which store properties values
        private ChartValueType _xValueType = ChartValueType.Auto;

        private ChartValueType _yValueType = ChartValueType.Auto;
        private bool _isXValueIndexed = false;
        private int _yValuesPerPoint = 1;
        private int _markersStep = 1;
        private ChartColorPalette _colorPalette = ChartColorPalette.None;
        private AxisType _xAxisType = AxisType.Primary;
        private AxisType _yAxisType = AxisType.Primary;

        private DataPointCustomProperties _emptyPointCustomProperties = null;
        private DataPointCollection _points;
        private int _shadowOffset = 0;
        private SKColor _shadowColor = new(0, 0, 0, 128);
        private string _chartType = ChartTypeNames.Column;
        private string _chartArea = String.Empty;

        // Series enabled flag
        private bool _enabled = true;

        // Legend name used by this series
        private string _legend = String.Empty;

        // Member of the chart data source used to data bind to the X value of the series.
        private string _dataSourceXMember = String.Empty;

        // Members of the chart data source used to data bind to the Y values of the series.
        private string _dataSourceYMembers = String.Empty;

        // Automatic values type flags
        internal bool autoXValueType = false;

        internal bool autoYValueType = false;

        // Total Y value of all data points
        private double _totalYvalue = double.NaN;

        // Array of dummy data used at design time
        private double[] _dummyDoubleValues = null;

        // X value type if X value is indexed
        internal ChartValueType indexedXValueType = ChartValueType.Auto;

        // Default properties
        static readonly internal DataPointCustomProperties defaultCustomProperties = InitializeDefaultCustomProperties();

        // Indicates that a temp. marker style was set for drawing
        internal bool tempMarkerStyleIsSet = false;

        // Indicates that number of Y values should be checked
        private bool _checkPointsNumber = true;

        // SmartLabelStyle style
        private SmartLabelStyle _smartLabelStyle = null;

        // Indicates that there is no custom axis labels in data points or series
        internal bool noLabelsInPoints = true;

        // Indicates if series has all X values set to 0
        internal bool xValuesZeros = false;

        // Indicates if check for series X zero values was done
        internal bool xValuesZerosChecked = false;

        // fake data points for selector service in design time.
        // note: in design time fake points are generated
        // with short life time - during painting.
        // this collection keep a copy of design time datapoints.
        internal DataPointCollection fakeDataPoints;

        #endregion Fields

        #region Series properties fields

        /// <summary>
        /// Data point label text.
        /// </summary>
        internal string label = "";

        /// <summary>
        /// Data point X axis label text.
        /// </summary>
        internal string axisLabel = "";

        /// <summary>
        /// Data point label format string
        /// </summary>
        internal string labelFormat = "";

        /// <summary>
        /// If true shows point's value as a label.
        /// </summary>
        internal bool showLabelAsValue = false;

        /// <summary>
        /// Data point color
        /// </summary>
        internal SKColor color = SKColor.Empty;

        /// <summary>
        /// Data point border color
        /// </summary>
        internal SKColor borderColor = SKColor.Empty;

        /// <summary>
        /// Data point border style
        /// </summary>
        internal ChartDashStyle borderDashStyle = ChartDashStyle.Solid;

        /// <summary>
        /// Data point border width
        /// </summary>
        internal int borderWidth = 1;

        /// <summary>
        /// Data point marker border width
        /// </summary>
        internal int markerBorderWidth = 1;

        /// <summary>
        /// Data point background image
        /// </summary>
        internal string backImage = "";

        /// <summary>
        /// Data point background image drawing mode.
        /// </summary>
        internal ChartImageWrapMode backImageWrapMode = ChartImageWrapMode.Tile;

        /// <summary>
        /// Background image transparent color.
        /// </summary>
        internal SKColor backImageTransparentColor = SKColor.Empty;

        /// <summary>
        /// Background image alignment used by ClampUnscale drawing mode.
        /// </summary>
        internal ChartImageAlignmentStyle backImageAlignment = ChartImageAlignmentStyle.TopLeft;

        /// <summary>
        /// Data point background gradient type.
        /// </summary>
        internal GradientStyle backGradientStyle = GradientStyle.None;

        /// <summary>
        /// Data point background gradient end color
        /// </summary>
        internal SKColor backSecondaryColor = SKColor.Empty;

        /// <summary>
        /// Data point hatch style
        /// </summary>
        internal ChartHatchStyle backHatchStyle = ChartHatchStyle.None;

        /// <summary>
        /// Font cache for the fonts used in the Series and DataPoint
        /// </summary>
        private FontCache _fontCache = new();

        /// <summary>
        /// Data point font
        /// </summary>
        internal SKFont font = null;

        /// <summary>
        /// Data point line color
        /// </summary>
        internal SKColor fontColor = SKColors.Black;

        /// <summary>
        /// Data point font angle
        /// </summary>
        internal int fontAngle = 0;

        /// <summary>
        /// Data point marker style
        /// </summary>
        internal MarkerStyle markerStyle = MarkerStyle.None;

        /// <summary>
        /// Data point marker size
        /// </summary>
        internal int markerSize = 5;

        /// <summary>
        /// Data point marker image
        /// </summary>
        internal string markerImage = "";

        /// <summary>
        /// Data point marker image transparent color.
        /// </summary>
        internal SKColor markerImageTransparentColor = SKColor.Empty;

        /// <summary>
        /// Data point marker color
        /// </summary>
        internal SKColor markerColor = SKColor.Empty;

        /// <summary>
        /// Data point marker border color
        /// </summary>
        internal SKColor markerBorderColor = SKColor.Empty;

        /// <summary>
        /// The tooltip.
        /// </summary>
        internal string toolTip = "";

        /// <summary>
        /// Indicates that item is shown in the legend.
        /// </summary>
        internal bool showInLegend = true;

        /// <summary>
        /// Text of the item in the legend
        /// </summary>
        internal string legendText = "";

        /// <summary>
        /// Tooltip of the item in the legend
        /// </summary>
        internal string legendToolTip = "";

        /// <summary>
        /// Data point label back color
        /// </summary>
        internal SKColor labelBackColor = SKColor.Empty;

        /// <summary>
        /// Data point label border color
        /// </summary>
        internal SKColor labelBorderColor = SKColor.Empty;

        /// <summary>
        /// Data point label border style
        /// </summary>
        internal ChartDashStyle labelBorderDashStyle = ChartDashStyle.Solid;

        /// <summary>
        /// Data point label border width
        /// </summary>
        internal int labelBorderWidth = 1;

        /// <summary>
        /// Tooltip of the data point label
        /// </summary>
        internal string labelToolTip = "";

        #endregion Series properties fields

        #region Constructors and initialization

        /// <summary>
        /// Initializes the default custom properties field.
        /// </summary>
        /// <returns>A DataPointCustomProperties initialized to defaults</returns>
        private static DataPointCustomProperties InitializeDefaultCustomProperties()
        {
            DataPointCustomProperties customProperties = new(null, false);
            customProperties.SetDefault(true);
            customProperties.pointCustomProperties = true;

            return customProperties;
        }

        /// <summary>
        /// Series object constructor.
        /// </summary>
        public Series() : base(null, false)
        {
            InitProperties(null, 0);
        }

        /// <summary>
        /// Series object constructor.
        /// </summary>
        /// <param name="name">Name of the data series</param>
        public Series(string name) : base(null, false)
        {
            if (name == null)
            {
                throw (new ArgumentNullException(SR.ExceptionDataSeriesNameIsEmpty));
            }

            InitProperties(name, 0);
        }

        /// <summary>
        /// Series object constructor.
        /// </summary>
        /// <param name="name">Name of the data series.</param>
        /// <param name="yValues">Number of y values per data point.</param>
        public Series(string name, int yValues)
            : base(null, false)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name), SR.ExceptionDataSeriesNameIsEmpty);
            }
            if (YValuesPerPoint < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(yValues), SR.ExceptionDataSeriesYValuesPerPointIsZero);
            }

            InitProperties(name, yValues);
        }

        /// <summary>
        /// Initialize series properties
        /// </summary>
        private void InitProperties(string name, int YValuesPerPoint)
        {
            font = _fontCache.DefaultFont;
            series = this;
            _emptyPointCustomProperties = new DataPointCustomProperties(this, false)
            {
                series = this
            };

            // Initialize properties
            _points = new DataPointCollection(this);

            fakeDataPoints = new DataPointCollection(this);

            if (name != null)
            {
                base.Name = name;
            }
            if (YValuesPerPoint != 0)
            {
                _yValuesPerPoint = YValuesPerPoint;
            }
            base.SetDefault(true);
            _emptyPointCustomProperties.SetDefault(true);
            _emptyPointCustomProperties.pointCustomProperties = true;
            //TODO : check if this is still needed.
            //#if !SQLRS_CONTROL
            //			    // Use transparent colors for empty points
            //			    emptyPointAttributes.Color = SKColors.Transparent;
            //			    emptyPointAttributes.BorderColor = SKColors.Transparent;
            //			    emptyPointAttributes.FontColor = SKColors.Transparent;
            //			    emptyPointAttributes.MarkerColor = SKColors.Transparent;
            //			    emptyPointAttributes.MarkerBorderColor = SKColors.Transparent;
            //#endif	//!SQLRS_CONTROL

            // Create SmartLabelStyle style object
            _smartLabelStyle = new SmartLabelStyle(this);
        }

        #endregion Constructors and initialization

        #region Helper methods

        /// <summary>
        /// Gets series caption that may not be the same as series name.
        /// </summary>
        /// <returns>Series caption string.</returns>
        internal string GetCaption()
        {
            if (IsCustomPropertySet("SeriesCaption"))
            {
                return this["SeriesCaption"];
            }
            return Name;
        }

        /// <summary>
        /// Gets custom points depth and gap depth from series properties.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="axis">Categorical axis.</param>
        /// <param name="pointDepth">Returns point depth.</param>
        /// <param name="pointGapDepth">Return point gap depth.</param>
        internal void GetPointDepthAndGap(
            ChartGraphics graph,
            Axis axis,
            ref double pointDepth,
            ref double pointGapDepth)
        {
            // Check if series provide custom value for point depth in pixels
            string attribValue = this[CustomPropertyName.PixelPointDepth];
            if (attribValue != null)
            {
                try
                {
                    pointDepth = CommonElements.ParseDouble(attribValue);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("PixelPointDepth")));
                }

                if (pointDepth <= 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotLargerThenZiro("PixelPointDepth")));
                }
                if (pointDepth > CustomPropertyRegistry.MaxValueOfPixelAttribute)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeMustBeInRange("PixelPointDepth", (0).ToString(CultureInfo.CurrentCulture), CustomPropertyRegistry.MaxValueOfPixelAttribute.ToString(CultureInfo.CurrentCulture))));
                }

                SKSize relativeSize = graph.GetRelativeSize(new SKSize((float)pointDepth, (float)pointDepth));
                pointDepth = relativeSize.Width;
                if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
                {
                    pointDepth = relativeSize.Height;
                }
            }

            // Check if series provide custom value for point gap depth in pixels
            attribValue = this[CustomPropertyName.PixelPointGapDepth];
            if (attribValue != null)
            {
                try
                {
                    pointGapDepth = CommonElements.ParseDouble(attribValue);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("PixelPointGapDepth")));
                }

                if (pointGapDepth <= 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotLargerThenZiro("PixelPointGapDepth")));
                }
                if (pointGapDepth > CustomPropertyRegistry.MaxValueOfPixelAttribute)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeMustBeInRange("PixelPointGapDepth", (0).ToString(CultureInfo.CurrentCulture), CustomPropertyRegistry.MaxValueOfPixelAttribute.ToString(CultureInfo.CurrentCulture))));
                }

                SKSize relativeSize = graph.GetRelativeSize(new SKSize((float)pointGapDepth, (float)pointGapDepth));
                pointGapDepth = relativeSize.Width;
                if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
                {
                    pointGapDepth = relativeSize.Height;
                }
            }
        }

        /// <summary>
        /// Gets data point width in relative coordinates.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="axis">Axis object.</param>
        /// <param name="interval">Current minimum axis interval.</param>
        /// <param name="defaultWidth">Default width in percentage of interval.</param>
        /// <returns>Point width.</returns>
        internal double GetPointWidth(
            ChartGraphics graph,
            Axis axis,
            double interval,
            double defaultWidth)
        {
            double pointPercentageWidth = defaultWidth;
            double pointWidth = 0.0;

            // Check if series provide custom value for point width in percentage of interval
            string strWidth = this[CustomPropertyName.PointWidth];
            if (strWidth != null)
            {
                pointPercentageWidth = CommonElements.ParseDouble(strWidth);
            }

            // Get column width in relative and pixel coordinates
            pointWidth = axis.GetPixelInterval(interval * pointPercentageWidth);
            SKSize pointSize = graph.GetAbsoluteSize(new SKSize((float)pointWidth, (float)pointWidth));
            double pixelPointWidth = pointSize.Width;
            if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
            {
                pixelPointWidth = pointSize.Height;
            }

            // Check if series provide custom value for Min point width in pixels
            bool usePixelWidth = false;
            string attribValue = this[CustomPropertyName.MinPixelPointWidth];
            if (attribValue != null)
            {
                double minPixelPointWidth = 0.0;
                try
                {
                    minPixelPointWidth = CommonElements.ParseDouble(attribValue);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("MinPixelPointWidth")));
                }
                if (minPixelPointWidth <= 0.0)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotLargerThenZiro("MinPixelPointWidth")));
                }
                if (minPixelPointWidth > CustomPropertyRegistry.MaxValueOfPixelAttribute)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeMustBeInRange("MinPixelPointWidth", (0).ToString(CultureInfo.CurrentCulture), CustomPropertyRegistry.MaxValueOfPixelAttribute.ToString(CultureInfo.CurrentCulture))));
                }

                if (pixelPointWidth < minPixelPointWidth)
                {
                    usePixelWidth = true;
                    pixelPointWidth = minPixelPointWidth;
                }
            }

            // Check if series provide custom value for Max point width in pixels
            attribValue = this[CustomPropertyName.MaxPixelPointWidth];
            if (attribValue != null)
            {
                double maxPixelPointWidth = 0.0;
                try
                {
                    maxPixelPointWidth = CommonElements.ParseDouble(attribValue);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("MaxPixelPointWidth")));
                }
                if (maxPixelPointWidth <= 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotLargerThenZiro("MaxPixelPointWidth")));
                }

                if (pixelPointWidth > maxPixelPointWidth)
                {
                    usePixelWidth = true;
                    pixelPointWidth = maxPixelPointWidth;
                }
            }

            // Check if series provide custom value for point width in pixels
            attribValue = this[CustomPropertyName.PixelPointWidth];
            if (attribValue != null)
            {
                usePixelWidth = true;
                pixelPointWidth = 0.0;
                try
                {
                    pixelPointWidth = CommonElements.ParseDouble(attribValue);
                }
                catch
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("PixelPointWidth")));
                }
                if (pixelPointWidth <= 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotLargerThenZiro("PixelPointWidth")));
                }
                if (pixelPointWidth > CustomPropertyRegistry.MaxValueOfPixelAttribute)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeMustBeInRange("PixelPointWidth", (0).ToString(CultureInfo.CurrentCulture), CustomPropertyRegistry.MaxValueOfPixelAttribute.ToString(CultureInfo.CurrentCulture))));
                }
            }

            // Translate pixel width to relative coordinates
            if (usePixelWidth)
            {
                SKSize pointRelativeSize = graph.GetRelativeSize(new SKSize((float)pixelPointWidth, (float)pixelPointWidth));
                pointWidth = pointRelativeSize.Width;
                if (axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
                {
                    pointWidth = pointRelativeSize.Height;
                }
            }

            return pointWidth;
        }

        /// <summary>
        /// Get chart type name by it's type
        /// </summary>
        /// <param name="type">Chart type.</param>
        /// <returns>Chart type name.</returns>
        static internal string GetChartTypeName(SeriesChartType type)
        {
            if (type == SeriesChartType.StackedArea100)
                return ChartTypeNames.OneHundredPercentStackedArea;
            if (type == SeriesChartType.StackedBar100)
                return ChartTypeNames.OneHundredPercentStackedBar;
            if (type == SeriesChartType.StackedColumn100)
                return ChartTypeNames.OneHundredPercentStackedColumn;
            return Enum.GetName(typeof(SeriesChartType), type);
        }

        /// <summary>
        /// Checks if Y values of the series represent date-time.
        /// </summary>
        /// <returns>True if date-time.</returns>
		internal bool IsYValueDateTime()
        {
            if (YValueType == ChartValueType.Date ||
                YValueType == ChartValueType.DateTime ||
                YValueType == ChartValueType.Time ||
                YValueType == ChartValueType.DateTimeOffset)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if X values of the series represent date-time.
        /// </summary>
        /// <returns>True if date-time.</returns>
		internal bool IsXValueDateTime()
        {
            if (XValueType == ChartValueType.Date ||
                XValueType == ChartValueType.DateTime ||
                XValueType == ChartValueType.Time ||
                XValueType == ChartValueType.DateTimeOffset)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if series is visible.
        /// </summary>
        /// <returns>True if series is visible.</returns>
        internal bool IsVisible()
        {
            // Check if enabled flag is set and the ChartArea is defined
            if (Enabled && !String.IsNullOrEmpty(ChartArea))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if series chart type uses a "Fast" mode chart type.
        /// </summary>
        /// <returns>True if series uses "Fast" mode chart type.</returns>
        internal bool IsFastChartType()
        {
            // Check if fast mode chart type is used in the series
            if (ChartType == SeriesChartType.FastLine)
            {
                return true;
            }

            // Check if fast mode chart type is used in the series
            if (ChartType == SeriesChartType.FastPoint)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throws exception if specified value type is not supported.
        /// </summary>
        /// <param name="type">Value type to check.</param>
        internal static void CheckSupportedTypes(Type type)
        {
            // Check parameters type
            if (type == typeof(Double) ||
                type == typeof(DateTime) ||
                type == typeof(String) ||
                type == typeof(Int32) ||
                type == typeof(UInt32) ||
                type == typeof(Decimal) ||
                type == typeof(Single) ||
                type == typeof(Int16) ||
                type == typeof(UInt16) ||
                type == typeof(Int64) ||
                type == typeof(UInt64) ||
                type == typeof(Byte) ||
                type == typeof(SByte) ||
                type == typeof(System.DBNull) ||
                type == typeof(Boolean))
            {
                return;
            }

            // Unsupported parameter type
            throw (new ArgumentException(SR.ExceptionDataSeriesPointTypeUnsupported(type.ToString())));
        }

        /// <summary>
        /// Apply palette colors to the data series points if UsePaletteColors property is set.
        /// </summary>
        internal void ApplyPaletteColors()
        {
            // Use Series or Data Manager palette
            DataManager dataManager = (DataManager)Common.DataManager;

            ChartColorPalette currentPalette = (Palette == ChartColorPalette.None) ?
                                                dataManager.Palette : Palette;

            // if it is still none - check if custom colors pallete is empty.
            if (
                currentPalette == ChartColorPalette.None &&
                dataManager.PaletteCustomColors.Length == 0
                )
            {
                currentPalette = ChartColorPalette.BrightPastel;
            }

            // Get palette colors
            int colorIndex = 0;
            SKColor[] paletteColors = (currentPalette == ChartColorPalette.None) ?
                dataManager.PaletteCustomColors : ChartPaletteColors.GetPaletteColors(currentPalette);
            foreach (DataPoint dataPoint in _points)
            {
                // Change color of the series data points only if no color is set
                if ((!dataPoint.IsCustomPropertySet(CommonCustomProperties.Color) || dataPoint.tempColorIsSet) && !dataPoint.IsEmpty)
                {
                    dataPoint.SetAttributeObject(CommonCustomProperties.Color, paletteColors[colorIndex]);
                    dataPoint.tempColorIsSet = true;
                    ++colorIndex;
                    if (colorIndex >= paletteColors.Length)
                    {
                        colorIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Gets design time dummy data.
        /// </summary>
        /// <param name="type">AxisName of the data to get.</param>
        /// <returns>Dummy data for chart in design-time.</returns>
        internal IEnumerable GetDummyData(ChartValueType type)
        {
            string[] stringValues = { "abc1", "abc2", "abc3", "abc4", "abc5", "abc6" };
            DateTime[] dateValues = { DateTime.Now.Date, DateTime.Now.Date.AddDays(1), DateTime.Now.Date.AddDays(2), DateTime.Now.Date.AddDays(3), DateTime.Now.Date.AddDays(4), DateTime.Now.Date.AddDays(4) };

            // Fill array of random data
            if (_dummyDoubleValues == null)
            {
                //TODO: Check, what is needed from here...
                //#if !SQLRS_CONTROL
                //				Random random2 = new Random(unchecked((int)DateTime.Now.Ticks +
                //					this.Color.B + this.Color.G + this.Color.R));
                //#else
                int seed = 0;
                for (int index = 0; index < Name.Length; index++)
                    seed += (int)Name[index];

                Random random2 = new(seed);

                //#endif
                _dummyDoubleValues = new double[6];
                for (int valueIndex = 0; valueIndex < 6; valueIndex++)
                {
                    _dummyDoubleValues[valueIndex] = random2.Next(10, 100);
                }
            }

            // Return dummy data
            if (type == ChartValueType.DateTime || type == ChartValueType.Date || type == ChartValueType.DateTimeOffset)
            {
                return dateValues;
            }
            else if (type == ChartValueType.Time)
            {
                dateValues = new DateTime[] { DateTime.Now, DateTime.Now.AddMinutes(1), DateTime.Now.AddMinutes(2), DateTime.Now.AddMinutes(3), DateTime.Now.AddMinutes(4), DateTime.Now.AddMinutes(4) };
                return dateValues;
            }
            else if (type == ChartValueType.String)
            {
                return stringValues;
            }
            return _dummyDoubleValues;
        }

        /// <summary>
        /// Returns total of the Y values.
        /// </summary>
        /// <returns>Y values total.</returns>
        internal double GetTotalYValue()
        {
            return GetTotalYValue(0);
        }

        /// <summary>
        /// Returns total of the Y values.
        /// </summary>
        /// <param name="yValueIndex">Index of the Y value to use</param>
        /// <returns>Y values total.</returns>
        internal double GetTotalYValue(int yValueIndex)
        {
            if (yValueIndex == 0)
            {
                // Total was already calculated
                if (!double.IsNaN(_totalYvalue))
                {
                    return _totalYvalue;
                }

                // Calculate total
                _totalYvalue = 0;
                foreach (DataPoint point in Points)
                {
                    _totalYvalue += point.YValues[yValueIndex];
                }

                return _totalYvalue;
            }

            // Check if series has enough Y values
            if (yValueIndex >= YValuesPerPoint)
            {
                throw (new InvalidOperationException(SR.ExceptionDataSeriesYValueIndexNotExists(yValueIndex.ToString(CultureInfo.InvariantCulture), Name)));
            }

            // Calculate total
            double yValue = 0;
            foreach (DataPoint point in Points)
            {
                yValue += point.YValues[yValueIndex];
            }

            return yValue;
        }

        /// <summary>
        /// Replaces predefined keyword inside the string with their values.
        /// </summary>
        /// <param name="strOriginal">Original string with keywords.</param>
        /// <returns>Modified string.</returns>
        internal override string ReplaceKeywords(string strOriginal)
        {
            // Nothing to process
            if (strOriginal == null || strOriginal.Length == 0)
                return strOriginal;

            // Replace all "\n" strings with '\n' character
            string result = strOriginal.Replace("\\n", "\n");

            // #SERIESNAME - series name
            result = result.Replace(KeywordName.SeriesName, Name);
            result = result.Replace(KeywordName.Ser, Name);			// #SER Depricated Keyword

            // #CUSTOMPROPERTY - one of the custom attributes by name
            result = DataPoint.ReplaceCustomPropertyKeyword(result, this);

            // #TOTAL - total of Y values
            result = ReplaceOneKeyword(
                Chart,
                this,
                Tag,
                ChartElementType.Nothing,
                result,
                KeywordName.Total,
                SeriesValuesFormulaType.Total,
                YValueType,
                "");

            // #AVG - total of Y values
            result = ReplaceOneKeyword(
                Chart,
                this,
                Tag,
                ChartElementType.Nothing,
                result,
                KeywordName.Avg,
                SeriesValuesFormulaType.Average,
                YValueType,
                "");

            // #MAX - total of Y values
            result = ReplaceOneKeyword(
                Chart,
                this,
                Tag,
                ChartElementType.Nothing,
                result,
                KeywordName.Max,
                SeriesValuesFormulaType.Maximum,
                YValueType,
                "");

            // #MIN - total of Y values
            result = ReplaceOneKeyword(
                Chart,
                this,
                Tag,
                ChartElementType.Nothing,
                result,
                KeywordName.Min,
                SeriesValuesFormulaType.Minimum,
                YValueType,
                "");

            // #FIRST - total of Y values
            result = ReplaceOneKeyword(
                Chart,
                this,
                Tag,
                ChartElementType.Nothing,
                result,
                KeywordName.First,
                SeriesValuesFormulaType.First,
                YValueType,
                "");

            // #LAST - total of Y values
            result = ReplaceOneKeyword(
                Chart,
                this,
                Tag,
                ChartElementType.Nothing,
                result,
                KeywordName.Last,
                SeriesValuesFormulaType.Last,
                YValueType,
                "");

            // #LEGENDTEXT - series name
            result = result.Replace(KeywordName.LegendText, LegendText);

            return result;
        }

        /// <summary>
        /// Helper function which replaces one keyword.
        /// </summary>
        /// <param name="chart">Chart object reference.</param>
        /// <param name="elementType">Chart element type.</param>
        /// <param name="obj">Object being formatted.</param>
        /// <param name="objTag">Additional object tag.</param>
        /// <param name="strOriginal">Original string.</param>
        /// <param name="keyword">Keyword to replace.</param>
        /// <param name="formulaType">Formula used to calculate the value.</param>
        /// <param name="valueType">AxisName of value.</param>
        /// <param name="defaultFormat">Default format string.</param>
        /// <returns>Result string.</returns>
		internal string ReplaceOneKeyword(
            ChartService chart,
            object obj,
            object objTag,
            ChartElementType elementType,
            string strOriginal,
            string keyword,
            SeriesValuesFormulaType formulaType,
            ChartValueType valueType,
            string defaultFormat)
        {
            string result = strOriginal;
            int keyIndex = -1;
            while ((keyIndex = result.IndexOf(keyword, StringComparison.Ordinal)) != -1)
            {
                int keyEndIndex = keyIndex + keyword.Length;

                // Get optional Y value index
                int yValueIndex = 0;
                if (result.Length > keyEndIndex + 1 &&
                    result[keyEndIndex] == 'Y' &&
                    char.IsDigit(result[keyEndIndex + 1]))
                {
                    yValueIndex = int.Parse(result.Substring(keyEndIndex + 1, 1), CultureInfo.InvariantCulture);
                    keyEndIndex += 2;
                }

                // Get optional format
                string format = defaultFormat;
                if (result.Length > keyEndIndex && result[keyEndIndex] == '{')
                {
                    int formatEnd = result.IndexOf('}', keyEndIndex);
                    if (formatEnd == -1)
                    {
                        throw (new InvalidOperationException(SR.ExceptionDataSeriesKeywordFormatInvalid(result)));
                    }

                    format = result[keyEndIndex..formatEnd].Trim('{', '}');
                    keyEndIndex = formatEnd + 1;
                }

                // Remove keyword string (with optional format)
                result = result.Remove(keyIndex, keyEndIndex - keyIndex);

                // Calculate value
                double totalValue = GetTotalYValue(yValueIndex);
                double keywordValue = 0.0;
                switch (formulaType)
                {
                    case (SeriesValuesFormulaType.Average):
                        {
                            if (Points.Count > 0)
                            {
                                keywordValue = totalValue / Points.Count;
                            }
                            break;
                        }
                    case (SeriesValuesFormulaType.First):
                        {
                            if (Points.Count > 0)
                            {
                                keywordValue = Points[0].YValues[yValueIndex];
                            }
                            break;
                        }
                    case (SeriesValuesFormulaType.Last):
                        {
                            if (Points.Count > 0)
                            {
                                keywordValue = Points[^1].YValues[yValueIndex];
                            }
                            break;
                        }
                    case (SeriesValuesFormulaType.Maximum):
                        {
                            if (Points.Count > 0)
                            {
                                keywordValue = double.MinValue;
                                foreach (DataPoint point in Points)
                                {
                                    keywordValue = Math.Max(keywordValue, point.YValues[yValueIndex]);
                                }
                            }
                            break;
                        }
                    case (SeriesValuesFormulaType.Minimum):
                        {
                            if (Points.Count > 0)
                            {
                                keywordValue = double.MaxValue;
                                foreach (DataPoint point in Points)
                                {
                                    keywordValue = Math.Min(keywordValue, point.YValues[yValueIndex]);
                                }
                            }
                            break;
                        }
                    case (SeriesValuesFormulaType.Total):
                        {
                            keywordValue = totalValue;
                            break;
                        }
                }

                // Insert value
                result = result.Insert(keyIndex,
                    ValueConverter.FormatValue(chart, obj, objTag, keywordValue, format, valueType, elementType));
            }

            return result;
        }

        /// <summary>
        /// Helper function which replaces one keyword.
        /// </summary>
        /// <param name="chart">Chart object reference.</param>
        /// <param name="elementType">Chart element type.</param>
        /// <param name="obj">Object being formatted.</param>
        /// <param name="objTag">Additional object tag.</param>
        /// <param name="strOriginal">Original string.</param>
        /// <param name="keyword">Keyword to replace.</param>
        /// <param name="value">Value to replace with.</param>
        /// <param name="valueType">AxisName of value.</param>
        /// <param name="defaultFormat">Default format string.</param>
        /// <returns>Result string.</returns>
		internal string ReplaceOneKeyword(ChartService chart, object obj, object objTag, ChartElementType elementType, string strOriginal, string keyword, double value, ChartValueType valueType, string defaultFormat)

        {
            string result = strOriginal;
            int keyIndex = -1;
            while ((keyIndex = result.IndexOf(keyword, StringComparison.Ordinal)) != -1)
            {
                // Get optional format
                int keyEndIndex = keyIndex + keyword.Length;
                string format = defaultFormat;
                if (result.Length > keyEndIndex && result[keyEndIndex] == '{')
                {
                    int formatEnd = result.IndexOf('}', keyEndIndex);
                    if (formatEnd == -1)
                    {
                        throw (new InvalidOperationException(SR.ExceptionDataSeriesKeywordFormatInvalid(result)));
                    }

                    format = result[keyEndIndex..formatEnd].Trim('{', '}');
                    keyEndIndex = formatEnd + 1;
                }

                // Remove keyword string (with optional format)
                result = result.Remove(keyIndex, keyEndIndex - keyIndex);

                // Insert value
                result = result.Insert(keyIndex,
                    ValueConverter.FormatValue(chart, obj, objTag, value, format, valueType, elementType));
            }

            return result;
        }

        #endregion Helper methods

        #region Points sorting methods

        /// <summary>
        /// Sorts the points in the series.
        /// </summary>
        /// <param name="pointSortOrder">Sorting order.</param>
        /// <param name="sortBy">Value used for sorting (X, Y, Y2, ...).</param>
        public void Sort(PointSortOrder pointSortOrder, string sortBy)
        {
            // Check arguments
            if (sortBy == null)
                throw new ArgumentNullException(nameof(sortBy));

            // Sort items using data points comparer class
            DataPointComparer comparer = new(this, pointSortOrder, sortBy);
            Points.ItemList.Sort(comparer);

            // Invalidate chart area only
            Invalidate(true, false);
        }

        /// <summary>
        /// Sorts the points in the series.
        /// </summary>
        /// <param name="pointSortOrder">Sorting order.</param>
        public void Sort(PointSortOrder pointSortOrder)
        {
            Sort(pointSortOrder, "Y");
        }

        /// <summary>
        /// Sorts the points in the series using IComparer interface.
        /// </summary>
        /// <param name="comparer">IComparer interface.</param>
        public void Sort(IComparer<DataPoint> comparer)
        {
            // Check arguments
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            // Sort points
            Points.ItemList.Sort(comparer);

            // Invalidate chart area only
            Invalidate(true, false);
        }

        #endregion Points sorting methods

        #region Series preparation/cleanup for drawing

        /// <summary>
        /// Moves the position markers.
        /// </summary>
        /// <param name="fromSeries">From series.</param>
        /// <param name="toSeries">To series.</param>
        internal static void MovePositionMarkers(Series fromSeries, Series toSeries)
        {
            foreach (DataPoint dp in fromSeries.Points)
            {
                if (dp.IsCustomPropertySet("OriginalPointIndex") 
                    && int.TryParse(dp["OriginalPointIndex"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                    && index > -1 && index < toSeries.Points.Count)
                {
                    toSeries.Points[index].positionRel = dp.positionRel;
                }
            }
        }

        /// <summary>
        /// Called after the series was drawn.
        /// </summary>
        /// <param name="controlSite">Site interface of the control.</param>
        /// <returns>True if series was removed from collection.</returns>
        internal bool UnPrepareData()
        {
            bool result = false;

            // Process Renko chart type data calculations
            if (RenkoChart.UnPrepareData(this))
            {
                result = true;
            }

            // Process ThreeLineBreak chart type data calculations
            if (ThreeLineBreakChart.UnPrepareData(this))
            {
                result = true;
            }

            // Process Kagi chart type data calculations
            if (KagiChart.UnPrepareData(this))
            {
                result = true;
            }

            // Process PointAndFigure chart type data calculations
            if (PointAndFigureChart.UnPrepareData(this))
            {
                result = true;
            }

            // Undo all changes done for the collected slice support
            if (PieChart.UnPrepareData(this))
            {
                result = true;
            }

            // Reset original value type which was temp. set to String
            if (_isXValueIndexed)
            {
                _xValueType = indexedXValueType;
            }

            // Reset auro values only at design time
            bool reset = false;

            ResetAutoValues(reset);

            return result;
        }

        /// <summary>
        /// Reset auto calculated series values.
        /// </summary>
        internal void ResetAutoValues()
        {
            ResetAutoValues(true);
        }

        /// <summary>
        /// Reset auto calculated series values.
        /// </summary>
        /// <param name="reset">Indicates that value types should be reset.</param>
        internal void ResetAutoValues(bool reset)
        {
            // If temporary data attribute is set - remove all data points
            if (IsCustomPropertySet("TempDesignData"))
            {
                DeleteCustomProperty("TempDesignData");

                // save the fake DataPoints for selector service
                bool savePoints = true;
                if (Chart != null)
                {
                    savePoints = false;
                }
                if (savePoints)
                {
                    fakeDataPoints.Clear();
                    foreach (DataPoint p in Points)
                    {
                        fakeDataPoints.Add(p);
                    }
                }

                Points.Clear();
            }

            // Reset series color
            if (tempColorIsSet)
            {
                tempColorIsSet = false;
                Color = SKColor.Empty;
            }

            // Reset series marker
            if (tempMarkerStyleIsSet)
            {
                tempMarkerStyleIsSet = false;
                MarkerStyle = MarkerStyle.None;
            }

            // Reset points color
            foreach (DataPoint dataPoint in _points)
            {
                if (dataPoint.tempColorIsSet)
                {
                    dataPoint.Color = SKColor.Empty;
                }
            }

            // Reset value type to Auto (if not Serializing data)
            if (reset && (Chart == null || !Chart.serializing))
            {
                if (autoXValueType)
                {
                    _xValueType = ChartValueType.Auto;
                    autoXValueType = false;
                }
                if (autoYValueType)
                {
                    _yValueType = ChartValueType.Auto;
                    autoYValueType = false;
                }
            }
        }

        /// <summary>
        /// Called just before the data from the series to be used to perform these operations:
        /// - apply palette colors to the data points
        /// - fill empty data points
        /// - provide fake data in design mode
        /// - retrieving data from the DataSource
        /// </summary>
        /// <param name="applyPaletteColors">If true each data point will be assigned a color from the palette (if it's set)</param>
		internal void PrepareData(bool applyPaletteColors)
        {
            if (!IsVisible())
            {
                return;
            }

            // Series chart area name can be empty or a valid area name
            Chart.ChartAreas.VerifyNameReference(ChartArea);

            // Check if sereis data points have required number of Y values
            if (Points.Count > 0 && Points[0].YValues.Length < YValuesPerPoint)
            {
                // Resize data points Y value(s) arrays
                foreach (DataPoint dp in Points)
                {
                    dp.ResizeYValueArray(YValuesPerPoint);
                }
            }

            // Get series data source
            bool fillTempData = false;
            if (Points.Count == 0 && IsCustomPropertySet("UseDummyData") && String.Compare(this["UseDummyData"], "True", StringComparison.OrdinalIgnoreCase) == 0)
            {
                fillTempData = true;
            }

            // Create dummy data only if there was no points
            if (fillTempData)
            {
                if (IsXValueDateTime() || _xValueType == ChartValueType.String)
                {
                    Points.DataBindXY(GetDummyData(_xValueType), GetDummyData(_yValueType));
                }
                else
                {
                    double[] xValues = new double[] { 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };
                    if (ChartType == SeriesChartType.Polar)
                    {
                        xValues = new double[] { 0.0, 45.0, 115.0, 145.0, 180.0, 220.0 };
                    }
                    Points.DataBindXY(xValues, GetDummyData(_yValueType));
                }

                // If point has more than one Y value - copy the data from first value
                if (YValuesPerPoint > 1)
                {
                    foreach (DataPoint point in Points)
                    {
                        for (int valueIndex = 1; valueIndex < YValuesPerPoint; valueIndex++)
                        {
                            point.YValues[valueIndex] = point.YValues[0];
                        }

                        if (YValuesPerPoint >= 2)
                        {
                            point.YValues[1] = point.YValues[0] / 2 - 1;
                        }

                        if (YValuesPerPoint >= 4)
                        {
                            point.YValues[2] = point.YValues[1] + (point.YValues[0] - point.YValues[1]) / 3;
                            point.YValues[3] = point.YValues[2] + (point.YValues[0] - point.YValues[1]) / 3;
                        }

                        if (YValuesPerPoint >= 6)
                        {
                            point.YValues[4] = point.YValues[2] + (point.YValues[3] - point.YValues[2]) / 2;
                            point.YValues[5] = point.YValues[2] + (point.YValues[3] - point.YValues[2]) / 3;
                        }
                    }
                }

                // Set data series attribute that data is temporary
                this["TempDesignData"] = "true";
            }

            // If value type was not Auto detected - set it to double
            if (_xValueType == ChartValueType.Auto)
            {
                _xValueType = ChartValueType.Double;
                autoXValueType = true;
            }
            if (_yValueType == ChartValueType.Auto)
            {
                _yValueType = ChartValueType.Double;
                autoYValueType = true;
            }

            // Use data point index as X value
            indexedXValueType = _xValueType;

            // Reset total Y value
            _totalYvalue = double.NaN;

            // Supress zero and negative values with logarithmic axis exceptions
            if (Chart != null && Chart.chartPicture.SuppressExceptions)
            {
                // Get series axis
                Axis axisY = Chart.ChartAreas[ChartArea].GetAxis(AxisName.Y, YAxisType, YSubAxisName);

                foreach (DataPoint point in Points)
                {
                    for (int yValueIndex = 0; yValueIndex < point.YValues.Length; yValueIndex++)
                    {
                        if (axisY.IsLogarithmic && point.YValues[yValueIndex] <= 0.0)
                        {
                            point.YValues[yValueIndex] = 1.0;
                            point.IsEmpty = true;
                        }

                        // Check All Y values for NaN
                        if (double.IsNaN(point.YValues[yValueIndex]))
                        {
                            point.YValues[yValueIndex] = 0.0;
                            point.IsEmpty = true;
                        }
                    }
                }
            }

            // Process Error Bar chart type data linking and calculations
            ErrorBarChart.GetDataFromLinkedSeries(this);
            ErrorBarChart.CalculateErrorAmount(this);

            // Process Box chart type data calculations
            BoxPlotChart.CalculateBoxPlotFromLinkedSeries(this);

            // Process Renko chart type data calculations
            RenkoChart.PrepareData(this);

            // Process ThreeLineBreak chart type data calculations
            ThreeLineBreakChart.PrepareData(this);

            // Process Kagi chart type data calculations
            KagiChart.PrepareData(this);

            // Process PointAndFigure chart type data calculations
            PointAndFigureChart.PrepareData(this);

            // Check if Collected slice should be displayed in Pie/Doughnut charts
            PieChart.PrepareData(this);

            // Apply palette colors to the data points
            if (applyPaletteColors)
            {
                ApplyPaletteColors();
            }
        }

        #endregion Series preparation/cleanup for drawing

        #region Series Properties

        /// <summary>
        /// Data series name.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),

        SRDescription("DescriptionAttributeSeries_Name"),
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
        /// Member of the chart data source used to data bind to the X value of the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeDataSource"),
        SRDescription("DescriptionAttributeSeries_ValueMemberX"),
        ]
        public string XValueMember
        {
            get
            {
                return _dataSourceXMember;
            }
            set
            {
                if (value == "(none)")
                {
                    _dataSourceXMember = String.Empty;
                }
                else
                {
                    _dataSourceXMember = value;
                }

                // Reset data bound flag
                if (Common != null && Common.ChartPicture != null)
                {
                    Common.ChartPicture.boundToDataSource = false;
                }
            }
        }

        /// <summary>
        /// Members of the chart data source used to data bind to the Y values of the series.
        /// </summary>
        [

        SRCategory("CategoryAttributeDataSource"),
        SRDescription("DescriptionAttributeSeries_ValueMembersY"),
        ]
        public string YValueMembers
        {
            get
            {
                return _dataSourceYMembers;
            }
            set
            {
                if (value == "(none)")
                {
                    _dataSourceYMembers = String.Empty;
                }
                else
                {
                    _dataSourceYMembers = value;
                }

                // Reset data bound flag
                if (Common != null && Common.ChartPicture != null)
                {
                    Common.ChartPicture.boundToDataSource = false;
                }
            }
        }

        /// <summary>
        /// Name of the Chart legend used by the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeLegend"),
        SRDescription("DescriptionAttributeSeries_Legend"),
        ]
        public string Legend
        {
            get
            {
                return _legend;
            }
            set
            {
                if (value != _legend)
                {
                    if (Chart != null && Chart.Legends != null)
                    {
                        Chart.Legends.VerifyNameReference(value);
                    }
                    _legend = value;
                    Invalidate(false, true);
                }
            }
        }

        /// <summary>
        /// The value type of the X axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeSeries_XValueType"),
        ]
        public ChartValueType XValueType
        {
            get
            {
                return _xValueType;
            }
            set
            {
                _xValueType = value;
                autoXValueType = false;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Indicates whether a data point index (1,2,...) will be used for the X value.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeSeries_XValueIndexed"),
        ]
        public bool IsXValueIndexed
        {
            get
            {
                return _isXValueIndexed;
            }
            set
            {
                _isXValueIndexed = value;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// The value type of the Y axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeSeries_YValueType"),
        ]
        public ChartValueType YValueType
        {
            get
            {
                return _yValueType;
            }
            set
            {
                _yValueType = value;
                autoYValueType = false;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Number of Y values stored for each Data Point.
        /// </summary>
        [

        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeSeries_YValuesPerPoint"),
        ]
        public int YValuesPerPoint
        {
            get
            {
                // If number of Y value(s) is not set - get one from the chart type
                if (_checkPointsNumber && ChartTypeName.Length > 0 && Common != null)
                {
                    _checkPointsNumber = false;
                    ChartTypeRegistry chartTypeRegistry = Common.ChartTypeRegistry;
                    IChartType chartType = chartTypeRegistry.GetChartType(ChartTypeName);
                    if (chartType.YValuesPerPoint > _yValuesPerPoint)
                    {
                        _yValuesPerPoint = chartType.YValuesPerPoint;

                        // Resize Y value(s) array of data points
                        if (_points.Count > 0)
                        {
                            // Resize data points Y value(s) arrays
                            foreach (DataPoint dp in _points)
                            {
                                dp.ResizeYValueArray(_yValuesPerPoint);
                            }
                        }
                    }
                }

                return _yValuesPerPoint;
            }
            set
            {
                // Check if argument is in range
                if (value < 1 || value > 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionDataSeriesYValueNumberInvalid);
                }

                _checkPointsNumber = true;

                // Resize Y value(s) array of data points
                if (_points.Count > 0)
                {
                    // Resize data points Y value(s) arrays
                    foreach (DataPoint dp in _points)
                    {
                        dp.ResizeYValueArray(value);
                    }
                }

                _yValuesPerPoint = value;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Collection of data points in the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeSeries_Points"),
        ]
        public DataPointCollection Points
        {
            get
            {
                return _points;
            }
        }

        /// <summary>
        /// Default properties of an empty data point.
        /// </summary>
        [
        SRCategory("CategoryAttributeEmptyPoints"),
        SRDescription("DescriptionAttributeSeries_EmptyPointStyle"),
        ]
        public DataPointCustomProperties EmptyPointStyle
        {
            get
            {
                return _emptyPointCustomProperties;
            }
            set
            {
                if (value.series == null && _emptyPointCustomProperties.series != null)
                {
                    value.series = _emptyPointCustomProperties.series;
                }
                _emptyPointCustomProperties = value;
                _emptyPointCustomProperties.pointCustomProperties = false;
                _emptyPointCustomProperties.SetDefault(false);
                _emptyPointCustomProperties.pointCustomProperties = true;
                _emptyPointCustomProperties.Parent = this;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Color palette to use.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributePalette"),
        ]
        public ChartColorPalette Palette
        {
            get
            {
                return _colorPalette;
            }
            set
            {
                _colorPalette = value;
                Invalidate(true, true);
            }
        }

        /// <summary>
        /// Specify how often to display data point markers.
        /// </summary>
		[
        SRCategory("CategoryAttributeMarker"),
        SRDescription("DescriptionAttributeSeries_MarkerStep"),
        ]
        public int MarkerStep
        {
            get
            {
                return _markersStep;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(SR.ExceptionMarkerStepNegativeValue, nameof(value));
                }
                _markersStep = value;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Shadow offset of series.
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
                Invalidate(true, true);
            }
        }

        /// <summary>
        /// Shadow color of series.
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
                Invalidate(true, true);
            }
        }

#if SUBAXES

			/// <summary>
			/// Name of the Y sub-axis this series is attached to.
			/// </summary>
			[
			SRCategory("CategoryAttributeAxes"),

			SRDescription("DescriptionAttributeSeries_YSubAxisName"),
			DefaultValue("")
			]
			public string YSubAxisName
			{
				get
				{
					return this._ySubAxisName;
				}
				set
				{
					this._ySubAxisName = value;
					this.Invalidate(true, false);
				}
			}

			/// <summary>
			/// Name of the X sub-axis this series is attached to.
			/// </summary>
			[

			SRCategory("CategoryAttributeAxes"),

			SRDescription("DescriptionAttributeSeries_XSubAxisName"),
			DefaultValue("")
			]
			public string XSubAxisName
			{
				get
				{
					return this._xSubAxisName;
				}
				set
				{
					this._xSubAxisName = value;
					this.Invalidate(true, false);
				}
			}

#else // SUBAXES

        /// <summary>
        /// Name of the Y sub-axis this series is attached to.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxes"),
        SRDescription("DescriptionAttributeSeries_YSubAxisName")
        ]
        internal string YSubAxisName
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Name of the X sub-axis this series is attached to.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxes"),
        SRDescription("DescriptionAttributeSeries_XSubAxisName"),
        ]
        internal string XSubAxisName
        {
            get
            {
                return string.Empty;
            }
        }

#endif // SUBAXES

        /// <summary>
		/// Axis type of horizontal axes.
		/// </summary>
		[
        SRCategory("CategoryAttributeAxes"),
        SRDescription("DescriptionAttributeSeries_XAxisType"),
        ]
        public AxisType XAxisType
        {
            get
            {
                return _xAxisType;
            }
            set
            {
                _xAxisType = value;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Axis type of vertical axes.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxes"),
        SRDescription("DescriptionAttributeSeries_YAxisType"),
        ]
        public AxisType YAxisType
        {
            get
            {
                return _yAxisType;
            }
            set
            {
                _yAxisType = value;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the series is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeSeries_Enabled"),
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
                Invalidate(true, true);
            }
        }

        /// <summary>
        /// Chart type used to draw the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeSeries_Type"),
        ]
        public SeriesChartType ChartType
        {
            get
            {
                SeriesChartType type = SeriesChartType.Column;
                if (String.Compare(ChartTypeName, ChartTypeNames.OneHundredPercentStackedArea, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    type = SeriesChartType.StackedArea100;
                }
                else if (String.Compare(ChartTypeName, ChartTypeNames.OneHundredPercentStackedBar, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    type = SeriesChartType.StackedBar100;
                }
                else if (String.Compare(ChartTypeName, ChartTypeNames.OneHundredPercentStackedColumn, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    type = SeriesChartType.StackedColumn100;
                }
                else
                {
                    try
                    {
                        type = (SeriesChartType)Enum.Parse(typeof(SeriesChartType), ChartTypeName, true);
                    }
                    catch (ArgumentException)
                    {
                        // Ignore
                    }
                }

                return type;
            }
            set
            {
                ChartTypeName = Series.GetChartTypeName(value);
            }
        }

        /// <summary>
        /// Chart type used to draw the series.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeSeries_Type"),
        ]
        public string ChartTypeName
        {
            get
            {
                return _chartType;
            }
            set
            {
                if (_chartType != value && value.Length > 0 && Common != null)
                {
                    ChartTypeRegistry chartTypeRegistry = Common.ChartTypeRegistry;
                    if (chartTypeRegistry != null)
                    {
                        IChartType type = chartTypeRegistry.GetChartType(value);
                        if (_yValuesPerPoint < type.YValuesPerPoint)
                        {
                            // Set minimum Y values number for the chart type
                            _yValuesPerPoint = type.YValuesPerPoint;

                            // Resize Y value(s) array of data points
                            if (_points.Count > 0)
                            {
                                // Resize data points Y value(s) arrays
                                foreach (DataPoint dp in _points)
                                {
                                    dp.ResizeYValueArray(_yValuesPerPoint);
                                }
                            }
                        }
                        // Refresh Minimum and Maximum from data
                        // after recalc and set data
                        if (Chart != null && Chart.chartPicture != null)
                        {
                            Chart.chartPicture.ResetMinMaxFromData();
                        }
                    }
                }

                _chartType = value;

                Invalidate(false, true);
            }
        }

        /// <summary>
        /// Chart area in which this series is drawn.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeSeries_ChartArea"),
        ]
        public string ChartArea
        {
            get
            {
                return _chartArea;
            }
            set
            {
                if (value != _chartArea)
                {
                    if (Chart != null && Chart.ChartAreas != null)
                    {
                        Chart.ChartAreas.VerifyNameReference(value);
                    }
                    _chartArea = value;
                    Invalidate(false, true);
                }
            }
        }

        /*
                /// <summary>
                /// If set to true, each data point of the series will use a random color from the palette.
                /// </summary>
                [
                SRCategory("CategoryAttributeChart"),

                SRDescription("DescriptionAttributeDataSeriesGroupID"),
                PersistenceModeAttribute(PersistenceMode.Attribute),
                DefaultValue("")
                ]
                public string GroupID
                {
                    get
                    {
                        return groupID;
                    }
                    set
                    {
                        groupID = value;
                    }
                }
        */

        /// <summary>
        /// Text of X axis label.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisLabel"),
        ]
        override public string AxisLabel
        {
            get
            {
                return base.AxisLabel;
            }
            set
            {
                base.AxisLabel = value;
                Invalidate(true, false);
            }
        }

        /// <summary>
        /// Style of the SmartLabel.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabel"),
        SRDescription("DescriptionAttributeSeries_SmartLabels"),
        ]
        public SmartLabelStyle SmartLabelStyle
        {
            get
            {
                return _smartLabelStyle;
            }
            set
            {
                value.chartElement = this;
                _smartLabelStyle = value;
                Invalidate(false, false);
            }
        }

        /// <summary>
        /// Series font cache is reused by points.
        /// </summary>
        /// <value>The font cache.</value>
        internal FontCache FontCache
        {
            get { return _fontCache; }
        }

        #endregion Series Properties

        #region Invalidating method

        /// <summary>
        /// Invalidate chart or just a chart area and/or legend when collection is changed
        /// </summary>
        /// <param name="invalidateAreaOnly">Invalidate chart area only.</param>
        /// <param name="invalidateLegend">Invalidate legend area only.</param>
         internal void Invalidate(bool invalidateAreaOnly, bool invalidateLegend)
        {
            if (Chart != null)
            {
                if (!invalidateAreaOnly)
                {
                    Invalidate();
                }
                else
                {
                    // Invalidate one chart area (area with this name may not exist)
                    try
                    {
                        Chart.ChartAreas[ChartArea].Invalidate();
                    }
                    catch (ArgumentException)
                    {
                        // occurs if the chart area is not found in the collection
                    }

                    // Invalidate legend
                    if (invalidateLegend && Chart.Legends.IndexOf(Legend) >= 0)
                    {
                        Chart.Legends[Legend].Invalidate(true);
                    }
                }
            }
        }

        #endregion Invalidating method

        #region Series Enumeration

        /// <summary>
        /// Series values formula type used in the keywords
        /// </summary>
        internal enum SeriesValuesFormulaType
        {
            Total,
            Average,
            Maximum,
            Minimum,
            First,
            Last
        }

        #endregion Series Enumeration

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
                if (_emptyPointCustomProperties != null)
                {
                    _emptyPointCustomProperties.Dispose();
                    _emptyPointCustomProperties = null;
                }
                if (_points != null)
                {
                    _points.Dispose();
                    _points = null;
                }
                if (fakeDataPoints != null)
                {
                    fakeDataPoints.Dispose();
                    fakeDataPoints = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}