using WebCharts.Services.Models.Common;

namespace WebCharts.Services.Enums
{
    /// <summary>
    /// Data point label callout style.
    /// </summary>
    [
    SRDescription("DescriptionAttributeLabelCalloutStyle_LabelCalloutStyle")
    ]
    public enum LabelCalloutStyle
    {
        /// <summary>
        /// Label connected with the marker using just a line.
        /// </summary>
        None,
        /// <summary>
        /// Label is undelined and connected with the marker using a line.
        /// </summary>
        Underlined,
        /// <summary>
        /// Box is drawn around the label and it's connected with the marker using a line.
        /// </summary>
        Box
    }
}
