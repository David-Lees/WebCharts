namespace WebCharts.Services.Enums
{
    /// <summary>
    /// An enumeration that specifies a background image drawing mode.
    /// </summary>
    public enum ChartImageWrapMode
	{
		/// <summary>
		/// Background image is scaled to fit the entire chart element.
		/// </summary>		
		Scaled = WrapMode.Clamp,

		/// <summary>
		/// Background image is tiled to fit the entire chart element.
		/// </summary>
		Tile = WrapMode.Tile,

		/// <summary>
		/// Every other tiled image is reversed around the X-axis.
		/// </summary>
		TileFlipX = WrapMode.TileFlipX,

		/// <summary>
		/// Every other tiled image is reversed around the X-axis and Y-axis.
		/// </summary>
		TileFlipXY = WrapMode.TileFlipXY,

		/// <summary>
		/// Every other tiled image is reversed around the Y-axis.
		/// </summary>
		TileFlipY = WrapMode.TileFlipY,

		/// <summary>
		/// Background image is not scaled.
		/// </summary>
		Unscaled = 100
	};

}
