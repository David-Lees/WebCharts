// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Base class for the Axis class which defines axis
//				csale related properties and methods.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WebCharts.Services
{
    #region Axis enumerations

    /// <summary>
    /// An enumeration of the mode of automatically calculating intervals.
    /// </summary>
    public enum IntervalAutoMode
    {
        /// <summary>
        /// Fixed number of intervals always created on the axis.
        /// </summary>
        FixedCount,

        /// <summary>
        /// Number of axis intervals depends on the axis length.
        /// </summary>
        VariableCount
    }

    /// <summary>
    /// An enumeration of axis position.
    /// </summary>
    internal enum AxisPosition
    {
        /// <summary>
        /// Left position
        /// </summary>
        Left,

        /// <summary>
        /// Right position
        /// </summary>
        Right,

        /// <summary>
        /// Top position
        /// </summary>
        Top,

        /// <summary>
        /// Bottom position
        /// </summary>
        Bottom
    }

    /// <summary>
    /// An enumeration of axis arrow styles.
    /// </summary>
    public enum AxisArrowStyle
    {
        /// <summary>
        /// No arrow
        /// </summary>
        None,

        /// <summary>
        /// Triangle type
        /// </summary>
        Triangle,

        /// <summary>
        /// Sharp triangle type
        /// </summary>
        SharpTriangle,

        /// <summary>
        /// Lines type
        /// </summary>
        Lines
    }

    #endregion Axis enumerations

    /// <summary>
    /// The Axis class keeps information about minimum, maximum
    /// and interval values and it is responsible for setting
    /// these values automatically. It also handles
    /// logarithmic and reversed axis.
    /// </summary>
    public partial class Axis
    {
        #region Axis scale fields

        // Represents the distance between the data points and its
        // chart area margin, Measured as a percentage of default
        // margin size.
        internal double margin = 100.0;

        internal double marginView = 0.0;
        internal bool offsetTempSet = false;

        // Used for column chart margin
        internal double marginTemp = 0.0;

        private readonly ArrayList _stripLineOffsets = new();

        // Data members, which store properties values
        private bool _isLogarithmic = false;

        internal double logarithmBase = 10.0;
        internal bool isReversed = false;
        internal bool isStartedFromZero = true;
        internal TickMark minorTickMark = null;
        internal TickMark majorTickMark = null;
        internal Grid minorGrid = null;
        internal Grid majorGrid = null;
        internal bool enabled = false;
        internal bool autoEnabled = true;
        internal LabelStyle labelStyle = null;
        private DateTimeIntervalType _internalIntervalType = DateTimeIntervalType.Auto;
        internal double maximum = Double.NaN;
        internal double crossing = Double.NaN;
        internal double minimum = Double.NaN;

        // Temporary Minimum and maximum values.
        internal double tempMaximum = Double.NaN;

        internal double tempMinimum = Double.NaN;
        internal double tempCrossing = Double.NaN;
        internal CustomLabelsCollection tempLabels;
        internal bool tempAutoMaximum = true;
        internal bool tempAutoMinimum = true;
        internal double tempMajorGridInterval = Double.NaN;
        internal double tempMinorGridInterval = 0.0;
        internal double tempMajorTickMarkInterval = Double.NaN;
        internal double tempMinorTickMarkInterval = 0.0;
        internal double tempLabelInterval = Double.NaN;
        internal DateTimeIntervalType tempGridIntervalType = DateTimeIntervalType.NotSet;
        internal DateTimeIntervalType tempTickMarkIntervalType = DateTimeIntervalType.NotSet;
        internal DateTimeIntervalType tempLabelIntervalType = DateTimeIntervalType.NotSet;

        // Paint mode
        internal bool paintMode = false;

        // Axis type (X, Y, X2, Y2)
        internal AxisName axisType = AxisName.X;

        // Automatic maximum value (from data point values).
        private bool _autoMaximum = true;

        // Automatic minimum value (from data point values).
        private bool _autoMinimum = true;

        /// <summary>
        /// Axis position: Left, Right, Top Bottom
        /// </summary>
        private AxisPosition _axisPosition = AxisPosition.Left;

        /// <summary>
        /// Opposite Axis for this Axis. Necessary for Crossing.
        /// </summary>
        internal Axis oppositeAxis = null;

        // Axis data scaleView
        private AxisScaleView _scaleView = null;

        // Axis scroll bar class
        internal AxisScrollBar scrollBar = null;

        // For scater chart X values could be rounded.
        internal bool roundedXValues = false;

        // If Axis is logarithmic value shoud be converted to
        // linear only once.
        internal bool logarithmicConvertedToLinear = false;

        // IsLogarithmic minimum value
        internal double logarithmicMinimum;

        // IsLogarithmic maximum value
        internal double logarithmicMaximum;

        // Correction of interval because of
        // 3D Rotation and perspective
        internal double interval3DCorrection = Double.NaN;

        // Axis coordinate convertion optimization fields
        internal bool optimizedGetPosition = false;

        internal double paintViewMax = 0.0;
        internal double paintViewMin = 0.0;
        internal double paintRange = 0.0;
        internal double valueMultiplier = 0.0;
        internal SKRect paintAreaPosition = SKRect.Empty;
        internal double paintAreaPositionBottom = 0.0;
        internal double paintAreaPositionRight = 0.0;
        internal double paintChartAreaSize = 0.0;

        // Determines how number of intervals automatically calculated
        private IntervalAutoMode _intervalAutoMode = IntervalAutoMode.FixedCount;

        // True if scale segments are used
        internal bool scaleSegmentsUsed = false;

        // Preffered number of intervals on the axis
        internal int prefferedNumberofIntervals = 5;

        private readonly Stack<Double> _intervalsStore = new();

        #endregion Axis scale fields

        #region Axis scale properties

        /// <summary>
        /// Axis position
        /// </summary>
        [
        SRDescription("DescriptionAttributeReverse"),
        ]
        virtual internal AxisPosition AxisPosition
        {
            get
            {
                return _axisPosition;
            }
            set
            {
                _axisPosition = value;
#if SUBAXES
				// Update axis position of the sub axis
				if( !((Axis)this).IsSubAxis )
				{
					foreach(SubAxis subAxis in ((Axis)this).SubAxes)
					{
						subAxis._axisPosition = value;
					}
				}

#endif // SUBAXES
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the number of intervals
        /// on the axis is fixed or varies with the axis size.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeIntervalAutoMode"),
        ]
        public IntervalAutoMode IntervalAutoMode
        {
            get
            {
                return _intervalAutoMode;
            }
            set
            {
                _intervalAutoMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the axis is reversed.
        /// If set to reversed, the values on the axis are in reversed sort order
        /// and the direction of values on the axis is flipped.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeReverse"),
        ]
        public bool IsReversed
        {
            get
            {
                return isReversed;
            }
            set
            {
                isReversed = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the minimum value
        /// of the axis will be automatically set to zero if all data point
        /// values are positive.  If there are negative data point values,
        /// the minimum value of the data points will be used.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeStartFromZero3"),
        ]
        public bool IsStartedFromZero
        {
            get
            {
                return isStartedFromZero;
            }
            set
            {
                isStartedFromZero = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag to add a margin to the axis.
        /// If true, a space is added between the first/last data
        /// point and the border of chart area.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeMargin"),
        ]
        public bool IsMarginVisible
        {
            get
            {
                if (margin > 0)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    margin = 100;
                else
                    margin = 0;

                Invalidate();
            }
        }

        /// <summary>
        /// Date and time interval type.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeInternalIntervalType"),
        ]
        internal DateTimeIntervalType InternalIntervalType
        {
            get
            {
                return _internalIntervalType;
            }
            set
            {
                // Set intervals for labels, grids and tick marks. ( Auto interval type )
                if (tempMajorGridInterval <= 0.0 ||
                    (double.IsNaN(tempMajorGridInterval) && Interval <= 0.0))
                {
                    majorGrid.intervalType = value;
                }

                if (tempMajorTickMarkInterval <= 0.0 ||
                    (double.IsNaN(tempMajorTickMarkInterval) && Interval <= 0.0))
                {
                    majorTickMark.intervalType = value;
                }

                if (tempLabelInterval <= 0.0 ||
                    (double.IsNaN(tempLabelInterval) && Interval <= 0.0))
                {
                    labelStyle.intervalType = value;
                }

                _internalIntervalType = value;

                Invalidate();
            }
        }

        /// <summary>
        /// Sets auto interval values to grids, tick marks
        /// and labels
        /// </summary>
        internal void SetInterval(double value)
        {
            if (tempMajorGridInterval <= 0.0 ||
                (double.IsNaN(tempMajorGridInterval) && Interval <= 0.0))
            {
                majorGrid.interval = value;
            }

            if (tempMajorTickMarkInterval <= 0.0 ||
                (double.IsNaN(tempMajorTickMarkInterval) && Interval <= 0.0))
            {
                majorTickMark.interval = value;
            }

            if (tempLabelInterval <= 0.0 ||
                (double.IsNaN(tempLabelInterval) && Interval <= 0.0))
            {
                labelStyle.interval = value;
            }

            Invalidate();
        }

        /// <summary>
        /// Sets auto interval values to grids, tick marks
        /// and labels
        /// </summary>
        internal void SetIntervalAndType(double newInterval, DateTimeIntervalType newIntervalType)
        {
            if (tempMajorGridInterval <= 0.0 ||
                (double.IsNaN(tempMajorGridInterval) && Interval <= 0.0))
            {
                majorGrid.interval = newInterval;
                majorGrid.intervalType = newIntervalType;
            }

            if (tempMajorTickMarkInterval <= 0.0 ||
                (double.IsNaN(tempMajorTickMarkInterval) && Interval <= 0.0))
            {
                majorTickMark.interval = newInterval;
                majorTickMark.intervalType = newIntervalType;
            }

            if (tempLabelInterval <= 0.0 ||
                (double.IsNaN(tempLabelInterval) && Interval <= 0.0))
            {
                labelStyle.interval = newInterval;
                labelStyle.intervalType = newIntervalType;
            }

            Invalidate();
        }

        /// <summary>
        /// Gets or sets the maximum axis value.
        /// </summary>
        [

        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeMaximum"),
        ]
        public double Maximum
        {
            get
            {
                // Get maximum
                if (_isLogarithmic && logarithmicConvertedToLinear && !Double.IsNaN(maximum))
                    return logarithmicMaximum;
                else
                    return maximum;
            }
            set
            {
                // Split a value to maximum and auto maximum
                if (Double.IsNaN(value))
                {
                    _autoMaximum = true;
                    maximum = Double.NaN;
                }
                else
                {
                    // Set maximum
                    maximum = value;

                    // Set non linearized Maximum for logarithmic scale
                    logarithmicMaximum = value;

                    _autoMaximum = false;
                }

                // Reset original property value fields
                tempMaximum = maximum;

                // This line is added because of Save ScaleView State August 29, 2003
                // in Web Forms. This place could cause problems with Reset Auto Values.
                tempAutoMaximum = _autoMaximum;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the minimum axis value
        /// </summary>
        [
        SRDescription("DescriptionAttributeMinimum"),
        ]
        public double Minimum
        {
            get
            {
                // Get minimum
                if (_isLogarithmic && logarithmicConvertedToLinear && !Double.IsNaN(maximum))
                    return logarithmicMinimum;
                else
                    return minimum;
            }
            set
            {
                // Split a value to minimum and auto minimum
                if (Double.IsNaN(value))
                {
                    _autoMinimum = true;
                    minimum = Double.NaN;
                }
                else
                {
                    // Set maximum
                    minimum = value;
                    _autoMinimum = false;

                    // Set non linearized Minimum for logarithmic scale
                    logarithmicMinimum = value;
                }

                // Reset original property value fields
                tempMinimum = minimum;

                // This line is added because of Save ScaleView State August 29, 2003
                // in Web Forms. This place could cause problems with Reset Auto Values.
                tempAutoMinimum = _autoMinimum;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the point where axis is crossed by another axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        ]
        virtual public double Crossing
        {
            get
            {
                if (paintMode)
                    if (_isLogarithmic)
                        return Math.Pow(logarithmBase, GetCrossing());
                    else
                        return GetCrossing();
                else
                    return crossing;
            }
            set
            {
                crossing = value;

                // Reset original property value fields
                tempCrossing = crossing;

                Invalidate();
            }
        }

        /// <summary>
        /// Enables or disables the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeEnabled7"),
        ]
        public AxisEnabled Enabled
        {
            get
            {
                // Take Enabled from two fields: enabled and auto enabled
                if (autoEnabled)
                {
                    return AxisEnabled.Auto;
                }
                else if (enabled)
                {
                    return AxisEnabled.True;
                }
                else
                {
                    return AxisEnabled.False;
                }
            }
            set
            { // Split Enabled to two fields: enabled and auto enabled
                if (value == AxisEnabled.Auto)
                {
                    autoEnabled = true;
                }
                else if (value == AxisEnabled.True)
                {
                    enabled = true;
                    autoEnabled = false;
                }
                else
                {
                    enabled = false;
                    autoEnabled = false;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the axis is logarithmic.
        /// Zeros or negative data values are not allowed on logarithmic charts.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeLogarithmic"),
        ]
        public bool IsLogarithmic
        {
            get
            {
                return _isLogarithmic;
            }
            set
            {
                _isLogarithmic = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Base of the logarithm used in logarithmic scale.
        /// By default, this value is 10.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeLogarithmBase"),
        ]
        public double LogarithmBase
        {
            get
            {
                return logarithmBase;
            }
            set
            {
                if (value < 2.0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAxisScaleLogarithmBaseInvalid));
                }

                logarithmBase = value;

                Invalidate();
            }
        }

        #endregion Axis scale properties

        #region Axis Segments and Scale Breaks Properties

        // Field that stores Axis automatic scale breaks style.
        internal AxisScaleBreakStyle axisScaleBreakStyle = null;

        /// <summary>
        /// Gets or sets the style of scale breaks.
        /// </summary>
		[
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeScaleBreakStyle"),
        ]
        virtual public AxisScaleBreakStyle ScaleBreakStyle
        {
            get
            {
                return axisScaleBreakStyle;
            }
            set
            {
                axisScaleBreakStyle = value;
                axisScaleBreakStyle.axis = this;
                this.Invalidate();
            }
        }

        // Field that stores axis scale segments
        internal AxisScaleSegmentCollection scaleSegments = null;

        /// <summary>
        /// Axis scale segment collection.
        /// </summary>
        [
        SRCategory("CategoryAttributeScale"),
        SRDescription("DescriptionAttributeAxisScaleSegmentCollection_AxisScaleSegmentCollection")
        ]
        internal AxisScaleSegmentCollection ScaleSegments
        {
            get
            {
                return scaleSegments;
            }
        }

        #endregion Axis Segments and Scale Breaks Properties

        #region Axis data scaleView properies and methods

        /// <summary>
        /// Gets or sets the scale view settings of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeDataView"),
        SRDescription("DescriptionAttributeView"),
        ]
        public AxisScaleView ScaleView
        {
            get
            {
                return _scaleView;
            }
            set
            {
                _scaleView = value;
                _scaleView.axis = this;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the scroll bar settings of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeDataView"),
        SRDescription("DescriptionAttributeScrollBar"),
        ]
        public AxisScrollBar ScrollBar
        {
            get
            {
                return scrollBar;
            }
            set
            {
                scrollBar = value;
                scrollBar.axis = this;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets axis data scaleView minimum position.
        /// </summary>
        /// <returns>Axis data scaleView minimum position.</returns>
        internal double ViewMinimum
        {
            get { return _scaleView.ViewMinimum; }
        }

        /// <summary>
        /// Gets axis data scaleView minimum position.
        /// </summary>
        /// <returns>Axis data scaleView minimum position.</returns>
        internal double ViewMaximum
        {
            get { return _scaleView.ViewMaximum; }
        }

        /// <summary>
        /// Gets automatic maximum value (from data point values).
        /// </summary>
        internal bool AutoMaximum
        {
            get { return _autoMaximum; }
        }

        /// <summary>
        /// Gets automatic minimum value (from data point values).
        /// </summary>
        internal bool AutoMinimum
        {
            get { return _autoMinimum; }
        }

        #endregion Axis data scaleView properies and methods

        #region Axis position converters methos

        /// <summary>
        /// This function converts axis value to relative position (0-100%).
        /// If an axis has a logarithmic scale, the value is converted to a linear scale.
        /// </summary>
        /// <param name="axisValue">Value from axis.</param>
        /// <returns>Relative position (0-100%).</returns>
        public double GetPosition(double axisValue)
        {
            // Adjust for the IsLogarithmic axis
            if (_isLogarithmic && axisValue != 0.0)
            {
                axisValue = Math.Log(axisValue, logarithmBase);
            }

            // Get linear position
            return GetLinearPosition(axisValue);
        }

        /// <summary>
        /// This function converts an axis value to relative position (0-100%).
        /// If an axis has a logarithmic scale, the value is converted to a linear scale.
        /// </summary>
        /// <param name="axisValue">Axis value.</param>
        /// <returns>Relative position (0-100%).</returns>
        public double ValueToPosition(double axisValue)
        {
            return GetPosition(axisValue);
        }

        /// <summary>
        /// This function converts an axis value to a pixel position.
        /// If an axis has a logarithmic scale, the value is converted to a linear scale.
        /// </summary>
        /// <param name="axisValue">Value from axis.</param>
        /// <returns>Pixel position.</returns>
        public double ValueToPixelPosition(double axisValue)
        {
            // Get relative value
            double val = ValueToPosition(axisValue);

            // Convert it to pixels
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                val *= (Common.ChartPicture.Width - 1) / 100F;
            }
            else
            {
                val *= (Common.ChartPicture.Height - 1) / 100F;
            }

            return val;
        }

        /// <summary>
        /// This function converts a relative position to an axis value.
        /// If an axis has a logarithmic scale, the value is converted to a linear scale.
        /// </summary>
        /// <param name="position">Relative position (0-100%).</param>
        /// <returns>Axis value.</returns>
        public double PositionToValue(double position)
        {
            return PositionToValue(position, true);
        }

        /// <summary>
        /// This function converts a relative position to an axis value.
        /// If an axis has a logarithmic scale, the value is converted to a linear scale.
        /// </summary>
        /// <param name="position">Relative position (0-100%).</param>
        /// <param name="validateInput">Indicates if input value range should be checked.</param>
        /// <returns>Axis value.</returns>
        internal double PositionToValue(double position, bool validateInput)
        {
            // Check parameters
            if (validateInput &&
                (position < 0 || position > 100))
            {
                throw (new ArgumentException(SR.ExceptionAxisScalePositionInvalid, nameof(position)));
            }

            // Check if plot area position was already calculated
            if (PlotAreaPosition == null)
            {
                throw (new InvalidOperationException(SR.ExceptionAxisScalePositionToValueCallFailed));
            }

            // Convert chart picture position to plotting position
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                position -= PlotAreaPosition.X;
            else
                position = PlotAreaPosition.Bottom - position;

            // The Chart area size
            double ChartAreaSize;
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                ChartAreaSize = PlotAreaPosition.Width;
            else
                ChartAreaSize = PlotAreaPosition.Height;

            // The Real range as double
            double viewMax = ViewMaximum;
            double viewMin = ViewMinimum;
            double range = viewMax - viewMin;

            // Avoid division by zero
            double axisValue = 0;
            if (range != 0)
            {
                // Find axis value from position
                axisValue = range / ChartAreaSize * position;
            }

            // Corrected axis value for reversed
            if (isReversed)
                axisValue = viewMax - axisValue;
            else
                axisValue = viewMin + axisValue;

            return axisValue;
        }

        /// <summary>
        /// This function converts a pixel position to an axis value.
        /// If an axis has a logarithmic scale, the value is converted to a linear scale.
        /// </summary>
        /// <param name="position">Pixel position.</param>
        /// <returns>Axis value.</returns>
        public double PixelPositionToValue(double position)
        {
            // Convert it to pixels
            double val = position;
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                val *= 100F / (Common.ChartPicture.Width - 1);
            }
            else
            {
                val *= 100F / (Common.ChartPicture.Height - 1);
            }

            // Get from relative position
            return PositionToValue(val);
        }

        #endregion Axis position converters methos

        #region Axis scale methods

        /// <summary>
        /// Sets axis position. Axis position depends
        /// on crossing and reversed value.
        /// </summary>
        internal void SetAxisPosition()
        {
            // Change position of the axis
            if (GetOppositeAxis().isReversed)
            {
                if (AxisPosition == AxisPosition.Left)
                    AxisPosition = AxisPosition.Right;
                else if (AxisPosition == AxisPosition.Right)
                    AxisPosition = AxisPosition.Left;
                else if (AxisPosition == AxisPosition.Top)
                    AxisPosition = AxisPosition.Bottom;
                else if (AxisPosition == AxisPosition.Bottom)
                    AxisPosition = AxisPosition.Top;
            }
        }

        /// <summary>
        /// Sets temporary offset value.
        /// </summary>
        internal void SetTempAxisOffset()
        {
            if (ChartArea.Series.Count == 0)
            {
                return;
            }
            // Conditions when this code changes margin size: Column chart,
            // margin is turned off, Interval offset is not used for
            // gridlines, tick marks and labels.
            Series ser = ChartArea.GetFirstSeries();
            if ((ser.ChartType == SeriesChartType.Column ||
                ser.ChartType == SeriesChartType.StackedColumn ||
                ser.ChartType == SeriesChartType.StackedColumn100 ||
                ser.ChartType == SeriesChartType.Bar ||

                ser.ChartType == SeriesChartType.RangeBar ||
                ser.ChartType == SeriesChartType.RangeColumn ||

                ser.ChartType == SeriesChartType.StackedBar ||
                ser.ChartType == SeriesChartType.StackedBar100) &&
                margin != 100.0 && !offsetTempSet &&
                _autoMinimum)
            {
                // Find offset correction for Column chart margin.
                double offset;
                marginTemp = margin;

                // Find point width
                // Check if series provide custom value for point width
                double pointWidthSize;
                string strWidth = ser[CustomPropertyName.PointWidth];
                if (strWidth != null)
                {
                    pointWidthSize = CommonElements.ParseDouble(strWidth);
                }
                else
                {
                    pointWidthSize = 0.8;
                }

                margin = (pointWidthSize / 2) * 100;
                offset = (margin) / 100;
                double contraOffset = (100 - margin) / 100;

                if (_intervalsStore.Count == 0)
                {
                    _intervalsStore.Push(labelStyle.intervalOffset);
                    _intervalsStore.Push(majorGrid.intervalOffset);
                    _intervalsStore.Push(majorTickMark.intervalOffset);
                    _intervalsStore.Push(minorGrid.intervalOffset);
                    _intervalsStore.Push(minorTickMark.intervalOffset);
                }

                labelStyle.intervalOffset = double.IsNaN(labelStyle.intervalOffset) ? offset : labelStyle.intervalOffset + offset;
                majorGrid.intervalOffset = double.IsNaN(majorGrid.intervalOffset) ? offset : majorGrid.intervalOffset + offset;
                majorTickMark.intervalOffset = double.IsNaN(majorTickMark.intervalOffset) ? offset : majorTickMark.intervalOffset + offset;
                minorGrid.intervalOffset = double.IsNaN(minorGrid.intervalOffset) ? offset : minorGrid.intervalOffset + offset;
                minorTickMark.intervalOffset = double.IsNaN(minorTickMark.intervalOffset) ? offset : minorTickMark.intervalOffset + offset;

                foreach (StripLine strip in StripLines)
                {
                    _stripLineOffsets.Add(strip.IntervalOffset);
                    strip.IntervalOffset -= contraOffset;
                }
                offsetTempSet = true;
            }
        }

        /// <summary>
        /// Resets temporary offset value.
        /// </summary>
        internal void ResetTempAxisOffset()
        {
            if (offsetTempSet)
            {
                System.Diagnostics.Debug.Assert(_intervalsStore.Count == 5, "Fail in interval store count");

                minorTickMark.intervalOffset = _intervalsStore.Pop();
                minorGrid.intervalOffset = _intervalsStore.Pop();
                majorTickMark.intervalOffset = _intervalsStore.Pop();
                majorGrid.intervalOffset = _intervalsStore.Pop();
                labelStyle.intervalOffset = _intervalsStore.Pop();
                int index = 0;
                foreach (StripLine strip in StripLines)
                {
                    if (_stripLineOffsets.Count > index)
                    {
                        strip.IntervalOffset = (double)_stripLineOffsets[index];
                    }
                    index++;
                }
                _stripLineOffsets.Clear();
                offsetTempSet = false;
                margin = marginTemp;
            }
        }

        /// <summary>
        /// This function will create auto maximum and minimum values
        /// using the interval. This function will make a gap between
        /// data points and border of the chart area.
        /// </summary>
        /// <param name="inter">Interval</param>
        /// <param name="shouldStartFromZero">True if minimum scale value should start from zero.</param>
        /// <param name="autoMax">Maximum is auto</param>
        /// <param name="autoMin">Minimum is auto</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Interval</returns>
        internal double RoundedValues(
            double inter,
            bool shouldStartFromZero,
            bool autoMax,
            bool autoMin,
            ref double min,
            ref double max)
        {
            // For X Axes
            if (axisType == AxisName.X || axisType == AxisName.X2)
            {
                if (margin == 0.0 && !roundedXValues)
                {
                    return inter;
                }
            }
            else // For Y Axes
            {
                // Avoid dividing with 0. There is no gap.
                if (margin == 0.0)
                {
                    return inter;
                }
            }

            if (autoMin)
            { // Set minimum value
                if (min < 0.0 || (!shouldStartFromZero && !ChartArea.stacked))
                {
                    min = (double)(((decimal)Math.Ceiling(min / inter) - 1m) * (decimal)inter);
                }
                else
                {
                    min = 0.0;
                }
            }
            if (autoMax)
            {// Set maximum value
                if (max <= 0.0 && shouldStartFromZero)
                {
                    max = 0.0;
                }
                else
                {
                    max = (double)(((decimal)Math.Floor(max / inter) + 1m) * (decimal)inter);
                }
            }
            return inter;
        }

        /// <summary>
        /// Recalculates an intelligent interval from real interval.
        /// </summary>
        /// <param name="diff">Real interval.</param>
        /// <returns>Inteligent interval.</returns>
        internal double CalcInterval(double diff)
        {
            // If the interval is zero return error
            if (diff == 0.0)
            {
                throw (new ArgumentOutOfRangeException(nameof(diff), SR.ExceptionAxisScaleIntervalIsZero));
            }

            // If the real interval is > 1.0
            double step = -1;
            double temp = diff;
            while (temp > 1.0)
            {
                step++;
                temp /= 10.0;
                if (step > 1000)
                {
                    throw new InvalidOperationException(SR.ExceptionAxisScaleMinimumMaximumInvalid);
                }
            }

            // If the real interval is < 1.0
            temp = diff;
            if (temp < 1.0)
            {
                step = 0;
            }

            while (temp < 1.0)
            {
                step--;
                temp *= 10.0;
                if (step < -1000)
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisScaleMinimumMaximumInvalid));
                }
            }

            double power = (IsLogarithmic) ? logarithmBase : 10.0;
            double tempDiff = diff / Math.Pow(power, step);

            if (tempDiff < 3)
                tempDiff = 2;
            else if (tempDiff < 7)
                tempDiff = 5;
            else
                tempDiff = 10;

            // Make a correction of the real interval
            return tempDiff * Math.Pow(power, step);
        }

        /// <summary>
        /// Recalculates a intelligent interval from real interval
        /// obtained from maximum and minimum values
        /// </summary>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Auto Interval</returns>
        private double CalcInterval(double min, double max)
        {
            // Approximated interval value
            return CalcInterval((max - min) / 5);
        }

        /// <summary>
        /// Recalculates a intelligent interval from real interval
        /// obtained from maximum, minimum and date type if
        /// the values is date-time value.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="date">True if date.</param>
        /// <param name="type">Date time interval type.</param>
        /// <param name="valuesType">AxisName of date-time values.</param>
        /// <returns>Auto Interval.</returns>
        internal double CalcInterval(
            double min,
            double max,
            bool date,
            out DateTimeIntervalType type,
            ChartValueType valuesType)
        {
            // AxisName is date time
            if (date)
            {
                DateTime dateTimeMin = DateTime.FromOADate(min);
                DateTime dateTimeMax = DateTime.FromOADate(max);
                TimeSpan timeSpan = dateTimeMax.Subtract(dateTimeMin);

                // Minutes
                double inter = timeSpan.TotalMinutes;

                // For Range less than 60 seconds interval is 5 sec
                if (inter <= 1.0 && valuesType != ChartValueType.Date)
                {
                    // Milli Seconds
                    double mlSeconds = timeSpan.TotalMilliseconds;
                    if (mlSeconds <= 10)
                    {
                        type = DateTimeIntervalType.Milliseconds;
                        return 1;
                    }
                    if (mlSeconds <= 50)
                    {
                        type = DateTimeIntervalType.Milliseconds;
                        return 4;
                    }
                    if (mlSeconds <= 200)
                    {
                        type = DateTimeIntervalType.Milliseconds;
                        return 20;
                    }
                    if (mlSeconds <= 500)
                    {
                        type = DateTimeIntervalType.Milliseconds;
                        return 50;
                    }

                    // Seconds
                    double seconds = timeSpan.TotalSeconds;

                    if (seconds <= 7)
                    {
                        type = DateTimeIntervalType.Seconds;
                        return 1;
                    }
                    else if (seconds <= 15)
                    {
                        type = DateTimeIntervalType.Seconds;
                        return 2;
                    }
                    else if (seconds <= 30)
                    {
                        type = DateTimeIntervalType.Seconds;
                        return 5;
                    }
                    else if (seconds <= 60)
                    {
                        type = DateTimeIntervalType.Seconds;
                        return 10;
                    }
                }// For Range less than 120 seconds interval is 10 sec
                else if (inter <= 2.0 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Seconds;
                    return 20;
                }// For Range less than 180 seconds interval is 30 sec
                else if (inter <= 3.0 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Seconds;
                    return 30;
                }

                // For Range less than 10 minutes interval is 1 min
                else if (inter <= 10 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Minutes;
                    return 1;
                }
                // For Range less than 20 minutes interval is 1 min
                else if (inter <= 20 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Minutes;
                    return 2;
                }// For Range less than 60 minutes interval is 5 min
                else if (inter <= 60 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Minutes;
                    return 5;
                }// For Range less than 120 minutes interval is 10 min
                else if (inter <= 120 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Minutes;
                    return 10;
                }// For Range less than 180 minutes interval is 30 min
                else if (inter <= 180 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Minutes;
                    return 30;
                }
                // For Range less than 12 hours interval is 1 hour
                else if (inter <= 60 * 12 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Hours;
                    return 1;
                }
                // For Range less than 24 hours interval is 4 hour
                else if (inter <= 60 * 24 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Hours;
                    return 4;
                }
                // For Range less than 2 days interval is 6 hour
                else if (inter <= 60 * 24 * 2 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Hours;
                    return 6;
                }
                // For Range less than 3 days interval is 12 hour
                else if (inter <= 60 * 24 * 3 && valuesType != ChartValueType.Date)
                {
                    type = DateTimeIntervalType.Hours;
                    return 12;
                }

                // For Range less than 10 days interval is 1 day
                else if (inter <= 60 * 24 * 10)
                {
                    type = DateTimeIntervalType.Days;
                    return 1;
                }
                // For Range less than 20 days interval is 2 day
                else if (inter <= 60 * 24 * 20)
                {
                    type = DateTimeIntervalType.Days;
                    return 2;
                }
                // For Range less than 30 days interval is 3 day
                else if (inter <= 60 * 24 * 30)
                {
                    type = DateTimeIntervalType.Days;
                    return 3;
                }
                // For Range less than 2 months interval is 1 week
                else if (inter <= 60 * 24 * 30.5 * 2)
                {
                    type = DateTimeIntervalType.Weeks;
                    return 1;
                }
                // For Range less than 5 months interval is 2weeks
                else if (inter <= 60 * 24 * 30.5 * 5)
                {
                    type = DateTimeIntervalType.Weeks;
                    return 2;
                }
                // For Range less than 12 months interval is 1 month
                else if (inter <= 60 * 24 * 30.5 * 12)
                {
                    type = DateTimeIntervalType.Months;
                    return 1;
                }
                // For Range less than 24 months interval is 3 month
                else if (inter <= 60 * 24 * 30.5 * 24)
                {
                    type = DateTimeIntervalType.Months;
                    return 3;
                }
                // For Range less than 48 months interval is 6 months
                else if (inter <= 60 * 24 * 30.5 * 48)
                {
                    type = DateTimeIntervalType.Months;
                    return 6;
                }
                // For Range more than 48 months interval is year
                else if (inter >= 60 * 24 * 30.5 * 48)
                {
                    type = DateTimeIntervalType.Years;
                    return CalcYearInterval(inter / 60 / 24 / 365);
                }
            }

            // Else numbers
            type = DateTimeIntervalType.Number;
            return CalcInterval(min, max);
        }

        /// <summary>
        /// Recalculates a intelligent interval for years
        /// </summary>
        /// <param name="years">Number of years</param>
        /// <returns>Interval in years</returns>
        private static double CalcYearInterval(double years)
        {
            // If the interval is zero return error
            if (years <= 1.0)
            {
                throw (new ArgumentOutOfRangeException(nameof(years), SR.ExceptionAxisScaleIntervalIsLessThen1Year));
            }

            if (years < 5)
                return 1;
            else if (years < 10)
                return 2;

            // Make a correction of the interval
            return Math.Floor(years / 5);
        }

        /// <summary>
        /// This method returns the number of units
        /// between min and max.
        /// </summary>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <param name="type">Date type.</param>
        /// <returns>Number of units.</returns>
        private static int GetNumOfUnits(double min, double max, DateTimeIntervalType type)
        {
            double current = ChartHelper.GetIntervalSize(min, 1, type);
            return (int)Math.Round((max - min) / current);
        }

        /// <summary>
        /// This method checks if value type is date-time.
        /// </summary>
        /// <returns>Date-time type or Auto.</returns>
        internal ChartValueType GetDateTimeType()
        {
            ChartValueType dateType = ChartValueType.Auto;

            List<string> list;
            // Check if Value type is date from first series in the axis
            if (axisType == AxisName.X)
            {
                // Check X axes type
                list = ChartArea.GetXAxesSeries(AxisType.Primary, SubAxisName);
                if (list.Count == 0)
                {
                    return ChartValueType.Auto;
                }

                if (Common.DataManager.Series[list[0]].IsXValueDateTime())
                {
                    dateType = Common.DataManager.Series[list[0]].XValueType;
                }
            }
            else if (axisType == AxisName.X2)
            {
                // Check X2 axes type
                list = ChartArea.GetXAxesSeries(AxisType.Secondary, SubAxisName);
                if (list.Count == 0)
                {
                    return ChartValueType.Auto;
                }

                if (Common.DataManager.Series[list[0]].IsXValueDateTime())
                {
                    dateType = Common.DataManager.Series[list[0]].XValueType;
                }
            }
            else if (axisType == AxisName.Y)
            {
                // Check Y axes type
                list = ChartArea.GetYAxesSeries(AxisType.Primary, SubAxisName);
                if (list.Count == 0)
                {
                    return ChartValueType.Auto;
                }

                if (Common.DataManager.Series[list[0]].IsYValueDateTime())
                {
                    dateType = Common.DataManager.Series[list[0]].YValueType;
                }
            }
            else if (axisType == AxisName.Y2)
            {
                // Check Y2 axes type
                list = ChartArea.GetYAxesSeries(AxisType.Secondary, SubAxisName);
                if (list.Count == 0)
                {
                    return ChartValueType.Auto;
                }

                if (Common.DataManager.Series[list[0]].IsYValueDateTime())
                {
                    dateType = Common.DataManager.Series[list[0]].YValueType;
                }
            }

            return dateType;
        }

        /// <summary>
        /// This method removes "Auto", "min", "max" from crossing
        /// value and creates a double value.
        /// </summary>
        /// <returns>Crossing value</returns>
        private double GetCrossing()
        {
            if (Double.IsNaN(crossing))
            {
                if (Common.ChartTypeRegistry.GetChartType((string)ChartArea.ChartTypes[0]).ZeroCrossing)
                {
                    if (ViewMinimum > 0.0)
                    {
                        return ViewMinimum;
                    }
                    else if (ViewMaximum < 0.0)
                    {
                        return ViewMaximum;
                    }
                    else
                    {
                        return 0.0;
                    }
                }
                else
                {
                    return ViewMinimum;
                }
            }
            else if (crossing == Double.MaxValue)
            {
                return ViewMaximum;
            }
            else if (crossing == Double.MinValue)
            {
                return ViewMinimum;
            }

            return crossing;
        }

        /// <summary>
        /// Set auto minimum number. The minimum number
        /// which was sent to this function will be used to
        /// estimate a rounded minimum.
        /// </summary>
        /// <param name="min"> This value is a recommendation for the minimum value. </param>
        internal void SetAutoMinimum(double min)
        {
            // Set the minimum
            if (_autoMinimum)
            {
                minimum = min;
            }
        }

        /// <summary>
        /// Set auto maximum number. The maximum number
        /// which was sent to this function will be used to
        /// estimate a rounded maximum.
        /// </summary>
        /// <param name="max">This value is a recommendation for the maximum value.</param>
        internal void SetAutoMaximum(double max)
        {
            // Set the maximum
            if (_autoMaximum)
            {
                maximum = max;
            }
        }

        /// <summary>
        /// Find opposite axis of this axis.  What is opposite
        /// axis depend on first series in chart area and primary
        /// and secondary X and Y axes for the first series.
        /// </summary>
        /// <returns>Opposite axis</returns>
        internal Axis GetOppositeAxis()
        {
            // Oppoiste axis found
            if (oppositeAxis != null)
            {
                return oppositeAxis;
            }

            List<string> list;

            switch (axisType)
            {
                // X Axis
                case AxisName.X:
                    list = ChartArea.GetXAxesSeries(AxisType.Primary, SubAxisName);
                    // There aren't data series
                    if (list.Count == 0)
                        oppositeAxis = ChartArea.AxisY;
                    // Take opposite axis from the first series from chart area
                    else if (Common.DataManager.Series[list[0]].YAxisType == AxisType.Primary)
                        oppositeAxis = ChartArea.AxisY.GetSubAxis(Series.YSubAxisName);
                    else
                        oppositeAxis = ChartArea.AxisY2.GetSubAxis(Series.YSubAxisName);
                    break;
                // X2 Axis
                case AxisName.X2:
                    list = ChartArea.GetXAxesSeries(AxisType.Secondary, SubAxisName);
                    // There aren't data series
                    if (list.Count == 0)
                        oppositeAxis = ChartArea.AxisY2;
                    // Take opposite axis from the first series from chart area
                    else if (Common.DataManager.Series[list[0]].YAxisType == AxisType.Primary)
                        oppositeAxis = ChartArea.AxisY.GetSubAxis(Series.YSubAxisName);
                    else
                        oppositeAxis = ChartArea.AxisY2.GetSubAxis(Series.YSubAxisName);
                    break;
                // Y Axis
                case AxisName.Y:
                    list = ChartArea.GetYAxesSeries(AxisType.Primary, SubAxisName);
                    // There aren't data series
                    if (list.Count == 0)
                        oppositeAxis = ChartArea.AxisX;
                    // Take opposite axis from the first series from chart area
                    else if (Common.DataManager.Series[list[0]].XAxisType == AxisType.Primary)
                        oppositeAxis = ChartArea.AxisX.GetSubAxis(Series.XSubAxisName);
                    else
                        oppositeAxis = ChartArea.AxisX2.GetSubAxis(Series.XSubAxisName);
                    break;
                // Y2 Axis
                case AxisName.Y2:
                    list = ChartArea.GetYAxesSeries(AxisType.Secondary, SubAxisName);
                    // There aren't data series
                    if (list.Count == 0)
                        oppositeAxis = ChartArea.AxisX2;
                    // Take opposite axis from the first series from chart area
                    else if (Common.DataManager.Series[list[0]].XAxisType == AxisType.Primary)
                        oppositeAxis = ChartArea.AxisX.GetSubAxis(Series.XSubAxisName);
                    else
                        oppositeAxis = ChartArea.AxisX2.GetSubAxis(Series.XSubAxisName);
                    break;
            }
            return oppositeAxis;
        }

        /// <summary>
        /// This function converts Values from Axes to
        /// linear relative positions.
        /// </summary>
        /// <param name="axisValue">Value from axis.</param>
        /// <returns>Relative position.</returns>
        internal double GetLinearPosition(double axisValue)
        {
            bool circularArea = ChartArea != null && ChartArea.chartAreaIsCurcular;

            // Check if some value calculation is optimized
            if (!optimizedGetPosition)
            {
                paintViewMax = ViewMaximum;
                paintViewMin = ViewMinimum;
                paintRange = paintViewMax - paintViewMin;
                paintAreaPosition = PlotAreaPosition.ToSKRect();

                // Update position for circular chart area
                if (circularArea)
                {
                    paintAreaPosition.Right = paintAreaPosition.Left + paintAreaPosition.Width / 2.0f;
                    paintAreaPosition.Bottom = paintAreaPosition.Top + paintAreaPosition.Height / 2.0f;
                }

                paintAreaPositionBottom = paintAreaPosition.Top + paintAreaPosition.Height;
                paintAreaPositionRight = paintAreaPosition.Left + paintAreaPosition.Width;

                // The Chart area size
                if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                    paintChartAreaSize = paintAreaPosition.Width;
                else
                    paintChartAreaSize = paintAreaPosition.Height;

                valueMultiplier = 0.0;
                if (paintRange != 0)
                {
                    valueMultiplier = paintChartAreaSize / paintRange;
                }
            }

            // The Chart area pixel size
            double position = valueMultiplier * (axisValue - paintViewMin);

            // Check if axis scale segments are enabled
            if (scaleSegmentsUsed)
            {
                AxisScaleSegment scaleSegment = ScaleSegments.FindScaleSegmentForAxisValue(axisValue);
                if (scaleSegment != null)
                {
                    scaleSegment.GetScalePositionAndSize(paintChartAreaSize, out double segmentPosition, out double segmentSize);

                    // Make sure value do not exceed max possible
                    if (!ScaleSegments.AllowOutOfScaleValues)
                    {
                        if (axisValue > scaleSegment.ScaleMaximum)
                        {
                            axisValue = scaleSegment.ScaleMaximum;
                        }
                        else if (axisValue < scaleSegment.ScaleMinimum)
                        {
                            axisValue = scaleSegment.ScaleMinimum;
                        }
                    }

                    double segmentScaleRange = scaleSegment.ScaleMaximum - scaleSegment.ScaleMinimum;

                    position = (segmentSize / segmentScaleRange) * (axisValue - scaleSegment.ScaleMinimum);
                    position += segmentPosition;
                }
            }

            // Window position
            // (Do Not use .Right or .Bottom methods below) - rounding issue!
            if (isReversed)
            {
                if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                    position = paintAreaPositionRight - position;
                else
                    position = paintAreaPosition.Top + position;
            }
            else
            {
                if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                    position = paintAreaPosition.Left + position;
                else
                    position = paintAreaPositionBottom - position;
            }

            return position;
        }

        #endregion Axis scale methods

        #region Axis estimate axis methods

        /// <summary>
        /// This function recalculates minimum maximum and interval.
        /// The function uses current values for minimum and maximum to
        /// find rounding values. If the value from the data source for the
        /// maximum value is 376.5 this function will return 380. This function
        /// also set interval type for date
        /// </summary>
        internal void EstimateAxis()
        {
            double axisInterval;

            // Check if veiw size specified without scaleView position
            if (!double.IsNaN(ScaleView.Size) && double.IsNaN(ScaleView.Position))
            {
                ScaleView.Position = Minimum;
            }

            // Zooming Mode
            if (!double.IsNaN(_scaleView.Position) && !double.IsNaN(_scaleView.Size))
            {
                double viewMaximum = ViewMaximum;
                double viewMinimum = ViewMinimum;

                // IsLogarithmic axes
                if (_isLogarithmic)
                {
                    viewMaximum = Math.Pow(logarithmBase, viewMaximum);
                    viewMinimum = Math.Pow(logarithmBase, viewMinimum);
                }
                else
                {
                    // Add rounding and gap for maximum and minimum
                    EstimateAxis(ref minimum, ref maximum, _autoMaximum, _autoMinimum);
                }

                // Find Interval for Zoom
                axisInterval = EstimateAxis(ref viewMinimum, ref viewMaximum, true, true);
            }
            else // No Zooming mode
            {
                // Estimate axis shoud be always called for non logarithmic axis
                axisInterval = EstimateAxis(ref minimum, ref maximum, _autoMaximum, _autoMinimum);
            }

            // Set intervals for grids, tick marks and labels
            if (axisInterval <= 0.0)
            {
                throw (new InvalidOperationException(SR.ExceptionAxisScaleAutoIntervalInvalid));
            }
            else
            {
                // This code checks if all series in the chart area have “integer type”
                // for specified axes, which means int, uint, long and ulong and rounds interval.
#if SUBAXES
				if( ChartArea.SeriesIntegerType( this.axisType, ((Axis)this).SubAxisName ) )
#else // SUBAXES
                if (ChartArea.SeriesIntegerType(axisType, string.Empty))
#endif // SUBAXES
                {
                    axisInterval = Math.Round(axisInterval);
                    if (axisInterval == 0.0)
                    {
                        axisInterval = 1.0;
                    }

                    // Round Minimum to floor value if type is integer
                    minimum = Math.Floor(minimum);
                }

                SetInterval(axisInterval);
            }
        }

        /// <summary>
        /// This function recalculates minimum maximum and interval.
        /// The function uses current values for minimum and maximum to
        /// find rounding values. If the value from the data source for the
        /// maximum value is 376.5 this function will return 380. This function
        /// also set interval type for date
        /// </summary>
        /// <param name="minimumValue">Minimum</param>
        /// <param name="maximumValue">Maximum</param>
        /// <param name="autoMaximum">Maximum value is Auto</param>
        /// <param name="autoMinimum">Minimum value is Auto</param>
        /// <returns>Interval</returns>
        internal double EstimateAxis(ref double minimumValue, ref double maximumValue, bool autoMaximum, bool autoMinimum)
        {
            double axisInterval;

            // The axis minimum value is greater than the maximum value.
            if (maximumValue < minimumValue)
            {
                if (!Common.ChartPicture.SuppressExceptions)
                {
                    throw (new InvalidOperationException(SR.ExceptionAxisScaleMinimumValueIsGreaterThenMaximumDataPoint));
                }
                else
                {
                    // Max axis scale should be always bigger
                    double tempValue = maximumValue;
                    maximumValue = minimumValue;
                    minimumValue = tempValue;
                }
            }

            // Take Value type
            ChartValueType dateType = GetDateTimeType();

            // Axis type is logarithmic
            if (_isLogarithmic)
            {
                axisInterval = EstimateLogarithmicAxis(ref minimumValue, ref maximumValue, crossing, autoMaximum, autoMinimum);
            }

            // Axis type is date
            else if (dateType != ChartValueType.Auto)
            {
                axisInterval = EstimateDateAxis(ref minimumValue, ref maximumValue, autoMaximum, autoMinimum, dateType);
            }

            // Axis type is number
            else
            {
                axisInterval = EstimateNumberAxis(ref minimumValue, ref maximumValue, IsStartedFromZero, prefferedNumberofIntervals, autoMaximum, autoMinimum);
            }

            // Set intervals for grids, tick marks and labels
            if (axisInterval <= 0.0)
            {
                throw (new InvalidOperationException(SR.ExceptionAxisScaleAutoIntervalInvalid));
            }
            else
            {
                // Set interval for Grid lines Tick Marks and labels
                SetInterval(axisInterval);
            }

            return axisInterval;
        }

        /// <summary>
        /// This function recalculates minimum maximum and interval for
        /// logarithmic axis. The function uses current values for minimum and
        /// maximum to find new rounding values.
        /// </summary>
        /// <param name="minimumValue">Current Minimum value</param>
        /// <param name="maximumValue">Current Maximum value</param>
        /// <param name="crossingValue">Crossing value</param>
        /// <param name="autoMaximum">Maximum value is Auto</param>
        /// <param name="autoMinimum">Minimum value is Auto</param>
        /// <returns>Interval</returns>
		private double EstimateLogarithmicAxis(ref double minimumValue, ref double maximumValue, double crossingValue, bool autoMaximum, bool autoMinimum)
        {
            double axisInterval;

            if (!logarithmicConvertedToLinear)
            {
                // Remember values. Do not use POW function because of rounding.
                logarithmicMinimum = minimum;
                logarithmicMaximum = maximum;
            }

            // For log axis margin always turn on.
            margin = 100;

            // Supress zero and negative values with logarithmic axis exceptions
            if (Common != null && Common.Chart != null && Common.Chart.chartPicture.SuppressExceptions)
            {
                if (minimumValue <= 0.0)
                {
                    minimumValue = 1.0;
                }
                if (maximumValue <= 0.0)
                {
                    maximumValue = 1.0;
                }
                if (crossingValue <= 0.0 && crossingValue != Double.MinValue)
                {
                    crossingValue = 1.0;
                }
            }

            // The logarithmic axes can not show negative values.
            if (minimumValue <= 0.0 || maximumValue <= 0.0 || crossingValue <= 0.0)
            {
                if (minimumValue <= 0.0)
                    throw (new ArgumentOutOfRangeException(nameof(minimumValue), SR.ExceptionAxisScaleLogarithmicNegativeValues));
                if (maximumValue <= 0.0)
                    throw (new ArgumentOutOfRangeException(nameof(maximumValue), SR.ExceptionAxisScaleLogarithmicNegativeValues));
            }

            // Change crossing to linear scale
            _ = Math.Log(crossingValue, logarithmBase);

            // Change minimum and maximum to linear scale
            minimumValue = Math.Log(minimumValue, logarithmBase);
            maximumValue = Math.Log(maximumValue, logarithmBase);

            logarithmicConvertedToLinear = true;

            // Find interval - Make approximately 5 grid lines and labels.
            double diff = (maximumValue - minimumValue) / 5;

            // Make good interval for logarithmic scale
            axisInterval = Math.Floor(diff);
            if (axisInterval == 0) axisInterval = 1;

            if (autoMinimum && autoMaximum)
            {
                // The maximum and minimum rounding with interval
                RoundedValues(axisInterval, IsStartedFromZero, autoMaximum, autoMinimum, ref minimumValue, ref maximumValue);
            }

            // Do not allow min/max values more than a hundred
            if (ChartArea.hundredPercent)
            {
                if (autoMinimum && minimumValue < 0)
                    minimumValue = 0;

                if (autoMaximum && maximumValue > 2)
                    maximumValue = 2;
            }

            // Set interval for Grid lines Tick Marks and labels
            return axisInterval;
        }

        /// <summary>
        /// This function recalculates minimum maximum and interval for
        /// Date axis. The function uses current values for minimum and
        /// maximum to find new rounding values.
        /// </summary>
        /// <param name="minimumValue">Current Minimum value</param>
        /// <param name="maximumValue">Current Maximum value</param>
        /// <param name="autoMaximum">Maximum value is Auto</param>
        /// <param name="autoMinimum">Minimum value is Auto</param>
        /// <param name="valuesType">AxisName of date-time values.</param>
        /// <returns>Interval</returns>
        private double EstimateDateAxis(
            ref double minimumValue,
            ref double maximumValue,
            bool autoMaximum,
            bool autoMinimum,
            ChartValueType valuesType)
        {
            double axisInterval;

            double min = minimumValue;
            double max = maximumValue;

            // Find interval for this date type
            axisInterval = CalcInterval(min, max, true, out _internalIntervalType, valuesType);

            // For 3D Charts interval could be changed. After rotation
            // projection of axis could be very small.
            if (!double.IsNaN(interval3DCorrection) &&
                ChartArea.Area3DStyle.Enable3D &&
                !ChartArea.chartAreaIsCurcular)
            {
                axisInterval = Math.Floor(axisInterval / interval3DCorrection);

                interval3DCorrection = double.NaN;
            }

            // Find number of units between minimum and maximum
            int numberOfUnits = GetNumOfUnits(min, max, _internalIntervalType);

            // Make a gap between max point and axis for Y axes
            if (axisType == AxisName.Y || axisType == AxisName.Y2)
            {
                if (autoMinimum && minimumValue > ChartHelper.GetIntervalSize(min, axisInterval, _internalIntervalType))
                {
                    // Add gap to the minimum value from the series
                    // equal half of the interval
                    minimumValue += ChartHelper.GetIntervalSize(
                        min,
                        -(axisInterval / 2.0) * margin / 100,
                        _internalIntervalType,
                        null,
                        0.0,
                        DateTimeIntervalType.Number,
                        false,
                        false);

                    // Align minimum sacale value on the interval
                    minimumValue = ChartHelper.AlignIntervalStart(
                        minimumValue,
                        axisInterval * margin / 100,
                        _internalIntervalType);
                }

                // Increase maximum if not zero. Make a space between chart type
                // and the end of the chart area.
                if (autoMaximum && max > 0 && margin != 0.0)
                {
                    maximumValue = minimumValue + ChartHelper.GetIntervalSize(
                        minimumValue,
                        (Math.Floor(numberOfUnits / axisInterval / margin * 100) + 2) * axisInterval * margin / 100,
                        _internalIntervalType);
                }
            }

            InternalIntervalType = _internalIntervalType;

            // Set interval for Grid lines Tick Marks and labels
            return axisInterval;
        }

        /// <summary>
        /// This function recalculates minimum maximum and interval for
        /// number type axis. The function uses current values for minimum and
        /// maximum to find new rounding values.
        /// </summary>
        /// <param name="minimumValue">Current Minimum value</param>
        /// <param name="maximumValue">Current Maximum value</param>
        /// <param name="shouldStartFromZero">Should start from zero flag.</param>
        /// <param name="preferredNumberOfIntervals">Preferred number of intervals. Can be set to zero for dynamic mode.</param>
        /// <param name="autoMaximum">Maximum value is Auto</param>
        /// <param name="autoMinimum">Minimum value is Auto</param>
        /// <returns>Interval</returns>
        internal double EstimateNumberAxis(
            ref double minimumValue,
            ref double maximumValue,
            bool shouldStartFromZero,
            int preferredNumberOfIntervals,
            bool autoMaximum,
            bool autoMinimum)
        {
            double axisInterval;
            double min = minimumValue;
            double max = maximumValue;
            double diff;

            if (!roundedXValues && (axisType == AxisName.X || axisType == AxisName.X2))
            {
                diff = ChartArea.GetPointsInterval(false, 10);
                if (diff == 0 || (max - min) / diff > 20)
                {
                    diff = (max - min) / preferredNumberOfIntervals;
                }
            }
            else
            {
                diff = (max - min) / preferredNumberOfIntervals;
            }

            // For 3D Charts interval could be changed. After rotation
            // projection of axis could be very small.
            if (!double.IsNaN(interval3DCorrection) &&
                ChartArea.Area3DStyle.Enable3D &&
                !ChartArea.chartAreaIsCurcular)
            {
                diff /= interval3DCorrection;

                // Do not change minimum and maximum with 3D correction.
                if (max - min < diff)
                {
                    diff = max - min;
                }

                interval3DCorrection = double.NaN;

                if (diff != 0.0)
                {
                    diff = CalcInterval(diff);
                }
            }

            if (autoMaximum || autoMinimum)
            {
                if (diff == 0)
                {
                    // Can not find interval. Minimum and maximum are same
                    axisInterval = 0.2;
                }
                else
                {
                    axisInterval = CalcInterval(diff);
                }
            }
            else
            {
                axisInterval = diff;
            }

            // Case when minimum or maximum is set and interval is > maximum.
            // Reasons overflow exception.
            if (interval != 0 && interval > axisInterval && minimumValue + interval > maximumValue)
            {
                axisInterval = interval;
                if (autoMaximum)
                {
                    maximumValue = minimumValue + axisInterval;
                }

                if (autoMinimum)
                {
                    minimumValue = maximumValue - axisInterval;
                }
            }

            // The maximum and minimum rounding for Y Axes
            if (axisType == AxisName.Y || axisType == AxisName.Y2 || (roundedXValues && (axisType == AxisName.X || axisType == AxisName.X2)))
            {
                // Start from zero for the 100% chart types
                bool minIsZero = false;
                bool maxIsZero = false;
                if (ChartArea.hundredPercent)
                {
                    minIsZero = (minimumValue == 0.0);
                    maxIsZero = (maximumValue == 0.0);
                }

                // Round min/max values
                RoundedValues(axisInterval, shouldStartFromZero, autoMaximum, autoMinimum, ref minimumValue, ref maximumValue);

                // Do not allow min/max values more than a hundred
                if (ChartArea.hundredPercent)
                {
                    if (autoMinimum)
                    {
                        if (minimumValue < -100)
                            minimumValue = -100;
                        if (minIsZero)
                            minimumValue = 0;
                    }

                    if (autoMaximum)
                    {
                        if (maximumValue > 100)
                            maximumValue = 100;
                        if (maxIsZero)
                            maximumValue = 0;
                    }
                }
            }

            // Set interval for Grid lines Tick Marks and labels
            return axisInterval;
        }

        #endregion Axis estimate axis methods
    }
}