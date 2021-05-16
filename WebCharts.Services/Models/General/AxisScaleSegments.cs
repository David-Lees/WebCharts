// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using SkiaSharp;
using System;
using System.Collections;
using System.Linq;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.General
{
    /// <summary>
    /// <b>AxisScaleSegment</b> class represents a single segment of the axis with
    /// it's own scale and intervals.
    /// </summary>
    [
    SRDescription("DescriptionAttributeAxisScaleSegment_AxisScaleSegment"),
    ]
    internal class AxisScaleSegment
    {
        #region Fields

        // Associated axis
        internal Axis axis = null;

        // Axis segment position in percent of the axis size
        private double _position = 0.0;

        // Axis segment size in percent of the axis size
        private double _size = 0.0;

        // Axis segment spacing in percent of the axis size
        private double _spacing = 0.0;

        // Axis segment interval offset.
        private readonly double _intervalOffset = 0;

        // Axis segment interval.
        private double _interval = 0;

        // Axis segment interval units type.
        private DateTimeIntervalType _intervalType = DateTimeIntervalType.Auto;

        // Axis segment interval offset units type.
        private readonly DateTimeIntervalType _intervalOffsetType = DateTimeIntervalType.Auto;

        // Stack used to save/load axis settings
        private readonly Stack _oldAxisSettings = new();

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Default object constructor.
        /// </summary>
        public AxisScaleSegment()
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Axis segment position in axis size percentage.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisScaleSegment_Position"),
        ]
        public double Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAxisScaleSegmentsPositionInvalid));
                }
                _position = value;
            }
        }

        /// <summary>
        /// Axis segment size in axis size percentage.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisScaleSegment_Size"),
        ]
        public double Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAxisScaleSegmentsSizeInvalid);
                }
                _size = value;
            }
        }

        /// <summary>
        /// Axis segment spacing in axis size percentage.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisScaleSegment_Spacing"),
        ]
        public double Spacing
        {
            get
            {
                return _spacing;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAxisScaleSegmentsSpacingInvalid);
                }
                _spacing = value;
            }
        }

        /// <summary>
        /// Axis segment scale maximum value.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisScaleSegment_ScaleMaximum"),
        ]
        public double ScaleMaximum { get; set; } = 0.0;

        /// <summary>
        /// Axis segment scale minimum value.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisScaleSegment_ScaleMinimum"),
        ]
        public double ScaleMinimum { get; set; } = 0.0;


        /// <summary>
        /// Axis segment interval size.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeAxisScaleSegment_Interval"),
        ]
        public double Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                // Axis interval properties must be set
                if (double.IsNaN(value))
                {
                    _interval = 0;
                }
                else
                {
                    _interval = value;
                }
            }
        }

        /// <summary>
        /// Axis segment interval offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeAxisScaleSegment_IntervalOffset"),
        ]
        public double IntervalOffset
        {
            get
            {
                return _intervalOffset;
            }
        }

        /// <summary>
        /// Axis segment interval type.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeAxisScaleSegment_IntervalType"),
        ]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return _intervalType;
            }
            set
            {
                // Axis interval properties must be set
                if (value == DateTimeIntervalType.NotSet)
                {
                    _intervalType = DateTimeIntervalType.Auto;
                }
                else
                {
                    _intervalType = value;
                }
            }
        }

        /// <summary>
        /// Axis segment interval offset type.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        SRDescription("DescriptionAttributeAxisScaleSegment_IntervalOffsetType"),
        ]
        public DateTimeIntervalType IntervalOffsetType
        {
            get
            {
                return _intervalOffsetType;
            }
        }

        /// <summary>
        /// Object associated with axis scale segment.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAxisScaleSegment_Tag"),
        ]
        public object Tag { get; set; } = null;

        #endregion // Properties

        #region Break Line Painting Methods

        /// <summary>
        /// Paints the axis break line.
        /// </summary>
        /// <param name="graph">Chart graphics to use.</param>
        /// <param name="nextSegment">Axis scale segment next to current.</param>
        internal void PaintBreakLine(ChartGraphics graph, AxisScaleSegment nextSegment)
        {
            // Get break line position 
            SKRect breakPosition = GetBreakLinePosition(graph, nextSegment);

            // Get top line graphics path
            SKPath breakLinePathTop = GetBreakLinePath(breakPosition, true);
            SKPath breakLinePathBottom = null;

            // Clear break line space using chart color behind the area
            if (breakPosition.Width > 0f && breakPosition.Height > 0f)
            {
                // Get bottom line graphics path
                breakLinePathBottom = GetBreakLinePath(breakPosition, false);

                // Clear plotting area background
                using SKPath fillPath = new();

                // Create fill path out of top and bottom break lines
                fillPath.AddPathReverse(breakLinePathTop);
                fillPath.AddPath(breakLinePathBottom);
                fillPath.Close();

                // Use chart back color to fill the area
                using SKPaint fillBrush = GetChartFillBrush(graph);
                graph.FillPath(fillBrush, fillPath);

                // Check if shadow exsits in chart area
                if (axis.ChartArea.ShadowOffset != 0 && axis.ChartArea.ShadowColor != SKColor.Empty)
                {
                    // Clear shadow
                    SKRect shadowPartRect = breakPosition;
                    if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                    {
                        shadowPartRect.Top += axis.ChartArea.ShadowOffset;
                        shadowPartRect.Bottom -= axis.ChartArea.ShadowOffset;
                        shadowPartRect.Left = shadowPartRect.Right - 1;
                        shadowPartRect.Right = shadowPartRect.Left + axis.ChartArea.ShadowOffset + 2;
                    }
                    else
                    {
                        shadowPartRect.Left += axis.ChartArea.ShadowOffset;
                        shadowPartRect.Right -= axis.ChartArea.ShadowOffset;
                        shadowPartRect.Top = shadowPartRect.Bottom - 1;
                        shadowPartRect.Bottom = shadowPartRect.Top + axis.ChartArea.ShadowOffset + 2;
                    }
                    graph.FillRectangle(fillBrush, shadowPartRect);

                    // Draw new shadow
                    using SKPath shadowPath = new();
                    shadowPath.AddPath(breakLinePathTop);

                    // Define maximum size
                    float size = axis.ChartArea.ShadowOffset;
                    if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                    {
                        size = Math.Min(size, breakPosition.Height);
                    }
                    else
                    {
                        size = Math.Min(size, breakPosition.Width);
                    }

                    // Define step to increase transperancy
                    int transparencyStep = (int)(axis.ChartArea.ShadowColor.Alpha / size);

                    // Set clip region to achieve spacing of the shadow
                    // Start with the plotting rectangle position
                    SKRect clipRegion = graph.GetAbsoluteRectangle(axis.PlotAreaPosition.ToSKRect());
                    if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                    {
                        clipRegion.Left += axis.ChartArea.ShadowOffset;
                        clipRegion.Right += axis.ChartArea.ShadowOffset;
                    }
                    else
                    {
                        clipRegion.Top += axis.ChartArea.ShadowOffset;
                        clipRegion.Bottom += axis.ChartArea.ShadowOffset;
                    }
                    graph.SetClip(graph.GetRelativeRectangle(clipRegion));

                    // Draw several lines to form shadow
                    for (int index = 0; index < size; index++)
                    {
                        SKMatrix newMatrix = new();

                        // Shift top break line by 1 pixel
                        if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                        {
                            newMatrix.Translate(0f, 1f);
                        }
                        else
                        {
                            newMatrix.Translate(1f, 0f);
                        }
                        shadowPath.Transform(newMatrix);


                        // Get line color
                        SKColor color = Color.FromArgb(
                            (byte)(axis.ChartArea.ShadowColor.Alpha - transparencyStep * index),
                            axis.ChartArea.ShadowColor);

                        using SKPaint shadowPen = new() { Color = color, StrokeWidth = 1 };
                        // Draw shadow
                        graph.DrawPath(shadowPen, shadowPath);
                    }

                    graph.ResetClip();
                }
            }

            // Draw Separator Line(s)
            if (axis.ScaleBreakStyle.BreakLineStyle != BreakLineStyle.None)
            {
                using SKPaint pen = new() { Color = axis.ScaleBreakStyle.LineColor, StrokeWidth = axis.ScaleBreakStyle.LineWidth };
                // Set line style
                pen.PathEffect = ChartGraphics.GetPenStyle(axis.ScaleBreakStyle.LineDashStyle, axis.ScaleBreakStyle.LineWidth);

                // Draw break lines
                graph.DrawPath(pen, breakLinePathTop);
                if (breakPosition.Width > 0f && breakPosition.Height > 0f)
                {
                    graph.DrawPath(pen, breakLinePathBottom);
                }
            }

            // Dispose break line paths
            breakLinePathTop.Dispose();
            if (breakLinePathBottom != null)
            {
                breakLinePathBottom.Dispose();
            }

        }

        /// <summary>
        /// Get fill brush used to fill axis break lines spacing.
        /// </summary>
        /// <param name="graph">chart graphics.</param>
        /// <returns>Fill brush.</returns>
        private SKPaint GetChartFillBrush(ChartGraphics graph)
        {
            ChartService chart = axis.ChartArea.Common.Chart;
            SKPaint brush;
            if (chart.BackGradientStyle == GradientStyle.None)
            {
                brush = new SKPaint() { Color = chart.BackColor };
            }
            else
            {
                // If a gradient type is set create a brush with gradient
                brush = ChartGraphics.GetGradientBrush(new SKRect(0, 0, chart.chartPicture.Width - 1, chart.chartPicture.Height - 1), chart.BackColor, chart.BackSecondaryColor, chart.BackGradientStyle);
            }

            if (chart.BackHatchStyle != ChartHatchStyle.None)
            {
                brush = ChartGraphics.GetHatchBrush(chart.BackHatchStyle, chart.BackColor, chart.BackSecondaryColor);
            }

            if (chart.BackImage.Length > 0 &&
                chart.BackImageWrapMode != ChartImageWrapMode.Unscaled &&
                chart.BackImageWrapMode != ChartImageWrapMode.Scaled)
            {
                brush = graph.GetTextureBrush(chart.BackImage, chart.BackImageTransparentColor, chart.BackImageWrapMode, chart.BackColor);
            }

            return brush;
        }

        /// <summary>
        /// Gets a path of the break line for specified position.
        /// </summary>
        /// <param name="breakLinePosition">Break line position.</param>
        /// <param name="top">Indicates if top or bottom break line path should be retrieved.</param>
        /// <returns>Graphics path with break line path.</returns>
        private SKPath GetBreakLinePath(SKRect breakLinePosition, bool top)
        {
            SKPath path = new();

            if (axis.ScaleBreakStyle.BreakLineStyle == BreakLineStyle.Wave)
            {
                SKPoint[] points;
                int pointNumber;
                if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                {
                    float startX = breakLinePosition.Left;
                    float endX = breakLinePosition.Right;
                    float y = (top) ? breakLinePosition.Top : breakLinePosition.Bottom;
                    pointNumber = ((int)(endX - startX) / 40) * 2;
                    if (pointNumber < 2)
                    {
                        pointNumber = 2;
                    }
                    float step = (endX - startX) / pointNumber;
                    points = new SKPoint[pointNumber + 1];
                    for (int pointIndex = 1; pointIndex < pointNumber + 1; pointIndex++)
                    {
                        points[pointIndex] = new SKPoint(
                            startX + pointIndex * step,
                            y + ((pointIndex % 2 == 0) ? -2f : 2f));
                    }

                    points[0] = new SKPoint(startX, y);
                    points[^1] = new SKPoint(endX, y);
                }
                else
                {
                    float startY = breakLinePosition.Top;
                    float endY = breakLinePosition.Bottom;
                    float x = (top) ? breakLinePosition.Left : breakLinePosition.Right;
                    pointNumber = ((int)(endY - startY) / 40) * 2;
                    if (pointNumber < 2)
                    {
                        pointNumber = 2;
                    }
                    float step = (endY - startY) / pointNumber;
                    points = new SKPoint[pointNumber + 1];
                    for (int pointIndex = 1; pointIndex < pointNumber + 1; pointIndex++)
                    {
                        points[pointIndex] = new SKPoint(
                            x + ((pointIndex % 2 == 0) ? -2f : 2f),
                            startY + pointIndex * step);
                    }

                    points[0] = new SKPoint(x, startY);
                    points[^1] = new SKPoint(x, endY);
                }

                path.AddPath(SkiaSharpExtensions.CreateSpline(points));
            }
            else if (axis.ScaleBreakStyle.BreakLineStyle == BreakLineStyle.Ragged)
            {
                Random rand = new(435657);
                SKPoint[] points;
                if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                {
                    float startX = breakLinePosition.Left;
                    float endX = breakLinePosition.Right;
                    float y = (top) ? breakLinePosition.Top : breakLinePosition.Bottom;
                    float step = 10f;
                    int pointNumber = (int)((endX - startX) / step);
                    if (pointNumber < 2)
                    {
                        pointNumber = 2;
                    }
                    points = new SKPoint[pointNumber];

                    for (int pointIndex = 1; pointIndex < pointNumber - 1; pointIndex++)
                    {
                        points[pointIndex] = new SKPoint(
                            startX + pointIndex * step,
                            y + rand.Next(-3, 3));
                    }

                    points[0] = new SKPoint(startX, y);
                    points[^1] = new SKPoint(endX, y);
                }
                else
                {
                    float startY = breakLinePosition.Top;
                    float endY = breakLinePosition.Bottom;
                    float x = (top) ? breakLinePosition.Left : breakLinePosition.Right;
                    float step = 10f;
                    int pointNumber = (int)((endY - startY) / step);
                    if (pointNumber < 2)
                    {
                        pointNumber = 2;
                    }
                    points = new SKPoint[pointNumber];

                    for (int pointIndex = 1; pointIndex < pointNumber - 1; pointIndex++)
                    {
                        points[pointIndex] = new SKPoint(
                            x + rand.Next(-3, 3),
                            startY + pointIndex * step);
                    }

                    points[0] = new SKPoint(x, startY);
                    points[^1] = new SKPoint(x, endY);
                }

                path.AddLines(points);
            }
            else
            {
                if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
                {
                    if (top)
                    {
                        path.AddLine(breakLinePosition.Left, breakLinePosition.Top, breakLinePosition.Right, breakLinePosition.Top);
                    }
                    else
                    {
                        path.AddLine(breakLinePosition.Left, breakLinePosition.Bottom, breakLinePosition.Right, breakLinePosition.Bottom);
                    }
                }
                else
                {
                    if (top)
                    {
                        path.AddLine(breakLinePosition.Left, breakLinePosition.Top, breakLinePosition.Left, breakLinePosition.Bottom);
                    }
                    else
                    {
                        path.AddLine(breakLinePosition.Right, breakLinePosition.Top, breakLinePosition.Right, breakLinePosition.Bottom);
                    }
                }
            }
            return path;
        }

        /// <summary>
        /// Gets position of the axis break line. Break line may be shown as a single 
        /// line or two lines separated with a spacing.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="nextSegment">Next segment reference.</param>
        /// <returns>Position of the axis break line in pixel coordinates.</returns>
        internal SKRect GetBreakLinePosition(ChartGraphics graph, AxisScaleSegment nextSegment)
        {
            // Start with the plotting rectangle position
            SKRect breakPosition = axis.PlotAreaPosition.ToSKRect();

            // Find maximum scale value of the current segment and minimuj of the next
            double from = axis.GetLinearPosition(nextSegment.ScaleMinimum);
            double to = axis.GetLinearPosition(ScaleMaximum);
            if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
            {
                breakPosition.Top = (float)Math.Min(from, to);
                breakPosition.Size = new(breakPosition.Width, (float)Math.Max(from, to));
            }
            else
            {
                breakPosition.Left = (float)Math.Min(from, to);
                breakPosition.Size = new((float)Math.Max(from, to), breakPosition.Height);
            }

            // Convert to pixels
            breakPosition = graph.GetAbsoluteRectangle(breakPosition).Round();

            // Add border width
            if (axis.AxisPosition == AxisPosition.Right || axis.AxisPosition == AxisPosition.Left)
            {
                var h = Math.Abs(breakPosition.Top - breakPosition.Height);
                var w = breakPosition.Width;
                breakPosition.Left -= axis.ChartArea.BorderWidth;                
                breakPosition.Size = new(w + 2 * axis.ChartArea.BorderWidth, h);                    
            }
            else
            {
                var w = Math.Abs(breakPosition.Left - breakPosition.Width);
                var h = breakPosition.Height + 2 * axis.ChartArea.BorderWidth;
                breakPosition.Top -= axis.ChartArea.BorderWidth;
                breakPosition.Size = new SKSize(w, h);
            }

            return breakPosition;
        }

        #endregion // Break Line Painting Methods

        #region Helper Methods

        /// <summary>
        /// Gets segment scale position and size in relative coordinates.
        /// Method takes in consideration segment spacing and space required fro separatorType.
        /// </summary>
        /// <param name="plotAreaSize">Plotting area size in relative coordinates.</param>
        /// <param name="scalePosition">Returns scale position.</param>
        /// <param name="scaleSize">Returns scale size.</param>
        internal void GetScalePositionAndSize(double plotAreaSize, out double scalePosition, out double scaleSize)
        {
            scaleSize = (Size - Spacing) * (plotAreaSize / 100.0);
            scalePosition = Position * (plotAreaSize / 100.0);
        }

        /// <summary>
        /// Saves axis settings and set them from the current segment.
        /// </summary>
        internal void SetTempAxisScaleAndInterval()
        {
            // Save current axis settings
            if (_oldAxisSettings.Count == 0)
            {
                _oldAxisSettings.Push(axis.maximum);
                _oldAxisSettings.Push(axis.minimum);

                _oldAxisSettings.Push(axis.majorGrid.interval);
                _oldAxisSettings.Push(axis.majorGrid.intervalType);
                _oldAxisSettings.Push(axis.majorGrid.intervalOffset);
                _oldAxisSettings.Push(axis.majorGrid.intervalOffsetType);

                _oldAxisSettings.Push(axis.majorTickMark.interval);
                _oldAxisSettings.Push(axis.majorTickMark.intervalType);
                _oldAxisSettings.Push(axis.majorTickMark.intervalOffset);
                _oldAxisSettings.Push(axis.majorTickMark.intervalOffsetType);

                _oldAxisSettings.Push(axis.LabelStyle.interval);
                _oldAxisSettings.Push(axis.LabelStyle.intervalType);
                _oldAxisSettings.Push(axis.LabelStyle.intervalOffset);
                _oldAxisSettings.Push(axis.LabelStyle.intervalOffsetType);
            }

            // Copy settings from the segment into the axis
            axis.maximum = ScaleMaximum;
            axis.minimum = ScaleMinimum;

            axis.majorGrid.interval = Interval;
            axis.majorGrid.intervalType = IntervalType;
            axis.majorGrid.intervalOffset = IntervalOffset;
            axis.majorGrid.intervalOffsetType = IntervalOffsetType;

            axis.majorTickMark.interval = Interval;
            axis.majorTickMark.intervalType = IntervalType;
            axis.majorTickMark.intervalOffset = IntervalOffset;
            axis.majorTickMark.intervalOffsetType = IntervalOffsetType;

            axis.LabelStyle.interval = Interval;
            axis.LabelStyle.intervalType = IntervalType;
            axis.LabelStyle.intervalOffset = IntervalOffset;
            axis.LabelStyle.intervalOffsetType = IntervalOffsetType;
        }

        /// <summary>
        /// Restore previously saved axis settings.
        /// </summary>
        internal void RestoreAxisScaleAndInterval()
        {
            if (_oldAxisSettings.Count > 0)
            {
                axis.LabelStyle.intervalOffsetType = (DateTimeIntervalType)_oldAxisSettings.Pop();
                axis.LabelStyle.intervalOffset = (double)_oldAxisSettings.Pop();
                axis.LabelStyle.intervalType = (DateTimeIntervalType)_oldAxisSettings.Pop();
                axis.LabelStyle.interval = (double)_oldAxisSettings.Pop();

                axis.majorTickMark.intervalOffsetType = (DateTimeIntervalType)_oldAxisSettings.Pop();
                axis.majorTickMark.intervalOffset = (double)_oldAxisSettings.Pop();
                axis.majorTickMark.intervalType = (DateTimeIntervalType)_oldAxisSettings.Pop();
                axis.majorTickMark.interval = (double)_oldAxisSettings.Pop();

                axis.majorGrid.intervalOffsetType = (DateTimeIntervalType)_oldAxisSettings.Pop();
                axis.majorGrid.intervalOffset = (double)_oldAxisSettings.Pop();
                axis.majorGrid.intervalType = (DateTimeIntervalType)_oldAxisSettings.Pop();
                axis.majorGrid.interval = (double)_oldAxisSettings.Pop();

                axis.minimum = (double)_oldAxisSettings.Pop();
                axis.maximum = (double)_oldAxisSettings.Pop();
            }
        }

        #endregion // Helper Methods

    }

    /// <summary>
    /// <b>AxisScaleSegmentCollection</b> is a class that stores collection of axis segments.
    /// </summary>
    [
    SRDescription("DescriptionAttributeAxisScaleSegmentCollection_AxisScaleSegmentCollection"),
    ]
    internal class AxisScaleSegmentCollection : CollectionBase
    {
        #region Fields

        // Axis this segment collection belongs to.
        private readonly Axis _axis = null;

        // Segment which is always used to convert scale values.
        // This value is set tmporarly when only one segment has 
        // to handle all the values.
        private AxisScaleSegment _enforcedSegment = null;

        // Indicates that values allowed to be outside of the scale segment.
        // Otherwise they will be rounded to Min and Max values.
        internal bool AllowOutOfScaleValues = false;

        #endregion // Fields

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        /// <remarks>
        /// This constructor is for internal use and should not be part of documentation.
        /// </remarks>
        public AxisScaleSegmentCollection()
        {
        }

        /// <summary>
        /// Default public constructor.
        /// </summary>
        /// <remarks>
        /// This constructor is for internal use and should not be part of documentation.
        /// </remarks>
        /// <param name="axis">
        /// Chart axis this collection belongs to
        /// </param>
        internal AxisScaleSegmentCollection(Axis axis)
        {
            _axis = axis;
        }

        #endregion // Construction and Initialization

        #region Indexer

        /// <summary>
        /// Axis scale segment collection indexer.
        /// </summary>
        /// <remarks>
        /// The <b>AxisScaleSegment</b> object index can be provided as a parameter. Returns the <see cref="AxisScaleSegment"/> object. 
        /// </remarks>
        [
        SRDescription("DescriptionAttributeAxisScaleSegmentCollection_Item"),
        ]
        public AxisScaleSegment this[int index]
        {
            get
            {
                return (AxisScaleSegment)List[index];
            }
        }

        #endregion // Indexer

        #region Collection Add and Insert methods

        /// <summary>
        /// Adds a segment to the end of the collection.
        /// </summary>
        /// <param name="segment">
        /// <see cref="AxisScaleSegment"/> object to add.
        /// </param>
        /// <returns>
        /// Index of the newly added object.
        /// </returns>
        public int Add(AxisScaleSegment segment)
        {
            return List.Add(segment);
        }


        #endregion // Collection Add and Insert methods

        #region Items Inserting and Removing Notification methods

        /// <summary>
        /// After new item inserted.
        /// </summary>
        /// <param name="index">Item index.</param>
        /// <param name="value">Item object.</param>
        /// <remarks>
        /// This is an internal method and should not be part of the documentation.
        /// </remarks>
        protected override void OnInsertComplete(int index, object value)
        {
            ((AxisScaleSegment)value).axis = _axis;
        }

        /// <summary>
        /// After items is set.
        /// </summary>
        /// <param name="index">The zero-based index at which oldValue can be found.</param>
        /// <param name="oldValue">The value to replace with newValue.</param>
        /// <param name="newValue">The new value of the element at index.</param>
        /// <remarks>
        /// This is an internal method and should not be part of the documentation.
        /// </remarks>
        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            ((AxisScaleSegment)newValue).axis = _axis;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Ensures that specified axis scale segment is used for all coordinate transformations.
        /// Set tot NULL to reset.
        /// </summary>
        /// <param name="segment"></param>
        internal void EnforceSegment(AxisScaleSegment segment)
        {
            _enforcedSegment = segment;
        }

        /// <summary>
        /// Find axis scale segment that should be used to translate axis value to relative coordinates.
        /// </summary>
        /// <param name="axisValue">Axis value to convert.</param>
        /// <returns>Scale segment to use for the convertion.</returns>
        public AxisScaleSegment FindScaleSegmentForAxisValue(double axisValue)
        {
            // Check if no segments defined
            if (List.Count == 0)
            {
                return null;
            }

            // Check if segment enforcment is enabled
            if (_enforcedSegment != null)
            {
                return _enforcedSegment;
            }

            // Iterate through all segments
            for (int index = 0; index < Count; index++)
            {
                if (axisValue < this[index].ScaleMinimum)
                {
                    if (index == 0)
                    {
                        return this[index];
                    }
                    else
                    {
                        // Find the segment which is "closer" to the value
                        if (Math.Abs(this[index].ScaleMinimum - axisValue) < Math.Abs(axisValue - this[index - 1].ScaleMaximum))
                        {
                            return this[index];
                        }
                        else
                        {
                            return this[index - 1];
                        }
                    }
                }

                if (axisValue <= this[index].ScaleMaximum)
                {
                    return this[index];
                }
                else if (index == Count - 1)
                {
                    return this[index];
                }
            }

            return null;
        }

        #endregion // Helper Methods
    }
}

