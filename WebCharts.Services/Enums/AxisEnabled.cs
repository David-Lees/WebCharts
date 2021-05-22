namespace WebCharts.Services
{
    /// <summary>
    /// An enumeration that specifies the state of an axis.
    /// </summary>
    public enum AxisEnabled
    {
        /// <summary>
        /// The axis is only enabled if it used to plot a Series.
        /// </summary>
        Auto,

        /// <summary>
        /// The axis is always enabled.
        /// </summary>
        True,

        /// <summary>
        /// The axis is never enabled.
        /// </summary>
        False
    };
}