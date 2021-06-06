// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	AxisScrollBar class represent axis scroolbar. There
//              is a big difference how this UI  functionality
//              implemented for Windows Forms and ASP.NET. For
//              Windows Forms a custom drawn scrollbar control is
//              drawn in the chart which reacts to the mouse input and
//              changes current axis data scaleView.
//

using SkiaSharp;
using System;

namespace WebCharts.Services
{
    #region Scroll bar enumerations

    /// <summary>
    /// An enumeration of scrollbar button types.
    /// </summary>
    public enum ScrollBarButtonType
    {
        /// <summary>
        /// Thumb tracker button.
        /// </summary>
        ThumbTracker,

        /// <summary>
        /// Scroll by substracting small distance.
        /// </summary>
        SmallDecrement,

        /// <summary>
        /// Scroll by adding small distance.
        /// </summary>
        SmallIncrement,

        /// <summary>
        /// Scroll by substracting large distance.
        /// </summary>
        LargeDecrement,

        /// <summary>
        /// Scroll by adding large distance.
        /// </summary>
        LargeIncrement,

        /// <summary>
        /// Zoom reset button.
        /// </summary>
        ZoomReset
    }

    /// <summary>
    /// An enumeration of scrollbar button style flags.
    /// </summary>
    [Flags]
    public enum ScrollBarButtonStyles
    {
        /// <summary>
        /// No buttons are shown.
        /// </summary>
        None = 0,

        /// <summary>
        /// Small increment or decrement buttons are shown.
        /// </summary>
        SmallScroll = 1,

        /// <summary>
        /// Reset zoom buttons are shown.
        /// </summary>
        ResetZoom = 2,

        /// <summary>
        /// All buttons are shown.
        /// </summary>
        All = SmallScroll | ResetZoom
    }

    #endregion Scroll bar enumerations

    /// <summary>
    /// AxisScrollBar class represents the axis scrollbar. It is exposed as the
    /// ScrollBar property of the Axis class. It contains scrollbar appearance
    /// properties and drawing methods.
    /// </summary>
    public class AxisScrollBar : IDisposable
    {
        #region Scroll bar fields

        // Reference to the axis data scaleView class
        internal Axis axis = null;

        // Indicates that scollbra will be drawn
        private bool _enabled = true;

        // Axis data scaleView scroll bar style
        private ScrollBarButtonStyles _scrollBarButtonStyle = ScrollBarButtonStyles.All;

        // Axis data scaleView scroll bar size
        private double _scrollBarSize = 14.0;

        // Axis data scaleView scroll bar buttons color
        private SKColor _buttonColor = SKColor.Empty;

        // Axis data scaleView scroll bar back color
        private SKColor _backColor = SKColor.Empty;

        // Axis data scaleView scroll bar lines color
        private SKColor _lineColor = SKColor.Empty;

        // Current scroll bar drawing colors
        private readonly SKColor _buttonCurrentColor = SKColor.Empty;

        private readonly SKColor _lineCurrentColor = SKColor.Empty;

        // Position of the scrollbar (true - edge of PlotArea, false - edge of chart area)
        private bool _isPositionedInside = true;

        #endregion Scroll bar fields

        #region Scroll bar constructors and initialization

        /// <summary>
        /// AxisScrollBar constructor.
        /// </summary>
        public AxisScrollBar()
        {
        }

        /// <summary>
        /// Axis scroll bar constructor.
        /// </summary>
        /// <param name="axis">Reference to the axis class.</param>
        internal AxisScrollBar(Axis axis)
        {
            // Save reference to the axis data scaleView
            this.axis = axis;
        }

        #endregion Scroll bar constructors and initialization

        #region Scroll bar properties

        /// <summary>
        /// Gets or sets a flag which indicates whether scroll bar is positioned inside or outside of chart area.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisScrollBar_PositionInside")
        ]
        public bool IsPositionedInside
        {
            get
            {
                return _isPositionedInside;
            }
            set
            {
                if (_isPositionedInside != value)
                {
                    _isPositionedInside = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether the scroll bar is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisScrollBar_Enabled")
        ]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ChartArea that contains this scrollbar.
        /// </summary>
        public ChartArea ChartArea
        {
            get
            {
                return axis.ChartArea;
            }
        }

        /// <summary>
        /// Gets the Axis that contains this scrollbar.
        /// </summary>
        public Axis Axis
        {
            get
            {
                return axis;
            }
        }

        /// <summary>
        /// Gets or sets the style of the scrollbar button.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisScrollBar_Buttons"),
        ]
        public ScrollBarButtonStyles ButtonStyle
        {
            get
            {
                return _scrollBarButtonStyle;
            }
            set
            {
                if (_scrollBarButtonStyle != value)
                {
                    _scrollBarButtonStyle = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the scrollbar.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisScrollBar_Size"),
        ]
        public double Size
        {
            get
            {
                return _scrollBarSize;
            }
            set
            {
                if (_scrollBarSize != value)
                {
                    // Check values range
                    if (value < 5.0 || value > 20.0)
                    {
                        throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionScrollBarSizeInvalid));
                    }
                    _scrollBarSize = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the button color of the scrollbar.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeAxisScrollBar_ButtonColor"),
        ]
        public SKColor ButtonColor
        {
            get
            {
                return _buttonColor;
            }
            set
            {
                if (_buttonColor != value)
                {
                    _buttonColor = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the line color of the scrollbar.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeLineColor"),
        ]
        public SKColor LineColor
        {
            get
            {
                return _lineColor;
            }
            set
            {
                if (_lineColor != value)
                {
                    _lineColor = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of the scrollbar.
        /// </summary>
        [
        SRCategory("CategoryAttributeAxisView"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        public SKColor BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    if (axis != null)
                    {
                        axis.ChartArea.Invalidate();
                    }
                }
            }
        }

        #endregion Scroll bar properties

        #region Scroll bar public methods

        /// <summary>
        /// This method returns true if the scrollbar is visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return false;
            }
        }

        #endregion Scroll bar public methods

        #region Scroll bar painting methods

        /// <summary>
        /// Draws axis scroll bar.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        internal void Paint(ChartGraphics graph)
        {
            // Do nothing
        }

        /// <summary>
        /// Draws 3D button in the scroll bar
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="buttonRect">Button position.</param>
        /// <param name="pressedState">Indicates that button is pressed.</param>
        /// <param name="buttonType">Button type to draw.</param>
        internal void PaintScrollBar3DButton(
            ChartGraphics graph,
            SKRect buttonRect,
            bool pressedState,
            ScrollBarButtonType buttonType)
        {
            // Page Up/Down buttons do not require drawing
            if (buttonType == ScrollBarButtonType.LargeIncrement || buttonType == ScrollBarButtonType.LargeDecrement)
            {
                return;
            }

            // Get 3 levels of colors for button drawing
            var darkerColor = ChartGraphics.GetGradientColor(_buttonCurrentColor, SKColors.Black, 0.5);
            var darkestColor = ChartGraphics.GetGradientColor(_buttonCurrentColor, SKColors.Black, 0.8);
            var lighterColor = ChartGraphics.GetGradientColor(_buttonCurrentColor, SKColors.White, 0.5);

            // Fill button rectangle background
            graph.FillRectangleRel(
                buttonRect,
                _buttonCurrentColor,
                ChartHatchStyle.None,
                "",
                ChartImageWrapMode.Tile,
                SKColor.Empty,
                ChartImageAlignmentStyle.Center,
                GradientStyle.None,
                SKColor.Empty,
                darkerColor,
                (pressedState) ? 1 : 0,
                ChartDashStyle.Solid,
                SKColor.Empty,
                0,
                PenAlignment.Outset);

            // Check if 2 or 1 pixel border will be drawn (if size too small)
            bool singlePixelBorder = Size <= 12;

            // Draw 3D effect around the button when not pressed
            if (!pressedState)
            {
                // Get relative size of 1 pixel
                SKSize pixelRelativeSize = new(1, 1);
                pixelRelativeSize = graph.GetRelativeSize(pixelRelativeSize);

                // Draw top/left border with button color
                graph.DrawLineRel(
                    (singlePixelBorder) ? lighterColor : _buttonCurrentColor, 1, ChartDashStyle.Solid,
                    new SKPoint(buttonRect.Left, buttonRect.Bottom),
                    new SKPoint(buttonRect.Left, buttonRect.Top));
                graph.DrawLineRel(
                    (singlePixelBorder) ? lighterColor : _buttonCurrentColor, 1, ChartDashStyle.Solid,
                    new SKPoint(buttonRect.Left, buttonRect.Top),
                    new SKPoint(buttonRect.Right, buttonRect.Top));

                // Draw right/bottom border with the darkest color
                graph.DrawLineRel(
                    (singlePixelBorder) ? darkerColor : darkestColor, 1, ChartDashStyle.Solid,
                    new SKPoint(buttonRect.Right, buttonRect.Bottom),
                    new SKPoint(buttonRect.Right, buttonRect.Top));
                graph.DrawLineRel(
                    (singlePixelBorder) ? darkerColor : darkestColor, 1, ChartDashStyle.Solid,
                    new SKPoint(buttonRect.Left, buttonRect.Bottom),
                    new SKPoint(buttonRect.Right, buttonRect.Bottom));

                if (!singlePixelBorder)
                {
                    // Draw right/bottom border (offset 1) with the dark color
                    graph.DrawLineRel(
                        darkerColor, 1, ChartDashStyle.Solid,
                        new SKPoint(buttonRect.Right - pixelRelativeSize.Width, buttonRect.Bottom - pixelRelativeSize.Height),
                        new SKPoint(buttonRect.Right - pixelRelativeSize.Width, buttonRect.Top + pixelRelativeSize.Height));
                    graph.DrawLineRel(
                        darkerColor, 1, ChartDashStyle.Solid,
                        new SKPoint(buttonRect.Left + pixelRelativeSize.Width, buttonRect.Bottom - pixelRelativeSize.Height),
                        new SKPoint(buttonRect.Right - pixelRelativeSize.Width, buttonRect.Bottom - pixelRelativeSize.Height));

                    // Draw top/left border (offset 1) with lighter color
                    graph.DrawLineRel(
                        lighterColor, 1, ChartDashStyle.Solid,
                        new SKPoint(buttonRect.Left + pixelRelativeSize.Width, buttonRect.Bottom - pixelRelativeSize.Height),
                        new SKPoint(buttonRect.Left + pixelRelativeSize.Width, buttonRect.Top + pixelRelativeSize.Height));
                    graph.DrawLineRel(
                        lighterColor, 1, ChartDashStyle.Solid,
                        new SKPoint(buttonRect.Left + pixelRelativeSize.Width, buttonRect.Left + pixelRelativeSize.Height),
                        new SKPoint(buttonRect.Right - pixelRelativeSize.Width, buttonRect.Left + pixelRelativeSize.Height));
                }
            }

            // Check axis orientation
            bool verticalAxis = (axis.AxisPosition == AxisPosition.Left ||
                axis.AxisPosition == AxisPosition.Right);

            // Set graphics transformation for button pressed mode
            float pressedShifting = (singlePixelBorder) ? 0.5f : 1f;
            if (pressedState)
            {
                graph.TranslateTransform(pressedShifting, pressedShifting);
            }

            // Draw button image
            SKRect buttonAbsRect = graph.GetAbsoluteRectangle(buttonRect);
            float imageOffset = (singlePixelBorder) ? 2 : 3;
            switch (buttonType)
            {
                case (ScrollBarButtonType.SmallDecrement):
                    {
                        // Calculate triangal points position
                        SKPoint[] points = new SKPoint[3];
                        if (verticalAxis)
                        {
                            points[0].X = buttonAbsRect.Left + imageOffset;
                            points[0].Y = buttonAbsRect.Top + (imageOffset + 1f);
                            points[1].X = buttonAbsRect.Left + buttonAbsRect.Width / 2f;
                            points[1].Y = buttonAbsRect.Bottom - imageOffset;
                            points[2].X = buttonAbsRect.Right - imageOffset;
                            points[2].Y = buttonAbsRect.Top + (imageOffset + 1f);
                        }
                        else
                        {
                            points[0].X = buttonAbsRect.Left + imageOffset;
                            points[0].Y = buttonAbsRect.Top + buttonAbsRect.Height / 2f;
                            points[1].X = buttonAbsRect.Right - (imageOffset + 1f);
                            points[1].Y = buttonAbsRect.Top + imageOffset;
                            points[2].X = buttonAbsRect.Right - (imageOffset + 1f);
                            points[2].Y = buttonAbsRect.Bottom - imageOffset;
                        }

                        using var brush = new SKPaint { Style = SKPaintStyle.Fill, Color = _lineCurrentColor };

                        graph.FillPolygon(brush, points);

                        break;
                    }
                case (ScrollBarButtonType.SmallIncrement):
                    {
                        // Calculate triangal points position
                        SKPoint[] points = new SKPoint[3];
                        if (verticalAxis)
                        {
                            points[0].X = buttonAbsRect.Left + imageOffset;
                            points[0].Y = buttonAbsRect.Bottom - (imageOffset + 1f);
                            points[1].X = buttonAbsRect.Left + buttonAbsRect.Width / 2f;
                            points[1].Y = buttonAbsRect.Top + imageOffset;
                            points[2].X = buttonAbsRect.Right - imageOffset;
                            points[2].Y = buttonAbsRect.Bottom - (imageOffset + 1f);
                        }
                        else
                        {
                            points[0].X = buttonAbsRect.Right - imageOffset;
                            points[0].Y = buttonAbsRect.Top + buttonAbsRect.Height / 2f;
                            points[1].X = buttonAbsRect.Left + (imageOffset + 1f);
                            points[1].Y = buttonAbsRect.Top + imageOffset;
                            points[2].X = buttonAbsRect.Left + (imageOffset + 1f);
                            points[2].Y = buttonAbsRect.Bottom - imageOffset;
                        }

                        using var brush = new SKPaint { Style = SKPaintStyle.Fill, Color = _lineCurrentColor };
                        graph.FillPolygon(brush, points);

                        break;
                    }
                case (ScrollBarButtonType.ZoomReset):
                    {
                        // Draw circule with a minus sign

                        using var pen = new SKPaint { Style = SKPaintStyle.Fill, Color = _lineCurrentColor };

                        graph.DrawEllipse(pen, buttonAbsRect.Left + imageOffset - 0.5f, buttonAbsRect.Top + imageOffset - 0.5f, buttonAbsRect.Width - 2f * imageOffset, buttonAbsRect.Height - 2f * imageOffset);
                        graph.DrawLine(pen, buttonAbsRect.Left + imageOffset + 1.5f, buttonAbsRect.Top + buttonAbsRect.Height / 2f - 0.5f, buttonAbsRect.Right - imageOffset - 2.5f, buttonAbsRect.Top + buttonAbsRect.Height / 2f - 0.5f);

                        break;
                    }
            }

            // Reset graphics transformation for button pressed mode
            if (pressedState)
            {
                graph.TranslateTransform(-pressedShifting, -pressedShifting);
            }
        }

        #endregion Scroll bar painting methods

        #region Coordinate convertion methods

        /// <summary>
        /// Converts Relative size to Absolute size
        /// </summary>
        /// <param name="relative">Relative size in %</param>
        /// <returns>Absolute size</returns>
        internal SKSize GetAbsoluteSize(SKSize relative)
        {
            SKSize absolute = SKSize.Empty;

            // Convert relative coordinates to absolute coordinates
            absolute.Width = relative.Width * (axis.Common.Width - 1) / 100F;
            absolute.Height = relative.Height * (axis.Common.Height - 1) / 100F;

            // Return Absolute coordinates
            return absolute;
        }

        /// <summary>
        /// Converts Absolute size to Relative size
        /// </summary>
        /// <param name="size">Absolute size</param>
        /// <returns>Relative size</returns>
        internal SKSize GetRelativeSize(SKSize size)
        {
            SKSize relative = SKSize.Empty;

            // Convert absolute coordinates to relative coordinates
            relative.Width = size.Width * 100F / ((float)(axis.Common.Width - 1));
            relative.Height = size.Height * 100F / ((float)(axis.Common.Height - 1));

            // Return relative coordinates
            return relative;
        }

        #endregion Coordinate convertion methods

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Do nothing
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// The arguments for a scrollbar event.
    /// </summary>
    public class ScrollBarEventArgs : EventArgs
    {
        #region Private fields

        // Private fields for properties values storage
        private readonly Axis _axis = null;

        #endregion Private fields

        #region Constructors

        /// <summary>
        /// ScrollBarEventArgs constructor.
        /// </summary>
        /// <param name="axis">Axis containing the scrollbar.</param>
        public ScrollBarEventArgs(Axis axis)
        {
            _axis = axis;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Axis containing the scrollbar of the event.
        /// </summary>
        [
        SRDescription("DescriptionAttributeAxis"),
        ]
        public Axis Axis
        {
            get
            {
                return _axis;
            }
        }

        /// <summary>
        /// ChartArea containing the scrollbar of the event.
        /// </summary>
        [
        SRDescription("DescriptionAttributeChartArea"),
        ]
        public ChartArea ChartArea
        {
            get
            {
                return _axis.ChartArea;
            }
        }

        /// <summary>
        /// Indicates if the event is handled by the user and no further processing is required.
        /// </summary>
        [
        SRDescription("DescriptionAttributeScrollBarEventArgs_Handled"),
        ]
        public bool IsHandled { get; set; } = false;

        #endregion Properties
    }
}