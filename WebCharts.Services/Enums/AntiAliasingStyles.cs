using System;

namespace WebCharts.Services.Enums
{
    /// <summary>
    /// An enumeration of anti-aliasing flags.
    /// </summary>
    [Flags]
    public enum AntiAliasingStyles
    {
        /// <summary>
        /// No anti-aliasing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use anti-aliasing when drawing text.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Use anti-aliasing when drawing grahics primitives (e.g. lines, rectangle)
        /// </summary>
        Graphics = 2,

        /// <summary>
        /// Use anti-alias for everything.
        /// </summary>
        All = Text | Graphics

    };

}
