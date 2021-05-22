namespace WebCharts.Services
{
    /// <summary>
    /// An enumeration of line styles.
    /// </summary>
    public enum ChartDashStyle
    {
        /// <summary>
        /// Line style not set
        /// </summary>
        NotSet,

        /// <summary>
        /// Specifies a line consisting of dashes.
        /// </summary>
        Dash,

        /// <summary>
        /// Specifies a line consisting of a repeating pattern of dash-dot.
        /// </summary>
        DashDot,

        /// <summary>
        /// Specifies a line consisting of a repeating pattern of dash-dot-dot.
        /// </summary>
        DashDotDot,

        /// <summary>
        /// Specifies a line consisting of dots.
        /// </summary>
        Dot,

        /// <summary>
        /// Specifies a solid line.
        /// </summary>
        Solid,
    }
}