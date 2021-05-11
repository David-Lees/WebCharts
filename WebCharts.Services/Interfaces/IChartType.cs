using System;
using System.Collections;
using System.IO;
using WebCharts.Services.Models.ChartTypes;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Interfaces
{
    /// <summary>
    /// IChartType interface must be implemented for any standard or custom 
    /// chart type displayed in the chart control. This interface defines 
    /// properties which provide information on chart type behaviour including 
    /// how many Y values supported, is it a stacked chart type, how it 
    /// interacts with axes and much more.
    /// 
    /// IChartType interface methods define how to draw series data point, 
    /// calculate Y values and process SmartLabelStyle.
    /// </summary>
    internal interface IChartType : IDisposable
	{
		#region Properties

		/// <summary>
		/// Chart type name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets chart type image
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		MemoryStream GetImage(ChartTypeRegistry registry);

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		bool Stacked { get; }


		/// <summary>
		/// True if stacked chart type supports groups
		/// </summary>
		bool SupportStackedGroups { get; }


		/// <summary>
		/// True if stacked chart type should draw separately positive and 
		/// negative data points ( Bar and column Stacked types ).
		/// </summary>
		bool StackSign { get; }

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		bool RequireAxes { get; }

		/// <summary>
		/// True if chart type requires circular chart area.
		/// </summary>
		bool CircularChartArea { get; }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		bool SupportLogarithmicAxes { get; }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		bool SwitchValueAxes { get; }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		bool SideBySideSeries { get; }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		bool DataPointsInLegend { get; }

		/// <summary>
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		bool ApplyPaletteColorsToPoints { get; }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		bool ExtraYValuesConnectedToYAxis { get; }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		bool ZeroCrossing { get; }

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		int YValuesPerPoint { get; }

		/// <summary>
		/// Chart type with two y values used for scale ( bubble chart type )
		/// </summary>
		bool SecondYScale { get; }

		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		bool HundredPercent { get; }

		/// <summary>
		/// Indicates that negative 100% stacked values are shown on
		/// the other side of the X axis
		/// </summary>
		bool HundredPercentSupportNegative { get; }

		/// <summary>
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		LegendImageStyle GetLegendImageStyle(Series series);

		#endregion

		#region Painting and Selection methods

		/// <summary>
		/// Draw chart on specified chart graphics.
		/// </summary>
		/// <param name="graph">Chart grahhics object.</param>
		/// <param name="common">Common elements.</param>
		/// <param name="area">Chart area to draw on.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		void Paint(ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw);

        #endregion

        #region Y values methods

        /// <summary>
        /// Helper function, which returns the Y value of the data point.
        /// </summary>
        /// <param name="common">Chart common elements.</param>
        /// <param name="area">Chart area the series belongs to.</param>
        /// <param name="series">Sereis of the point.</param>
        /// <param name="point">Point object.</param>
        /// <param name="pointIndex">Index of the point.</param>
        /// <param name="yValueIndex">Index of the Y value to get.</param>
        /// <returns>Y value of the point.</returns>
        double GetYValue(CommonElements common, ChartArea area, Series series, DataPoint point, int pointIndex, int yValueIndex);

		#endregion

		#region SmartLabelStyle methods

		/// <summary>
		/// Adds markers position to the list. Used to check SmartLabelStyle overlapping.
		/// </summary>
		/// <param name="common">Common chart elements.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="series">Series values to be used.</param>
		/// <param name="list">List to add to.</param>
		void AddSmartLabelMarkerPositions(CommonElements common, ChartArea area, Series series, ArrayList list);

		#endregion
	}
}
