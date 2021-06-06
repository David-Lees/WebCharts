// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	LabelStyle and CustomLabel classes are used to determine
//              chart axis labels. Labels can be automatically
//              generated based on the series data or be “manually”
//              set by the user.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WebCharts.Services
{
    #region Labels enumerations

    /// <summary>
    /// An enumeration that specifies a mark for custom labels.
    /// </summary>
    public enum LabelMarkStyle
    {
        /// <summary>
        /// No label marks are used.
        /// </summary>
        None,

        /// <summary>
        /// Labels use side marks.
        /// </summary>
        SideMark,

        /// <summary>
        /// Labels use line and side marks.
        /// </summary>
        LineSideMark,

        /// <summary>
        /// Draws a box around the label. The box always starts at the axis position.
        /// </summary>
        Box
    };

    /// <summary>
    /// An enumeration of custom grid lines and tick marks flags used in the custom labels.
    /// </summary>
    [Flags]
    public enum GridTickTypes
    {
        /// <summary>
        /// No tick mark or grid line are shown.
        /// </summary>
        None = 0,

        /// <summary>
        /// Tick mark is shown.
        /// </summary>
        TickMark = 1,

        /// <summary>
        /// Grid line is shown.
        /// </summary>
        Gridline = 2,

        /// <summary>
        /// Tick mark and grid line are shown.
        /// </summary>
        All = TickMark | Gridline
    }

    /// <summary>
    /// An enumeration of label styles for circular chart area axis.
    /// </summary>
    internal enum CircularAxisLabelsStyle
    {
        /// <summary>
        /// Style depends on number of labels.
        /// </summary>
        Auto,

        /// <summary>
        /// Label text positions around the circular area.
        /// </summary>
        Circular,

        /// <summary>
        /// Label text is always horizontal.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Label text has the same angle as circular axis.
        /// </summary>
        Radial
    }

    #endregion Labels enumerations

    /// <summary>
    /// The CustomLabelsCollection class is a strongly typed collection of
    /// custom axis labels.
    /// </summary>
    [
        SRDescription("DescriptionAttributeCustomLabelsCollection_CustomLabelsCollection"),
    ]
    public class CustomLabelsCollection : ChartElementCollection<CustomLabel>
    {
        #region Constructors

        /// <summary>
        /// Custom labels collection object constructor
        /// </summary>
        /// <param name="axis">Reference to the axis object.</param>
        internal CustomLabelsCollection(Axis axis) : base(axis)
        {
        }

        #endregion Constructors

        #region Properties

        internal Axis Axis
        {
            get { return Parent as Axis; }
        }

        #endregion Properties

        #region Labels adding methods

        /// <summary>
		/// Adds a custom label into the collection.
		/// </summary>
		/// <param name="fromPosition">Label left position.</param>
		/// <param name="toPosition">Label right position.</param>
		/// <param name="text">Label text.</param>
        /// <returns>Newly added item.</returns>
        public CustomLabel Add(double fromPosition, double toPosition, string text)
        {
            CustomLabel label = new(fromPosition, toPosition, text, 0, LabelMarkStyle.None);
            Add(label);
            return label;
        }

        /// <summary>
        /// Adds one custom label into the collection. Custom label flag may be specified.
        /// </summary>
        /// <param name="fromPosition">Label left position.</param>
        /// <param name="toPosition">Label right position.</param>
        /// <param name="text">Label text.</param>
        /// <param name="customLabel">Indicates if label is custom (created by user).</param>
        /// <returns>Newly added item.</returns>
        internal CustomLabel Add(double fromPosition, double toPosition, string text, bool customLabel)
        {
            CustomLabel label = new(fromPosition, toPosition, text, 0, LabelMarkStyle.None);
            label.customLabel = customLabel;
            Add(label);
            return label;
        }

        /// <summary>
        /// Adds a custom label into the collection.
        /// </summary>
        /// <param name="fromPosition">Label left position.</param>
        /// <param name="toPosition">Label right position.</param>
        /// <param name="text">Label text.</param>
        /// <param name="rowIndex">Label row index.</param>
        /// <param name="markStyle">Label marking style.</param>
        /// <returns>Newly added item.</returns>
        public CustomLabel Add(double fromPosition, double toPosition, string text, int rowIndex, LabelMarkStyle markStyle)
        {
            CustomLabel label = new(fromPosition, toPosition, text, rowIndex, markStyle);
            Add(label);
            return label;
        }

        /// <summary>
        /// Adds a custom label into the collection.
        /// </summary>
        /// <param name="fromPosition">Label left position.</param>
        /// <param name="toPosition">Label right position.</param>
        /// <param name="text">Label text.</param>
        /// <param name="rowIndex">Label row index.</param>
        /// <param name="markStyle">Label marking style.</param>
        /// <returns>Index of newly added item.</returns>
        /// <param name="gridTick">Custom grid line and tick mark flag.</param>
        public CustomLabel Add(double fromPosition, double toPosition, string text, int rowIndex, LabelMarkStyle markStyle, GridTickTypes gridTick)
        {
            CustomLabel label = new(fromPosition, toPosition, text, rowIndex, markStyle, gridTick);
            Add(label);
            return label;
        }

        /// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type,
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
        /// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
        /// <param name="min">Minimum value..</param>
        /// <param name="max">Maximum value..</param>
        /// <param name="format">Label text format.</param>
        /// <param name="rowIndex">Label row index.</param>
        /// <param name="markStyle">Label marking style.</param>
        public void Add(double labelsStep, DateTimeIntervalType intervalType, double min, double max, string format, int rowIndex, LabelMarkStyle markStyle)
        {
            // Find labels range min/max values
            if (min == 0.0 &&
                max == 0.0 &&
                Axis != null &&
                !double.IsNaN(Axis.Minimum) &&
                !double.IsNaN(Axis.Maximum))
            {
                min = Axis.Minimum;
                max = Axis.Maximum;
            }
            double fromX = Math.Min(min, max);
            double toX = Math.Max(min, max);

            SuspendUpdates();
            try
            {
                // Loop through all label points
                double labelStart = fromX;
                double labelEnd = 0;
                while (labelStart < toX)
                {
                    // Determine label end location
                    if (intervalType == DateTimeIntervalType.Number)
                    {
                        labelEnd = labelStart + labelsStep;
                    }
                    else if (intervalType == DateTimeIntervalType.Milliseconds)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddMilliseconds(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Seconds)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddSeconds(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Minutes)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddMinutes(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Hours)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddHours(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Days)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddDays(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Weeks)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddDays(7 * labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Months)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddMonths((int)labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Years)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddYears((int)labelsStep).ToOADate();
                    }
                    else
                    {
                        // Unsupported step type
                        throw (new ArgumentException(SR.ExceptionAxisLabelsIntervalTypeUnsupported(intervalType.ToString())));
                    }
                    if (labelEnd > toX)
                    {
                        labelEnd = toX;
                    }

                    // Generate label text
                    ChartValueType valueType = ChartValueType.Double;
                    if (intervalType != DateTimeIntervalType.Number)
                    {
                        if (Axis.GetAxisValuesType() == ChartValueType.DateTimeOffset)
                            valueType = ChartValueType.DateTimeOffset;
                        else
                            valueType = ChartValueType.DateTime;
                    }
                    string text = ValueConverter.FormatValue(
                        Common.Chart,
                        Axis,
                        null,
                        labelStart + (labelEnd - labelStart) / 2,
                        format,
                        valueType,
                        ChartElementType.AxisLabels);

                    // Add label
                    CustomLabel label = new(labelStart, labelEnd, text, rowIndex, markStyle);
                    Add(label);

                    labelStart = labelEnd;
                }
            }
            finally
            {
                ResumeUpdates();
            }
        }

        /// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type,
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
        /// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
        public void Add(double labelsStep, DateTimeIntervalType intervalType)
        {
            Add(labelsStep, intervalType, 0, 0, "", 0, LabelMarkStyle.None);
        }

        /// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type,
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
        /// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
        /// <param name="format">Label text format.</param>
        public void Add(double labelsStep, DateTimeIntervalType intervalType, string format)
        {
            Add(labelsStep, intervalType, 0, 0, format, 0, LabelMarkStyle.None);
        }

        /// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type,
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
        /// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
        /// <param name="format">Label text format.</param>
        /// <param name="rowIndex">Label row index.</param>
        /// <param name="markStyle">Label marking style.</param>
        public void Add(double labelsStep, DateTimeIntervalType intervalType, string format, int rowIndex, LabelMarkStyle markStyle)
        {
            Add(labelsStep, intervalType, 0, 0, format, rowIndex, markStyle);
        }

        #endregion Labels adding methods
    }

    /// <summary>
    /// The CustomLabel class represents a single custom axis label. Text and
    /// position along the axis is provided by the caller.
    /// </summary>
    [
    SRDescription("DescriptionAttributeCustomLabel_CustomLabel"),
    ]
    public class CustomLabel : ChartNamedElement
    {
        #region Fields and Constructors

        // Private data members, which store properties values
        private double _fromPosition = 0;

        private double _toPosition = 0;
        private string _text = "";
        private LabelMarkStyle _labelMark = LabelMarkStyle.None;
        private SKColor _foreColor = SKColor.Empty;
        private SKColor _markColor = SKColor.Empty;
        private int _labelRowIndex = 0;

        // Custom grid lines and tick marks flags
        private GridTickTypes _gridTick = GridTickTypes.None;

        // Indicates if label was automatically created or cpecified by user (custom)
        internal bool customLabel = true;

        // Image associated with the label
        private string _image = string.Empty;

        // Image transparent color
        private SKColor _imageTransparentColor = SKColor.Empty;

        private Axis _axis = null;

        #endregion Fields and Constructors

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomLabel()
        {
        }

        /// <summary>
        /// CustomLabel constructor
        /// </summary>
        /// <param name="fromPosition">From position.</param>
        /// <param name="toPosition">To position.</param>
        /// <param name="text">Label text.</param>
        /// <param name="labelRow">Label row index.</param>
        /// <param name="markStyle">Label mark style.</param>
        public CustomLabel(double fromPosition, double toPosition, string text, int labelRow, LabelMarkStyle markStyle)
        {
            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _text = text;
            _labelRowIndex = labelRow;
            _labelMark = markStyle;
            _gridTick = GridTickTypes.None;
        }

        /// <summary>
        /// CustomLabel constructor
        /// </summary>
        /// <param name="fromPosition">From position.</param>
        /// <param name="toPosition">To position.</param>
        /// <param name="text">Label text.</param>
        /// <param name="labelRow">Label row index.</param>
        /// <param name="markStyle">Label mark style.</param>
        /// <param name="gridTick">Custom grid line and tick marks flag.</param>
        public CustomLabel(double fromPosition, double toPosition, string text, int labelRow, LabelMarkStyle markStyle, GridTickTypes gridTick)
        {
            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _text = text;
            _labelRowIndex = labelRow;
            _labelMark = markStyle;
            _gridTick = gridTick;
        }

        #endregion Constructors

        #region Helper methods

        /// <summary>
        /// Returns a cloned label object.
        /// </summary>
        /// <returns>Copy of current custom label.</returns>
        public CustomLabel Clone()
        {
            CustomLabel newLabel = new();

            newLabel.FromPosition = FromPosition;
            newLabel.ToPosition = ToPosition;
            newLabel.Text = Text;
            newLabel.ForeColor = ForeColor;
            newLabel.MarkColor = MarkColor;
            newLabel.RowIndex = RowIndex;
            newLabel.LabelMark = LabelMark;
            newLabel.GridTicks = GridTicks;

            newLabel.ToolTip = ToolTip;
            newLabel.Tag = Tag;
            newLabel.Image = Image;
            newLabel.ImageTransparentColor = ImageTransparentColor;

            return newLabel;
        }

        internal override IChartElement Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;
                if (value != null)
                {
                    _axis = Parent.Parent as Axis;
                }
            }
        }

        /// <summary>
        /// Gets the axis to which this object is attached to.
        /// </summary>
        /// <returns>Axis.</returns>
        public Axis Axis
        {
            get
            {
                return _axis;
            }
        }

        #endregion Helper methods

        #region	CustomLabel properties

        /// <summary>
        /// Gets or sets the tooltip of the custom label.
        /// </summary>
        [
        SRCategory("CategoryAttributeMapArea"),
        SRDescription("DescriptionAttributeToolTip"),
        ]
        public string ToolTip { set; get; } = string.Empty;

        /// <summary>
		/// Gets or sets the label image.
		/// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_Image"),
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the image.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
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
        /// Custom label name. This property is for internal use only.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_Name"),
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
        /// Gets or sets a property which specifies whether
        /// custom tick marks and grid lines will be drawn in the center of the label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_GridTicks"),
        ]
        public GridTickTypes GridTicks
        {
            get
            {
                return _gridTick;
            }
            set
            {
                _gridTick = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the end position of the custom label in axis coordinates.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_From"),
        ]
        public double FromPosition
        {
            get
            {
                return _fromPosition;
            }
            set
            {
                _fromPosition = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the starting position of the custom label in axis coordinates.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_To"),
        ]
        public double ToPosition
        {
            get
            {
                return _toPosition;
            }
            set
            {
                _toPosition = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the text of the custom label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_Text")
        ]
        public string Text
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
        /// Gets or sets the text color of the custom label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeForeColor"),
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the label mark line of the custom label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_MarkColor"),
        ]
        public SKColor MarkColor
        {
            get
            {
                return _markColor;
            }
            set
            {
                _markColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the row index of the custom label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_RowIndex")
        ]
        public int RowIndex
        {
            get
            {
                return _labelRowIndex;
            }
            set
            {
                if (value < 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelRowIndexIsNegative));
                }

                _labelRowIndex = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a property which define the marks for the labels in the second row.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCustomLabel_LabelMark")
        ]
        public LabelMarkStyle LabelMark
        {
            get
            {
                return _labelMark;
            }
            set
            {
                _labelMark = value;
                Invalidate();
            }
        }

        #endregion
    }

    /// <summary>
    /// The LabelStyle class contains properties which define the visual appearance of
    /// the axis labels, their interval and position. This class is also
    /// responsible for calculating the position of all the labels and
    /// drawing them.
    /// </summary>
    [
        SRDescription("DescriptionAttributeLabel_Label"),
    ]
    public class LabelStyle : ChartElement
    {
        #region Fields

        // Reference to the Axis
        private Axis _axis = null;

        // Private data members, which store properties values
        private bool _enabled = true;

        internal double intervalOffset = double.NaN;
        internal double interval = double.NaN;
        internal DateTimeIntervalType intervalType = DateTimeIntervalType.NotSet;
        internal DateTimeIntervalType intervalOffsetType = DateTimeIntervalType.NotSet;

        private FontCache _fontCache = new();
        private SKFont _font;
        private SKColor _foreColor = SKColors.Black;
        internal int angle = 0;
        internal bool isStaggered = false;
        private bool _isEndLabelVisible = true;
        private bool _truncatedLabels = false;
        private string _format = "";

        #endregion

        #region Constructors

        /// <summary>
        /// Public default constructor.
        /// </summary>
        public LabelStyle()
        {
            _font = _fontCache.DefaultFont;
        }

        public LabelStyle(IChartElement parent) : base(parent)
        {
            _font = _fontCache.DefaultFont;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="axis">Axis which owns the grid.</param>
		internal LabelStyle(Axis axis)
            : this()
        {
            _axis = axis;
        }

        #endregion

        #region Axis labels drawing methods

        /// <summary>
        /// Draws axis labels on the circular chart area.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        internal void PaintCircular(ChartGraphics graph)
        {
            // Label string drawing format
            using StringFormat format = new();
            format.FormatFlags |= StringFormatFlags.LineLimit;
            format.Trimming = StringTrimming.EllipsisCharacter;

            // Labels are disabled for this axis
            if (!_axis.LabelStyle.Enabled)
                return;

            // Draw text with anti-aliasing
            /*
            if( (graph.AntiAliasing & AntiAliasing.Text) == AntiAliasing.Text )
            {
                graph.TextRenderingHint = TextRenderingHint.AntiAlias;
            }
            else
            {
                graph.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            }
            */

            // Gets axis labels style
            CircularAxisLabelsStyle labelsStyle = _axis.ChartArea.GetCircularAxisLabelsStyle();

            // Get list of circular axes with labels
            ArrayList circularAxes = _axis.ChartArea.GetCircularAxisList();

            // Draw each axis label
            int index = 0;
            foreach (CircularChartAreaAxis circAxis in circularAxes)
            {
                if (circAxis.Title.Length > 0)
                {
                    //******************************************************************
                    //** Calculate label position corner position
                    //******************************************************************
                    SKPoint labelRelativePosition = new(
                        _axis.ChartArea.circularCenter.X,
                        _axis.ChartArea.PlotAreaPosition.Y);

                    // Adjust labels Y position
                    labelRelativePosition.Y -= _axis.markSize + Axis.elementSpacing;

                    // Convert to absolute
                    SKPoint[] labelPosition = new SKPoint[] { graph.GetAbsolutePoint(labelRelativePosition) };

                    // Get label rotation angle
                    float labelAngle = circAxis.AxisPosition;
                    ICircularChartType chartType = _axis.ChartArea.GetCircularChartType();
                    if (chartType != null && chartType.XAxisCrossingSupported() && !double.IsNaN(_axis.ChartArea.AxisX.Crossing))
                    {
                        labelAngle += (float)_axis.ChartArea.AxisX.Crossing;
                    }

                    // Make sure angle is presented as a positive number
                    while (labelAngle < 0)
                    {
                        labelAngle = 360f + labelAngle;
                    }

                    // Set graphics rotation matrix
                    var p = graph.GetAbsolutePoint(_axis.ChartArea.circularCenter);
                    SKMatrix newMatrix = SKMatrix.CreateRotationDegrees(labelAngle, p.X, p.Y);
                    newMatrix.TransformPoints(labelPosition);

                    // Set text alignment
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Near;
                    if (labelsStyle != CircularAxisLabelsStyle.Radial)
                    {
                        if (labelAngle < 5f || labelAngle > 355f)
                        {
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Far;
                        }
                        if (labelAngle < 185f && labelAngle > 175f)
                        {
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Near;
                        }
                        if (labelAngle > 185f && labelAngle < 355f)
                        {
                            format.Alignment = StringAlignment.Far;
                        }
                    }
                    else
                    {
                        if (labelAngle > 180f)
                        {
                            format.Alignment = StringAlignment.Far;
                        }
                    }

                    // Set text rotation angle
                    float textAngle = labelAngle;
                    if (labelsStyle == CircularAxisLabelsStyle.Radial)
                    {
                        if (labelAngle > 180)
                        {
                            textAngle += 90f;
                        }
                        else
                        {
                            textAngle -= 90f;
                        }
                    }
                    else if (labelsStyle == CircularAxisLabelsStyle.Circular)
                    {
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Far;
                    }

                    // Set text rotation matrix
                    SKMatrix oldMatrix = graph.Transform;
                    if (labelsStyle == CircularAxisLabelsStyle.Radial || labelsStyle == CircularAxisLabelsStyle.Circular)
                    {
                        SKMatrix textRotationMatrix = SKMatrix.CreateRotationDegrees(textAngle, labelPosition[0].X, labelPosition[0].Y);
                        graph.Transform = textRotationMatrix;
                    }

                    // Get axis titl (label) color
                    SKColor labelColor = _foreColor;
                    if (circAxis.TitleForeColor != SKColor.Empty)
                    {
                        labelColor = circAxis.TitleForeColor;
                    }

                    // Draw label
                    using (SKPaint brush = new() { Color = labelColor, Style = SKPaintStyle.Fill })
                    {
                        graph.DrawString(
                            circAxis.Title.Replace("\\n", "\n"),
                            _axis.autoLabelFont ?? _font,
                            brush,
                            labelPosition[0]);
                    }

                    // Process selection region
                    if (_axis.Common.ProcessModeRegions)
                    {
                        SKSize size = graph.MeasureString(circAxis.Title.Replace("\\n", "\n"), _axis.autoLabelFont ?? _font);
                        SKRect labelRect = GetLabelPosition(
                            labelPosition[0],
                            size,
                            format);
                        SKPoint[] points = new SKPoint[]
                        {
                                labelRect.Location,
                                new SKPoint(labelRect.Right, labelRect.Top),
                                new SKPoint(labelRect.Right, labelRect.Bottom),
                                new SKPoint(labelRect.Left, labelRect.Bottom)
                        };

                        using SKPath path = new();
                        path.AddPoly(points);
                        path.Close();
                        path.Transform(graph.Transform);
                        _axis.Common.HotRegionsList.AddHotRegion(
                            path,
                            false,
                            ChartElementType.AxisLabels,
                            circAxis.Title);
                    }

                    // Restore graphics
                    if (labelsStyle == CircularAxisLabelsStyle.Radial || labelsStyle == CircularAxisLabelsStyle.Circular)
                    {
                        graph.Transform = oldMatrix;
                    }
                }

                ++index;
            }
        }

        /// <summary>
        /// Gets rectangle position of the label.
        /// </summary>
        /// <param name="position">Original label position.</param>
        /// <param name="size">Label text size.</param>
        /// <param name="format">Label string format.</param>
        /// <returns>Label rectangle position.</returns>
        internal static SKRect GetLabelPosition(
            SKPoint position,
            SKSize size,
            StringFormat format)
        {
            // Calculate label position rectangle
            SKRect labelPosition = SKRect.Empty;
            labelPosition.Size = size;

            if (format.Alignment == StringAlignment.Far)
            {
                labelPosition.Left = position.X - size.Width;
            }
            else if (format.Alignment == StringAlignment.Near)
            {
                labelPosition.Left = position.X;
            }
            else if (format.Alignment == StringAlignment.Center)
            {
                labelPosition.Left = position.X - size.Width / 2F;
            }

            if (format.LineAlignment == StringAlignment.Far)
            {
                labelPosition.Top = position.Y - size.Height;
            }
            else if (format.LineAlignment == StringAlignment.Near)
            {
                labelPosition.Top = position.Y;
            }
            else if (format.LineAlignment == StringAlignment.Center)
            {
                labelPosition.Top = position.Y - size.Height / 2F;
            }

            return labelPosition;
        }

        /// <summary>
        /// Draws axis labels.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void Paint(ChartGraphics graph, bool backElements)
        {
            // Label string drawing format
            using StringFormat format = new();
            format.FormatFlags |= StringFormatFlags.LineLimit;
            format.Trimming = StringTrimming.EllipsisCharacter;

            // Labels are disabled for this axis
            if (!_axis.LabelStyle.Enabled)
                return;
            // deliant fix-> VSTS #157848, #143286 - drawing custom label in empty axis
            if (Double.IsNaN(_axis.ViewMinimum) || Double.IsNaN(_axis.ViewMaximum))
                return;

            // Draw labels in 3D space
            if (_axis.ChartArea.Area3DStyle.Enable3D && !_axis.ChartArea.chartAreaIsCurcular)
            {
                Paint3D(graph, backElements);
                return;
            }

            // Initialize all labels position rectangle
            SKRect rectLabels = _axis.ChartArea.Position.ToSKRect();
            float labelSize = _axis.labelSize;

            if (_axis.AxisPosition == AxisPosition.Left)
            {
                if (_axis.GetIsMarksNextToAxis())
                    rectLabels.Left = (float)_axis.GetAxisPosition();
                else
                    rectLabels.Left = _axis.PlotAreaPosition.X;

                rectLabels.Left -= labelSize + _axis.markSize;
                rectLabels.Right = rectLabels.Left + labelSize;

                // Set label text alignment
                format.Alignment = StringAlignment.Far;
                format.LineAlignment = StringAlignment.Center;
            }
            else if (_axis.AxisPosition == AxisPosition.Right)
            {
                if (_axis.GetIsMarksNextToAxis())
                    rectLabels.Left = (float)_axis.GetAxisPosition();
                else
                    rectLabels.Left = _axis.PlotAreaPosition.Right;
                rectLabels.Left += _axis.markSize;
                rectLabels.Right = rectLabels.Left + labelSize;

                // Set label text alignment
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Center;
            }
            else if (_axis.AxisPosition == AxisPosition.Top)
            {
                if (_axis.GetIsMarksNextToAxis())
                    rectLabels.Top = (float)_axis.GetAxisPosition();
                else
                    rectLabels.Top = _axis.PlotAreaPosition.Y;
                rectLabels.Top -= labelSize + _axis.markSize;
                rectLabels.Bottom = rectLabels.Top + labelSize;

                // Set label text alignment
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Far;
            }
            else if (_axis.AxisPosition == AxisPosition.Bottom)
            {
                if (_axis.GetIsMarksNextToAxis())
                    rectLabels.Top = (float)_axis.GetAxisPosition();
                else
                    rectLabels.Top = _axis.PlotAreaPosition.Bottom;
                rectLabels.Top += _axis.markSize;
                rectLabels.Bottom = rectLabels.Top + labelSize;

                // Set label text alignment
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Near;
            }

            // Calculate bounding rectangle
            SKRect boundaryRect = rectLabels;
            if (boundaryRect != SKRect.Empty && _axis.totlaGroupingLabelsSize > 0)
            {
                if (_axis.AxisPosition == AxisPosition.Left)
                {
                    boundaryRect.Left += _axis.totlaGroupingLabelsSize;
                    boundaryRect.Right -= _axis.totlaGroupingLabelsSize;
                }
                else if (_axis.AxisPosition == AxisPosition.Right)
                {
                    boundaryRect.Right -= _axis.totlaGroupingLabelsSize;
                }
                else if (_axis.AxisPosition == AxisPosition.Top)
                {
                    boundaryRect.Top += _axis.totlaGroupingLabelsSize;
                    boundaryRect.Bottom -= _axis.totlaGroupingLabelsSize;
                }
                else if (_axis.AxisPosition == AxisPosition.Bottom)
                {
                    boundaryRect.Bottom -= _axis.totlaGroupingLabelsSize;
                }
            }

            // Check if the AJAX zooming and scrolling mode is enabled.
            // Labels are drawn slightly different in this case.
            bool ajaxScrollingEnabled = false;
            bool firstFrame = true;
            bool lastFrame = true;

            // Draw all labels from the collection
            int labelIndex = 0;
            foreach (CustomLabel label in _axis.CustomLabels)
            {
                bool truncatedLeft = false;
                bool truncatedRight = false;
                double labelFrom = label.FromPosition;
                double labelTo = label.ToPosition;
                bool useRelativeCoordiantes = false;
                double labelFromRelative = double.NaN;
                double labelToRelative = double.NaN;

                // Skip if label middle point is outside current scaleView
                if (label.RowIndex == 0)
                {
                    double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                    decimal viewMin = (decimal)_axis.ViewMinimum;
                    decimal viewMax = (decimal)_axis.ViewMaximum;

                    if (ajaxScrollingEnabled)
                    {
                        // Skip very first and last labels if they are partialy outside the scaleView
                        if (firstFrame && (decimal)label.FromPosition < (decimal)_axis.Minimum)
                        {
                            continue;
                        }
                        if (lastFrame && (decimal)label.ToPosition > (decimal)_axis.Maximum)
                        {
                            continue;
                        }

                        // Skip label only if it is compleltly out of the scaleView
                        if ((decimal)label.ToPosition < viewMin ||
                            (decimal)label.FromPosition > viewMax)
                        {
                            continue;
                        }

                        // RecalculateAxesScale label index starting from the first frame.
                        // Index is used to determine position of the offset labels
                        if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                        {
                            // Reset index
                            labelIndex = 0;

                            // Get first series attached to this axis
                            Series axisSeries = null;
                            if (_axis.axisType == AxisName.X || _axis.axisType == AxisName.X2)
                            {
                                List<string> seriesArray = _axis.ChartArea.GetXAxesSeries((_axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, _axis.SubAxisName);
                                if (seriesArray.Count > 0)
                                {
                                    axisSeries = _axis.Common.DataManager.Series[seriesArray[0]];
                                    if (axisSeries != null && !axisSeries.IsXValueIndexed)
                                    {
                                        axisSeries = null;
                                    }
                                }
                            }

                            // Set start position and iterate through label positions
                            // NOTE: Labels offset should not be taken in the account
                            double currentPosition = _axis.Minimum;
                            while (currentPosition < _axis.Maximum)
                            {
                                if (currentPosition >= middlePoint)
                                {
                                    break;
                                }

                                currentPosition += ChartHelper.GetIntervalSize(currentPosition, _axis.LabelStyle.GetInterval(), _axis.LabelStyle.GetIntervalType(),
                                    axisSeries, 0.0, DateTimeIntervalType.Number, true);
                                ++labelIndex;
                            }
                        }
                    }
                    else
                    {
                        // Skip label if label middle point is not in the scaleView
                        if ((decimal)middlePoint < viewMin ||
                            (decimal)middlePoint > viewMax)
                        {
                            continue;
                        }
                    }

                    // Make sure label To and From coordinates are processed by one scale segment based
                    // on the label middle point position.
                    if (_axis.ScaleSegments.Count > 0)
                    {
                        AxisScaleSegment scaleSegment = _axis.ScaleSegments.FindScaleSegmentForAxisValue(middlePoint);
                        _axis.ScaleSegments.AllowOutOfScaleValues = true;
                        _axis.ScaleSegments.EnforceSegment(scaleSegment);
                    }

                    // Use center point instead of the To/From if label takes all scaleView
                    // This is done to avoid issues with labels drawing with high
                    // zooming levels.
                    if ((decimal)label.FromPosition < viewMin &&
                        (decimal)label.ToPosition > viewMax)
                    {
                        // Indicates that chart relative coordinates should be used instead of axis values
                        useRelativeCoordiantes = true;

                        // Calculate label From/To in relative coordinates using
                        // label middle point and 100% width.
                        labelFromRelative = _axis.GetLinearPosition(middlePoint) - 50.0;
                        labelToRelative = labelFromRelative + 100.0;
                    }
                }
                else
                {
                    // Skip labels completly outside the scaleView
                    if (label.ToPosition <= _axis.ViewMinimum || label.FromPosition >= _axis.ViewMaximum)
                    {
                        continue;
                    }

                    // Check if label is partially visible.
                    if (!ajaxScrollingEnabled &&
                        _axis.ScaleView.IsZoomed)
                    {
                        if (label.FromPosition < _axis.ViewMinimum)
                        {
                            truncatedLeft = true;
                            labelFrom = _axis.ViewMinimum;
                        }
                        if (label.ToPosition > _axis.ViewMaximum)
                        {
                            truncatedRight = true;
                            labelTo = _axis.ViewMaximum;
                        }
                    }
                }

                // Calculate single label position
                SKRect rect = rectLabels;

                // Label is in the first row
                if (label.RowIndex == 0)
                {
                    if (_axis.AxisPosition == AxisPosition.Left)
                    {
                        rect.Left = rectLabels.Right - _axis.unRotatedLabelSize;
                        rect.Right = rect.Left + _axis.unRotatedLabelSize;

                        // Adjust label rectangle if offset labels are used
                        if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                        {
                            var w = rect.Width;
                            w /= 2F;
                            rect.Right -= w;
                            if (labelIndex % 2 != 0F)
                            {
                                rect.Left += rect.Width;
                            }
                        }
                    }
                    else if (_axis.AxisPosition == AxisPosition.Right)
                    {
                        rect.Right = rect.Left + _axis.unRotatedLabelSize;

                        // Adjust label rectangle if offset labels are used
                        if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                        {
                            var w = rect.Width;
                            w /= 2F;
                            rect.Right -= w;
                            if (labelIndex % 2 != 0F)
                            {
                                rect.Left += rect.Width;
                            }
                        }
                    }
                    else if (_axis.AxisPosition == AxisPosition.Top)
                    {
                        rect.Top = rectLabels.Bottom - _axis.unRotatedLabelSize;
                        rect.Bottom = rect.Top + _axis.unRotatedLabelSize;

                        // Adjust label rectangle if offset labels are used
                        if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                        {
                            var h = rect.Height / 2F;
                            rect.Bottom -= h;
                            if (labelIndex % 2 != 0F)
                            {
                                rect.Top += rect.Height;
                            }
                        }
                    }
                    else if (_axis.AxisPosition == AxisPosition.Bottom)
                    {
                        rect.Bottom = rect.Top + _axis.unRotatedLabelSize;

                        // Adjust label rectangle if offset labels are used
                        if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                        {
                            var h = rect.Height / 2F;
                            rect.Bottom -= h;
                            if (labelIndex % 2 != 0F)
                            {
                                rect.Top += rect.Height;
                            }
                        }
                    }

                    // Increase label index
                    ++labelIndex;
                }

                // Label is in the second row
                else if (label.RowIndex > 0)
                {
                    if (_axis.AxisPosition == AxisPosition.Left)
                    {
                        rect.Left += _axis.totlaGroupingLabelsSizeAdjustment;
                        for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                        {
                            rect.Left += _axis.groupingLabelSizes[index - 1];
                        }
                        rect.Size = new(_axis.groupingLabelSizes[label.RowIndex - 1], rect.Height);
                    }
                    else if (_axis.AxisPosition == AxisPosition.Right)
                    {
                        rect.Left = rect.Right - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;// + Axis.elementSpacing * 0.25f;
                        for (int index = 1; index < label.RowIndex; index++)
                        {
                            rect.Left += _axis.groupingLabelSizes[index - 1];
                        }
                        rect.Size = new(_axis.groupingLabelSizes[label.RowIndex - 1], rect.Height);
                    }
                    else if (_axis.AxisPosition == AxisPosition.Top)
                    {
                        rect.Top += _axis.totlaGroupingLabelsSizeAdjustment;
                        for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                        {
                            rect.Top += _axis.groupingLabelSizes[index - 1];
                        }
                        rect.Size = new SKSize(rect.Width, _axis.groupingLabelSizes[label.RowIndex - 1]);
                    }
                    if (_axis.AxisPosition == AxisPosition.Bottom)
                    {
                        rect.Top = rect.Bottom - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;
                        for (int index = 1; index < label.RowIndex; index++)
                        {
                            rect.Top += _axis.groupingLabelSizes[index - 1];
                        }
                        rect.Size = new(rect.Width, _axis.groupingLabelSizes[label.RowIndex - 1]);
                    }
                }

                // Unknown label row value
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelIndexIsNegative));
                }

                // Set label From and To coordinates
                double fromPosition = _axis.GetLinearPosition(labelFrom);
                double toPosition = _axis.GetLinearPosition(labelTo);
                if (useRelativeCoordiantes)
                {
                    useRelativeCoordiantes = false;
                    fromPosition = labelFromRelative;
                    toPosition = labelToRelative;
                }

                if (_axis.AxisPosition == AxisPosition.Top || _axis.AxisPosition == AxisPosition.Bottom)
                {
                    rect.Left = (float)Math.Min(fromPosition, toPosition);
                    rect.Size = new((float)Math.Max(fromPosition, toPosition) - rect.Left, rect.Height);

                    // Adjust label To/From position if offset labels are used
                    if (label.RowIndex == 0 &&
                        ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1)))
                    {
                        rect.Left -= rect.Width / 2F;
                        rect.Size = new(rect.Width * 2F, rect.Height);
                    }
                }
                else
                {
                    rect.Top = (float)Math.Min(fromPosition, toPosition);
                    rect.Size = new(rect.Width, (float)Math.Max(fromPosition, toPosition) - rect.Top);

                    // Adjust label To/From position if offset labels are used
                    if (label.RowIndex == 0 &&
                        ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1)))
                    {
                        rect.Top -= rect.Height / 2F;
                        rect.Size = new(rect.Width, rect.Height * 2F);
                    }
                }

                // Draw label
                using (SKPaint brush = new() { Color = (label.ForeColor == SKColor.Empty) ? _foreColor : label.ForeColor, Style = SKPaintStyle.Fill })
                {
                    graph.DrawLabelStringRel(_axis,
                        label.RowIndex,
                        label.LabelMark,
                        label.MarkColor,
                        label.Text,
                        label.Image,
                        label.ImageTransparentColor,
                        _axis.autoLabelFont ?? _font,
                        brush,
                        rect,
                        format,
                        (_axis.autoLabelAngle < -90) ? angle : _axis.autoLabelAngle,
                        (!TruncatedLabels || label.RowIndex > 0) ? SKRect.Empty : boundaryRect,
                        label,
                        truncatedLeft,
                        truncatedRight);
                }

                // Clear scale segment enforcement
                _axis.ScaleSegments.EnforceSegment(null);
                _axis.ScaleSegments.AllowOutOfScaleValues = false;
            }
        }

        #endregion

        #region 3D Axis labels drawing methods

        /// <summary>
        /// Get a rectangle between chart area position and plotting area on specified side.
        /// Also sets axis labels string formatting for the specified labels position.
        /// </summary>
        /// <param name="area">Chart area object.</param>
        /// <param name="position">Position in chart area.</param>
        /// <param name="stringFormat">Axis labels string format.</param>
        /// <returns>Axis labels rectangle.</returns>
        private static SKRect GetAllLabelsRect(ChartArea area, AxisPosition position, StringFormat stringFormat)
        {
            // Find axis with same position
            Axis labelsAxis = null;
            foreach (Axis curAxis in area.Axes)
            {
                if (curAxis.AxisPosition == position)
                {
                    labelsAxis = curAxis;
                    break;
                }
            }

            if (labelsAxis == null)
            {
                return SKRect.Empty;
            }

            // Calculate rect for different positions
            SKRect rectLabels = area.Position.ToSKRect();
            if (position == AxisPosition.Left)
            {
                rectLabels.Size = new(labelsAxis.labelSize, rectLabels.Height);
                if (labelsAxis.GetIsMarksNextToAxis())
                {
                    rectLabels.Left = (float)labelsAxis.GetAxisPosition();
                    rectLabels.Size = new(Math.Max(rectLabels.Width, rectLabels.Left - labelsAxis.PlotAreaPosition.X), rectLabels.Height);
                }
                else
                {
                    rectLabels.Left = labelsAxis.PlotAreaPosition.X;
                }

                rectLabels.Left -= rectLabels.Width;

                if (area.IsSideSceneWallOnLeft() || area.Area3DStyle.WallWidth == 0)
                {
                    rectLabels.Left -= labelsAxis.markSize;
                }

                // Set label text alignment
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Center;
            }
            else if (position == AxisPosition.Right)
            {
                rectLabels.Size = new(labelsAxis.labelSize, rectLabels.Height);
                if (labelsAxis.GetIsMarksNextToAxis())
                {
                    rectLabels.Left = (float)labelsAxis.GetAxisPosition();
                    rectLabels.Size = new(Math.Max(rectLabels.Width, labelsAxis.PlotAreaPosition.Right - rectLabels.Left), rectLabels.Height);
                }
                else
                {
                    rectLabels.Left = labelsAxis.PlotAreaPosition.Right;
                }

                if (!area.IsSideSceneWallOnLeft() || area.Area3DStyle.WallWidth == 0)
                {
                    rectLabels.Left += labelsAxis.markSize;
                }

                // Set label text alignment
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Center;
            }
            else if (position == AxisPosition.Top)
            {
                rectLabels.Size = new(rectLabels.Width, labelsAxis.labelSize);
                if (labelsAxis.GetIsMarksNextToAxis())
                {
                    rectLabels.Top = (float)labelsAxis.GetAxisPosition();
                    rectLabels.Size = new(rectLabels.Width, Math.Max(rectLabels.Height, rectLabels.Top - labelsAxis.PlotAreaPosition.Y));
                }
                else
                {
                    rectLabels.Top = labelsAxis.PlotAreaPosition.Y;
                }

                rectLabels.Top -= rectLabels.Height;

                if (area.Area3DStyle.WallWidth == 0)
                {
                    rectLabels.Top -= labelsAxis.markSize;
                }

                // Set label text alignment
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Far;
            }
            else if (position == AxisPosition.Bottom)
            {
                rectLabels.Size = new(rectLabels.Width, labelsAxis.labelSize);
                if (labelsAxis.GetIsMarksNextToAxis())
                {
                    rectLabels.Top = (float)labelsAxis.GetAxisPosition();
                    rectLabels.Size = new(rectLabels.Width, Math.Max(rectLabels.Height, labelsAxis.PlotAreaPosition.Bottom - rectLabels.Top));
                }
                else
                {
                    rectLabels.Top = labelsAxis.PlotAreaPosition.Bottom;
                }
                rectLabels.Top += labelsAxis.markSize;

                // Set label text alignment
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Near;
            }

            return rectLabels;
        }

        /// <summary>
        /// Gets position of axis labels.
        /// Top and Bottom axis labels can be drawn on the sides (left or right)
        /// of the plotting area. If angle between axis and it's projection is
        /// between -25 and 25 degrees the axis are drawn at the bottom/top,
        /// otherwise labels are moved on the left or right side.
        /// </summary>
        /// <param name="axis">Axis object.</param>
        /// <returns>Position where axis labels should be drawn.</returns>
		private AxisPosition GetLabelsPosition(Axis axis)
        {
            // Get angle between 2D axis and it's 3D projection.
            double axisAngle = axis.GetAxisProjectionAngle();

            // Pick the side to draw the labels on
            if (axis.AxisPosition == AxisPosition.Bottom)
            {
                if (axisAngle <= -25)
                    return AxisPosition.Right;
                else if (axisAngle >= 25)
                    return AxisPosition.Left;
            }
            else if (axis.AxisPosition == AxisPosition.Top)
            {
                if (axisAngle <= -25)
                    return AxisPosition.Left;
                else if (axisAngle >= 25)
                    return AxisPosition.Right;
            }

            // Labels are on the same side as the axis
            return axis.AxisPosition;
        }

        /// <summary>
        /// Draws axis labels in 3D space.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void Paint3D(ChartGraphics graph, bool backElements)
        {
            // Label string drawing format
            using StringFormat format = new();
            format.Trimming = StringTrimming.EllipsisCharacter;

            // Calculate single pixel size in relative coordinates
            SKSize pixelSize = graph.GetRelativeSize(new SKSize(1f, 1f));

            //********************************************************************
            //** Determine the side of the plotting area to draw the labels on.
            //********************************************************************
            AxisPosition labelsPosition = GetLabelsPosition(_axis);

            //*****************************************************************
            //** Set the labels Z position
            //*****************************************************************
            float labelsZPosition = _axis.GetMarksZPosition(out bool axisOnEdge);

            // Adjust Z position for the "bent" tick marks
            bool adjustForWallWidth = false;
            if (_axis.AxisPosition == AxisPosition.Top &&
                !_axis.ChartArea.ShouldDrawOnSurface(SurfaceNames.Top, backElements, false))
            {
                adjustForWallWidth = true;
            }
            if (_axis.AxisPosition == AxisPosition.Left &&
                !_axis.ChartArea.ShouldDrawOnSurface(SurfaceNames.Left, backElements, false))
            {
                adjustForWallWidth = true;
            }
            if (_axis.AxisPosition == AxisPosition.Right &&
                !_axis.ChartArea.ShouldDrawOnSurface(SurfaceNames.Right, backElements, false))
            {
                adjustForWallWidth = true;
            }

            if (adjustForWallWidth && _axis.ChartArea.Area3DStyle.WallWidth > 0)
            {
                if (_axis.MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
                {
                    labelsZPosition -= _axis.ChartArea.areaSceneWallWidth.Width;
                }
                else if (_axis.MajorTickMark.TickMarkStyle == TickMarkStyle.OutsideArea)
                {
                    labelsZPosition -= _axis.MajorTickMark.Size + _axis.ChartArea.areaSceneWallWidth.Width;
                }
                else if (_axis.MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis)
                {
                    labelsZPosition -= _axis.MajorTickMark.Size / 2f + _axis.ChartArea.areaSceneWallWidth.Width;
                }
            }

            //*****************************************************************
            //** Check if labels should be drawn as back or front element.
            //*****************************************************************
            bool labelsInsidePlotArea = (_axis.GetIsMarksNextToAxis() && !axisOnEdge);
            if (backElements == labelsInsidePlotArea)
            {
                // Skip drawing
                return;
            }

            //********************************************************************
            //** Initialize all labels position rectangle
            //********************************************************************
            SKRect rectLabels = GetAllLabelsRect(_axis.ChartArea, _axis.AxisPosition, format);

            //********************************************************************
            //** Calculate bounding rectangle used to truncate labels on the
            //** chart area boundary if TruncatedLabels property is set to true.
            //********************************************************************
            SKRect boundaryRect = rectLabels;
            if (boundaryRect != SKRect.Empty && _axis.totlaGroupingLabelsSize > 0)
            {
                if (_axis.AxisPosition == AxisPosition.Left)
                {
                    boundaryRect.Left += _axis.totlaGroupingLabelsSize;
                    boundaryRect.Right -= _axis.totlaGroupingLabelsSize;
                }
                else if (_axis.AxisPosition == AxisPosition.Right)
                {
                    boundaryRect.Right -= _axis.totlaGroupingLabelsSize;
                }
                else if (_axis.AxisPosition == AxisPosition.Top)
                {
                    boundaryRect.Top += _axis.totlaGroupingLabelsSize;
                    boundaryRect.Bottom -= _axis.totlaGroupingLabelsSize;
                }
                else if (_axis.AxisPosition == AxisPosition.Bottom)
                {
                    boundaryRect.Bottom -= _axis.totlaGroupingLabelsSize;
                }
            }

            // Pre-calculated height of the first labels row
            float firstLabelsRowHeight = -1f;

            // For 3D axis labels the first row of labels
            // has to be drawn after all other rows because
            // of hot regions.
            for (int selectionRow = 0; selectionRow <= _axis.GetGroupLabelLevelCount(); selectionRow++)
            {
                //********************************************************************
                //** Draw all labels from the collection
                //********************************************************************
                int labelIndex = 0;
                foreach (CustomLabel label in _axis.CustomLabels)
                {
                    bool truncatedLeft = false;
                    bool truncatedRight = false;
                    double labelFrom = label.FromPosition;
                    double labelTo = label.ToPosition;

                    if (label.RowIndex != selectionRow)
                    {
                        continue;
                    }

                    // Skip if label middle point is outside current scaleView
                    if (label.RowIndex == 0)
                    {
                        double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                        if ((decimal)middlePoint < (decimal)_axis.ViewMinimum ||
                            (decimal)middlePoint > (decimal)_axis.ViewMaximum)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // Skip labels completly outside the scaleView
                        if (label.ToPosition <= _axis.ViewMinimum || label.FromPosition >= _axis.ViewMaximum)
                        {
                            continue;
                        }

                        // Check if label is partially visible
                        if (_axis.ScaleView.IsZoomed)
                        {
                            if (label.FromPosition < _axis.ViewMinimum)
                            {
                                truncatedLeft = true;
                                labelFrom = _axis.ViewMinimum;
                            }
                            if (label.ToPosition > _axis.ViewMaximum)
                            {
                                truncatedRight = true;
                                labelTo = _axis.ViewMaximum;
                            }
                        }
                    }

                    // Calculate single label position
                    SKRect rect = rectLabels;

                    // Label is in the first row
                    if (label.RowIndex == 0)
                    {
                        if (_axis.AxisPosition == AxisPosition.Left)
                        {
                            if (!_axis.GetIsMarksNextToAxis())
                            {
                                rect.Left = rectLabels.Right - _axis.unRotatedLabelSize;
                                rect.Size = new(_axis.unRotatedLabelSize, rect.Height);
                            }

                            // Adjust label rectangle if offset labels are used
                            if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                            {
                                rect.Size = new(rect.Width / 2F, rect.Height);
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.Left += rect.Width;
                                }
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Right)
                        {
                            if (!_axis.GetIsMarksNextToAxis())
                            {
                                rect.Size = new(_axis.unRotatedLabelSize, rect.Height);
                            }

                            // Adjust label rectangle if offset labels are used
                            if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                            {
                                rect.Size = new(rect.Width / 2F, rect.Height);
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.Left += rect.Width;
                                }
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Top)
                        {
                            if (!_axis.GetIsMarksNextToAxis())
                            {
                                rect.Top = rectLabels.Bottom - _axis.unRotatedLabelSize;
                                rect.Size = new(rect.Width, _axis.unRotatedLabelSize);
                            }

                            // Adjust label rectangle if offset labels are used
                            if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                            {
                                rect.Size = new(rect.Width, rect.Height / 2F);
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.Left += rect.Height;
                                }
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Bottom)
                        {
                            if (!_axis.GetIsMarksNextToAxis())
                            {
                                rect.Size = new(rect.Width, _axis.unRotatedLabelSize);
                            }

                            // Adjust label rectangle if offset labels are used
                            if ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1))
                            {
                                rect.Size = new(rect.Width, rect.Height / 2F);
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.Top += rect.Height;
                                }
                            }
                        }

                        // Increase label index
                        ++labelIndex;
                    }

                    // Label is in the second row
                    else if (label.RowIndex > 0)
                    {
                        // Hide grouping labels (where index of row > 0) when they are displayed
                        // not on the same side as their axis. Fixes MS issue #64.
                        if (labelsPosition != _axis.AxisPosition)
                        {
                            continue;
                        }

                        if (_axis.AxisPosition == AxisPosition.Left)
                        {
                            rect.Left += _axis.totlaGroupingLabelsSizeAdjustment;
                            for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                            {
                                rect.Left += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Size = new(_axis.groupingLabelSizes[label.RowIndex - 1], rect.Height
                                );
                        }
                        else if (_axis.AxisPosition == AxisPosition.Right)
                        {
                            rect.Left = rect.Right - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;// + Axis.elementSpacing * 0.25f;
                            for (int index = 1; index < label.RowIndex; index++)
                            {
                                rect.Left += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Size = new(_axis.groupingLabelSizes[label.RowIndex - 1], rect.Height);
                        }
                        else if (_axis.AxisPosition == AxisPosition.Top)
                        {
                            rect.Top += _axis.totlaGroupingLabelsSizeAdjustment;
                            for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                            {
                                rect.Top += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Size = new(rect.Width, _axis.groupingLabelSizes[label.RowIndex - 1]);
                        }
                        if (_axis.AxisPosition == AxisPosition.Bottom)
                        {
                            rect.Top = rect.Bottom - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;
                            for (int index = 1; index < label.RowIndex; index++)
                            {
                                rect.Top += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Size = new(rect.Width, _axis.groupingLabelSizes[label.RowIndex - 1]);
                        }
                    }

                    // Unknown label row value
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionAxisLabelRowIndexMustBe1Or2));
                    }

                    //********************************************************************
                    //** Set label From and To coordinates.
                    //********************************************************************
                    double fromPosition = _axis.GetLinearPosition(labelFrom);
                    double toPosition = _axis.GetLinearPosition(labelTo);
                    if (_axis.AxisPosition == AxisPosition.Top || _axis.AxisPosition == AxisPosition.Bottom)
                    {
                        rect.Left = (float)Math.Min(fromPosition, toPosition);
                        rect.Right = (float)Math.Max(fromPosition, toPosition);
                        if (rect.Width < pixelSize.Width)
                        {
                            rect.Size = new(pixelSize.Width, rect.Height);
                        }

                        // Adjust label To/From position if offset labels are used
                        if (label.RowIndex == 0 &&
                            ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1)))
                        {
                            rect.Left -= rect.Width / 2F;
                            rect.Right += rect.Width;
                        }
                    }
                    else
                    {
                        rect.Top = (float)Math.Min(fromPosition, toPosition);
                        rect.Bottom = (float)Math.Max(fromPosition, toPosition);
                        if (rect.Height < pixelSize.Height)
                        {
                            rect.Size = new(rect.Width, pixelSize.Height);
                        }

                        // Adjust label To/From position if offset labels are used
                        if (label.RowIndex == 0 &&
                            ((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1)))
                        {
                            var h = rect.Height;
                            rect.Top -= h / 2F;
                            rect.Bottom = rect.Top + h * 2F;
                        }
                    }

                    // Save original rect
                    SKRect initialRect = new(rect.Left, rect.Top, rect.Right, rect.Bottom);

                    //********************************************************************
                    //** Transform and adjust label rectangle coordinates in 3D space.
                    //********************************************************************
                    Point3D[] rectPoints = new Point3D[3];
                    if (_axis.AxisPosition == AxisPosition.Left)
                    {
                        rectPoints[0] = new Point3D(rect.Right, rect.Top, labelsZPosition);
                        rectPoints[1] = new Point3D(rect.Right, rect.Top + rect.Height / 2f, labelsZPosition);
                        rectPoints[2] = new Point3D(rect.Right, rect.Bottom, labelsZPosition);
                        _axis.ChartArea.matrix3D.TransformPoints(rectPoints);
                        rect.Top = rectPoints[0].Y;
                        rect.Size = new(rectPoints[1].X - rect.Left, rectPoints[2].Y - rect.Top);
                    }
                    else if (_axis.AxisPosition == AxisPosition.Right)
                    {
                        rectPoints[0] = new Point3D(rect.Left, rect.Top, labelsZPosition);
                        rectPoints[1] = new Point3D(rect.Left, rect.Top + rect.Height / 2f, labelsZPosition);
                        rectPoints[2] = new Point3D(rect.Left, rect.Bottom, labelsZPosition);
                        _axis.ChartArea.matrix3D.TransformPoints(rectPoints);
                        rect.Top = rectPoints[0].Y;
                        rect.Size = new(rect.Right - rectPoints[1].X, rectPoints[2].Y - rect.Top);
                        rect.Left = rectPoints[1].X;
                    }
                    else if (_axis.AxisPosition == AxisPosition.Top)
                    {
                        // Transform 3 points of the rectangle
                        rectPoints[0] = new Point3D(rect.Left, rect.Bottom, labelsZPosition);
                        rectPoints[1] = new Point3D(rect.Left + rect.Width / 2f, rect.Bottom, labelsZPosition);
                        rectPoints[2] = new Point3D(rect.Right, rect.Bottom, labelsZPosition);
                        _axis.ChartArea.matrix3D.TransformPoints(rectPoints);

                        if (labelsPosition == AxisPosition.Top)
                        {
                            rect.Left = rectPoints[0].X;
                            rect.Size = new SKSize(rectPoints[2].X - rect.Left, rectPoints[1].Y - rect.Top);
                        }
                        else if (labelsPosition == AxisPosition.Right)
                        {
                            SKRect rightLabelsRect = GetAllLabelsRect(_axis.ChartArea, labelsPosition, format);
                            rect.Top = rectPoints[0].Y;
                            rect.Left = rectPoints[1].X;
                            rect.Size = new(rightLabelsRect.Right - rect.Left, rectPoints[2].Y - rect.Top);
                        }
                        else if (labelsPosition == AxisPosition.Left)
                        {
                            SKRect rightLabelsRect = GetAllLabelsRect(_axis.ChartArea, labelsPosition, format);
                            rect.Top = rectPoints[2].Y;
                            rect.Left = rightLabelsRect.Left;
                            rect.Size = new(rectPoints[1].X - rightLabelsRect.Left, rectPoints[0].Y - rect.Top);
                        }
                    }
                    else if (_axis.AxisPosition == AxisPosition.Bottom)
                    {
                        // Transform 3 points of the rectangle
                        rectPoints[0] = new Point3D(rect.Left, rect.Top, labelsZPosition);
                        rectPoints[1] = new Point3D(rect.Left + rect.Width / 2f, rect.Top, labelsZPosition);
                        rectPoints[2] = new Point3D(rect.Right, rect.Top, labelsZPosition);
                        _axis.ChartArea.matrix3D.TransformPoints(rectPoints);

                        if (labelsPosition == AxisPosition.Bottom)
                        {
                            rect.Left = rectPoints[0].X;
                            rect.Top = rectPoints[1].Y;
                            rect.Size = new(rectPoints[2].X - rect.Left, rect.Bottom - rectPoints[1].Y);
                        }
                        else if (labelsPosition == AxisPosition.Right)
                        {
                            SKRect rightLabelsRect = GetAllLabelsRect(_axis.ChartArea, labelsPosition, format);
                            rect.Top = rectPoints[2].Y;
                            rect.Left = rectPoints[1].X;
                            rect.Size = new(rightLabelsRect.Right - rect.Left, rectPoints[0].Y - rect.Top);

                            // Adjust label rect by shifting it down by quarter of the tick size
                            if (_axis.autoLabelAngle == 0)
                            {
                                rect.Top += _axis.markSize / 4f;
                            }
                        }
                        else if (labelsPosition == AxisPosition.Left)
                        {
                            SKRect rightLabelsRect = GetAllLabelsRect(_axis.ChartArea, labelsPosition, format);
                            rect.Top = rectPoints[0].Y;
                            rect.Left = rightLabelsRect.Left;
                            rect.Size = new SKSize(rectPoints[1].X - rightLabelsRect.Left, rectPoints[2].Y - rect.Top);

                            // Adjust label rect by shifting it down by quarter of the tick size
                            if (_axis.autoLabelAngle == 0)
                            {
                                rect.Top += _axis.markSize / 4f;
                            }
                        }
                    }

                    // Find axis with same position
                    Axis labelsAxis = null;
                    foreach (Axis curAxis in _axis.ChartArea.Axes)
                    {
                        if (curAxis.AxisPosition == labelsPosition)
                        {
                            labelsAxis = curAxis;
                            break;
                        }
                    }

                    //********************************************************************
                    //** Adjust font angles for Top and Bottom axis
                    //********************************************************************
                    int labelsFontAngle = (_axis.autoLabelAngle < -90) ? angle : _axis.autoLabelAngle;
                    if (labelsPosition != _axis.AxisPosition)
                    {
                        if ((_axis.AxisPosition == AxisPosition.Top || _axis.AxisPosition == AxisPosition.Bottom) &&
                            (labelsFontAngle == 90 || labelsFontAngle == -90))
                        {
                            labelsFontAngle = 0;
                        }
                        else if (_axis.AxisPosition == AxisPosition.Bottom)
                        {
                            if (labelsPosition == AxisPosition.Left && labelsFontAngle > 0)
                            {
                                labelsFontAngle = -labelsFontAngle;
                            }
                            else if (labelsPosition == AxisPosition.Right && labelsFontAngle < 0)
                            {
                                labelsFontAngle = -labelsFontAngle;
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Top)
                        {
                            if (labelsPosition == AxisPosition.Left && labelsFontAngle < 0)
                            {
                                labelsFontAngle = -labelsFontAngle;
                            }
                            else if (labelsPosition == AxisPosition.Right && labelsFontAngle > 0)
                            {
                                labelsFontAngle = -labelsFontAngle;
                            }
                        }
                    }

                    //********************************************************************
                    //** NOTE: Code below improves chart labels readability in scenarios
                    //** described in MS issue #65.
                    //**
                    //** Prevent labels in the first row from overlapping the grouping
                    //** labels in the rows below. The solution only apply to the limited
                    //** use cases defined by the condition below.
                    //********************************************************************
                    StringFormatFlags oldFormatFlags = format.FormatFlags;

                    if (label.RowIndex == 0 &&
                        labelsFontAngle == 0 &&
                        _axis.groupingLabelSizes != null &&
                        _axis.groupingLabelSizes.Length > 0 &&
                        _axis.AxisPosition == AxisPosition.Bottom &&
                        labelsPosition == AxisPosition.Bottom &&
                        !((_axis.autoLabelOffset == -1) ? IsStaggered : (_axis.autoLabelOffset == 1)))
                    {
                        if (firstLabelsRowHeight == -1f)
                        {
                            // Calculate first labels row max height
                            Point3D[] labelPositionPoints = new Point3D[1];
                            labelPositionPoints[0] = new Point3D(initialRect.Left, initialRect.Bottom - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment, labelsZPosition);
                            _axis.ChartArea.matrix3D.TransformPoints(labelPositionPoints);

                            float height = labelPositionPoints[0].Y - rect.Top;

                            firstLabelsRowHeight = (height > 0f) ? height : rect.Height;
                        }

                        // Resuse pre-calculated first labels row height
                        rect.Size = new(rect.Width, firstLabelsRowHeight);

                        // Change current string format to prevent strings to go out of the
                        // specified bounding rectangle
                        if ((format.FormatFlags & StringFormatFlags.LineLimit) == 0)
                        {
                            format.FormatFlags |= StringFormatFlags.LineLimit;
                        }
                    }

                    //********************************************************************
                    //** Draw label text.
                    //********************************************************************

                    using (SKPaint brush = new() { Color = (label.ForeColor == SKColor.Empty) ? _foreColor : label.ForeColor, Style = SKPaintStyle.Fill })
                    {
                        graph.DrawLabelStringRel(
                            labelsAxis,
                            label.RowIndex,
                            label.LabelMark,
                            label.MarkColor,
                            label.Text,
                            label.Image,
                            label.ImageTransparentColor,
                            _axis.autoLabelFont ?? _font,
                            brush,
                            rect,
                            format,
                            labelsFontAngle,
                            (!TruncatedLabels || label.RowIndex > 0) ? SKRect.Empty : boundaryRect,
                            label,
                            truncatedLeft,
                            truncatedRight);
                    }

                    // Restore old string format that was temporary modified
                    if (format.FormatFlags != oldFormatFlags)
                    {
                        format.FormatFlags = oldFormatFlags;
                    }
                }
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Sets the axis to which this object attached to.
        /// </summary>
        /// <returns>Axis object.</returns>
        internal Axis Axis
        {
            set { _axis = value; }
        }

        /// <summary>
        /// Invalidate chart picture
        /// </summary>
        internal override void Invalidate()
        {
            if (_axis != null)
            {
                _axis.Invalidate();
            }
            base.Invalidate();
        }

        #endregion

        #region	Label properties

        /// <summary>
        /// Gets or sets the interval offset of the label.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeLabel_IntervalOffset"),
        ]
        public double IntervalOffset
        {
            get
            {
                return intervalOffset;
            }
            set
            {
                intervalOffset = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets the interval offset.
        /// </summary>
        /// <returns></returns>
        internal double GetIntervalOffset()
        {
            if (double.IsNaN(intervalOffset) && _axis != null)
            {
                return _axis.IntervalOffset;
            }
            return intervalOffset;
        }

        /// <summary>
        /// Gets or sets the unit of measurement of the label offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeLabel_IntervalOffsetType"),
        ]
        public DateTimeIntervalType IntervalOffsetType
        {
            get
            {
                return intervalOffsetType;
            }
            set
            {
                intervalOffsetType = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets the type of the interval offset.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalOffsetType()
        {
            if (intervalOffsetType == DateTimeIntervalType.NotSet && _axis != null)
            {
                return _axis.IntervalOffsetType;
            }
            return intervalOffsetType;
        }

        /// <summary>
        /// Gets or sets the interval size of the label.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeLabel_Interval"),
        ]
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;

                // Reset original property value fields
                if (_axis != null)
                {
                    _axis.tempLabelInterval = interval;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <returns></returns>
		internal double GetInterval()
        {
            if (double.IsNaN(interval) && _axis != null)
            {
                return _axis.Interval;
            }
            return interval;
        }

        /// <summary>
        /// Gets or sets the unit of measurement of the interval size of the label.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeLabel_IntervalType"),
        ]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return intervalType;
            }
            set
            {
                intervalType = value;

                // Reset original property value fields
                if (_axis != null)
                {
                    _axis.tempLabelIntervalType = intervalType;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets the type of the interval.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalType()
        {
            if (intervalType == DateTimeIntervalType.NotSet && _axis != null)
            {
                return _axis.IntervalType;
            }
            return intervalType;
        }

        /// <summary>
        /// Gets or sets the font of the label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_Font")
        ]
        public SKFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                // Turn off labels autofitting
                if (_axis != null && _axis.Common != null && _axis.Common.Chart != null && !_axis.Common.Chart.serializing)
                {
                    _axis.IsLabelAutoFit = false;
                }

                _font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the fore color of the label.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeFontColor"),
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
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the angle at which font is drawn.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_FontAngle"),
        ]
        public int Angle
        {
            get
            {
                return angle;
            }
            set
            {
                if (value < -90 || value > 90)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAxisLabelFontAngleInvalid));
                }

                // Turn of label offset if angle is not 0, 90 or -90
                if (IsStaggered && value != 0 && value != -90 && value != 90)
                {
                    IsStaggered = false;
                }

                // Turn off labels autofitting
                if (_axis != null && _axis.Common != null && _axis.Common.Chart != null && !_axis.Common.Chart.serializing)
                {
                    _axis.IsLabelAutoFit = false;
                }

                angle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a property which specifies whether the labels are shown with offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_OffsetLabels"),
        ]
        public bool IsStaggered
        {
            get
            {
                return isStaggered;
            }
            set
            {
                // Make sure that angle is 0, 90 or -90
                if (value && (Angle != 0 || Angle != -90 || Angle != 90))
                {
                    Angle = 0;
                }

                // Turn off labels autofitting
                if (_axis != null && _axis.Common != null && _axis.Common.Chart != null && !_axis.Common.Chart.serializing)
                {
                    _axis.IsLabelAutoFit = false;
                }

                isStaggered = value;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a property which specifies whether the labels are shown at axis ends.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_ShowEndLabels"),
        ]
        public bool IsEndLabelVisible
        {
            get
            {
                return _isEndLabelVisible;
            }
            set
            {
                _isEndLabelVisible = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a property which specifies whether the label can be truncated.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_TruncatedLabels"),
        ]
        public bool TruncatedLabels
        {
            get
            {
                return _truncatedLabels;
            }
            set
            {
                _truncatedLabels = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the formatting string for the label text.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_Format"),
        ]
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a property which indicates whether the label is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLabel_Enabled"),
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
                Invalidate();
            }
        }

        #endregion

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

        #endregion
    }
}