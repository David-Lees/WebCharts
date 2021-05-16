using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCharts.Services.Enums
{
    /// <summary>
    /// Defines the style how the bars/columns are drawn.
    /// </summary>
    public enum BarDrawingStyle
    {
        /// <summary>
        /// Default bar/column style.
        /// </summary>
        Default,

        /// <summary>
        /// Cylinder bar/column style.
        /// </summary>
        Cylinder,

        /// <summary>
        /// Emboss bar/column style.
        /// </summary>
        Emboss,

        /// <summary>
        /// LightToDark bar/column style.
        /// </summary>
        LightToDark,

        /// <summary>
        /// Wedge bar/column style.
        /// </summary>
        Wedge,
    }
}
