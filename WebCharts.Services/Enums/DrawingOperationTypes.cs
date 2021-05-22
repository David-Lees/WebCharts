using System;

namespace WebCharts.Services
{
    /// <summary>
    /// AxisName of drawing operation.
    /// </summary>
    [Flags]
    internal enum DrawingOperationTypes
    {
        /// <summary>
        /// Draw element.
        /// </summary>
        DrawElement = 1,

        /// <summary>
        /// Calculate element path. (for selection or tooltips)
        /// </summary>
        CalcElementPath = 2,
    }
}