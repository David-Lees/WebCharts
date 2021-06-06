// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Axis tick marks and grid lines a very similar chart
//              elements and most of the functionality is located
//              in the Grid class. TickMark class is derived from
//              the Grid class and provides tick mark specific
//              functionality.
//

using SkiaSharp;
using System;
using System.Collections.Generic;

namespace WebCharts.Services
{
    #region Tick marks style enumeration

    /// <summary>
    /// An enumeration of tick mark styles.
    /// </summary>
    public enum TickMarkStyle
    {
        /// <summary>
        /// Tickmarks are disabled.
        /// </summary>
        None,

        /// <summary>
        /// Tickmarks are located outside of the chart area.
        /// </summary>
        OutsideArea,

        /// <summary>
        /// Tickmarks are located inside of the chart area.
        /// </summary>
        InsideArea,

        /// <summary>
        /// Tickmarks are set across the axis line.
        /// </summary>
        AcrossAxis
    };

    #endregion Tick marks style enumeration

    /// <summary>
    /// The TickMark class represents axis tick marks which are drawn next to
    /// the axis line. TickMark shares many common properties with the Grid
    /// class. This class also contains methods for tick marks drawing.
    /// </summary>
    [
        SRDescription("DescriptionAttributeTickMark_TickMark"),
    ]
    public class TickMark : Grid
    {
        #region Private fields and Constructors

        // Tick marks style
        private TickMarkStyle _style = TickMarkStyle.OutsideArea;

        // Tick marks size
        private float _size = 1;

        /// <summary>
        /// Public default constructor
        /// </summary>
        public TickMark() : base(null, true)
        {
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="axis">Axis which owns the grid or tick mark.</param>
        /// <param name="major">Major axis element.</param>
		internal TickMark(Axis axis, bool major) : base(axis, major)
        {
        }

        #endregion Private fields and Constructors

        #region Tick marks painting method

        /// <summary>
        /// Draws and hit test for TickMarks.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void Paint(ChartGraphics graph, bool backElements)
        {
            SKPoint first = SKPoint.Empty; // The First point of a tick mark
            SKPoint second = SKPoint.Empty; // The Second point of a tick mark
            float axisPosition; // Axis position.

            // Tick Marks are disabled
            if (!enabled)
            {
                return;
            }

            // ****************************************************************
            // This code creates auto interval for auto tick marks and
            // gridlines. If type is not date there are always four tickmarks
            // or gridlines between major gridlines and tickmarks. For date
            // type interval is calculated using CalcInterval function.
            // ****************************************************************
            double oldInterval = this.interval;
            DateTimeIntervalType oldIntervalType = intervalType;
            double oldIntervalOffset = intervalOffset;
            DateTimeIntervalType oldIntervalOffsetType = intervalOffsetType;
            if (!majorGridTick && (this.interval == 0 || double.IsNaN(this.interval)))
            {
                // Number type
                if (Axis.majorGrid.GetIntervalType() == DateTimeIntervalType.Auto)
                {
                    this.interval = Axis.majorGrid.GetInterval() / Grid.NumberOfIntervals;
                }
                // Date type
                else
                {
                    DateTimeIntervalType localIntervalType = Axis.majorGrid.GetIntervalType();
                    this.interval = Axis.CalcInterval(
                        Axis.ViewMinimum,
                        Axis.ViewMinimum + (Axis.ViewMaximum - Axis.ViewMinimum) / Grid.NumberOfDateTimeIntervals,
                        true,
                        out localIntervalType,
                        ChartValueType.DateTime);
                    intervalType = localIntervalType;
                    intervalOffsetType = Axis.majorGrid.GetIntervalOffsetType();
                    intervalOffset = Axis.majorGrid.GetIntervalOffset();
                }
            }

            if (_style == TickMarkStyle.None)
            {
                return;
            }

            // Check if custom tick marks should be drawn from custom labels
            if (Axis.IsCustomTickMarks())
            {
                PaintCustom(graph, backElements);
                return;
            }

            // Get first series attached to this axis
            Series axisSeries = null;
            if (Axis.axisType == AxisName.X || Axis.axisType == AxisName.X2)
            {
                List<string> seriesArray = Axis.ChartArea.GetXAxesSeries((Axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, Axis.SubAxisName);
                if (seriesArray.Count > 0)
                {
                    axisSeries = Axis.Common.DataManager.Series[seriesArray[0]];
                    if (axisSeries != null && !axisSeries.IsXValueIndexed)
                    {
                        axisSeries = null;
                    }
                }
            }

            // Current position for tick mark is minimum
            double current = Axis.ViewMinimum;

            // Get offse type
            DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();

            // ***********************************
            // Check if the AJAX zooming and scrolling mode is enabled.
            // ***********************************

            // Adjust start position depending on the interval type
            if (!Axis.ChartArea.chartAreaIsCurcular ||
                Axis.axisType == AxisName.Y ||
                Axis.axisType == AxisName.Y2)
            {
                current = ChartHelper.AlignIntervalStart(current, GetInterval(), GetIntervalType(), axisSeries, majorGridTick);
            }

            // The Current position is start position, not minimum
            if (GetIntervalOffset() != 0 && !double.IsNaN(GetIntervalOffset()) && axisSeries == null)
            {
                current += ChartHelper.GetIntervalSize(current, GetIntervalOffset(),
                    offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
            }

            // Too many tick marks
            if ((Axis.ViewMaximum - Axis.ViewMinimum) / ChartHelper.GetIntervalSize(current, GetInterval(), GetIntervalType(), axisSeries, 0, DateTimeIntervalType.Number, true) > ChartHelper.MaxNumOfGridlines)
            {
                return;
            }

            // If Maximum, minimum and interval don’t have
            // proper value do not draw tick marks.
            if (Axis.ViewMaximum <= Axis.ViewMinimum)
            {
                return;
            }

            // Left tickmarks
            if (Axis.AxisPosition == AxisPosition.Left)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.X;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.X = axisPosition;
                    second.X = axisPosition + _size;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.X = axisPosition - _size;
                    second.X = axisPosition;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.X = axisPosition - _size / 2;
                    second.X = axisPosition + _size / 2;
                }
            }

            // Right tickmarks
            else if (Axis.AxisPosition == AxisPosition.Right)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.Right;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.X = axisPosition - _size;
                    second.X = axisPosition;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.X = axisPosition;
                    second.X = axisPosition + _size;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.X = axisPosition - _size / 2;
                    second.X = axisPosition + _size / 2;
                }
            }

            // Top tickmarks
            else if (Axis.AxisPosition == AxisPosition.Top)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.Y;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.Y = axisPosition;
                    second.Y = axisPosition + _size;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.Y = axisPosition - _size;
                    second.Y = axisPosition;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.Y = axisPosition - _size / 2;
                    second.Y = axisPosition + _size / 2;
                }
            }

            // Bottom tickmarks
            else if (Axis.AxisPosition == AxisPosition.Bottom)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.Bottom;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.Y = axisPosition - _size;
                    second.Y = axisPosition;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.Y = axisPosition;
                    second.Y = axisPosition + _size;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.Y = axisPosition - _size / 2;
                    second.Y = axisPosition + _size / 2;
                }
            }

            // Loop for drawing grid tick marks
            int counter = 0;
            int logStep = 1;
            double oldCurrent = current;
            double interval = 0;
            while (current <= Axis.ViewMaximum)
            {
                double logInterval = 0;

                // Take an interval between gridlines. Interval
                // depends on interval type.
                if (majorGridTick || !Axis.IsLogarithmic)
                {
                    // Take an interval between tickmarks. Interval
                    // depends on interval type.
                    interval = ChartHelper.GetIntervalSize(current, GetInterval(), GetIntervalType(), axisSeries, GetIntervalOffset(), offsetType, true);
                }
                // Code for linear minor gridlines and tickmarks
                // if scale is logarithmic.
                else
                {
                    // This code is used only for logarithmic scale and minor tick marks or
                    // gridlines which have linear minor scale in logarithmic major scale.
                    // This code is used to find minimum value for the interval. For example
                    // if logarithmic base is 2 and interval is between 4 and 8; current value
                    // is 5.6; this method will return linearised value for 4. This code works
                    // like Math.Floor for logarithmic scale.
                    double logMinimum = GetLogMinimum(current, axisSeries);

                    if (oldCurrent != logMinimum)
                    {
                        oldCurrent = logMinimum;
                        logStep = 1;
                    }

                    // Find interval for logarithmic linearised scale
                    logInterval = Math.Log(1 + this.interval * logStep, Axis.logarithmBase);

                    current = oldCurrent;

                    interval = logInterval;

                    logStep++;

                    // Reset current position if major interval is passed.
                    if (GetLogMinimum(current + logInterval, axisSeries) != logMinimum)
                    {
                        current += logInterval;
                        continue;
                    }
                }

                // For indexed series do not draw the last tickmark
                if (current == Axis.ViewMaximum && axisSeries != null)
                {
                    current += interval;

                    continue;
                }

                // Check interval size
                if (interval == 0)
                {
                    throw (new InvalidOperationException(SR.ExceptionTickMarksIntervalIsZero));
                }

                // Check if we do not exceed max number of elements
                if (counter++ > ChartHelper.MaxNumOfGridlines)
                {
                    break;
                }

                // Do not draw the very first tick mark for circular chart area
                if (Axis != null && Axis.ChartArea != null && Axis.ChartArea.chartAreaIsCurcular &&
                        ((!Axis.IsReversed && current == Axis.ViewMinimum) ||
                        (Axis.IsReversed && current == Axis.ViewMaximum)))
                {
                    current += interval;

                    continue;
                }

                if (!majorGridTick && Axis.IsLogarithmic)
                {
                    current += logInterval;

                    if (current > Axis.ViewMaximum)
                    {
                        break;
                    }
                }

                if ((decimal)current >= (decimal)Axis.ViewMinimum)
                {
                    // Left tickmarks
                    // Right tickmarks
                    if (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right)
                    {
                        first.Y = (float)Axis.GetLinearPosition(current);
                        second.Y = first.Y;
                    }

                    // Top tickmarks
                    // Bottom tickmarks
                    else if (Axis.AxisPosition == AxisPosition.Top || Axis.AxisPosition == AxisPosition.Bottom)
                    {
                        first.X = (float)Axis.GetLinearPosition(current);
                        second.X = first.X;
                    }

                    if (Axis.Common.ProcessModeRegions)
                    {
                        if (Axis.ChartArea.chartAreaIsCurcular)
                        {
                            SKRect rect = new(first.X - 0.5f, first.Y - 0.5f, first.X + Math.Abs(second.X - first.X) + 1, first.Y + Math.Abs(second.Y - first.Y) + 1);
                            using SKPath path = new();
                            path.AddRect(graph.GetAbsoluteRectangle(rect));
                            path.Transform(graph.Transform);
                            Axis.Common.HotRegionsList.AddHotRegion(
                                path,
                                false,
                                ChartElementType.TickMarks,
                                this);
                        }
                        else if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                        {
                            SKRect rect = new(first.X - 0.5f, first.Y - 0.5f, first.X - +Math.Abs(second.X - first.X) + 1, first.Y + Math.Abs(second.Y - first.Y) + 1);

                            Axis.Common.HotRegionsList.AddHotRegion(rect, this, ChartElementType.TickMarks, true);
                        }
                        else
                        {
                            if (!Axis.Common.ProcessModePaint) //if ProcessModePaint is true it will be called later
                                Draw3DTickLine(graph, first, second, backElements);
                        }
                    }

                    if (Axis.Common.ProcessModePaint)
                    {
                        // Draw grid line
                        if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                        {
                            graph.DrawLineRel(borderColor, borderWidth, borderDashStyle, first, second);
                        }
                        else
                        {
                            Draw3DTickLine(graph, first, second, backElements);
                        }
                    }
                }

                // Move position
                if (majorGridTick || !Axis.IsLogarithmic)
                {
                    current += interval;
                }
            }

            // Used for auto interval for auto tick marks and
            // gridlines
            if (!majorGridTick)
            {
                this.interval = oldInterval;
                intervalType = oldIntervalType;
                intervalOffset = oldIntervalOffset;
                intervalOffsetType = oldIntervalOffsetType;
            }
        }

        /// <summary>
        /// This method returns linearized logarithmic value
        /// which is minimum for range with interval 1.
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="axisSeries">First series attached to axis.</param>
        /// <returns>Returns Minimum for the range which contains current value</returns>
        private double GetLogMinimum(double current, Series axisSeries)
        {
            double viewMinimum = Axis.ViewMinimum;
            DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();
            if (GetIntervalOffset() != 0 && axisSeries == null)
            {
                viewMinimum += ChartHelper.GetIntervalSize(viewMinimum, GetIntervalOffset(),
                    offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
            }

            return viewMinimum + Math.Floor((current - viewMinimum));
        }

        /// <summary>
        /// Draws and hit test for custom TickMarks from the custom labels collection.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void PaintCustom(ChartGraphics graph, bool backElements)
        {
            SKPoint first = SKPoint.Empty;  // The First point of a tick mark
            SKPoint second = SKPoint.Empty; // The Second point of a tick mark
            float axisPosition;             // Axis position.

            // Axis scroll bar will increase size of the Outside and Cross style tick marks
            float scrollBarSize = 0;

            // Left tickmarks
            if (Axis.AxisPosition == AxisPosition.Left)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.X;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.X = axisPosition;
                    second.X = axisPosition + _size;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.X = axisPosition - _size - scrollBarSize;
                    second.X = axisPosition;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.X = axisPosition - _size / 2 - scrollBarSize;
                    second.X = axisPosition + _size / 2;
                }
            }

            // Right tickmarks
            else if (Axis.AxisPosition == AxisPosition.Right)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.Right;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.X = axisPosition - _size;
                    second.X = axisPosition;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.X = axisPosition;
                    second.X = axisPosition + _size + scrollBarSize;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.X = axisPosition - _size / 2;
                    second.X = axisPosition + _size / 2 + scrollBarSize;
                }
            }

            // Top tickmarks
            else if (Axis.AxisPosition == AxisPosition.Top)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.Y;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.Y = axisPosition;
                    second.Y = axisPosition + _size;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.Y = axisPosition - _size - scrollBarSize;
                    second.Y = axisPosition;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.Y = axisPosition - _size / 2 - scrollBarSize;
                    second.Y = axisPosition + _size / 2;
                }
            }

            // Bottom tickmarks
            else if (Axis.AxisPosition == AxisPosition.Bottom)
            {
                // The tick marks will follow axis or they will
                // be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
                else
                    axisPosition = Axis.PlotAreaPosition.Bottom;

                if (_style == TickMarkStyle.InsideArea)
                {
                    first.Y = axisPosition - _size;
                    second.Y = axisPosition;
                }
                else if (_style == TickMarkStyle.OutsideArea)
                {
                    first.Y = axisPosition;
                    second.Y = axisPosition + _size + scrollBarSize;
                }
                else if (_style == TickMarkStyle.AcrossAxis)
                {
                    first.Y = axisPosition - _size / 2;
                    second.Y = axisPosition + _size / 2 + scrollBarSize;
                }
            }

            // Loop through all custom labels
            foreach (CustomLabel label in Axis.CustomLabels)
            {
                if ((label.GridTicks & GridTickTypes.TickMark) == GridTickTypes.TickMark)
                {
                    double position = (label.ToPosition + label.FromPosition) / 2.0;
                    if (position >= Axis.ViewMinimum && position <= Axis.ViewMaximum)
                    {
                        // Left tickmarks
                        // Right tickmarks
                        if (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right)
                        {
                            first.Y = (float)Axis.GetLinearPosition(position);
                            second.Y = first.Y;
                        }

                        // Top tickmarks
                        // Bottom tickmarks
                        else if (Axis.AxisPosition == AxisPosition.Top || Axis.AxisPosition == AxisPosition.Bottom)
                        {
                            first.X = (float)Axis.GetLinearPosition(position);
                            second.X = first.X;
                        }

                        if (Axis.Common.ProcessModeRegions)
                        {
                            if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                            {
                                SKRect rect = new(first.X - 0.5f, first.Y - 0.5f, Math.Abs(second.X - first.X) + 1, Math.Abs(second.Y - first.Y) + 1);

                                Axis.Common.HotRegionsList.AddHotRegion(rect, this, ChartElementType.TickMarks, true);
                            }
                            else
                            {
                                Draw3DTickLine(graph, first, second, backElements);
                            }
                        }

                        if (Axis.Common.ProcessModePaint)
                        {
                            // Draw grid line
                            if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                            {
                                graph.DrawLineRel(borderColor, borderWidth, borderDashStyle, first, second);
                            }
                            else
                            {
                                Draw3DTickLine(graph, first, second, backElements);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws tick mark line in 3D space.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="point1">First line point.</param>
        /// <param name="point2">Second line point.</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void Draw3DTickLine(
            ChartGraphics graph,
            SKPoint point1,
            SKPoint point2,
            bool backElements
            )
        {
            ChartArea area = Axis.ChartArea;

            //*****************************************************************
            //** Set the tick marks line depth
            //*****************************************************************
            float wallZPosition = Axis.GetMarksZPosition(out bool axisOnEdge);

            //*****************************************************************
            //** Check if tick mark should be drawn as back or front element
            //*****************************************************************

            // Check if axis tick marks are drawn inside plotting area
            bool tickMarksOnEdge = axisOnEdge;
            if (tickMarksOnEdge &&
                Axis.MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                Axis.MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea ||
                Axis.MinorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                Axis.MinorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
            {
                tickMarksOnEdge = false;
            }

            SurfaceNames surfaceName = (wallZPosition == 0f) ? SurfaceNames.Back : SurfaceNames.Front;
            if (!area.ShouldDrawOnSurface(surfaceName, backElements, tickMarksOnEdge))
            {
                // Skip drawing
                return;
            }

            //*****************************************************************
            //** Add area scene wall width to the length of the tick mark
            //*****************************************************************
            if (Axis.AxisPosition == AxisPosition.Bottom &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge) &&
                area.IsBottomSceneWallVisible())
            {
                point2.Y += area.areaSceneWallWidth.Height;
            }
            else if (Axis.AxisPosition == AxisPosition.Left &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge) &&
                area.IsSideSceneWallOnLeft())
            {
                point1.X -= area.areaSceneWallWidth.Width;
            }
            else if (Axis.AxisPosition == AxisPosition.Right &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge) &&
                !area.IsSideSceneWallOnLeft())
            {
                point2.X += area.areaSceneWallWidth.Width;
            }
            else if (Axis.AxisPosition == AxisPosition.Top &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge))
            {
                point1.Y -= area.areaSceneWallWidth.Height;
            }

            //*****************************************************************
            //** Adjust grid line direction for the Top axis
            //*****************************************************************
            Point3D point3 = null, point4 = null;
            if (axisOnEdge && area.areaSceneWallWidth.Width != 0f)
            {
                if (Axis.AxisPosition == AxisPosition.Top)
                {
                    // Always use plot area position to draw tick mark
                    float axisPosition = Axis.PlotAreaPosition.Y;

                    if (_style == TickMarkStyle.InsideArea)
                    {
                        point1.Y = axisPosition;
                        point2.Y = axisPosition + _size;

                        point3 = new Point3D(point1.X, point1.Y, -area.areaSceneWallWidth.Width);
                        point4 = new Point3D(point1.X, point1.Y, 0f);
                    }
                    else if (_style == TickMarkStyle.OutsideArea)
                    {
                        point1.Y = axisPosition;
                        point2.Y = axisPosition;

                        point3 = new Point3D(point1.X, axisPosition, wallZPosition);
                        point4 = new Point3D(point1.X, point1.Y, -_size - area.areaSceneWallWidth.Width);
                    }
                    else if (_style == TickMarkStyle.AcrossAxis)
                    {
                        point1.Y = axisPosition;
                        point2.Y = axisPosition + _size / 2;

                        point3 = new Point3D(point1.X, axisPosition, wallZPosition);
                        point4 = new Point3D(point1.X, point1.Y, -_size / 2 - area.areaSceneWallWidth.Width);
                    }

                    // Do not show "bent" tick marks on the top surface
                    if (area.ShouldDrawOnSurface(SurfaceNames.Top, backElements, false))
                    {
                        point3 = null;
                        point4 = null;
                    }
                }

                //*****************************************************************
                //** Adjust grid line direction for the Left axis
                //*****************************************************************
                if (Axis.AxisPosition == AxisPosition.Left && !area.IsSideSceneWallOnLeft())
                {
                    // Always use plot area position to draw tick mark
                    float axisPosition = Axis.PlotAreaPosition.X;

                    if (_style == TickMarkStyle.InsideArea)
                    {
                        point1.X = axisPosition;
                        point2.X = axisPosition + _size;

                        point3 = new Point3D(point1.X, point1.Y, -area.areaSceneWallWidth.Width);
                        point4 = new Point3D(point1.X, point1.Y, 0f);
                    }
                    else if (_style == TickMarkStyle.OutsideArea)
                    {
                        point1.X = axisPosition;
                        point2.X = axisPosition;

                        point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
                        point4 = new Point3D(axisPosition, point1.Y, -_size - area.areaSceneWallWidth.Width);
                    }
                    else if (_style == TickMarkStyle.AcrossAxis)
                    {
                        point1.X = axisPosition;
                        point2.X = axisPosition + _size / 2;

                        point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
                        point4 = new Point3D(axisPosition, point1.Y, -_size / 2 - area.areaSceneWallWidth.Width);
                    }

                    // Do not show "bent" tick marks on the left surface
                    if (area.ShouldDrawOnSurface(SurfaceNames.Left, backElements, false))
                    {
                        point3 = null;
                        point4 = null;
                    }
                }

                //*****************************************************************
                //** Adjust grid line direction for the Right axis
                //*****************************************************************
                else if (Axis.AxisPosition == AxisPosition.Right && area.IsSideSceneWallOnLeft())
                {
                    // Always use plot area position to draw tick mark
                    float axisPosition = Axis.PlotAreaPosition.Right;

                    if (_style == TickMarkStyle.InsideArea)
                    {
                        point1.X = axisPosition - _size;
                        point2.X = axisPosition;

                        point3 = new Point3D(point2.X, point2.Y, -area.areaSceneWallWidth.Width);
                        point4 = new Point3D(point2.X, point2.Y, 0f);
                    }
                    else if (_style == TickMarkStyle.OutsideArea)
                    {
                        point1.X = axisPosition;
                        point2.X = axisPosition;

                        point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
                        point4 = new Point3D(axisPosition, point1.Y, -_size - area.areaSceneWallWidth.Width);
                    }
                    else if (_style == TickMarkStyle.AcrossAxis)
                    {
                        point1.X = axisPosition - _size / 2;
                        point2.X = axisPosition;

                        point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
                        point4 = new Point3D(axisPosition, point1.Y, -_size / 2 - area.areaSceneWallWidth.Width);
                    }

                    // Do not show "bent" tick marks on the right surface
                    if (area.ShouldDrawOnSurface(SurfaceNames.Right, backElements, false))
                    {
                        point3 = null;
                        point4 = null;
                    }
                }
            }

            //*****************************************************************
            //** Draw tick mark (first line)
            //*****************************************************************
            graph.Draw3DLine(
                area.matrix3D,
                borderColor, borderWidth, borderDashStyle,
                new Point3D(point1.X, point1.Y, wallZPosition),
                new Point3D(point2.X, point2.Y, wallZPosition),
                Axis.Common,
                this,
                ChartElementType.TickMarks
                );

            //*****************************************************************
            //** Draw tick mark (second line)
            //*****************************************************************
            if (point3 != null && point4 != null)
            {
                graph.Draw3DLine(
                    area.matrix3D,
                    borderColor, borderWidth, borderDashStyle,
                    point3,
                    point4,
                    Axis.Common,
                    this,
                    ChartElementType.TickMarks
                    );
            }
        }

        #endregion Tick marks painting method

        #region	TickMark properties

        /// <summary>
        /// Tick mark style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTickMark_Style")
        ]
        public TickMarkStyle TickMarkStyle
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Tick mark size.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeTickMark_Size")
        ]
        public float Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                Invalidate();
            }
        }

        #endregion
    }

    /// <summary>
    /// The Grid class represents axis grid lines which are drawn in the
    /// plotting area. It contains grid interval and visual appearance
    /// properties. This class also contains methods for grid lines drawing.
    /// </summary>
    [
        SRDescription("DescriptionAttributeGrid_Grid")
    ]
    public class Grid
    {
        #region Grid fields and Constructors

        // Reference to the Axis object

        // Flags indicate that interval properties where changed
        internal bool intervalOffsetChanged = false;

        internal bool intervalChanged = false;
        internal bool intervalTypeChanged = false;
        internal bool intervalOffsetTypeChanged = false;

        internal bool enabledChanged = false;

        // Data members, which store properties values
        internal double intervalOffset = 0;

        internal double interval = 0;
        internal DateTimeIntervalType intervalType = DateTimeIntervalType.Auto;
        internal DateTimeIntervalType intervalOffsetType = DateTimeIntervalType.Auto;
        internal SKColor borderColor = SKColors.Black;
        internal int borderWidth = 1;
        internal ChartDashStyle borderDashStyle = ChartDashStyle.Solid;
        internal bool enabled = true;

        // Indicates that object describes Major Tick Mark or Grid Line
        internal bool majorGridTick = false;

        // Common number of intervals on the numeric and date-time axis
        internal const double NumberOfIntervals = 5.0;

        internal const double NumberOfDateTimeIntervals = 4.0;

        /// <summary>
        /// Public default constructor.
        /// </summary>
        public Grid()
        {
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="axis">Axis which owns the grid.</param>
        /// <param name="major">Major axis element.</param>
		internal Grid(Axis axis, bool major)
        {
            Initialize(axis, major);
        }

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="axis">Axis which owns the grid.</param>
        /// <param name="major">Major axis element.</param>
		internal void Initialize(Axis axis, bool major)
        {
            // Minor elements are disabled by default
            if (!enabledChanged &&
                Axis == null &&
                !major)
            {
                enabled = false;
            }

            // If object was first created and populated with data and then added into the axis
            // we need to remember changed values.
            // NOTE: Fixes issue #6237
            if (Axis == null)
            {
                TickMark tickMark = this as TickMark;

                if (interval != 0)
                {
                    if (tickMark != null)
                    {
                        if (major)
                        {
                            axis.tempMajorTickMarkInterval = interval;
                        }
                        else
                        {
                            axis.tempMinorTickMarkInterval = interval;
                        }
                    }
                    else
                    {
                        if (major)
                        {
                            axis.tempMajorGridInterval = interval;
                        }
                        else
                        {
                            axis.tempMinorGridInterval = interval;
                        }
                    }
                }
                if (intervalType != DateTimeIntervalType.Auto)
                {
                    if (tickMark != null)
                    {
                        if (major)
                        {
                            axis.tempTickMarkIntervalType = intervalType;
                        }
                    }
                    else
                    {
                        if (major)
                        {
                            axis.tempGridIntervalType = intervalType;
                        }
                    }
                }
            }

            // Set axis object reference
            Axis = axis;

            // Set a flag if this object represent minor or major tick
            majorGridTick = major;

            //		internal double							interval = 0;
            //		internal DateTimeIntervalType			intervalType = DateTimeIntervalType.Auto;
        }

        #endregion

        #region Grid helper functions

        /// <summary>
        /// Gets axes to which this object attached to
        /// </summary>
        /// <returns>Axis object.</returns>
        internal Axis GetAxis()
        {
            return Axis;
        }

        /// <summary>
		/// Invalidate chart area the axis belong to.
		/// </summary>
		internal void Invalidate()
        {
            if (Axis != null)
            {
                Axis.Invalidate();
            }
        }

        #endregion

        #region Grid lines drawing functions

        /// <summary>
        /// Draws grid lines.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        internal void Paint(ChartGraphics graph)
        {
            // Grids are disabled
            if (!enabled)
            {
                return;
            }

            // Check if custom grid lines should be drawn from custom labels
            if (Axis.IsCustomGridLines())
            {
                PaintCustom(graph);
                return;
            }

            double gridInterval; // Grid interval
            double current; // Current position

            // Get first series attached to this axis
            Series axisSeries = null;
            if (Axis.axisType == AxisName.X || Axis.axisType == AxisName.X2)
            {
                List<string> seriesArray = Axis.ChartArea.GetXAxesSeries((Axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, Axis.SubAxisName);
                if (seriesArray.Count > 0)
                {
                    axisSeries = Axis.Common.DataManager.Series[seriesArray[0]];
                    if (axisSeries != null && !axisSeries.IsXValueIndexed)
                    {
                        axisSeries = null;
                    }
                }
            }

            // ****************************************************************
            // This code creates auto interval for auto tick marks and
            // gridlines. If type is not date there are always four tickmarks
            // or gridlines between major gridlines and tickmarks. For date
            // type interval is calculated using CalcInterval function.
            // ****************************************************************
            double oldInterval = interval;
            DateTimeIntervalType oldIntervalType = intervalType;
            double oldIntervalOffset = intervalOffset;
            DateTimeIntervalType oldIntervalOffsetType = intervalOffsetType;
            if (!majorGridTick && (interval == 0 || double.IsNaN(interval)))
            {
                // Number type
                if (Axis.majorGrid.GetIntervalType() == DateTimeIntervalType.Auto)
                {
                    interval = Axis.majorGrid.GetInterval() / Grid.NumberOfIntervals;
                }
                // Date type
                else
                {
                    DateTimeIntervalType localIntervalType = Axis.majorGrid.GetIntervalType();
                    interval = Axis.CalcInterval(
                        Axis.minimum,
                        Axis.minimum + (Axis.maximum - Axis.minimum) / Grid.NumberOfDateTimeIntervals,
                        true,
                        out localIntervalType,
                        ChartValueType.DateTime);
                    intervalType = localIntervalType;
                    intervalOffsetType = Axis.majorGrid.GetIntervalOffsetType();
                    intervalOffset = Axis.majorGrid.GetIntervalOffset();
                }
            }

            // Current position for grid lines is minimum
            current = Axis.ViewMinimum;

            // ***********************************
            // Check if the AJAX zooming and scrolling mode is enabled.
            // ***********************************

            // Adjust start position depending on the interval type
            if (!Axis.ChartArea.chartAreaIsCurcular ||
                Axis.axisType == AxisName.Y ||
                Axis.axisType == AxisName.Y2)
            {
                current = ChartHelper.AlignIntervalStart(current, GetInterval(), GetIntervalType(), axisSeries, majorGridTick);
            }

            // The Current position is start position, not minimum
            DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();
            if (GetIntervalOffset() != 0 && !double.IsNaN(GetIntervalOffset()) && axisSeries == null)
            {
                current += ChartHelper.GetIntervalSize(current, GetIntervalOffset(), offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
            }

            // Too many gridlines
            if ((Axis.ViewMaximum - Axis.ViewMinimum) / ChartHelper.GetIntervalSize(current, GetInterval(), GetIntervalType(), axisSeries, 0, DateTimeIntervalType.Number, true) > ChartHelper.MaxNumOfGridlines)
                return;

            // If Maximum, minimum and interval don’t have
            // proper value do not draw grid lines.
            if (Axis.ViewMaximum <= Axis.ViewMinimum)
                return;

            if (GetInterval() <= 0)
                return;

            // Loop for drawing grid lines
            int counter = 0;
            int logStep = 1;
            double oldCurrent = current;
            decimal viewMaximum = (decimal)Axis.ViewMaximum;
            while ((decimal)current <= viewMaximum)
            {
                // Take an interval between gridlines. Interval
                // depends on interval type.
                if (majorGridTick || !Axis.IsLogarithmic)
                {
                    double autoInterval = GetInterval();

                    gridInterval = ChartHelper.GetIntervalSize(current, autoInterval, GetIntervalType(), axisSeries, GetIntervalOffset(), offsetType, true);

                    // Check interval size
                    if (gridInterval == 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionTickMarksIntervalIsZero));
                    }

                    // Draw between min & max values only
                    if ((decimal)current >= (decimal)Axis.ViewMinimum)
                    {
                        DrawGrid(graph, current);
                    }

                    // Move position
                    current += gridInterval;
                }
                // Code for linear minor gridlines and tickmarks
                // if scale is logarithmic.
                else
                {
                    // This code is used only for logarithmic scale and minor tick marks or
                    // gridlines which have linear minor scale in logarithmic major scale.
                    // This code is used to find minimum value for the interval. For example
                    // if logarithmic base is 2 and interval is between 4 and 8; current value
                    // is 5.6; this method will return linearised value for 4. This code works
                    // like Math.Floor for logarithmic scale.
                    double logMinimum = GetLogMinimum(current, axisSeries);

                    if (oldCurrent != logMinimum)
                    {
                        oldCurrent = logMinimum;
                        logStep = 1;
                    }

                    // Find interval for logarithmic linearised scale
                    double logInterval = Math.Log(1 + interval * logStep, Axis.logarithmBase);

                    current = oldCurrent;

                    // Move position
                    current += logInterval;

                    logStep++;

                    // Reset current position if major interval is passed.
                    if (GetLogMinimum(current, axisSeries) != logMinimum)
                    {
                        continue;
                    }

                    // Check interval size
                    if (logInterval == 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionTickMarksIntervalIsZero));
                    }

                    // Draw between min & max values only
                    if ((decimal)current >= (decimal)Axis.ViewMinimum && (decimal)current <= (decimal)Axis.ViewMaximum)
                    {
                        DrawGrid(graph, current);
                    }
                }

                // Check if we do not exceed max number of elements
                if (counter++ > ChartHelper.MaxNumOfGridlines)
                {
                    break;
                }
            }

            // Used for auto interval for auto tick marks and
            // gridlines
            if (!majorGridTick)
            {
                interval = oldInterval;
                intervalType = oldIntervalType;
                intervalOffset = oldIntervalOffset;
                intervalOffsetType = oldIntervalOffsetType;
            }
        }

        /// <summary>
        /// This method returns linearized logarithmic value
        /// which is minimum for range with interval 1.
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="axisSeries">First series attached to axis.</param>
        /// <returns>Returns Minimum for the range which contains current value</returns>
        private double GetLogMinimum(double current, Series axisSeries)
        {
            double viewMinimum = Axis.ViewMinimum;
            DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();
            if (GetIntervalOffset() != 0 && axisSeries == null)
            {
                viewMinimum += ChartHelper.GetIntervalSize(viewMinimum, GetIntervalOffset(),
                    offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
            }

            return viewMinimum + Math.Floor((current - viewMinimum));
        }

        /// <summary>
        /// Draw the grid line
        /// </summary>
        /// <param name="graph">Chart Graphics object</param>
        /// <param name="current">Current position of the gridline</param>
        private void DrawGrid(ChartGraphics graph, double current)
        {
            // Common elements
            CommonElements common = Axis.Common;

            SKPoint first = SKPoint.Empty; // The First point of a grid line
            SKPoint second = SKPoint.Empty; // The Second point of a grid line
            SKRect plotArea; // Plot area position

            plotArea = Axis.PlotAreaPosition.ToSKRect();

            // Horizontal gridlines
            if (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right)
            {
                first.X = plotArea.Left;
                second.X = plotArea.Right;
                first.Y = (float)Axis.GetLinearPosition(current);
                second.Y = first.Y;
            }

            // Vertical gridlines
            if (Axis.AxisPosition == AxisPosition.Top || Axis.AxisPosition == AxisPosition.Bottom)
            {
                first.Y = plotArea.Top;
                second.Y = plotArea.Bottom;
                first.X = (float)Axis.GetLinearPosition(current);
                second.X = first.X;
            }

            if (common.ProcessModeRegions)
            {
                if (Axis.ChartArea.Area3DStyle.Enable3D && !Axis.ChartArea.chartAreaIsCurcular)
                {
                    if (!common.ProcessModePaint) //if ProcessModePaint is true it will be called later
                        graph.Draw3DGridLine(Axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right), common, this);
                }
                else if (!Axis.ChartArea.chartAreaIsCurcular)
                {
                    using SKPath path = new();
                    if (Math.Abs(first.X - second.X) > Math.Abs(first.Y - second.Y))
                    {
                        path.AddLine(first.X, first.Y - 1, second.X, second.Y - 1);
                        path.AddLine(second.X, second.Y + 1, first.X, first.Y + 1);
                        path.Close();
                    }
                    else
                    {
                        path.AddLine(first.X - 1, first.Y, second.X - 1, second.Y);
                        path.AddLine(second.X + 1, second.Y, first.X + 1, first.Y);
                        path.Close();
                    }
                    common.HotRegionsList.AddHotRegion(path, true, ChartElementType.Gridlines, this);
                }
            }

            if (common.ProcessModePaint)
            {
                // Check if grid lines should be drawn for circular chart area
                if (Axis.ChartArea.chartAreaIsCurcular)
                {
                    if (Axis.axisType == AxisName.Y)
                    {
                        Axis.DrawCircularLine(this, graph, borderColor, borderWidth, borderDashStyle, first.Y);
                    }
                    if (Axis.axisType == AxisName.X)
                    {
                        ICircularChartType chartType = Axis.ChartArea.GetCircularChartType();
                        if (chartType != null && chartType.RadialGridLinesSupported())
                        {
                            Axis.DrawRadialLine(this, graph, borderColor, borderWidth, borderDashStyle, current);
                        }
                    }
                }
                else if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                {
                    var sm = graph.SmoothingMode;
                    graph.SmoothingMode = SmoothingMode.None;
                    graph.DrawLineRel(borderColor, borderWidth, borderDashStyle, first, second);
                    graph.SmoothingMode = sm;
                }
                else
                {
                    graph.Draw3DGridLine(Axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right), Axis.Common, this);
                }
            }
        }

        /// <summary>
        /// Draws custom grid lines from custom labels.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        internal void PaintCustom(ChartGraphics graph)
        {
            // Common Elements
            CommonElements common = Axis.Common;

            SKPoint first = SKPoint.Empty; // The First point of a grid line
            SKPoint second = SKPoint.Empty; // The Second point of a grid line
            SKRect plotArea = Axis.PlotAreaPosition.ToSKRect(); // Plot area position

            // Loop through all custom labels
            foreach (CustomLabel label in Axis.CustomLabels)
            {
                if ((label.GridTicks & GridTickTypes.Gridline) == GridTickTypes.Gridline)
                {
                    double position = (label.ToPosition + label.FromPosition) / 2.0;
                    if (position >= Axis.ViewMinimum && position <= Axis.ViewMaximum)
                    {
                        // Horizontal gridlines
                        if (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right)
                        {
                            first.X = plotArea.Left;
                            second.X = plotArea.Right;
                            first.Y = (float)Axis.GetLinearPosition(position);
                            second.Y = first.Y;
                        }

                        // Vertical gridlines
                        if (Axis.AxisPosition == AxisPosition.Top || Axis.AxisPosition == AxisPosition.Bottom)
                        {
                            first.Y = plotArea.Top;
                            second.Y = plotArea.Bottom;
                            first.X = (float)Axis.GetLinearPosition(position);
                            second.X = first.X;
                        }

                        if (common.ProcessModeRegions)
                        {
                            if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                            {
                                using SKPath path = new();

                                if (Math.Abs(first.X - second.X) > Math.Abs(first.Y - second.Y))
                                {
                                    path.AddLine(first.X, first.Y - 1, second.X, second.Y - 1);
                                    path.AddLine(second.X, second.Y + 1, first.X, first.Y + 1);
                                    path.Close();
                                }
                                else
                                {
                                    path.AddLine(first.X - 1, first.Y, second.X - 1, second.Y);
                                    path.AddLine(second.X + 1, second.Y, first.X + 1, first.Y);
                                    path.Close();
                                }
                                common.HotRegionsList.AddHotRegion(path, true, ChartElementType.Gridlines, this);
                            }
                            else
                            {
                                graph.Draw3DGridLine(Axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right), common, this);
                            }
                        }

                        if (common.ProcessModePaint)
                        {
                            if (!Axis.ChartArea.Area3DStyle.Enable3D || Axis.ChartArea.chartAreaIsCurcular)
                            {
                                graph.DrawLineRel(borderColor, borderWidth, borderDashStyle, first, second);
                            }
                            else
                            {
                                graph.Draw3DGridLine(Axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, (Axis.AxisPosition == AxisPosition.Left || Axis.AxisPosition == AxisPosition.Right), Axis.Common, this);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region	Grid properties

        /// <summary>
        /// Gets or sets grid or tick mark interval offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeIntervalOffset3"),
        ]
        public double IntervalOffset
        {
            get
            {
                return intervalOffset;
            }
            set
            {
                intervalOffset = value;
                intervalOffsetChanged = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Determines if Enabled property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeIntervalOffset()
        {
            if (majorGridTick)
            {
                return !double.IsNaN(intervalOffset);
            }
            return intervalOffset != 0d;
        }

        /// <summary>
        /// Gets the interval offset.
        /// </summary>
        /// <returns></returns>
		internal double GetIntervalOffset()
        {
            if (majorGridTick && double.IsNaN(intervalOffset) && Axis != null)
            {
                return Axis.IntervalOffset;
            }
            return intervalOffset;
        }

        /// <summary>
        /// Gets or sets the unit of measurement of grid or tick mark offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeIntervalOffsetType6"),
        ]
        public DateTimeIntervalType IntervalOffsetType
        {
            get
            {
                return intervalOffsetType;
            }
            set
            {
                intervalOffsetType = value;
                intervalOffsetTypeChanged = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Determines if IntervalOffsetType property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeIntervalOffsetType()
        {
            if (majorGridTick)
            {
                return intervalOffsetType != DateTimeIntervalType.NotSet;
            }
            return intervalOffsetType != DateTimeIntervalType.Auto;
        }

        /// <summary>
        /// Gets the type of the interval offset.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalOffsetType()
        {
            if (majorGridTick && intervalOffsetType == DateTimeIntervalType.NotSet && Axis != null)
            {
                return Axis.IntervalOffsetType;
            }
            return intervalOffsetType;
        }

        /// <summary>
        /// Gets or sets grid or tick mark interval size.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeInterval6"),
        ]
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                // Validation
                if (value < 0.0)
                    throw new ArgumentException(SR.ExceptionTickMarksIntervalIsNegative, nameof(value));

                interval = value;
                intervalChanged = true;

                // Enable minor elements
                if (!majorGridTick && value != 0.0 && !Double.IsNaN(value) && Axis != null && Axis.Chart != null && Axis.Chart.serializing)
                {
                    Enabled = true;
                }

                // Reset original property value fields
                if (Axis != null)
                {
                    if (this is TickMark)
                    {
                        if (majorGridTick)
                        {
                            Axis.tempMajorTickMarkInterval = interval;
                        }
                        else
                        {
                            Axis.tempMinorTickMarkInterval = interval;
                        }
                    }
                    else
                    {
                        if (majorGridTick)
                        {
                            Axis.tempMajorGridInterval = interval;
                        }
                        else
                        {
                            Axis.tempMinorGridInterval = interval;
                        }
                    }
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Determines if IntervalOffsetType property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeInterval()
        {
            if (majorGridTick)
            {
                return !double.IsNaN(interval);
            }
            return interval != 0d;
        }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <returns></returns>
		internal double GetInterval()
        {
            if (majorGridTick && double.IsNaN(interval) && Axis != null)
            {
                return Axis.Interval;
            }
            return interval;
        }

        /// <summary>
        /// Gets or sets the unit of measurement of grid or tick mark interval.
        /// </summary>
        [
        SRCategory("CategoryAttributeData"),
        SRDescription("DescriptionAttributeIntervalType3"),
        ]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return intervalType;
            }
            set
            {
                intervalType = value;
                intervalTypeChanged = true;

                // Reset original property value fields
                if (Axis != null)
                {
                    if (this is TickMark)
                    {
                        Axis.tempTickMarkIntervalType = intervalType;
                    }
                    else
                    {
                        Axis.tempGridIntervalType = intervalType;
                    }
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Determines if IntervalOffsetType property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeIntervalType()
        {
            if (majorGridTick)
            {
                return intervalType != DateTimeIntervalType.NotSet;
            }
            return intervalType != DateTimeIntervalType.Auto;
        }

        /// <summary>
        /// Gets the type of the interval.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalType()
        {
            if (majorGridTick && intervalType == DateTimeIntervalType.NotSet && Axis != null)
            {
                // Return default value during serialization
                return Axis.IntervalType;
            }
            return intervalType;
        }

        /// <summary>
        /// Gets or sets grid or tick mark line color.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineColor"),
        ]
        public SKColor LineColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets grid or tick mark line style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineDashStyle")
        ]
        public ChartDashStyle LineDashStyle
        {
            get
            {
                return borderDashStyle;
            }
            set
            {
                borderDashStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets grid or tick mark line width.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeLineWidth")
        ]
        public int LineWidth
        {
            get
            {
                return borderWidth;
            }
            set
            {
                borderWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates if the grid or tick mark is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeEnabled5")
        ]
        public bool Enabled
        {
            get
            {
                //// Never serialize this property for minor elements
                //// "Disabled" property should be serialized instead.
                //if(this.axis != null && this.axis.IsSerializing())
                //{
                //    if(!this.majorGridTick)
                //    {
                //        // Return default value to prevent serialization
                //        return true;
                //    }
                //}

                return enabled;
            }
            set
            {
                enabled = value;
                enabledChanged = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Determines if Enabled property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeEnabled()
        {
            if (majorGridTick)
            {
                return !Enabled;
            }
            return Enabled;
        }

        /// <summary>
        /// Gets or sets the reference to the Axis object
        /// </summary>
        internal Axis Axis { get; set; } = null;

        #endregion
    }
}