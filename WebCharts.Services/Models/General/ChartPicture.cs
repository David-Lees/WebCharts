using SkiaSharp;
using System;
using System.Collections;

namespace WebCharts.Services
{
    /// <summary>
    /// ChartPicture class represents chart content like legends, titles,
    /// chart areas and series. It provides methods for positioning and
    /// drawing all chart elements.
    /// </summary>
    public class ChartPicture : ChartElement, IServiceProvider
    {
        #region Fields

        /// <summary>
        /// Indicates that chart exceptions should be suppressed.
        /// </summary>

        // Chart Graphic object
        internal ChartGraphics ChartGraph { get; set; }

        private int _borderWidth = 1;
        private int _width = 300;
        private int _height = 300;
        private readonly DataManipulator _dataManipulator = new();

        // Chart areas collection
        private ChartAreaCollection _chartAreas = null;

        // Chart legend collection
        private LegendCollection _legends = null;

        // Chart title collection
        private TitleCollection _titles = null;

        // Chart annotation collection
        private AnnotationCollection _annotations = null;

        // Annotation smart labels class
        internal AnnotationSmartLabel annotationSmartLabel = new();

        // Chart picture events
        internal event EventHandler<ChartPaintEventArgs> BeforePaint;

        internal event EventHandler<ChartPaintEventArgs> AfterPaint;

        // Element spacing size
        internal const float elementSpacing = 3F;

        // Maximum size of the font in percentage
        internal const float maxTitleSize = 15F;

        // Printing indicator
        internal bool isPrinting = false;

        // Indicates chart selection mode
        internal bool isSelectionMode = false;

        private FontCache _fontCache = new();

        // Position of the chart 3D border
        private SKRect _chartBorderPosition = SKRect.Empty;

        // Saving As Image indicator
        internal bool isSavingAsImage = false;

        // Indicates that chart background is restored from the double buffer
        // prior to drawing top level objects like annotations, cursors and selection.
        internal bool backgroundRestored = false;

        // Buffered image of non-top level chart elements
        internal SKBitmap nonTopLevelChartBuffer = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Service container</param>
        public ChartPicture(ChartService chart)
        {
            // Create and set Common Elements
            Common = chart.CommonElements;
            ChartGraph = new ChartGraphics(Common);

            // Create border properties class
            BorderSkin = new BorderSkin(this);

            // Create a collection of chart areas
            _chartAreas = new ChartAreaCollection(this);

            // Create a collection of legends
            _legends = new LegendCollection(this);

            // Create a collection of titles
            _titles = new TitleCollection(this);

            // Create a collection of annotations
            _annotations = new AnnotationCollection(this);

            // Set Common elements for data manipulator
            _dataManipulator.Common = Common;
        }

        /// <summary>
        /// Returns Chart service object
        /// </summary>
        /// <param name="serviceType">Service AxisName</param>
        /// <returns>Chart picture</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(ChartPicture))
            {
                return this;
            }
            throw (new ArgumentException(SR.ExceptionChartPictureUnsupportedType(serviceType.ToString())));
        }

        #endregion Constructors

        #region Painting and selection methods

        /// <summary>
        /// Performs empty painting.
        /// </summary>
        internal void PaintOffScreen()
        {
            // Check chart size
            // NOTE: Fixes issue #4733
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            // Set process Mode to hot regions
            Common.HotRegionsList.ProcessChartMode |= ProcessMode.HotRegions;
            Common.HotRegionsList.hitTestCalled = true;

            // Enable selection mode
            isSelectionMode = true;

            // Hot Region list does not exist. Create the list.
            Common.HotRegionsList.Clear();

            // Create a new bitmap
            SKBitmap image = new(Math.Max(1, Width), Math.Max(1, Height));

            // Creates a new Graphics object from the
            // specified Image object.
            SKCanvas offScreen = new(image);

            // Connect Graphics object with Chart Graphics object
            ChartGraph.Graphics = offScreen;

            // Remember the previous dirty flag
            bool oldDirtyFlag = Common.Chart.dirtyFlag;

            Paint(ChartGraph.Graphics, false);

            image.Dispose();

            // Restore the previous dirty flag
            Common.Chart.dirtyFlag = oldDirtyFlag;

            // Disable selection mode
            isSelectionMode = false;

            // Set process Mode to hot regions
            Common.HotRegionsList.ProcessChartMode |= ProcessMode.HotRegions;
        }

        internal bool GetBorderSkinVisibility()
        {
            return BorderSkin.SkinStyle != BorderSkinStyle.None && Width > 20 && Height > 20;
        }

        /// <summary>
        /// This function paints a chart.
        /// </summary>
        /// <param name="graph">The graph provides drawing object to the display device. A Graphics object is associated with a specific device context.</param>
        /// <param name="paintTopLevelElementOnly">Indicates that only chart top level elements like cursors, selection or annotation objects must be redrawn.</param>
        internal void Paint(
            SKCanvas graph,
            bool paintTopLevelElementOnly)
        {
            // Reset restored and saved backgound flags
            backgroundRestored = false;

            // Reset Annotation Smart Labels
            annotationSmartLabel.Reset();

            // Do not draw the control if size is less than 5 pixel
            if (Width < 5 || Height < 5)
            {
                return;
            }

            bool resetHotRegionList = false;

            if (
                Common.HotRegionsList.hitTestCalled
                || IsToolTipsEnabled()
                )
            {
                Common.HotRegionsList.ProcessChartMode = ProcessMode.HotRegions | ProcessMode.Paint;

                Common.HotRegionsList.hitTestCalled = false;

                // Clear list of hot regions
                if (paintTopLevelElementOnly)
                {
                    // If repainting only top level elements (annotations) -
                    // clear top level objects hot regions only
                    for (int index = 0; index < Common.HotRegionsList.List.Count; index++)
                    {
                        HotRegion region = (HotRegion)Common.HotRegionsList.List[index];
                        if (region.Type == ChartElementType.Annotation)
                        {
                            Common.HotRegionsList.List.RemoveAt(index);
                            --index;
                        }
                    }
                }
                else
                {
                    // If repainting whole chart - clear all hot regions
                    resetHotRegionList = true;
                }
            }
            else
            {
                Common.HotRegionsList.ProcessChartMode = ProcessMode.Paint;

                // If repainting whole chart - clear all hot regions
                resetHotRegionList = true;
            }

            // Reset hot region list
            if (resetHotRegionList)
            {
                Common.HotRegionsList.Clear();
            }

            // Check if control was data bound
            if (this is ChartImage chartImage && !chartImage.boundToDataSource && Common != null && Common.Chart != null)
            {
                Common.Chart.DataBind();
            }

            // Connect Graphics object with Chart Graphics object
            ChartGraph.Graphics = graph;

            Common.graph = ChartGraph;

            // Set anti alias mode
            ChartGraph.AntiAliasing = AntiAliasing;
            ChartGraph.softShadows = IsSoftShadows;

            try
            {
                // Check if only chart area cursors and annotations must be redrawn
                if (!paintTopLevelElementOnly)
                {
                    // Fire Before Paint event
                    OnBeforePaint(new ChartPaintEventArgs(Chart, ChartGraph, Common, new ElementPosition(0, 0, 100, 100)));

                    // Flag indicates that resize method should be called
                    // after adjusting the intervals in 3D charts
                    bool resizeAfterIntervalAdjusting = false;

                    // RecalculateAxesScale paint chart areas
                    foreach (ChartArea area in _chartAreas)
                    {
                        // Check if area is visible
                        if (area.Visible)

                        {
                            area.Set3DAnglesAndReverseMode();
                            area.SetTempValues();
                            area.ReCalcInternal();

                            // Resize should be called the second time
                            if (area.Area3DStyle.Enable3D && !area.chartAreaIsCurcular)
                            {
                                resizeAfterIntervalAdjusting = true;
                            }
                        }
                    }

                    // Call Customize event
                    Common.Chart.CallOnCustomize();

                    // Resize picture
                    Resize(ChartGraph, resizeAfterIntervalAdjusting);

                    // This code is introduce because labels has to
                    // be changed when scene is rotated.
                    bool intervalReCalculated = false;
                    foreach (ChartArea area in _chartAreas)
                    {
                        if (area.Area3DStyle.Enable3D &&
                            !area.chartAreaIsCurcular

                            && area.Visible

                            )

                        {
                            // Make correction for interval in 3D space
                            intervalReCalculated = true;
                            area.Estimate3DInterval(ChartGraph);
                            area.ReCalcInternal();
                        }
                    }

                    // Resize chart areas after updating 3D interval
                    if (resizeAfterIntervalAdjusting)
                    {
                        // NOTE: Fixes issue #6808.
                        // In 3D chart area interval will be changed to compenstae for the axis rotation angle.
                        // This will cause all standard labels to be changed. We need to call the customize event
                        // the second time to give user a chance to modify those labels.
                        if (intervalReCalculated)
                        {
                            // Call Customize event
                            Common.Chart.CallOnCustomize();
                        }

                        // Resize chart elements
                        Resize(ChartGraph);
                    }

                    //***********************************************************************
                    //** Draw chart 3D border
                    //***********************************************************************
                    if (GetBorderSkinVisibility())
                    {
                        // Fill rectangle with page color
                        ChartGraph.FillRectangleAbs(new SKRect(0, 0, Width - 1, Height - 1),
                            BorderSkin.PageColor,
                            ChartHatchStyle.None,
                            "",
                            ChartImageWrapMode.Tile,
                            SKColor.Empty,
                            ChartImageAlignmentStyle.Center,
                            GradientStyle.None,
                            SKColor.Empty,
                            BorderSkin.PageColor,
                            1,
                            ChartDashStyle.Solid,
                            PenAlignment.Inset);

                        // Draw 3D border
                        ChartGraph.Draw3DBorderAbs(
                            BorderSkin,
                            _chartBorderPosition,
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
                            BorderDashStyle);
                    }

                    // Paint Background
                    else
                    {
                        ChartGraph.FillRectangleAbs(new SKRect(0, 0, Width - 1, Height - 1),
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
                            PenAlignment.Inset);
                    }

                    // Call BackPaint event
                    Chart.CallOnPrePaint(new ChartPaintEventArgs(Chart, ChartGraph, Common, new ElementPosition(0, 0, 100, 100)));

                    // Call paint function for each chart area.
                    foreach (ChartArea area in _chartAreas)
                    {
                        // Check if area is visible
                        if (area.Visible)

                        {
                            area.Paint(ChartGraph);
                        }
                    }

                    // This code is introduced because of GetPointsInterval method,
                    // which is very time consuming. There is no reason to calculate
                    // interval after painting.
                    foreach (ChartArea area in _chartAreas)
                    {
                        // Reset interval data
                        area.intervalData = double.NaN;
                    }

                    // Draw Legends
                    foreach (Legend legendCurrent in Legends)
                    {
                        legendCurrent.Paint(ChartGraph);
                    }

                    // Draw chart titles from the collection
                    foreach (Title titleCurrent in Titles)
                    {
                        titleCurrent.Paint(ChartGraph);
                    }

                    // Call Paint event
                    Chart.CallOnPostPaint(new ChartPaintEventArgs(Chart, ChartGraph, Common, new ElementPosition(0, 0, 100, 100)));
                }

                // Draw annotation objects
                Annotations.Paint(ChartGraph, paintTopLevelElementOnly);

                // Draw chart areas cursors in all areas.
                // Only if not in selection
                if (!isSelectionMode)
                {
                    foreach (ChartArea area in _chartAreas)
                    {
                        // Check if area is visible
                        if (area.Visible)

                        {
                            area.PaintCursors(ChartGraph, paintTopLevelElementOnly);
                        }
                    }
                }

                // Return default values
                foreach (ChartArea area in _chartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)

                    {
                        area.Restore3DAnglesAndReverseMode();
                        area.GetTempValues();
                    }
                }
            }
            finally
            {
                // Fire After Paint event
                OnAfterPaint(new ChartPaintEventArgs(Chart, ChartGraph, Common, new ElementPosition(0, 0, 100, 100)));

                // Restore temp values for each chart area
                foreach (ChartArea area in _chartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)

                    {
                        area.Restore3DAnglesAndReverseMode();
                        area.GetTempValues();
                    }
                }
            }
        }

        /// <summary>
        /// Invoke before paint delegates.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnBeforePaint(ChartPaintEventArgs e)
        {
            //Invokes the delegates.
            BeforePaint?.Invoke(this, e);
        }

        /// <summary>
        /// Invoke after paint delegates.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnAfterPaint(ChartPaintEventArgs e)
        {
            //Invokes the delegates.
            AfterPaint?.Invoke(this, e);
        }

        internal override void Invalidate()
        {
            base.Invalidate();

            if (Chart != null)
                Chart.Invalidate();
        }

        #endregion Painting and selection methods

        #region Resizing methods

        /// <summary>
        /// Resize the chart picture.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        public void Resize(ChartGraphics chartGraph)
        {
            Resize(chartGraph, false);
        }

        /// <summary>
        /// Resize the chart picture.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="calcAreaPositionOnly">Indicates that only chart area position is calculated.</param>
        public void Resize(ChartGraphics chartGraph, bool calcAreaPositionOnly)
        {
            // Set the chart size for Common elements
            Common.Width = _width;
            Common.Height = _height;

            // Set the chart size for Chart graphics
            chartGraph.SetPictureSize(_width, _height);

            // Initialize chart area(s) rectangle
            SKRect chartAreasRectangle = new(0, 0, _width - 1, _height - 1);
            chartAreasRectangle = chartGraph.GetRelativeRectangle(chartAreasRectangle);

            //******************************************************
            //** Get the 3D border interface
            //******************************************************
            SKRect titlePosition = SKRect.Empty;
            bool titleInBorder = false;

            if (BorderSkin.SkinStyle != BorderSkinStyle.None)
            {
                // Set border size
                _chartBorderPosition = chartGraph.GetAbsoluteRectangle(chartAreasRectangle);

                // Get border interface
                IBorderType border3D = Common.BorderTypeRegistry.GetBorderType(BorderSkin.SkinStyle.ToString());
                if (border3D != null)
                {
                    border3D.Resolution = 96;
                    // Check if title should be displayed in the border
                    titleInBorder = border3D.GetTitlePositionInBorder() != SKRect.Empty;
                    titlePosition = chartGraph.GetRelativeRectangle(border3D.GetTitlePositionInBorder());
                    titlePosition.Size = new(chartAreasRectangle.Width - titlePosition.Width, titlePosition.Height);

                    // Adjust are position to the border size
                    border3D.AdjustAreasPosition(chartGraph, ref chartAreasRectangle);
                }
            }

            //******************************************************
            //** Calculate position of all titles in the collection
            //******************************************************
            SKRect frameTitlePosition = SKRect.Empty;
            if (titleInBorder)
            {
                frameTitlePosition = new SKRect(titlePosition.Left, titlePosition.Top, titlePosition.Right, titlePosition.Bottom);
            }
            foreach (Title title in Titles)
            {
                if (title.DockedToChartArea == Constants.NotSetValue &&
                    title.Position.Auto &&
                    title.Visible)
                {
                    title.CalcTitlePosition(chartGraph, ref chartAreasRectangle, ref frameTitlePosition, elementSpacing);
                }
            }

            //******************************************************
            //** Calculate position of all legends in the collection
            //******************************************************
            Legends.CalcLegendPosition(chartGraph, ref chartAreasRectangle, elementSpacing);

            //******************************************************
            //** Calculate position of the chart area(s)
            //******************************************************
            chartAreasRectangle.Right -= elementSpacing;
            chartAreasRectangle.Bottom -= elementSpacing;
            SKRect areaPosition = new();

            // Get number of chart areas that requeres automatic positioning
            int areaNumber = 0;
            foreach (ChartArea area in _chartAreas)
            {
                // Check if area is visible
                if (area.Visible && area.Position.Auto)
                {
                    ++areaNumber;
                }
            }

            // Calculate how many columns & rows of areas we going to have
            int areaColumns = (int)Math.Floor(Math.Sqrt(areaNumber));
            if (areaColumns < 1)
            {
                areaColumns = 1;
            }
            int areaRows = (int)Math.Ceiling(areaNumber / ((float)areaColumns));

            // Set position for all areas
            int column = 0;
            int row = 0;
            foreach (ChartArea area in _chartAreas)
            {
                // Check if area is visible
                if (area.Visible)

                {
                    if (area.Position.Auto)
                    {
                        // Calculate area position
                        areaPosition.Left = chartAreasRectangle.Left + column * (chartAreasRectangle.Width / areaColumns) + elementSpacing;
                        areaPosition.Top = chartAreasRectangle.Top + row * (chartAreasRectangle.Height / areaRows) + elementSpacing;
                        areaPosition.Size = new(chartAreasRectangle.Width / areaColumns - elementSpacing,
                             chartAreasRectangle.Height / areaRows - elementSpacing);

                        // Calculate position of all titles in the collection docked outside of the chart area
                        TitleCollection.CalcOutsideTitlePosition(this, chartGraph, area, ref areaPosition, elementSpacing);

                        // Calculate position of the legend if it's docked outside of the chart area
                        Legends.CalcOutsideLegendPosition(chartGraph, area, ref areaPosition, elementSpacing);

                        // Set area position without changing the Auto flag
                        area.Position.SetPositionNoAuto(areaPosition.Left, areaPosition.Top, areaPosition.Width, areaPosition.Height);

                        // Go to next area
                        ++row;
                        if (row >= areaRows)
                        {
                            row = 0;
                            ++column;
                        }
                    }
                    else
                    {
                        SKRect rect = area.Position.ToSKRect();

                        // Calculate position of all titles in the collection docked outside of the chart area
                        TitleCollection.CalcOutsideTitlePosition(this, chartGraph, area, ref rect, elementSpacing);

                        // Calculate position of the legend if it's docked outside of the chart area
                        Legends.CalcOutsideLegendPosition(chartGraph, area, ref rect, elementSpacing);
                    }
                }
            }

            //******************************************************
            //** Align chart areas Position if required
            //******************************************************
            AlignChartAreasPosition();

            //********************************************************
            //** Check if only chart area position must be calculated.
            //********************************************************
            if (!calcAreaPositionOnly)
            {
                //******************************************************
                //** Call Resize function for each chart area.
                //******************************************************
                foreach (ChartArea area in _chartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)

                    {
                        area.Resize(chartGraph);
                    }
                }

                //******************************************************
                //** Align chart areas InnerPlotPosition if required
                //******************************************************
                AlignChartAreas(AreaAlignmentStyles.PlotPosition);

                //******************************************************
                //** Calculate position of the legend if it's inside
                //** chart plotting area
                //******************************************************

                // Calculate position of all titles in the collection docked outside of the chart area
                TitleCollection.CalcInsideTitlePosition(this, chartGraph, elementSpacing);

                Legends.CalcInsideLegendPosition(chartGraph, elementSpacing);
            }
        }

        /// <summary>
        /// Minimum and maximum do not have to be calculated
        /// from data series every time. It is very time
        /// consuming. Minimum and maximum are buffered
        /// and only when this flags are set Minimum and
        /// Maximum are refreshed from data.
        /// </summary>
        internal void ResetMinMaxFromData()
        {
            if (_chartAreas != null)
            {
                // Call ResetMinMaxFromData function for each chart area.
                foreach (ChartArea area in _chartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)
                    {
                        area.ResetMinMaxFromData();
                    }
                }
            }
        }

        /// <summary>
        /// RecalculateAxesScale the chart picture.
        /// </summary>
        public void Recalculate()
        {
            // Call ReCalc function for each chart area.
            foreach (ChartArea area in _chartAreas)
            {
                // Check if area is visible
                if (area.Visible)

                {
                    area.ReCalcInternal();
                }
            }
        }

        #endregion Resizing methods

        #region Chart picture properties

        /// <summary>
        /// Indicates that non-critical chart exceptions will be suppressed.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeSuppressExceptions"),
        ]
        internal bool SuppressExceptions { set; get; } = false;

        /// <summary>
        /// Chart border skin style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin"),
        ]
        public BorderSkin BorderSkin { get; set; } = null;

        /// <summary>
        /// Reference to chart area collection
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChartAreas"),
        ]
        public ChartAreaCollection ChartAreas
        {
            get
            {
                return _chartAreas;
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
                return _legends;
            }
        }

        /// <summary>
        /// Chart title collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeCharttitle"),
        SRDescription("DescriptionAttributeTitles"),
        ]
        public TitleCollection Titles
        {
            get
            {
                return _titles;
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
                return _annotations;
            }
        }

        /// <summary>
        /// Background color for the Chart
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        public SKColor BackColor { get; set; } = SKColors.White;

        /// <summary>
        /// Border color for the Chart
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderColor"),
        ]
        public SKColor BorderColor { get; set; } = SKColors.White;

        /// <summary>
        /// Chart width
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeWidth"),
        ]
        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                InspectChartDimensions(value, Height);
                _width = value;
                Common.Width = _width;
            }
        }

        /// <summary>
        /// Series Data Manipulator
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeDataManipulator"),
        ]
        public DataManipulator DataManipulator
        {
            get
            {
                return _dataManipulator;
            }
        }

        /// <summary>
        /// Chart height
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeHeight3"),
        ]
        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                InspectChartDimensions(Width, value);
                _height = value;
                Common.Height = value;
            }
        }

        /// <summary>
        /// Back Hatch style
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackHatchStyle"),
        ]
        public ChartHatchStyle BackHatchStyle { get; set; } = ChartHatchStyle.None;

        /// <summary>
        /// Chart area background image
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImage"),
        ]
        public string BackImage { get; set; } = "";

        /// <summary>
        /// Chart area background image drawing mode.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageWrapMode"),
        ]
        public ChartImageWrapMode BackImageWrapMode { get; set; } = ChartImageWrapMode.Tile;

        /// <summary>
        /// Background image transparent color.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        ]
        public SKColor BackImageTransparentColor { get; set; } = SKColor.Empty;

        /// <summary>
        /// Background image alignment used by ClampUnscale drawing mode.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImageAlign"),
        ]
        public ChartImageAlignmentStyle BackImageAlignment { get; set; } = ChartImageAlignmentStyle.TopLeft;

        /// <summary>
        /// Indicates that smoothing is applied while drawing shadows.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeSoftShadows3"),
        ]
        public bool IsSoftShadows { get; set; } = true;

        /// <summary>
        /// Specifies whether smoothing (antialiasing) is applied while drawing chart.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeAntiAlias"),
        ]
        public AntiAliasingStyles AntiAliasing { get; set; } = AntiAliasingStyles.All;

        /// <summary>
        /// Specifies the quality of text antialiasing.
        /// </summary>
        [
        SRCategory("CategoryAttributeImage"),
        SRDescription("DescriptionAttributeTextAntiAliasingQuality"),
        ]
        public TextAntiAliasingQuality TextAntiAliasingQuality { get; set; } = TextAntiAliasingQuality.High;

        /// <summary>
        /// A type for the background gradient
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle"),
        ]
        public GradientStyle BackGradientStyle { get; set; } = GradientStyle.None;

        /// <summary>
        /// The second color which is used for a gradient
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        ]
        public SKColor BackSecondaryColor { get; set; } = SKColor.Empty;

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
                return _borderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionChartBorderIsNegative);
                }
                _borderWidth = value;
            }
        }

        /// <summary>
        /// The style of the border line
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderDashStyle"),
        ]
        public ChartDashStyle BorderDashStyle { get; set; } = ChartDashStyle.NotSet;

        /// <summary>
        /// Gets the font cache.
        /// </summary>
        /// <value>The font cache.</value>
        internal FontCache FontCache
        {
            get { return _fontCache; }
        }

        #endregion Chart picture properties

        #region Chart areas alignment methods

        /// <summary>
        /// Checks if any of the chart areas are aligned.
        /// Also checks if the chart ares name in AlignWithChartArea property is valid.
        /// </summary>
        /// <returns>True if at least one area requires alignment.</returns>
        private bool IsAreasAlignmentRequired()
        {
            bool alignmentRequired = false;

            // Loop through all chart areas
            foreach (ChartArea area in ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible && area.AlignWithChartArea != Constants.NotSetValue)
                {
                    alignmentRequired = true;

                    // Check the chart area used for alignment
                    if (_chartAreas.IndexOf(area.AlignWithChartArea) < 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionChartAreaNameReferenceInvalid(area.Name, area.AlignWithChartArea)));
                    }
                }
            }

            return alignmentRequired;
        }

        /// <summary>
        /// Creates a list of the aligned chart areas.
        /// </summary>
        /// <param name="masterArea">Master chart area.</param>
        /// <param name="type">Alignment type.</param>
        /// <param name="orientation">Vertical or Horizontal orientation.</param>
        /// <returns>List of areas that area aligned to the master area.</returns>
        private ArrayList GetAlignedAreasGroup(ChartArea masterArea, AreaAlignmentStyles type, AreaAlignmentOrientations orientation)
        {
            ArrayList areaList = new();

            // Loop throught the chart areas and get the ones aligned with specified master area
            foreach (ChartArea area in ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible && area.Name != masterArea.Name &&
                        area.AlignWithChartArea == masterArea.Name &&
                        (area.AlignmentStyle & type) == type &&
                        (area.AlignmentOrientation & orientation) == orientation)
                {
                    // Add client area into the list
                    areaList.Add(area);
                }
            }

            // If list is not empty insert "master" area in the beginning
            if (areaList.Count > 0)
            {
                areaList.Insert(0, masterArea);
            }

            return areaList;
        }

        /// <summary>
        /// Performs specified type of alignment for the chart areas.
        /// </summary>
        /// <param name="type">Alignment type required.</param>
        internal void AlignChartAreas(AreaAlignmentStyles type)
        {
            // Check if alignment required
            if (IsAreasAlignmentRequired())
            {
                // Loop through all chart areas
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if chart area is visible
                    if (area.Visible)

                    {
                        // Get vertical areas alignment group using current area as a master
                        ArrayList alignGroup = GetAlignedAreasGroup(
                            area,
                            type,
                            AreaAlignmentOrientations.Vertical);

                        // Align each area in the group
                        if (alignGroup.Count > 0)
                        {
                            AlignChartAreasPlotPosition(alignGroup, AreaAlignmentOrientations.Vertical);
                        }

                        // Get horizontal areas alignment group using current area as a master
                        alignGroup = GetAlignedAreasGroup(
                            area,
                            type,
                            AreaAlignmentOrientations.Horizontal);

                        // Align each area in the group
                        if (alignGroup.Count > 0)
                        {
                            AlignChartAreasPlotPosition(alignGroup, AreaAlignmentOrientations.Horizontal);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Align inner plot position of the chart areas in the group.
        /// </summary>
        /// <param name="areasGroup">List of areas in the group.</param>
        /// <param name="orientation">Group orientation.</param>
        private void AlignChartAreasPlotPosition(ArrayList areasGroup, AreaAlignmentOrientations orientation)
        {
            //****************************************************************
            //** Find the smalles size of the inner plot
            //****************************************************************
            SKRect areaPlotPosition = ((ChartArea)areasGroup[0]).PlotAreaPosition.ToSKRect();
            foreach (ChartArea area in areasGroup)
            {
                if (area.PlotAreaPosition.X > areaPlotPosition.Left)
                {
                    areaPlotPosition.Left += area.PlotAreaPosition.X - areaPlotPosition.Left;
                    areaPlotPosition.Right -= area.PlotAreaPosition.X;
                }
                if (area.PlotAreaPosition.Y > areaPlotPosition.Top)
                {
                    areaPlotPosition.Top += area.PlotAreaPosition.Y - areaPlotPosition.Top;
                    areaPlotPosition.Bottom -= area.PlotAreaPosition.Y - areaPlotPosition.Top;
                }
                if (area.PlotAreaPosition.Right < areaPlotPosition.Right)
                {
                    areaPlotPosition.Right -= areaPlotPosition.Right - area.PlotAreaPosition.Right;
                    if (areaPlotPosition.Width < 5)
                    {
                        areaPlotPosition.Right = areaPlotPosition.Left + 5;
                    }
                }
                if (area.PlotAreaPosition.Bottom < areaPlotPosition.Bottom)
                {
                    areaPlotPosition.Bottom -= areaPlotPosition.Bottom - area.PlotAreaPosition.Bottom;
                    if (areaPlotPosition.Height < 5)
                    {
                        areaPlotPosition.Bottom = areaPlotPosition.Top + 5;
                    }
                }
            }

            //****************************************************************
            //** Align inner plot position for all areas
            //****************************************************************
            foreach (ChartArea area in areasGroup)
            {
                // Get curretn plot position of the area
                SKRect rect = area.PlotAreaPosition.ToSKRect();

                // Adjust area position
                if ((orientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
                {
                    rect.Left = areaPlotPosition.Left;
                    rect.Right = rect.Left + areaPlotPosition.Width;
                }
                if ((orientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
                {
                    rect.Top = areaPlotPosition.Top;
                    rect.Bottom = rect.Top + areaPlotPosition.Height;
                }

                // Set new plot position in coordinates relative to chart picture
                area.PlotAreaPosition.SetPositionNoAuto(rect.Left, rect.Top, rect.Width, rect.Height);

                // Set new plot position in coordinates relative to chart area position
                rect.Left = (rect.Left - area.Position.X) / area.Position.Width * 100f;
                rect.Top = (rect.Top - area.Position.Y) / area.Position.Height * 100f;
                rect.Right = rect.Left + rect.Width / area.Position.Width * 100f;
                rect.Bottom = rect.Top + rect.Height / area.Position.Height * 100f;
                area.InnerPlotPosition.SetPositionNoAuto(rect.Left, rect.Top, rect.Width, rect.Height);

                if ((orientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
                {
                    area.AxisX2.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
                    area.AxisX.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
                }
                if ((orientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
                {
                    area.AxisY2.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
                    area.AxisY.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
                }
            }
        }

        /// <summary>
        /// Aligns positions of the chart areas.
        /// </summary>
        private void AlignChartAreasPosition()
        {
            // Check if alignment required
            if (IsAreasAlignmentRequired())
            {
                // Loop through all chart areas
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if chart area is visible
                    if (area.Visible && area.AlignWithChartArea != Constants.NotSetValue && (area.AlignmentStyle & AreaAlignmentStyles.Position) == AreaAlignmentStyles.Position)
                    {
                        // Get current area position
                        SKRect areaPosition = area.Position.ToSKRect();

                        // Get master chart area
                        ChartArea masterArea = ChartAreas[area.AlignWithChartArea];

                        // Vertical alignment
                        if ((area.AlignmentOrientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
                        {
                            // Align area position
                            areaPosition.Left = masterArea.Position.X;
                            areaPosition.Size = new(masterArea.Position.Width, areaPosition.Height);
                        }

                        // Horizontal alignment
                        if ((area.AlignmentOrientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
                        {
                            // Align area position
                            areaPosition.Top = masterArea.Position.Y;
                            areaPosition.Bottom = areaPosition.Top + masterArea.Position.Height;
                        }

                        // Set new position
                        area.Position.SetPositionNoAuto(areaPosition.Left, areaPosition.Top, areaPosition.Width, areaPosition.Height);
                    }
                }
            }
        }

        /// <summary>
		/// Align chart areas cursor.
		/// </summary>
		/// <param name="changedArea">Changed chart area.</param>
		/// <param name="orientation">Orientation of the changed cursor.</param>
		/// <param name="selectionChanged">AxisName of change cursor or selection.</param>
		internal void AlignChartAreasCursor(ChartArea changedArea, AreaAlignmentOrientations orientation, bool selectionChanged)
        {
            // Check if alignment required
            if (IsAreasAlignmentRequired())
            {
                // Loop through all chart areas
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if chart area is visible
                    if (area.Visible)

                    {
                        // Get vertical areas alignment group using current area as a master
                        ArrayList alignGroup = GetAlignedAreasGroup(
                            area,
                            AreaAlignmentStyles.Cursor,
                            orientation);

                        // Align each area in the group if it contains changed area
                        if (alignGroup.Contains(changedArea))
                        {
                            // Set cursor position for all areas in the group
                            foreach (ChartArea groupArea in alignGroup)
                            {
                                groupArea.alignmentInProcess = true;

                                if (orientation == AreaAlignmentOrientations.Vertical)
                                {
                                    if (selectionChanged)
                                    {
                                        groupArea.CursorX.SelectionStart = changedArea.CursorX.SelectionStart;
                                        groupArea.CursorX.SelectionEnd = changedArea.CursorX.SelectionEnd;
                                    }
                                    else
                                    {
                                        groupArea.CursorX.Position = changedArea.CursorX.Position;
                                    }
                                }
                                if (orientation == AreaAlignmentOrientations.Horizontal)
                                {
                                    if (selectionChanged)
                                    {
                                        groupArea.CursorY.SelectionStart = changedArea.CursorY.SelectionStart;
                                        groupArea.CursorY.SelectionEnd = changedArea.CursorY.SelectionEnd;
                                    }
                                    else
                                    {
                                        groupArea.CursorY.Position = changedArea.CursorY.Position;
                                    }
                                }

                                groupArea.alignmentInProcess = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
		/// One of the chart areas was zoomed by the user.
		/// </summary>
		/// <param name="changedArea">Changed chart area.</param>
		/// <param name="orientation">Orientation of the changed scaleView.</param>
		/// <param name="disposeBufferBitmap">Area double fuffer image must be disposed.</param>
		internal void AlignChartAreasZoomed(ChartArea changedArea, AreaAlignmentOrientations orientation, bool disposeBufferBitmap)
        {
            // Check if alignment required
            if (IsAreasAlignmentRequired())
            {
                // Loop through all chart areas
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if chart area is visible
                    if (area.Visible)

                    {
                        // Get vertical areas alignment group using current area as a master
                        ArrayList alignGroup = GetAlignedAreasGroup(
                            area,
                            AreaAlignmentStyles.AxesView,
                            orientation);

                        // Align each area in the group if it contains changed area
                        if (alignGroup.Contains(changedArea))
                        {
                            // Set cursor position for all areas in the group
                            foreach (ChartArea groupArea in alignGroup)
                            {
                                // Clear image buffer
                                if (groupArea.areaBufferBitmap != null && disposeBufferBitmap)
                                {
                                    groupArea.areaBufferBitmap.Dispose();
                                    groupArea.areaBufferBitmap = null;
                                }

                                if (orientation == AreaAlignmentOrientations.Vertical)
                                {
                                    groupArea.CursorX.SelectionStart = double.NaN;
                                    groupArea.CursorX.SelectionEnd = double.NaN;
                                }
                                if (orientation == AreaAlignmentOrientations.Horizontal)
                                {
                                    groupArea.CursorY.SelectionStart = double.NaN;
                                    groupArea.CursorY.SelectionEnd = double.NaN;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
		/// Align chart areas axes views.
		/// </summary>
		/// <param name="changedArea">Changed chart area.</param>
		/// <param name="orientation">Orientation of the changed scaleView.</param>
		internal void AlignChartAreasAxesView(ChartArea changedArea, AreaAlignmentOrientations orientation)
        {
            // Check if alignment required
            if (IsAreasAlignmentRequired())
            {
                // Loop through all chart areas
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if chart area is visible
                    if (area.Visible)

                    {
                        // Get vertical areas alignment group using current area as a master
                        ArrayList alignGroup = GetAlignedAreasGroup(
                            area,
                            AreaAlignmentStyles.AxesView,
                            orientation);

                        // Align each area in the group if it contains changed area
                        if (alignGroup.Contains(changedArea))
                        {
                            // Set cursor position for all areas in the group
                            foreach (ChartArea groupArea in alignGroup)
                            {
                                groupArea.alignmentInProcess = true;

                                if (orientation == AreaAlignmentOrientations.Vertical)
                                {
                                    groupArea.AxisX.ScaleView.Position = changedArea.AxisX.ScaleView.Position;
                                    groupArea.AxisX.ScaleView.Size = changedArea.AxisX.ScaleView.Size;
                                    groupArea.AxisX.ScaleView.SizeType = changedArea.AxisX.ScaleView.SizeType;

                                    groupArea.AxisX2.ScaleView.Position = changedArea.AxisX2.ScaleView.Position;
                                    groupArea.AxisX2.ScaleView.Size = changedArea.AxisX2.ScaleView.Size;
                                    groupArea.AxisX2.ScaleView.SizeType = changedArea.AxisX2.ScaleView.SizeType;
                                }
                                if (orientation == AreaAlignmentOrientations.Horizontal)
                                {
                                    groupArea.AxisY.ScaleView.Position = changedArea.AxisY.ScaleView.Position;
                                    groupArea.AxisY.ScaleView.Size = changedArea.AxisY.ScaleView.Size;
                                    groupArea.AxisY.ScaleView.SizeType = changedArea.AxisY.ScaleView.SizeType;

                                    groupArea.AxisY2.ScaleView.Position = changedArea.AxisY2.ScaleView.Position;
                                    groupArea.AxisY2.ScaleView.Size = changedArea.AxisY2.ScaleView.Size;
                                    groupArea.AxisY2.ScaleView.SizeType = changedArea.AxisY2.ScaleView.SizeType;
                                }

                                groupArea.alignmentInProcess = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion Chart areas alignment methods

        #region Helper methods

        /// <summary>
        /// Inspects the chart dimensions.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        internal void InspectChartDimensions(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentException(SR.ExceptionValueMustBeGreaterThan(nameof(Width), "0px"));
            }
            if (height < 0)
            {
                throw new ArgumentException(SR.ExceptionValueMustBeGreaterThan(nameof(Height), "0px"));
            }
        }

        /// <summary>
        /// Returns the default title from Titles collection.
        /// </summary>
        /// <param name="create">Create title if it doesn't exists.</param>
        /// <returns>Default title.</returns>
        internal Title GetDefaultTitle(bool create)
        {
            // Check if default title exists
            Title defaultTitle = null;
            foreach (Title title in Titles)
            {
                if (title.Name == "Default Title")
                {
                    defaultTitle = title;
                }
            }

            // Create new default title
            if (defaultTitle == null && create)
            {
                defaultTitle = new();
                defaultTitle.Name = "Default Title";
                Titles.Insert(0, defaultTitle);
            }

            return defaultTitle;
        }

        /// <summary>
        /// Checks if tooltips are enabled
        /// </summary>
        /// <returns>true if tooltips enabled</returns>
        private bool IsToolTipsEnabled()
        {
            // Data series loop
            foreach (Series series in Common.DataManager.Series)
            {
                // Check series tooltips
                if (series.ToolTip.Length > 0)
                {
                    // ToolTips enabled
                    return true;
                }

                // Check series tooltips
                if (series.LegendToolTip.Length > 0 ||
                    series.LabelToolTip.Length > 0)
                {
                    // ToolTips enabled
                    return true;
                }

                // Check point tooltips only for "non-Fast" chart types
                if (!series.IsFastChartType())
                {
                    // Data point loop
                    foreach (DataPoint point in series.Points)
                    {
                        // ToolTip empty
                        if (point.ToolTip.Length > 0)
                        {
                            // ToolTips enabled
                            return true;
                        }
                        // ToolTip empty
                        if (point.LegendToolTip.Length > 0 ||
                            point.LabelToolTip.Length > 0)
                        {
                            // ToolTips enabled
                            return true;
                        }
                    }
                }
            }

            // Legend items loop
            foreach (Legend legend in Legends)
            {
                foreach (LegendItem legendItem in legend.CustomItems)
                {
                    // ToolTip empty
                    if (legendItem.ToolTip.Length > 0)
                    {
                        return true;
                    }
                }
            }

            // Title items loop
            foreach (Title title in Titles)
            {
                // ToolTip empty
                if (title.ToolTip.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Helper methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (ChartGraph != null)
                {
                    ChartGraph.Dispose();
                    ChartGraph = null;
                }
                if (_legends != null)
                {
                    _legends.Dispose();
                    _legends = null;
                }
                if (_titles != null)
                {
                    _titles.Dispose();
                    _titles = null;
                }
                if (_chartAreas != null)
                {
                    _chartAreas.Dispose();
                    _chartAreas = null;
                }
                if (_annotations != null)
                {
                    _annotations.Dispose();
                    _annotations = null;
                }
                if (_fontCache != null)
                {
                    _fontCache.Dispose();
                    _fontCache = null;
                }
                if (BorderSkin != null)
                {
                    BorderSkin.Dispose();
                    BorderSkin = null;
                }

                if (nonTopLevelChartBuffer != null)
                {
                    nonTopLevelChartBuffer.Dispose();
                    nonTopLevelChartBuffer = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}