using System;
using WebCharts.Services;

namespace WebCharts.Services
{
    /// <summary>
    /// Event arguments of Chart paint event.
    /// </summary>
    public class ChartPaintEventArgs : EventArgs
    {
        #region Fields

        // Private fields
        private readonly object _chartElement = null;
        private readonly ChartGraphics _chartGraph = null;
        private readonly CommonElements _common = null;
        private ChartService _chart = null;
        private readonly ElementPosition _position = null;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the chart element of the event.
        /// </summary>
        /// <value>The chart element.</value>
        public object ChartElement
        {
            get
            {
                return _chartElement;
            }
        }


        /// <summary>
        /// Gets the ChartGraphics object of the event.
        /// </summary>
        public ChartGraphics ChartGraphics
        {
            get
            {
                return _chartGraph;
            }
        }

        /// <summary>
        /// Chart Common elements.
        /// </summary>
        internal CommonElements CommonElements
        {
            get
            {
                return _common;
            }
        }

        /// <summary>
        /// Chart element position in relative coordinates of the event.
        /// </summary>
        public ElementPosition Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Chart object of the event.
        /// </summary>
        public ChartService Chart
        {
            get
            {
                if (_chart == null && _common != null)
                {
                    _chart = _common.Chart;
                }

                return _chart;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Default constructor is not accessible
        /// </summary>
        private ChartPaintEventArgs()
        {
        }

        /// <summary>
        /// Paint event arguments constructor.
        /// </summary>
        /// <param name="chartElement">Chart element.</param>
        /// <param name="chartGraph">Chart graphics.</param>
        /// <param name="common">Common elements.</param>
        /// <param name="position">Position.</param>
        internal ChartPaintEventArgs(object chartElement, ChartGraphics chartGraph, CommonElements common, ElementPosition position)
        {
            _chartElement = chartElement;
            _chartGraph = chartGraph;
            _common = common;
            _position = position;
        }

        #endregion
    }
}
