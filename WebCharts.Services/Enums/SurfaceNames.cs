using System;

namespace WebCharts.Services
{
    /// <summary>
    /// 3D cube surfaces names.
    /// </summary>
    [Flags]
    internal enum SurfaceNames
    {
        /// <summary>
        /// Front.
        /// </summary>
        Front = 1,

        /// <summary>
        /// Back.
        /// </summary>
        Back = 2,

        /// <summary>
        /// Left.
        /// </summary>
        Left = 4,

        /// <summary>
        /// Right.
        /// </summary>
        Right = 8,

        /// <summary>
        /// Top.
        /// </summary>
        Top = 16,

        /// <summary>
        /// Bottom.
        /// </summary>
        Bottom = 32
    }
}