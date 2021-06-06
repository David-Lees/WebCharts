// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	A cursor is a horizontal or vertical line that
//              defines a position along an axis. A range selection
//              is a range along an axis that is defined by a beginning
//              and end position, and is displayed using a semi-transparent
//              color.
//              Both cursors and range selections are implemented by the
//              Cursor class, which is exposed as the CursorX and CursorY
//              properties of the ChartArea object. The CursorX object is
//              for the X axis of a chart area, and the CursorY object is
//              for the Y axis. The AxisType property of these objects
//              determines if the associated axis is primary or secondary.
//              Cursors and range selections can be set via end-user
//              interaction and programmatically.
//

using SkiaSharp;
using System;
using System.Collections.Generic;

namespace WebCharts.Services
{
    /// <summary>
    /// The Cursor class is responsible for chart axes cursor and selection
    /// functionality. It contains properties which define visual appearance,
    /// position and behavior settings. It also contains methods for
    /// drawing cursor and selection in the plotting area.
    /// </summary>
    [
        SRDescription("DescriptionAttributeCursor_Cursor"),
    ]
    public class Cursor : IDisposable
    {
        #region Cursor constructors and initialization

        /// <summary>
		/// Public constructor
		/// </summary>
		public Cursor()
        {
        }

        /// <summary>
        /// Initialize cursor class.
        /// </summary>
        /// <param name="chartArea">Chart area the cursor belongs to.</param>
        /// <param name="attachedToXAxis">Indicates which axes should be used X or Y.</param>
        internal void Initialize(ChartArea chartArea, AxisName attachedToXAxis)
        {
            // Set chart are reference
            _chartArea = chartArea;

            // Attach cursor to specified axis
            _attachedToXAxis = attachedToXAxis;
        }

        #endregion Cursor constructors and initialization

        #region Cursor fields

        // Reference to the chart area object the cursor belongs to
        private ChartArea _chartArea = null;

        // Defines which axis the cursor attached to X or Y
        private AxisName _attachedToXAxis = AxisName.X;

        // Cursor line color
        private SKColor _lineColor = SKColors.Red;

        // Cursor line width
        private int _lineWidth = 1;

        // Cursor line style
        private ChartDashStyle _lineDashStyle = ChartDashStyle.Solid;

        // Chart area selection color
        private SKColor _selectionColor = SKColors.LightGray;

        // AxisName of the axes (primary/secondary) the cursor is attached to
        private AxisType _axisType = AxisType.Primary;

        // Cursor position
        private double _position = Double.NaN;

        // Range selection start position.
        private double _selectionStart = Double.NaN;

        // Range selection end position.
        private double _selectionEnd = Double.NaN;

        // Cursor movement interval type
        private DateTimeIntervalType _intervalType = DateTimeIntervalType.Auto;

        // Cursor movement interval offset current & original values
        private double _intervalOffset = 0;

        // Cursor movement interval offset type
        private DateTimeIntervalType _intervalOffsetType = DateTimeIntervalType.Auto;

        // Reference to the axis obhect
        private Axis _axis = null;

        // Indicates that selection must be drawn
        private bool _drawSelection = true;

        // Indicates that axis data scaleView was scrolled as a result of the mouse move event
        private readonly bool _viewScrolledOnMouseMove = false;

        #endregion Cursor fields

        #region Cursor "Behavior" public properties.

        /// <summary>
        /// Gets or sets the position of a cursor.
		/// </summary>
		[
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_Position"),
        ]
        public double Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    _position = value;

                    // Align cursor in connected areas
                    if (_chartArea != null && _chartArea.Common != null && _chartArea.Common.ChartPicture != null && !_chartArea.alignmentInProcess)
                    {
                        AreaAlignmentOrientations orientation = (_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ?
                            AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
                        _chartArea.Common.ChartPicture.AlignChartAreasCursor(_chartArea, orientation, false);
                    }

                    if (_chartArea != null && !_chartArea.alignmentInProcess)
                    {
                        Invalidate(false);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the starting position of a cursor's selected range.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_SelectionStart"),
        ]
        public double SelectionStart
        {
            get
            {
                return _selectionStart;
            }
            set
            {
                if (_selectionStart != value)
                {
                    _selectionStart = value;

                    // Align cursor in connected areas
                    if (_chartArea != null && _chartArea.Common != null && _chartArea.Common.ChartPicture != null && !_chartArea.alignmentInProcess)
                    {
                        AreaAlignmentOrientations orientation = (_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ?
                            AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
                        _chartArea.Common.ChartPicture.AlignChartAreasCursor(_chartArea, orientation, false);
                    }

                    if (_chartArea != null && !_chartArea.alignmentInProcess)
                    {
                        Invalidate(false);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the ending position of a range selection.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_SelectionEnd"),
        ]
        public double SelectionEnd
        {
            get
            {
                return _selectionEnd;
            }
            set
            {
                if (_selectionEnd != value)
                {
                    _selectionEnd = value;

                    // Align cursor in connected areas
                    if (_chartArea != null && _chartArea.Common != null && _chartArea.Common.ChartPicture != null && !_chartArea.alignmentInProcess)
                    {
                        AreaAlignmentOrientations orientation = (_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ?
                            AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
                        _chartArea.Common.ChartPicture.AlignChartAreasCursor(_chartArea, orientation, false);
                    }

                    if (_chartArea != null && !_chartArea.alignmentInProcess)
                    {
                        Invalidate(false);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a property that enables or disables the cursor interface.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_UserEnabled"),
        ]
        public bool IsUserEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets a property that enables or disables the range selection interface.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_UserSelection"),
        ]
        public bool IsUserSelectionEnabled { get; set; } = false;

        /// <summary>
        /// Determines if scrolling will occur if a range selection operation
        /// extends beyond a boundary of the chart area.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_AutoScroll"),
        ]
        public bool AutoScroll { get; set; } = true;

        /// <summary>
        ///  Gets or sets the type of axis that the cursor is attached to.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_AxisType"),
        ]
        public AxisType AxisType
        {
            get
            {
                return _axisType;
            }
            set
            {
                _axisType = value;

                // Reset reference to the axis object
                _axis = null;

                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets the cursor movement interval.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_Interval"),
        ]
        public double Interval { get; set; } = 1;

        /// <summary>
        /// Gets or sets the unit of measurement of the Interval property.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_IntervalType")
        ]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return _intervalType;
            }
            set
            {
                _intervalType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
            }
        }

        /// <summary>
        /// Gets or sets the interval offset, which determines
        /// where to draw the cursor and range selection.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_IntervalOffset"),
        ]
        public double IntervalOffset
        {
            get
            {
                return _intervalOffset;
            }
            set
            {
                // Validation
                if (value < 0.0)
                {
                    throw new ArgumentException(SR.ExceptionCursorIntervalOffsetIsNegative, nameof(value));
                }

                _intervalOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the unit of measurement of the IntervalOffset property.
        /// </summary>
        [
        SRCategory("CategoryAttributeBehavior"),
        SRDescription("DescriptionAttributeCursor_IntervalOffsetType"),
        ]
        public DateTimeIntervalType IntervalOffsetType
        {
            get
            {
                return _intervalOffsetType;
            }
            set
            {
                _intervalOffsetType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
            }
        }

        #endregion Cursor "Behavior" public properties.

        #region Cursor "Appearance" public properties

        /// <summary>
        /// Gets or sets the color the cursor line.
        /// </summary>
        public SKColor LineColor
        {
            get
            {
                return _lineColor;
            }
            set
            {
                _lineColor = value;
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the style of the cursor line.
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
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets the width of the cursor line.
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
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionCursorLineWidthIsNegative));
                }
                _lineWidth = value;
                Invalidate(true);
            }
        }

        /// <summary>
        /// Gets or sets a semi-transparent color that highlights a range of data.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeCursor_SelectionColor"),
        ]
        public SKColor SelectionColor
        {
            get
            {
                return _selectionColor;
            }
            set
            {
                _selectionColor = value;
                Invalidate(false);
            }
        }

        #endregion Cursor "Appearance" public properties

        #region Cursor painting methods

        /// <summary>
        /// Draws chart area cursor and selection.
        /// </summary>
        /// <param name="graph">Reference to the ChartGraphics object.</param>
        internal void Paint(ChartGraphics graph)
        {
            //***************************************************
            //** Prepare for drawing
            //***************************************************

            // Do not proceed with painting if cursor is not attached to the axis
            if (GetAxis() == null ||
                _chartArea == null ||
                _chartArea.Common == null ||
                _chartArea.Common.ChartPicture == null ||
                _chartArea.Common.ChartPicture.isPrinting)
            {
                return;
            }

            // Get plot area position
            SKRect plotAreaPosition = _chartArea.PlotAreaPosition.ToSKRect();

            // Detect if cursor is horizontal or vertical
            bool horizontal = true;
            if (GetAxis().AxisPosition == AxisPosition.Bottom || GetAxis().AxisPosition == AxisPosition.Top)
            {
                horizontal = false;
            }

            //***************************************************
            //** Draw selection
            //***************************************************

            // Check if selection need to be drawn
            if (_drawSelection &&
                !double.IsNaN(SelectionStart) &&
                !double.IsNaN(SelectionEnd) &&
                SelectionColor != SKColor.Empty)
            {
                // Calculate selection rectangle
                SKRect rectSelection = GetSelectionRect(plotAreaPosition);
                rectSelection.Intersect(plotAreaPosition);

                // Get opposite axis selection rectangle
                SKRect rectOppositeSelection = GetOppositeSelectionRect(plotAreaPosition);

                // Draw selection if rectangle is not empty
                if (!rectSelection.IsEmpty && rectSelection.Width > 0 && rectSelection.Height > 0)
                {
                    // Limit selection rectangle to the area of the opposite selection
                    if (!rectOppositeSelection.IsEmpty && rectOppositeSelection.Width > 0 && rectOppositeSelection.Height > 0)
                    {
                        rectSelection.Intersect(rectOppositeSelection);

                        // We do not need to draw selection in the opposite axis
                        Cursor oppositeCursor =
                            (_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ?
                            _chartArea.CursorY : _chartArea.CursorX;
                        oppositeCursor._drawSelection = false;
                    }

                    // Make sure selection is inside plotting area
                    rectSelection.Intersect(plotAreaPosition);

                    // If selection rectangle is not empty
                    if (rectSelection.Width > 0 && rectSelection.Height > 0)
                    {
                        // Add transparency to solid colors
                        var rangeSelectionColor = SelectionColor;
                        if (rangeSelectionColor.Alpha == 255)
                        {
                            rangeSelectionColor = new(rangeSelectionColor.Red, rangeSelectionColor.Green, rangeSelectionColor.Blue, 120);
                        }

                        // Draw selection
                        graph.FillRectangleRel(rectSelection,
                            rangeSelectionColor,
                            ChartHatchStyle.None,
                            "",
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
                }
            }

            //***************************************************
            //** Draw cursor
            //***************************************************

            // Check if cursor need to be drawn
            if (!double.IsNaN(Position) &&
                LineColor != SKColor.Empty &&
                LineWidth > 0 &&
                LineDashStyle != ChartDashStyle.NotSet)
            {
                // Calculate line position
                bool insideArea = false;
                SKPoint point1 = SKPoint.Empty;
                SKPoint point2 = SKPoint.Empty;
                if (horizontal)
                {
                    // Set cursor coordinates
                    point1.X = plotAreaPosition.Left;
                    point1.Y = (float)GetAxis().GetLinearPosition(Position);
                    point2.X = plotAreaPosition.Right;
                    point2.Y = point1.Y;

                    // Check if cursor is inside plotting rect
                    if (point1.Y >= plotAreaPosition.Top && point1.Y <= plotAreaPosition.Bottom)
                    {
                        insideArea = true;
                    }
                }
                else
                {
                    // Set cursor coordinates
                    point1.X = (float)GetAxis().GetLinearPosition(Position);
                    point1.Y = plotAreaPosition.Top;
                    point2.X = point1.X;
                    point2.Y = plotAreaPosition.Bottom;

                    // Check if cursor is inside plotting rect
                    if (point1.X >= plotAreaPosition.Left && point1.X <= plotAreaPosition.Right)
                    {
                        insideArea = true;
                    }
                }

                // Draw cursor if it's inside the chart area plotting rectangle
                if (insideArea)
                {
                    graph.DrawLineRel(LineColor, LineWidth, LineDashStyle, point1, point2);
                }
            }
            // Reset draw selection flag
            _drawSelection = true;
        }

        #endregion Cursor painting methods

        #region Position rounding methods

        /// <summary>
		/// Rounds new position of the cursor or range selection
		/// </summary>
		/// <param name="cursorPosition"></param>
		/// <returns></returns>
		internal double RoundPosition(double cursorPosition)
        {
            double roundedPosition = cursorPosition;

            if (!double.IsNaN(roundedPosition) && GetAxis() != null &&
                    Interval != 0 &&
                    !double.IsNaN(Interval))
            {
                // Get first series attached to this axis
                Series axisSeries = null;
                if (_axis.axisType == AxisName.X || _axis.axisType == AxisName.X2)
                {
                    List<string> seriesArray = _axis.ChartArea.GetXAxesSeries((_axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, _axis.SubAxisName);
                    if (seriesArray.Count > 0)
                    {
                        string seriesName = seriesArray[0] as string;
                        axisSeries = _axis.Common.DataManager.Series[seriesName];
                        if (axisSeries != null && !axisSeries.IsXValueIndexed)
                        {
                            axisSeries = null;
                        }
                    }
                }

                // If interval type is not set - use number
                DateTimeIntervalType intervalType =
                    (IntervalType == DateTimeIntervalType.Auto) ?
                    DateTimeIntervalType.Number : IntervalType;

                // If interval offset type is not set - use interval type
                DateTimeIntervalType offsetType =
                    (IntervalOffsetType == DateTimeIntervalType.Auto) ?
                intervalType : IntervalOffsetType;

                // Round numbers
                if (intervalType == DateTimeIntervalType.Number)
                {
                    double newRoundedPosition = Math.Round(roundedPosition / Interval) * Interval;

                    // Add offset number
                    if (IntervalOffset != 0 &&
                        !double.IsNaN(IntervalOffset) &&
                        offsetType != DateTimeIntervalType.Auto)
                    {
                        if (IntervalOffset > 0)
                        {
                            newRoundedPosition += ChartHelper.GetIntervalSize(newRoundedPosition, IntervalOffset, offsetType);
                        }
                        else
                        {
                            newRoundedPosition -= ChartHelper.GetIntervalSize(newRoundedPosition, IntervalOffset, offsetType);
                        }
                    }

                    // Find rounded position after/before the current
                    double nextPosition = newRoundedPosition;
                    if (newRoundedPosition <= cursorPosition)
                    {
                        nextPosition += ChartHelper.GetIntervalSize(newRoundedPosition, Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
                    }
                    else
                    {
                        nextPosition -= ChartHelper.GetIntervalSize(newRoundedPosition, Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
                    }

                    // Choose closest rounded position
                    if (Math.Abs(nextPosition - cursorPosition) > Math.Abs(cursorPosition - newRoundedPosition))
                    {
                        roundedPosition = newRoundedPosition;
                    }
                    else
                    {
                        roundedPosition = nextPosition;
                    }
                }

                // Round date/time
                else
                {
                    // Find one rounded position prior and one after current position
                    // Adjust start position depending on the interval and type
                    double prevPosition = ChartHelper.AlignIntervalStart(cursorPosition, Interval, intervalType, axisSeries);

                    // Adjust start position depending on the interval offset and offset type
                    if (IntervalOffset != 0 && axisSeries == null)
                    {
                        if (IntervalOffset > 0)
                        {
                            prevPosition += ChartHelper.GetIntervalSize(
                                prevPosition,
                                IntervalOffset,
                                offsetType,
                                axisSeries,
                                0,
                                DateTimeIntervalType.Number,
                                true);
                        }
                        else
                        {
                            prevPosition += ChartHelper.GetIntervalSize(
                                prevPosition,
                                -IntervalOffset,
                                offsetType,
                                axisSeries,
                                0,
                                DateTimeIntervalType.Number,
                                true);
                        }
                    }

                    // Find rounded position after/before the current
                    double nextPosition = prevPosition;
                    if (prevPosition <= cursorPosition)
                    {
                        nextPosition += ChartHelper.GetIntervalSize(prevPosition, Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
                    }
                    else
                    {
                        nextPosition -= ChartHelper.GetIntervalSize(prevPosition, Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
                    }

                    // Choose closest rounded position
                    if (Math.Abs(nextPosition - cursorPosition) > Math.Abs(cursorPosition - prevPosition))
                    {
                        roundedPosition = prevPosition;
                    }
                    else
                    {
                        roundedPosition = nextPosition;
                    }
                }
            }

            return roundedPosition;
        }

        #endregion Position rounding methods

        #region Cursor helper methods

        /// <summary>
        /// Helper function which returns a reference to the chart object
        /// </summary>
        /// <returns>Chart object reference.</returns>
        private ChartService GetChartObject()
        {
            if (_chartArea != null)
            {
                return _chartArea.Chart;
            }

            return null;
        }

        /// <summary>
        /// Get rectangle of the axis range selection.
        /// </summary>
        /// <returns>Selection rectangle.</returns>
        /// <param name="plotAreaPosition">Plot area rectangle.</param>
        /// <returns></returns>
        private SKRect GetSelectionRect(SKRect plotAreaPosition)
        {
            SKRect rect = SKRect.Empty;

            if (_axis != null &&
                SelectionStart != SelectionEnd)
            {
                double start = (float)_axis.GetLinearPosition(SelectionStart);
                double end = (float)_axis.GetLinearPosition(SelectionEnd);

                // Detect if cursor is horizontal or vertical
                bool horizontal = true;
                if (GetAxis().AxisPosition == AxisPosition.Bottom || GetAxis().AxisPosition == AxisPosition.Top)
                {
                    horizontal = false;
                }

                if (horizontal)
                {
                    rect.Left = plotAreaPosition.Left;
                    rect.Right = plotAreaPosition.Right;
                    rect.Top = (float)Math.Min(start, end);
                    rect.Bottom = (float)Math.Max(start, end);
                }
                else
                {
                    rect.Top = plotAreaPosition.Top;
                    rect.Bottom = plotAreaPosition.Bottom;
                    rect.Left = (float)Math.Min(start, end);
                    rect.Right = (float)Math.Max(start, end);
                }
            }

            return rect;
        }

        /// <summary>
        /// Get rectangle of the opposite axis selection
        /// </summary>
        /// <param name="plotAreaPosition">Plot area rectangle.</param>
        /// <returns>Opposite selection rectangle.</returns>
        private SKRect GetOppositeSelectionRect(SKRect plotAreaPosition)
        {
            if (_chartArea != null)
            {
                // Get opposite cursor
                Cursor oppositeCursor =
                    (_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ?
                    _chartArea.CursorY : _chartArea.CursorX;
                return oppositeCursor.GetSelectionRect(plotAreaPosition);
            }

            return SKRect.Empty;
        }

        /// <summary>
        /// Invalidate chart are with the cursor.
        /// </summary>
        /// <param name="invalidateArea">Chart area must be invalidated.</param>
        private void Invalidate(bool invalidateArea)
        {
            if (GetChartObject() != null && _chartArea != null && !GetChartObject().disableInvalidates)
            {
                // If data scaleView was scrolled - just invalidate the chart area
                if (_viewScrolledOnMouseMove || invalidateArea || GetChartObject().dirtyFlag)
                {
                    _chartArea.Invalidate();
                }

                // If only cursor/selection position was changed - use optimized drawing algorithm
                else
                {
                    // Set flag to redraw cursor/selection only
                    GetChartObject().paintTopLevelElementOnly = true;

                    // Invalidate and update the chart
                    _chartArea.Invalidate();

                    // Clear flag to redraw cursor/selection only
                    GetChartObject().paintTopLevelElementOnly = false;
                }
            }
        }

        /// <summary>
        /// Gets axis objects the cursor is attached to.
        /// </summary>
        /// <returns>Axis object.</returns>
        internal Axis GetAxis()
        {
            if (_axis == null && _chartArea != null)
            {
                if (_attachedToXAxis == AxisName.X)
                {
                    _axis = (_axisType == AxisType.Primary) ? _chartArea.AxisX : _chartArea.AxisX2;
                }
                else
                {
                    _axis = (_axisType == AxisType.Primary) ? _chartArea.AxisY : _chartArea.AxisY2;
                }
            }

            return _axis;
        }

        #endregion Cursor helper methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// The CursorEventArgs class stores the event arguments for cursor and selection events.
    /// </summary>
    public class CursorEventArgs : EventArgs
    {
        #region Private fields

        // Private fields for properties values storage
        private readonly ChartArea _chartArea = null;

        private readonly Axis _axis = null;

        #endregion Private fields

        #region Constructors

        /// <summary>
        /// CursorEventArgs constructor.
        /// </summary>
        /// <param name="chartArea">ChartArea of the cursor.</param>
        /// <param name="axis">Axis of the cursor.</param>
        /// <param name="newPosition">New cursor position.</param>
        public CursorEventArgs(ChartArea chartArea, Axis axis, double newPosition)
        {
            _chartArea = chartArea;
            _axis = axis;
            NewPosition = newPosition;
            NewSelectionStart = double.NaN;
            NewSelectionEnd = double.NaN;
        }

        /// <summary>
        /// CursorEventArgs constructor.
        /// </summary>
        /// <param name="chartArea">ChartArea of the cursor.</param>
        /// <param name="axis">Axis of the cursor.</param>
        /// <param name="newSelectionStart">New range selection starting position.</param>
        /// <param name="newSelectionEnd">New range selection ending position.</param>
        public CursorEventArgs(ChartArea chartArea, Axis axis, double newSelectionStart, double newSelectionEnd)
        {
            _chartArea = chartArea;
            _axis = axis;
            NewPosition = double.NaN;
            NewSelectionStart = newSelectionStart;
            NewSelectionEnd = newSelectionEnd;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// ChartArea of the event.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartArea"),
        ]
        public ChartArea ChartArea
        {
            get
            {
                return _chartArea;
            }
        }

        /// <summary>
        /// Axis of the event.
        /// </summary>
        [
        SRDescription("DescriptionAttributeAxis"),
        ]
        public Axis Axis
        {
            get
            {
                return _axis;
            }
        }

        /// <summary>
        /// New cursor position.
        /// </summary>
        [
        SRDescription("DescriptionAttributeCursorEventArgs_NewPosition"),
        ]
        public double NewPosition { get; set; }

        /// <summary>
        /// New range selection starting position.
        /// </summary>
        [
        SRDescription("DescriptionAttributeCursorEventArgs_NewSelectionStart"),
        ]
        public double NewSelectionStart { get; set; }

        /// <summary>
        /// New range selection ending position.
        /// </summary>
        [
        SRDescription("DescriptionAttributeCursorEventArgs_NewSelectionEnd"),
        ]
        public double NewSelectionEnd { get; set; }

        #endregion Properties
    }
}