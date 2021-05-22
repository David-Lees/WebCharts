using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCharts.Models
{
    /// <summary>
    /// Raw data value for the summary table.
    /// Each object represents a single value in a table.
    /// These are pivoted into DetailPivoted objects.
    /// </summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.SummaryTableInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.SummaryTableExt"/>
    public class DetailData
    {
        /// <summary>Sensor parameter code</summary>
        public string ParCode { get; set; }

        /// <summary>Formatted result</summary>
        public string ResultMean { get; set; }

        /// <summary>Label for unit of measurement</summary>
        public string ParUnit { get; set; }

        /// <summary>Time of recording</summary>
        public DateTime TelTime { get; set; }

        /// <summary>ID of telemetry header record</summary>
        public int TelId { get; set; }

        /// <summary>Flag specifying if spectra data is available</summary>
        public int Spectra { get; set; }

        /// <summary>Sort order</summary>
        public Int16 SortOrder { get; set; }
    }

    /// <summary>
    /// Structure representing one row in the summary table.
    /// constructed from up to 10 DetailData objects via a pivot.
    /// </summary>
    public class DetailPivoted
    {
        /// <summary>The ID of the telemetry header record, or 0 in the case of forecast data</summary>
        public int id { get; set; }

        /// <summary>Date/time of the data point</summary>
        public DateTime TelTime { get; set; }

        /// <summary>Flag denoting if spectral data is available</summary>
        public int Spectra { get; set; }

        /// <summary>First pivoted data column</summary>
        public string Col0 { get; set; }

        /// <summary>Second pivoted data column</summary>
        public string Col1 { get; set; }

        /// <summary>Third pivoted data column</summary>
        public string Col2 { get; set; }

        /// <summary>Fourth pivoted data column</summary>
        public string Col3 { get; set; }

        /// <summary>Fifth pivoted data column</summary>
        public string Col4 { get; set; }

        /// <summary>Sixth pivoted data column</summary>
        public string Col5 { get; set; }

        /// <summary>Seventh pivoted data column</summary>
        public string Col6 { get; set; }

        /// <summary>Eighth pivoted data column</summary>
        public string Col7 { get; set; }

        /// <summary>Ninth pivoted data column</summary>
        public string Col8 { get; set; }

        /// <summary>Tenth pivoted data column</summary>
        public string Col9 { get; set; }

        /// <summary>Flag specifying if this is an observed value(false) or a forecast(true)</summary>
        public bool forecast { get; set; }
    }

    /// <summary>Data point for a line graph.</summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.GraphDataInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.GraphDataExt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.GraphDataWaterQuality"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.TelemetryResults(string)"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.BurstMeanResults(string)"/>
    /// <remarks>DepId is returned so that we can break the line between deployments</remarks>
    public class LineData
    {
        /// <summary>X value</summary>
        public DateTime time { get; set; }

        /// <summary>Y value</summary>
        public double? value { get; set; }

        /// <summary>Deployment ID</summary>
        public string DepId { get; set; }
    }

    /// <summary>
    /// Represents a single graph to display. A collection of these
    /// list which graphs to render in the basic charts tab
    /// <seealso cref="WaveNet.Models.SQLQueries.GraphDefExt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.GraphDefInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.GraphDefWaterQuality"/>
    /// </summary>
    public class GraphDefinition
    {
        /// <summary>Platform ID</summary>
        public string ID { get; set; }

        /// <summary>Text to use as title above graph</summary>
        public string Label { get; set; }

        /// <summary>Sensor parameter code to plot</summary>
        public string ParCode { get; set; }

        /// <summary>Source of data, either INT or EXT</summary>
        public string datasource { get; set; }

        /// <summary>Sort order, defined in database</summary>
        public Int16 SortOrder { get; set; }
    }

    /// <summary>Single data point for the 3D Spectra graphs.</summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.Spectra3DInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.Spectra3DExt"/>
    public class Spectra3DData
    {
        /// <summary>Series (Z axis)</summary>
        public Int64 Series { get; set; }

        /// <summary>Wave power (Y axis)</summary>
        public double? WavePower { get; set; }

        /// <summary>Wave Period (X axis)</summary>
        public double? WavePeriod { get; set; }
    }

    /// <summary>Single data point for use in Polar Plots</summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.PolarPlotInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.PolarPlotExt"/>
    public class PolarPoint
    {
        /// <summary>Age of reading in minutes</summary>
        public Int32 TimeLapse { get; set; }

        /// <summary>Wave height in metres</summary>
        public double? WaveHeight { get; set; }

        /// <summary>Wave period in seconds</summary>
        public double? WavePeriod { get; set; }

        /// <summary>Wave direction in degrees</summary>
        public double? Direction { get; set; }

        /// <summary>Unused?</summary>
        public string ParDescrDir { get; set; }

        /// <summary>Unused?</summary>
        public string ParDescrPer { get; set; }
    }

    /// <summary>Data point for the Spectra dialog.</summary>
    /// <remarks>These are used for the spectra table and the 2D graph.
    /// These are not used for the 3D spectra graph.</remarks>
    /// <seealso cref="WaveNet.Models.SQLQueries.SpectraInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.SpectraExt"/>
    public class SpectraPlotData
    {
        /// <summary>ID of the record in the header table</summary>
        public int SpectrumId { get; set; }

        /// <summary>Row index, used for sorting</summary>
        public Int16 Ordinal { get; set; }

        /// <summary>Minimum frequency</summary>
        public double? FreqMin { get; set; }

        /// <summary>Maximum frequency</summary>
        public double? FreqMax { get; set; }

        /// <summary>Wave power in m²/Hz</summary>
        public double? WavePower { get; set; }

        /// <summary>Wave direction in degrees</summary>
        public double? WaveDirection { get; set; }

        /// <summary>Wave spread in degrees</summary>
        public double? WaveSpread { get; set; }
    }

    /// <summary>Stores information for the heading in the Spectra dialog.</summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.SpectraHeaderInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.SpectraHeaderExt"/>
    public class SpectraHeader
    {
        /// <summary>Description</summary>
        public string Description { get; set; }

        /// <summary>Time</summary>
        public DateTime Time { get; set; }
    }

    /// <summary>Forecast data point. Used in the graphs and summary table.</summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.ForecastInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.ForecastExt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.ForecastSummaryInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.ForecastSummaryExt"/>
    public class ForecastData
    {
        /// <summary>Sensor parameter code</summary>
        public string ParCode { get; set; }

        /// <summary>Date/time the forecast was generated</summary>
        public DateTime RecordTime { get; set; }

        /// <summary>The raw value of the point used for charting</summary>
        public double? ResultValue { get; set; }

        /// <summary>The time the data point relates to</summary>
        public DateTime? ForecastTime { get; set; }

        /// <summary>The rounded/formatted result that is used for displaying in the table</summary>
        public string ResultFormatted { get; set; }
    }

    /// <summary>
    /// Summary information that appears at the top of the basic details
    /// dialog. SQLQueries.SummaryTableInt/SummaryTableExt returns one of
    /// these records
    /// </summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.SiteLabelInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.SiteLabelExt"/>
    public class SummaryInfo
    {
        /// <summary>The descriptive name of the platform</summary>
        public string Name { get; set; }

        /// <summary>WMO code assigned to the platform</summary>
        public int? WMO { get; set; }

        /// <summary>Latitude</summary>
        public double degLat { get; set; }

        /// <summary>Longitude</summary>
        public double degLong { get; set; }

        /// <summary>Name of organisation responsible for providing the data</summary>
        /// <remarks>Also used to determine the logo to be displayed</remarks>
        public string Provider { get; set; }

        /// <summary>Is the data to be restricted to certain users</summary>
        public bool IsRestricted { get; set; }
    }

    /// <summary>Data structure for the information on the basic Additional Info tab.</summary>
    /// <seealso cref="WaveNet.Models.SQLQueries.AdditionalInfoInt"/>
    /// <seealso cref="WaveNet.Models.SQLQueries.AdditionalInfoExt"/>
    public class AdditionalInfo
    {
        /// <summary>Indicates whether or not this is a WaveNet platform (if false, SmartBuoy)</summary>
        public bool IsWaveNet { get; set; }

        /// <summary>deployment group/platform description</summary>
        public string DepGroupDescr { get; set; }

        /// <summary>Latitude</summary>
        public double DepLocLat { get; set; }

        /// <summary>Longitude</summary>
        public double DepLocLong { get; set; }

        /// <summary>Depth of water at deployment site</summary>
        public double? DepDepth { get; set; }

        /// <summary>Instrument type description</summary>
        public string InstTypeDescr { get; set; }

        /// <summary>Start date of platform</summary>
        public DateTime? DepDateFrom { get; set; }

        /// <summary>End date of platform</summary>
        public DateTime? DepDateTo { get; set; }

        /// <summary>HTML formatted comments about the current deployment</summary>
        public string WebSiteComments { get; set; }

        /// <summary>Deployment description</summary>
        public string DepDescr { get; set; }

        /// <summary>Text for the copyright notice</summary>
        public string CopyrightLabel { get; set; }

        /// <summary>Licence Id. Used when generating download information</summary>
        public int LicenceId { get; set; }

        /// <summary>Is download restricted</summary>
        public bool IsRestricted { get; set; }
    }

    /// <summary>Label offset definition</summary>
    public class labeloffet
    {
        /// <summary>Left or right alignment</summary>
        public string align { get; set; }

        /// <summary>X Offset in pixels</summary>
        public int xoffset { get; set; }

        /// <summary>Y offset in pixels</summary>
        public int yoffset { get; set; }
    }

    /// <summary>Stores the text for the map labels drop down on the WaveNet map page</summary>
    [Table("WebsiteMapLabels")]
    public class MapLabel
    {
        /// <summary>Primary key, also used as sort order</summary>
        [Key]
        public int id { get; set; }

        /// <summary>The value for the option</summary>
        [MaxLength(20)]
        public string Value { get; set; }

        /// <summary>The description to display in the drop down</summary>
        [MaxLength(100)]
        public string Description { get; set; }

        /// <summary>Unit display</summary>
        [MaxLength(10)]
        public string Unit { get; set; }
    }
}
