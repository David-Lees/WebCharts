namespace WebCharts.Services.Enums
{
    /// <summary>
    /// Defines the style how the pie and doughnut charts are drawn.
    /// </summary>
    public enum PieDrawingStyle
    {
        /// <summary>
        /// Default pie/doughnut drawing style.
        /// </summary>
        Default,

        /// <summary>
        /// Soft edge shadow is drawn on the edges of the pie/doughnut slices.
        /// </summary>
		SoftEdge,

        /// <summary>
        /// A shadow is drawn from the top to the bottom of the pie/doughnut chart.
        /// </summary>
        Concave,
    }
}
