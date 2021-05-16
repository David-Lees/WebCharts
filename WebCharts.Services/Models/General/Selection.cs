// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	This file contains methods used for Win Form selection
//


using SkiaSharp;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.General
{

    #region Enumerations


    // Plase keep the folowing enumaration in chart layering order - ex. ChartArea is under DataPoint 
    /// <summary>
    /// An enumeration of types of Chart Element.
    /// </summary>
    public enum ChartElementType
    {
        /// <summary>
        /// No chart element.
        /// </summary>
        Nothing,

        /// <summary>
        /// The title of a chart.
        /// </summary>
        Title,

        /// <summary>
        /// Plotting area (chart area excluding axes, labels, etc.).  
        /// Also excludes the regions that data points may occupy.
        /// </summary>
        PlottingArea,

        /// <summary>
        /// An Axis object.
        /// </summary>
        Axis,

        /// <summary>
        /// Any major or minor tick mark.
        /// </summary>
        TickMarks,

        /// <summary>
        /// Any major or minor grid line (both vertical or horizontal).
        /// </summary>
        Gridlines,

        /// <summary>
        /// A StripLine object.
        /// </summary>
        StripLines,

        /// <summary>
        /// Axis label Image.
        /// </summary>
        AxisLabelImage,

        /// <summary>
        /// Axis labels
        /// </summary>
        AxisLabels,

        /// <summary>
        /// Axis title
        /// </summary>
        AxisTitle,

        /// <summary>
        /// A scrollbar tracking thumb.
        /// </summary>
        ScrollBarThumbTracker,

        /// <summary>
        /// A scrollbar small decrement button.  A "down arrow" 
        /// button for a vertical scrollbar, or a "left arrow" 
        /// button for a horizontal scroll bar.
        /// </summary>
        ScrollBarSmallDecrement,

        /// <summary>
        /// A scrollbar small increment button.  An "up arrow" 
        /// button for a vertical scrollbar, or a "right arrow" 
        /// button for a horizontal scroll bar.
        /// </summary>
        ScrollBarSmallIncrement,

        /// <summary>
        /// The background of a scrollbar that will result in 
        /// a large decrement in the scale view size when clicked.  
        /// This is the background below the thumb for 
        /// a vertical scrollbar, and to the left of 
        /// the thumb for a horizontal scrollbar.
        /// </summary>
        ScrollBarLargeDecrement,

        /// <summary>
        /// The background of a scrollbar that will result in 
        /// a large increment in the scale view size when clicked.  
        /// This is the background above the thumb for 
        /// a vertical scrollbar, and to the right of 
        /// the thumb for a horizontal scrollbar.
        /// </summary>
        ScrollBarLargeIncrement,

        /// <summary>
        /// The zoom reset button of a scrollbar.
        /// </summary>
        ScrollBarZoomReset,

        /// <summary>
        /// A DataPoint object.
        /// </summary>
        DataPoint,

        /// <summary>
        /// Series data point label.
        /// </summary>
        DataPointLabel,

        /// <summary>
        /// The area inside a Legend object.  Does not include 
        /// the space occupied by legend items.
        /// </summary>
        LegendArea,

        /// <summary>
        /// Legend title.
        /// </summary>
        LegendTitle,

        /// <summary>
        /// Legend header.
        /// </summary>
        LegendHeader,

        /// <summary>
        /// A LegendItem object.
        /// </summary>
        LegendItem,


        /// <summary>
        /// Chart annotation object.
        /// </summary>
        Annotation,


    }

    /// <summary>
    /// Enumeration (Flag) used for processing chart types.
    /// </summary>
    [Flags]
    internal enum ProcessMode
    {
        /// <summary>
        /// Paint mode
        /// </summary>
        Paint = 1,

        /// <summary>
        /// Selection mode. Collection of hot regions has to be created.
        /// </summary>
        HotRegions = 2,

        /// <summary>
        /// Used for image maps
        /// </summary>
        ImageMaps = 4
    }

    #endregion

    /// <summary>
    /// This class presents item in
    /// the collection of hot regions.
    /// </summary>
    internal class HotRegion : IDisposable
    {
        #region Fields

        // Private data members, which store properties values
        private SKPath _path = null;
        private bool _relativeCoordinates = true;
        private SKRect _boundingRectangle = SKRect.Empty;
        private object _selectedObject = null;
        private int _pointIndex = -1;
        private string _seriesName = "";
        private ChartElementType _type = ChartElementType.Nothing;


        private object _selectedSubObject = null;


        #endregion // Fields

        #region Properties

        /// <summary>
        /// Region is Graphics path
        /// </summary>
        internal SKPath Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }

        /// <summary>
        /// Relative coordinates are used 
        /// to define region
        /// </summary>
        internal bool RelativeCoordinates
        {
            get
            {
                return _relativeCoordinates;
            }
            set
            {
                _relativeCoordinates = value;
            }
        }

        /// <summary>
        /// Bounding Rectangle of an shape
        /// </summary>
        internal SKRect BoundingRectangle
        {
            get
            {
                return _boundingRectangle;
            }
            set
            {
                _boundingRectangle = value;
            }
        }

        /// <summary>
        /// Object which is presented with this region
        /// </summary>
        internal object SelectedObject
        {
            get
            {
                return _selectedObject;
            }
            set
            {
                _selectedObject = value;
            }
        }



        /// <summary>
        /// Sub-Object which is presented with this region
        /// </summary>
        internal object SelectedSubObject
        {
            get
            {
                return _selectedSubObject;
            }
            set
            {
                _selectedSubObject = value;
            }
        }



        /// <summary>
        /// Index of the data point which is presented with this region
        /// </summary>
        internal int PointIndex
        {
            get
            {
                return _pointIndex;
            }
            set
            {
                _pointIndex = value;
            }
        }

        /// <summary>
        /// Name of the series which is presented with the region
        /// </summary>
        internal string SeriesName
        {
            get
            {
                return _seriesName;
            }
            set
            {
                _seriesName = value;
            }
        }

        /// <summary>
        /// Chart Element AxisName
        /// </summary>
        internal ChartElementType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        #endregion // Properties

        #region IDisposable members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_path != null)
                {
                    _path.Dispose();
                    _path = null;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            string objectType = SelectedObject != null ? SelectedObject.ToString() : "null";
            if (SelectedObject == null && !string.IsNullOrEmpty(SeriesName))
            {
                objectType = SeriesName;
            }
            return string.Format(CultureInfo.CurrentCulture, "{0} of {1}", Type, objectType);
        }

        #endregion //Methods
    }

    /// <summary>
    /// This class is used to fill and 
    /// manage collection with Hot Regions
    /// </summary>
    internal class HotRegionsList : IDisposable
    {
        #region Fields

        /// <summary>
        /// Process chart mode Flag
        /// </summary>
        private ProcessMode _processChartMode = ProcessMode.Paint;

        /// <summary>
        /// Collection with Hor Region Elements
        /// </summary>
        private readonly ArrayList _regionList = new();

        /// <summary>
        /// Reference to the common elements object
        /// </summary>
        private readonly CommonElements _common = null;

        /// <summary>
		/// True if hit test function is called
		/// </summary>
		internal bool hitTestCalled = false;

        #endregion // Fields

        #region Properties

        /// <summary>
		/// Flag used for processing chart types. It could 
		/// be Paint, HotRegion or both mode.
		/// </summary>
		internal ProcessMode ProcessChartMode
        {
            get
            {
                return _processChartMode;
            }
            set
            {
                _processChartMode = value;
                if (_common != null)
                {
                    _common.processModePaint =
                        (_processChartMode & ProcessMode.Paint) == ProcessMode.Paint;
                    _common.processModeRegions =
                        (_processChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions ||
                        (_processChartMode & ProcessMode.ImageMaps) == ProcessMode.ImageMaps;
                }
            }
        }

        /// <summary>
        /// Collection with Hor Region Elements
        /// </summary>
        internal ArrayList List
        {
            get
            {
                return _regionList;
            }
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="common">Reference to the CommonElements</param>
        internal HotRegionsList(CommonElements common)
        {
            _common = common;
        }

        /// <summary>
        /// Add hot region to the collection.
        /// </summary>
        /// <param name="rectSize">Rectangle which presents an Hot Region</param>
        /// <param name="point">Data Point</param>
        /// <param name="seriesName">Data Series</param>
        /// <param name="pointIndex">Index of an Data Point in the series</param>
        public void AddHotRegion(
            SKRect rectSize,
            DataPoint point,
            string seriesName,
            int pointIndex
            )
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {
                HotRegion region = new();

                region.BoundingRectangle = rectSize;
                region.SeriesName = seriesName;
                region.PointIndex = pointIndex;
                region.Type = ChartElementType.DataPoint;
                region.RelativeCoordinates = true;



                // Use index of the original data point
                if (point != null && point.IsCustomPropertySet("OriginalPointIndex"))
                {
                    region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);
                }

                _regionList.Add(region);
            }
        }


        /// <summary>
        /// Adds the hot region.
        /// </summary>
        /// <param name="path">Bounding SKPath.</param>
        /// <param name="relativePath">if set to <c>true</c> the is relative path.</param>
        /// <param name="graph">Chart Graphics Object</param>
        /// <param name="point">Selected data point</param>
        /// <param name="seriesName">Name of the series.</param>
        /// <param name="pointIndex">Index of the point.</param>
        internal void AddHotRegion(
            SKPath path,
            bool relativePath,
            ChartGraphics graph,
            DataPoint point,
            string seriesName,
            int pointIndex
            )
        {
            if (path == null)
            {
                return;
            }

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {

                HotRegion region = new();

                region.SeriesName = seriesName;
                region.PointIndex = pointIndex;
                region.Type = ChartElementType.DataPoint;
                region.Path = path;
                path.GetBounds(out var b);
                region.BoundingRectangle = b;
                region.RelativeCoordinates = relativePath;



                // Use index of the original data point
                if (point != null && point.IsCustomPropertySet("OriginalPointIndex"))
                {
                    region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);
                }

                _regionList.Add(region);

            }
        }

        /// <summary>
        /// Adds the hot region.
        /// </summary>
        /// <param name="insertIndex">Position where to insert element. Used for image maps only</param>
        /// <param name="path">Bounding SKPath.</param>
        /// <param name="relativePath">if set to <c>true</c> the is relative path.</param>
        /// <param name="graph">Chart Graphics Object</param>
        /// <param name="point">Selected data point</param>
        /// <param name="seriesName">Name of the series.</param>
        /// <param name="pointIndex">Index of the point.</param>
       internal void AddHotRegion(
            int insertIndex,
            SKPath path,
            bool relativePath,
            ChartGraphics graph,
            DataPoint point,
            string seriesName,
            int pointIndex
            )
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {

                HotRegion region = new();

                region.SeriesName = seriesName;
                region.PointIndex = pointIndex;
                region.Type = ChartElementType.DataPoint;
                region.Path = path;
                path.GetBounds(out var b);
                region.BoundingRectangle = b;
                region.RelativeCoordinates = relativePath;



                // Use index of the original data point
                if (point != null && point.IsCustomPropertySet("OriginalPointIndex"))
                {
                    region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);
                }



                _regionList.Add(region);

            }
        }

        /// <summary>
        /// Add hot region to the collection.
        /// </summary>
        /// <param name="path">Graphics path which presents hot region</param>
        /// <param name="relativePath">Graphics path uses relative or absolute coordinates</param>
        /// <param name="coord">Coordinates which defines polygon (Graphics Path). Used for image maps</param>
        /// <param name="point">Selected data point</param>
        /// <param name="seriesName">Data Series</param>
        /// <param name="pointIndex">Index of an Data Point in the series</param>
        internal void AddHotRegion(SKPath path, bool relativePath, float[] coord, DataPoint point, string seriesName, int pointIndex)
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {

                HotRegion region = new();

                region.SeriesName = seriesName;
                region.PointIndex = pointIndex;
                region.Type = ChartElementType.DataPoint;
                region.Path = path;
                path.GetBounds(out var b);
                region.BoundingRectangle = b;
                region.RelativeCoordinates = relativePath;



                // Use index of the original data point
                if (point != null && point.IsCustomPropertySet("OriginalPointIndex"))
                {
                    region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);
                }



                _regionList.Add(region);

            }

        }

        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="insertIndex">Position where to insert element. Used for image maps only</param>
        /// <param name="graph">Chart Graphics Object</param>
        /// <param name="x">x coordinate.</param>
        /// <param name="y">y coordinate.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="point">Selected data point</param>
        /// <param name="seriesName">Data Series</param>
        /// <param name="pointIndex">Index of an Data Point in the series</param>
        internal void AddHotRegion(int insertIndex, ChartGraphics graph, float x, float y, float radius, DataPoint point, string seriesName, int pointIndex)
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {
                HotRegion region = new();

                SKPoint circleCenter = graph.GetAbsolutePoint(new SKPoint(x, y));
                SKSize circleRadius = graph.GetAbsoluteSize(new SKSize(radius, radius));

                SKPath path = new();
                path.AddOval(new SKRect(
                    circleCenter.X - circleRadius.Width,
                    circleCenter.Y - circleRadius.Width,
                    circleCenter.X + circleRadius.Width,
                    circleCenter.Y + circleRadius.Width
                    ));
                region.BoundingRectangle = path.GetBounds();
                region.SeriesName = seriesName;
                region.Type = ChartElementType.DataPoint;
                region.PointIndex = pointIndex;
                region.Path = path;
                region.RelativeCoordinates = false;



                // Use index of the original data point
                if (point != null && point.IsCustomPropertySet("OriginalPointIndex"))
                {
                    region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);
                }

                _regionList.Add(region);
            }
        }


        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="rectArea">Hot Region rectangle</param>
        /// <param name="toolTip">Tool Tip Text</param>
        /// <param name="hRef">HRef string</param>
        /// <param name="mapAreaAttributes">Map area Attribute string</param>
        /// <param name="postBackValue">The post back value associated with this item</param>
        /// <param name="selectedObject">Object which present hot region</param>
        /// <param name="type">AxisName of the object which present hot region</param>
        /// <param name="series">Selected series</param>
        internal void AddHotRegion(SKRect rectArea, string toolTip, string hRef, string mapAreaAttributes, string postBackValue, object selectedObject, ChartElementType type, string series)
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {
                HotRegion region = new();

                region.BoundingRectangle = rectArea;
                region.RelativeCoordinates = true;
                region.Type = type;
                region.SelectedObject = selectedObject;
                if (!string.IsNullOrEmpty(series))
                {
                    region.SeriesName = series;
                }
                _regionList.Add(region);
            }
        }



        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="rectArea">Hot Region rectangle</param>
        /// <param name="toolTip">Tool Tip Text</param>
        /// <param name="hRef">HRef string</param>
        /// <param name="mapAreaAttributes">Map area Attribute string</param>
        /// <param name="postBackValue">The post back value associated with this item</param>
        /// <param name="selectedObject">Object which present hot region</param>
        /// <param name="selectedSubObject">Sub-Object which present hot region</param>
        /// <param name="type">AxisName of the object which present hot region</param>
        /// <param name="series">Selected series</param>
        internal void AddHotRegion(
            SKRect rectArea,
            string toolTip,
            string hRef,
            string mapAreaAttributes,
            string postBackValue,
            object selectedObject,
            object selectedSubObject,
            ChartElementType type,
            string series)
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {
                HotRegion region = new();

                region.BoundingRectangle = rectArea;
                region.RelativeCoordinates = true;
                region.Type = type;
                region.SelectedObject = selectedObject;
                region.SelectedSubObject = selectedSubObject;
                if (!string.IsNullOrEmpty(series))
                {
                    region.SeriesName = series;
                }
                _regionList.Add(region);
            }
        }

        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="graph">Chart Graphics Object</param>
        /// <param name="path">Graphics path</param>
        /// <param name="relativePath">Used relative coordinates for graphics path.</param>
        /// <param name="toolTip">Tool Tip Text</param>
        /// <param name="hRef">HRef string</param>
        /// <param name="mapAreaAttributes">Map area Attribute string</param>
        /// <param name="postBackValue">The post back value associated with this item</param>
        /// <param name="selectedObject">Object which present hot region</param>
        /// <param name="type">AxisName of the object which present hot region</param>
         internal void AddHotRegion(ChartGraphics graph, SKPath path, bool relativePath, string toolTip, string hRef, string mapAreaAttributes, string postBackValue, object selectedObject, ChartElementType type)
        {

            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {

                HotRegion region = new();

                region.Type = type;
                region.Path = path;
                region.SelectedObject = selectedObject;
                region.BoundingRectangle = path.GetBounds();
                region.RelativeCoordinates = relativePath;

                _regionList.Add(region);

            }
        }

        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="rectArea">Hot Region rectangle</param>
        /// <param name="selectedObject">Object which present hot region</param>
        /// <param name="type">AxisName of the object which present hot region</param>
        /// <param name="relativeCoordinates">Coordinates for rectangle are relative</param>
		internal void AddHotRegion(SKRect rectArea, object selectedObject, ChartElementType type, bool relativeCoordinates)
        {
            AddHotRegion(rectArea, selectedObject, type, relativeCoordinates, false);
        }

        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="rectArea">Hot Region rectangle</param>
        /// <param name="selectedObject">Object which present hot region</param>
        /// <param name="type">AxisName of the object which present hot region</param>
        /// <param name="relativeCoordinates">Coordinates for rectangle are relative</param>
        /// <param name="insertAtBeginning">Insert the hot region at the beginning of the collection</param>
		internal void AddHotRegion(SKRect rectArea, object selectedObject, ChartElementType type, bool relativeCoordinates, bool insertAtBeginning)
        {
            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {
                HotRegion region = new();

                region.BoundingRectangle = rectArea;
                region.RelativeCoordinates = relativeCoordinates;
                region.Type = type;
                region.SelectedObject = selectedObject;

                if (insertAtBeginning)
                {
                    _regionList.Insert(_regionList.Count - 1, region);
                }
                else
                {
                    _regionList.Add(region);
                }
            }
        }

        /// <summary>
        /// Add Hot region to the collection.
        /// </summary>
        /// <param name="path">Graphics path</param>
        /// <param name="relativePath">Used relative coordinates for graphics path.</param>
        /// <param name="type">Type of the object which present hot region</param>
        /// <param name="selectedObject">Object which present hot region</param>
        internal void AddHotRegion(
            SKPath path,
            bool relativePath,
            ChartElementType type,
            object selectedObject
            )
        {
            if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
            {

                HotRegion region = new();

                region.SelectedObject = selectedObject;
                region.Type = type;
                region.Path = path;
                region.BoundingRectangle = path.GetBounds();
                region.RelativeCoordinates = relativePath;

                _regionList.Add(region);

            }
        }

        /// <summary>
        /// This method search for position in Map Areas which is the first 
        /// position after Custom areas.
        /// </summary>
        /// <returns>Insert Index</returns>
		internal static int FindInsertIndex()
        {
            int insertIndex = 0;

            return insertIndex;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            foreach (HotRegion hotRegion in _regionList)
                hotRegion.Dispose();

            _regionList.Clear();
        }

        #endregion // Methods

        #region IDisposable members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _regionList != null)
            {
                foreach (HotRegion hotRegion in _regionList)
                    hotRegion.Dispose();

                _regionList.Clear();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    /// <summary>
    /// The HitTestResult class contains the result of the hit test function.
    /// </summary>
    public class HitTestResult
    {
        #region Fields

        // Private members

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the data series object.
        /// </summary>
        public Series Series { get; set; } = null;

        /// <summary>
        /// Gets or sets the data point index.
        /// </summary>
        public int PointIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the chart area object.
        /// </summary>
        public ChartArea ChartArea { get; set; } = null;

        /// <summary>
        /// Gets or sets the axis object.
        /// </summary>
        public Axis Axis { get; set; } = null;

        /// <summary>
        /// Gets or sets the chart element type.
        /// </summary>
        public ChartElementType ChartElementType { get; set; } = ChartElementType.Nothing;

        /// <summary>
        ///  Gets or sets the selected object.
        /// </summary>
		public object Object { get; set; } = null;

        /// <summary>
        ///  Gets or sets the selected sub object.
        /// </summary>
        public object SubObject { get; set; } = null;

        #endregion
    }


    /// <summary>
    /// This class represents an array of marker points and 
    /// the outline path used for visual object selection in the chart.
    /// </summary>
    /// <remarks>
    /// <see cref="OutlinePath"/> may be null for complex objects or objects with two points or fewer.
    /// </remarks>
    public class ChartElementOutline : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartElementOutline"/> class.
        /// </summary>
        internal ChartElementOutline()
        {
            Markers = new ReadOnlyCollection<SKPoint>(Array.Empty<SKPoint>());
        }

        /// <summary>
        /// Gets the markers.  
        /// </summary>
        /// <value>The markers.</value>
        public ReadOnlyCollection<SKPoint> Markers { get; internal set; }

        /// <summary>
        /// Gets or sets the outline path. The result could be null for complex objects and objects with two points or fewer.
        /// </summary>
        /// <value>The outline path.</value>
        public SKPath OutlinePath { get; internal set; }


        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (OutlinePath != null)
                {
                    OutlinePath.Dispose();
                    OutlinePath = null;
                }
                Markers = null;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// The ToolTipEventArgs class stores the tool tips event arguments.
    /// </summary>
    public class ToolTipEventArgs : EventArgs
    {
        #region Private fields

        // Private fields for properties values storage
        private int x = 0;
        private int y = 0;
        private string text = "";
        private HitTestResult result = new HitTestResult();

        #endregion

        #region Constructors

        /// <summary>
        /// ToolTipEventArgs constructor.  Creates ToolTip event arguments.
        /// </summary>
        /// <param name="x">X-coordinate of mouse.</param>
        /// <param name="y">Y-coordinate of mouse.</param>
        /// <param name="text">Tooltip text.</param>
        /// <param name="result">Hit test result object.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "X and Y are cartesian coordinates and well understood")]
        public ToolTipEventArgs(int x, int y, string text, HitTestResult result)
        {
            this.x = x;
            this.y = y;
            this.text = text;
            this.result = result;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the x-coordinate of the mouse.
        /// </summary>
        [
        SRDescription("DescriptionAttributeToolTipEventArgs_X"),
        ]
        public int X
        {
            get
            {
                return x;
            }
        }

        /// <summary>
        /// Gets the result of the hit test.
        /// </summary>
        [
        SRDescription("DescriptionAttributeToolTipEventArgs_HitTestResult"),
        ]
        public HitTestResult HitTestResult
        {
            get
            {
                return result;
            }
        }

        /// <summary>
        /// Gets the y-coordinate of the mouse.
        /// </summary>
        [
        SRDescription("DescriptionAttributeToolTipEventArgs_Y"),
        ]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        public int Y
        {
            get
            {
                return y;
            }
        }

        /// <summary>
        /// Gets the text of the tooltip.
        /// </summary>
        [
        SRDescription("DescriptionAttributeToolTipEventArgs_Text"),
        ]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        #endregion
    }
}
