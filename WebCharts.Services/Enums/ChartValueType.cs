namespace WebCharts.Services.Enums
{
    /// <summary>
    /// An enumeration that specifies value types for various chart properties
    /// </summary>
    public enum ChartValueType
	{
		/// <summary>
		/// Property type is set automatically by the Chart control.
		/// </summary>
		Auto,

		/// <summary>
		/// Double value.
		/// </summary>
		Double,

		/// <summary>
		/// Single value.
		/// </summary>
		Single,

		/// <summary>
		/// Int32 value.
		/// </summary>
		Int32,

		/// <summary>
		/// Int64 value.
		/// </summary>
		Int64,

		/// <summary>
		/// UInt32 value.
		/// </summary>
		UInt32,

		/// <summary>
		/// UInt64 value.
		/// </summary>
		UInt64,

		/// <summary>
		/// String value.
		/// </summary>
		String,

		/// <summary>
		/// DateTime value.
		/// </summary>
		DateTime,

		/// <summary>
		/// Date portion of the DateTime value.
		/// </summary>
		Date,

		/// <summary>
		/// Time portion of the DateTime value.
		/// </summary>
		Time,

		/// <summary>
		/// DateTime with offset
		/// </summary>
		DateTimeOffset
	};

}
