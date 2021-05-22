// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Main windows forms chart control class.
//

using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace WebCharts.Services
{
    /// <summary>
    /// Chart windows forms control
    /// </summary>
    [SRDescription("DescriptionAttributeChart_Chart")]
    public class ChartService : IDisposable, IChartService
    {
        #region Control fields

        // Indicates that control invalidation is temporary disabled
        internal bool disableInvalidates = false;

        // Indicates that chart is serializing the data
        internal bool serializing = false;

        // Indicates that only chart area cursor/selection must be drawn during the next paint event
        internal bool paintTopLevelElementOnly = false;

        // Indicates that some chart properties where changed (used for painting)
        internal bool dirtyFlag = true;

        internal readonly ChartImage chartPicture;

        /// <summary>
        /// Determines whether or not to show debug markings in debug mode. For internal use.
        /// </summary>
        internal bool ShowDebugMarkings = false;

        // Chart services components
        private readonly ChartTypeRegistry _chartTypeRegistry;

        private readonly ImageLoader _imageLoader = null;

        // Named images collection
        private readonly NamedImagesCollection _namedImages = null;

        private readonly DataManager _dataManager;

        #endregion Control fields

        #region Control constructors

        private readonly IServiceProvider _provider;

        /// <summary>
        /// Chart control constructor.
        /// </summary>
        public ChartService(IServiceProvider provider, int width, int height)
        {
            CommonElements = new CommonElements(this);

            _provider = provider;
            chartPicture = new ChartImage(this) { Width = width, Height = height };

            _dataManager = chartPicture.Common.DataManager;
            _dataManager.Initialize(chartPicture);

            // Register known chart types
            _chartTypeRegistry = chartPicture.Common.ChartTypeRegistry;
            _chartTypeRegistry.Register(ChartTypeNames.Bar, typeof(BarChart));
            _chartTypeRegistry.Register(ChartTypeNames.Column, typeof(ColumnChart));
            _chartTypeRegistry.Register(ChartTypeNames.Point, typeof(PointChart));
            _chartTypeRegistry.Register(ChartTypeNames.Bubble, typeof(BubbleChart));
            _chartTypeRegistry.Register(ChartTypeNames.Line, typeof(LineChart));
            _chartTypeRegistry.Register(ChartTypeNames.Spline, typeof(SplineChart));
            _chartTypeRegistry.Register(ChartTypeNames.StepLine, typeof(StepLineChart));
            _chartTypeRegistry.Register(ChartTypeNames.Area, typeof(AreaChart));
            _chartTypeRegistry.Register(ChartTypeNames.SplineArea, typeof(SplineAreaChart));
            _chartTypeRegistry.Register(ChartTypeNames.StackedArea, typeof(StackedAreaChart));
            _chartTypeRegistry.Register(ChartTypeNames.Pie, typeof(PieChart));
            _chartTypeRegistry.Register(ChartTypeNames.Stock, typeof(StockChart));
            _chartTypeRegistry.Register(ChartTypeNames.Candlestick, typeof(CandleStickChart));
            _chartTypeRegistry.Register(ChartTypeNames.Doughnut, typeof(DoughnutChart));
            _chartTypeRegistry.Register(ChartTypeNames.StackedBar, typeof(StackedBarChart));
            _chartTypeRegistry.Register(ChartTypeNames.StackedColumn, typeof(StackedColumnChart));
            _chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedColumn, typeof(HundredPercentStackedColumnChart));
            _chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedBar, typeof(HundredPercentStackedBarChart));
            _chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedArea, typeof(HundredPercentStackedAreaChart));

            _chartTypeRegistry.Register(ChartTypeNames.Range, typeof(RangeChart));
            _chartTypeRegistry.Register(ChartTypeNames.SplineRange, typeof(SplineRangeChart));
            _chartTypeRegistry.Register(ChartTypeNames.RangeBar, typeof(RangeBarChart));
            _chartTypeRegistry.Register(ChartTypeNames.Radar, typeof(RadarChart));
            _chartTypeRegistry.Register(ChartTypeNames.RangeColumn, typeof(RangeColumnChart));
            _chartTypeRegistry.Register(ChartTypeNames.ErrorBar, typeof(ErrorBarChart));
            _chartTypeRegistry.Register(ChartTypeNames.BoxPlot, typeof(BoxPlotChart));

            _chartTypeRegistry.Register(ChartTypeNames.Renko, typeof(RenkoChart));
            _chartTypeRegistry.Register(ChartTypeNames.ThreeLineBreak, typeof(ThreeLineBreakChart));
            _chartTypeRegistry.Register(ChartTypeNames.Kagi, typeof(KagiChart));
            _chartTypeRegistry.Register(ChartTypeNames.PointAndFigure, typeof(PointAndFigureChart));

            _chartTypeRegistry.Register(ChartTypeNames.Polar, typeof(PolarChart));
            _chartTypeRegistry.Register(ChartTypeNames.FastLine, typeof(FastLineChart));
            _chartTypeRegistry.Register(ChartTypeNames.Funnel, typeof(FunnelChart));
            _chartTypeRegistry.Register(ChartTypeNames.Pyramid, typeof(PyramidChart));

            _chartTypeRegistry.Register(ChartTypeNames.FastPoint, typeof(FastPointChart));

            // Register known formula modules
            FormulaRegistry formulaRegistry = chartPicture.Common.FormulaRegistry;
            formulaRegistry.Register(SR.FormulaNamePriceIndicators, typeof(PriceIndicators));
            formulaRegistry.Register(SR.FormulaNameGeneralTechnicalIndicators, typeof(GeneralTechnicalIndicators));
            formulaRegistry.Register(SR.FormulaNameTechnicalVolumeIndicators, typeof(VolumeIndicators));
            formulaRegistry.Register(SR.FormulaNameOscillator, typeof(Oscillators));
            formulaRegistry.Register(SR.FormulaNameGeneralFormulas, typeof(GeneralFormulas));
            formulaRegistry.Register(SR.FormulaNameTimeSeriesAndForecasting, typeof(TimeSeriesAndForecasting));
            formulaRegistry.Register(SR.FormulaNameStatisticalAnalysis, typeof(StatisticalAnalysis));

            // Register known 3D border types
            BorderTypeRegistry borderTypeRegistry = chartPicture.Common.BorderTypeRegistry;
            borderTypeRegistry.Register("Emboss", typeof(EmbossBorder));
            borderTypeRegistry.Register("Raised", typeof(RaisedBorder));
            borderTypeRegistry.Register("Sunken", typeof(SunkenBorder));
            borderTypeRegistry.Register("FrameThin1", typeof(FrameThin1Border));
            borderTypeRegistry.Register("FrameThin2", typeof(FrameThin2Border));
            borderTypeRegistry.Register("FrameThin3", typeof(FrameThin3Border));
            borderTypeRegistry.Register("FrameThin4", typeof(FrameThin4Border));
            borderTypeRegistry.Register("FrameThin5", typeof(FrameThin5Border));
            borderTypeRegistry.Register("FrameThin6", typeof(FrameThin6Border));
            borderTypeRegistry.Register("FrameTitle1", typeof(FrameTitle1Border));
            borderTypeRegistry.Register("FrameTitle2", typeof(FrameTitle2Border));
            borderTypeRegistry.Register("FrameTitle3", typeof(FrameTitle3Border));
            borderTypeRegistry.Register("FrameTitle4", typeof(FrameTitle4Border));
            borderTypeRegistry.Register("FrameTitle5", typeof(FrameTitle5Border));
            borderTypeRegistry.Register("FrameTitle6", typeof(FrameTitle6Border));
            borderTypeRegistry.Register("FrameTitle7", typeof(FrameTitle7Border));
            borderTypeRegistry.Register("FrameTitle8", typeof(FrameTitle8Border));

            // Create named images collection
            _namedImages = new NamedImagesCollection();

            // Hook up event handlers
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Series.ChartAreaNameReferenceChanged);
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Legends.ChartAreaNameReferenceChanged);
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Titles.ChartAreaNameReferenceChanged);
            //ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Annotations.ChartAreaNameReferenceChanged);
            ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(ChartAreas.ChartAreaNameReferenceChanged);
            Legends.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Series.LegendNameReferenceChanged);
        }

        public CommonElements CommonElements { get; set; }

        #endregion Control constructors

        #region Chart image saving methods

        /// <summary>
        /// Saves chart image into the file.
        /// </summary>
        /// <param name="imageFileName">Image file name</param>
        /// <param name="format">Image format.</param>
        public void SaveImage(string imageFileName, ChartImageFormat format)
        {
            // Check arguments
            if (imageFileName == null)
                throw new ArgumentNullException(nameof(imageFileName));

            // Create file stream for the specified file name
            FileStream fileStream = new FileStream(imageFileName, FileMode.Create);

            // Save into stream
            try
            {
                SaveImage(fileStream, format);
            }
            finally
            {
                // Close file stream
                fileStream.Close();
            }
        }

        /// <summary>
        /// Saves chart image into the stream.
        /// </summary>
        /// <param name="imageStream">Image stream.</param>
        /// <param name="format">Image format.</param>
        public void SaveImage(Stream imageStream, ChartImageFormat format)
        {
            // Check arguments
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            // Indicate that chart is saved into the image
            chartPicture.isSavingAsImage = true;

            // Get chart image
            var chartImage = chartPicture.GetImage();

            SKEncodedImageFormat standardImageFormat = SKEncodedImageFormat.Png;

            switch (format)
            {
                case ChartImageFormat.Bmp:
                    standardImageFormat = SKEncodedImageFormat.Bmp;
                    break;

                case ChartImageFormat.Gif:
                    standardImageFormat = SKEncodedImageFormat.Gif;
                    break;

                case ChartImageFormat.Jpeg:
                    standardImageFormat = SKEncodedImageFormat.Jpeg;
                    break;

                case ChartImageFormat.Png:
                    standardImageFormat = SKEncodedImageFormat.Png;
                    break;
            }

            var image = SKImage.FromBitmap(chartImage);
            image.Encode(standardImageFormat, 100).SaveTo(imageStream);

            // Dispose image
            chartImage.Dispose();
            image.Dispose();

            // Reset flag
            chartPicture.isSavingAsImage = false;
        }

        #endregion Chart image saving methods

        #region Control public properties

        /// <summary>
        /// Array of custom palette colors.
        /// </summary>
        /// <remarks>
        /// When this custom colors array is non-empty the <b>Palette</b> property is ignored.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChart_PaletteCustomColors"),
        ]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SKColor[] PaletteCustomColors
        {
            set
            {
                _dataManager.PaletteCustomColors = value;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
            get
            {
                return _dataManager.PaletteCustomColors;
            }
        }

        /// <summary>
        /// Method resets custom colors array. Internal use only.
        /// </summary>
        internal void ResetPaletteCustomColors()
        {
            PaletteCustomColors = Array.Empty<SKColor>();
        }

        /// <summary>
        /// Method resets custom colors array. Internal use only.
        /// </summary>
        internal bool ShouldSerializePaletteCustomColors()
        {
            if (PaletteCustomColors == null ||
                PaletteCustomColors.Length == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Indicates that non-critical chart exceptions will be suppressed.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeSuppressExceptions"),
        ]
        public bool SuppressExceptions
        {
            set
            {
                chartPicture.SuppressExceptions = value;
            }
            get
            {
                return chartPicture.SuppressExceptions;
            }
        }

        /// <summary>
        /// "The data source used to populate series data. Series ValueMember properties must be also set."
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeDataSource"),
        ]
        public object DataSource
        {
            get
            {
                return chartPicture.DataSource;
            }
            set
            {
                chartPicture.DataSource = value;
            }
        }

        /// <summary>
        /// Chart named images collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeChart_Images")
        ]
        public NamedImagesCollection Images
        {
            get
            {
                return _namedImages;
            }
        }

        /// <summary>
        /// Chart series collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeChart_Series"),
        ]
        public SeriesCollection Series
        {
            get
            {
                return _dataManager.Series;
            }
        }

        /// <summary>
        /// Chart legend collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeLegends"),
        ]
        public LegendCollection Legends
        {
            get
            {
                return chartPicture.Legends;
            }
        }

        /// <summary>
        /// Chart title collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeTitles"),
        ]
        public TitleCollection Titles
        {
            get
            {
                return chartPicture.Titles;
            }
        }

        /// <summary>
        /// Chart annotation collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeAnnotations3"),
        ]
        public AnnotationCollection Annotations
        {
            get
            {
                return chartPicture.Annotations;
            }
        }

        /// <summary>
        /// Color palette to use
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributePalette"),
        ]
        public ChartColorPalette Palette
        {
            get
            {
                return _dataManager.Palette;
            }
            set
            {
                _dataManager.Palette = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Specifies whether smoothing (antialiasing) is applied while drawing chart.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeAntiAlias"),
        ]
        public AntiAliasingStyles AntiAliasing
        {
            get
            {
                return chartPicture.AntiAliasing;
            }
            set
            {
                if (chartPicture.AntiAliasing != value)
                {
                    chartPicture.AntiAliasing = value;

                    dirtyFlag = true;
                    if (!disableInvalidates)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Specifies the quality of text antialiasing.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeTextAntiAliasingQuality")
        ]
        public TextAntiAliasingQuality TextAntiAliasingQuality
        {
            get
            {
                return chartPicture.TextAntiAliasingQuality;
            }
            set
            {
                chartPicture.TextAntiAliasingQuality = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Specifies whether smoothing is applied while drawing shadows.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeChart_SoftShadows"),
        ]
        public bool IsSoftShadows
        {
            get
            {
                return chartPicture.IsSoftShadows;
            }
            set
            {
                chartPicture.IsSoftShadows = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Reference to chart area collection
        /// </summary>
        [
        SRCategory("CategoryAttributeChart"),
        SRDescription("DescriptionAttributeChartAreas"),
        ]
        public ChartAreaCollection ChartAreas
        {
            get
            {
                return chartPicture.ChartAreas;
            }
        }

        /// <summary>
        /// Back ground color for the Chart
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackColor")
        ]
        public SKColor BackColor
        {
            get
            {
                return chartPicture.BackColor;
            }
            set
            {
                if (chartPicture.BackColor != value)
                {
                    chartPicture.BackColor = value;
                    dirtyFlag = true;
                    if (!disableInvalidates)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Fore color propery (not used)
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeForeColor")
        ]
        public SKColor ForeColor
        {
            get
            {
                return SKColor.Empty;
            }
            set
            {
            }
        }

        private SKSize _size;

        /// <summary>
        /// Fore color propery (not used)
        /// </summary>
        [
        SRCategory("CategoryAttributeLayout"),
        SRDescription("DescriptionAttributeChart_Size"),
        ]
        public SKSize Size
        {
            get
            {
                return _size;
            }
            set
            {
                chartPicture.InspectChartDimensions((int)value.Width, (int)value.Height);
                _size = value;
            }
        }

        /// <summary>
        /// Series data manipulator
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeDataManipulator")
        ]
        public DataManipulator DataManipulator
        {
            get
            {
                return chartPicture.DataManipulator;
            }
        }

        /// <summary>
        /// Title font
        /// </summary>
        [
        SRCategory("CategoryAttributeCharttitle")
        ]
        public SKFont Font { get; set; }

        /// <summary>
        /// Back Hatch style
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackHatchStyle"),
        ]
        public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return chartPicture.BackHatchStyle;
            }
            set
            {
                chartPicture.BackHatchStyle = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        public void Invalidate()
        {
            // Nothing to do
        }

        /// <summary>
        /// Chart area background image
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImage"),
        ]
        public string BackImage
        {
            get
            {
                return chartPicture.BackImage;
            }
            set
            {
                chartPicture.BackImage = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Chart area background image drawing mode.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageWrapMode"),
        ]
        public ChartImageWrapMode BackImageWrapMode
        {
            get
            {
                return chartPicture.BackImageWrapMode;
            }
            set
            {
                chartPicture.BackImageWrapMode = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Background image transparent color.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageTransparentColor")
        ]
        public SKColor BackImageTransparentColor
        {
            get
            {
                return chartPicture.BackImageTransparentColor;
            }
            set
            {
                chartPicture.BackImageTransparentColor = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Background image alignment used by ClampUnscale drawing mode.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImageAlign"),
        ]
        public ChartImageAlignmentStyle BackImageAlignment
        {
            get
            {
                return chartPicture.BackImageAlignment;
            }
            set
            {
                chartPicture.BackImageAlignment = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// A type for the background gradient
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle"),
        ]
        public GradientStyle BackGradientStyle
        {
            get
            {
                return chartPicture.BackGradientStyle;
            }
            set
            {
                chartPicture.BackGradientStyle = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The second color which is used for a gradient
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        ]
        public SKColor BackSecondaryColor
        {
            get
            {
                return chartPicture.BackSecondaryColor;
            }
            set
            {
                chartPicture.BackSecondaryColor = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Border color for the Chart
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderColor")
        ]
        public SKColor BorderColor
        {
            get
            {
                return chartPicture.BorderColor;
            }
            set
            {
                chartPicture.BorderColor = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The width of the border line
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChart_BorderlineWidth"),
        ]
        public int BorderWidth
        {
            get
            {
                return chartPicture.BorderWidth;
            }
            set
            {
                chartPicture.BorderWidth = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The style of the border line
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderDashStyle")
        ]
        public ChartDashStyle BorderDashStyle
        {
            get
            {
                return chartPicture.BorderDashStyle;
            }
            set
            {
                chartPicture.BorderDashStyle = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Border color for the Chart
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderColor")
        ]
        public SKColor BorderlineColor
        {
            get
            {
                return chartPicture.BorderColor;
            }
            set
            {
                chartPicture.BorderColor = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The width of the border line
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChart_BorderlineWidth"),
        ]
        public int BorderlineWidth
        {
            get
            {
                return chartPicture.BorderWidth;
            }
            set
            {
                chartPicture.BorderWidth = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The style of the border line
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderDashStyle"),
        ]
        public ChartDashStyle BorderlineDashStyle
        {
            get
            {
                return chartPicture.BorderDashStyle;
            }
            set
            {
                chartPicture.BorderDashStyle = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Chart border skin style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin"),
        ]
        public BorderSkin BorderSkin
        {
            get
            {
                return chartPicture.BorderSkin;
            }
            set
            {
                chartPicture.BorderSkin = value;
                dirtyFlag = true;
                if (!disableInvalidates)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Build number of the control
        /// </summary>
        [
        SRDescription("DescriptionAttributeChart_BuildNumber")
        ]
        public string BuildNumber
        {
            get
            {
                // Get build number from the assembly
                string buildNumber = String.Empty;
                Assembly assembly = Assembly.GetExecutingAssembly();
                if (assembly != null)
                {
                    buildNumber = assembly.FullName.ToUpper(CultureInfo.InvariantCulture);
                    int versionIndex = buildNumber.IndexOf("VERSION=", StringComparison.Ordinal);
                    if (versionIndex >= 0)
                    {
                        buildNumber = buildNumber.Substring(versionIndex + 8);
                    }
                    versionIndex = buildNumber.IndexOf(",", StringComparison.Ordinal);
                    if (versionIndex >= 0)
                    {
                        buildNumber = buildNumber.Substring(0, versionIndex);
                    }
                }
                return buildNumber;
            }
        }

        /// <summary>
        /// Vertical resolution of the chart renderer.
        /// </summary>
        /// <remarks>
        /// This property is for the internal use only.
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        ]
        public double RenderingDpiY { get; set; } = 96.0;

        /// <summary>
        /// Horizontal resolution of the chart renderer.
        /// </summary>
        /// <remarks>
        /// This property is for the internal use only.
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        ]
        public double RenderingDpiX { get; set; } = 96.0;

        #endregion Control public properties

        #region Control public methods

        /// <summary>
        /// Applies palette colors to series or data points.
        /// </summary>
        public void ApplyPaletteColors()
        {
            // Apply palette colors to series
            _dataManager.ApplyPaletteColors();

            // Apply palette colors to data Points in series
            foreach (Series series in Series)
            {
                // Check if palette colors should be aplied to the points
                bool applyToPoints = false;
                if (series.Palette != ChartColorPalette.None)
                {
                    applyToPoints = true;
                }
                else
                {
                    IChartType chartType = _chartTypeRegistry.GetChartType(series.ChartType);
                    applyToPoints = chartType.ApplyPaletteColorsToPoints;
                }

                // Apply palette colors to the points
                if (applyToPoints)
                {
                    series.ApplyPaletteColors();
                }
            }
        }

        /// <summary>
        /// Reset auto calculated chart properties values to "Auto".
        /// </summary>
        public void ResetAutoValues()
        {
            // Reset auto calculated series properties values
            foreach (Series series in Series)
            {
                series.ResetAutoValues();
            }

            // Reset auto calculated axis properties values
            foreach (ChartArea chartArea in ChartAreas)
            {
                chartArea.ResetAutoValues();
            }
        }

        #endregion Control public methods

        #region ISupportInitialize implementation

        /// <summary>
        /// Signals the object that initialization is starting.
        /// </summary>
        public void BeginInit()
        {
            // Disable control invalidation
            disableInvalidates = true;
        }

        /// <summary>
        /// Signals the object that initialization is complete.
        /// </summary>
        public void EndInit()
        {
            // Enable control invalidation
            disableInvalidates = false;

            // If control is durty - invalidate it
            if (dirtyFlag)
            {
                Invalidate();
            }
        }

        #endregion ISupportInitialize implementation

        #region Axis data scaleView position/size changing events

        /// <summary>
        /// Called when axis scaleView position/size is about to change.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_AxisViewChanging"),
        SRCategory("CategoryAttributeAxisView")]
        public event EventHandler<ViewEventArgs> AxisViewChanging;

        /// <summary>
        /// Called when axis scaleView position/size is changed.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_AxisViewChanged"),
        SRCategory("CategoryAttributeAxisView")]
        public event EventHandler<ViewEventArgs> AxisViewChanged;

        /// <summary>
        /// Calls event delegate.
        /// </summary>
        /// <param name="arguments">Axis scaleView event arguments.</param>
        internal void OnAxisViewChanging(ViewEventArgs arguments)
        {
            if (AxisViewChanging != null)
            {
                AxisViewChanging(this, arguments);
            }
        }

        /// <summary>
        /// Calls event delegate.
        /// </summary>
        /// <param name="arguments">Axis scaleView event arguments.</param>
        internal void OnAxisViewChanged(ViewEventArgs arguments)
        {
            if (AxisViewChanged != null)
            {
                AxisViewChanged(this, arguments);
            }
        }

        #endregion Axis data scaleView position/size changing events

        #region Axis scroll bar events

        /// <summary>
        /// Called when axis scroll bar is used by user.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_AxisScrollBarClicked"),
        SRCategory("CategoryAttributeAxisView")]
        public event EventHandler<ScrollBarEventArgs> AxisScrollBarClicked;

        /// <summary>
        /// Calls event delegate.
        /// </summary>
        /// <param name="arguments">Axis scroll bar event arguments.</param>
        internal void OnAxisScrollBarClicked(ScrollBarEventArgs arguments)
        {
            if (AxisScrollBarClicked != null)
            {
                AxisScrollBarClicked(this, arguments);
            }
        }

        #endregion Axis scroll bar events

        #region Painting events

        /// <summary>
        /// Called when chart element is painted.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_PostPaint"),
        SRCategory("CategoryAttributeAppearance")]
        public event EventHandler<ChartPaintEventArgs> PostPaint;

        /// <summary>
        /// Called when chart element back ground is painted.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_PrePaint"),
        SRCategory("CategoryAttributeAppearance")]
        public event EventHandler<ChartPaintEventArgs> PrePaint;

        /// <summary>
        /// Fires when chart element backround must be drawn.
        /// This event is fired for elements like: ChartPicture, ChartArea and Legend
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnPrePaint(ChartPaintEventArgs e)
        {
            if (PrePaint != null)
            {
                PrePaint(this, e);
            }
        }

        /// <summary>
        /// Fires when chart element backround must be drawn.
        /// This event is fired for elements like: ChartPicture, ChartArea and Legend
        /// </summary>
        /// <param name="e">Event arguments.</param>
        internal void CallOnPrePaint(ChartPaintEventArgs e)
        {
            OnPrePaint(e);
        }

        /// <summary>
        /// Fires when chart element must be drawn.
        /// This event is fired for elements like: ChartPicture, ChartArea and Legend
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnPostPaint(ChartPaintEventArgs e)
        {
            if (PostPaint != null)
            {
                PostPaint(this, e);
            }
        }

        /// <summary>
        /// Fires when chart element must be drawn.
        /// This event is fired for elements like: ChartPicture, ChartArea and Legend
        /// </summary>
        /// <param name="e">Event arguments.</param>
        internal void CallOnPostPaint(ChartPaintEventArgs e)
        {
            OnPostPaint(e);
        }

        #endregion Painting events

        #region Customize event

        /// <summary>
        /// Fires just before the chart image is drawn. Use this event to customize the chart picture.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_Customize")
        ]
        public event EventHandler Customize;

        /// <summary>
        /// Fires when all chart data is prepared to be customized before drawing.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChart_OnCustomize")
        ]
        protected virtual void OnCustomize()
        {
            if (Customize != null)
            {
                Customize(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fires when all chart data is prepared to be customized before drawing.
        /// </summary>
        internal void CallOnCustomize()
        {
            OnCustomize();
        }

        /// <summary>
        /// Use this event to customize chart legend.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_CustomizeLegend")
        ]
        public event EventHandler<CustomizeLegendEventArgs> CustomizeLegend;

        /// <summary>
        /// Fires when all chart data is prepared to be customized before drawing.
        /// </summary>
        [
            SRDescription("DescriptionAttributeChart_OnCustomizeLegend")
        ]
        protected virtual void OnCustomizeLegend(LegendItemsCollection legendItems, string legendName)
        {
            CustomizeLegend?.Invoke(this, new CustomizeLegendEventArgs(legendItems, legendName));
        }

        /// <summary>
        /// Fires when all chart data is prepared to be customized before drawing.
        /// </summary>
        internal void CallOnCustomizeLegend(LegendItemsCollection legendItems, string legendName)
        {
            OnCustomizeLegend(legendItems, legendName);
        }

        #endregion Customize event

        #region Annotation events

        /// <summary>
        /// Fires when annotation text was changed.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_AnnotationTextChanged"),
        SRCategory("CategoryAttributeAnnotation")
        ]
        public event EventHandler AnnotationTextChanged;

        /// <summary>
        /// Fires when annotation text is changed.
        /// </summary>
        /// <param name="annotation">Annotation which text was changed.</param>
        internal void OnAnnotationTextChanged(Annotation annotation)
        {
            AnnotationTextChanged?.Invoke(annotation, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when selected annotation changes.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_AnnotationSelectionChanged"),
        SRCategory("CategoryAttributeAnnotation")
        ]
        public event EventHandler AnnotationSelectionChanged;

        /// <summary>
        /// Fires when annotation position was changed.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_AnnotationPositionChanged"),
        SRCategory("CategoryAttributeAnnotation")
        ]
        public event EventHandler AnnotationPositionChanged;

        /// <summary>
        /// Fires when annotation position is changing.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_AnnotationPositionChanging"),
        SRCategory("CategoryAttributeAnnotation")
        ]
        public event EventHandler<AnnotationPositionChangingEventArgs> AnnotationPositionChanging;

        /// <summary>
        /// Fires when annotation is placed by the user on the chart.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartEvent_AnnotationPlaced"),
        SRCategory("CategoryAttributeAnnotation")
        ]
        public event EventHandler AnnotationPlaced;

        /// <summary>
        /// Fires when annotation is placed by the user on the chart.
        /// </summary>
        /// <param name="annotation">Annotation which was placed.</param>
        internal void OnAnnotationPlaced(Annotation annotation)
        {
            AnnotationPlaced?.Invoke(annotation, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when selected annotation changes.
        /// </summary>
        /// <param name="annotation">Annotation which have it's selection changed.</param>
        internal void OnAnnotationSelectionChanged(Annotation annotation)
        {
            AnnotationSelectionChanged?.Invoke(annotation, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when annotation position was changed.
        /// </summary>
        /// <param name="annotation">Annotation which have it's position changed.</param>
        internal void OnAnnotationPositionChanged(Annotation annotation)
        {
            AnnotationPositionChanged?.Invoke(annotation, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when annotation position is changing.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        /// <returns>True if event was processed.</returns>
        internal bool OnAnnotationPositionChanging(ref AnnotationPositionChangingEventArgs args)
        {
            if (AnnotationPositionChanging != null)
            {
                AnnotationPositionChanging(args.Annotation, args);
                return true;
            }
            return false;
        }

        #endregion Annotation events

        #region Control DataBind method

        /// <summary>
        /// Data binds control to the selected data source.
        /// </summary>
        public void DataBind()
        {
            chartPicture.DataBind();
        }

        /// <summary>
        /// Aligns data points using their axis labels.
        /// </summary>
        public void AlignDataPointsByAxisLabel()
        {
            chartPicture.AlignDataPointsByAxisLabel(false, PointSortOrder.Ascending);
        }

        /// <summary>
        /// Aligns data points using their axis labels.
        /// </summary>
        /// <param name="series">Comma separated list of series that should be aligned by axis label.</param>
        public void AlignDataPointsByAxisLabel(string series)
        {
            //Check arguments
            if (series == null)
                throw new ArgumentNullException(nameof(series));

            // Create list of series
            ArrayList seriesList = new();
            string[] seriesNames = series.Split(',');
            foreach (string name in seriesNames)
            {
                seriesList.Add(Series[name.Trim()]);
            }

            // Align series
            chartPicture.AlignDataPointsByAxisLabel(seriesList, false, PointSortOrder.Ascending);
        }

        /// <summary>
        /// Aligns data points using their axis labels.
        /// </summary>
        /// <param name="series">Comma separated list of series that should be aligned by axis label.</param>
        /// <param name="sortingOrder">Points sorting order by axis labels.</param>
        public void AlignDataPointsByAxisLabel(string series, PointSortOrder sortingOrder)
        {
            //Check arguments
            if (series == null)
                throw new ArgumentNullException(nameof(series));

            // Create list of series
            ArrayList seriesList = new();
            string[] seriesNames = series.Split(',');
            foreach (string name in seriesNames)
            {
                seriesList.Add(Series[name.Trim()]);
            }

            // Align series
            chartPicture.AlignDataPointsByAxisLabel(seriesList, true, sortingOrder);
        }

        /// <summary>
        /// Aligns data points using their axis labels.
        /// </summary>
        /// <param name="sortingOrder">Points sorting order by axis labels.</param>
        public void AlignDataPointsByAxisLabel(PointSortOrder sortingOrder)
        {
            chartPicture.AlignDataPointsByAxisLabel(true, sortingOrder);
        }

        /// <summary>
        /// Automatically creates and binds series to specified data table.
        /// Each column of the table becomes a Y value in a separate series.
        /// Series X value field may also be provided.
        /// </summary>
        /// <param name="dataSource">Data source.</param>
        /// <param name="xField">Name of the field for series X values.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X is a cartesian coordinate and well understood")]
        public void DataBindTable(
            IEnumerable dataSource,
            string xField)
        {
            chartPicture.DataBindTable(
                dataSource,
                xField);
        }

        /// <summary>
        /// Automatically creates and binds series to specified data table.
        /// Each column of the table becomes a Y value in a separate series.
        /// </summary>
        /// <param name="dataSource">Data source.</param>
        public void DataBindTable(IEnumerable dataSource)
        {
            chartPicture.DataBindTable(
                dataSource,
                String.Empty);
        }

        /// <summary>
        /// Data bind chart to the table. Series will be automatically added to the chart depending ont
        /// yhe number of unique values in the seriesGroupByField column of the data source.
        /// Data source can be the Ole(SQL)DataReader, DataView, DataSet, DataTable or DataRow.
        /// </summary>
        /// <param name="dataSource">Data source.</param>
        /// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
        /// <param name="xField">Name of the field for X values.</param>
        /// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
        /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public void DataBindCrossTable(
            IEnumerable dataSource,
            string seriesGroupByField,
            string xField,
            string yFields,
            string otherFields)
        {
            chartPicture.DataBindCrossTab(
                dataSource,
                seriesGroupByField,
                xField,
                yFields,
                otherFields,
                false,
                PointSortOrder.Ascending);
        }

        /// <summary>
        /// Data bind chart to the table. Series will be automatically added to the chart depending ont
        /// yhe number of unique values in the seriesGroupByField column of the data source.
        /// Data source can be the Ole(SQL)DataReader, DataView, DataSet, DataTable or DataRow.
        /// </summary>
        /// <param name="dataSource">Data source.</param>
        /// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
        /// <param name="xField">Name of the field for X values.</param>
        /// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
        /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
        /// <param name="sortingOrder">Series will be sorted by group field values in specified order.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public void DataBindCrossTable(
            IEnumerable dataSource,
            string seriesGroupByField,
            string xField,
            string yFields,
            string otherFields,
            PointSortOrder sortingOrder)
        {
            chartPicture.DataBindCrossTab(
                dataSource,
                seriesGroupByField,
                xField,
                yFields,
                otherFields,
                true,
                sortingOrder);
        }

        #endregion Control DataBind method

        #region Special Extension Methods and Properties

        /// <summary>
        /// Gets the requested chart service.
        /// </summary>
        /// <param name="serviceType">AxisName of requested service.</param>
        /// <returns>Instance of the service or null if it can't be found.</returns>
        public object GetService(Type serviceType)
        {
            // Check arguments
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            object service = null;
            if (_provider != null)
            {
                service = _provider.GetService(serviceType);
            }

            return service;
        }

        /// <summary>
        /// Called when a numeric value has to be converted to a string.
        /// </summary>
        [SRDescription("DescriptionAttributeChartEvent_PrePaint")]
        public event EventHandler<FormatNumberEventArgs> FormatNumber;

        /// <summary>
        /// Utility method for firing the FormatNumber event. Allows it to be
        /// handled via OnFormatNumber as is the usual pattern as well as via
        /// CallOnFormatNumber.
        /// </summary>
        /// <param name="caller">Event caller. Can be ChartPicture, ChartArea or Legend objects.</param>
        /// <param name="e">Event arguemtns</param>
        private void OnFormatNumber(object caller, FormatNumberEventArgs e)
        {
            FormatNumber?.Invoke(caller, e);
        }

        /// <summary>
        /// Called when a numeric value has to be converted to a string.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnFormatNumber(FormatNumberEventArgs e)
        {
            OnFormatNumber(this, e);
        }

        /// <summary>
        /// Called when a numeric value has to be converted to a string.
        /// </summary>
        /// <param name="caller">Event caller. Can be ChartPicture, ChartArea or Legend objects.</param>
        /// <param name="e">Event arguments.</param>
        internal void CallOnFormatNumber(object caller, FormatNumberEventArgs e)
        {
            OnFormatNumber(caller, e);
        }

        #endregion Special Extension Methods and Properties

        #region IDisposable override

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing control resourses
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed objects here
                if (_imageLoader != null)
                {
                    _imageLoader.Dispose();
                }

                if (_namedImages != null)
                {
                    _namedImages.Dispose();
                }

                if (_chartTypeRegistry != null)
                {
                    _chartTypeRegistry.Dispose();
                }

                if (CommonElements != null)
                {
                    CommonElements.Dispose();
                }
            }

            //The chart picture and datamanager will be the last to be disposed
            if (disposing)
            {
                if (_dataManager != null)
                {
                    _dataManager.Dispose();
                }
                if (chartPicture != null)
                {
                    chartPicture.Dispose();
                }
            }
        }

        #endregion IDisposable override
    }
}