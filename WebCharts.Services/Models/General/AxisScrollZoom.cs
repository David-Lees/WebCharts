// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	AxisScaleView class represents a data scaleView, and is
//              exposed using the Axis.ScaleView property. A data scaleView is
//              a "scaleView" of data that has a start position (represented
//              by the Position property) and a size (represented by
//              the Size property).
//              Axis data scaleView is used in zooming and scrolling when
//              only part of the data must be visible. Views always
//              belong to an axis, and a scaleView can result from either
//              user interaction or by calling the Zoom or Scroll
//              methods. User interaction, accomplished using range
//              selection along an axis using the mouse, is possible
//              if the IsUserSelectionEnabled property of the chart area'
//              s cursor property is set to true. The end-user selects
//              a range by left-clicking the mouse and dragging the
//              mouse, and when the mouse button is released the
//              selected range is then displayed as a scaleView.
//

using System;
using System.Collections;

namespace WebCharts.Services
{
    #region Scrolling  enumerations

    /// <summary>
	/// Scrolling type enumeration.
	/// </summary>
    public enum ScrollType
    {
        /// <summary>
        /// Scrolls by substracting one small size.
        /// </summary>
        SmallDecrement,

        /// <summary>
        /// Scrolls by adding one small size.
        /// </summary>
        SmallIncrement,

        /// <summary>
        /// Scrolls by substracting one scaleView size.
        /// </summary>
        LargeDecrement,

        /// <summary>
        /// Scrolls by adding one scaleView size.
        /// </summary>
        LargeIncrement,

        /// <summary>
        /// Scrolls to the first scaleView.
        /// </summary>
        First,

        /// <summary>
        /// Scrolls to the last scaleView.
        /// </summary>
        Last
    }

    #endregion Scrolling  enumerations

    /// <summary>
    /// AxisScaleView class represents a scale view which allows to display
    /// only part of the available data.
    /// </summary>
    [
        SRDescription("DescriptionAttributeAxisDataView_AxisDataView"),
    ]
    public class AxisScaleView
    {
        #region Fields

        // Reference to the axis object
        internal Axis axis = null;

        // Axis data scaleView position
        private double _position = double.NaN;

        // Axis data scaleView size
        private double _size = double.NaN;

        // Axis data scaleView size units type
        private DateTimeIntervalType _sizeType = DateTimeIntervalType.Auto;

        // Axis data scaleView minimum scaleView/scrolling size units type
        private DateTimeIntervalType _minSizeType = DateTimeIntervalType.Auto;

        // Axis data scaleView scroll line size
        private double _smallScrollSize = double.NaN;

        // Axis data scaleView scroll line size units type
        private DateTimeIntervalType _smallScrollSizeType = DateTimeIntervalType.Auto;

        // Axis data scaleView scroll line minimum size
        private double _smallScrollMinSize = 1.0;

        // Axis data scaleView scroll line minimum size units type
        private DateTimeIntervalType _smallScrollMinSizeType = DateTimeIntervalType.Auto;

        // Axis data scaleView scroll line minimum size
        private double _currentSmallScrollSize = double.NaN;

        // Axis data scaleView scroll line minimum size units type
        private DateTimeIntervalType _currentSmallScrollSizeType = DateTimeIntervalType.Auto;

        // Storage for the saved data scaleView states (position/size/sizetype)
        internal ArrayList dataViewStates = null;

        // Ignore validation flag
        private bool _ignoreValidation = false;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public AxisScaleView()
        {
            axis = null;
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="axis">Data scaleView axis.</param>
        internal AxisScaleView(Axis axis)
        {
            this.axis = axis;
        }

        #endregion Constructor

        #region Axis data scaleView properties

        /// <summary>
        /// Gets or sets the position of the AxisScaleView.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_Position"),
        ]
        public double Position
        {
            get
            {
                // Axis scaleView is not supported in circular chrt areas
                if (axis != null && axis.ChartArea != null && axis.ChartArea.chartAreaIsCurcular)
                {
                    return Double.NaN;
                }
                return _position;
            }
            set
            {
                // Axis scaleView is not supported in circular chrt areas
                if (axis != null && axis.ChartArea != null && axis.ChartArea.chartAreaIsCurcular)
                {
                    return;
                }

                if (_position != value)
                {
                    // Set new position
                    _position = value;

                    // Align scaleView in connected areas
                    if (axis != null && axis.ChartArea != null && axis.Common != null && axis.Common.ChartPicture != null && !axis.ChartArea.alignmentInProcess)
                    {
                        AreaAlignmentOrientations orientation = (axis.axisType == AxisName.X || axis.axisType == AxisName.X2) ?
                            AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
                        axis.Common.ChartPicture.AlignChartAreasAxesView(axis.ChartArea, orientation);
                    }

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the AxisScaleView
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_Size"),
        ]
        public double Size
        {
            get
            {
                // Axis scaleView is not supported in circular chrt areas
                if (axis != null && axis.ChartArea != null && axis.ChartArea.chartAreaIsCurcular)
                {
                    return Double.NaN;
                }

                return _size;
            }
            set
            {
                // Axis scaleView is not supported in circular chrt areas
                if (axis != null && axis.ChartArea != null && axis.ChartArea.chartAreaIsCurcular)
                {
                    return;
                }

                if (_size != value)
                {
                    // Set size value
                    _size = value;

                    // Align scaleView in connected areas
                    if (axis != null && axis.ChartArea != null && axis.Common != null && axis.Common.ChartPicture != null && !axis.ChartArea.alignmentInProcess)
                    {
                        AreaAlignmentOrientations orientation = (axis.axisType == AxisName.X || axis.axisType == AxisName.X2) ?
                            AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
                        axis.Common.ChartPicture.AlignChartAreasAxesView(axis.ChartArea, orientation);
                    }
                    // Reset current scrolling line size
                    _currentSmallScrollSize = double.NaN;

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the unit of measurement of the Size property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_SizeType"),
        ]
        public DateTimeIntervalType SizeType
        {
            get
            {
                return _sizeType;
            }
            set
            {
                if (_sizeType != value)
                {
                    // Set size type
                    _sizeType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;

                    // Align scaleView in connected areas
                    if (axis != null && axis.ChartArea != null && axis.Common != null && axis.Common.ChartPicture != null && !axis.ChartArea.alignmentInProcess)
                    {
                        AreaAlignmentOrientations orientation = (axis.axisType == AxisName.X || axis.axisType == AxisName.X2) ?
                            AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
                        axis.Common.ChartPicture.AlignChartAreasAxesView(axis.ChartArea, orientation);
                    }

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if axis is zoomed-in.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_IsZoomed"),
        ]
        public bool IsZoomed
        {
            get
            {
                return (
                    !double.IsNaN(Size) &&
                    Size != 0.0 &&
                    !double.IsNaN(Position));
            }
        }

        /// <summary>
        /// Gets or sets the minimum size of the AxisScaleView.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_MinSize"),
        ]
        public double MinSize { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the unit of measurement of the MinSize property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_MinSizeType"),
        ]
        public DateTimeIntervalType MinSizeType
        {
            get
            {
                return _minSizeType;
            }
            set
            {
                _minSizeType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the zooming user interface is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_Zoomable")
        ]
        public bool Zoomable { get; set; } = true;

        /// <summary>
        /// Gets or sets the small scrolling size.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_SmallScrollSize"),
        ]
        public double SmallScrollSize
        {
            get
            {
                return _smallScrollSize;
            }
            set
            {
                if (_smallScrollSize != value)
                {
                    // Set size value
                    _smallScrollSize = value;

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the unit of measurement for the SmallScrollMinSize property
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_SmallScrollSizeType"),
        ]
        public DateTimeIntervalType SmallScrollSizeType
        {
            get
            {
                return _smallScrollSizeType;
            }
            set
            {
                if (_smallScrollSizeType != value)
                {
                    // Set size type
                    _smallScrollSizeType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum small scrolling size.
        /// Only used if the small scrolling size is not set.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_SmallScrollMinSize")
        ]
        public double SmallScrollMinSize
        {
            get
            {
                return _smallScrollMinSize;
            }
            set
            {
                if (_smallScrollMinSize != value)
                {
                    // Set size value
                    _smallScrollMinSize = value;

                    _currentSmallScrollSize = double.NaN;

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the unit of measurement for the SmallScrollMinSize property.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisDataView_SmallScrollMinSizeType"),
        ]
        public DateTimeIntervalType SmallScrollMinSizeType
        {
            get
            {
                return _smallScrollMinSizeType;
            }
            set
            {
                if (_smallScrollMinSizeType != value)
                {
                    // Set size type
                    _smallScrollMinSizeType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;

                    _currentSmallScrollSize = double.NaN;

                    // Validate chart
                    if (!_ignoreValidation && axis != null)
                    {
                        axis.Invalidate();
                    }
                }
            }
        }

        #endregion Axis data scaleView properties

        #region ScaleView position internal methods

        /// <summary>
        /// Call this method to get the minimum axis value of a data view.
		/// </summary>
		/// <returns>The minimum axis value for the data view.</returns>
        public double ViewMinimum
        {
            get
            {
                // If zooming is enabled
                if (!Double.IsNaN(Size))
                {
                    // If size set only use axis minimum for scaleView position
                    if (Double.IsNaN(Position))
                    {
                        Position = axis.Minimum;
                    }
                    // Check if scaleView position and size are set
                    else
                    {
                        // Calculate and add axis side margin
                        if (Position <= axis.minimum)
                        {
                            return Position;
                        }
                        else // Add a margin only if scaleView is inside data point scaleView
                        {
                            return Position - axis.marginView;
                        }
                    }
                }

                // Return axis scale minimum value if scaleView position is not set
                return axis.minimum;
            }
        }

        /// <summary>
        /// Maximum axis value of a data view.
        /// </summary>
        /// <returns>The maximum axis value for the data view.</returns>
        public double ViewMaximum
        {
            get
            {
                // If zooming is enabled
                if (!Double.IsNaN(Size))
                {
                    // If size set only use axis minimum for scaleView position
                    if (Double.IsNaN(Position))
                    {
                        Position = axis.Minimum;
                    }

                    // Check if scaleView position and size are set
                    else
                    {
                        // Get axis interval
                        double viewSize = ChartHelper.GetIntervalSize(Position, Size, SizeType);

                        // Calculate and add axis side margin
                        if (Position + viewSize >= axis.maximum)
                        {
                            return Position + viewSize;
                        }
                        else // Add a margin only if scaleView is inside data point scaleView
                        {
                            return Position + viewSize + axis.marginView;
                        }
                    }
                }

                // Return axis scale maximum value if scaleView position is not set
                return axis.maximum;
            }
        }

        #endregion ScaleView position internal methods

        #region Scrolling methods

        /// <summary>
        /// Call this method to scroll to a specified position along an axis.
        /// </summary>
        /// <param name="scrollType">Direction and size to scroll.</param>
        public void Scroll(ScrollType scrollType)
        {
            Scroll(scrollType, false);
        }

        /// <summary>
        /// Scrolls axis data scaleView from current position.
        /// </summary>
        /// <param name="scrollType">Direction and size to scroll.</param>
        /// <param name="fireChangeEvents">Fire scaleView position events from this method.</param>
        internal void Scroll(ScrollType scrollType, bool fireChangeEvents)
        {
            // Adjust current position depending on the scroll type
            double newPosition = _position;
            switch (scrollType)
            {
                case (ScrollType.SmallIncrement):
                    newPosition += ((axis.IsReversed) ? -1 : 1) * ChartHelper.GetIntervalSize(_position, GetScrollingLineSize(), GetScrollingLineSizeType());
                    break;

                case (ScrollType.SmallDecrement):
                    newPosition -= ((axis.IsReversed) ? -1 : 1) * ChartHelper.GetIntervalSize(_position, GetScrollingLineSize(), GetScrollingLineSizeType());
                    break;

                case (ScrollType.LargeIncrement):
                    newPosition += ((axis.IsReversed) ? -1 : 1) * ChartHelper.GetIntervalSize(_position, Size, SizeType);
                    break;

                case (ScrollType.LargeDecrement):
                    newPosition -= ((axis.IsReversed) ? -1 : 1) * ChartHelper.GetIntervalSize(_position, Size, SizeType);
                    break;

                case (ScrollType.First):
                    if (!axis.IsReversed)
                    {
                        newPosition = (axis.minimum + axis.marginView);
                    }
                    else
                    {
                        newPosition = (axis.maximum - axis.marginView);
                    }
                    break;

                case (ScrollType.Last):
                    {
                        double viewSize = ChartHelper.GetIntervalSize(newPosition, Size, SizeType);
                        if (!axis.IsReversed)
                        {
                            newPosition = (axis.maximum - axis.marginView - viewSize);
                        }
                        else
                        {
                            newPosition = (axis.minimum + axis.marginView + viewSize);
                        }
                        break;
                    }
            }

            // Scroll to the new position
            Scroll(newPosition, fireChangeEvents);
        }

        /// <summary>
		/// Call this method to scroll to a specified position along an axis.
		/// </summary>
		/// <param name="newPosition">New position.</param>
		public void Scroll(double newPosition)
        {
            Scroll(newPosition, false);
        }

        /// <summary>
        /// Call this method to scroll to a specified position along an axis.
        /// </summary>
        /// <param name="newPosition">New position.</param>
        public void Scroll(DateTime newPosition)
        {
            Scroll(newPosition.ToOADate(), false);
        }

        /// <summary>
        /// Internal helper method for scrolling into specified position.
        /// </summary>
        /// <param name="newPosition">New data scaleView position.</param>
        /// <param name="fireChangeEvents">Fire scaleView position events from this method.</param>
        internal void Scroll(double newPosition, bool fireChangeEvents)
        {
            // Get current scaleView size
            double viewSize = ChartHelper.GetIntervalSize(newPosition, Size, SizeType);

            // Validate new scaleView position
            if (newPosition < (axis.minimum + axis.marginView))
            {
                newPosition = (axis.minimum + axis.marginView);
            }
            else if (newPosition > (axis.maximum - axis.marginView - viewSize))
            {
                newPosition = (axis.maximum - axis.marginView - viewSize);
            }

            // Fire scaleView position changing events
            ViewEventArgs arguments = new(axis, newPosition, Size, SizeType);
            if (fireChangeEvents && GetChartObject() != null)
            {
                GetChartObject().OnAxisViewChanging(arguments);
                newPosition = arguments.NewPosition;
            }

            // Check if data scaleView position and size is different from current
            if (newPosition == Position)
            {
                return;
            }

            // Change scaleView position
            Position = newPosition;

            // Fire scaleView position changed events
            if (fireChangeEvents && GetChartObject() != null)
            {
                GetChartObject().OnAxisViewChanged(arguments);
            }
        }

        #endregion Scrolling methods

        #region Zooming and ZoomResetting methods

        /// <summary>
        /// Sets a new axis data view/position based on the start and end dates specified.
        /// </summary>
        /// <param name="viewPosition">New start position for the axis scale view.</param>
        /// <param name="viewSize">New size for the axis scale view.</param>
        /// <param name="viewSizeType">New unit of measurement of the size.</param>
        /// <param name="saveState">Indicates whether the current size/position needs to be saved.</param>
        public void Zoom(double viewPosition, double viewSize, DateTimeIntervalType viewSizeType, bool saveState)
        {
            Zoom(viewPosition, viewSize, viewSizeType, false, saveState);
        }

        /// <summary>
        /// Sets a new axis data view/position based on the specified start and end values.
        /// </summary>
        /// <param name="viewStart">New start position for the axis scale view.</param>
        /// <param name="viewEnd">New end position for the axis scale view.</param>
        public void Zoom(double viewStart, double viewEnd)
        {
            Zoom(viewStart, viewEnd - viewStart, DateTimeIntervalType.Number, false, false);
        }

        /// <summary>
        /// Sets a new axis data view/position based on the start and end dates specified.
        /// </summary>
        /// <param name="viewPosition">New start position for the axis scale view.</param>
        /// <param name="viewSize">New size for the axis scale view.</param>
        /// <param name="viewSizeType">New unit of measurement of the size.</param>
        public void Zoom(double viewPosition, double viewSize, DateTimeIntervalType viewSizeType)
        {
            Zoom(viewPosition, viewSize, viewSizeType, false, false);
        }

        /// <summary>
		/// Reset the specified number of zooming operations by restoring axis data view.
		/// </summary>
		/// <param name="numberOfViews">Number of zoom operations to reset. Zero for all.</param>
		public void ZoomReset(int numberOfViews)
        {
            LoadDataViewState(numberOfViews, false);
        }

        /// <summary>
        /// Reset one zooming operation by restoring axis data view.
        /// </summary>
        public void ZoomReset()
        {
            LoadDataViewState(1, false);
        }

        /// <summary>
        /// Reset several zooming operation by restoring data scaleView size/position.
        /// </summary>
        /// <param name="numberOfViews">How many scaleView zoom operations to reset. Zero for all.</param>
        /// <param name="fireChangeEvents">Fire scaleView position events from this method.</param>
        internal void ZoomReset(int numberOfViews, bool fireChangeEvents)
        {
            LoadDataViewState(numberOfViews, fireChangeEvents);
        }

        /// <summary>
		/// Internal helper zooming method.
		/// </summary>
		/// <param name="viewPosition">New data scaleView start posiion.</param>
		/// <param name="viewSize">New data scaleView size.</param>
		/// <param name="viewSizeType">New data scaleView size units type.</param>
		/// <param name="fireChangeEvents">Fire scaleView position events from this method.</param>
		/// <param name="saveState">Indicates that current scaleView size/position must be save, so it can be restored later.</param>
		/// <returns>True if zoom operation was made.</returns>
		internal bool Zoom(
            double viewPosition,
            double viewSize,
            DateTimeIntervalType viewSizeType,
            bool fireChangeEvents,
            bool saveState)
        {
            // Validate new scaleView position and size
            ValidateViewPositionSize(ref viewPosition, ref viewSize, ref viewSizeType);

            // Fire scaleView position/size changing events
            ViewEventArgs arguments = new(axis, viewPosition, viewSize, viewSizeType);
            if (fireChangeEvents && GetChartObject() != null)
            {
                GetChartObject().OnAxisViewChanging(arguments);
                viewPosition = arguments.NewPosition;
                viewSize = arguments.NewSize;
                viewSizeType = arguments.NewSizeType;
            }

            // Check if data scaleView position and size is different from current
            if (viewPosition == Position &&
                viewSize == Size &&
                viewSizeType == SizeType)
            {
                return false;
            }

            // Save current data scaleView state, so it can be restored
            if (saveState)
            {
                SaveDataViewState();
            }

            // Change scaleView position/size
            _ignoreValidation = true;
            Position = viewPosition;
            Size = viewSize;
            SizeType = viewSizeType;
            _ignoreValidation = false;

            // Reset current scrolling line size
            _currentSmallScrollSize = double.NaN;

            // Invalidate chart
            axis.Invalidate();

            // Fire scaleView position/size changed events
            if (fireChangeEvents && GetChartObject() != null)
            {
                GetChartObject().OnAxisViewChanged(arguments);
            }

            return true;
        }

        #endregion Zooming and ZoomResetting methods

        #region Data scaleView state saving/restoring methods

        /// <summary>
        /// Saves current data scaleView position/size/sizetype, so
        /// it can be restored later.
        /// </summary>
        /// <param name="numberOfViews">Number of time to reset zoom. Zero for all.</param>
        /// <param name="fireChangeEvents">Fire scaleView position events from this method.</param>
        private void LoadDataViewState(int numberOfViews, bool fireChangeEvents)
        {
            // Check parameters
            if (numberOfViews < 0)
            {
                throw (new ArgumentOutOfRangeException(nameof(numberOfViews), SR.ExceptionScrollBarZoomResetsNumberInvalid));
            }
            // Check if storage was created
            if (dataViewStates != null && dataViewStates.Count >= 3)
            {
                // Find starting index of restoring state
                int dataStartIndex = 0;
                if (numberOfViews > 0)
                {
                    dataStartIndex = dataViewStates.Count - numberOfViews * 3;
                    if (dataStartIndex < 0)
                    {
                        dataStartIndex = 0;
                    }
                }

                // Fire scaleView position/size changing events
                ViewEventArgs arguments = new(
                    axis,
                    (double)dataViewStates[dataStartIndex],
                    (double)dataViewStates[dataStartIndex + 1],
                    (DateTimeIntervalType)dataViewStates[dataStartIndex + 2]);
                if (fireChangeEvents && GetChartObject() != null)
                {
                    GetChartObject().OnAxisViewChanging(arguments);
                }

                // Restore data
                Position = arguments.NewPosition;
                Size = arguments.NewSize;
                SizeType = arguments.NewSizeType;

                // Fire scaleView position/size changed events
                if (fireChangeEvents && GetChartObject() != null)
                {
                    GetChartObject().OnAxisViewChanged(arguments);
                }

                // Clear data
                int itemsToRemove = numberOfViews * 3;
                if (itemsToRemove > (dataViewStates.Count - dataStartIndex))
                {
                    itemsToRemove = dataViewStates.Count - dataStartIndex;
                }
                dataViewStates.RemoveRange(dataStartIndex, itemsToRemove);

                // clean up the history state when the numberOfViews == 0 (reset all by docs)
                if (numberOfViews == 0)
                {
                    dataViewStates.Clear();
                }
                if (Double.IsNaN(Position) || Double.IsNaN(Size))
                {
                    Position = Double.NaN;
                    Size = Double.NaN;
                }
            }

            // Nothing to restore - just disable the data scaleView
            else
            {
                // Fire scaleView position/size changing events
                ViewEventArgs arguments = new(
                    axis,
                    double.NaN,
                    double.NaN,
                    DateTimeIntervalType.Auto);

                if (fireChangeEvents && GetChartObject() != null)
                {
                    GetChartObject().OnAxisViewChanging(arguments);
                }

                // Restore data
                Position = arguments.NewPosition;
                Size = arguments.NewSize;
                SizeType = arguments.NewSizeType;

                // Fire scaleView position/size changed events
                if (fireChangeEvents && GetChartObject() != null)
                {
                    GetChartObject().OnAxisViewChanged(arguments);
                }
            }
        }

        /// <summary>
        /// Saves current data scaleView position/size/sizetype, so
        /// it can be restored later.
        /// </summary>
        private void SaveDataViewState()
        {
            // Create storage array
            if (dataViewStates == null)
            {
                dataViewStates = new ArrayList();
            }

            // Save data scaleView state
            dataViewStates.Add(Position);
            dataViewStates.Add(Size);
            dataViewStates.Add(SizeType);
        }

        #endregion Data scaleView state saving/restoring methods

        #region Helper methods

        /// <summary>
        /// Initialize internal scrolling line size variables for later use.
        /// This size is used in to scroll chart one line up or down.
        /// </summary>
        private void GetCurrentViewSmallScrollSize()
        {
            //**************************************************************************
            //** Check if current scrolling line size was not already calculated
            //**************************************************************************
            if (double.IsNaN(_currentSmallScrollSize))
            {
                //**************************************************************************
                //** Calculate line size depending on the current scaleView size
                //**************************************************************************
                if (SizeType == DateTimeIntervalType.Auto || SizeType == DateTimeIntervalType.Number)
                {
                    // Set line size type
                    _currentSmallScrollSizeType = DateTimeIntervalType.Number;

                    // Devide scaleView by 20 to find the scrolling line size
                    double newSize = Size / 20.0;

                    // Make sure that current line size is even with minimum value
                    if (!double.IsNaN(SmallScrollMinSize) && SmallScrollMinSize != 0.0)
                    {
                        double rounder = (Math.Round(newSize / SmallScrollMinSize));
                        if (rounder < 0)
                        {
                            rounder = 1;
                        }
                        newSize = rounder * SmallScrollMinSize;
                    }

                    // Set new current line size
                    _currentSmallScrollSize = newSize;
                }
                else
                {
                    // Calculate line size for date/time
                    double viewEndPosition = Position + ChartHelper.GetIntervalSize(Position, Size, SizeType);
                    _currentSmallScrollSize = axis.CalcInterval(
                        Position,
                        viewEndPosition,
                        true,
                        out _currentSmallScrollSizeType,
                        ChartValueType.Auto);
                }

                //**************************************************************************
                //** Make sure calculated scroll line size is not smaller than the minimum
                //**************************************************************************
                if (!double.IsNaN(SmallScrollMinSize) && SmallScrollMinSize != 0.0)
                {
                    double newLineSize = ChartHelper.GetIntervalSize(Position, _currentSmallScrollSize, _currentSmallScrollSizeType);
                    double minLineSize = ChartHelper.GetIntervalSize(Position, SmallScrollMinSize, SmallScrollMinSizeType);
                    if (newLineSize < minLineSize)
                    {
                        _currentSmallScrollSize = SmallScrollMinSize;
                        _currentSmallScrollSizeType = SmallScrollMinSizeType;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the scroll line size.
        /// </summary>
        /// <returns>Scroll line size.</returns>
        internal double GetScrollingLineSize()
        {
            // Scroll line size/type is specificly set by user
            if (!double.IsNaN(SmallScrollSize))
            {
                return SmallScrollSize;
            }

            // Calcualte scroll line size depending on the current scaleView size
            GetCurrentViewSmallScrollSize();

            // Return line size
            return _currentSmallScrollSize;
        }

        /// <summary>
        /// Returns the scroll line size units type.
        /// </summary>
        /// <returns>Scroll line size units type.</returns>
        internal DateTimeIntervalType GetScrollingLineSizeType()
        {
            // Scroll line size/type is specificly set by user
            if (!double.IsNaN(SmallScrollSize))
            {
                return SmallScrollSizeType;
            }

            // Calcualte scroll line size depending on the current scaleView size
            GetCurrentViewSmallScrollSize();

            // Return line size units type
            return _currentSmallScrollSizeType;
        }

        /// <summary>
        /// Helper method, which validates the axis data scaleView position and size.
        /// Returns adjusted scaleView position and size.
        /// </summary>
        /// <param name="viewPosition">ScaleView position.</param>
        /// <param name="viewSize">ScaleView size.</param>
        /// <param name="viewSizeType">ScaleView size units type.</param>
        private void ValidateViewPositionSize(ref double viewPosition, ref double viewSize, ref DateTimeIntervalType viewSizeType)
        {
            //****************************************************************
            //** Check if new scaleView position is inside axis scale
            //** minimum/maximum without margin.
            //****************************************************************
            if (viewPosition < (axis.minimum + axis.marginView))
            {
                if (viewSizeType == DateTimeIntervalType.Auto || viewSizeType == DateTimeIntervalType.Number)
                {
                    viewSize -= (axis.minimum + axis.marginView) - viewPosition;
                }
                viewPosition = (axis.minimum + axis.marginView);
            }
            else if (viewPosition > (axis.maximum - axis.marginView))
            {
                if (viewSizeType == DateTimeIntervalType.Auto || viewSizeType == DateTimeIntervalType.Number)
                {
                    viewSize -= viewPosition - (axis.maximum - axis.marginView);
                }
                viewPosition = (axis.maximum - axis.marginView);
            }

            //****************************************************************
            //** Check if new scaleView size is not smaller than minimum size
            //** set by the user
            //****************************************************************
            double newViewSize = ChartHelper.GetIntervalSize(viewPosition, viewSize, viewSizeType);
            double minViewSize = ChartHelper.GetIntervalSize(viewPosition, 1, MinSizeType);
            if (!double.IsNaN(MinSize))
            {
                minViewSize = ChartHelper.GetIntervalSize(viewPosition, MinSize, MinSizeType);
                if (newViewSize < minViewSize)
                {
                    viewSize = (double.IsNaN(MinSize)) ? 1 : MinSize;
                    viewSizeType = MinSizeType;
                    newViewSize = ChartHelper.GetIntervalSize(viewPosition, viewSize, viewSizeType);
                }
            }

            //****************************************************************
            //** Check if new scaleView size is smaller than (0.000000001)
            //****************************************************************
            if (newViewSize < 0.000000001)
            {
                viewSize = 0.000000001;
                viewSizeType = DateTimeIntervalType.Number;
                newViewSize = ChartHelper.GetIntervalSize(viewPosition, viewSize, viewSizeType);
            }

            //****************************************************************
            //** Check if new scaleView end position (position + size) is inside
            //** axis scale minimum/maximum without margin.
            //****************************************************************
            while ((viewPosition + newViewSize) > (axis.maximum - axis.marginView))
            {
                double currentSize = viewSize;
                DateTimeIntervalType currentSizeType = viewSizeType;

                // Try to reduce the scaleView size
                if (newViewSize > minViewSize)
                {
                    // Try to adjust the scaleView size
                    if (viewSize > 1)
                    {
                        --viewSize;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Years)
                    {
                        viewSize = 11;
                        viewSizeType = DateTimeIntervalType.Months;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Months)
                    {
                        viewSize = 4;
                        viewSizeType = DateTimeIntervalType.Weeks;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Weeks)
                    {
                        viewSize = 6;
                        viewSizeType = DateTimeIntervalType.Days;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Days)
                    {
                        viewSize = 23;
                        viewSizeType = DateTimeIntervalType.Hours;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Hours)
                    {
                        viewSize = 59;
                        viewSizeType = DateTimeIntervalType.Minutes;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Minutes)
                    {
                        viewSize = 59;
                        viewSizeType = DateTimeIntervalType.Seconds;
                    }
                    else if (viewSizeType == DateTimeIntervalType.Seconds)
                    {
                        viewSize = 999;
                        viewSizeType = DateTimeIntervalType.Milliseconds;
                    }
                    else
                    {
                        viewPosition = (axis.maximum - axis.marginView) - minViewSize;
                        break;
                    }

                    // Double check that scaleView size is not smaller than min size
                    newViewSize = ChartHelper.GetIntervalSize(viewPosition, viewSize, viewSizeType);
                    if (newViewSize < minViewSize)
                    {
                        // Can't adjust size no more (restore prev. value)
                        viewSize = currentSize;
                        viewSizeType = currentSizeType;

                        // Adjust the start position
                        viewPosition = (axis.maximum - axis.marginView) - minViewSize;
                        break;
                    }
                }
                else
                {
                    // Adjust the start position
                    viewPosition = (axis.maximum - axis.marginView) - newViewSize;
                    break;
                }
            }
        }

        /// <summary>
		/// Helper function which returns a reference to the chart object.
		/// </summary>
		/// <returns>Chart object reference.</returns>
		internal ChartService GetChartObject()
        {
            if (axis != null && axis.Common != null)
            {
                return axis.Common.Chart;
            }

            return null;
        }

        #endregion Helper methods
    }

    /// <summary>
    /// This class is used as a parameter object in the AxisViewChanged and AxisViewChanging events of the root Chart object.
    /// </summary>
    public class ViewEventArgs : EventArgs
    {
        #region Private fields

        // Private fields for properties values storage
        private readonly Axis _axis = null;

        private DateTimeIntervalType _newSizeType = DateTimeIntervalType.Auto;

        #endregion Private fields

        #region Constructors

        /// <summary>
        /// ViewEventArgs constructor.
        /// </summary>
        /// <param name="axis">Axis of the scale view.</param>
        /// <param name="newPosition">New scale view start position.</param>
        public ViewEventArgs(Axis axis, double newPosition)
        {
            _axis = axis;
            NewPosition = newPosition;
        }

        /// <summary>
        /// ViewEventArgs constructor.
        /// </summary>
        /// <param name="axis">Axis of the scale view.</param>
        /// <param name="newPosition">New scale view start position.</param>
        /// <param name="newSize">New scale view size.</param>
        /// <param name="newSizeType">New unit of measurement of the size.</param>
        public ViewEventArgs(Axis axis, double newPosition, double newSize, DateTimeIntervalType newSizeType)
        {
            _axis = axis;
            NewPosition = newPosition;
            NewSize = newSize;
            _newSizeType = newSizeType;
        }

        #endregion Constructors

        #region Properties

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
        /// ChartArea of the event.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartArea"),
        ]
        public ChartArea ChartArea
        {
            get
            {
                return _axis.ChartArea;
            }
        }

        /// <summary>
        /// New scale view start position.
        /// </summary>
        [
        SRDescription("DescriptionAttributeViewEventArgs_NewPosition"),
        ]
        public double NewPosition { get; set; }

        /// <summary>
        /// New scale view size.
        /// </summary>
        [
        SRDescription("DescriptionAttributeViewEventArgs_NewSize"),
        ]
        public double NewSize { get; set; } = double.NaN;

        /// <summary>
        /// New unit of measurement of the scale view.
        /// </summary>
        [
        SRDescription("DescriptionAttributeViewEventArgs_NewSizeType"),
        ]
        public DateTimeIntervalType NewSizeType
        {
            get
            {
                return _newSizeType;
            }
            set
            {
                _newSizeType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
            }
        }

        #endregion Properties
    }
}