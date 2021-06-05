// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Axis related properties and methods. Axis class gives
//				information to Common.Chart series about
//				position in the Common.Chart area and keeps all necessary
//				information about axes.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Text;

namespace WebCharts.Services
{
    /// <summary>
    /// The Axis class gives information to the Common.Chart series
    /// about positions in the Common.Chart area and keeps all of
    ///	the data about the axis.
    /// </summary>
    [
        SRDescription("DescriptionAttributeAxis_Axis"),
    ]
    public partial class Axis : ChartNamedElement
    {
        #region Axis fields

        /// <summary>
        /// Plot area position
        /// </summary>
        internal ElementPosition PlotAreaPosition;

        // This field synchronies Store and Reset temporary values
        private bool _storeValuesEnabled = true;

        private FontCache _fontCache = new();
        private SKFont _titleFont;
        private SKColor _titleForeColor = SKColors.Black;
        private StringAlignment _titleAlignment = StringAlignment.Center;
        private string _title = "";
        private int _lineWidth = 1;
        private ChartDashStyle _lineDashStyle = ChartDashStyle.Solid;
        private SKColor _lineColor = SKColors.Black;
        private bool _isLabelAutoFit = true;
        private AxisArrowStyle _arrowStyle = AxisArrowStyle.None;
        private StripLinesCollection _stripLines = null;
        private bool _isMarksNextToAxis = true;

        // Default text orientation
        private TextOrientation _textOrientation = TextOrientation.Auto;

        // Size of the axis elements in percentage
        internal float titleSize = 0F;

        internal float labelSize = 0F;
        internal float labelNearOffset = 0F;
        internal float labelFarOffset = 0F;
        internal float unRotatedLabelSize = 0F;
        internal float markSize = 0F;
        internal float scrollBarSize = 0F;
        internal float totlaGroupingLabelsSize = 0F;
        internal float[] groupingLabelSizes = null;
        internal float totlaGroupingLabelsSizeAdjustment = 0f;

        private LabelAutoFitStyles _labelAutoFitStyle = LabelAutoFitStyles.DecreaseFont |
                                                            LabelAutoFitStyles.IncreaseFont |
                                                            LabelAutoFitStyles.LabelsAngleStep30 |
                                                            LabelAutoFitStyles.StaggeredLabels |
                                                            LabelAutoFitStyles.WordWrap;

        // Auto calculated font for labels
        internal SKFont autoLabelFont = null;

        internal int autoLabelAngle = -1000;
        internal int autoLabelOffset = -1;

        // Labels auto fitting constants
        private float _aveLabelFontSize = 10F;

        private float _minLabelFontSize = 5F;

        // Determines maximum label size of the chart area.
        private float _maximumAutoSize = 75f;

        // Chart title position rectangle
        private SKRect _titlePosition = SKRect.Empty;

        // Element spacing size
        internal const float elementSpacing = 1F;

        // Maximum total size of the axis's elements in percentage
        private const float maxAxisElementsSize = 75F;

        // Maximum size of the axis title in percentage
        private const float maxAxisTitleSize = 20F;

        // Maximum size of the axis second row of labels in percentage
        // of the total labels size
        private const float maxAxisLabelRow2Size = 45F;

        // Maximum size of the axis tick marks in percentage
        private const float maxAxisMarkSize = 20F;

        // Minimum cached value from data series.
        internal double minimumFromData = double.NaN;

        // Maximum cached value from data series.
        internal double maximumFromData = double.NaN;

        // Flag, which tells to Set Data method to take, again values from
        // data source and not to use cached values.
        internal bool refreshMinMaxFromData = true;

        // Flag, which tells to Set Data method to take, again values from
        // data source and not to use cached values.
        internal int numberOfPointsInAllSeries = 0;

        // Original axis scaleView position
        private double _originalViewPosition = double.NaN;

        /// <summary>
        /// Indicates that isInterlaced strip lines will be displayed for the axis.
        /// </summary>
        private bool _isInterlaced = false;

        /// <summary>
        /// Color used to draw isInterlaced strip lines for the axis.
        /// </summary>
        private SKColor _interlacedColor = SKColor.Empty;

        /// <summary>
        /// Axis interval offset.
        /// </summary>
        private double _intervalOffset = 0;

        /// <summary>
        /// Axis interval.
        /// </summary>
        internal double interval = 0;

        /// <summary>
        /// Axis interval units type.
        /// </summary>
        internal DateTimeIntervalType intervalType = DateTimeIntervalType.Auto;

        /// <summary>
        /// Axis interval offset units type.
        /// </summary>
        internal DateTimeIntervalType intervalOffsetType = DateTimeIntervalType.Auto;

        /// <summary>
		/// Minimum font size that can be used by the labels auto-fitting algorithm.
		/// </summary>
		internal int labelAutoFitMinFontSize = 6;

        /// <summary>
        /// Maximum font size that can be used by the labels auto-fitting algorithm.
        /// </summary>
        internal int labelAutoFitMaxFontSize = 10;

        #endregion Axis fields

        #region Axis constructor and initialization

        /// <summary>
        /// Default constructor of Axis.
        /// </summary>
        public Axis()
            : base(null, GetName(AxisName.X))
        {
            Initialize(AxisName.X);
        }

        /// <summary>
        /// Axis constructor.
        /// </summary>
        /// <param name="chartArea">The chart area the axis belongs to.</param>
        /// <param name="axisTypeName">The type of the axis.</param>
        public Axis(ChartArea chartArea, AxisName axisTypeName)
            : base(chartArea, GetName(axisTypeName))
        {
            Initialize(axisTypeName);
        }

        /// <summary>
        /// Initialize axis class
        /// </summary>
        /// <param name="axisTypeName">Name of the axis type.</param>
        private void Initialize(AxisName axisTypeName)
        {
            // DT: Axis could be already created. Don't recreate new labelstyle and other objects.
            // Initialize axis labels
            if (labelStyle == null)
            {
                labelStyle = new LabelStyle(this);
            }
            if (_customLabels == null)
            {
                _customLabels = new CustomLabelsCollection(this);
            }
            if (_scaleView == null)
            {
                // Create axis data scaleView object
                _scaleView = new AxisScaleView(this);
            }
            if (scrollBar == null)
            {
                // Create axis croll bar class
                scrollBar = new AxisScrollBar(this);
            }

            axisType = axisTypeName;

            // Create grid & tick marks objects
            if (minorTickMark == null)
            {
                minorTickMark = new TickMark(this, false);
            }
            if (majorTickMark == null)
            {
                majorTickMark = new(this, true);
                majorTickMark.Interval = double.NaN;
                majorTickMark.IntervalOffset = double.NaN;
                majorTickMark.IntervalType = DateTimeIntervalType.NotSet;
                majorTickMark.IntervalOffsetType = DateTimeIntervalType.NotSet;
            }
            if (minorGrid == null)
            {
                minorGrid = new Grid(this, false);
            }
            if (majorGrid == null)
            {
                majorGrid = new(this, true);
                majorGrid.Interval = double.NaN;
                majorGrid.IntervalOffset = double.NaN;
                majorGrid.IntervalType = DateTimeIntervalType.NotSet;
                majorGrid.IntervalOffsetType = DateTimeIntervalType.NotSet;
            }
            if (_stripLines == null)
            {
                _stripLines = new StripLinesCollection(this);
            }

            if (_titleFont == null)
            {
                _titleFont = _fontCache.DefaultFont;
            }

            // Create collection of scale segments
            if (scaleSegments == null)
            {
                scaleSegments = new AxisScaleSegmentCollection(this);
            }

            // Create scale break style
            if (axisScaleBreakStyle == null)
            {
                axisScaleBreakStyle = new AxisScaleBreakStyle(this);
            }
        }

        /// <summary>
        /// Initialize axis class
        /// </summary>
        /// <param name="chartArea">Chart area that the axis belongs.</param>
        /// <param name="axisTypeName">Axis type.</param>
        internal void Initialize(ChartArea chartArea, AxisName axisTypeName)
        {
            Initialize(axisTypeName);
            Parent = chartArea;
            Name = GetName(axisTypeName);
        }

        /// <summary>
        /// Set Axis Name
        /// </summary>
        internal static string GetName(AxisName axisName)
        {
            // Set axis name.
            // NOTE: Strings below should never be localized. Name properties in the chart are never localized
            // and represent consisten object name in all locales.
            return axisName switch
            {
                (AxisName.X) => "X axis",
                (AxisName.Y) => "Y (Value) axis",
                (AxisName.X2) => "Secondary X axis",
                (AxisName.Y2) => "Secondary Y (Value) axis",
                _ => null,
            };
        }

        #endregion Axis constructor and initialization

        #region Axis properies

        // Internal
        internal ChartArea ChartArea
        {
            get { return Parent as ChartArea; }
        }

        /// <summary>
        /// Text orientation.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
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
                Invalidate();
            }
        }

        /// <summary>
        /// Returns sub-axis name.
        /// </summary>
        virtual internal string SubAxisName
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether interlaced strip lines will be displayed for the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeInterlaced"),
        ]
        public bool IsInterlaced
        {
            get
            {
                return _isInterlaced;
            }
            set
            {
                _isInterlaced = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color used to draw interlaced strip lines for the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeInterlacedColor"),
        ]
        public SKColor InterlacedColor
        {
            get
            {
                return _interlacedColor;
            }
            set
            {
                _interlacedColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Axis name. This field is reserved for internal use only.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeAxis_Name"),
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
        /// Axis name. This field is reserved for internal use only.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeType"),
        ]
        virtual public AxisName AxisName
        {
            get
            {
                return axisType;
            }
        }

        /// <summary>
        /// Gets or sets the arrow style used for the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeArrows"),
        ]
        public AxisArrowStyle ArrowStyle
        {
            get
            {
                return _arrowStyle;
            }
            set
            {
                _arrowStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the major gridlines.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        SRDescription("DescriptionAttributeMajorGrid"),
        ]
        public Grid MajorGrid
        {
            get
            {
                return majorGrid;
            }
            set
            {
                majorGrid = value;
                majorGrid.Axis = this;
                majorGrid.majorGridTick = true;

                if (!majorGrid.intervalChanged)
                    majorGrid.Interval = double.NaN;
                if (!majorGrid.intervalOffsetChanged)
                    majorGrid.IntervalOffset = double.NaN;
                if (!majorGrid.intervalTypeChanged)
                    majorGrid.IntervalType = DateTimeIntervalType.NotSet;
                if (!majorGrid.intervalOffsetTypeChanged)
                    majorGrid.IntervalOffsetType = DateTimeIntervalType.NotSet;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the minor gridlines.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        SRDescription("DescriptionAttributeMinorGrid"),
        ]
        public Grid MinorGrid
        {
            get
            {
                return minorGrid;
            }
            set
            {
                minorGrid = value;
                minorGrid.Initialize(this, false);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the major tick marks.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        SRDescription("DescriptionAttributeMajorTickMark"),
        ]
        public TickMark MajorTickMark
        {
            get
            {
                return majorTickMark;
            }
            set
            {
                majorTickMark = value;
                majorTickMark.Axis = this;
                majorTickMark.majorGridTick = true;

                if (!majorTickMark.intervalChanged)
                    majorTickMark.Interval = double.NaN;
                if (!majorTickMark.intervalOffsetChanged)
                    majorTickMark.IntervalOffset = double.NaN;
                if (!majorTickMark.intervalTypeChanged)
                    majorTickMark.IntervalType = DateTimeIntervalType.NotSet;
                if (!majorTickMark.intervalOffsetTypeChanged)
                    majorTickMark.IntervalOffsetType = DateTimeIntervalType.NotSet;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the minor tick marks.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        SRDescription("DescriptionAttributeMinorTickMark"),
        ]
        public TickMark MinorTickMark
        {
            get
            {
                return minorTickMark;
            }
            set
            {
                minorTickMark = value;
                minorTickMark.Initialize(this, false);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether auto-fitting of labels is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabels"),
        SRDescription("DescriptionAttributeLabelsAutoFit"),
        ]
        public bool IsLabelAutoFit
        {
            get
            {
                return _isLabelAutoFit;
            }
            set
            {
                _isLabelAutoFit = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the minimum font size that can be used by
        /// the label auto-fitting algorithm.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabels"),
        SRDescription("DescriptionAttributeLabelsAutoFitMinFontSize"),
        ]
        public int LabelAutoFitMinFontSize
        {
            get
            {
                return labelAutoFitMinFontSize;
            }
            set
            {
                // Font size cannot be less than 5
                if (value < 5)
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelsAutoFitMinFontSizeValueInvalid));
                }

                labelAutoFitMinFontSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the maximum font size that can be used by
        /// the label auto-fitting algorithm.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabels"),
        SRDescription("DescriptionAttributeLabelsAutoFitMaxFontSize"),
        ]
        public int LabelAutoFitMaxFontSize
        {
            get
            {
                return labelAutoFitMaxFontSize;
            }
            set
            {
                // Font size cannot be less than 5
                if (value < 5)
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelsAutoFitMaxFontSizeInvalid));
                }

                labelAutoFitMaxFontSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the auto-fitting style used for the labels.
        /// IsLabelAutoFit must be set to true.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabels"),
        SRDescription("DescriptionAttributeLabelsAutoFitStyle"),
        ]
        public LabelAutoFitStyles LabelAutoFitStyle
        {
            get
            {
                return _labelAutoFitStyle;
            }
            set
            {
                _labelAutoFitStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether
        /// tick marks and labels move with the axis when
        /// the crossing value changes.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeMarksNextToAxis"),
        ]
        virtual public bool IsMarksNextToAxis
        {
            get
            {
                return _isMarksNextToAxis;
            }
            set
            {
                _isMarksNextToAxis = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeTitle6"),
        ]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeTitleColor"),
        ]
        public SKColor TitleForeColor
        {
            get
            {
                return _titleForeColor;
            }
            set
            {
                _titleForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        SRDescription("DescriptionAttributeTitleAlignment"),
        ]
        public StringAlignment TitleAlignment
        {
            get
            {
                return _titleAlignment;
            }
            set
            {
                _titleAlignment = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the font used for the axis title.
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
                _titleFont = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the line color of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineColor"),
        ]
        public SKColor LineColor
        {
            get
            {
                return _lineColor;
            }
            set
            {
                _lineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the line width of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineWidth"),
        ]
        public int LineWidth
        {
            get
            {
                return _lineWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAxisWidthIsNegative);
                }
                _lineWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the line style of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineDashStyle"),
        ]
        public ChartDashStyle LineDashStyle
        {
            get
            {
                return _lineDashStyle;
            }
            set
            {
                _lineDashStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The collection of strip lines of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeStripLines"),
        ]
        public StripLinesCollection StripLines
        {
            get
            {
                return _stripLines;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size (in percentage) of the axis used in the automatic layout algorithm.
        /// </summary>
        /// <remarks>
        /// This property determines the maximum size of the axis, measured as a percentage of the chart area.
        /// </remarks>
        [
        SRCategory("CategoryAttributeLabels"),
        SRDescription("DescriptionAttributeAxis_MaxAutoSize"),
        ]
        public float MaximumAutoSize
        {
            get
            {
                return _maximumAutoSize;
            }
            set
            {
                if (value < 0f || value > 100f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionValueMustBeInRange(nameof(MaximumAutoSize), "0", "100"));
                }
                _maximumAutoSize = value;
                Invalidate();
            }
        }

        #endregion Axis properies

        #region	IMapAreaAttributes Properties implementation

        /// <summary>
        /// Tooltip of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeMapArea"),
        SRDescription("DescriptionAttributeToolTip"),
        ]
        public string ToolTip { set; get; } = string.Empty;

        #endregion

        #region Axis Interavl properies

        /// <summary>
        /// Axis interval size.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeInterval4"),
        ]
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                // Axis interval properties must be set
                if (double.IsNaN(value))
                {
                    interval = 0;
                }
                else
                {
                    interval = value;
                }

                // Reset initial values
                majorGrid.interval = tempMajorGridInterval;
                majorTickMark.interval = tempMajorTickMarkInterval;
                minorGrid.interval = tempMinorGridInterval;
                minorTickMark.interval = tempMinorTickMarkInterval;
                labelStyle.interval = tempLabelInterval;

                Invalidate();
            }
        }

        /// <summary>
        /// Axis interval offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeIntervalOffset6"),
        ]
        public double IntervalOffset
        {
            get
            {
                return _intervalOffset;
            }
            set
            {
                // Axis interval properties must be set
                if (double.IsNaN(value))
                {
                    _intervalOffset = 0;
                }
                else
                {
                    _intervalOffset = value;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Axis interval type.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeIntervalType4"),
        ]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return intervalType;
            }
            set
            {
                // Axis interval properties must be set
                if (value == DateTimeIntervalType.NotSet)
                {
                    intervalType = DateTimeIntervalType.Auto;
                }
                else
                {
                    intervalType = value;
                }

                // Reset initial values
                majorGrid.intervalType = tempGridIntervalType;
                majorTickMark.intervalType = tempTickMarkIntervalType;
                labelStyle.intervalType = tempLabelIntervalType;

                Invalidate();
            }
        }

        /// <summary>
        /// Axis interval offset type.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeIntervalOffsetType4"),
        ]
        public DateTimeIntervalType IntervalOffsetType
        {
            get
            {
                return intervalOffsetType;
            }
            set
            {
                // Axis interval properties must be set
                if (value == DateTimeIntervalType.NotSet)
                {
                    intervalOffsetType = DateTimeIntervalType.Auto;
                }
                else
                {
                    intervalOffsetType = value;
                }

                Invalidate();
            }
        }

        #endregion

        #region Axis painting methods

        /// <summary>
        /// Checks if Common.Chart axis title is drawn vertically.
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
        /// Returns axis title text orientation. If set to Auto automatically determines the
        /// orientation based on the axis position.
        /// </summary>
        /// <returns>Current text orientation.</returns>
        private TextOrientation GetTextOrientation()
        {
            if (TextOrientation == TextOrientation.Auto)
            {
                if (AxisPosition == AxisPosition.Left)
                {
                    return TextOrientation.Rotated270;
                }
                else if (AxisPosition == AxisPosition.Right)
                {
                    return TextOrientation.Rotated90;
                }
                return TextOrientation.Horizontal;
            }
            return TextOrientation;
        }

        /// <summary>
        /// Paint Axis elements on the back of the 3D scene.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void PrePaint(ChartGraphics graph)
        {
            if (enabled)
            {
                // draw axis hot region
                DrawAxisLineHotRegion(graph, true);

                // Paint Major Tick Marks
                majorTickMark.Paint(graph, true);

                // Paint Minor Tick Marks
                minorTickMark.Paint(graph, true);

                // Draw axis line
                DrawAxisLine(graph, true);

                // Paint Labels
                labelStyle.Paint(graph, true);
            }
        }

        /// <summary>
        /// Paint Axis
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void Paint(ChartGraphics graph)
        {
            // Only Y axis is drawn in the circular Common.Chart area
            if (ChartArea != null && ChartArea.chartAreaIsCurcular)
            {
                // Y circular axes
                if (axisType == AxisName.Y && enabled)
                {
                    ICircularChartType chartType = ChartArea.GetCircularChartType();
                    if (chartType != null)
                    {
                        SKMatrix oldMatrix = graph.Transform;
                        float[] axesLocation = chartType.GetYAxisLocations(ChartArea);
                        bool drawLabels = true;
                        foreach (float curentSector in axesLocation)
                        {
                            // Set graphics rotation matrix
                            var p = graph.GetAbsolutePoint(ChartArea.circularCenter);
                            var newMatrix = SKMatrix.CreateRotationDegrees(curentSector, p.X, p.Y);
                            graph.Transform = newMatrix;

                            // draw axis hot region
                            DrawAxisLineHotRegion(graph, false);

                            // Paint Minor Tick Marks
                            minorTickMark.Paint(graph, false);

                            // Paint Major Tick Marks
                            majorTickMark.Paint(graph, false);

                            // Draw axis line
                            DrawAxisLine(graph, false);

                            // Only first Y axis has labels
                            if (drawLabels)
                            {
                                drawLabels = false;

                                // Save current font angle
                                int currentAngle = labelStyle.Angle;

                                // Set labels text angle
                                if (labelStyle.Angle == 0)
                                {
                                    if (curentSector >= 45f && curentSector <= 180f)
                                    {
                                        labelStyle.angle = -90;
                                    }
                                    else if (curentSector > 180f && curentSector <= 315f)
                                    {
                                        labelStyle.angle = 90;
                                    }
                                }

                                // Draw labels
                                labelStyle.Paint(graph, false);

                                // Restore font angle
                                labelStyle.angle = currentAngle;
                            }
                        }

                        graph.Transform = oldMatrix;
                    }
                }

                // X circular axes
                if (axisType == AxisName.X && enabled)
                {
                    labelStyle.PaintCircular(graph);
                }

                DrawAxisTitle(graph);

                return;
            }

            // If axis is disabled draw only Title
            if (enabled)
            {
                // draw axis hot region
                DrawAxisLineHotRegion(graph, false);

                // Paint Minor Tick Marks
                minorTickMark.Paint(graph, false);

                // Paint Major Tick Marks
                majorTickMark.Paint(graph, false);

                // Draw axis line
                DrawAxisLine(graph, false);

                // Paint Labels
                labelStyle.Paint(graph, false);

                // Scroll bar is supoorted only in 2D charts
                if (ChartArea != null && !ChartArea.Area3DStyle.Enable3D)
                {
                    // Draw axis scroll bar
                    ScrollBar.Paint(graph);
                }
            }

            // Draw axis title
            DrawAxisTitle(graph);

            // Reset temp axis offset for side-by-side charts like column
            ResetTempAxisOffset();
        }

        /// <summary>
        /// Paint Axis element when segmented axis scale feature is used.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void PaintOnSegmentedScalePassOne(ChartGraphics graph)
        {
            // If axis is disabled draw only Title
            if (enabled)
            {
                // Paint Minor Tick Marks
                minorTickMark.Paint(graph, false);

                // Paint Major Tick Marks
                majorTickMark.Paint(graph, false);
            }
        }

        /// <summary>
        /// Paint Axis element when segmented axis scale feature is used.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void PaintOnSegmentedScalePassTwo(ChartGraphics graph)
        {
            // If axis is disabled draw only Title
            if (enabled)
            {
                // Draw axis line
                DrawAxisLine(graph, false);

                // Paint Labels
                labelStyle.Paint(graph, false);
            }

            // Draw axis title
            DrawAxisTitle(graph);

            // Reset temp axis offset for side-by-side charts like column
            ResetTempAxisOffset();
        }

        /// <summary>
        /// Draw axis title
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        private void DrawAxisTitle(ChartGraphics graph)
        {
            if (!enabled)
                return;

            // Draw axis title
            if (Title.Length > 0)
            {
                SKMatrix oldTransform = SKMatrix.Empty;

                // Draw title in 3D
                if (ChartArea.Area3DStyle.Enable3D && !ChartArea.chartAreaIsCurcular)
                {
                    DrawAxis3DTitle(graph);
                    return;
                }

                string axisTitle = Title;

                //******************************************************
                //** Check axis position
                //******************************************************
                float axisPosition = (float)GetAxisPosition();
                if (AxisPosition == AxisPosition.Bottom)
                {
                    if (!GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.Bottom;
                    }
                    axisPosition = ChartArea.PlotAreaPosition.Bottom - axisPosition;
                }
                else if (AxisPosition == AxisPosition.Top)
                {
                    if (!GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.Y;
                    }
                    axisPosition -= ChartArea.PlotAreaPosition.Y;
                }
                else if (AxisPosition == AxisPosition.Right)
                {
                    if (!GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.Right;
                    }
                    axisPosition = ChartArea.PlotAreaPosition.Right - axisPosition;
                }
                else if (AxisPosition == AxisPosition.Left)
                {
                    if (!GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.X;
                    }
                    axisPosition -= ChartArea.PlotAreaPosition.X;
                }

                //******************************************************
                //** Adjust axis elements size with axis position
                //******************************************************
                // Calculate total size of axis elements
                float axisSize = markSize + labelSize;
                axisSize -= axisPosition;
                if (axisSize < 0)
                {
                    axisSize = 0;
                }
                // Set title alignment
                using (StringFormat format = new())
                {
                    format.Alignment = TitleAlignment;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    // VSTS #144398
                    // We need to have the StringFormatFlags set to FitBlackBox as othwerwise axis titles using Fonts like
                    // "Algerian" or "Forte" are completly clipped (= not drawn) due to the fact that MeasureString returns
                    // a bounding rectangle that is too small.
                    format.FormatFlags |= StringFormatFlags.FitBlackBox;

                    // Calculate title rectangle
                    _titlePosition = ChartArea.PlotAreaPosition.ToSKRect();
                    float titleSizeWithoutSpacing = titleSize - elementSpacing;
                    if (AxisPosition == AxisPosition.Left)
                    {
                        _titlePosition.Left = ChartArea.PlotAreaPosition.X - titleSizeWithoutSpacing - axisSize;
                        _titlePosition.Top = ChartArea.PlotAreaPosition.Y;

                        if (!IsTextVertical)
                        {
                            SKSize axisTitleSize = new(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height);
                            _titlePosition.Right = _titlePosition.Left + axisTitleSize.Width;
                            _titlePosition.Bottom = _titlePosition.Top + axisTitleSize.Height;

                            format.Alignment = StringAlignment.Center;
                            if (TitleAlignment == StringAlignment.Far)
                            {
                                format.LineAlignment = StringAlignment.Near;
                            }
                            else if (TitleAlignment == StringAlignment.Near)
                            {
                                format.LineAlignment = StringAlignment.Far;
                            }
                            else
                            {
                                format.LineAlignment = StringAlignment.Center;
                            }
                        }
                        else
                        {
                            SKSize axisTitleSize = graph.GetAbsoluteSize(new SKSize(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height));
                            axisTitleSize = graph.GetRelativeSize(new SKSize(axisTitleSize.Height, axisTitleSize.Width));

                            _titlePosition.Top += ChartArea.PlotAreaPosition.Height / 2f - _titlePosition.Height / 2f;
                            _titlePosition.Left += titleSizeWithoutSpacing / 2f - _titlePosition.Width / 2f;
                            _titlePosition.Right = _titlePosition.Left + axisTitleSize.Width;
                            _titlePosition.Bottom = _titlePosition.Top + axisTitleSize.Height;

                            // Set graphics rotation transformation
                            oldTransform = SetRotationTransformation(graph, _titlePosition);

                            // Set alignment
                            format.LineAlignment = StringAlignment.Center;
                        }
                    }
                    else if (AxisPosition == AxisPosition.Right)
                    {
                        _titlePosition.Left = ChartArea.PlotAreaPosition.Right + axisSize;
                        _titlePosition.Top = ChartArea.PlotAreaPosition.Y;

                        if (!IsTextVertical)
                        {
                            SKSize axisTitleSize = new(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height);
                            _titlePosition.Right = _titlePosition.Left + axisTitleSize.Width;
                            _titlePosition.Bottom = _titlePosition.Top + axisTitleSize.Height;

                            format.Alignment = StringAlignment.Center;
                            if (TitleAlignment == StringAlignment.Far)
                            {
                                format.LineAlignment = StringAlignment.Near;
                            }
                            else if (TitleAlignment == StringAlignment.Near)
                            {
                                format.LineAlignment = StringAlignment.Far;
                            }
                            else
                            {
                                format.LineAlignment = StringAlignment.Center;
                            }
                        }
                        else
                        {
                            SKSize axisTitleSize = graph.GetAbsoluteSize(new SKSize(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height));
                            axisTitleSize = graph.GetRelativeSize(new SKSize(axisTitleSize.Height, axisTitleSize.Width));

                            _titlePosition.Left += titleSizeWithoutSpacing / 2f - _titlePosition.Width / 2f;
                            _titlePosition.Top += ChartArea.PlotAreaPosition.Height / 2f - _titlePosition.Height / 2f;
                            _titlePosition.Right = _titlePosition.Left + axisTitleSize.Width;
                            _titlePosition.Bottom = _titlePosition.Top + axisTitleSize.Height;

                            // Set graphics rotation transformation
                            oldTransform = SetRotationTransformation(graph, _titlePosition);

                            // Set alignment
                            format.LineAlignment = StringAlignment.Center;
                        }
                    }
                    else if (AxisPosition == AxisPosition.Top)
                    {
                        _titlePosition.Top = ChartArea.PlotAreaPosition.Y - titleSizeWithoutSpacing - axisSize;
                        _titlePosition.Bottom = _titlePosition.Top + titleSizeWithoutSpacing;
                        _titlePosition.Left = ChartArea.PlotAreaPosition.X;
                        _titlePosition.Right = _titlePosition.Left + ChartArea.PlotAreaPosition.Width;

                        if (IsTextVertical)
                        {
                            // Set graphics rotation transformation
                            oldTransform = SetRotationTransformation(graph, _titlePosition);
                        }

                        // Set alignment
                        format.LineAlignment = StringAlignment.Center;
                    }
                    else if (AxisPosition == AxisPosition.Bottom)
                    {
                        _titlePosition.Top = ChartArea.PlotAreaPosition.Bottom + axisSize;
                        _titlePosition.Bottom = _titlePosition.Top + titleSizeWithoutSpacing;
                        _titlePosition.Left = ChartArea.PlotAreaPosition.X;
                        _titlePosition.Right = _titlePosition.Left + ChartArea.PlotAreaPosition.Width;

                        if (IsTextVertical)
                        {
                            // Set graphics rotation transformation
                            oldTransform = SetRotationTransformation(graph, _titlePosition);
                        }

                        // Set alignment
                        format.LineAlignment = StringAlignment.Center;
                    }

                    // Draw title
                    using SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = TitleForeColor };
                    graph.DrawStringRel(
                        axisTitle.Replace("\\n", "\n"),
                        TitleFont,
                        brush,
                        _titlePosition,
                        format,
                        GetTextOrientation());
                }

                // Process selection regions
                if (Common.ProcessModeRegions)
                {
                    // NOTE: Solves Issue #4423
                    // Transform title position coordinates using curent Graphics matrix
                    SKRect transformedTitlePosition = graph.GetAbsoluteRectangle(_titlePosition);
                    SKPoint[] rectPoints = new SKPoint[] {
                        new SKPoint(transformedTitlePosition.Left, transformedTitlePosition.Top),
                        new SKPoint(transformedTitlePosition.Right, transformedTitlePosition.Bottom) };
                    graph.Transform.TransformPoints(rectPoints);
                    transformedTitlePosition = new SKRect(
                        rectPoints[0].X,
                        rectPoints[0].Y,
                        rectPoints[1].X - rectPoints[0].X,
                        rectPoints[1].Y - rectPoints[0].Y);
                    if (transformedTitlePosition.Width < 0 || transformedTitlePosition.Height < 0)
                    {
                        transformedTitlePosition = transformedTitlePosition.Standardized;
                    }

                    // Add hot region
                    Common.HotRegionsList.AddHotRegion(
                        transformedTitlePosition, this, ChartElementType.AxisTitle, false, false);
                }

                // Restore old transformation
                if (oldTransform != SKMatrix.Empty)
                {
                    graph.Transform = oldTransform;
                }
            }
        }

        /// <summary>
        /// Helper method which sets 90 or -90 degrees transformation in the middle of the
        /// specified rectangle. It is used to draw title text rotated 90 or 270 degrees.
        /// </summary>
        /// <param name="graph">Chart graphics to apply transformation for.</param>
        /// <param name="titlePosition">Title position.</param>
        /// <returns>Old graphics transformation matrix.</returns>
        private SKMatrix SetRotationTransformation(ChartGraphics graph, SKRect titlePosition)
        {
            // Save old graphics transformation
            SKMatrix oldTransform = graph.Transform;

            // Rotate left tile 90 degrees at center
            SKPoint center = SKPoint.Empty;
            center.X = titlePosition.MidX;
            center.Y = titlePosition.MidY;

            // Create and set new transformation matrix
            float angle = (GetTextOrientation() == TextOrientation.Rotated90) ? 90f : -90f;
            var p = graph.GetAbsolutePoint(center);
            SKMatrix newMatrix = SKMatrix.CreateRotationDegrees(angle, p.X, p.Y);
            graph.Transform = newMatrix;

            return oldTransform;
        }

        /// <summary>
        /// Draws a radial line in circular Common.Chart area.
        /// </summary>
        /// <param name="obj">Object requesting the painting.</param>
        /// <param name="graph">Graphics path.</param>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="position">X axis circular position.</param>
        internal void DrawRadialLine(
            object obj,
            ChartGraphics graph,
            SKColor color,
            int width,
            ChartDashStyle style,
            double position)
        {
            // Create circle position rectangle
            SKRect rect = ChartArea.PlotAreaPosition.ToSKRect();
            rect = graph.GetAbsoluteRectangle(rect);

            // Make sure the rectangle width equals rectangle height for the circle
            if (rect.Width != rect.Height)
            {
                if (rect.Width > rect.Height)
                {
                    var h = rect.Height;
                    rect.Left += (rect.Width - h) / 2f;
                    rect.Right = rect.Left + h;
                }
                else
                {
                    var w = rect.Width;
                    rect.Top += (rect.Height - w) / 2f;
                    rect.Bottom = rect.Top + w;
                }
            }

            // Convert axis position to angle
            float angle = ChartArea.CircularPositionToAngle(position);

            // Set clipping region to the polygon
            SKRegion oldRegion = null;
            if (ChartArea.CircularUsePolygons)
            {
                oldRegion = graph.Clip;
                graph.Clip = new SKRegion(graph.GetPolygonCirclePath(rect, ChartArea.CircularSectorsNumber));
            }

            // Get center point
            SKPoint centerPoint = graph.GetAbsolutePoint(ChartArea.circularCenter);

            // Set graphics rotation matrix
            SKMatrix oldMatrix = graph.Transform;
            SKMatrix newMatrix = SKMatrix.CreateRotationDegrees(angle, centerPoint.X, centerPoint.Y);
            graph.Transform = newMatrix;

            // Draw Line
            SKPoint endPoint = new(rect.Left + rect.Width / 2f, rect.Top);
            graph.DrawLineAbs(color, width, style, centerPoint, endPoint);

            // Process selection regions
            if (Common.ProcessModeRegions)
            {
                using SKPath path = new();
                path.AddLine(centerPoint, endPoint);
                path.Transform(newMatrix);
                try
                {
                    using SKPaint pen = new() { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = width + 2 };
                    //path.Widen(pen)
                    Common.HotRegionsList.AddHotRegion(path, false, ChartElementType.Gridlines, obj);
                }
                catch (OutOfMemoryException)
                {
                    // SKPath.Widen incorrectly throws OutOfMemoryException
                    // catching here and reacting by not widening
                }
                catch (ArgumentException)
                {
                    // Do nothing
                }
            }

            // Restore graphics
            graph.Transform = oldMatrix;

            // Restore clip region
            if (ChartArea.CircularUsePolygons)
            {
                graph.Clip = oldRegion;
            }
        }

        /// <summary>
        /// Draws a circular line in circular Common.Chart area.
        /// </summary>
        /// <param name="obj">Object requesting the painting.</param>
        /// <param name="graph">Graphics path.</param>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="position">Line position.</param>
        internal void DrawCircularLine(
            object obj,
            ChartGraphics graph,
            SKColor color,
            int width,
            ChartDashStyle style,
            float position
            )
        {
            // Create circle position rectangle
            SKRect rect = ChartArea.PlotAreaPosition.ToSKRect();
            rect = graph.GetAbsoluteRectangle(rect);

            // Make sure the rectangle width equals rectangle height for the circle
            if (rect.Width != rect.Height)
            {
                if (rect.Width > rect.Height)
                {
                    rect.Left += (rect.Width - rect.Height) / 2f;
                    rect.Right = rect.Left + rect.Height;
                }
                else
                {
                    rect.Top += (rect.Height - rect.Width) / 2f;
                    rect.Bottom = rect.Top + rect.Width;
                }
            }

            // Inflate rectangle
            SKPoint absPoint = graph.GetAbsolutePoint(new SKPoint(position, position));
            float rectInflate = absPoint.Y - rect.Top;
            rect.Inflate(-rectInflate, -rectInflate);

            // Create circle pen
            using SKPaint circlePen = new() { Style = SKPaintStyle.Fill, Color = color, StrokeWidth = width };
            circlePen.PathEffect = ChartGraphics.GetPenStyle(style, width);

            // Draw circle
            if (ChartArea.CircularUsePolygons)
            {
                // Draw eaqula sides polygon
                graph.DrawCircleAbs(circlePen, null, rect, ChartArea.CircularSectorsNumber, false);
            }
            else
            {
                graph.DrawEllipse(circlePen, rect);
            }

            // Process selection regions
            if (Common.ProcessModeRegions && rect.Width >= 1f && rect.Height > 1)
            {
                SKPath path = null;
                try
                {
                    if (ChartArea.CircularUsePolygons)
                    {
                        path = graph.GetPolygonCirclePath(rect, ChartArea.CircularSectorsNumber);
                    }
                    else
                    {
                        path = new SKPath();
                        path.AddOval(rect);
                    }
                    circlePen.StrokeWidth += 2;
                    //path.Widen(circlePen)
                    Common.HotRegionsList.AddHotRegion(path, false, ChartElementType.Gridlines, obj);
                }
                catch (OutOfMemoryException)
                {
                    // SKPath.Widen incorrectly throws OutOfMemoryException
                    // catching here and reacting by not widening
                }
                catch (ArgumentException)
                {
                    // Do nothing
                }
                finally
                {
                    path.Dispose();
                }
            }
        }

        /// <summary>
        /// Draw axis title in 3D.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        private void DrawAxis3DTitle(ChartGraphics graph)
        {
            // Do not draw title if axis is not enabled
            if (!enabled)
            {
                return;
            }

            string axisTitle = Title;

            // Draw axis title
            SKPoint rotationCenter = SKPoint.Empty;
            int angle = 0;

            // Set title alignment
            using StringFormat format = new();
            format.Alignment = TitleAlignment;
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags |= StringFormatFlags.LineLimit;

            // Measure title size for non-centered aligment
            SKSize realTitleSize = ChartGraphics.MeasureString(axisTitle.Replace("\\n", "\n"), TitleFont, new SKSize(10000f, 10000f), format, GetTextOrientation());
            SKSize axisTitleSize = SKSize.Empty;
            if (format.Alignment != StringAlignment.Center)
            {
                axisTitleSize = realTitleSize;
                if (IsTextVertical)
                {
                    // Switch height and width for vertical axis
                    float tempValue = axisTitleSize.Height;
                    axisTitleSize.Height = axisTitleSize.Width;
                    axisTitleSize.Width = tempValue;
                }

                // Get relative size
                axisTitleSize = graph.GetRelativeSize(axisTitleSize);

                // Change format aligment for the reversed mode
                if (ChartArea.ReverseSeriesOrder)
                {
                    if (format.Alignment == StringAlignment.Near)
                    {
                        format.Alignment = StringAlignment.Far;
                    }
                    else
                    {
                        format.Alignment = StringAlignment.Near;
                    }
                }
            }

            // Set text rotation angle based on the text orientation
            if (GetTextOrientation() == TextOrientation.Rotated90)
            {
                angle = 90;
            }
            else if (GetTextOrientation() == TextOrientation.Rotated270)
            {
                angle = -90;
            }

            // Calculate title center point on the axis
            if (AxisPosition == AxisPosition.Left)
            {
                rotationCenter = new SKPoint(ChartArea.PlotAreaPosition.X, ChartArea.PlotAreaPosition.Y + ChartArea.PlotAreaPosition.Height / 2f);
                if (format.Alignment == StringAlignment.Near)
                {
                    rotationCenter.Y = ChartArea.PlotAreaPosition.Bottom - axisTitleSize.Height / 2f;
                }
                else if (format.Alignment == StringAlignment.Far)
                {
                    rotationCenter.Y = ChartArea.PlotAreaPosition.Y + axisTitleSize.Height / 2f;
                }
            }
            else if (AxisPosition == AxisPosition.Right)
            {
                rotationCenter = new SKPoint(ChartArea.PlotAreaPosition.Right, ChartArea.PlotAreaPosition.Y + ChartArea.PlotAreaPosition.Height / 2f);
                if (format.Alignment == StringAlignment.Near)
                {
                    rotationCenter.Y = ChartArea.PlotAreaPosition.Bottom - axisTitleSize.Height / 2f;
                }
                else if (format.Alignment == StringAlignment.Far)
                {
                    rotationCenter.Y = ChartArea.PlotAreaPosition.Y + axisTitleSize.Height / 2f;
                }
            }
            else if (AxisPosition == AxisPosition.Top)
            {
                rotationCenter = new SKPoint(ChartArea.PlotAreaPosition.X + ChartArea.PlotAreaPosition.Width / 2f, ChartArea.PlotAreaPosition.Y);
                if (format.Alignment == StringAlignment.Near)
                {
                    rotationCenter.X = ChartArea.PlotAreaPosition.X + axisTitleSize.Width / 2f;
                }
                else if (format.Alignment == StringAlignment.Far)
                {
                    rotationCenter.X = ChartArea.PlotAreaPosition.Right - axisTitleSize.Width / 2f;
                }
            }
            else if (AxisPosition == AxisPosition.Bottom)
            {
                rotationCenter = new SKPoint(ChartArea.PlotAreaPosition.X + ChartArea.PlotAreaPosition.Width / 2f, ChartArea.PlotAreaPosition.Bottom);
                if (format.Alignment == StringAlignment.Near)
                {
                    rotationCenter.X = ChartArea.PlotAreaPosition.X + axisTitleSize.Width / 2f;
                }
                else if (format.Alignment == StringAlignment.Far)
                {
                    rotationCenter.X = ChartArea.PlotAreaPosition.Right - axisTitleSize.Width / 2f;
                }
            }

            // Transform center of title coordinates and calculate axis angle
            float zPosition = GetMarksZPosition(out bool isOnEdge);
            Point3D[] rotationCenterPoints = null;
            float angleAxis = 0;
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                rotationCenterPoints = new Point3D[] {
                    new Point3D(rotationCenter.X, rotationCenter.Y, zPosition),
                    new Point3D(rotationCenter.X - 20f, rotationCenter.Y, zPosition) };

                // Transform coordinates of text rotation point
                ChartArea.matrix3D.TransformPoints(rotationCenterPoints);
                rotationCenter = rotationCenterPoints[0].SKPoint;

                // Get absolute coordinates
                rotationCenterPoints[0].SKPoint = graph.GetAbsolutePoint(rotationCenterPoints[0].SKPoint);
                rotationCenterPoints[1].SKPoint = graph.GetAbsolutePoint(rotationCenterPoints[1].SKPoint);

                // Calculate X axis angle
                angleAxis = (float)Math.Atan(
                    (rotationCenterPoints[1].Y - rotationCenterPoints[0].Y) /
                    (rotationCenterPoints[1].X - rotationCenterPoints[0].X));
            }
            else
            {
                rotationCenterPoints = new Point3D[] {
                    new Point3D(rotationCenter.X, rotationCenter.Y, zPosition),
                    new Point3D(rotationCenter.X, rotationCenter.Y - 20f, zPosition) };

                // Transform coordinates of text rotation point
                ChartArea.matrix3D.TransformPoints(rotationCenterPoints);
                rotationCenter = rotationCenterPoints[0].SKPoint;

                // Get absolute coordinates
                rotationCenterPoints[0].SKPoint = graph.GetAbsolutePoint(rotationCenterPoints[0].SKPoint);
                rotationCenterPoints[1].SKPoint = graph.GetAbsolutePoint(rotationCenterPoints[1].SKPoint);

                // Calculate Y axis angle
                if (rotationCenterPoints[1].Y != rotationCenterPoints[0].Y)
                {
                    angleAxis = -(float)Math.Atan(
                        (rotationCenterPoints[1].X - rotationCenterPoints[0].X) /
                        (rotationCenterPoints[1].Y - rotationCenterPoints[0].Y));
                }
            }
            angle += (int)Math.Round(angleAxis * 180f / (float)Math.PI);

            // Calculate title center offset from the axis line
            float offset = labelSize + markSize + titleSize / 2f;
            float dX = 0f, dY = 0f;

            // Adjust center of title with labels, marker and title size
            if (AxisPosition == AxisPosition.Left)
            {
                dX = (float)(offset * Math.Cos(angleAxis));
                rotationCenter.X -= dX;
            }
            else if (AxisPosition == AxisPosition.Right)
            {
                dX = (float)(offset * Math.Cos(angleAxis));
                rotationCenter.X += dX;
            }
            else if (AxisPosition == AxisPosition.Top)
            {
                dY = (float)(offset * Math.Cos(angleAxis));
                dX = (float)(offset * Math.Sin(angleAxis));
                rotationCenter.Y -= dY;
                if (dY > 0)
                {
                    rotationCenter.X += dX;
                }
                else
                {
                    rotationCenter.X -= dX;
                }
            }
            else if (AxisPosition == AxisPosition.Bottom)
            {
                dY = (float)(offset * Math.Cos(angleAxis));
                dX = (float)(offset * Math.Sin(angleAxis));
                rotationCenter.Y += dY;
                if (dY > 0)
                {
                    rotationCenter.X -= dX;
                }
                else
                {
                    rotationCenter.X += dX;
                }
            }

            // Always align text in the center
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            // SQL VSTS Fix #259954, Dev10: 591135 Windows 7 crashes on empty transformation.
            if (rotationCenter.IsEmpty || float.IsNaN(rotationCenter.X) || float.IsNaN(rotationCenter.Y))
            {
                return;
            }

            // Draw 3D title
            using (SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = TitleForeColor })
            {
                graph.DrawStringRel(
                    axisTitle.Replace("\\n", "\n"),
                    TitleFont,
                    brush,
                    rotationCenter,
                    format,
                    angle,
                    GetTextOrientation());
            }

            // Add hot region
            if (Common.ProcessModeRegions)
            {
                using SKPath hotPath = graph.GetTranformedTextRectPath(rotationCenter, realTitleSize, angle);
                Common.HotRegionsList.AddHotRegion(hotPath, false, ChartElementType.AxisTitle, this);
            }
        }

        /// <summary>
        /// Select Axis line
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void DrawAxisLine(ChartGraphics graph, bool backElements)
        {
            Axis opositeAxis;
            ArrowOrientation arrowOrientation = ArrowOrientation.Top;
            SKPoint first = SKPoint.Empty;
            SKPoint second = SKPoint.Empty;

            // Set the position of axis
            switch (AxisPosition)
            {
                case AxisPosition.Left:
                case AxisPosition.Right:

                    first.X = (float)GetAxisPosition();
                    first.Y = PlotAreaPosition.Bottom;
                    second.X = (float)GetAxisPosition();
                    second.Y = PlotAreaPosition.Y;
                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;

                case AxisPosition.Bottom:
                case AxisPosition.Top:

                    first.X = PlotAreaPosition.X;
                    first.Y = (float)GetAxisPosition();
                    second.X = PlotAreaPosition.Right;
                    second.Y = (float)GetAxisPosition();
                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;
            }

            // Update axis line position for circular area
            if (ChartArea.chartAreaIsCurcular)
            {
                first.Y = PlotAreaPosition.Y + PlotAreaPosition.Height / 2f;
            }

            if (Common.ProcessModePaint)
            {
                if (!ChartArea.Area3DStyle.Enable3D || ChartArea.chartAreaIsCurcular)
                {
                    var sm = graph.SmoothingMode;
                    graph.SmoothingMode = SmoothingMode.None;
                    graph.DrawLineRel(_lineColor, _lineWidth, _lineDashStyle, first, second);
                    graph.SmoothingMode = sm;

                    // Opposite axis. Arrow uses this axis to find
                    // a shift from Common.Chart area border. This shift
                    // depend on Tick mark size.
                    opositeAxis = arrowOrientation switch
                    {
                        ArrowOrientation.Left => ChartArea.AxisX,
                        ArrowOrientation.Right => ChartArea.AxisX2,
                        ArrowOrientation.Top => ChartArea.AxisY2,
                        ArrowOrientation.Bottom => ChartArea.AxisY,
                        _ => ChartArea.AxisX,
                    };

                    // Draw arrow
                    SKPoint arrowPosition;
                    if (isReversed)
                        arrowPosition = first;
                    else
                        arrowPosition = second;

                    // Draw Arrow
                    graph.DrawArrowRel(arrowPosition, arrowOrientation, _arrowStyle, _lineColor, _lineWidth, _lineDashStyle, opositeAxis.majorTickMark.Size, _lineWidth);
                }
                else
                {
                    Draw3DAxisLine(graph, first, second, (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom), backElements);
                }
            }
        }

        /// <summary>
        /// Draws the axis line hot region.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="backElements">set to <c>true</c> if we draw back elements.</param>
        private void DrawAxisLineHotRegion(ChartGraphics graph, bool backElements)
        {
            //VSTS #229835: During the 3D rendering the axis is drawn twice:
            //1. In PrePaint() both axis and backelements (labels) are drawn.
            //2. In Paint() the axis is redrawn without labels and as a result it creates a second hot region which covered the labels' hotregions.
            //In order to avoid this we have to suppress the hotregion drawing in the Paint using the backElements flag (it's false during the Paint)
            //The circular charts and 2D charts are drawn only once in Paint() so we draw the hot regions.
            if (Common.ProcessModeRegions && (backElements || !ChartArea.Area3DStyle.Enable3D || ChartArea.chartAreaIsCurcular))
            {
                DrawAxisLineHotRegion(graph);
            }
        }

        /// <summary>
        /// Adds the axis hot region
        /// </summary>
        /// <param name="graph">The chart graphics instance.</param>
        private void DrawAxisLineHotRegion(ChartGraphics graph)
        {
            using SKPath path = new();
            // Find the topLeft(first) and bottomRight(second) points of the hotregion rectangle
            SKPoint first = SKPoint.Empty;
            SKPoint second = SKPoint.Empty;
            float axisPosition = (float)GetAxisPosition();

            switch (AxisPosition)
            {
                case AxisPosition.Left:
                    first.X = axisPosition - (labelSize + markSize);
                    first.Y = PlotAreaPosition.Y;
                    second.X = axisPosition;
                    second.Y = PlotAreaPosition.Bottom;
                    break;

                case AxisPosition.Right:
                    first.X = axisPosition;
                    first.Y = PlotAreaPosition.Y;
                    second.X = axisPosition + labelSize + markSize;
                    second.Y = PlotAreaPosition.Bottom;
                    break;

                case AxisPosition.Bottom:
                    first.X = PlotAreaPosition.X;
                    first.Y = axisPosition;
                    second.X = PlotAreaPosition.Right;
                    second.Y = axisPosition + labelSize + markSize;
                    break;

                case AxisPosition.Top:
                    first.X = PlotAreaPosition.X;
                    first.Y = axisPosition - (labelSize + markSize);
                    second.X = PlotAreaPosition.Right;
                    second.Y = axisPosition;
                    break;
            }

            // Update axis line position for circular area
            if (ChartArea.chartAreaIsCurcular)
            {
                second.Y = PlotAreaPosition.Y + PlotAreaPosition.Height / 2f;
            }

            // Create rectangle and inflate it
            SKRect rect = new(first.X, first.Y, second.X - first.X, second.Y - first.Y);
            SKSize size = graph.GetRelativeSize(new SKSize(3, 3));

            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                rect.Inflate(2, size.Height);
            }
            else
            {
                rect.Inflate(size.Width, 2);
            }

            // Get the rectangle points
            SKPoint[] points = new SKPoint[] {
                    new SKPoint(rect.Left, rect.Top),
                    new SKPoint(rect.Right, rect.Top),
                    new SKPoint(rect.Right, rect.Bottom),
                    new SKPoint(rect.Left, rect.Bottom)};

            // If we are dealing with the 3D - transform the rectangle
            if (ChartArea.Area3DStyle.Enable3D && !ChartArea.chartAreaIsCurcular)
            {
                float zPositon = GetMarksZPosition(out _);

                // Convert points to 3D
                Point3D[] points3D = new Point3D[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points3D[i] = new Point3D(points[i].X, points[i].Y, zPositon);
                }

                // Transform
                ChartArea.matrix3D.TransformPoints(points3D);

                // Convert to 2D
                for (int i = 0; i < points3D.Length; i++)
                {
                    points[i] = points3D[i].SKPoint;
                }
            }

            // Transform points to absolute cooordinates
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = graph.GetAbsolutePoint(points[i]);
            }

            // Add the points to the path
            path.AddPoly(points);

            Common.HotRegionsList.AddHotRegion(
                graph,
                path,
                false,
                ToolTip,
                string.Empty,
                string.Empty,
                string.Empty,
                this,
                ChartElementType.Axis);
        }

        /// <summary>
        /// Draws axis line in 3D space.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="point1">First line point.</param>
        /// <param name="point2">Second line point.</param>
        /// <param name="horizontal">Indicates that tick mark line is horizontal</param>
        /// <param name="backElements">Only back elements of axis should be drawn.</param>
        private void Draw3DAxisLine(
            ChartGraphics graph,
            SKPoint point1,
            SKPoint point2,
            bool horizontal,
            bool backElements
            )
        {
            // Check if axis is positioned on the plot area adge
            bool onEdge = IsAxisOnAreaEdge;

            // Check if axis tick marks are drawn inside plotting area
            bool tickMarksOnEdge = onEdge;
            if (tickMarksOnEdge &&
                MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea ||
                MinorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                MinorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
            {
                tickMarksOnEdge = false;
            }

            // Make sure first point of axis coordinates has smaller values
            if ((horizontal && point1.X > point2.X) ||
                (!horizontal && point1.Y > point2.Y))
            {
                SKPoint tempPoint = new(point1.X, point1.Y);
                point1.X = point2.X;
                point1.Y = point2.Y;
                point2 = tempPoint;
            }

            // Check if the front/back wall is on the top drawing layer
            float zPositon = ChartArea.IsMainSceneWallOnFront() ? ChartArea.areaSceneDepth : 0f;
            _ = ChartArea.IsMainSceneWallOnFront() ? SurfaceNames.Front : SurfaceNames.Back;
            if (ChartArea.ShouldDrawOnSurface(SurfaceNames.Back, backElements, tickMarksOnEdge))
            {
                // Draw axis line on the back/front wall
                graph.Draw3DLine(
                    ChartArea.matrix3D,
                    _lineColor, _lineWidth, _lineDashStyle,
                    new Point3D(point1.X, point1.Y, zPositon),
                    new Point3D(point2.X, point2.Y, zPositon),
                    Common,
                    this,
                    ChartElementType.Nothing
                    );
            }

            // Check if the back wall is on the top drawing layer
            zPositon = ChartArea.IsMainSceneWallOnFront() ? 0f : ChartArea.areaSceneDepth;
            SurfaceNames surfName = ChartArea.IsMainSceneWallOnFront() ? SurfaceNames.Back : SurfaceNames.Front;
            // Draw axis line on the front wall
            if (ChartArea.ShouldDrawOnSurface(surfName, backElements, tickMarksOnEdge) && (!onEdge ||
                    (AxisPosition == AxisPosition.Bottom && ChartArea.IsBottomSceneWallVisible()) ||
                    (AxisPosition == AxisPosition.Left && ChartArea.IsSideSceneWallOnLeft()) ||
                    (AxisPosition == AxisPosition.Right && !ChartArea.IsSideSceneWallOnLeft())))
            {
                graph.Draw3DLine(
                    ChartArea.matrix3D,
                    _lineColor, _lineWidth, _lineDashStyle,
                    new Point3D(point1.X, point1.Y, zPositon),
                    new Point3D(point2.X, point2.Y, zPositon),
                    Common,
                    this,
                    ChartElementType.Nothing
                    );
            }

            // Check if the left/top wall is on the top drawing layer
            SurfaceNames surfaceName = (AxisPosition == AxisPosition.Left || AxisPosition == AxisPosition.Right) ? SurfaceNames.Top : SurfaceNames.Left;
            // Draw axis line on the left/top side walls
            if (ChartArea.ShouldDrawOnSurface(surfaceName, backElements, tickMarksOnEdge) && (!onEdge ||
                    (AxisPosition == AxisPosition.Bottom && (ChartArea.IsBottomSceneWallVisible() || ChartArea.IsSideSceneWallOnLeft())) ||
                    (AxisPosition == AxisPosition.Left && ChartArea.IsSideSceneWallOnLeft()) ||
                    (AxisPosition == AxisPosition.Right && !ChartArea.IsSideSceneWallOnLeft()) ||
                    (AxisPosition == AxisPosition.Top && ChartArea.IsSideSceneWallOnLeft())))
            {
                graph.Draw3DLine(
                    ChartArea.matrix3D,
                    _lineColor, _lineWidth, _lineDashStyle,
                    new Point3D(point1.X, point1.Y, ChartArea.areaSceneDepth),
                    new Point3D(point1.X, point1.Y, 0f),
                    Common,
                    this,
                    ChartElementType.Nothing
                );
            }

            // Check if the right/bottom wall is on the top drawing layer
            surfaceName = (AxisPosition == AxisPosition.Left || AxisPosition == AxisPosition.Right) ? SurfaceNames.Bottom : SurfaceNames.Right;
            if (ChartArea.ShouldDrawOnSurface(surfaceName, backElements, tickMarksOnEdge) &&
                (!onEdge ||
                    (AxisPosition == AxisPosition.Bottom && (ChartArea.IsBottomSceneWallVisible() || !ChartArea.IsSideSceneWallOnLeft())) ||
                    (AxisPosition == AxisPosition.Left && (ChartArea.IsSideSceneWallOnLeft() || ChartArea.IsBottomSceneWallVisible())) ||
                    (AxisPosition == AxisPosition.Right && (!ChartArea.IsSideSceneWallOnLeft() || ChartArea.IsBottomSceneWallVisible())) ||
                    (AxisPosition == AxisPosition.Top && !ChartArea.IsSideSceneWallOnLeft()))
)
            {
                graph.Draw3DLine(
                    ChartArea.matrix3D,
                    _lineColor, _lineWidth, _lineDashStyle,
                    new Point3D(point2.X, point2.Y, ChartArea.areaSceneDepth),
                    new Point3D(point2.X, point2.Y, 0f),
                    Common,
                    this,
                    ChartElementType.Nothing
                    );
            }
        }

        /// <summary>
        /// Gets Z position of axis tick marks and labels.
        /// </summary>
        /// <param name="axisOnEdge">Returns true if axis is on the edge.</param>
        /// <returns>Marks Z position.</returns>
        internal float GetMarksZPosition(out bool axisOnEdge)
        {
            axisOnEdge = IsAxisOnAreaEdge;
            if (!GetIsMarksNextToAxis())
            {
                // Marks are forced to be on the area edge
                axisOnEdge = true;
            }
            float wallZPosition = 0f;
            if (AxisPosition == AxisPosition.Bottom && (ChartArea.IsBottomSceneWallVisible() || !axisOnEdge))
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }
            if (AxisPosition == AxisPosition.Left && (ChartArea.IsSideSceneWallOnLeft() || !axisOnEdge))
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }
            if (AxisPosition == AxisPosition.Right && (!ChartArea.IsSideSceneWallOnLeft() || !axisOnEdge))
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }
            if (AxisPosition == AxisPosition.Top && !axisOnEdge)
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }

            // Check if front wall is shown
            if (ChartArea.IsMainSceneWallOnFront())
            {
                // Switch Z position of tick mark
                wallZPosition = (wallZPosition == 0f) ? ChartArea.areaSceneDepth : 0f;
            }

            return wallZPosition;
        }

        /// <summary>
        /// Paint Axis Grid lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void PaintGrids(ChartGraphics graph)
        {
            PaintGrids(graph, out _);
        }

        /// <summary>
        /// Paint Axis Grid lines or
        /// hit test function for grid lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        /// <param name="obj">Returns selected grid object</param>
        internal void PaintGrids(ChartGraphics graph, out object obj)
        {
            obj = null;

            // Axis is disabled
            if (!enabled)
                return;

            // Paint Minor grid lines
            minorGrid.Paint(graph);

            // Paint Major grid lines
            majorGrid.Paint(graph);
        }

        /// <summary>
        /// Paint Axis Strip lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        /// <param name="drawLinesOnly">Indicates if Lines or Stripes should be drawn.</param>
        internal void PaintStrips(ChartGraphics graph, bool drawLinesOnly)
        {
            PaintStrips(graph, false, 0, 0, out _, drawLinesOnly);
        }

        /// <summary>
        /// Paint Axis Strip lines or
        /// hit test function for Strip lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        /// <param name="selectionMode">The selection mode is active</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="obj">Returns selected grid object</param>
        /// <param name="drawLinesOnly">Indicates if Lines or Stripes should be drawn.</param>
        internal void PaintStrips(ChartGraphics graph, bool selectionMode, int x, int y, out object obj, bool drawLinesOnly)
        {
            obj = null;

            // Axis is disabled
            if (!enabled)
                return;

            // Add axis isInterlaced strip lines into the collection
            bool interlacedStripAdded = AddInterlacedStrip();

            // Draw axis strips and lines
            foreach (StripLine strip in StripLines)
            {
                strip.Paint(graph, Common, drawLinesOnly);
            }

            // Remove axis isInterlaced strip line from the collection after drawing
            if (interlacedStripAdded)
            {
                // Remove isInterlaced strips which always is the first strip line
                StripLines.RemoveAt(0);
            }
        }

        /// <summary>
        /// Helper function which adds temp. strip lines into the collection
        /// to display isInterlaced lines in axis.
        /// </summary>
        private bool AddInterlacedStrip()
        {
            bool addStrip = false;
            if (IsInterlaced)
            {
                StripLine stripLine = new();
                stripLine.interlaced = true;
                // VSTS fix of 164115 IsInterlaced StripLines with no border are rendered with black border, regression of VSTS 136763
                stripLine.BorderColor = SKColor.Empty;

                // Get interval from grid lines, tick marks or labels
                if (MajorGrid.Enabled && MajorGrid.GetInterval() != 0.0)
                {
                    addStrip = true;
                    stripLine.Interval = MajorGrid.GetInterval() * 2.0;
                    stripLine.IntervalType = MajorGrid.GetIntervalType();
                    stripLine.IntervalOffset = MajorGrid.GetIntervalOffset();
                    stripLine.IntervalOffsetType = MajorGrid.GetIntervalOffsetType();
                    stripLine.StripWidth = MajorGrid.GetInterval();
                    stripLine.StripWidthType = MajorGrid.GetIntervalType();
                }
                else if (MajorTickMark.Enabled && MajorTickMark.GetInterval() != 0.0)
                {
                    addStrip = true;
                    stripLine.Interval = MajorTickMark.GetInterval() * 2.0;
                    stripLine.IntervalType = MajorTickMark.GetIntervalType();
                    stripLine.IntervalOffset = MajorTickMark.GetIntervalOffset();
                    stripLine.IntervalOffsetType = MajorTickMark.GetIntervalOffsetType();
                    stripLine.StripWidth = MajorTickMark.GetInterval();
                    stripLine.StripWidthType = MajorTickMark.GetIntervalType();
                }
                else if (LabelStyle.Enabled && LabelStyle.GetInterval() != 0.0)
                {
                    addStrip = true;
                    stripLine.Interval = LabelStyle.GetInterval() * 2.0;
                    stripLine.IntervalType = LabelStyle.GetIntervalType();
                    stripLine.IntervalOffset = LabelStyle.GetIntervalOffset();
                    stripLine.IntervalOffsetType = LabelStyle.GetIntervalOffsetType();
                    stripLine.StripWidth = LabelStyle.GetInterval();
                    stripLine.StripWidthType = LabelStyle.GetIntervalType();
                }

                // Insert item into the strips collection
                if (addStrip)
                {
                    // Define stip color
                    if (InterlacedColor != SKColor.Empty)
                    {
                        stripLine.BackColor = InterlacedColor;
                    }
                    else
                    {
                        // If isInterlaced strips color is not set - use darker color of the area
                        if (ChartArea.BackColor == SKColor.Empty)
                        {
                            stripLine.BackColor = (ChartArea.Area3DStyle.Enable3D) ? SKColors.DarkGray : SKColors.LightGray;
                        }
                        else if (ChartArea.BackColor == SKColors.Transparent)
                        {
                            if (Common.Chart.BackColor != SKColors.Transparent && Common.Chart.BackColor != SKColors.Black)
                            {
                                stripLine.BackColor = ChartGraphics.GetGradientColor(Common.Chart.BackColor, SKColors.Black, 0.2);
                            }
                            else
                            {
                                stripLine.BackColor = SKColors.LightGray;
                            }
                        }
                        else
                        {
                            stripLine.BackColor = ChartGraphics.GetGradientColor(ChartArea.BackColor, SKColors.Black, 0.2);
                        }
                    }

                    // Insert strip
                    StripLines.Insert(0, stripLine);
                }
            }

            return addStrip;
        }

        #endregion

        #region Axis parameters recalculation and resizing methods

        /// <summary>
        /// This method will calculate the maximum and minimum values
        /// using interval on the X axis automatically. It will make a gap between
        /// data points and border of the Common.Chart area.
        /// Note that this method can only be called for primary or secondary X axes.
        /// </summary>
        public void RoundAxisValues()
        {
            roundedXValues = true;
        }

        /// <summary>
        /// RecalculateAxesScale axis.
        /// </summary>
        /// <param name="position">Plotting area position.</param>
        internal void ReCalc(ElementPosition position)
        {
            PlotAreaPosition = position;

#if SUBAXES

			// Recalculate all sub-axis
			foreach(SubAxis subAxis in this.SubAxes)
			{
				subAxis.ReCalc( position );
			}
#endif // SUBAXES
        }

        /// <summary>
        /// This method store Axis values as minimum, maximum,
        /// crossing, etc. Axis auto algorithm changes these
        /// values and they have to be set to default values
        /// after painting.
        /// </summary>
        internal void StoreAxisValues()
        {
            tempLabels = new CustomLabelsCollection(this);
            foreach (CustomLabel label in CustomLabels)
            {
                tempLabels.Add(label.Clone());
            }

            paintMode = true;

            // This field synchronies the Storing and
            // resetting of temporary values
            if (_storeValuesEnabled)
            {
                tempMaximum = maximum;
                tempMinimum = minimum;
                tempCrossing = crossing;
                tempAutoMinimum = _autoMinimum;
                tempAutoMaximum = _autoMaximum;

                tempMajorGridInterval = majorGrid.interval;
                tempMajorTickMarkInterval = majorTickMark.interval;

                tempMinorGridInterval = minorGrid.interval;
                tempMinorTickMarkInterval = minorTickMark.interval;

                tempGridIntervalType = majorGrid.intervalType;
                tempTickMarkIntervalType = majorTickMark.intervalType;

                tempLabelInterval = labelStyle.interval;
                tempLabelIntervalType = labelStyle.intervalType;

                // Remember original ScaleView Position
                _originalViewPosition = ScaleView.Position;

                // This field synchronies the Storing and
                // resetting of temporary values
                _storeValuesEnabled = false;
            }

#if SUBAXES

			// Store values of all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.StoreAxisValues( );
				}
			}
#endif // SUBAXES
        }

        /// <summary>
        /// This method reset Axis values as minimum, maximum,
        /// crossing, etc. Axis auto algorithm changes these
        /// values and they have to be set to default values
        /// after painting.
        /// </summary>
        internal void ResetAxisValues()
        {
            // Paint mode is finished
            paintMode = false;

            // Reset back original custom labels
            if (tempLabels != null)
            {
                CustomLabels.Clear();
                foreach (CustomLabel label in tempLabels)
                {
                    CustomLabels.Add(label.Clone());
                }

                tempLabels = null;
            }

#if SUBAXES

			// Reset values of all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.ResetAxisValues( );
				}
			}
#endif // SUBAXES
        }

        /// <summary>
        /// Reset auto calculated axis values
        /// </summary>
        internal void ResetAutoValues()
        {
            refreshMinMaxFromData = true;
            maximum = tempMaximum;
            minimum = tempMinimum;
            crossing = tempCrossing;
            _autoMinimum = tempAutoMinimum;
            _autoMaximum = tempAutoMaximum;

            majorGrid.interval = tempMajorGridInterval;
            majorTickMark.interval = tempMajorTickMarkInterval;

            minorGrid.interval = tempMinorGridInterval;
            minorTickMark.interval = tempMinorTickMarkInterval;

            labelStyle.interval = tempLabelInterval;
            majorGrid.intervalType = tempGridIntervalType;
            majorTickMark.intervalType = tempTickMarkIntervalType;
            labelStyle.intervalType = tempLabelIntervalType;

            // Restore original ScaleView Position
            if (Common.Chart != null && !Common.Chart.serializing)
            {
                ScaleView.Position = _originalViewPosition;
            }

            // This field synchronies the Storing and
            // resetting of temporary values
            _storeValuesEnabled = true;
        }

        /// <summary>
        /// Calculate size of the axis elements like title, labels and marks.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="chartAreaPosition">The Chart area position.</param>
        /// <param name="plotArea">Plotting area size.</param>
        /// <param name="axesNumber">Number of axis of the same orientation.</param>
        /// <param name="autoPlotPosition">Indicates that inner plot position is automatic.</param>
        virtual internal void Resize(
            ChartGraphics chartGraph,
            ElementPosition chartAreaPosition,
            SKRect plotArea,
            float axesNumber,
            bool autoPlotPosition)
        {
            // Disable Common.Chart invalidation
            bool oldDisableInvalidates = Common.Chart.disableInvalidates;
            Common.Chart.disableInvalidates = true;

            // Set Common.Chart area position
            PlotAreaPosition = chartAreaPosition;

            // Initialize plot area size
            PlotAreaPosition.FromSKRect(plotArea);

            //******************************************************
            //** Calculate axis title size
            //******************************************************
            titleSize = 0F;
            if (Title.Length > 0)
            {
                // Measure axis title
                SKSize titleStringSize = chartGraph.MeasureStringRel(Title.Replace("\\n", "\n"), TitleFont, new SKSize(10000f, 10000f), StringFormat.GenericTypographic, GetTextOrientation());

                // Switch Width & Heigth for vertical axes
                // If axis is horizontal
                float maxTitlesize;
                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                {
                    maxTitlesize = (plotArea.Height / 100F) * (maxAxisTitleSize / axesNumber);
                    if (IsTextVertical)
                    {
                        titleSize = Math.Min(titleStringSize.Width, maxTitlesize);
                    }
                    else
                    {
                        titleSize = Math.Min(titleStringSize.Height, maxTitlesize);
                    }
                }
                // If axis is vertical
                else
                {
                    titleStringSize = chartGraph.GetAbsoluteSize(titleStringSize);
                    titleStringSize = chartGraph.GetRelativeSize(new SKSize(titleStringSize.Height, titleStringSize.Width));
                    maxTitlesize = (plotArea.Width / 100F) * (maxAxisTitleSize / axesNumber);
                    if (IsTextVertical)
                    {
                        titleSize = Math.Min(titleStringSize.Width, maxTitlesize);
                    }
                    else
                    {
                        titleSize = Math.Min(titleStringSize.Height, maxTitlesize);
                    }
                }
            }
            if (titleSize > 0)
            {
                titleSize += elementSpacing;
            }

            SKSize arrowSizePrimary = SKSize.Empty;
            SKSize arrowSizeSecondary = SKSize.Empty;
            ArrowOrientation arrowOrientation;
            if (axisType == AxisName.X || axisType == AxisName.X2)
            {
                if (ChartArea.AxisY.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizePrimary = ChartArea.AxisY.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, AxisPosition))
                    {
                        arrowSizePrimary = SKSize.Empty;
                    }
                }

                if (ChartArea.AxisY2.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizeSecondary = ChartArea.AxisY2.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, AxisPosition))
                    {
                        arrowSizeSecondary = SKSize.Empty;
                    }
                }
            }
            else
            {
                if (ChartArea.AxisX.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizePrimary = ChartArea.AxisX.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, AxisPosition))
                    {
                        arrowSizePrimary = SKSize.Empty;
                    }
                }

                if (ChartArea.AxisX2.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizeSecondary = ChartArea.AxisX2.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, AxisPosition))
                    {
                        arrowSizeSecondary = SKSize.Empty;
                    }
                }
            }


            //*********************************************************
            //** Get arrow size of the opposite axis
            //*********************************************************
            float arrowSize;
            // If axis is horizontal
            if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
            {
                arrowSize = Math.Max(arrowSizePrimary.Height, arrowSizeSecondary.Height);
            }
            // If axis is vertical
            else
            {
                arrowSize = Math.Max(arrowSizePrimary.Width, arrowSizeSecondary.Width);
            }

            //*********************************************************
            //** Calculate axis tick marks, axis thickness, arrow size
            //** and scroll bar size
            //*********************************************************
            markSize = 0F;

            // Get major and minor tick marks sizes
            float majorTickSize = 0;
            if (MajorTickMark.Enabled && MajorTickMark.TickMarkStyle != TickMarkStyle.None)
            {
                if (MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
                {
                    majorTickSize = 0F;
                }
                else if (MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis)
                {
                    majorTickSize = MajorTickMark.Size / 2F;
                }
                else if (MajorTickMark.TickMarkStyle == TickMarkStyle.OutsideArea)
                {
                    majorTickSize = MajorTickMark.Size;
                }
            }

            float minorTickSize = 0;
            if (MinorTickMark.Enabled && MinorTickMark.TickMarkStyle != TickMarkStyle.None && MinorTickMark.GetInterval() != 0)
            {
                if (MinorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
                {
                    minorTickSize = 0F;
                }
                else if (MinorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis)
                {
                    minorTickSize = MinorTickMark.Size / 2F;
                }
                else if (MinorTickMark.TickMarkStyle == TickMarkStyle.OutsideArea)
                {
                    minorTickSize = MinorTickMark.Size;
                }
            }

            markSize += Math.Max(majorTickSize, minorTickSize);

            // Add axis line size
            SKSize borderSize = chartGraph.GetRelativeSize(new SKSize(LineWidth, LineWidth));

            // If axis is horizontal
            if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
            {
                markSize += borderSize.Height / 2f;
                markSize = Math.Min(markSize, (plotArea.Height / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }
            // If axis is vertical
            else
            {
                markSize += borderSize.Width / 2f;
                markSize = Math.Min(markSize, (plotArea.Width / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }

            //*********************************************************
            //** Adjust mark size using area scene wall width
            //*********************************************************
            if (ChartArea.Area3DStyle.Enable3D &&
                !ChartArea.chartAreaIsCurcular &&
                ChartArea.BackColor != SKColors.Transparent &&
                ChartArea.Area3DStyle.WallWidth > 0)
            {
                SKSize areaWallSize = chartGraph.GetRelativeSize(new SKSize(ChartArea.Area3DStyle.WallWidth, ChartArea.Area3DStyle.WallWidth));
                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                {
                    markSize += areaWallSize.Height;
                }
                else
                {
                    markSize += areaWallSize.Width;
                }

                // Ignore Max marks size for the 3D wall size.
                //this.markSize = Math.Min(this.markSize, (plotArea.Width / 100F) * (Axis.maxAxisMarkSize / axesNumber))
            }

            //*********************************************************
            //** Adjust title size and mark size using arrow size
            //*********************************************************
            if (arrowSize > (markSize + scrollBarSize + titleSize))
            {
                markSize = Math.Max(markSize, arrowSize - (markSize + scrollBarSize + titleSize));
                markSize = Math.Min(markSize, (plotArea.Width / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }

            //*********************************************************
            //** Calculate max label size
            //*********************************************************
            float maxLabelSize = 0;

            if (!autoPlotPosition)
            {
                if (GetIsMarksNextToAxis())
                {
                    if (AxisPosition == AxisPosition.Top)
                        maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.Y;
                    else if (AxisPosition == AxisPosition.Bottom)
                        maxLabelSize = ChartArea.Position.Bottom - (float)GetAxisPosition();
                    if (AxisPosition == AxisPosition.Left)
                        maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.X;
                    else if (AxisPosition == AxisPosition.Right)
                        maxLabelSize = ChartArea.Position.Right - (float)GetAxisPosition();
                }
                else
                {
                    if (AxisPosition == AxisPosition.Top)
                        maxLabelSize = plotArea.Top - ChartArea.Position.Y;
                    else if (AxisPosition == AxisPosition.Bottom)
                        maxLabelSize = ChartArea.Position.Bottom - plotArea.Bottom;
                    if (AxisPosition == AxisPosition.Left)
                        maxLabelSize = plotArea.Left - ChartArea.Position.X;
                    else if (AxisPosition == AxisPosition.Right)
                        maxLabelSize = ChartArea.Position.Right - plotArea.Right;
                }

                maxLabelSize *= 2F;
            }
            else
            {
                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                    maxLabelSize = plotArea.Height * (_maximumAutoSize / 100f);
                else
                    maxLabelSize = plotArea.Width * (_maximumAutoSize / 100f);
            }

            //******************************************************
            //** First try to select the interval that will
            //** generate best fit labels.
            //******************************************************

            // Make sure the variable interval mode is enabled and
            // no custom label interval used.
            if (Enabled != AxisEnabled.False &&
                LabelStyle.Enabled &&
                IsVariableLabelCountModeEnabled())
            {
                // Increase font by several points when height of the font is the most important
                // dimension. Use original size whenwidth is the most important size.
                float extraSize = 3f;
                if ((AxisPosition == AxisPosition.Left || AxisPosition == AxisPosition.Right) &&
                    (LabelStyle.Angle == 90 || LabelStyle.Angle == -90))
                {
                    extraSize = 0f;
                }
                if ((AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom) &&
                    (LabelStyle.Angle == 180 || LabelStyle.Angle == 0))
                {
                    extraSize = 0f;
                }

                // If 3D Common.Chart is used make the measurements with font several point larger
                if (ChartArea.Area3DStyle.Enable3D)
                {
                    extraSize += 1f;
                }

                autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(LabelStyle.Font.Typeface.FamilyName,
                    LabelStyle.Font.Size + extraSize, LabelStyle.Font.Typeface.FontStyle);

                // Reset angle and stagged flag used in the auto-fitting algorithm
                autoLabelAngle = LabelStyle.Angle;
                autoLabelOffset = (LabelStyle.IsStaggered) ? 1 : 0;

                // Adjust interval
                AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, false);
            }

            //******************************************************
            //** Automatically calculate the best font size, angle
            //** and try to use offset labels.
            //******************************************************
            // Reset all automatic label properties
            autoLabelFont = null;
            autoLabelAngle = -1000;
            autoLabelOffset = -1;

            // For circular Common.Chart area process auto-fitting for Y Axis only
            if (IsLabelAutoFit &&
                LabelAutoFitStyle != LabelAutoFitStyles.None &&
                !ChartArea.chartAreaIsCurcular)
            {
                bool fitDone = false;
                bool noWordWrap = false;

                // Set default font angle and labels offset flag
                autoLabelAngle = 0;
                autoLabelOffset = 0;

                // Original labels collection
                CustomLabelsCollection originalLabels = null;

                // Pick up maximum font size
                float size = Math.Max(LabelAutoFitMaxFontSize, LabelAutoFitMinFontSize);
                _minLabelFontSize = Math.Min(LabelAutoFitMinFontSize, LabelAutoFitMaxFontSize);
                _aveLabelFontSize = _minLabelFontSize + Math.Abs(size - _minLabelFontSize) / 2f;

                // Check if common font size should be used
                if (ChartArea.IsSameFontSKSizeorAllAxes)
                {
                    size = Math.Min(size, ChartArea.axesAutoFontSize);
                }

                //Set new font
                autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(LabelStyle.Font.Typeface.FamilyName,
                    size,
                    LabelStyle.Font.Typeface.FontStyle
                );

                // Check if we allowed to increase font size while auto-fitting
                if ((LabelAutoFitStyle & LabelAutoFitStyles.IncreaseFont) != LabelAutoFitStyles.IncreaseFont)
                {
                    // Use axis labels font as starting point
                    autoLabelFont = LabelStyle.Font;
                }

                // Loop while labels do not fit
                float spacer = 0f;
                while (!fitDone)
                {
                    //******************************************************
                    //** Check if labels fit
                    //******************************************************

                    // Check if grouping labels fit should be checked
                    bool checkLabelsFirstRowOnly = true;
                    if ((LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                    {
                        // Only check grouping labels if we can reduce fonts size
                        checkLabelsFirstRowOnly = false;
                    }

                    // Check labels fit
                    fitDone = CheckLabelsFit(
                        chartGraph,
                        markSize + scrollBarSize + titleSize + spacer,
                        autoPlotPosition,
                        checkLabelsFirstRowOnly,
                        false);

                    //******************************************************
                    //** Adjust labels text properties to fit
                    //******************************************************
                    if (!fitDone)
                    {
                        // If font is bigger than average try to make it smaller
                        if (autoLabelFont.Size >= _aveLabelFontSize &&
                            (LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                        {
                            //Clean up the old font
                            autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                autoLabelFont.Typeface.FamilyName,
                                autoLabelFont.Size - 0.5f,
                                autoLabelFont.Typeface.FontStyle);
                        }

                        // Try to use offset labels (2D charts and non-circular arae only!!!)
                        else if (!ChartArea.Area3DStyle.Enable3D &&
                            !ChartArea.chartAreaIsCurcular &&
                            originalLabels == null &&
                            autoLabelAngle == 0 &&
                            autoLabelOffset == 0 &&
                            (LabelAutoFitStyle & LabelAutoFitStyles.StaggeredLabels) == LabelAutoFitStyles.StaggeredLabels)
                        {
                            autoLabelOffset = 1;
                        }

                        // Try to insert new line characters in labels text
                        else if (!noWordWrap &&
                            (LabelAutoFitStyle & LabelAutoFitStyles.WordWrap) == LabelAutoFitStyles.WordWrap)
                        {
                            autoLabelOffset = 0;

                            // Check if backup copy of the original lables was made
                            if (originalLabels == null)
                            {
                                // Copy current labels collection
                                originalLabels = new CustomLabelsCollection(this);
                                foreach (CustomLabel label in CustomLabels)
                                {
                                    originalLabels.Add(label.Clone());
                                }
                            }

                            // Try to insert new line character into the longest label
                            bool changed = WordWrapLongestLabel(CustomLabels);

                            // Word wrapping do not solve the labels overlapping issue
                            if (!changed)
                            {
                                noWordWrap = true;

                                // Restore original labels
                                if (originalLabels != null)
                                {
                                    CustomLabels.Clear();
                                    foreach (CustomLabel label in originalLabels)
                                    {
                                        CustomLabels.Add(label.Clone());
                                    }

                                    originalLabels = null;
                                }

                                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                                {
                                    if ((spacer == 0 ||
                                        spacer == 30f ||
                                        spacer == 20f) &&
                                        ((LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30 ||
                                        (LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45 ||
                                        (LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90))
                                    {
                                        // Try to use 90 degrees angle
                                        autoLabelAngle = 90;
                                        noWordWrap = false;

                                        // Usually 55% of Common.Chart area size is allowed for labels
                                        // Reduce that space.
                                        if (spacer == 0f)
                                        {
                                            // 30
                                            spacer = 30f;
                                        }
                                        else if (spacer == 30f)
                                        {
                                            // 20
                                            spacer = 20f;
                                        }
                                        else if (spacer == 20f)
                                        {
                                            // 5
                                            spacer = 5f;
                                        }
                                        else
                                        {
                                            autoLabelAngle = 0;
                                            noWordWrap = true;
                                        }
                                    }
                                    else
                                    {
                                        spacer = 0f;
                                    }
                                }
                            }
                        }

                        // Try to change font angle
                        else if (autoLabelAngle != 90 &&
                            ((LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30 ||
                            (LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45 ||
                            (LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90))
                        {
                            spacer = 0f;
                            autoLabelOffset = 0;

                            if ((LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30)
                            {
                                // Increase angle by 45 degrees in 2D and 45 in 3D
                                autoLabelAngle += (ChartArea.Area3DStyle.Enable3D) ? 45 : 30;
                            }
                            else if ((LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45)
                            {
                                // Increase angle by 45 degrees
                                autoLabelAngle += 45;
                            }
                            else if ((LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90)
                            {
                                // Increase angle by 90 degrees
                                autoLabelAngle += 90;
                            }
                        }

                        // Try to reduce font again
                        else if (autoLabelFont.Size > _minLabelFontSize &&
                            (LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                        {
                            //Clean up the old font
                            autoLabelAngle = 0;
                            autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                autoLabelFont.Typeface.FamilyName,
                                autoLabelFont.Size - 0.5f,
                                autoLabelFont.Typeface.FontStyle);
                        }

                        // Failed to fit
                        else
                        {
                            // Use last font
                            if ((LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30 ||
                                (LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45 ||
                                (LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90)
                            {
                                // Reset angle
                                if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                                {
                                    autoLabelAngle = 90;
                                }
                                else
                                {
                                    autoLabelAngle = 0;
                                }
                            }
                            if ((LabelAutoFitStyle & LabelAutoFitStyles.StaggeredLabels) == LabelAutoFitStyles.StaggeredLabels)
                            {
                                // Reset offset labels
                                autoLabelOffset = 0;
                            }
                            fitDone = true;
                        }
                    }
                    else if (ChartArea.Area3DStyle.Enable3D &&
                        !ChartArea.chartAreaIsCurcular &&
                        autoLabelFont.Size > _minLabelFontSize)
                    {
                        // Reduce auto-fit font by 1 for the 3D charts
                        autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                            autoLabelFont.Typeface.FamilyName,
                            autoLabelFont.Size - 0.5f,
                            autoLabelFont.Typeface.FontStyle);
                    }
                }

                // Change the auto-fit angle for top and bottom axes from 90 to -90
                if ((AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top) && autoLabelAngle == 90)
                {
                    autoLabelAngle = -90;
                }
            }

            //*********************************************************
            //** Calculate overall labels size
            //*********************************************************
            labelSize = 0;

            // if labels are not enabled their size needs to remain zero
            if (LabelStyle.Enabled)
            {
                //******************************************************
                //** Calculate axis second labels row size
                //******************************************************
                labelSize = (maxAxisElementsSize) - markSize - scrollBarSize - titleSize;
                if (labelSize > 0)
                {
                    groupingLabelSizes = GetRequiredGroupLabelSize(chartGraph, (maxLabelSize / 100F) * maxAxisLabelRow2Size);
                    totlaGroupingLabelsSize = GetGroupLablesToatalSize();
                }

                //******************************************************
                //** Calculate axis labels size
                //******************************************************
                labelSize -= totlaGroupingLabelsSize;
                if (labelSize > 0)
                {
                    labelSize = elementSpacing + GetRequiredLabelSize(chartGraph,
                        (maxLabelSize / 100F) * (maxAxisElementsSize - markSize - scrollBarSize - titleSize), out unRotatedLabelSize);

                    if (!LabelStyle.Enabled)
                    {
                        labelSize -= elementSpacing;
                    }
                }
                else
                {
                    labelSize = 0;
                }

                labelSize += totlaGroupingLabelsSize;
            }

            // Restore previous invalidation flag
            Common.Chart.disableInvalidates = oldDisableInvalidates;
        }

        /// <summary>
        /// Calculates axis interval so that labels will fit most efficiently.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="autoPlotPosition">True if plot position is auto calculated.</param>
        /// <param name="onlyIncreaseInterval">True if interval should only be increased.</param>
        private void AdjustIntervalToFitLabels(ChartGraphics chartGraph, bool autoPlotPosition, bool onlyIncreaseInterval)
        {
            // Calculates axis interval so that labels will fit most efficiently.
            if (ScaleSegments.Count == 0)
            {
                AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, null, onlyIncreaseInterval);
            }
            else
            {
                // Allow values to go outside the segment boundary
                ScaleSegments.AllowOutOfScaleValues = true;

                // Adjust interval of each segment first
                foreach (AxisScaleSegment axisScaleSegment in ScaleSegments)
                {
                    AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, axisScaleSegment, onlyIncreaseInterval);
                }

                // Fill labels using new segment intervals
                bool removeLabels = true;
                int segmentIndex = 0;
                ArrayList removedLabels = new();
                ArrayList removedLabelsIndexes = new();
                foreach (AxisScaleSegment scaleSegment in ScaleSegments)
                {
                    scaleSegment.SetTempAxisScaleAndInterval();
                    FillLabels(removeLabels);
                    removeLabels = false;
                    scaleSegment.RestoreAxisScaleAndInterval();

                    // Remove last label of all segmenst except of the last
                    if (segmentIndex < ScaleSegments.Count - 1 &&
                        CustomLabels.Count > 0)
                    {
                        // Remove label and save it in the list
                        removedLabels.Add(CustomLabels[^1]);
                        removedLabelsIndexes.Add(CustomLabels.Count - 1);
                        CustomLabels.RemoveAt(CustomLabels.Count - 1);
                    }

                    ++segmentIndex;
                }

                // Check all previously removed last labels of each segment if there
                // is enough space to fit them
                int reInsertedLabelsCount = 0;
                int labelIndex = 0;
                foreach (CustomLabel label in removedLabels)
                {
                    // Re-insert the label
                    int labelInsertIndex = (int)removedLabelsIndexes[labelIndex] + reInsertedLabelsCount;
                    if (labelIndex < CustomLabels.Count)
                    {
                        CustomLabels.Insert(labelInsertIndex, label);
                    }
                    else
                    {
                        CustomLabels.Add(label);
                    }

                    // Check labels fit. Only horizontal or vertical fit is checked depending
                    // on the axis orientation.
                    ArrayList labelPositions = new();
                    bool fitDone = CheckLabelsFit(
                        chartGraph,
                        markSize + scrollBarSize + titleSize,
                        autoPlotPosition,
                        true,
                        false,
                        AxisPosition != AxisPosition.Left && AxisPosition != AxisPosition.Right,
                        AxisPosition == AxisPosition.Left || AxisPosition == AxisPosition.Right,
                        labelPositions);

                    // If labels fit check if any of the label positions overlap
                    if (fitDone)
                    {
                        for (int index = 0; fitDone && index < labelPositions.Count; index++)
                        {
                            SKRect rect1 = (SKRect)labelPositions[index];
                            for (int index2 = index + 1; fitDone && index2 < labelPositions.Count; index2++)
                            {
                                SKRect rect2 = (SKRect)labelPositions[index2];
                                if (rect1.IntersectsWith(rect2))
                                {
                                    fitDone = false;
                                }
                            }
                        }
                    }

                    // If labels do not fit or overlapp - remove completly
                    if (!fitDone)
                    {
                        CustomLabels.RemoveAt(labelInsertIndex);
                    }
                    else
                    {
                        ++reInsertedLabelsCount;
                    }

                    ++labelIndex;
                }

                // Make sure now values are rounded on segment boundary
                ScaleSegments.AllowOutOfScaleValues = false;
            }
        }

        /// <summary>
        /// Checks if variable count labels mode is enabled.
        /// </summary>
        /// <returns>True if variable count labels mode is enabled.</returns>
        private bool IsVariableLabelCountModeEnabled()
        {
            // Make sure the variable interval mode is enabled and
            // no custom label interval used.
            if ((IntervalAutoMode == IntervalAutoMode.VariableCount || ScaleSegments.Count > 0) &&
                !IsLogarithmic &&
                (tempLabelInterval <= 0.0 || (double.IsNaN(tempLabelInterval) && Interval <= 0.0)))
            {
                // This feature is not supported for charts that do not
                // require X and Y axes (Pie, Radar, ...)
                if (!ChartArea.requireAxes)
                {
                    return false;
                }
                // This feature is not supported if the axis doesn't have data range
                if (Double.IsNaN(minimum) || double.IsNaN(maximum))
                {
                    return false;
                }
                // Check if custom labels are used in the first row
                bool customLabels = false;
                foreach (CustomLabel label in CustomLabels)
                {
                    if (label.customLabel && label.RowIndex == 0)
                    {
                        customLabels = true;
                        break;
                    }
                }

                // Proceed only if no custom labels are used in the first row
                if (!customLabels)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates axis interval so that labels will fit most efficiently.
        /// </summary>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="autoPlotPosition">True if plot position is auto calculated.</param>
        /// <param name="axisScaleSegment">Axis scale segment to process.</param>
        /// <param name="onlyIncreaseInterval">True if interval should only be increased.</param>
        private void AdjustIntervalToFitLabels(
            ChartGraphics chartGraph,
            bool autoPlotPosition,
            AxisScaleSegment axisScaleSegment,
            bool onlyIncreaseInterval)
        {
            // Re-fill the labels just for the scale segment provided
            if (axisScaleSegment != null)
            {
                // Re-fill new axis labels
                if (tempLabels != null)
                {
                    CustomLabels.Clear();
                    foreach (CustomLabel label in tempLabels)
                    {
                        CustomLabels.Add(label.Clone());
                    }
                }

                // Fill labels just for the segment
                axisScaleSegment.SetTempAxisScaleAndInterval();
                FillLabels(true);
                axisScaleSegment.RestoreAxisScaleAndInterval();
            }

            // Calculate minimum interval size
            double minIntervalSzie = double.NaN;
            ArrayList axisSeries = AxisScaleBreakStyle.GetAxisSeries(this);
            foreach (Series series in axisSeries)
            {
                if (axisType == AxisName.X || axisType == AxisName.X2)
                {
                    if (ChartHelper.IndexedSeries(series))
                    {
                        minIntervalSzie = 1.0;
                    }
                    else if (series.XValueType == ChartValueType.String ||
                        series.XValueType == ChartValueType.Int32 ||
                        series.XValueType == ChartValueType.UInt32 ||
                        series.XValueType == ChartValueType.UInt64 ||
                        series.XValueType == ChartValueType.Int64)
                    {
                        minIntervalSzie = 1.0;
                    }
                }
                else
                {
                    if (series.YValueType == ChartValueType.String ||
                        series.YValueType == ChartValueType.Int32 ||
                        series.YValueType == ChartValueType.UInt32 ||
                        series.YValueType == ChartValueType.UInt64 ||
                        series.YValueType == ChartValueType.Int64)
                    {
                        minIntervalSzie = 1.0;
                    }
                }
            }

            // Iterate while interval is not found
            bool firstIteration = true;
            bool increaseNumberOfLabels = true;
            double currentInterval = (axisScaleSegment == null) ? labelStyle.GetInterval() : axisScaleSegment.Interval;
            DateTimeIntervalType currentIntervalType = (axisScaleSegment == null) ? labelStyle.GetIntervalType() : axisScaleSegment.IntervalType;
            DateTimeIntervalType lastFitIntervalType = currentIntervalType;
            double lastFitInterval = currentInterval;
            ArrayList lastFitLabels = new();
            bool intervalFound = false;
            int iterationNumber = 0;
            while (!intervalFound && iterationNumber <= 1000)
            {
                bool fillNewLabels = true;
#if DEBUG
                if (iterationNumber >= 999)
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisDynamicIntervalCalculationFailed));
                }
#endif // DEBUG

                // Check labels fit. Only horizontal or vertical fit is checked depending
                // on the axis orientation.
                bool fitDone = CheckLabelsFit(
                    chartGraph,
                    markSize + scrollBarSize + titleSize,
                    autoPlotPosition,
                    true,
                    false,
                    AxisPosition != AxisPosition.Left && AxisPosition != AxisPosition.Right,
                    AxisPosition == AxisPosition.Left || AxisPosition == AxisPosition.Right,
                    null);

                // Check if we need to increase or reduce number of labels
                if (firstIteration)
                {
                    firstIteration = false;
                    increaseNumberOfLabels = fitDone;

                    // Check if we can decrease the interval
                    if (onlyIncreaseInterval && increaseNumberOfLabels)
                    {
                        intervalFound = true;
                        continue;
                    }
                }

                // Find new interval. Value 0.0 means that interval cannot be
                // reduced/increased any more and current interval should be used
                double newInterval = 0.0;
                DateTimeIntervalType newIntervalType = DateTimeIntervalType.Number;
                if (increaseNumberOfLabels)
                {
                    if (fitDone)
                    {
                        // Make a copy of last interval and labels collection that previously fit
                        lastFitInterval = currentInterval;
                        lastFitIntervalType = currentIntervalType;
                        lastFitLabels.Clear();
                        foreach (CustomLabel label in CustomLabels)
                        {
                            lastFitLabels.Add(label);
                        }

                        newIntervalType = currentIntervalType;
                        newInterval = ReduceLabelInterval(
                            currentInterval,
                            minIntervalSzie,
                            ref newIntervalType);
                    }
                    else
                    {
                        newInterval = lastFitInterval;
                        newIntervalType = lastFitIntervalType;
                        intervalFound = true;

                        // Reuse previously saved labels
                        fillNewLabels = false;
                        CustomLabels.Clear();
                        foreach (CustomLabel label in lastFitLabels)
                        {
                            CustomLabels.Add(label);
                        }
                    }
                }
                else
                {
                    if (!fitDone && CustomLabels.Count > 1)
                    {
                        newIntervalType = currentIntervalType;
                        newInterval = IncreaseLabelInterval(
                            currentInterval,
                            ref newIntervalType);
                    }
                    else
                    {
                        intervalFound = true;
                    }
                }

                // Set new interval
                if (newInterval != 0.0)
                {
                    currentInterval = newInterval;
                    currentIntervalType = newIntervalType;

                    if (axisScaleSegment == null)
                    {
                        SetIntervalAndType(newInterval, newIntervalType);
                    }
                    else
                    {
                        axisScaleSegment.Interval = newInterval;
                        axisScaleSegment.IntervalType = newIntervalType;
                    }

                    // Re-fill new axis labels
                    if (fillNewLabels)
                    {
                        if (tempLabels != null)
                        {
                            CustomLabels.Clear();
                            foreach (CustomLabel label in tempLabels)
                            {
                                CustomLabels.Add(label.Clone());
                            }
                        }

                        if (axisScaleSegment == null)
                        {
                            FillLabels(true);
                        }
                        else
                        {
                            axisScaleSegment.SetTempAxisScaleAndInterval();
                            FillLabels(true);
                            axisScaleSegment.RestoreAxisScaleAndInterval();
                        }
                    }
                }
                else
                {
                    intervalFound = true;
                }

                ++iterationNumber;
            }
        }

        /// <summary>
        /// Reduces current label interval, so that more labels can fit.
        /// </summary>
        /// <param name="oldInterval">An interval to reduce.</param>
        /// <param name="minInterval">Minimum interval size.</param>
        /// <param name="axisIntervalType">Interval type.</param>
        /// <returns>New interval or 0.0 if interval cannot be reduced.</returns>
        private double ReduceLabelInterval(
            double oldInterval,
            double minInterval,
            ref DateTimeIntervalType axisIntervalType)
        {
            double newInterval = oldInterval;

            // Calculate rounded interval value
            double range = maximum - minimum;
            int iterationIndex = 0;
            if (axisIntervalType == DateTimeIntervalType.Auto ||
                axisIntervalType == DateTimeIntervalType.NotSet ||
                axisIntervalType == DateTimeIntervalType.Number)
            {
                // Process numeric scale
                double devider = 2.0;
                do
                {
#if DEBUG
                    if (iterationIndex >= 99)
                    {
                        throw (new InvalidOperationException(SR.ExceptionAxisIntervalDecreasingFailed));
                    }
#endif // DEBUG

                    newInterval = CalcInterval(range / (range / (newInterval / devider)));
                    if (newInterval == oldInterval)
                    {
                        devider *= 2.0;
                    }

                    ++iterationIndex;
                } while (newInterval == oldInterval && iterationIndex <= 100);
            }
            else
            {
                // Process date scale
                if (oldInterval > 1.0 || oldInterval < 1.0)
                {
                    if (axisIntervalType == DateTimeIntervalType.Minutes ||
                        axisIntervalType == DateTimeIntervalType.Seconds)
                    {
                        if (oldInterval >= 60)
                        {
                            newInterval = Math.Round(oldInterval / 2.0);
                        }
                        else if (oldInterval >= 30.0)
                        {
                            newInterval = 15.0;
                        }
                        else if (oldInterval >= 15.0)
                        {
                            newInterval = 5.0;
                        }
                        else if (oldInterval >= 5.0)
                        {
                            newInterval = 1.0;
                        }
                    }
                    else
                    {
                        newInterval = Math.Round(oldInterval / 2.0);
                    }
                    if (newInterval < 1.0)
                    {
                        newInterval = 1.0;
                    }
                }
                if (oldInterval == 1.0)
                {
                    if (axisIntervalType == DateTimeIntervalType.Years)
                    {
                        newInterval = 6.0;
                        axisIntervalType = DateTimeIntervalType.Months;
                    }
                    else if (axisIntervalType == DateTimeIntervalType.Months)
                    {
                        newInterval = 2.0;
                        axisIntervalType = DateTimeIntervalType.Weeks;
                    }
                    else if (axisIntervalType == DateTimeIntervalType.Weeks)
                    {
                        newInterval = 2.0;
                        axisIntervalType = DateTimeIntervalType.Days;
                    }
                    else if (axisIntervalType == DateTimeIntervalType.Days)
                    {
                        newInterval = 12.0;
                        axisIntervalType = DateTimeIntervalType.Hours;
                    }
                    else if (axisIntervalType == DateTimeIntervalType.Hours)
                    {
                        newInterval = 30.0;
                        axisIntervalType = DateTimeIntervalType.Minutes;
                    }
                    else if (axisIntervalType == DateTimeIntervalType.Minutes)
                    {
                        newInterval = 30.0;
                        axisIntervalType = DateTimeIntervalType.Seconds;
                    }
                    else if (axisIntervalType == DateTimeIntervalType.Seconds)
                    {
                        newInterval = 100.0;
                        axisIntervalType = DateTimeIntervalType.Milliseconds;
                    }
                }
            }

            // Make sure interal is not less than min interval specified
            if (!double.IsNaN(minInterval) && newInterval < minInterval)
            {
                newInterval = 0.0;
            }

            return newInterval;
        }

        /// <summary>
        /// Increases current label interval, so that less labels fit.
        /// </summary>
        /// <param name="oldInterval">An interval to increase.</param>
        /// <param name="axisIntervalType">Interval type.</param>
        /// <returns>New interval or 0.0 if interval cannot be increased.</returns>
        private double IncreaseLabelInterval(
            double oldInterval,
            ref DateTimeIntervalType axisIntervalType)
        {
            double newInterval = oldInterval;

            // Calculate rounded interval value
            double range = maximum - minimum;
            int iterationIndex = 0;
            if (axisIntervalType == DateTimeIntervalType.Auto ||
                axisIntervalType == DateTimeIntervalType.NotSet ||
                axisIntervalType == DateTimeIntervalType.Number)
            {
                // Process numeric scale
                double devider = 2.0;
                do
                {
#if DEBUG
                    if (iterationIndex >= 99)
                    {
                        throw (new InvalidOperationException(SR.ExceptionAxisIntervalIncreasingFailed));
                    }
#endif // DEBUG

                    newInterval = CalcInterval(range / (range / (newInterval * devider)));
                    if (newInterval == oldInterval)
                    {
                        devider *= 2.0;
                    }
                    ++iterationIndex;
                } while (newInterval == oldInterval && iterationIndex <= 100);
            }
            else
            {
                // Process date scale
                newInterval = oldInterval * 2.0;
                if (axisIntervalType == DateTimeIntervalType.Years)
                {
                    // Do nothing for years
                }
                else if (axisIntervalType == DateTimeIntervalType.Months)
                {
                    if (newInterval >= 12.0)
                    {
                        newInterval = 1.0;
                        axisIntervalType = DateTimeIntervalType.Years;
                    }
                }
                else if (axisIntervalType == DateTimeIntervalType.Weeks)
                {
                    if (newInterval >= 4.0)
                    {
                        newInterval = 1.0;
                        axisIntervalType = DateTimeIntervalType.Months;
                    }
                }
                else if (axisIntervalType == DateTimeIntervalType.Days)
                {
                    if (newInterval >= 7.0)
                    {
                        newInterval = 1.0;
                        axisIntervalType = DateTimeIntervalType.Weeks;
                    }
                }
                else if (axisIntervalType == DateTimeIntervalType.Hours)
                {
                    if (newInterval >= 60.0)
                    {
                        newInterval = 1.0;
                        axisIntervalType = DateTimeIntervalType.Days;
                    }
                }
                else if (axisIntervalType == DateTimeIntervalType.Minutes)
                {
                    if (newInterval >= 60.0)
                    {
                        newInterval = 1.0;
                        axisIntervalType = DateTimeIntervalType.Hours;
                    }
                }
                else if (axisIntervalType == DateTimeIntervalType.Seconds)
                {
                    if (newInterval >= 60.0)
                    {
                        newInterval = 1.0;
                        axisIntervalType = DateTimeIntervalType.Minutes;
                    }
                }
                else if (axisIntervalType == DateTimeIntervalType.Milliseconds && newInterval >= 1000.0)
                {
                    newInterval = 1.0;
                    axisIntervalType = DateTimeIntervalType.Seconds;
                }
            }

            return newInterval;
        }

        /// <summary>
        /// Finds the longest labels with the space and inserts the new line character.
        /// </summary>
        /// <param name="labels">Labels collection.</param>
        /// <returns>True if collection was modified.</returns>
        private static bool WordWrapLongestLabel(CustomLabelsCollection labels)
        {
            bool changed = false;

            // Each label may contain several lines of text.
            // Create a list that contains an array of text for each label.
            ArrayList labelTextRows = new(labels.Count);
            foreach (CustomLabel label in labels)
            {
                labelTextRows.Add(label.Text.Split('\n'));
            }

            // Find the longest label with a space
            int longestLabelSize = 5;
            int longestLabelIndex = -1;
            int longestLabelRowIndex = -1;
            int index = 0;
            foreach (string[] textRows in labelTextRows)
            {
                for (int rowIndex = 0; rowIndex < textRows.Length; rowIndex++)
                {
                    if (textRows[rowIndex].Length > longestLabelSize && textRows[rowIndex].Trim().IndexOf(' ') > 0)
                    {
                        longestLabelSize = textRows[rowIndex].Length;
                        longestLabelIndex = index;
                        longestLabelRowIndex = rowIndex;
                    }
                }
                ++index;
            }

            // Longest label with a space was found
            if (longestLabelIndex >= 0 && longestLabelRowIndex >= 0)
            {
                // Try to find a space and replace it with a new line
                string newText = ((string[])labelTextRows[longestLabelIndex])[longestLabelRowIndex];
                for (index = 0; index < (newText.Length) / 2 - 1; index++)
                {
                    if (newText[(newText.Length) / 2 - index] == ' ')
                    {
                        newText =
                            newText.Substring(0, newText.Length / 2 - index) +
                            "\n" +
                            newText[(newText.Length / 2 - index + 1)..];
                        changed = true;
                    }
                    else if (newText[(newText.Length) / 2 + index] == ' ')
                    {
                        newText =
                            newText.Substring(0, (newText.Length) / 2 + index) +
                            "\n" +
                            newText[(newText.Length / 2 + index + 1)..];
                        changed = true;
                    }

                    if (changed)
                    {
                        ((string[])labelTextRows[longestLabelIndex])[longestLabelRowIndex] = newText;
                        break;
                    }
                }

                // Update label text
                if (changed)
                {
                    // Construct label text from multiple rows separated by "\n"
                    CustomLabel label = labels[longestLabelIndex];
                    var sb = new StringBuilder();
                    for (int rowIndex = 0; rowIndex < ((string[])labelTextRows[longestLabelIndex]).Length; rowIndex++)
                    {
                        if (rowIndex > 0)
                        {
                            sb.Append('\n');
                        }
                        sb.Append(((string[])labelTextRows[longestLabelIndex])[rowIndex]);
                    }
                    label.Text = sb.ToString();
                }
            }

            return changed;
        }

        /// <summary>
        /// Calculates the auto-fit font for the circular Common.Chart area axis labels.
        /// </summary>
        /// <param name="graph">Chart graphics object.</param>
        /// <param name="axisList">List of sector labels.</param>
        /// <param name="labelsStyle">Circular labels style.</param>
        /// <param name="plotAreaRectAbs">Plotting area position.</param>
        /// <param name="areaRectAbs">Chart area position.</param>
        /// <param name="labelsSizeEstimate">Estimated size of labels.</param>
        internal void GetCircularAxisLabelsAutoFitFont(
            ChartGraphics graph,
            ArrayList axisList,
            CircularAxisLabelsStyle labelsStyle,
            SKRect plotAreaRectAbs,
            SKRect areaRectAbs,
            float labelsSizeEstimate)
        {
            // X axis settings defines if auto-fit font should be calculated
            if (!IsLabelAutoFit ||
                LabelAutoFitStyle == LabelAutoFitStyles.None ||
                !LabelStyle.Enabled)
            {
                return;
            }

            // Set minimum font size
            _minLabelFontSize = Math.Min(LabelAutoFitMinFontSize, LabelAutoFitMaxFontSize);

            // Create new auto-fit font
            autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                LabelStyle.Font.Typeface.FamilyName,
                Math.Max(LabelAutoFitMaxFontSize, LabelAutoFitMinFontSize),
                LabelStyle.Font.Typeface.FontStyle);

            // Check if we allowed to increase font size while auto-fitting
            if ((LabelAutoFitStyle & LabelAutoFitStyles.IncreaseFont) != LabelAutoFitStyles.IncreaseFont)
            {
                // Use axis labels font as starting point
                autoLabelFont = LabelStyle.Font;
            }

            // Loop while labels do not fit
            bool fitDone = false;
            while (!fitDone)
            {
                //******************************************************
                //** Check if labels fit
                //******************************************************
                fitDone = CheckCircularLabelsFit(
                    graph,
                    axisList,
                    labelsStyle,
                    plotAreaRectAbs,
                    areaRectAbs,
                    labelsSizeEstimate);

                //******************************************************
                //** Adjust labels text properties to fit
                //******************************************************
                if (!fitDone)
                {
                    // Try to reduce font size
                    if (autoLabelFont.Size > _minLabelFontSize &&
                        (LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                    {
                        autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                            autoLabelFont.Typeface.FamilyName,
                            autoLabelFont.Size - 1,
                            autoLabelFont.Typeface.FontStyle);
                    }

                    // Failed to fit
                    else
                    {
                        // Use last font with no angles
                        autoLabelAngle = 0;
                        autoLabelOffset = 0;
                        fitDone = true;
                    }
                }
            }
        }

        /// <summary>
        /// Checks id circular axis labels fits using current auto-fit font.
        /// </summary>
        /// <param name="graph">Chart graphics object.</param>
        /// <param name="axisList">List of sector labels.</param>
        /// <param name="labelsStyle">Circular labels style.</param>
        /// <param name="plotAreaRectAbs">Plotting area position.</param>
        /// <param name="areaRectAbs">Chart area position.</param>
        /// <param name="labelsSizeEstimate">Estimated size of labels.</param>
        /// <returns>True if labels fit.</returns>
        internal bool CheckCircularLabelsFit(
            ChartGraphics graph,
            ArrayList axisList,
            CircularAxisLabelsStyle labelsStyle,
            SKRect plotAreaRectAbs,
            SKRect areaRectAbs,
            float labelsSizeEstimate)
        {
            bool labelsFit = true;

            // Get absolute center of the area
            SKPoint areaCenterAbs = graph.GetAbsolutePoint(ChartArea.circularCenter);

            // Get absolute markers size and spacing
            float spacing = graph.GetAbsolutePoint(new SKPoint(0, markSize + Axis.elementSpacing)).Y;

            //*****************************************************************
            //** Loop through all axis labels
            //*****************************************************************
            SKRect prevLabelPosition = SKRect.Empty;
            float prevLabelSideAngle = float.NaN;
            foreach (CircularChartAreaAxis axis in axisList)
            {
                //*****************************************************************
                //** Measure label text
                //*****************************************************************
                SKSize textSize = graph.MeasureString(
                    axis.Title.Replace("\\n", "\n"),
                    autoLabelFont);

                //*****************************************************************
                //** Get circular style label position.
                //*****************************************************************
                if (labelsStyle == CircularAxisLabelsStyle.Circular ||
                    labelsStyle == CircularAxisLabelsStyle.Radial)
                {
                    // Swith text size for the radial style
                    if (labelsStyle == CircularAxisLabelsStyle.Radial)
                    {
                        float tempValue = textSize.Width;
                        textSize.Width = textSize.Height;
                        textSize.Height = tempValue;
                    }

                    //*****************************************************************
                    //** Check overlapping with previous label
                    //*****************************************************************

                    // Get radius of plot area
                    float plotAreaRadius = areaCenterAbs.Y - plotAreaRectAbs.Top;
                    plotAreaRadius -= labelsSizeEstimate;
                    plotAreaRadius += spacing;

                    // Calculate angle on the side of the label
                    float leftSideAngle = (float)(Math.Atan((textSize.Width / 2f) / plotAreaRadius) * 180f / Math.PI);
                    float rightSideAngle = axis.AxisPosition + leftSideAngle;
                    leftSideAngle = axis.AxisPosition - leftSideAngle;

                    // Check if label overlap the previous label
                    if (!float.IsNaN(prevLabelSideAngle) && prevLabelSideAngle > leftSideAngle)
                    {
                        // Labels overlap
                        labelsFit = false;
                        break;
                    }

                    // Remember label side angle
                    prevLabelSideAngle = rightSideAngle - 1;

                    //*****************************************************************
                    //** Check if label is inside the Common.Chart area
                    //*****************************************************************

                    // Find the most outside point of the label
                    SKPoint outsidePoint = new(areaCenterAbs.X, plotAreaRectAbs.Top);
                    outsidePoint.Y += labelsSizeEstimate;
                    outsidePoint.Y -= textSize.Height;
                    outsidePoint.Y -= spacing;

                    SKPoint[] rotatedPoint = new SKPoint[] { outsidePoint };
                    SKMatrix newMatrix = SKMatrix.CreateRotationDegrees(axis.AxisPosition, areaCenterAbs.X, areaCenterAbs.Y);
                    newMatrix.TransformPoints(rotatedPoint);

                    // Check if rotated point is inside Common.Chart area
                    if (!areaRectAbs.Contains(rotatedPoint[0]))
                    {
                        // Label is not inside Common.Chart area
                        labelsFit = false;
                        break;
                    }
                }

                //*****************************************************************
                //** Get horizontal style label position.
                //*****************************************************************
                else if (labelsStyle == CircularAxisLabelsStyle.Horizontal)
                {
                    // Get text angle
                    float textAngle = axis.AxisPosition;
                    if (textAngle > 180f)
                    {
                        textAngle -= 180f;
                    }

                    // Get label rotated position
                    SKPoint[] labelPosition = new SKPoint[] { new SKPoint(areaCenterAbs.X, plotAreaRectAbs.Top) };
                    labelPosition[0].Y += labelsSizeEstimate;
                    labelPosition[0].Y -= spacing;
                    SKMatrix newMatrix = SKMatrix.CreateRotationDegrees(axis.AxisPosition, areaCenterAbs.X, areaCenterAbs.Y);
                    newMatrix.TransformPoints(labelPosition);

                    // Calculate label position
                    SKRect curLabelPosition = new(
                        labelPosition[0].X,
                        labelPosition[0].Y - textSize.Height / 2f,
                        textSize.Width,
                        textSize.Height);
                    if (textAngle < 5f)
                    {
                        curLabelPosition.Left = labelPosition[0].X - textSize.Width / 2f;
                        curLabelPosition.Top = labelPosition[0].Y - textSize.Height;
                    }
                    if (textAngle > 175f)
                    {
                        curLabelPosition.Left = labelPosition[0].X - textSize.Width / 2f;
                        curLabelPosition.Top = labelPosition[0].Y;
                    }

                    // Decrease label rectangle
                    curLabelPosition.Inflate(0f, -curLabelPosition.Height * 0.15f);

                    // Check if label position goes outside of the Common.Chart area.
                    if (!areaRectAbs.Contains(curLabelPosition))
                    {
                        // Label is not inside Common.Chart area
                        labelsFit = false;
                        break;
                    }

                    // Check if label position overlap previous label position.
                    if (!prevLabelPosition.IsEmpty && curLabelPosition.IntersectsWith(prevLabelPosition))
                    {
                        // Label intersects with previous label
                        labelsFit = false;
                        break;
                    }

                    // Set previous point position
                    prevLabelPosition = curLabelPosition;
                }
            }

            return labelsFit;
        }

        #endregion

        #region Axis labels auto-fitting methods

        /// <summary>
        /// Adjust labels font size at second pass of auto fitting.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="autoPlotPosition">Indicates that inner plot position is automatic.</param>
        internal void AdjustLabelFontAtSecondPass(ChartGraphics chartGraph, bool autoPlotPosition)
        {
#if SUBAXES
			// Process all sub-axis
			if(!ChartArea.Area3DStyle.Enable3D &&
				!ChartArea.chartAreaIsCurcular)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.AdjustLabelFontAtSecondPass(chartGraph, autoPlotPosition);
				}
			}
#endif //SUBAXES

            //******************************************************
            //** First try to select the interval that will
            //** generate best fit labels.
            //******************************************************

            // Make sure the variable interval mode is enabled
            if (Enabled != AxisEnabled.False &&
                LabelStyle.Enabled &&
                IsVariableLabelCountModeEnabled())
            {
                // Set font for labels fitting
                if (autoLabelFont == null)
                {
                    autoLabelFont = LabelStyle.Font;
                }

                // Reset angle and stagged flag used in the auto-fitting algorithm
                if (autoLabelAngle < 0)
                {
                    autoLabelAngle = LabelStyle.Angle;
                }
                if (autoLabelOffset < 0)
                {
                    autoLabelOffset = (LabelStyle.IsStaggered) ? 1 : 0;
                }

                // Check labels fit
                bool fitDone = CheckLabelsFit(
                    chartGraph,
                    markSize + scrollBarSize + titleSize,
                    autoPlotPosition,
                    true,
                    true,
                    AxisPosition != AxisPosition.Left && AxisPosition != AxisPosition.Right,
                    AxisPosition == AxisPosition.Left || AxisPosition == AxisPosition.Right,
                    null);

                // If there is a problem fitting labels try to reduce number of labels by
                // increasing of the interval.
                if (!fitDone)
                {
                    // Adjust interval
                    AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, true);
                }
            }

            //******************************************************
            //** If labels auto-fit is on try reducing font size.
            //******************************************************

            totlaGroupingLabelsSizeAdjustment = 0f;
            if (IsLabelAutoFit &&
                LabelAutoFitStyle != LabelAutoFitStyles.None &&
                Enabled != AxisEnabled.False)
            {
                bool fitDone = false;

                if (autoLabelFont == null)
                {
                    autoLabelFont = LabelStyle.Font;
                }

                // Loop while labels do not fit
                float oldLabelSecondRowSize = totlaGroupingLabelsSize;
                while (!fitDone)
                {
                    //******************************************************
                    //** Check if labels fit
                    //******************************************************
                    fitDone = CheckLabelsFit(
                        chartGraph,
                        markSize + scrollBarSize + titleSize,
                        autoPlotPosition,
                        true,
                        true);

                    //******************************************************
                    //** Adjust labels text properties to fit
                    //******************************************************
                    if (!fitDone)
                    {
                        // Try to reduce font
                        if (autoLabelFont.Size > _minLabelFontSize)
                        {
                            // Reduce auto fit font
                            if (ChartArea != null && ChartArea.IsSameFontSKSizeorAllAxes)
                            {
                                // Same font for all axes
                                foreach (Axis currentAxis in ChartArea.Axes)
                                {
                                    if (currentAxis.enabled && currentAxis.IsLabelAutoFit && currentAxis.autoLabelFont != null)
                                    {
                                        currentAxis.autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                            currentAxis.autoLabelFont.Typeface.FamilyName,
                                            autoLabelFont.Size - 1,
                                            currentAxis.autoLabelFont.Typeface.FontStyle);
                                    }
                                }
                            }
                            else if ((LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                            {
                                autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                    autoLabelFont.Typeface.FamilyName,
                                    autoLabelFont.Size - 1,
                                    autoLabelFont.Typeface.FontStyle);
                            }
                            else
                            {
                                // Failed to fit
                                fitDone = true;
                            }
                        }
                        else
                        {
                            // Failed to fit
                            fitDone = true;
                        }
                    }
                }

                totlaGroupingLabelsSizeAdjustment = oldLabelSecondRowSize - totlaGroupingLabelsSize;
            }
        }

        /// <summary>
        /// Check if axis is logarithmic
        /// </summary>
        /// <param name="yValue">Y value from data</param>
        /// <returns>Corected Y value if axis is logarithmic</returns>
        internal double GetLogValue(double yValue)
        {
            // Check if axis is logarithmic
            if (IsLogarithmic)
            {
                yValue = Math.Log(yValue, logarithmBase);
            }

            return yValue;
        }

        /// <summary>
        /// Checks if labels fit using current auto fit properties
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="otherElementsSize">Axis title and marks size.</param>
        /// <param name="autoPlotPosition">Indicates auto calculation of plotting area.</param>
        /// <param name="checkLabelsFirstRowOnly">Labels fit is checked during the second pass.</param>
        /// <param name="secondPass">Indicates second pass of labels fitting.</param>
        /// <returns>True if labels fit.</returns>
        private bool CheckLabelsFit(
            ChartGraphics chartGraph,
            float otherElementsSize,
            bool autoPlotPosition,
            bool checkLabelsFirstRowOnly,
            bool secondPass)
        {
            return CheckLabelsFit(
                chartGraph,
                otherElementsSize,
                autoPlotPosition,
                checkLabelsFirstRowOnly,
                secondPass,
                true,
                true,
                null);
        }

        /// <summary>
        /// Checks if labels fit using current auto fit properties
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="otherElementsSize">Axis title and marks size.</param>
        /// <param name="autoPlotPosition">Indicates auto calculation of plotting area.</param>
        /// <param name="checkLabelsFirstRowOnly">Labels fit is checked during the second pass.</param>
        /// <param name="secondPass">Indicates second pass of labels fitting.</param>
        /// <param name="checkWidth">True if width should be checked.</param>
        /// <param name="checkHeight">True if height should be checked.</param>
        /// <param name="labelPositions">Returns an array of label positions.</param>
        /// <returns>True if labels fit.</returns>
        private bool CheckLabelsFit(
            ChartGraphics chartGraph,
            float otherElementsSize,
            bool autoPlotPosition,
            bool checkLabelsFirstRowOnly,
            bool secondPass,
            bool checkWidth,
            bool checkHeight,
            ArrayList labelPositions)
        {
            // Reset list of label positions
            if (labelPositions != null)
            {
                labelPositions.Clear();
            }

            // Label string drawing format
            using (StringFormat format = new())
            {
                format.FormatFlags |= StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Initialize all labels position rectangle
                SKRect rect = SKRect.Empty;

                // Calculate max label size
                float maxLabelSize = 0;
                if (!autoPlotPosition)
                {
                    if (GetIsMarksNextToAxis())
                    {
                        if (AxisPosition == AxisPosition.Top)
                            maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.Y;
                        else if (AxisPosition == AxisPosition.Bottom)
                            maxLabelSize = ChartArea.Position.Bottom - (float)GetAxisPosition();
                        if (AxisPosition == AxisPosition.Left)
                            maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.X;
                        else if (AxisPosition == AxisPosition.Right)
                            maxLabelSize = ChartArea.Position.Right - (float)GetAxisPosition();
                    }
                    else
                    {
                        if (AxisPosition == AxisPosition.Top)
                            maxLabelSize = PlotAreaPosition.Y - ChartArea.Position.Y;
                        else if (AxisPosition == AxisPosition.Bottom)
                            maxLabelSize = ChartArea.Position.Bottom - PlotAreaPosition.Bottom;
                        if (AxisPosition == AxisPosition.Left)
                            maxLabelSize = PlotAreaPosition.X - ChartArea.Position.X;
                        else if (AxisPosition == AxisPosition.Right)
                            maxLabelSize = ChartArea.Position.Right - PlotAreaPosition.Right;
                    }
                    maxLabelSize *= 2F;
                }
                else
                {
                    if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                        maxLabelSize = ChartArea.Position.Height;
                    else
                        maxLabelSize = ChartArea.Position.Width;
                }

                // Loop through all grouping labels (all except first row)
                totlaGroupingLabelsSize = 0;

                // Get number of groups
                int groupLabelLevelCount = GetGroupLabelLevelCount();

                // Check ig grouping labels exist
                if (groupLabelLevelCount > 0)
                {
                    groupingLabelSizes = new float[groupLabelLevelCount];

                    // Loop through each level of grouping labels
                    bool fitResult = true;
                    for (int groupLevelIndex = 1; groupLevelIndex <= groupLabelLevelCount; groupLevelIndex++)
                    {
                        groupingLabelSizes[groupLevelIndex - 1] = 0f;

                        // Loop through all labels in the level
                        foreach (CustomLabel label in CustomLabels)
                        {
                            // Skip if label middle point is outside current scaleView
                            if (label.RowIndex == 0)
                            {
                                double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                                if (middlePoint < ViewMinimum || middlePoint > ViewMaximum)
                                {
                                    continue;
                                }
                            }

                            if (label.RowIndex == groupLevelIndex)
                            {
                                // Calculate label rect
                                double fromPosition = GetLinearPosition(label.FromPosition);
                                double toPosition = GetLinearPosition(label.ToPosition);
                                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                                {
                                    var h = (maxLabelSize / 100F) * maxAxisLabelRow2Size / groupLabelLevelCount;
                                    rect.Left = (float)Math.Min(fromPosition, toPosition);
                                    rect.Size = new((float)Math.Max(fromPosition, toPosition) - rect.Left, h);
                                }
                                else
                                {
                                    var w = (maxLabelSize / 100F) * maxAxisLabelRow2Size / groupLabelLevelCount;
                                    rect.Top = (float)Math.Min(fromPosition, toPosition);
                                    var h = (float)Math.Max(fromPosition, toPosition) - rect.Top;
                                    rect.Size = new(w, h);
                                }

                                // Measure string
                                SKSize axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"), autoLabelFont);

                                // Add image size
                                if (label.Image.Length > 0)
                                {
                                    SKSize imageAbsSize = new();

                                    if (Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                                    {
                                        SKSize imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
                                        axisLabelSize.Width += imageRelSize.Width;
                                        axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                    }
                                }

                                // Add extra spacing for the box marking of the label
                                if (label.LabelMark == LabelMarkStyle.Box)
                                {
                                    // Get relative size from pixels and add it to the label size
                                    SKSize spacerSize = chartGraph.GetRelativeSize(new SKSize(4, 4));
                                    axisLabelSize.Width += spacerSize.Width;
                                    axisLabelSize.Height += spacerSize.Height;
                                }

                                // Calculate max height of the second row of labels
                                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                                {
                                    groupingLabelSizes[groupLevelIndex - 1] = Math.Max(groupingLabelSizes[groupLevelIndex - 1], axisLabelSize.Height);
                                }
                                else
                                {
                                    axisLabelSize.Width = chartGraph.GetAbsoluteSize(new SKSize(axisLabelSize.Height, axisLabelSize.Height)).Height;
                                    axisLabelSize.Width = chartGraph.GetRelativeSize(new SKSize(axisLabelSize.Width, axisLabelSize.Width)).Width;
                                    groupingLabelSizes[groupLevelIndex - 1] = Math.Max(groupingLabelSizes[groupLevelIndex - 1], axisLabelSize.Width);
                                }

                                // Check if string fits
                                if (Math.Round(axisLabelSize.Width) >= Math.Round(rect.Width) &&
                                    checkWidth)
                                {
                                    fitResult = false;
                                }
                                if (Math.Round(axisLabelSize.Height) >= Math.Round(rect.Height) &&
                                    checkHeight)
                                {
                                    fitResult = false;
                                }
                            }
                        }
                    }

                    totlaGroupingLabelsSize = GetGroupLablesToatalSize();
                    if (!fitResult && !checkLabelsFirstRowOnly)
                    {
                        return false;
                    }
                }

                // Loop through all labels in the first row
                float angle = autoLabelAngle;
                int labelIndex = 0;
                foreach (CustomLabel label in CustomLabels)
                {
                    // Skip if label middle point is outside current scaleView
                    if (label.RowIndex == 0)
                    {
                        double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                        if (middlePoint < ViewMinimum || middlePoint > ViewMaximum)
                        {
                            continue;
                        }
                    }

                    if (label.RowIndex == 0)
                    {
                        // Force which scale segment to use when calculating label position
                        if (labelPositions != null)
                        {
                            ScaleSegments.EnforceSegment(ScaleSegments.FindScaleSegmentForAxisValue((label.FromPosition + label.ToPosition) / 2.0));
                        }

                        // Set label From and To coordinates
                        double fromPosition = GetLinearPosition(label.FromPosition);
                        double toPosition = GetLinearPosition(label.ToPosition);

                        // Reset scale segment to use when calculating label position
                        if (labelPositions != null)
                        {
                            ScaleSegments.EnforceSegment(null);
                        }

                        // Calculate single label position
                        rect.Left = PlotAreaPosition.X;
                        rect.Top = (float)Math.Min(fromPosition, toPosition);
                        rect.Size = new(rect.Width, (float)Math.Max(fromPosition, toPosition) - rect.Top);

                        float maxElementSize = maxAxisElementsSize;
                        if (maxAxisElementsSize - totlaGroupingLabelsSize > 55)
                        {
                            maxElementSize = 55 + totlaGroupingLabelsSize;
                        }
                        if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                        {
                            rect.Size = new((maxLabelSize / 100F) *
                                (maxElementSize - totlaGroupingLabelsSize - otherElementsSize - elementSpacing),
                                rect.Height);
                        }
                        else
                        {
                            rect.Size = new((maxLabelSize / 100F) *
                                (maxElementSize - totlaGroupingLabelsSize - otherElementsSize - elementSpacing),
                                rect.Height);
                        }

                        // Adjust label From/To position if labels are displayed with offset
                        if (autoLabelOffset == 1)
                        {
                            rect.Top -= rect.Height / 2F;
                            rect.Size = new(rect.Width / 2F, rect.Height * 2F);
                        }

                        // If horizontal axis
                        if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                        {
                            // Switch rectangle sizes
                            rect.Size = new(rect.Height, rect.Width);

                            // Set vertical font for measuring
                            if (angle != 0)
                            {
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }
                        else
                        {
                            // Set vertical font for measuring
                            if (angle == 90 || angle == -90)
                            {
                                angle = 0;
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }

                        // Measure label text size. Add the 'I' character to allow a little bit of spacing between labels.
                        SKSize axisLabelSize = chartGraph.MeasureStringRel(
                            label.Text.Replace("\\n", "\n") + "W",
                            autoLabelFont,
                            (secondPass) ? rect.Size : ChartArea.Position.ToSKRect().Size,
                            format);

                        // Width and height maybe zeros if rect is too small to fit the text and
                        // the LineLimit format flag is set.
                        if (label.Text.Length > 0 &&
                            (axisLabelSize.Width == 0f ||
                            axisLabelSize.Height == 0f))
                        {
                            // Measure string without the LineLimit flag
                            format.FormatFlags ^= StringFormatFlags.LineLimit;
                            axisLabelSize = chartGraph.MeasureStringRel(
                                label.Text.Replace("\\n", "\n"),
                                autoLabelFont,
                                (secondPass) ? rect.Size : ChartArea.Position.ToSKRect().Size,
                                format);
                            format.FormatFlags |= StringFormatFlags.LineLimit;
                        }

                        // Add image size
                        if (label.Image.Length > 0)
                        {
                            SKSize imageAbsSize = new();

                            if (Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                            {
                                SKSize imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
                                if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
                                {
                                    axisLabelSize.Height += imageRelSize.Height;
                                    axisLabelSize.Width = Math.Max(axisLabelSize.Width, imageRelSize.Width);
                                }
                                else
                                {
                                    axisLabelSize.Width += imageRelSize.Width;
                                    axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                }
                            }
                        }

                        // Add extra spacing for the box marking of the label
                        if (label.LabelMark == LabelMarkStyle.Box)
                        {
                            // Get relative size from pixels and add it to the label size
                            SKSize spacerSize = chartGraph.GetRelativeSize(new SKSize(4, 4));
                            axisLabelSize.Width += spacerSize.Width;
                            axisLabelSize.Height += spacerSize.Height;
                        }

                        // Calculate size using label angle
                        float width = axisLabelSize.Width;
                        float height = axisLabelSize.Height;
                        if (angle != 0)
                        {
                            // Decrease label rectangle width by 3%
                            rect.Size = new(rect.Width * 0.97f, rect.Height);

                            if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                            {
                                width = (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                                width += (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;

                                height = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                                height += (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                            }
                            else
                            {
                                width = (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                                width += (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;

                                height = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                                height += (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                            }
                        }

                        // Save label position
                        if (labelPositions != null)
                        {
                            SKRect labelPosition = rect;
                            if (angle == 0F || angle == 90F || angle == -90F)
                            {
                                if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                                {
                                    labelPosition.Left = labelPosition.Left + labelPosition.Width / 2f - width / 2f;
                                    labelPosition.Size = new(width, labelPosition.Height);
                                }
                                else
                                {
                                    labelPosition.Top = labelPosition.Top + labelPosition.Height / 2f - height / 2f;
                                    labelPosition.Size = new(labelPosition.Width, height);
                                }
                            }
                            labelPositions.Add(labelPosition);
                        }

                        // Check if string fits
                        if (angle == 0F)
                        {
                            if (width >= rect.Width && checkWidth)
                            {
                                return false;
                            }
                            if (height >= rect.Height && checkHeight)
                            {
                                return false;
                            }
                        }
                        if (angle == 90F || angle == -90F)
                        {
                            if (width >= rect.Width && checkWidth)
                            {
                                return false;
                            }
                            if (height >= rect.Height && checkHeight)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (width >= rect.Width * 2F && checkWidth)
                            {
                                return false;
                            }
                            if (height >= rect.Height * 2F && checkHeight)
                            {
                                return false;
                            }
                        }

                        ++labelIndex;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Calculates the best size for labels area.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="maxLabelSize">Maximum labels area size.</param>
        /// <param name="resultSize">Label size without angle = 0.</param>
        private float GetRequiredLabelSize(ChartGraphics chartGraph, float maxLabelSize, out float resultSize)
        {
            float resultRotatedSize = 0F;
            resultSize = 0F;
            float angle = (autoLabelAngle < -90) ? LabelStyle.Angle : autoLabelAngle;
            labelNearOffset = float.MaxValue;
            labelFarOffset = float.MinValue;

            // Label string drawing format
            using (StringFormat format = new())
            {
                format.FormatFlags |= StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Initialize all labels position rectangle
                SKRect rectLabels = ChartArea.Position.ToSKRect();

                // Loop through all labels in the first row
                foreach (CustomLabel label in CustomLabels)
                {
                    // Skip if label middle point is outside current scaleView
                    if (label.RowIndex == 0)
                    {
                        decimal middlePoint = (decimal)(label.FromPosition + label.ToPosition) / (decimal)2.0;
                        if (middlePoint < (decimal)ViewMinimum || middlePoint > (decimal)ViewMaximum)
                        {
                            continue;
                        }
                    }
                    if (label.RowIndex == 0)
                    {
                        // Calculate single label position
                        SKRect rect = rectLabels;

                        // Set label From and To coordinates
                        double fromPosition = GetLinearPosition(label.FromPosition);
                        double toPosition = GetLinearPosition(label.ToPosition);
                        rect.Top = (float)Math.Min(fromPosition, toPosition);
                        rect.Size = new SKSize(maxLabelSize, (float)Math.Max(fromPosition, toPosition) - rect.Top);

                        // Adjust label From/To position if labels are displayed with offset
                        if ((autoLabelOffset == -1) ? LabelStyle.IsStaggered : (autoLabelOffset == 1))
                        {
                            var h = rect.Height;
                            rect.Top -= rect.Height / 2F;
                            rect.Size = new(rect.Width, h * 2F);
                        }

                        // If horizontal axis
                        if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                        {
                            // Switch rectangle sizes
                            float val = rect.Height;
                            rect.Size = new SKSize(val, rect.Width);

                            // Set vertical font for measuring
                            if (angle != 0)
                            {
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }
                        else
                        {
                            // Set vertical font for measuring
                            if (angle == 90 || angle == -90)
                            {
                                angle = 0;
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }

                        // Measure label text size
                        rect.Size = new((float)Math.Ceiling(rect.Width), (float)Math.Ceiling(rect.Height));
                        SKSize axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"),
                            autoLabelFont ?? LabelStyle.Font,
                            rect.Size,
                            format);

                        // Width and height maybe zeros if rect is too small to fit the text and
                        // the LineLimit format flag is set.
                        if (axisLabelSize.Width == 0f || axisLabelSize.Height == 0f)
                        {
                            // Measure string without the LineLimit flag
                            format.FormatFlags ^= StringFormatFlags.LineLimit;
                            axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"),
                                autoLabelFont ?? LabelStyle.Font,
                                rect.Size,
                                format);
                            format.FormatFlags |= StringFormatFlags.LineLimit;
                        }

                        // Add image size
                        if (label.Image.Length > 0)
                        {
                            SKSize imageAbsSize = new();

                            if (Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                            {
                                SKSize imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);

                                if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
                                {
                                    axisLabelSize.Height += imageRelSize.Height;
                                    axisLabelSize.Width = Math.Max(axisLabelSize.Width, imageRelSize.Width);
                                }
                                else
                                {
                                    axisLabelSize.Width += imageRelSize.Width;
                                    axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                }
                            }
                        }

                        // Add extra spacing for the box marking of the label
                        if (label.LabelMark == LabelMarkStyle.Box)
                        {
                            // Get relative size from pixels and add it to the label size
                            SKSize spacerSize = chartGraph.GetRelativeSize(new SKSize(4, 4));
                            axisLabelSize.Width += spacerSize.Width;
                            axisLabelSize.Height += spacerSize.Height;
                        }

                        // Calculate size using label angle
                        float width = axisLabelSize.Width;
                        float height = axisLabelSize.Height;
                        if (angle != 0)
                        {
                            width = (float)Math.Cos((90 - Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                            width += (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;

                            height = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                            height += (float)Math.Sin((90 - Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                        }

                        width = (float)Math.Ceiling(width) * 1.05f;
                        height = (float)Math.Ceiling(height) * 1.05f;
                        axisLabelSize.Width = (float)Math.Ceiling(axisLabelSize.Width) * 1.05f;
                        axisLabelSize.Height = (float)Math.Ceiling(axisLabelSize.Height) * 1.05f;

                        // If axis is horizontal
                        if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                        {
                            if (angle == 90 || angle == -90 || angle == 0)
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Height);
                                resultRotatedSize = Math.Max(resultRotatedSize, axisLabelSize.Height);

                                // Calculate the overhang of labels on the side
                                labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - axisLabelSize.Width / 2f);
                                labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + axisLabelSize.Width / 2f);
                            }
                            else
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Height);
                                resultRotatedSize = Math.Max(resultRotatedSize, height);

                                // Calculate the overhang of labels on the side
                                if (angle > 0)
                                {
                                    labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + width * 1.1f);
                                }
                                else
                                {
                                    labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - width * 1.1f);
                                }
                            }
                        }
                        // If axis is vertical
                        else
                        {
                            if (angle == 90 || angle == -90 || angle == 0)
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Width);
                                resultRotatedSize = Math.Max(resultRotatedSize, axisLabelSize.Width);

                                // Calculate the overhang of labels on the side
                                labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - axisLabelSize.Height / 2f);
                                labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + axisLabelSize.Height / 2f);
                            }
                            else
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Width);
                                resultRotatedSize = Math.Max(resultRotatedSize, width);

                                // Calculate the overhang of labels on the side
                                if (angle > 0)
                                {
                                    labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + height * 1.1f);
                                }
                                else
                                {
                                    labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - height * 1.1f);
                                }
                            }
                        }

                        // Check if we exceed the maximum value
                        if (resultSize > maxLabelSize)
                        {
                            resultSize = maxLabelSize;
                        }
                    }
                }
            }

            // Adjust results if labels are displayed with offset
            if ((autoLabelOffset == -1) ? LabelStyle.IsStaggered : (autoLabelOffset == 1))
            {
                resultSize *= 2F;
                resultRotatedSize *= 2F;

                // Check if we exceed the maximum value
                if (resultSize > maxLabelSize)
                {
                    resultSize = maxLabelSize;
                    resultRotatedSize = maxLabelSize;
                }
            }

            // Adjust labels size for the 3D Common.Chart
            if (ChartArea.Area3DStyle.Enable3D && !ChartArea.chartAreaIsCurcular)
            {
                // Increase labels size
                resultSize *= 1.1f;
                resultRotatedSize *= 1.1f;
            }

            return resultRotatedSize;
        }

        /// <summary>
        /// Gets total size of all grouping labels.
        /// </summary>
        /// <returns>Total size of all grouping labels.</returns>
        internal float GetGroupLablesToatalSize()
        {
            float size = 0f;
            if (groupingLabelSizes != null && groupingLabelSizes.Length > 0)
            {
                foreach (float val in groupingLabelSizes)
                {
                    size += val;
                }
            }

            return size;
        }

        /// <summary>
        /// Gets number of levels of the grouping labels.
        /// </summary>
        /// <returns>Number of levels of the grouping labels.</returns>
        internal int GetGroupLabelLevelCount()
        {
            int groupLabelLevel = 0;
            foreach (CustomLabel label in CustomLabels)
            {
                if (label.RowIndex > 0)
                {
                    groupLabelLevel = Math.Max(groupLabelLevel, label.RowIndex);
                }
            }

            return groupLabelLevel;
        }

        /// <summary>
        /// Calculates the best size for axis labels for all rows except first one (grouping labels).
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="maxLabelSize">Maximum labels area size.</param>
        /// <returns>Array of grouping label sizes for each level.</returns>
        private float[] GetRequiredGroupLabelSize(ChartGraphics chartGraph, float maxLabelSize)
        {
            float[] resultSize = null;

            // Get number of groups
            int groupLabelLevelCount = GetGroupLabelLevelCount();

            // Check ig grouping labels exist
            if (groupLabelLevelCount > 0)
            {
                // Create result array
                resultSize = new float[groupLabelLevelCount];

                // Loop through each level of grouping labels
                for (int groupLevelIndex = 1; groupLevelIndex <= groupLabelLevelCount; groupLevelIndex++)
                {
                    resultSize[groupLevelIndex - 1] = 0f;

                    // Loop through all labels in the level
                    foreach (CustomLabel label in CustomLabels)
                    {
                        // Skip if label middle point is outside current scaleView
                        if (label.RowIndex == 0)
                        {
                            double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                            if (middlePoint < ViewMinimum || middlePoint > ViewMaximum)
                            {
                                continue;
                            }
                        }

                        if (label.RowIndex == groupLevelIndex)
                        {
                            // Measure label text size
                            SKSize axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"), autoLabelFont ?? LabelStyle.Font);
                            axisLabelSize.Width = (float)Math.Ceiling(axisLabelSize.Width);
                            axisLabelSize.Height = (float)Math.Ceiling(axisLabelSize.Height);

                            // Add image size
                            if (label.Image.Length > 0)
                            {
                                SKSize imageAbsSize = new();

                                if (Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                                {
                                    SKSize imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
                                    axisLabelSize.Width += imageRelSize.Width;
                                    axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                }
                            }

                            // Add extra spacing for the box marking of the label
                            if (label.LabelMark == LabelMarkStyle.Box)
                            {
                                // Get relative size from pixels and add it to the label size
                                SKSize spacerSize = chartGraph.GetRelativeSize(new SKSize(4, 4));
                                axisLabelSize.Width += spacerSize.Width;
                                axisLabelSize.Height += spacerSize.Height;
                            }

                            // If axis is horizontal
                            if (AxisPosition == AxisPosition.Bottom || AxisPosition == AxisPosition.Top)
                            {
                                resultSize[groupLevelIndex - 1] = Math.Max(resultSize[groupLevelIndex - 1], axisLabelSize.Height);
                            }
                            // If axis is vertical
                            else
                            {
                                axisLabelSize.Width = chartGraph.GetAbsoluteSize(new SKSize(axisLabelSize.Height, axisLabelSize.Height)).Height;
                                axisLabelSize.Width = chartGraph.GetRelativeSize(new SKSize(axisLabelSize.Width, axisLabelSize.Width)).Width;
                                resultSize[groupLevelIndex - 1] = Math.Max(resultSize[groupLevelIndex - 1], axisLabelSize.Width);
                            }
                        }
                    }
                }
            }

            return resultSize;
        }

        #endregion

        #region Axis helper methods

        /// <summary>
        /// Gets main or sub axis associated with this axis.
        /// </summary>
        /// <param name="subAxisName">Sub axis name or empty string to get the main axis.</param>
        /// <returns>Main or sub axis of the main axis.</returns>
        internal Axis GetSubAxis(string subAxisName)
        {
#if SUBAXES
			if(!this.IsSubAxis && subAxisName.Length > 0)
			{
				SubAxis subAxis = this.SubAxes.FindByName(subAxisName);
				if(subAxis == null)
				{
					throw(new InvalidOperationException( SR.ExceptionSubAxisNameNotFoundShort( subAxisName )));
				}
				return subAxis;
			}
#endif // SUBAXES
            return this;
        }

        /// <summary>
        /// Checks if axis marks should be next to the axis
        /// </summary>
        /// <returns>true if marks are next to axis.</returns>
        internal bool GetIsMarksNextToAxis()
        {
            if (ChartArea != null && ChartArea.chartAreaIsCurcular)
            {
                return true;
            }
            return IsMarksNextToAxis;
        }

        /// <summary>
        /// Gets axis auto interval type.
        /// </summary>
        /// <returns>Axis interval type.</returns>
        internal DateTimeIntervalType GetAxisIntervalType()
        {
            if (InternalIntervalType == DateTimeIntervalType.Auto)
            {
                if (GetAxisValuesType() == ChartValueType.DateTime ||
                    GetAxisValuesType() == ChartValueType.Date ||
                    GetAxisValuesType() == ChartValueType.Time ||
                    GetAxisValuesType() == ChartValueType.DateTimeOffset)
                {
                    return DateTimeIntervalType.Years;
                }

                return DateTimeIntervalType.Number;
            }

            return InternalIntervalType;
        }

        /// <summary>
        /// Gets axis values type depending on the series attached
        /// </summary>
        /// <returns>Axis values type.</returns>
        internal ChartValueType GetAxisValuesType()
        {
            ChartValueType type = ChartValueType.Double;

            // Check all series in this Common.Chart area attached to this axis
            if (Common != null && Common.DataManager.Series != null && ChartArea != null)
            {
                foreach (Series series in Common.DataManager.Series)
                {
                    bool seriesAttached = false;

                    // Check series name
                    if (series.ChartArea == ChartArea.Name && series.IsVisible())
                    {
                        // Check if axis type of series match
                        if (axisType == AxisName.X && series.XAxisType == AxisType.Primary)
                        {
                            seriesAttached = true;
                        }
                        else if (axisType == AxisName.X2 && series.XAxisType == AxisType.Secondary)
                        {
                            seriesAttached = true;
                        }
                        else if (axisType == AxisName.Y && series.YAxisType == AxisType.Primary)
                        {
                            seriesAttached = true;
                        }
                        else if (axisType == AxisName.Y2 && series.YAxisType == AxisType.Secondary)
                        {
                            seriesAttached = true;
                        }
                    }

                    // If series attached to this axes
                    if (seriesAttached)
                    {
                        if (axisType == AxisName.X || axisType == AxisName.X2)
                        {
                            type = series.XValueType;
                        }
                        else if (axisType == AxisName.Y || axisType == AxisName.Y2)
                        {
                            type = series.YValueType;
                        }
                        break;
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Returns Arrow size
        /// </summary>
        /// <param name="arrowOrientation">Return arrow orientation.</param>
        /// <returns>Size of arrow</returns>
        internal SKSize GetArrowSize(out ArrowOrientation arrowOrientation)
        {
            double size;
            double sizeOpposite;
            arrowOrientation = ArrowOrientation.Top;

            // Set the position of axis
            switch (AxisPosition)
            {
                case AxisPosition.Left:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;

                case AxisPosition.Right:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;

                case AxisPosition.Bottom:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;

                case AxisPosition.Top:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;
            }

            Axis opositeAxis = arrowOrientation switch
            {
                ArrowOrientation.Left => ChartArea.AxisX,
                ArrowOrientation.Right => ChartArea.AxisX2,
                ArrowOrientation.Top => ChartArea.AxisY2,
                ArrowOrientation.Bottom => ChartArea.AxisY,
                _ => ChartArea.AxisX,
            };

            // Arrow size has to have the same shape when width and height
            // are changed. When the picture is resized, width of the Common.Chart
            // picture is used only for arrow size.
            if (arrowOrientation == ArrowOrientation.Top || arrowOrientation == ArrowOrientation.Bottom)
            {
                size = _lineWidth;
                sizeOpposite = (double)(_lineWidth) * Common.Width / Common.Height;
            }
            else
            {
                size = (double)(_lineWidth) * Common.Width / Common.Height;
                sizeOpposite = _lineWidth;
            }

            // Arrow is sharp triangle
            if (_arrowStyle == AxisArrowStyle.SharpTriangle)
            {
                // Arrow direction is vertical
                if (arrowOrientation == ArrowOrientation.Top || arrowOrientation == ArrowOrientation.Bottom)
                    return new SKSize((float)(size * 2), (float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 4));
                else
                    // Arrow direction is horizontal
                    return new SKSize((float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 4), (float)(size * 2));
            }
            // There is no arrow
            else if (_arrowStyle == AxisArrowStyle.None)
                return new SKSize(0, 0);
            else// Arrow is triangle or line type
            {
                // Arrow direction is vertical
                if (arrowOrientation == ArrowOrientation.Top || arrowOrientation == ArrowOrientation.Bottom)
                    return new SKSize((float)(size * 2), (float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 2));
                else
                    // Arrow direction is horizontal
                    return new SKSize((float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 2), (float)(size * 2));
            }
        }

        /// <summary>
        /// Checks if arrow with specified orientation will require space
        /// in axis with specified position
        /// </summary>
        /// <param name="arrowOrientation">Arrow orientation.</param>
        /// <param name="axisPosition">Axis position.</param>
        /// <returns>True if arrow will be drawn in axis space</returns>
        private static bool IsArrowInAxis(ArrowOrientation arrowOrientation, AxisPosition axisPosition)
        {
            if (axisPosition == AxisPosition.Top && arrowOrientation == ArrowOrientation.Top)
                return true;
            else if (axisPosition == AxisPosition.Bottom && arrowOrientation == ArrowOrientation.Bottom)
                return true;
            if (axisPosition == AxisPosition.Left && arrowOrientation == ArrowOrientation.Left)
                return true;
            else if (axisPosition == AxisPosition.Right && arrowOrientation == ArrowOrientation.Right)
                return true;

            return false;
        }

        /// <summary>
        /// This function converts real Interval to
        /// absolute Interval
        /// </summary>
        /// <param name="realInterval">A interval represented as double value</param>
        /// <returns>A interval represented in pixels</returns>
        internal float GetPixelInterval(double realInterval)
        {
            double chartAreaSize;

            // The Chart area pixel size as double
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                chartAreaSize = PlotAreaPosition.Right - PlotAreaPosition.X;
            }
            else
            {
                chartAreaSize = PlotAreaPosition.Bottom - PlotAreaPosition.Y;
            }

            // Avoid division by zero.
            if (ViewMaximum - ViewMinimum == 0)
            {
                return (float)(chartAreaSize / realInterval);
            }

            // The interval integer
            return (float)(chartAreaSize / (ViewMaximum - ViewMinimum) * realInterval);
        }

        /// <summary>
        /// Find if axis is on the edge of the Common.Chart plot area
        /// </summary>
        internal bool IsAxisOnAreaEdge
        {
            get
            {
                double edgePosition = 0;
                if (AxisPosition == AxisPosition.Bottom)
                {
                    edgePosition = PlotAreaPosition.Bottom;
                }
                else if (AxisPosition == AxisPosition.Left)
                {
                    edgePosition = PlotAreaPosition.X;
                }
                else if (AxisPosition == AxisPosition.Right)
                {
                    edgePosition = PlotAreaPosition.Right;
                }
                else if (AxisPosition == AxisPosition.Top)
                {
                    edgePosition = PlotAreaPosition.Y;
                }

                // DT Fix : problems with values on edge ~0.0005
                if (Math.Abs(GetAxisPosition() - edgePosition) < 0.0015)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Find axis position using crossing value.
        /// </summary>
        /// <returns>Relative position</returns>
        internal double GetAxisPosition()
        {
            return GetAxisPosition(false);
        }

        /// <summary>
        /// Find axis position using crossing value.
        /// </summary>
        /// <param name="ignoreCrossing">Axis crossing should be ignored.</param>
        /// <returns>Relative position</returns>
        virtual internal double GetAxisPosition(bool ignoreCrossing)
        {
            Axis axisOpposite = GetOppositeAxis();

            // Get axis position for circular Common.Chart area
            if (ChartArea != null && ChartArea.chartAreaIsCurcular)
            {
                return PlotAreaPosition.X + PlotAreaPosition.Width / 2f;
            }

            // Axis is not connected with any series. There is no maximum and minimum
            if (axisOpposite.maximum == axisOpposite.minimum ||
                double.IsNaN(axisOpposite.maximum) ||
                double.IsNaN(axisOpposite.minimum) ||
                maximum == minimum ||
                double.IsNaN(maximum) ||
                double.IsNaN(minimum))
            {
                switch (AxisPosition)
                {
                    case AxisPosition.Top:
                        return PlotAreaPosition.Y;

                    case AxisPosition.Bottom:
                        return PlotAreaPosition.Bottom;

                    case AxisPosition.Right:
                        return PlotAreaPosition.Right;

                    case AxisPosition.Left:
                        return PlotAreaPosition.X;
                }
            }

            // Auto crossing enabled
            if (Double.IsNaN(axisOpposite.crossing) || ignoreCrossing)
            {
                // Primary
                if (axisType == AxisName.X || axisType == AxisName.Y)
                    return axisOpposite.GetLinearPosition(axisOpposite.ViewMinimum);
                else // Secondary
                    return axisOpposite.GetLinearPosition(axisOpposite.ViewMaximum);
            }
            else // Auto crossing disabled
            {
                axisOpposite.crossing = axisOpposite.tempCrossing;

                if (axisOpposite.crossing < axisOpposite.ViewMinimum)
                {
                    axisOpposite.crossing = axisOpposite.ViewMinimum;
                }
                else if (axisOpposite.crossing > axisOpposite.ViewMaximum)
                {
                    axisOpposite.crossing = axisOpposite.ViewMaximum;
                }
            }

            return axisOpposite.GetLinearPosition(axisOpposite.crossing);
        }

        #endregion

        #region Axis 3D helper methods

        /// <summary>
        /// Returns angle between 2D axis line and it's 3D transformed projection.
        /// </summary>
        /// <returns>Axis projection angle.</returns>
        internal double GetAxisProjectionAngle()
        {
            // Get Z position
            float zPosition = GetMarksZPosition(out _);

            // Get axis position
            float axisPosition = (float)GetAxisPosition();

            // Create two points on the sides of the axis
            Point3D[] axisPoints = new Point3D[2];
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                axisPoints[0] = new Point3D(0f, axisPosition, zPosition);
                axisPoints[1] = new Point3D(100f, axisPosition, zPosition);
            }
            else
            {
                axisPoints[0] = new Point3D(axisPosition, 0f, zPosition);
                axisPoints[1] = new Point3D(axisPosition, 100f, zPosition);
            }

            // Transform coordinates
            ChartArea.matrix3D.TransformPoints(axisPoints);

            // Round result
            axisPoints[0].X = (float)Math.Round(axisPoints[0].X, 4);
            axisPoints[0].Y = (float)Math.Round(axisPoints[0].Y, 4);
            axisPoints[1].X = (float)Math.Round(axisPoints[1].X, 4);
            axisPoints[1].Y = (float)Math.Round(axisPoints[1].Y, 4);

            // Calculate angle
            double angle;
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                angle = Math.Atan((axisPoints[1].Y - axisPoints[0].Y) / (axisPoints[1].X - axisPoints[0].X));
            }
            else
            {
                angle = Math.Atan((axisPoints[1].X - axisPoints[0].X) / (axisPoints[1].Y - axisPoints[0].Y));
            }

            // Conver to degrees
            return (angle * 180.0) / Math.PI;
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

                if (labelStyle != null)
                {
                    labelStyle.Dispose();
                    labelStyle = null;
                }

                if (_stripLines != null)
                {
                    _stripLines.Dispose();
                    _stripLines = null;
                }
                if (_customLabels != null)
                {
                    _customLabels.Dispose();
                    _customLabels = null;
                }
                if (tempLabels != null)
                {
                    tempLabels.Dispose();
                    tempLabels = null;
                }
                if (scrollBar != null)
                {
                    scrollBar.Dispose();
                    scrollBar = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}