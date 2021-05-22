using System;

namespace WebCharts.Services
{
    /// <summary>
    /// Chart legend customize events arguments
    /// </summary>
    public class CustomizeLegendEventArgs : EventArgs
    {
        private readonly LegendItemsCollection _legendItems = null;
        private readonly string _legendName = "";

        /// <summary>
        /// Default construvtor is not accessible
        /// </summary>
        private CustomizeLegendEventArgs()
        {
        }

        /// <summary>
        /// Customize legend event arguments constructor
        /// </summary>
        /// <param name="legendItems">Legend items collection.</param>
        public CustomizeLegendEventArgs(LegendItemsCollection legendItems)
        {
            _legendItems = legendItems;
        }

        /// <summary>
        /// Customize legend event arguments constructor
        /// </summary>
        /// <param name="legendItems">Legend items collection.</param>
        /// <param name="legendName">Legend name.</param>
        public CustomizeLegendEventArgs(LegendItemsCollection legendItems, string legendName)
        {
            _legendItems = legendItems;
            _legendName = legendName;
        }

        /// <summary>
        /// Legend name.
        /// </summary>
        public string LegendName
        {
            get
            {
                return _legendName;
            }
        }

        /// <summary>
        /// Legend items collection.
        /// </summary>
        public LegendItemsCollection LegendItems
        {
            get
            {
                return _legendItems;
            }
        }
    }
}