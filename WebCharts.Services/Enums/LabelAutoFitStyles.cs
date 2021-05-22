using System;

namespace WebCharts.Services
{
    /// <summary>
    /// An enumeration of auto-fitting styles of the axis labels.
    /// </summary>
    [Flags]
    public enum LabelAutoFitStyles
    {
        /// <summary>
        /// No auto-fitting.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow font size increasing.
        /// </summary>
        IncreaseFont = 1,

        /// <summary>
        /// Allow font size decreasing.
        /// </summary>
        DecreaseFont = 2,

        /// <summary>
        /// Allow using staggered labels.
        /// </summary>
        StaggeredLabels = 4,

        /// <summary>
        /// Allow changing labels angle using values of 0, 30, 60 and 90 degrees.
        /// </summary>
        LabelsAngleStep30 = 8,

        /// <summary>
        /// Allow changing labels angle using values of 0, 45, 90 degrees.
        /// </summary>
        LabelsAngleStep45 = 16,

        /// <summary>
        /// Allow changing labels angle using values of 0 and 90 degrees.
        /// </summary>
        LabelsAngleStep90 = 32,

        /// <summary>
        /// Allow replacing spaces with the new line character.
        /// </summary>
        WordWrap = 64,
    }
}