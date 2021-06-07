// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	The ChartArea class represents one chart area within
//              a chart image, and is used to plot one or more chart
//              series. The number of chart series that can be plotted
//              in a chart area is unlimited.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace WebCharts.Services
{
    #region Chart area aligment enumerations

    /// <summary>
    /// An enumeration of the alignment orientations of a ChartArea
    /// </summary>
    [Flags]
    public enum AreaAlignmentOrientations
    {
        /// <summary>
        /// Chart areas are not automatically aligned.
        /// </summary>
        None = 0,

        /// <summary>
        /// Chart areas are aligned vertically.
        /// </summary>
        Vertical = 1,

        /// <summary>
        /// Chart areas are aligned horizontally.
        /// </summary>
        Horizontal = 2,

        /// <summary>
        /// Chart areas are aligned using all values (horizontally and vertically).
        /// </summary>
        All = Vertical | Horizontal
    }

    /// <summary>
    /// An enumeration of the alignment styles of a ChartArea
    /// </summary>
    [Flags]
    public enum AreaAlignmentStyles
    {
        /// <summary>
        /// Chart areas are not automatically aligned.
        /// </summary>
        None = 0,

        /// <summary>
        /// Chart areas are aligned by positions.
        /// </summary>
        Position = 1,

        /// <summary>
        /// Chart areas are aligned by inner plot positions.
        /// </summary>
        PlotPosition = 2,

        /// <summary>
        /// Chart areas are aligned by axes views.
        /// </summary>
        AxesView = 4,

        /// <summary>
        /// Cursor and Selection alignment.
        /// </summary>
        Cursor = 8,

        /// <summary>
        /// Complete alignment.
        /// </summary>
        All = Position | PlotPosition | Cursor | AxesView
    }

    #endregion Chart area aligment enumerations

    /// <summary>
    /// The ChartArea class is used to create and display a chart
    /// area within a chart image. The chart area is a rectangular
    /// area on a chart image.  It has 4 axes, horizontal and vertical grids.
    /// A chart area can contain more than one different chart type.
    /// The number of chart series that can be plotted in a chart area
    /// is unlimited.
    ///
    /// ChartArea class exposes all the properties and methods
    /// of its base ChartArea3D class.
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartArea_ChartArea"),
    ]
    public partial class ChartArea : ChartNamedElement
    {
        #region Chart Area Fields

        /// <summary>
        /// Plot area position
        /// </summary>
        internal ElementPosition PlotAreaPosition;

        // Private data members, which store properties values
        private Axis[] _axisArray = new Axis[4];

        private SKColor _backColor = SKColor.Empty;
        private ChartHatchStyle _backHatchStyle = ChartHatchStyle.None;
        private string _backImage = "";
        private ChartImageWrapMode _backImageWrapMode = ChartImageWrapMode.Tile;
        private SKColor _backImageTransparentColor = SKColor.Empty;
        private ChartImageAlignmentStyle _backImageAlignment = ChartImageAlignmentStyle.TopLeft;
        private GradientStyle _backGradientStyle = GradientStyle.None;
        private SKColor _backSecondaryColor = SKColor.Empty;
        private SKColor _borderColor = SKColors.Black;
        private int _borderWidth = 1;
        private ChartDashStyle _borderDashStyle = ChartDashStyle.NotSet;
        private int _shadowOffset = 0;
        private SKColor _shadowColor = new(0, 0, 0, 128);
        private ElementPosition _areaPosition = null;
        private ElementPosition _innerPlotPosition = null;
        internal int IterationCounter = 0;

        private bool _isSameFontSKSizeorAllAxes = false;
        internal float axesAutoFontSize = 8f;

        private string _alignWithChartArea = Constants.NotSetValue;
        private AreaAlignmentOrientations _alignmentOrientation = AreaAlignmentOrientations.Vertical;
        private AreaAlignmentStyles _alignmentStyle = AreaAlignmentStyles.All;
        private int _circularSectorNumber = int.MinValue;
        private int _circularUsePolygons = int.MinValue;

        // Flag indicates that chart area is acurrently aligned
        internal bool alignmentInProcess = false;

        // Chart area position before adjustments
        internal SKRect originalAreaPosition = SKRect.Empty;

        // Chart area inner plot position before adjustments
        internal SKRect originalInnerPlotPosition = SKRect.Empty;

        // Chart area position before adjustments
        internal SKRect lastAreaPosition = SKRect.Empty;

        // Center of the circulat chart area
        internal SKPoint circularCenter = SKPoint.Empty;

        private ArrayList _circularAxisList = null;

        // Buffered plotting area image
        internal SKImage areaBufferBitmap = null;

        private Cursor _cursorX = new();
        private Cursor _cursorY = new();

        // Area SmartLabel class
        internal SmartLabel smartLabels = new();

        // Gets or sets a flag that specifies whether the chart area is visible.
        private bool _visible = true;

        #endregion Chart Area Fields

        #region Chart Area Cursor properties

        /// <summary>
        /// Gets or sets a Cursor object that is used for cursors and selected ranges along the X-axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeCursor"),
        SRDescription("DescriptionAttributeChartArea_CursorX"),
        ]
        public Cursor CursorX
        {
            get
            {
                return _cursorX;
            }
            set
            {
                _cursorX = value;

                // Initialize chart object
                _cursorX.Initialize(this, AxisName.X);
            }
        }

        /// <summary>
        /// Gets or sets a Cursor object that is used for cursors and selected ranges along the Y-axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeCursor"),
        SRDescription("DescriptionAttributeChartArea_CursorY"),
        ]
        public Cursor CursorY
        {
            get
            {
                return _cursorY;
            }
            set
            {
                _cursorY = value;

                // Initialize chart object
                _cursorY.Initialize(this, AxisName.Y);
            }
        }

        #endregion Chart Area Cursor properties

        #region Chart Area properties

        /// <summary>
        /// Gets or sets a flag that specifies whether the chart area is visible.
        /// </summary>
        /// <remarks>
        /// When this flag is set to false, all series, legends, titles and annotation objects
        /// associated with the chart area will also be hidden.
        /// </remarks>
        /// <value>
        /// <b>True</b> if the chart area is visible; <b>false</b> otherwise.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChartArea_Visible"),
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the name of the ChartArea object to which this chart area should be aligned.
        /// </summary>
        [
        SRCategory("CategoryAttributeAlignment"),
        SRDescription("DescriptionAttributeChartArea_AlignWithChartArea"),
        ]
        public string AlignWithChartArea
        {
            get
            {
                return _alignWithChartArea;
            }
            set
            {
                if (value != _alignWithChartArea)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        _alignWithChartArea = Constants.NotSetValue;
                    }
                    else
                    {
                        if (Chart != null && Chart.ChartAreas != null)
                        {
                            Chart.ChartAreas.VerifyNameReference(value);
                        }
                        _alignWithChartArea = value;
                    }
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment orientation of a chart area.
        /// </summary>
        [
        SRCategory("CategoryAttributeAlignment"),
        SRDescription("DescriptionAttributeChartArea_AlignOrientation"),
        ]
        public AreaAlignmentOrientations AlignmentOrientation
        {
            get
            {
                return _alignmentOrientation;
            }
            set
            {
                _alignmentOrientation = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the alignment style of the ChartArea.
        /// </summary>
        [
        SRCategory("CategoryAttributeAlignment"),
        SRDescription("DescriptionAttributeChartArea_AlignType"),
        ]
        public AreaAlignmentStyles AlignmentStyle
        {
            get
            {
                return _alignmentStyle;
            }
            set
            {
                _alignmentStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an array that represents all axes for a chart area.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxes"),
        SRDescription("DescriptionAttributeChartArea_Axes"),
        ]
        public Axis[] Axes
        {
            get
            {
                return _axisArray;
            }
            set
            {
                AxisX = value[0];
                AxisY = value[1];
                AxisX2 = value[2];
                AxisY2 = value[3];
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an Axis object that represents the primary Y-axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxis"),
        SRDescription("DescriptionAttributeChartArea_AxisY"),
        ]
        public Axis AxisY
        {
            get
            {
                return axisY;
            }
            set
            {
                axisY = value;
                axisY.Initialize(this, AxisName.Y);
                _axisArray[1] = axisY;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an Axis object that represents the primary X-axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxis"),
        SRDescription("DescriptionAttributeChartArea_AxisX"),
        ]
        public Axis AxisX
        {
            get
            {
                return axisX;
            }
            set
            {
                axisX = value;
                axisX.Initialize(this, AxisName.X);
                _axisArray[0] = axisX;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an Axis object that represents the secondary X-axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxis"),
        SRDescription("DescriptionAttributeChartArea_AxisX2"),
        ]
        public Axis AxisX2
        {
            get
            {
                return axisX2;
            }
            set
            {
                axisX2 = value;
                axisX2.Initialize(this, AxisName.X2);
                _axisArray[2] = axisX2;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an Axis object that represents the secondary Y-axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxis"),
        SRDescription("DescriptionAttributeChartArea_AxisY2"),
        ]
        public Axis AxisY2
        {
            get
            {
                return axisY2;
            }
            set
            {
                axisY2 = value;
                axisY2.Initialize(this, AxisName.Y2);
                _axisArray[3] = axisY2;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets an ElementPosition object, which defines the position of a chart area object within the chart image.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChartArea_Position"),
        ]
        public ElementPosition Position
        {
            get
            {
                return _areaPosition;
            }
            set
            {
                _areaPosition = value;
                _areaPosition.Parent = this;
                _areaPosition.resetAreaAutoPosition = true;
                Invalidate();
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
        /// Gets or sets an ElementPosition object, which defines the inner plot position of a chart area object.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChartArea_InnerPlotPosition"),
        ]
        public ElementPosition InnerPlotPosition
        {
            get
            {
                return _innerPlotPosition;
            }
            set
            {
                _innerPlotPosition = value;
                _innerPlotPosition.Parent = this;
                Invalidate();
            }
        }

        /// <summary>
        /// Determoines if this position should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeInnerPlotPosition()
        {
            return !InnerPlotPosition.Auto;
        }

        /// <summary>
        /// Gets or sets the background color of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the hatching style of a ChartArea object.
        /// </summary>
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background image of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the drawing mode of the background image of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of a ChartArea object's background image that will be drawn as transparent.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the alignment of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the orientation of a chart element's gradient,
        /// and also determines whether or not a gradient is used.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the secondary color of a ChartArea object.
        /// </summary>
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the shadow color of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the shadow offset (in pixels) of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the border color of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the border width of a ChartArea object.
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
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionBorderWidthIsNegative));
                }
                _borderWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the style of the border line of a ChartArea object.
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the unique name of a ChartArea object.
        /// </summary>
        [

        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeChartArea_Name"),
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
        /// Gets or sets a Boolean that determines if the labels of the axes for all chart area
        /// , which have LabelsAutoFit property set to true, are of equal size.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeChartArea_EquallySizedAxesFont"),
        ]
        public bool IsSameFontSKSizeorAllAxes
        {
            get
            {
                return _isSameFontSKSizeorAllAxes;
            }
            set
            {
                _isSameFontSKSizeorAllAxes = value;
                Invalidate();
            }
        }

        #endregion Chart Area properties

        #region Constructors

        /// <summary>
        /// ChartArea constructor.
        /// </summary>
        public ChartArea()
        {
            Initialize();
        }

        /// <summary>
        /// ChartArea constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        public ChartArea(string name) : base(name)
        {
            Initialize();
        }

        #endregion Constructors

        #region Chart Area Methods

        /// <summary>
        /// Restores series order and X axis reversed mode for the 3D charts.
        /// </summary>
        internal void Restore3DAnglesAndReverseMode()
        {
            if (Area3DStyle.Enable3D && !chartAreaIsCurcular)
            {
                // Restore axis "IsReversed" property and old Y angle
                AxisX.IsReversed = oldReverseX;
                AxisX2.IsReversed = oldReverseX;
                AxisY.IsReversed = oldReverseY;
                AxisY2.IsReversed = oldReverseY;
                Area3DStyle.Rotation = oldYAngle;
            }
        }

        /// <summary>
        /// Sets series order and X axis reversed mode for the 3D charts.
        /// </summary>
        internal void Set3DAnglesAndReverseMode()
        {
            // Clear series reversed flag
            _reverseSeriesOrder = false;

            // If 3D charting is enabled
            if (Area3DStyle.Enable3D)
            {
                // Make sure primary & secondary axis has the same IsReversed settings
                // This is a limitation for the 3D chart required for labels drawing.
                AxisX2.IsReversed = AxisX.IsReversed;
                AxisY2.IsReversed = AxisY.IsReversed;

                // Remember reversed order of X & Y axis and Angles
                oldReverseX = AxisX.IsReversed;
                oldReverseY = AxisY.IsReversed;
                oldYAngle = Area3DStyle.Rotation;

                // Check if Y angle
                if (Area3DStyle.Rotation > 90 || Area3DStyle.Rotation < -90)
                {
                    // This method depends on the 'switchValueAxes' field which is calculated based on the chart types
                    // of the series associated with the chart area. We need to call SetData method to make sure this field
                    // is correctly initialized. Because we only need to collect information about the series, we pass 'false'
                    // as parameters to limit the amount of work this function does.
                    SetData(false, false);

                    // Reversed series order
                    _reverseSeriesOrder = true;

                    // Reversed primary and secondary X axis
                    if (!switchValueAxes)
                    {
                        AxisX.IsReversed = !AxisX.IsReversed;
                        AxisX2.IsReversed = !AxisX2.IsReversed;
                    }

                    // Reversed primary and secondary Y axis for chart types like Bar
                    else
                    {
                        AxisY.IsReversed = !AxisY.IsReversed;
                        AxisY2.IsReversed = !AxisY2.IsReversed;
                    }

                    // Adjust Y angle
                    if (Area3DStyle.Rotation > 90)
                    {
                        Area3DStyle.Rotation = (Area3DStyle.Rotation - 90) - 90;
                    }
                    else if (Area3DStyle.Rotation < -90)
                    {
                        Area3DStyle.Rotation = (Area3DStyle.Rotation + 90) + 90;
                    }
                }
            }
        }

        /// <summary>
        /// Save all automatic values like Minimum and Maximum.
        /// </summary>
        internal void SetTempValues()
        {
            // Save non automatic area position
            if (!Position.Auto)
            {
                originalAreaPosition = Position.ToSKRect();
            }

            // Save non automatic area inner plot position
            if (!InnerPlotPosition.Auto)
            {
                originalInnerPlotPosition = InnerPlotPosition.ToSKRect();
            }

            _circularSectorNumber = int.MinValue;
            _circularUsePolygons = int.MinValue;
            _circularAxisList = null;

            // Save Minimum and maximum values for all axes
            axisX.StoreAxisValues();
            axisX2.StoreAxisValues();
            axisY.StoreAxisValues();
            axisY2.StoreAxisValues();
        }

        /// <summary>
        /// Load all automatic values like Minimum and Maximum with original values.
        /// </summary>
        internal void GetTempValues()
        {
            // Take Minimum and maximum values for all axes
            axisX.ResetAxisValues();
            axisX2.ResetAxisValues();
            axisY.ResetAxisValues();
            axisY2.ResetAxisValues();

            // Restore non automatic area position
            if (!originalAreaPosition.IsEmpty)
            {
                lastAreaPosition = Position.ToSKRect();
                Position.SetPositionNoAuto(originalAreaPosition.Left, originalAreaPosition.Top, originalAreaPosition.Width, originalAreaPosition.Height);
                originalAreaPosition = SKRect.Empty;
            }

            // Save non automatic area inner plot position
            if (!originalInnerPlotPosition.IsEmpty)
            {
                InnerPlotPosition.SetPositionNoAuto(originalInnerPlotPosition.Left, originalInnerPlotPosition.Top, originalInnerPlotPosition.Width, originalInnerPlotPosition.Height);
                originalInnerPlotPosition = SKRect.Empty;
            }
        }

        /// <summary>
        /// Initialize Chart area and axes
        /// </summary>
        internal void Initialize()
        {
            // Initialize 3D style class
            _area3DStyle = new ChartArea3DStyle(this);

            // Create axes for this chart area.
            axisY = new Axis();
            axisX = new Axis();
            axisX2 = new Axis();
            axisY2 = new Axis();

            // Initialize axes
            axisX.Initialize(this, AxisName.X);
            axisY.Initialize(this, AxisName.Y);
            axisX2.Initialize(this, AxisName.X2);
            axisY2.Initialize(this, AxisName.Y2);

            // Initialize axes array
            _axisArray[0] = axisX;
            _axisArray[1] = axisY;
            _axisArray[2] = axisX2;
            _axisArray[3] = axisY2;

            // Set flag to reset auto values for all areas
            _areaPosition = new ElementPosition(this)
            {
                resetAreaAutoPosition = true
            };

            _innerPlotPosition = new ElementPosition(this);

            // Set the position of the new chart area
            if (PlotAreaPosition == null)
            {
                PlotAreaPosition = new ElementPosition(this);
            }

            // Initialize cursor class
            _cursorX.Initialize(this, AxisName.X);
            _cursorY.Initialize(this, AxisName.Y);
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
            _axisArray[0].refreshMinMaxFromData = true;
            _axisArray[1].refreshMinMaxFromData = true;
            _axisArray[2].refreshMinMaxFromData = true;
            _axisArray[3].refreshMinMaxFromData = true;
        }

        /// <summary>
        /// Recalculates the axes scale of a chart area.
        /// </summary>
        public void RecalculateAxesScale()
        {
            // Read axis Max/Min from data
            ResetMinMaxFromData();

            Set3DAnglesAndReverseMode();
            SetTempValues();

            // Initialize area position
            _axisArray[0].ReCalc(PlotAreaPosition);
            _axisArray[1].ReCalc(PlotAreaPosition);
            _axisArray[2].ReCalc(PlotAreaPosition);
            _axisArray[3].ReCalc(PlotAreaPosition);

            // Find all Data and chart types which belong
            // to this chart area an set default values
            SetData();

            Restore3DAnglesAndReverseMode();
            GetTempValues();
        }

        /// <summary>
        /// RecalculateAxesScale the chart area
        /// </summary>
        internal void ReCalcInternal()
        {
            // Initialize area position
            _axisArray[0].ReCalc(PlotAreaPosition);
            _axisArray[1].ReCalc(PlotAreaPosition);
            _axisArray[2].ReCalc(PlotAreaPosition);
            _axisArray[3].ReCalc(PlotAreaPosition);

            // Find all Data and chart types which belong
            // to this chart area an set default values
            SetData();
        }

        /// <summary>
        /// Reset auto calculated chart area values.
        /// </summary>
        internal void ResetAutoValues()
        {
            _axisArray[0].ResetAutoValues();
            _axisArray[1].ResetAutoValues();
            _axisArray[2].ResetAutoValues();
            _axisArray[3].ResetAutoValues();
        }

        /// <summary>
        /// Calculates Position for the background.
        /// </summary>
        /// <param name="withScrollBars">Calculate with scroll bars</param>
        /// <returns>Background rectangle</returns>
        internal SKRect GetBackgroundPosition(bool withScrollBars)
        {
            // For pie and doughnut, which do not have axes, the position
            // for the background is Chart area position not plotting
            // area position.
            SKRect backgroundPosition = PlotAreaPosition.ToSKRect();
            if (!requireAxes)
            {
                backgroundPosition = Position.ToSKRect();
            }

            // Without scroll bars
            if (!withScrollBars)
            {
                return backgroundPosition;
            }

            // Add scroll bar rectangles to the area background
            SKRect backgroundPositionWithScrollBars = new(backgroundPosition.Left, backgroundPosition.Top, backgroundPosition.Right, backgroundPosition.Bottom);

            return backgroundPositionWithScrollBars;
        }

        /// <summary>
        /// Call when the chart area is resized.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        internal void Resize(ChartGraphics chartGraph)
        {
            // Initialize plotting area position
            SKRect plottingRect = Position.ToSKRect();
            if (!InnerPlotPosition.Auto)
            {
                plottingRect.Left += (Position.Width / 100F) * InnerPlotPosition.X;
                plottingRect.Top += (Position.Height / 100F) * InnerPlotPosition.Y;
                plottingRect.Right = plottingRect.Left + (Position.Width / 100F) * InnerPlotPosition.Width;
                plottingRect.Bottom = plottingRect.Top + (Position.Height / 100F) * InnerPlotPosition.Height;
            }

            //******************************************************
            //** Calculate number of vertical and horizontal axis
            //******************************************************
            int verticalAxes = 0;
            int horizontalAxes = 0;
            foreach (Axis axis in Axes)
            {
                if (axis.enabled)
                {
                    if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                        ++horizontalAxes;
                    }
                    else if (axis.AxisPosition == AxisPosition.Top)
                    {
                        ++horizontalAxes;
                    }
                    else if (axis.AxisPosition == AxisPosition.Left)
                    {
                        ++verticalAxes;
                    }
                    else if (axis.AxisPosition == AxisPosition.Right)
                    {
                        ++verticalAxes;
                    }
                }
            }
            if (horizontalAxes <= 0)
            {
                horizontalAxes = 1;
            }
            if (verticalAxes <= 0)
            {
                verticalAxes = 1;
            }

            //******************************************************
            //** Find same auto-fit font size
            //******************************************************
            Axis[] axisArray = (switchValueAxes) ?
                new Axis[] { AxisX, AxisX2, AxisY, AxisY2 } :
                new Axis[] { AxisY, AxisY2, AxisX, AxisX2 };
            if (IsSameFontSKSizeorAllAxes)
            {
                axesAutoFontSize = 20;
                foreach (Axis axis in axisArray)
                {
                    // Process only enabled axis
                    if (axis.enabled)
                    {
                        // Resize axis
                        if (axis.AxisPosition == AxisPosition.Bottom || axis.AxisPosition == AxisPosition.Top)
                        {
                            axis.Resize(chartGraph, PlotAreaPosition, plottingRect, horizontalAxes, InnerPlotPosition.Auto);
                        }
                        else
                        {
                            axis.Resize(chartGraph, PlotAreaPosition, plottingRect, verticalAxes, InnerPlotPosition.Auto);
                        }

                        // Calculate smallest font size
                        if (axis.IsLabelAutoFit && axis.autoLabelFont != null)
                        {
                            axesAutoFontSize = Math.Min(axesAutoFontSize, axis.autoLabelFont.Size);
                        }
                    }
                }
            }

            //******************************************************
            //** Adjust plotting area position according to the axes
            //** elements (title, labels, tick marks) size.
            //******************************************************
            SKRect rectLabelSideSpacing = SKRect.Empty;
            foreach (Axis axis in axisArray)
            {
                // Process only enabled axis
                if (!axis.enabled)
                {
                    //******************************************************
                    //** Adjust for the 3D Wall Width for disabled axis
                    //******************************************************
                    if (InnerPlotPosition.Auto && Area3DStyle.Enable3D && !chartAreaIsCurcular)
                    {
                        SKSize areaWallSize = chartGraph.GetRelativeSize(new SKSize(Area3DStyle.WallWidth, Area3DStyle.WallWidth));
                        if (axis.AxisPosition == AxisPosition.Bottom)
                        {
                            plottingRect.Bottom -= areaWallSize.Height;
                        }
                        else if (axis.AxisPosition == AxisPosition.Top)
                        {
                            plottingRect.Top += areaWallSize.Height;
                            plottingRect.Bottom -= areaWallSize.Height;
                        }
                        else if (axis.AxisPosition == AxisPosition.Right)
                        {
                            plottingRect.Right -= areaWallSize.Width;
                        }
                        else if (axis.AxisPosition == AxisPosition.Left)
                        {
                            plottingRect.Left += areaWallSize.Width;
                            plottingRect.Right -= areaWallSize.Width;
                        }
                    }

                    continue;
                }

                //******************************************************
                //** Calculate axes elements position
                //******************************************************
                if (axis.AxisPosition == AxisPosition.Bottom || axis.AxisPosition == AxisPosition.Top)
                {
                    axis.Resize(chartGraph, PlotAreaPosition, plottingRect, horizontalAxes, InnerPlotPosition.Auto);
                }
                else
                {
                    axis.Resize(chartGraph, PlotAreaPosition, plottingRect, verticalAxes, InnerPlotPosition.Auto);
                }

                // Shift top/bottom labels so they will not overlap with left/right labels
                PreventTopBottomAxesLabelsOverlapping(axis);

                //******************************************************
                //** Check axis position
                //******************************************************
                float axisPosition = (float)axis.GetAxisPosition();
                if (axis.AxisPosition == AxisPosition.Bottom)
                {
                    if (!axis.GetIsMarksNextToAxis())
                    {
                        axisPosition = plottingRect.Bottom;
                    }
                    axisPosition = plottingRect.Bottom - axisPosition;
                }
                else if (axis.AxisPosition == AxisPosition.Top)
                {
                    if (!axis.GetIsMarksNextToAxis())
                    {
                        axisPosition = plottingRect.Top;
                    }
                    axisPosition -= plottingRect.Top;
                }
                else if (axis.AxisPosition == AxisPosition.Right)
                {
                    if (!axis.GetIsMarksNextToAxis())
                    {
                        axisPosition = plottingRect.Right;
                    }
                    axisPosition = plottingRect.Right - axisPosition;
                }
                else if (axis.AxisPosition == AxisPosition.Left)
                {
                    if (!axis.GetIsMarksNextToAxis())
                    {
                        axisPosition = plottingRect.Left;
                    }
                    axisPosition -= plottingRect.Left;
                }

                //******************************************************
                //** Adjust axis elements size with axis position
                //******************************************************
                // Calculate total size of axis elements
                float axisSize = axis.markSize + axis.labelSize;

#if SUBAXES
					// Add sub-axis size
					if(!this.chartAreaIsCurcular && !this.Area3DStyle.Enable3D)
					{
						foreach(SubAxis subAxis in axis.SubAxes)
						{
							axisSize += subAxis.markSize + subAxis.labelSize + subAxis.titleSize;
						}
					}
#endif // SUBAXES

                // Adjust depending on the axis position
                axisSize -= axisPosition;
                if (axisSize < 0)
                {
                    axisSize = 0;
                }

                // Add axis title and scroll bar size (always outside of plotting area)
                axisSize += axis.titleSize + axis.scrollBarSize;

                // Calculate horizontal axes size for circualar area
                if (chartAreaIsCurcular &&
                    (axis.AxisPosition == AxisPosition.Top || axis.AxisPosition == AxisPosition.Bottom))
                {
                    axisSize = axis.titleSize + axis.markSize + axis.scrollBarSize;
                }

                //******************************************************
                //** Adjust plotting area
                //******************************************************
                if (InnerPlotPosition.Auto)
                {
                    if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                        plottingRect.Bottom -= axisSize;
                    }
                    else if (axis.AxisPosition == AxisPosition.Top)
                    {
                        plottingRect.Top += axisSize;
                        plottingRect.Bottom -= axisSize;
                    }
                    else if (axis.AxisPosition == AxisPosition.Left)
                    {
                        plottingRect.Left += axisSize;
                        plottingRect.Right -= axisSize;
                    }
                    else if (axis.AxisPosition == AxisPosition.Right)
                    {
                        plottingRect.Right -= axisSize;
                    }

                    // Check if labels side offset should be processed
                    bool addLabelsSideOffsets = true;

                    // Update the plotting area depending on the size required for labels on the sides
                    if (addLabelsSideOffsets)
                    {
                        if (axis.AxisPosition == AxisPosition.Bottom || axis.AxisPosition == AxisPosition.Top)
                        {
                            if (axis.labelNearOffset != 0 && axis.labelNearOffset < Position.X)
                            {
                                float offset = Position.X - axis.labelNearOffset;
                                if (Math.Abs(offset) > plottingRect.Width * 0.3f)
                                {
                                    offset = plottingRect.Width * 0.3f;
                                }

                                // NOTE: Code was removed to solve an issue with extra space when labels angle = 45
                                //rectLabelSideSpacing.Width = (float)Math.Max(offset, rectLabelSideSpacing.Width);
                                rectLabelSideSpacing.Left = Math.Max(offset, rectLabelSideSpacing.Left);
                            }

                            if (axis.labelFarOffset > Position.Right)
                            {
                                if ((axis.labelFarOffset - Position.Right) < plottingRect.Width * 0.3f)
                                {
                                    rectLabelSideSpacing.Right = rectLabelSideSpacing.Left + Math.Max(axis.labelFarOffset - Position.Right, rectLabelSideSpacing.Width);
                                }
                                else
                                {
                                    rectLabelSideSpacing.Right = rectLabelSideSpacing.Left + Math.Max(plottingRect.Width * 0.3f, rectLabelSideSpacing.Width);
                                }
                            }
                        }
                        else
                        {
                            if (axis.labelNearOffset != 0 && axis.labelNearOffset < Position.Y)
                            {
                                float offset = Position.Y - axis.labelNearOffset;
                                if (Math.Abs(offset) > plottingRect.Height * 0.3f)
                                {
                                    offset = plottingRect.Height * 0.3f;
                                }

                                // NOTE: Code was removed to solve an issue with extra space when labels angle = 45
                                //rectLabelSideSpacing.Height = (float)Math.Max(offset, rectLabelSideSpacing.Height);
                                rectLabelSideSpacing.Top = Math.Max(offset, rectLabelSideSpacing.Top);
                            }

                            if (axis.labelFarOffset > Position.Bottom)
                            {
                                if ((axis.labelFarOffset - Position.Bottom) < plottingRect.Height * 0.3f)
                                {
                                    rectLabelSideSpacing.Bottom = rectLabelSideSpacing.Top + Math.Max(axis.labelFarOffset - Position.Bottom, rectLabelSideSpacing.Height);
                                }
                                else
                                {
                                    rectLabelSideSpacing.Bottom = rectLabelSideSpacing.Top + Math.Max(plottingRect.Height * 0.3f, rectLabelSideSpacing.Height);
                                }
                            }
                        }
                    }
                }
            }

            //******************************************************
            //** Make sure there is enough space
            //** for labels on the chart sides
            //******************************************************
            if (!chartAreaIsCurcular)
            {
                if (rectLabelSideSpacing.Top > 0 && rectLabelSideSpacing.Top > plottingRect.Top - Position.Y)
                {
                    float delta = (plottingRect.Top - Position.Y) - rectLabelSideSpacing.Top;
                    plottingRect.Top -= delta;
                    plottingRect.Bottom += delta;
                }
                if (rectLabelSideSpacing.Left > 0 && rectLabelSideSpacing.Left > plottingRect.Left - Position.X)
                {
                    float delta = (plottingRect.Left - Position.X) - rectLabelSideSpacing.Left;
                    plottingRect.Left -= delta;
                    plottingRect.Right += delta;
                }
                if (rectLabelSideSpacing.Height > 0 && rectLabelSideSpacing.Height > Position.Bottom - plottingRect.Bottom)
                {
                    plottingRect.Bottom += (Position.Bottom - plottingRect.Bottom) - rectLabelSideSpacing.Height;
                }
                if (rectLabelSideSpacing.Width > 0 && rectLabelSideSpacing.Width > Position.Right - plottingRect.Right)
                {
                    plottingRect.Bottom += (Position.Right - plottingRect.Right) - rectLabelSideSpacing.Width;
                }
            }

            //******************************************************
            //** Plotting area must be square for the circular
            //** chart area (in pixels).
            //******************************************************
            if (chartAreaIsCurcular)
            {
                // Adjust area to fit the axis title
                float xTitleSize = Math.Max(AxisY.titleSize, AxisY2.titleSize);
                if (xTitleSize > 0)
                {
                    plottingRect.Left += xTitleSize;
                    plottingRect.Right -= 2f * xTitleSize;
                }
                float yTitleSize = Math.Max(AxisX.titleSize, AxisX2.titleSize);
                if (yTitleSize > 0)
                {
                    plottingRect.Top += yTitleSize;
                    plottingRect.Bottom -= 2f * yTitleSize;
                }

                // Make a square plotting rect
                SKRect rect = chartGraph.GetAbsoluteRectangle(plottingRect);
                if (rect.Width > rect.Height)
                {
                    rect.Left += (rect.Width - rect.Height) / 2f;
                    rect.Size = new(rect.Height, rect.Height);
                }
                else
                {
                    rect.Top += (rect.Height - rect.Width) / 2f;
                    rect.Size = new(rect.Width, rect.Width);
                }
                plottingRect = chartGraph.GetRelativeRectangle(rect);

                // Remember circular chart area center
                circularCenter = new SKPoint(plottingRect.Left + plottingRect.Width / 2f, plottingRect.Top + plottingRect.Height / 2f);

                // Calculate auto-fit font of the circular axis labels and update area position
                FitCircularLabels(chartGraph, PlotAreaPosition, ref plottingRect, xTitleSize, yTitleSize);
            }

            //******************************************************
            //** Set plotting area position
            //******************************************************
            if (plottingRect.Width < 0f)
            {
                plottingRect.Right = plottingRect.Left;
            }
            if (plottingRect.Height < 0f)
            {
                plottingRect.Bottom = plottingRect.Top;
            }
            PlotAreaPosition.FromSKRect(plottingRect);
            InnerPlotPosition.SetPositionNoAuto(
                (float)Math.Round((plottingRect.Left - Position.X) / (Position.Width / 100F), 5),
                (float)Math.Round((plottingRect.Top - Position.Y) / (Position.Height / 100F), 5),
                (float)Math.Round(plottingRect.Width / (Position.Width / 100F), 5),
                (float)Math.Round(plottingRect.Height / (Position.Height / 100F), 5));

            //******************************************************
            //** Adjust label font size for axis, which were
            //** automatically calculated after the opposite axis
            //** change the size of plotting area.
            //******************************************************
            AxisY2.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
            AxisY.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
            if (InnerPlotPosition.Auto)
            {
                AxisX2.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
                AxisX.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
            }
        }

        /// <summary>
        /// Finds axis by it's position. Can be Null.
        /// </summary>
        /// <param name="axisPosition">Axis position to find</param>
        /// <returns>Found axis.</returns>
        private Axis FindAxis(AxisPosition axisPosition)
        {
            foreach (Axis axis in Axes)
            {
                if (axis.AxisPosition == axisPosition)
                {
                    return axis;
                }
            }
            return null;
        }

        /// <summary>
        /// Shift top/bottom labels so they will not overlap with left/right labels.
        /// </summary>
        /// <param name="axis">Axis to shift up/down.</param>
        private void PreventTopBottomAxesLabelsOverlapping(Axis axis)
        {
            // If axis is not on the edge of the chart area do not
            // try to adjust it's position when axis labels overlap
            // labels of the oppositie axis.
            if (!axis.IsAxisOnAreaEdge)
            {
                return;
            }

            // Shift bottom axis labels down
            if (axis.AxisPosition == AxisPosition.Bottom)
            {
                // Get labels position
                float labelsPosition = (float)axis.GetAxisPosition();
                if (!axis.GetIsMarksNextToAxis())
                {
                    labelsPosition = axis.PlotAreaPosition.Bottom;
                }

                // Only adjust labels outside plotting area
                if (Math.Round(labelsPosition, 2) < Math.Round(axis.PlotAreaPosition.Bottom, 2))
                {
                    return;
                }

                // Check if labels may overlap with Left axis
                Axis leftAxis = FindAxis(AxisPosition.Left);
                if (leftAxis != null &&
                    leftAxis.enabled &&
                    leftAxis.labelFarOffset != 0 &&
                    leftAxis.labelFarOffset > labelsPosition &&
                    axis.labelNearOffset != 0 &&
                    axis.labelNearOffset < PlotAreaPosition.X)
                {
                    float overlap = (leftAxis.labelFarOffset - labelsPosition) * 0.75f;
                    if (overlap > axis.markSize)
                    {
                        axis.markSize += overlap - axis.markSize;
                    }
                }

                // Check if labels may overlap with Right axis
                Axis rightAxis = FindAxis(AxisPosition.Right);
                if (rightAxis != null &&
                    rightAxis.enabled &&
                    rightAxis.labelFarOffset != 0 &&
                    rightAxis.labelFarOffset > labelsPosition &&
                    axis.labelFarOffset != 0 &&
                    axis.labelFarOffset > PlotAreaPosition.Right)
                {
                    float overlap = (rightAxis.labelFarOffset - labelsPosition) * 0.75f;
                    if (overlap > axis.markSize)
                    {
                        axis.markSize += overlap - axis.markSize;
                    }
                }
            }

            // Shift top axis labels up
            else if (axis.AxisPosition == AxisPosition.Top)
            {
                // Get labels position
                float labelsPosition = (float)axis.GetAxisPosition();
                if (!axis.GetIsMarksNextToAxis())
                {
                    labelsPosition = axis.PlotAreaPosition.Y;
                }

                // Only adjust labels outside plotting area
                if (Math.Round(labelsPosition, 2) < Math.Round(axis.PlotAreaPosition.Y, 2))
                {
                    return;
                }

                // Check if labels may overlap with Left axis
                Axis leftAxis = FindAxis(AxisPosition.Left);
                if (leftAxis != null &&
                    leftAxis.enabled &&
                    leftAxis.labelNearOffset != 0 &&
                    leftAxis.labelNearOffset < labelsPosition &&
                    axis.labelNearOffset != 0 &&
                    axis.labelNearOffset < PlotAreaPosition.X)
                {
                    float overlap = (labelsPosition - leftAxis.labelNearOffset) * 0.75f;
                    if (overlap > axis.markSize)
                    {
                        axis.markSize += overlap - axis.markSize;
                    }
                }

                // Check if labels may overlap with Right axis
                Axis rightAxis = FindAxis(AxisPosition.Right);
                if (rightAxis != null &&
                    rightAxis.enabled &&
                    rightAxis.labelNearOffset != 0 &&
                    rightAxis.labelNearOffset < labelsPosition &&
                    axis.labelFarOffset != 0 &&
                    axis.labelFarOffset > PlotAreaPosition.Right)
                {
                    float overlap = (labelsPosition - rightAxis.labelNearOffset) * 0.75f;
                    if (overlap > axis.markSize)
                    {
                        axis.markSize += overlap - axis.markSize;
                    }
                }
            }
        }

        #endregion Chart Area Methods

        #region Painting and Selection Methods

        /// <summary>
        /// Draws chart area background and/or border.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="position">Background position.</param>
        /// <param name="borderOnly">Draws chart area border only.</param>
        private void PaintAreaBack(ChartGraphics graph, SKRect position, bool borderOnly)
        {
            if (!borderOnly)
            {
                // Draw background
                if (!Area3DStyle.Enable3D || !requireAxes || chartAreaIsCurcular)
                {
                    // 3D Pie Chart doesn't need scene
                    // Draw 2D background
                    graph.FillRectangleRel(
                        position,
                        BackColor,
                        BackHatchStyle,
                        BackImage,
                        BackImageWrapMode,
                        BackImageTransparentColor,
                        BackImageAlignment,
                        BackGradientStyle,
                        BackSecondaryColor,
                        (requireAxes) ? SKColor.Empty : BorderColor,
                        (requireAxes) ? 0 : BorderWidth,
                        BorderDashStyle,
                        ShadowColor,
                        ShadowOffset,
                        PenAlignment.Outset,
                        chartAreaIsCurcular,
                        (chartAreaIsCurcular && CircularUsePolygons) ? CircularSectorsNumber : 0,
                        Area3DStyle.Enable3D);
                }
                else
                {
                    // Draw chart area 3D scene
                    DrawArea3DScene(graph, position);
                }
            }
            else
            {
                if ((!Area3DStyle.Enable3D || !requireAxes || chartAreaIsCurcular) && BorderColor != SKColor.Empty && BorderWidth > 0)
                {


                    graph.FillRectangleRel(position,
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        "",
                        ChartImageWrapMode.Tile,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        BorderColor,
                        BorderWidth,
                        BorderDashStyle,
                        SKColor.Empty,
                        0,
                        PenAlignment.Outset,
                        chartAreaIsCurcular,
                        (chartAreaIsCurcular && CircularUsePolygons) ? CircularSectorsNumber : 0,
                        Area3DStyle.Enable3D);
                }
            }
        }

        /// <summary>
        /// Paint the chart area.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        internal void Paint(ChartGraphics graph)
        {
            // Check if plot area position was recalculated.
            // If not and non-auto InnerPlotPosition & Position were
            // specified - do all needed calculations
            if (PlotAreaPosition.Width == 0 &&
                PlotAreaPosition.Height == 0 &&
                !InnerPlotPosition.Auto
                && !Position.Auto)
            {
                // Initialize plotting area position
                SKRect plottingRect = Position.ToSKRect();
                if (!InnerPlotPosition.Auto)
                {
                    plottingRect.Left += (Position.Width / 100F) * InnerPlotPosition.X;
                    plottingRect.Top += (Position.Height / 100F) * InnerPlotPosition.Y;
                    plottingRect.Size = new(
                        (Position.Width / 100F) * InnerPlotPosition.Width,
                        (Position.Height / 100F) * InnerPlotPosition.Height);
                }

                PlotAreaPosition.FromSKRect(plottingRect);
            }

            // Get background position rectangle.
            SKRect backgroundPositionWithScrollBars = GetBackgroundPosition(true);
            SKRect backgroundPosition = GetBackgroundPosition(false);

            // Add hot region for plotting area.
            if (Common.ProcessModeRegions)
            {
                Common.HotRegionsList.AddHotRegion(backgroundPosition, this, ChartElementType.PlottingArea, true);
            }
            // Draw background
            PaintAreaBack(graph, backgroundPositionWithScrollBars, false);

            // Call BackPaint event
            Common.Chart.CallOnPrePaint(new ChartPaintEventArgs(this, graph, Common, PlotAreaPosition));

            // Draw chart types without axes - Pie.
            if (!requireAxes && ChartTypes.Count != 0)
            {
                // Find first chart type that do not require axis (like Pie) and draw it.
                // Chart types that do not require axes (circular charts) cannot be combined with
                // any other chart types.
                // NOTE: Fixes issues #4672 and #4692
                for (int chartTypeIndex = 0; chartTypeIndex < ChartTypes.Count; chartTypeIndex++)
                {
                    IChartType chartType = Common.ChartTypeRegistry.GetChartType((string)ChartTypes[chartTypeIndex]);
                    if (!chartType.RequireAxes)
                    {
                        chartType.Paint(graph, Common, this, null);
                        break;
                    }
                }

                // Call Paint event
                Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, graph, Common, PlotAreaPosition));
                return;
            }

            // Reset Smart Labels
            smartLabels.Reset();

            // Set values for optimized drawing
            foreach (Axis currentAxis in _axisArray)
            {
                currentAxis.optimizedGetPosition = true;
                currentAxis.paintViewMax = currentAxis.ViewMaximum;
                currentAxis.paintViewMin = currentAxis.ViewMinimum;
                currentAxis.paintRange = currentAxis.paintViewMax - currentAxis.paintViewMin;
                currentAxis.paintAreaPosition = PlotAreaPosition.ToSKRect();
                if (currentAxis.ChartArea != null && currentAxis.ChartArea.chartAreaIsCurcular)
                {
                    // Update position for circular chart area
                    currentAxis.paintAreaPosition.Size = new SKSize(
                        currentAxis.paintAreaPosition.Width / 2.0f,
                        currentAxis.paintAreaPosition.Height / 2.0f);
                }
                currentAxis.paintAreaPositionBottom = currentAxis.paintAreaPosition.Top + currentAxis.paintAreaPosition.Height;
                currentAxis.paintAreaPositionRight = currentAxis.paintAreaPosition.Left + currentAxis.paintAreaPosition.Width;
                if (currentAxis.AxisPosition == AxisPosition.Top || currentAxis.AxisPosition == AxisPosition.Bottom)
                    currentAxis.paintChartAreaSize = currentAxis.paintAreaPosition.Width;
                else
                    currentAxis.paintChartAreaSize = currentAxis.paintAreaPosition.Height;

                currentAxis.valueMultiplier = 0.0;
                if (currentAxis.paintRange != 0)
                {
                    currentAxis.valueMultiplier = currentAxis.paintChartAreaSize / currentAxis.paintRange;
                }
            }

            // Draw Axis Striplines (only when StripWidth > 0)
            bool useScaleSegments;
            Axis[] axesArray = new Axis[] { axisY, axisY2, axisX, axisX2 };
            foreach (Axis currentAxis in axesArray)
            {
                useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                if (!useScaleSegments)
                {
                    currentAxis.PaintStrips(graph, false);
                }
                else
                {
                    foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                    {
                        scaleSegment.SetTempAxisScaleAndInterval();

                        currentAxis.PaintStrips(graph, false);

                        scaleSegment.RestoreAxisScaleAndInterval();
                    }
                }
            }

            // Draw Axis Grids
            axesArray = new Axis[] { axisY, axisX2, axisY2, axisX };
            foreach (Axis currentAxis in axesArray)
            {
                useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                if (!useScaleSegments)
                {
                    currentAxis.PaintGrids(graph);
                }
                else
                {
                    foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                    {
                        scaleSegment.SetTempAxisScaleAndInterval();

                        currentAxis.PaintGrids(graph);

                        scaleSegment.RestoreAxisScaleAndInterval();
                    }
                }
            }

            // Draw Axis Striplines (only when StripWidth == 0)
            foreach (Axis currentAxis in axesArray)
            {
                useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                if (!useScaleSegments)
                {
                    currentAxis.PaintStrips(graph, true);
                }
                else
                {
                    foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                    {
                        scaleSegment.SetTempAxisScaleAndInterval();

                        currentAxis.PaintStrips(graph, true);

                        scaleSegment.RestoreAxisScaleAndInterval();
                    }
                }
            }

            // Draw Axis elements on the back of the 3D scene
            if (Area3DStyle.Enable3D && !chartAreaIsCurcular)
            {
                foreach (Axis currentAxis in axesArray)
                {
                    useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                    if (!useScaleSegments)
                    {
                        currentAxis.PrePaint(graph);
                    }
                    else
                    {
                        foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                        {
                            scaleSegment.SetTempAxisScaleAndInterval();

                            currentAxis.PrePaint(graph);

                            scaleSegment.RestoreAxisScaleAndInterval();
                        }
                    }
                }
            }

            // Draws chart area border
            bool borderDrawn = false;
            if (Area3DStyle.Enable3D || !IsBorderOnTopSeries())
            {
                borderDrawn = true;
                PaintAreaBack(graph, backgroundPosition, true);
            }

            // Draw chart types
            if (!Area3DStyle.Enable3D || chartAreaIsCurcular)
            {
                // Drawing in 2D space

                // NOTE: Fixes issue #6443 and #5385
                // If two chart series of the same type (for example Line) are separated
                // by other series (for example Area) the order is not correct.
                // Old implementation draws ALL series that belongs to the chart type.
                ArrayList typeAndSeries = GetChartTypesAndSeriesToDraw();

                // Draw series by chart type or by series
                foreach (ChartTypeAndSeriesInfo chartTypeInfo in typeAndSeries)
                {
                    IterationCounter = 0;
                    IChartType type = Common.ChartTypeRegistry.GetChartType(chartTypeInfo.ChartType);

                    // If 'chartTypeInfo.Series' set to NULL all series of that chart type are drawn at once
                    // TODO Reenable: type.Paint(graph, Common, this, chartTypeInfo.Series);
                }
            }
            else
            {
                // Drawing in 3D space
                PaintChartSeries3D(graph);
            }

            // Draw area border if it wasn't drawn prior to the series
            if (!borderDrawn)
            {
                // TODO: This is drawing a rectangle that is too small
                PaintAreaBack(graph, backgroundPosition, true);
            }

            // Draw Axis
            foreach (Axis currentAxis in axesArray)
            {
                useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                if (!useScaleSegments)
                {
                    // Paint axis and Reset temp axis offset for side-by-side charts like column
                    currentAxis.Paint(graph);
                }
                else
                {
                    // Some of the axis elements like grid lines and tickmarks
                    // are drawn for each segment
                    foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                    {
                        scaleSegment.SetTempAxisScaleAndInterval();

                        currentAxis.PaintOnSegmentedScalePassOne(graph);

                        scaleSegment.RestoreAxisScaleAndInterval();
                    }

                    // Other elements like labels, title, axis line are drawn once
                    currentAxis.PaintOnSegmentedScalePassTwo(graph);
                }
            }

            // Call Paint event
            Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, graph, Common, PlotAreaPosition));

            // Draw axis scale break lines
            axesArray = new Axis[] { axisY, axisY2 };
            foreach (Axis currentAxis in axesArray)
            {
                for (int segmentIndex = 0; segmentIndex < (currentAxis.ScaleSegments.Count - 1); segmentIndex++)
                {
                    currentAxis.ScaleSegments[segmentIndex].PaintBreakLine(graph, currentAxis.ScaleSegments[segmentIndex + 1]);
                }
            }

            // Reset values for optimized drawing
            foreach (Axis curentAxis in _axisArray)
            {
                curentAxis.optimizedGetPosition = false;

                // Reset preffered number of intervals on the axis
                curentAxis.prefferedNumberofIntervals = 5;

                // Reset flag that scale segments are used
                curentAxis.scaleSegmentsUsed = false;
            }
        }

        /// <summary>
        /// Checks if chart area border should be drawn on top of series.
        /// </summary>
        /// <returns>True if border should be darwn on top.</returns>
        private bool IsBorderOnTopSeries()
        {
            // For most of the chart types chart area border is drawn on top.
            bool result = true;
            foreach (Series series in Common.Chart.Series)
            {
                // It is common for the Bubble and Point chart types to draw markers
                // partially outside of the chart area. By drawing the border before
                // series we avoiding the possibility of drawing the border line on
                // top of the marker.
                if (series.ChartArea == Name && (series.ChartType == SeriesChartType.Bubble ||
                        series.ChartType == SeriesChartType.Point))
                {
                    return false;
                }
            }
            return result;
        }

        /// <summary>
        /// Paint the chart area cursors.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="cursorOnly">Indicates that only cursors are redrawn.</param>
        internal void PaintCursors(ChartGraphics graph, bool cursorOnly)
        {
            // Cursors and selection are supoorted only in 2D charts
            if (Area3DStyle.Enable3D)
            {
                return;
            }

            // Do not draw cursor/selection for chart types that do not require axis (like Pie)
            if (!requireAxes)
            {
                return;
            }

            // Cursors and selection are not supoorted in circular areas
            if (chartAreaIsCurcular)
            {
                return;
            }

            // Do not draw cursor/selection while printing
            if (Common != null &&
                Common.ChartPicture != null &&
                Common.ChartPicture.isPrinting)
            {
                return;
            }

            // Do not draw cursor/selection when chart area is not visible
            // because either width or height is set to zero
            if (Position.Width == 0f ||
                Position.Height == 0f)
            {
                return;
            }

            ChartPicture chartPicture = Common.ChartPicture;

            // Check if cursor should be drawn
            if (!double.IsNaN(_cursorX.SelectionStart) ||
                !double.IsNaN(_cursorX.SelectionEnd) ||
                !double.IsNaN(_cursorX.Position) ||
                !double.IsNaN(_cursorY.SelectionStart) ||
                !double.IsNaN(_cursorY.SelectionEnd) ||
                !double.IsNaN(_cursorY.Position))
            {
                if (!chartPicture.backgroundRestored &&
                    !chartPicture.isSelectionMode)
                {
                    chartPicture.backgroundRestored = true;

                    // Get chart area position
                    SKRect absAreaPlotPosition = (graph.GetAbsoluteRectangle(PlotAreaPosition.ToSKRect())).Round();
                    int maxCursorWidth = (CursorY.LineWidth > CursorX.LineWidth) ? CursorY.LineWidth + 1 : CursorX.LineWidth + 1;
                    absAreaPlotPosition.Inflate(maxCursorWidth, maxCursorWidth);
                    absAreaPlotPosition.Intersect(new SKRect(0, 0, Common.Width, Common.Height));

                    // Create area buffer bitmap
                    if (areaBufferBitmap == null ||
                        chartPicture.nonTopLevelChartBuffer == null ||
                        !cursorOnly)
                    {
                        // Dispose previous bitmap
                        if (areaBufferBitmap != null)
                        {
                            areaBufferBitmap.Dispose();
                            areaBufferBitmap = null;
                        }
                        if (chartPicture.nonTopLevelChartBuffer != null)
                        {
                            chartPicture.nonTopLevelChartBuffer.Dispose();
                            chartPicture.nonTopLevelChartBuffer = null;
                        }
                    }
                }

                // Draw chart area cursors and range selection

                _cursorY.Paint(graph);
                _cursorX.Paint(graph);
            }
        }

        #endregion Painting and Selection Methods

        #region Circular chart area methods

        /// <summary>
        /// Gets a circular chart type interface that belongs to this chart area.
        /// </summary>
        /// <returns>ICircularChartType interface or null.</returns>
        internal ICircularChartType GetCircularChartType()
        {
            // Get number of sectors in circular chart area
            foreach (Series series in Common.DataManager.Series)
            {
                if (series.IsVisible() && series.ChartArea == Name && Common.ChartTypeRegistry.GetChartType(series.ChartTypeName) is ICircularChartType type)
                {
                    return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculate size of the circular axis labels and sets auto-fit font.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="chartAreaPosition">The Chart area position.</param>
        /// <param name="plotArea">Plotting area size.</param>
        /// <param name="xTitleSize">Size of title on the axis.</param>
        /// <param name="yTitleSize">Size of title on the axis.</param>
        internal void FitCircularLabels(
            ChartGraphics chartGraph,
            ElementPosition chartAreaPosition,
            ref SKRect plotArea,
            float xTitleSize,
            float yTitleSize)
        {
            // Check if axis labels are enabled
            if (!AxisX.LabelStyle.Enabled)
            {
                return;
            }

            // Get absolute titles size
            SKSize titleSize = chartGraph.GetAbsoluteSize(new SKSize(xTitleSize, yTitleSize));

            // Get absolute position of area
            SKRect plotAreaRectAbs = chartGraph.GetAbsoluteRectangle(plotArea);
            SKRect areaRectAbs = chartGraph.GetAbsoluteRectangle(chartAreaPosition.ToSKRect());

            // Get absolute markers size and spacing
            float spacing = chartGraph.GetAbsolutePoint(new SKPoint(0, AxisX.markSize + Axis.elementSpacing)).Y;

            // Get circular axis list
            ArrayList axisList = GetCircularAxisList();

            // Get circular axis labels style
            CircularAxisLabelsStyle labelsStyle = GetCircularAxisLabelsStyle();

            //*****************************************************************
            //** Calculate the auto-fit font if required
            //*****************************************************************
            if (AxisX.LabelStyle.Enabled && AxisX.IsLabelAutoFit)
            {
                // Set max auto fit font
                AxisX.autoLabelFont = Common.ChartPicture.FontCache.GetFont(
                    AxisX.LabelStyle.Font.Typeface.FamilyName,
                    14,
                    AxisX.LabelStyle.Font.Typeface.FontStyle);

                // Get estimated labels size
                float labelsSizeEstimate = GetCircularLabelsSize(chartGraph, areaRectAbs, plotAreaRectAbs, titleSize);
                labelsSizeEstimate = Math.Min(labelsSizeEstimate * 1.1f, plotAreaRectAbs.Width / 5f);
                labelsSizeEstimate += spacing;

                // Calculate auto-fit font
                AxisX.GetCircularAxisLabelsAutoFitFont(chartGraph, axisList, labelsStyle, plotAreaRectAbs, areaRectAbs, labelsSizeEstimate);
            }

            //*****************************************************************
            //** Shrink plot area size proportionally
            //*****************************************************************

            // Get labels size
            float labelsSize = GetCircularLabelsSize(chartGraph, areaRectAbs, plotAreaRectAbs, titleSize);

            // Check if change size is smaller than radius
            labelsSize = Math.Min(labelsSize, plotAreaRectAbs.Width / 2.5f);
            labelsSize += spacing;

            plotAreaRectAbs.Left += labelsSize;
            plotAreaRectAbs.Right -= 2f * labelsSize;
            plotAreaRectAbs.Top += labelsSize;
            plotAreaRectAbs.Bottom -= 2f * labelsSize;

            // Restrict minimum plot area size
            if (plotAreaRectAbs.Width < 1.0f)
            {
                plotAreaRectAbs.Right = plotAreaRectAbs.Left + 1.0f;
            }
            if (plotAreaRectAbs.Height < 1.0f)
            {
                plotAreaRectAbs.Bottom = plotAreaRectAbs.Top + 1.0f;
            }

            plotArea = chartGraph.GetRelativeRectangle(plotAreaRectAbs);

            //*****************************************************************
            //** Set axes labels size
            //*****************************************************************
            SKSize relativeLabelSize = chartGraph.GetRelativeSize(new SKSize(labelsSize, labelsSize));
            AxisX.labelSize = relativeLabelSize.Height;
            AxisX2.labelSize = relativeLabelSize.Height;
            AxisY.labelSize = relativeLabelSize.Width;
            AxisY2.labelSize = relativeLabelSize.Width;
        }

        /// <summary>
        /// Calculate size of the circular axis labels.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="areaRectAbs">The Chart area position.</param>
        /// <param name="plotAreaRectAbs">Plotting area size.</param>
        /// <param name="titleSize">Size of title on the axes.</param>
        /// <returns>Circulat labels style.</returns>
        internal float GetCircularLabelsSize(
            ChartGraphics chartGraph,
            SKRect areaRectAbs,
            SKRect plotAreaRectAbs,
            SKSize titleSize)
        {
            // Find current horiz. and vert. spacing between plotting and chart areas
            SKSize areaDiff = new(plotAreaRectAbs.Left - areaRectAbs.Left, plotAreaRectAbs.Top - areaRectAbs.Top);
            areaDiff.Width -= titleSize.Width;
            areaDiff.Height -= titleSize.Height;

            // Get absolute center of the area
            SKPoint areaCenterAbs = chartGraph.GetAbsolutePoint(circularCenter);

            // Get circular axis list
            ArrayList axisList = GetCircularAxisList();

            // Get circular axis labels style
            CircularAxisLabelsStyle labelsStyle = GetCircularAxisLabelsStyle();

            // Defines on how much (pixels) the circular chart area radius should be reduced
            float labelsSize = 0f;

            //*****************************************************************
            //** Loop through all axis labels
            //*****************************************************************
            foreach (CircularChartAreaAxis axis in axisList)
            {
                //*****************************************************************
                //** Measure label text
                //*****************************************************************
                SKSize textSize = chartGraph.MeasureString(
                    axis.Title.Replace("\\n", "\n"),
                    AxisX.autoLabelFont ?? AxisX.LabelStyle.Font);
                textSize.Width = (float)Math.Ceiling(textSize.Width * 1.1f);
                textSize.Height = (float)Math.Ceiling(textSize.Height * 1.1f);

                //*****************************************************************
                //** Calculate area size change depending on labels style
                //*****************************************************************
                if (labelsStyle == CircularAxisLabelsStyle.Circular)
                {
                    labelsSize = Math.Max(
                        labelsSize,
                        textSize.Height);
                }
                else if (labelsStyle == CircularAxisLabelsStyle.Radial)
                {
                    float textAngle = axis.AxisPosition + 90;

                    // For angled text find it's X and Y components
                    float width = (float)Math.Cos(textAngle / 180F * Math.PI) * textSize.Width;
                    float height = (float)Math.Sin(textAngle / 180F * Math.PI) * textSize.Width;
                    width = (float)Math.Abs(Math.Ceiling(width));
                    height = (float)Math.Abs(Math.Ceiling(height));

                    // Reduce text size by current spacing between plotting area and chart area
                    width -= areaDiff.Width;
                    height -= areaDiff.Height;
                    if (width < 0)
                        width = 0;
                    if (height < 0)
                        height = 0;

                    labelsSize = Math.Max(
                        labelsSize,
                        Math.Max(width, height));
                }
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
                    SKMatrix newMatrix = SKMatrix.CreateRotationDegrees(textAngle, areaCenterAbs.X, areaCenterAbs.Y);
                    newMatrix.TransformPoints(labelPosition);

                    // Calculate width
                    float width = textSize.Width;
                    width -= areaRectAbs.Right - labelPosition[0].X;
                    if (width < 0f)
                    {
                        width = 0f;
                    }

                    labelsSize = Math.Max(
                        labelsSize,
                        Math.Max(width, textSize.Height));
                }
            }

            return labelsSize;
        }

        /// <summary>
        /// True if polygons should be used instead of the circles for the chart area.
        /// </summary>
        internal bool CircularUsePolygons
        {
            get
            {
                // Check if value was precalculated
                if (_circularUsePolygons == int.MinValue)
                {
                    _circularUsePolygons = 0;

                    // Look for custom properties in series
                    foreach (Series series in Common.DataManager.Series)
                    {
                        if (series.ChartArea == Name && series.IsVisible() && series.IsCustomPropertySet(CustomPropertyName.AreaDrawingStyle))
                        {
                            if (String.Compare(series[CustomPropertyName.AreaDrawingStyle], "Polygon", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _circularUsePolygons = 1;
                            }
                            else if (String.Compare(series[CustomPropertyName.AreaDrawingStyle], "Circle", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _circularUsePolygons = 0;
                            }
                            else
                            {
                                throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(series[CustomPropertyName.AreaDrawingStyle], "AreaDrawingStyle")));
                            }
                            break;
                        }
                    }
                }

                return (_circularUsePolygons == 1);
            }
        }

        /// <summary>
        /// Gets circular area axis labels style.
        /// </summary>
        /// <returns>Axis labels style.</returns>
        internal CircularAxisLabelsStyle GetCircularAxisLabelsStyle()
        {
            CircularAxisLabelsStyle style = CircularAxisLabelsStyle.Auto;

            // Get maximum number of points in all series
            foreach (Series series in Common.DataManager.Series)
            {
                if (series.IsVisible() && series.ChartArea == Name && series.IsCustomPropertySet(CustomPropertyName.CircularLabelsStyle))
                {
                    string styleName = series[CustomPropertyName.CircularLabelsStyle];
                    if (String.Compare(styleName, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        style = CircularAxisLabelsStyle.Auto;
                    }
                    else if (String.Compare(styleName, "Circular", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        style = CircularAxisLabelsStyle.Circular;
                    }
                    else if (String.Compare(styleName, "Radial", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        style = CircularAxisLabelsStyle.Radial;
                    }
                    else if (String.Compare(styleName, "Horizontal", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        style = CircularAxisLabelsStyle.Horizontal;
                    }
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(styleName, "CircularLabelsStyle")));
                    }
                }
            }

            // Get auto style
            if (style == CircularAxisLabelsStyle.Auto)
            {
                int sectorNumber = CircularSectorsNumber;
                style = CircularAxisLabelsStyle.Horizontal;
                if (sectorNumber > 30)
                {
                    style = CircularAxisLabelsStyle.Radial;
                }
            }

            return style;
        }

        /// <summary>
        /// Number of sectors in the circular area.
        /// </summary>
        internal int CircularSectorsNumber
        {
            get
            {
                // Check if value was precalculated
                if (_circularSectorNumber == int.MinValue)
                {
                    _circularSectorNumber = GetCircularSectorNumber();
                }

                return _circularSectorNumber;
            }
        }

        /// <summary>
        /// Gets number of sectors in the circular chart area.
        /// </summary>
        /// <returns>Number of sectors.</returns>
        private int GetCircularSectorNumber()
        {
            ICircularChartType type = GetCircularChartType();
            if (type != null)
            {
                return type.GetNumerOfSectors(this, Common.DataManager.Series);
            }
            return 0;
        }

        /// <summary>
        /// Fills a list of circular axis.
        /// </summary>
        /// <returns>Axes list.</returns>
        internal ArrayList GetCircularAxisList()
        {
            // Check if list was already created
            if (_circularAxisList == null)
            {
                _circularAxisList = new ArrayList();

                // Loop through all sectors
                int sectorNumber = GetCircularSectorNumber();
                for (int sectorIndex = 0; sectorIndex < sectorNumber; sectorIndex++)
                {
                    // Create new axis object
                    CircularChartAreaAxis axis = new(sectorIndex * 360f / sectorNumber);

                    // Check if custom X axis labels will be used
                    if (AxisX.CustomLabels.Count > 0)
                    {
                        if (sectorIndex < AxisX.CustomLabels.Count)
                        {
                            axis.Title = AxisX.CustomLabels[sectorIndex].Text;
                            axis.TitleForeColor = AxisX.CustomLabels[sectorIndex].ForeColor;
                        }
                    }
                    else
                    {
                        // Get axis title from all series
                        foreach (Series series in Common.DataManager.Series)
                        {
                            if (series.IsVisible() && series.ChartArea == Name && sectorIndex < series.Points.Count && series.Points[sectorIndex].AxisLabel.Length > 0)
                            {
                                axis.Title = series.Points[sectorIndex].AxisLabel;
                                break;
                            }
                        }
                    }

                    // Add axis into the list
                    _circularAxisList.Add(axis);
                }
            }
            return _circularAxisList;
        }

        /// <summary>
        /// Converts circular position of the X axis to angle in degrees.
        /// </summary>
        /// <param name="position">X axis position.</param>
        /// <returns>Angle in degrees.</returns>
        internal float CircularPositionToAngle(double position)
        {
            // Get X axis scale size
            double scaleRatio = 360.0 / Math.Abs(AxisX.Maximum - AxisX.Minimum);

            return (float)(position * scaleRatio + AxisX.Crossing);
        }

        #endregion Circular chart area methods

        #region 2D Series drawing order methods

        /// <summary>
        /// Helper method that returns a list of 'ChartTypeAndSeriesInfo' objects.
        /// This list is used for chart area series drawing in 2D mode. Each
        /// object may represent an individual series or all series that belong
        /// to one chart type.
        ///
        /// This method is intended to fix issues #6443 and #5385 when area chart
        /// type incorrectly overlaps point or line chart type.
        /// </summary>
        /// <returns>List of 'ChartTypeAndSeriesInfo' objects.</returns>
        private ArrayList GetChartTypesAndSeriesToDraw()
        {
            ArrayList resultList = new();

            // Build chart type or series position based lists
            if (ChartTypes.Count > 1 &&
                (ChartTypes.Contains(ChartTypeNames.Area)
                || ChartTypes.Contains(ChartTypeNames.SplineArea)
                )
                )
            {
                // Array of chart type names that do not require furher processing
                ArrayList processedChartType = new();
                ArrayList splitChartType = new();

                // Draw using the exact order in the series collection
                int seriesIndex = 0;
                foreach (Series series in Common.DataManager.Series)
                {
                    // Check if series is visible and belongs to the chart area
                    if (series.ChartArea == Name && series.IsVisible() && series.Points.Count > 0 && !processedChartType.Contains(series.ChartTypeName))
                    {
                        // Check if curent chart type can be individually processed
                        bool canBeIndividuallyProcessed = false;
                        if (series.ChartType == SeriesChartType.Point ||
                            series.ChartType == SeriesChartType.Line ||
                            series.ChartType == SeriesChartType.Spline ||
                            series.ChartType == SeriesChartType.StepLine)
                        {
                            canBeIndividuallyProcessed = true;
                        }

                        if (!canBeIndividuallyProcessed)
                        {
                            // Add a record to process all series of that chart type at once
                            resultList.Add(new ChartTypeAndSeriesInfo(series.ChartTypeName));
                            processedChartType.Add(series.ChartTypeName);
                        }
                        else
                        {
                            // Check if curent chart type has more that 1 series and they are split
                            // by other series
                            bool chartTypeIsSplit = false;

                            if (splitChartType.Contains(series.ChartTypeName))
                            {
                                chartTypeIsSplit = true;
                            }
                            else
                            {
                                bool otherChartTypeFound = false;
                                for (int curentSeriesIndex = seriesIndex + 1; curentSeriesIndex < Common.DataManager.Series.Count; curentSeriesIndex++)
                                {
                                    if (series.ChartTypeName == Common.DataManager.Series[curentSeriesIndex].ChartTypeName)
                                    {
                                        if (otherChartTypeFound)
                                        {
                                            chartTypeIsSplit = true;
                                            splitChartType.Add(series.ChartTypeName);
                                        }
                                    }
                                    else
                                    {
                                        if (Common.DataManager.Series[curentSeriesIndex].ChartType == SeriesChartType.Area ||
                                            Common.DataManager.Series[curentSeriesIndex].ChartType == SeriesChartType.SplineArea)
                                        {
                                            otherChartTypeFound = true;
                                        }
                                    }
                                }
                            }

                            if (chartTypeIsSplit)
                            {
                                // Add a record to process this series individually
                                resultList.Add(new ChartTypeAndSeriesInfo(series));
                            }
                            else
                            {
                                // Add a record to process all series of that chart type at once
                                resultList.Add(new ChartTypeAndSeriesInfo(series.ChartTypeName));
                                processedChartType.Add(series.ChartTypeName);
                            }
                        }
                    }

                    ++seriesIndex;
                }
            }
            else
            {
                foreach (string chartType in ChartTypes)
                {
                    resultList.Add(new ChartTypeAndSeriesInfo(chartType));
                }
            }

            return resultList;
        }

        /// <summary>
        /// Internal data structure that stores chart type name and optionally series object.
        /// </summary>
        internal class ChartTypeAndSeriesInfo
        {
            /// <summary>
            /// Object constructor.
            /// </summary>
            public ChartTypeAndSeriesInfo()
            {
            }

            /// <summary>
            /// Object constructor.
            /// </summary>
            /// <param name="chartType">Chart type name to initialize with.</param>
            public ChartTypeAndSeriesInfo(string chartType)
            {
                ChartType = chartType;
            }

            /// <summary>
            /// Object constructor.
            /// </summary>
            /// <param name="series">Series to initialize with.</param>
            public ChartTypeAndSeriesInfo(Series series)
            {
                ChartType = series.ChartTypeName;
                Series = series;
            }

            // Chart type name
            internal string ChartType = string.Empty;

            // Series object. Can be set to NULL!
            internal Series Series = null;
        }

        #endregion 2D Series drawing order methods

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
                if (_axisArray != null)
                {
                    foreach (Axis axis in _axisArray)
                    {
                        axis.Dispose();
                    }
                    _axisArray = null;
                }
                if (_areaPosition != null)
                {
                    _areaPosition.Dispose();
                    _areaPosition = null;
                }
                if (_innerPlotPosition != null)
                {
                    _innerPlotPosition.Dispose();
                    _innerPlotPosition = null;
                }
                if (PlotAreaPosition != null)
                {
                    PlotAreaPosition.Dispose();
                    PlotAreaPosition = null;
                }
                if (areaBufferBitmap != null)
                {
                    areaBufferBitmap.Dispose();
                    areaBufferBitmap = null;
                }
                if (_cursorX != null)
                {
                    _cursorX.Dispose();
                    _cursorX = null;
                }
                if (_cursorY != null)
                {
                    _cursorY.Dispose();
                    _cursorY = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}