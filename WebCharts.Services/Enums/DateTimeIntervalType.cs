namespace WebCharts.Services.Enums
{
    /// <summary>
    /// An enumeration of units of measurement of an interval.
    /// </summary>
    public enum DateTimeIntervalType
	{
		/// <summary>
		/// Automatically determined by the Chart control.
		/// </summary>
		Auto,

		/// <summary>
		/// The interval is numerical.
		/// </summary>
		Number,

		/// <summary>
		/// The interval is years.
		/// </summary>
		Years,

		/// <summary>
		/// The interval is months.
		/// </summary>
		Months,

		/// <summary>
		/// The interval is weeks.
		/// </summary>
		Weeks,

		/// <summary>
		/// The interval is days.
		/// </summary>
		Days,

		/// <summary>
		/// The interval is hours.
		/// </summary>
		Hours,

		/// <summary>
		/// The interval is minutes.
		/// </summary>
		Minutes,

		/// <summary>
		/// The interval is seconds.
		/// </summary>
		Seconds,

		/// <summary>
		/// The interval is milliseconds.
		/// </summary>
		Milliseconds,

		/// <summary>
		/// The interval type is not defined.
		/// </summary>
		NotSet,
	}
}
