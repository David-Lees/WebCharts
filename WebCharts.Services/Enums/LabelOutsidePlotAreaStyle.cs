namespace WebCharts.Services
{
    /// <summary>
    /// Data point label outside of the plotting area style.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLabelOutsidePlotAreaStyle_LabelOutsidePlotAreaStyle")
    ]
    public enum LabelOutsidePlotAreaStyle
    {
        /// <summary>
        /// Labels can be positioned outside of the plotting area.
        /// </summary>
        Yes,

        /// <summary>
        /// Labels can not be positioned outside of the plotting area.
        /// </summary>
        No,

        /// <summary>
        /// Labels can be partially outside of the plotting area.
        /// </summary>
        Partial
    }
}