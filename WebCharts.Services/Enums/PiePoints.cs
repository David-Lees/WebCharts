namespace WebCharts.Services
{
    /// <summary>
    /// This enumeration defines all significant points in a pie
    /// slice. Only these points should be transformed for pie
    /// chart using Matrix object.
    /// </summary>
    public enum PiePoints
    {
        /// <summary>
        /// Angle 180 Top point on the arc
        /// </summary>
        Top180,

        /// <summary>
        /// Angle 180 Bottom point on the arc
        /// </summary>
        Bottom180,

        /// <summary>
        /// Angle 0 Top point on the arc
        /// </summary>
        Top0,

        /// <summary>
        /// Angle 0 Bottom point on the arc
        /// </summary>
        Bottom0,

        /// <summary>
        /// Top Start Angle point on the arc
        /// </summary>
        TopStart,

        /// <summary>
        /// Top End Angle point on the arc
        /// </summary>
        TopEnd,

        /// <summary>
        /// Bottom Start Angle point on the arc
        /// </summary>
        BottomStart,

        /// <summary>
        /// Bottom End Angle point on the arc
        /// </summary>
        BottomEnd,

        /// <summary>
        /// Center Top
        /// </summary>
        TopCenter,

        /// <summary>
        /// Center Bottom
        /// </summary>
        BottomCenter,

        /// <summary>
        /// Top Label Line
        /// </summary>
        TopLabelLine,

        /// <summary>
        /// Top Label Line Out
        /// </summary>
        TopLabelLineout,

        /// <summary>
        /// Top Label Center
        /// </summary>
        TopLabelCenter,

        /// <summary>
        /// Top Rectangle Top Left Point
        /// </summary>
        TopRectTopLeftPoint,

        /// <summary>
        /// Top Rectangle Right Bottom Point
        /// </summary>
        TopRectBottomRightPoint,

        /// <summary>
        /// Bottom Rectangle Top Left Point
        /// </summary>
        BottomRectTopLeftPoint,

        /// <summary>
        /// Bottom Rectangle Right Bottom Point
        /// </summary>
        BottomRectBottomRightPoint,

        /// <summary>
        /// Angle 180 Top point on the Doughnut arc
        /// </summary>
        DoughnutTop180,

        /// <summary>
        /// Angle 180 Bottom point on the Doughnut arc
        /// </summary>
        DoughnutBottom180,

        /// <summary>
        /// Angle 0 Top point on the Doughnut arc
        /// </summary>
        DoughnutTop0,

        /// <summary>
        /// Angle 0 Bottom point on the Doughnut arc
        /// </summary>
        DoughnutBottom0,

        /// <summary>
        /// Top Start Angle point on the Doughnut arc
        /// </summary>
        DoughnutTopStart,

        /// <summary>
        /// Top End Angle point on the Doughnut arc
        /// </summary>
        DoughnutTopEnd,

        /// <summary>
        /// Bottom Start Angle point on the Doughnut arc
        /// </summary>
        DoughnutBottomStart,

        /// <summary>
        /// Bottom End Angle point on the Doughnut arc
        /// </summary>
        DoughnutBottomEnd,

        /// <summary>
        /// Doughnut Top Rectangle Top Left Point
        /// </summary>
        DoughnutTopRectTopLeftPoint,

        /// <summary>
        /// Doughnut Top Rectangle Right Bottom Point
        /// </summary>
        DoughnutTopRectBottomRightPoint,

        /// <summary>
        /// Doughnut Bottom Rectangle Top Left Point
        /// </summary>
        DoughnutBottomRectTopLeftPoint,

        /// <summary>
        /// Doughnut Bottom Rectangle Right Bottom Point
        /// </summary>
        DoughnutBottomRectBottomRightPoint,
    }
}