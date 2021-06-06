// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	CustomPropertyRegistry contains information for all
//              chart custom properties. This informatin is used at
//              design-time to provide simple editors for the
//              CustomProperty property of the Series and DataPoint.
// ---------------------------
// Custom Properties Overview:
// ---------------------------
// :
// Different chart types may have unique settings that only apply to
// this chart type. For example, ‘Exploded’ attribute on the data point
// only make sense in case of Pie and Doughnut chart types. Instead of
// adding properties that only will work with specific chart types
// CustomProperties were introduced.
// :
// Custom properties are implemented using the CustomProperties property
// of both Series objects and their associated DataPoint objects. Here
// is an example of setting data point custom attribute:
// :
//   Chart1.Series["Default"].Points[0][CustomPropertyName.Exploded] = "true";
// :
// Custom attribute names are case-sensitive. You should be mindful of
// this fact when formatting custom properties in code-behind. Further,
// if the CustomProperty value contains a comma, then each comma must
// be preceded by a '\' character to escape the comma. This is useful
// when, for example, an RGB color value is set in your application.
// In such cases, the setting of custom properties that contain commas
// can either be done at runtime, or design-time.
//

using System;
using System.Collections;
using System.ComponentModel;

namespace WebCharts.Services
{
    #region Enumerations

    /// <summary>
    /// Circular chart drawing style.
    /// </summary>
    internal enum PolarDrawingStyles
    {
        /// <summary>
        /// Series are drawn as lines.
        /// </summary>
        Line,

        /// <summary>
        /// Series are drawn as markers.
        /// </summary>
        Marker
    }

    /// <summary>
    /// CircularAreaDrawingStyle
    /// </summary>
    internal enum CircularAreaDrawingStyles
    {
        /// <summary>
        /// Drawn as polygon
        /// </summary>
        Polygon,

        /// <summary>
        /// Drawn as circle
        /// </summary>
        Circle = 1,
    }

    /// <summary>
    /// Marker Style
    /// </summary>
    internal enum ErrorBarMarkerStyles
    {
        /// <summary>
        /// Marker disabled
        /// </summary>
        None = 0,

        /// <summary>
        /// The marker style is Square
        /// </summary>
        Square = 1,

        /// <summary>
        /// The marker style is Circle
        /// </summary>
        Circle = 2,

        /// <summary>
        /// The marker style is Diamond
        /// </summary>
        Diamond = 3,

        /// <summary>
        /// The marker style is Triangle
        /// </summary>
        Triangle = 4,

        /// <summary>
        /// The marker style is Cross
        /// </summary>
        Cross = 5,

        /// <summary>
        /// The marker style is 4 corner star
        /// </summary>
        Star4 = 6,

        /// <summary>
        /// The marker style is 5 corner star
        /// </summary>
        Star5 = 7,

        /// <summary>
        /// The marker style is 6 corner star
        /// </summary>
        Star6 = 8,

        /// <summary>
        /// The marker style is 10 corner star
        /// </summary>
        Star10 = 9,

        /// <summary>
        /// Line marker
        /// </summary>
        Line = 10
    };

    /// <summary>
    /// AxisName of stock chart markers
    /// </summary>
    internal enum StockShowOpenCloseTypes
    {
        /// <summary>
        /// Open and Close markers are shown.
        /// </summary>
        Both,

        /// <summary>
        /// Only Open markers are shown.
        /// </summary>
        Open,

        /// <summary>
        /// Only Close markers are shown.
        /// </summary>
        Close,
    }

    /// <summary>
    /// IsEmpty point value attribute
    /// </summary>
    internal enum EmptyPointTypes
    {
        /// <summary>
        /// Average of two neighbor points is used.
        /// </summary>
		Average,

        /// <summary>
        /// Zero value is used
        /// </summary>
        Zero
    }

    /// <summary>
    /// Stock chart point labels attribute
    /// </summary>
    internal enum StockLabelValueTypes
    {
        /// <summary>
        /// High Y value is used to generate point label.
        /// </summary>
        High,

        /// <summary>
        /// Low Y value is used to generate point label.
        /// </summary>
        Low,

        /// <summary>
        /// Open Y value is used to generate point label.
        /// </summary>
        Open,

        /// <summary>
        /// Close Y value is used to generate point label.
        /// </summary>
        Close,
    }

    /// <summary>
    /// Data point label alignment.
    /// </summary>
    [Flags]
    internal enum LabelAlignments
    {
        /// <summary>
        /// Automatic position.
        /// </summary>
        None = 0,

        /// <summary>
        /// Label aligned on the top of the marker.
        /// </summary>
        Top = 1,

        /// <summary>
        /// Label aligned on the bottom of the marker.
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// Label aligned on the right of the marker.
        /// </summary>
        Right = 4,

        /// <summary>
        /// Label aligned on the left of the marker.
        /// </summary>
        Left = 8,

        /// <summary>
        /// Label aligned on the top-left of the marker.
        /// </summary>
        TopLeft = 16,

        /// <summary>
        /// Label aligned on the top-right of the marker.
        /// </summary>
        TopRight = 32,

        /// <summary>
        /// Label aligned on the bottom-left of the marker.
        /// </summary>
        BottomLeft = 64,

        /// <summary>
        /// Label aligned on the bottom-right of the marker.
        /// </summary>
        BottomRight = 128,

        /// <summary>
        /// Label aligned in the center of the marker.
        /// </summary>
        Center = 256,
    }

    #endregion Enumerations

    /// <summary>
    /// CustomPropertyName class contains constant strings defining
    /// names of all custom properties used in the chart.
    /// </summary>
    internal static class CustomPropertyName
    {
        #region Common Custom Properties Names

        internal const string DrawSideBySide = "DrawSideBySide";
        internal const string EmptyPointValue = "EmptyPointValue";
        internal const string IsXAxisQuantitative = "IsXAxisQuantitative";
        internal const string BarLabelStyle = "BarLabelStyle";
        internal const string StackedGroupName = "StackedGroupName";
        internal const string DrawingStyle = "DrawingStyle";
        internal const string PointWidth = "PointWidth";
        internal const string PixelPointWidth = "PixelPointWidth";
        internal const string MinPixelPointWidth = "MinPixelPointWidth";
        internal const string MaxPixelPointWidth = "MaxPixelPointWidth";
        internal const string PriceUpColor = "PriceUpColor";
        internal const string PriceDownColor = "PriceDownColor";
        internal const string LabelValueType = "LabelValueType";
        internal const string OpenCloseStyle = "OpenCloseStyle";
        internal const string ShowOpenClose = "ShowOpenClose";
        internal const string BubbleScaleMin = "BubbleScaleMin";
        internal const string BubbleScaleMax = "BubbleScaleMax";
        internal const string BubbleMaxSize = "BubbleMaxSize";
        internal const string BubbleMinSize = "BubbleMinSize";
        internal const string BubbleUseSKSizeorLabel = "BubbleUseSKSizeorLabel";
        internal const string PieDrawingStyle = "PieDrawingStyle";
        internal const string CollectedStyle = "CollectedStyle";
        internal const string CollectedThreshold = "CollectedThreshold";
        internal const string CollectedThresholdUsePercent = "CollectedThresholdUsePercent";
        internal const string CollectedSliceExploded = "CollectedSliceExploded";
        internal const string CollectedLabel = "CollectedLabel";
        internal const string CollectedLegendText = "CollectedLegendText";
        internal const string CollectedToolTip = "CollectedToolTip";
        internal const string CollectedColor = "CollectedColor";
        internal const string CollectedChartShowLegend = "CollectedChartShowLegend";
        internal const string CollectedChartShowLabels = "CollectedChartShowLabels";
        internal const string PieStartAngle = "PieStartAngle";
        internal const string Exploded = "Exploded";
        internal const string LabelsRadialLineSize = "LabelsRadialLineSize";
        internal const string LabelsHorizontalLineSize = "LabelsHorizontalLineSize";
        internal const string PieLabelStyle = "PieLabelStyle";
        internal const string MinimumRelativePieSize = "MinimumRelativePieSize";
        internal const string _3DLabelLineSize = "3DLabelLineSize";
        internal const string PieLineColor = "PieLineColor";
        internal const string PieAutoAxisLabels = "AutoAxisLabels";
        internal const string DoughnutRadius = "DoughnutRadius";
        internal const string LabelStyle = "LabelStyle";
        internal const string ShowMarkerLines = "ShowMarkerLines";
        internal const string LineTension = "LineTension";
        internal const string PixelPointDepth = "PixelPointDepth";
        internal const string PixelPointGapDepth = "PixelPointGapDepth";
        internal const string PermittedPixelError = "PermittedPixelError";
        internal const string CircularLabelsStyle = "CircularLabelsStyle";
        internal const string PolarDrawingStyle = "PolarDrawingStyle";
        internal const string AreaDrawingStyle = "AreaDrawingStyle";
        internal const string RadarDrawingStyle = "RadarDrawingStyle";
        internal const string BoxPlotPercentile = "BoxPlotPercentile";
        internal const string BoxPlotWhiskerPercentile = "BoxPlotWhiskerPercentile";
        internal const string BoxPlotShowAverage = "BoxPlotShowAverage";
        internal const string BoxPlotShowMedian = "BoxPlotShowMedian";
        internal const string BoxPlotShowUnusualValues = "BoxPlotShowUnusualValues";
        internal const string BoxPlotSeries = "BoxPlotSeries";
        internal const string ErrorBarStyle = "ErrorBarStyle";
        internal const string ErrorBarCenterMarkerStyle = "ErrorBarCenterMarkerStyle";
        internal const string ErrorBarSeries = "ErrorBarSeries";
        internal const string ErrorBarType = "ErrorBarType";
        internal const string UsedYValueHigh = "UsedYValueHigh";
        internal const string UsedYValueLow = "UsedYValueLow";
        internal const string BoxSize = "BoxSize";
        internal const string ProportionalSymbols = "ProportionalSymbols";
        internal const string ReversalAmount = "ReversalAmount";
        internal const string UsedYValue = "UsedYValue";
        internal const string NumberOfLinesInBreak = "NumberOfLinesInBreak";
        internal const string FunnelLabelStyle = "FunnelLabelStyle";
        internal const string FunnelNeckWidth = "FunnelNeckWidth";
        internal const string FunnelNeckHeight = "FunnelNeckHeight";
        internal const string FunnelMinPointHeight = "FunnelMinPointHeight";
        internal const string Funnel3DRotationAngle = "Funnel3DRotationAngle";
        internal const string FunnelPointGap = "FunnelPointGap";
        internal const string Funnel3DDrawingStyle = "Funnel3DDrawingStyle";
        internal const string FunnelStyle = "FunnelStyle";
        internal const string FunnelInsideLabelAlignment = "FunnelInsideLabelAlignment";
        internal const string FunnelOutsideLabelPlacement = "FunnelOutsideLabelPlacement";
        internal const string CalloutLineColor = "CalloutLineColor";
        internal const string PyramidLabelStyle = "PyramidLabelStyle";
        internal const string PyramidMinPointHeight = "PyramidMinPointHeight";
        internal const string Pyramid3DRotationAngle = "Pyramid3DRotationAngle";
        internal const string PyramidPointGap = "PyramidPointGap";
        internal const string Pyramid3DDrawingStyle = "Pyramid3DDrawingStyle";
        internal const string PyramidInsideLabelAlignment = "PyramidInsideLabelAlignment";
        internal const string PyramidOutsideLabelPlacement = "PyramidOutsideLabelPlacement";
        internal const string PyramidValueType = "PyramidValueType";

        #endregion Common Custom Properties Names
    }

    /// <summary>
    /// CustomPropertyRegistry contains information for all chart
    /// custom properties. This data is exposed through the
    /// ‘registeredCustomProperties’ field which is an ArrayList
    /// containing CustomPropertyInfo classes.
    /// </summary>
    internal class CustomPropertyRegistry : IServiceProvider, ICustomPropertyRegistry
    {
        #region Fields

        // List of registered properties
        internal ArrayList registeredCustomProperties = new();

        // Defines maximum value which can be set to the attribute which uses pixels
        internal readonly static int MaxValueOfPixelAttribute = 10000;

        internal readonly static System.Collections.Generic.List<SeriesChartType> IsXAxisQuantitativeChartTypes =
              new(
                  new SeriesChartType[] {
                                        SeriesChartType.Line,
                                        SeriesChartType.FastLine,
                                        SeriesChartType.Spline,
                                        SeriesChartType.Point,
                                        SeriesChartType.FastPoint,
                                        SeriesChartType.Bubble,
                                        SeriesChartType.RangeColumn,
                                        SeriesChartType.RangeBar,
                                    });

        #endregion Fields

        #region Constructor and Services

        /// <summary>
        /// Custom properties registry public constructor.
        /// </summary>
        public CustomPropertyRegistry()
        {
            // Register properties used in the chart
            RegisterProperties();
        }

        /// <summary>
        /// Returns custom properties registry service object.
        /// </summary>
        /// <param name="serviceType">Service type to get.</param>
        /// <returns>Custom properties registry service.</returns>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(CustomPropertyRegistry))
            {
                return this;
            }
            throw (new ArgumentException(SR.ExceptionCustomAttributesRegistryUnsupportedType(serviceType.ToString())));
        }

        #endregion Constructor and Services

        #region Properties Regestering methods

        /// <summary>
        /// This method registers all standard custom properties used in
        /// the chart and provides all the additional information like
        /// description, value validation and scenarios where custom
        /// attribute can be used.
        /// </summary>
        private void RegisterProperties()
        {
            //***********************************************************************
            //** DrawSideBySide properties
            //***********************************************************************
            SeriesChartType[] chartTypes = new SeriesChartType[] {
                                                    SeriesChartType.Bar,
                                                    SeriesChartType.Column,

                                                    SeriesChartType.RangeColumn,
                                                    SeriesChartType.BoxPlot,
                                                    SeriesChartType.RangeBar,
                                                    SeriesChartType.ErrorBar,
                                               };
            // "DrawSideBySide" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.DrawSideBySide,
                typeof(AxisEnabled),
                "Auto",
                SR.DescriptionCustomAttributeDrawSideBySide,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** IsXAxisQuantitative properties
            //***********************************************************************
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.IsXAxisQuantitative,
                typeof(bool),
                "false",
                SR.DescriptionCustomAttributeIsXAxisQuantitive,
                IsXAxisQuantitativeChartTypes.ToArray(),
                true,
                false));

            //***********************************************************************
            //** EmptyPointValue properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Point,
                                                   SeriesChartType.Bubble,
                                                   SeriesChartType.Line,
                                                   SeriesChartType.Spline,
                                                   SeriesChartType.StepLine,
                                                   SeriesChartType.Column,
                                                   SeriesChartType.RangeColumn,
                                                   SeriesChartType.RangeBar,
                                                   SeriesChartType.Radar,
                                                   SeriesChartType.Range,
                                                   SeriesChartType.SplineRange,
                                                   SeriesChartType.Polar,
                                                   SeriesChartType.Area,
                                                   SeriesChartType.SplineArea,
                                                   SeriesChartType.Bar,
                                               };
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.EmptyPointValue,
                typeof(EmptyPointTypes),
                "Average",
                SR.DescriptionCustomAttributeEmptyPointValue,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Bar label styles properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.StackedBar,
                                                   SeriesChartType.StackedBar100,
                                                   SeriesChartType.RangeBar,
                                               };
            // "BarLabelStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BarLabelStyle,
                typeof(BarValueLabelDrawingStyle),
                "Center",
                SR.DescriptionCustomAttributeBarLabelStyle,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Stacked Column/Bar properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.StackedBar,
                                                   SeriesChartType.StackedBar100,
                                                   SeriesChartType.StackedColumn,
                                                   SeriesChartType.StackedColumn100,
            };

            // "StackedGroupName" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.StackedGroupName,
                typeof(string),
                string.Empty,
                SR.DescriptionCustomAttributeStackedGroupName,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Bar label styles properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Bar,
                                               };
            // "BarLabelStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BarLabelStyle,
                typeof(BarValueLabelDrawingStyle),
                "Outside",
                SR.DescriptionCustomAttributeBarLabelStyle,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Bar and Columnt chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Bar,
                                                   SeriesChartType.Column,
                                                   SeriesChartType.StackedBar,
                                                   SeriesChartType.StackedBar100,
                                                   SeriesChartType.StackedColumn,
                                                   SeriesChartType.StackedColumn100,
                                                   SeriesChartType.RangeBar,
                                                   SeriesChartType.RangeColumn,
                                               };
            // "DrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.DrawingStyle,
                typeof(BarDrawingStyle),
                "Default",
                SR.DescriptionCustomAttributeDrawingStyle,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Chart types point width properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Bar,
                                                   SeriesChartType.Candlestick,
                                                   SeriesChartType.Column,
                                                   SeriesChartType.StackedBar,
                                                   SeriesChartType.StackedBar100,
                                                   SeriesChartType.StackedColumn,
                                                   SeriesChartType.StackedColumn100,
                                                   SeriesChartType.Stock,
                                                   SeriesChartType.BoxPlot,
                                                   SeriesChartType.ErrorBar,
                                                   SeriesChartType.RangeBar,
                                                   SeriesChartType.RangeColumn,
                                               };
            // "PointWidth" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PointWidth,
                typeof(float),
                0.8f,
                SR.DescriptionCustomAttributePointWidth,
                chartTypes,
                true,
                false)
            {
                MinValue = 0f,
                MaxValue = 2f
            });

            // "PixelPointWidth" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PixelPointWidth,
                typeof(int),
                0,
                SR.DescriptionCustomAttributePixelPointWidth,
                chartTypes,
                true,
                false)
            {
                MinValue = 0,
                MaxValue = MaxValueOfPixelAttribute
            });

            // "MinPixelPointWidth" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.MinPixelPointWidth,
                typeof(int),
                0,
                SR.DescriptionCustomAttributeMinPixelPointWidth,
                chartTypes,
                true,
                false)
            {
                MinValue = 0,
                MaxValue = MaxValueOfPixelAttribute
            });

            // "MaxPixelPointWidth" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.MaxPixelPointWidth,
                typeof(int),
                0,
                SR.DescriptionCustomAttributeMaxPixelPointWidth,
                chartTypes,
                true,
                false)
            {
                MinValue = 0,
                MaxValue = MaxValueOfPixelAttribute
            });

            //***********************************************************************
            //** CandleStick chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Candlestick
    };

            // "PriceUpColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PriceUpColor,
                        typeof(Color),
                        "",
                SR.DescriptionCustomAttributeCandlePriceUpColor,
                chartTypes,
                        true,
                        true));

            // "PriceDownColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PriceDownColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributePriceDownColor,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Stock and CandleStick chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Stock, SeriesChartType.Candlestick
};

            // "LabelValueType" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.LabelValueType,
                typeof(StockLabelValueTypes),
                "Close",
                SR.DescriptionCustomAttributeLabelValueType,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Stock chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Stock };

            // "OpenCloseStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.OpenCloseStyle,
                typeof(StockOpenCloseMarkStyle),
                "Line",
                SR.DescriptionCustomAttributeOpenCloseStyle,
                chartTypes,
                true,
                true));

            // "ShowOpenClose" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ShowOpenClose,
                typeof(StockShowOpenCloseTypes),
                "Both",
                SR.DescriptionCustomAttributeShowOpenClose,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Bubble chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Bubble };

            // "BubbleScaleMin" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BubbleScaleMin,
                typeof(float),
                0f,
                SR.DescriptionCustomAttributeBubbleScaleMin,
                chartTypes,
                true,
                false));

            // "BubbleScaleMax" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BubbleScaleMax,
                typeof(float),
                0f,
                SR.DescriptionCustomAttributeBubbleScaleMax,
                chartTypes,
                true,
                false));

            // "BubbleMaxSize" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BubbleMaxSize,
                typeof(float),
                15f,
                SR.DescriptionCustomAttributeBubbleMaxSize,
                chartTypes,
                true,
                false)
            {
                MinValue = 0f,
                MaxValue = 100f
            });

            // "BubbleMinSize" attribute of the Bubble chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BubbleMinSize,
                typeof(float),
                3f,
                SR.DescriptionCustomAttributeBubbleMaxSize,
                chartTypes,
                true,
                false)
            {
                MinValue = 0f,
                MaxValue = 100f
            });

            // "BubbleUseSKSizeorLabel" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BubbleUseSKSizeorLabel,
                typeof(bool),
                false,
                SR.DescriptionCustomAttributeBubbleUseSKSizeorLabel,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Pie and Doughnut chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Pie,
                                                   SeriesChartType.Doughnut
                                               };

            // "PieDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PieDrawingStyle,
                typeof(PieDrawingStyle),
                "Default",
                SR.DescriptionCustomAttributePieDrawingStyle,
                chartTypes,
                true,
                false)
            {
                AppliesTo3D = false
            });

            // "CollectedThreshold" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedThreshold,
                typeof(double),
                0.0,
                SR.DescriptionCustomAttributeCollectedThreshold,
                chartTypes,
                true,
                false)
            { MinValue = 0.0, MaxValue = double.MaxValue });

            // "CollectedThresholdUsePercent" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedThresholdUsePercent,
                typeof(bool),
                true,
                SR.DescriptionCustomAttributeCollectedThresholdUsePercent,
                chartTypes,
                true,
                false));

            // "CollectedSliceExploded" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedSliceExploded,
                typeof(bool),
                false,
                SR.DescriptionCustomAttributeCollectedSliceExploded,
                chartTypes,
                true,
                false));

            // "CollectedLabel" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedLabel,
                typeof(string),
                string.Empty,
                SR.DescriptionCustomAttributeCollectedLabel,
                chartTypes,
                true,
                false));

            // "CollectedLegendText" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedLegendText,
                typeof(string),
                SR.DescriptionCustomAttributeCollectedLegendDefaultText,
                SR.DescriptionCustomAttributeCollectedLegendText,
                chartTypes,
                true,
                false));

            // "CollectedToolTip" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedToolTip,
                typeof(string),
                string.Empty,
                SR.DescriptionCustomAttributeCollectedToolTip,
                chartTypes,
                true,
                false));

            // "CollectedColor" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CollectedColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributeCollectedColor,
                chartTypes,
                true,
                false));

            // "PieStartAngle" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PieStartAngle,
                typeof(int),
                0,
                SR.DescriptionCustomAttributePieStartAngle,
                chartTypes,
                true,
                false)
            {
                MinValue = 0,
                MaxValue = 360
            });

            // "Exploded" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.Exploded,
                typeof(bool),
                false,
                SR.DescriptionCustomAttributePieDonutExploded,
                chartTypes,
                false,
                true));

            // "LabelsRadialLineSize" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.LabelsRadialLineSize,
                typeof(float),
                1f,
                SR.DescriptionCustomAttributeLabelsRadialLineSize,
                chartTypes,
                true,
                true)
            { AppliesTo3D = false, MinValue = 0f, MaxValue = 100f });

            // "LabelsHorizontalLineSize" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.LabelsHorizontalLineSize,
                typeof(float),
                1f,
                SR.DescriptionCustomAttributeLabelsHorizontalLineSize,
                chartTypes,
                true,
                true)
            {
                AppliesTo3D = false,
                MinValue = 0f,
                MaxValue = 100f
            });

            // "PieLabelStyle" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PieLabelStyle,
                typeof(PieLabelStyle),
                "Inside",
                SR.DescriptionCustomAttributePieLabelStyle,
                chartTypes,
                true,
                true));

            // "MinimumRelativePieSize" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.MinimumRelativePieSize,
                typeof(float),
                30f,
                SR.DescriptionCustomAttributeMinimumRelativePieSize,
                chartTypes,
                true,
                false)
            { MinValue = 10f, MaxValue = 70f });

            // "3DLabelLineSize" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName._3DLabelLineSize,
                typeof(float),
                100f,
                SR.DescriptionCustomAttribute_3DLabelLineSize,
                chartTypes,
                true,
                false)
            { MaxValue = 200f, MinValue = 30f, AppliesTo2D = false });

            // "PieLineColor" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PieLineColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributePieLineColor,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Doughnut chart types properties
            //***********************************************************************

            // "DoughnutRadius" attribute of the Pie chart
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.DoughnutRadius,
                typeof(float),
                60f,
                SR.DescriptionCustomAttributeDoughnutRadius,
                new SeriesChartType[] { SeriesChartType.Doughnut },
                true,
                false)
            {
                MinValue = 0f,
                MaxValue = 99f
            });

            //***********************************************************************
            //** Other
            //***********************************************************************

            // "LabelStyle" attribute
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Point,
                                                   SeriesChartType.Column,
                                                   SeriesChartType.Bubble,
                                                   SeriesChartType.Line,
                                                   SeriesChartType.Spline,
                                                   SeriesChartType.StepLine,
                                                   SeriesChartType.Area,
                                                   SeriesChartType.SplineArea,
                                                   SeriesChartType.Range,
                                                   SeriesChartType.SplineRange,
                                                   SeriesChartType.Radar,
                                                   SeriesChartType.Polar,
                                               };
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.LabelStyle,
                typeof(LabelAlignments),
                "Auto",
                SR.DescriptionCustomAttributeLabelStyle,
                chartTypes,
                true,
                true));

            // "ShowMarkerLines" attribute
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Line,
                                                   SeriesChartType.Spline,
                                                   SeriesChartType.StepLine,
                                                   SeriesChartType.Area,
                                                   SeriesChartType.SplineArea,
                                                   SeriesChartType.Range,
                                                   SeriesChartType.SplineRange
                                               };

            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ShowMarkerLines,
                typeof(bool),
                false,
                SR.DescriptionCustomAttributeShowMarkerLines,
                chartTypes,
                true,
                true)
            {
                AppliesTo2D = false
            });

            // "LineTension" attribute
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Spline,
                                                   SeriesChartType.SplineArea,
                                                   SeriesChartType.SplineRange
                };

            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.LineTension,
                typeof(float),
                0.5f,
                SR.DescriptionCustomAttributeLineTension,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 2f });

            // "PixelPointDepth" attribute
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Area,
                                                   SeriesChartType.Bar,
                                                   SeriesChartType.Bubble,
                                                   SeriesChartType.Candlestick,
                                                   SeriesChartType.Column,
                                                   SeriesChartType.Line,
                                                   SeriesChartType.Point,
                                                   SeriesChartType.Spline,
                                                   SeriesChartType.SplineArea,
                                                   SeriesChartType.StackedArea,
                                                   SeriesChartType.StackedArea100,
                                                   SeriesChartType.StackedBar,
                                                   SeriesChartType.StackedBar100,
                                                   SeriesChartType.StackedColumn,
                                                   SeriesChartType.StackedColumn100,
                                                   SeriesChartType.StepLine,
                                                   SeriesChartType.Stock,

                                                   SeriesChartType.ThreeLineBreak,
                                                   SeriesChartType.BoxPlot,
                                                   SeriesChartType.ErrorBar,
                                                   SeriesChartType.RangeBar,
                                                   SeriesChartType.Kagi,
                                                   SeriesChartType.PointAndFigure,
                                                   SeriesChartType.Range,
                                                   SeriesChartType.RangeColumn,
                                                   SeriesChartType.Renko,
                                                   SeriesChartType.SplineRange,
                                                   SeriesChartType.FastLine,
                                                };

            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PixelPointDepth,
                typeof(int),
                0,
                SR.DescriptionCustomAttributePixelPointDepth,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = MaxValueOfPixelAttribute, AppliesTo2D = false });

            // "PixelPointGapDepth" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PixelPointGapDepth,
                typeof(int),
                0,
                SR.DescriptionCustomAttributePixelPointGapDepth,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = MaxValueOfPixelAttribute, AppliesTo2D = false });

            //***********************************************************************
            //** FastLine chart type properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                    SeriesChartType.FastLine,
                                                    SeriesChartType.FastPoint,
                                                   SeriesChartType.Polar
                                               };
            // "AreaDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.AreaDrawingStyle,
                typeof(CircularAreaDrawingStyles),
                "Circle",
                SR.DescriptionCustomAttributePolarAreaDrawingStyle,
                chartTypes,
                true,
                false));

            // "CircularLabelsStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CircularLabelsStyle,
                typeof(CircularAxisLabelsStyle),
                "Auto",
                SR.DescriptionCustomAttributePolarCircularLabelsStyle,
                chartTypes,
                true,
                false));

            // "PolarDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PolarDrawingStyle,
                typeof(PolarDrawingStyles),
                "Line",
                SR.DescriptionCustomAttributePolarDrawingStyle,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Radar chart type properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.Radar
                                               };
            // "AreaDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.AreaDrawingStyle,
                typeof(CircularAreaDrawingStyles),
                "Circle",
                SR.DescriptionCustomAttributeRadarAreaDrawingStyle,
                chartTypes,
                true,
                false));

            // "CircularLabelsStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CircularLabelsStyle,
                typeof(CircularAxisLabelsStyle),
                "Auto",
                SR.DescriptionCustomAttributeRadarCircularLabelsStyle,
                chartTypes,
                true,
                false));

            // "RadarDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.RadarDrawingStyle,
                typeof(RadarDrawingStyle),
                "Area",
                SR.DescriptionCustomAttributeRadarDrawingStyle,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** BoxPlot chart type properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.BoxPlot
                                               };
            // "BoxPlotPercentile" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxPlotPercentile,
                typeof(float),
                25f,
                SR.DescriptionCustomAttributeBoxPlotPercentile,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 1000f });

            // "BoxPlotWhiskerPercentile" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxPlotWhiskerPercentile,
                typeof(float),
                10f,
                SR.DescriptionCustomAttributeBoxPlotWhiskerPercentile,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 1000f });

            // "BoxPlotShowAverage" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxPlotShowAverage,
                typeof(bool),
                true,
                SR.DescriptionCustomAttributeBoxPlotShowAverage,
                chartTypes,
                true,
                false));

            // "BoxPlotShowMedian" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxPlotShowMedian,
                typeof(bool),
                true,
                SR.DescriptionCustomAttributeBoxPlotShowMedian,
                chartTypes,
                true,
                false));

            // "BoxPlotShowUnusualValues" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxPlotShowUnusualValues,
                typeof(bool),
                false,
                SR.DescriptionCustomAttributeBoxPlotShowUnusualValues,
                chartTypes,
                true,
                false));

            // "BoxPlotSeries" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxPlotSeries,
                typeof(string),
                "",
                SR.DescriptionCustomAttributeBoxPlotSeries,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** ErrorBar chart type properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] {
                                                   SeriesChartType.ErrorBar
                                               };
            // "ErrorBarStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ErrorBarStyle,
                typeof(ErrorBarStyle),
                "Both",
                SR.DescriptionCustomAttributeErrorBarStyle,
                chartTypes,
                true,
                true));

            // "ErrorBarCenterMarkerStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ErrorBarCenterMarkerStyle,
                typeof(ErrorBarMarkerStyles),
                "Line",
                SR.DescriptionCustomAttributeErrorBarCenterMarkerStyle,
                chartTypes,
                true,
                true));

            // "ErrorBarSeries" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ErrorBarSeries,
                typeof(string),
                "",
                SR.DescriptionCustomAttributeErrorBarSeries,
                chartTypes,
                true,
                false));

            // "ErrorBarType" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ErrorBarType,
                typeof(string),
                String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1:N0})", ErrorBarType.StandardError, ErrorBarChart.DefaultErrorBarTypeValue(ErrorBarType.StandardError)),
                SR.DescriptionCustomAttributeErrorBarType,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** PointAndFigure chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.PointAndFigure };

            // "UsedYValueHigh" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.UsedYValueHigh,
                typeof(int),
                0,
                SR.DescriptionCustomAttributeUsedYValueHigh,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = 20 });

            // "UsedYValueLow" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.UsedYValueLow,
                typeof(int),
                1,
                SR.DescriptionCustomAttributeUsedYValueLow,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = 20 });

            // "PriceUpColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PriceUpColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributeBarsPriceUpColor,
                chartTypes,
                true,
                false));

            // "BoxSize" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxSize,
                typeof(string),
                "4%",
                SR.DescriptionCustomAttributeSKPointigureBoxSize,
                chartTypes,
                true,
                false));

            // "ProportionalSymbols" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ProportionalSymbols,
                typeof(bool),
                true,
                SR.DescriptionCustomAttributeProportionalSymbols,
                chartTypes,
                true,
                false));

            // "ReversalAmount" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ReversalAmount,
                typeof(int),
                "3",
                SR.DescriptionCustomAttributeReversalAmount,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Kagi chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Kagi };

            // "UsedYValue" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.UsedYValue,
                typeof(int),
                0,
                SR.DescriptionCustomAttributeUsedYValue,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = 20 });

            // "PriceUpColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PriceUpColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributeBarsPriceUpColor,
                chartTypes,
                true,
                false));

            // "ReversalAmount" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.ReversalAmount,
                typeof(string),
                "3%",
                SR.DescriptionCustomAttributeKagiReversalAmount,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Renko chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Renko };

            // "UsedYValue" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.UsedYValue,
                typeof(int),
                0,
                SR.DescriptionCustomAttributeRenkoUsedYValue,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = 20 });

            // "PriceUpColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PriceUpColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributeBarsPriceUpColor,
                chartTypes,
                true,
                false));

            // "BoxSize" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.BoxSize,
                typeof(string),
                "4%",
                SR.DescriptionCustomAttributeBoxSize,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** ThreeLineBreak chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.ThreeLineBreak };

            // "UsedYValue" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.UsedYValue,
                typeof(int),
                0,
                SR.DescriptionCustomAttributeThreeLineBreakUsedYValue,
                chartTypes,
                true,
                false)
            { MinValue = 0, MaxValue = 20 });

            // "PriceUpColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PriceUpColor,
                typeof(Color),
                "",
                SR.DescriptionCustomAttributeBarsPriceUpColor,
                chartTypes,
                true,
                false));

            // "NumberOfLinesInBreak" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.NumberOfLinesInBreak,
                typeof(int),
                3,
                SR.DescriptionCustomAttributeNumberOfLinesInBreak,
                chartTypes,
                true,
                false));

            //***********************************************************************
            //** Funnel chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Funnel };

            // "FunnelLabelStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelLabelStyle,
                typeof(FunnelLabelStyle),
                "OutsideInColumn",
                SR.DescriptionCustomAttributeFunnelLabelStyle,
                chartTypes,
                true,
                true));

            // "FunnelNeckWidth" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelNeckWidth,
                typeof(float),
                5f,
                SR.DescriptionCustomAttributeFunnelNeckWidth,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 100f });

            // "FunnelNeckHeight" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelNeckHeight,
                typeof(float),
                5f,
                SR.DescriptionCustomAttributeFunnelNeckHeight,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 100f });

            // "FunnelMinPointHeight" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelMinPointHeight,
                typeof(float),
                0f,
                SR.DescriptionCustomAttributeFunnelMinPointHeight,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 100f });

            // "Funnel3DRotationAngle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.Funnel3DRotationAngle,
                typeof(float),
                5f,
                SR.DescriptionCustomAttributeFunnel3DRotationAngle,
                chartTypes,
                true,
                false)
            { AppliesTo2D = false, MinValue = -10f, MaxValue = 10f });

            // "FunnelPointGap" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelPointGap,
                typeof(float),
                0f,
                SR.DescriptionCustomAttributeFunnelPointGap,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 100f });

            // "Funnel3DDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.Funnel3DDrawingStyle,
                typeof(Funnel3DDrawingStyle),
                "CircularBase",
                SR.DescriptionCustomAttributeFunnel3DDrawingStyle,
                chartTypes,
                true,
                false)
            { AppliesTo2D = false });

            // "FunnelStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelStyle,
                typeof(FunnelStyle),
                "YIsHeight",
                SR.DescriptionCustomAttributeFunnelStyle,
                chartTypes,
                true,
                false));

            // "FunnelInsideLabelAlignment" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelInsideLabelAlignment,
                typeof(FunnelLabelVerticalAlignment),
                "Center",
                SR.DescriptionCustomAttributeFunnelInsideLabelAlignment,
                chartTypes,
                true,
                true));

            // "FunnelOutsideLabelPlacement" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.FunnelOutsideLabelPlacement,
                typeof(FunnelLabelPlacement),
                "Right",
                SR.DescriptionCustomAttributeFunnelOutsideLabelPlacement,
                chartTypes,
                true,
                true));

            // "CalloutLineColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CalloutLineColor,
                typeof(Color),
                "Black",
                SR.DescriptionCustomAttributeCalloutLineColor,
                chartTypes,
                true,
                true));

            //***********************************************************************
            //** Pyramid chart types properties
            //***********************************************************************
            chartTypes = new SeriesChartType[] { SeriesChartType.Pyramid };

            // "PyramidLabelStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PyramidLabelStyle,
                typeof(FunnelLabelStyle),
                "OutsideInColumn",
                SR.DescriptionCustomAttributePyramidLabelStyle,
                chartTypes,
                true,
                true));

            // "PyramidMinPointHeight" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PyramidMinPointHeight,
                typeof(float),
                0f,
                SR.DescriptionCustomAttributePyramidMinPointHeight,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 100f });

            // "Pyramid3DRotationAngle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.Pyramid3DRotationAngle,
                typeof(float),
                5f,
                SR.DescriptionCustomAttributePyramid3DRotationAngle,
                chartTypes,
                true,
                false)
            { AppliesTo2D = false, MinValue = -10f, MaxValue = 10f });

            // "PyramidPointGap" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PyramidPointGap,
                typeof(float),
                0f,
                SR.DescriptionCustomAttributePyramidPointGap,
                chartTypes,
                true,
                false)
            { MinValue = 0f, MaxValue = 100f });

            // "Pyramid3DDrawingStyle" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.Pyramid3DDrawingStyle,
                typeof(Funnel3DDrawingStyle),
                "SquareBase",
                SR.DescriptionCustomAttributePyramid3DDrawingStyle,
                chartTypes,
                true,
                false)
            { AppliesTo2D = false });

            // "PyramidInsideLabelAlignment" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PyramidInsideLabelAlignment,
                typeof(FunnelLabelVerticalAlignment),
                "Center",
                SR.DescriptionCustomAttributePyramidInsideLabelAlignment,
                chartTypes,
                true,
                true));

            // "PyramidOutsideLabelPlacement" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PyramidOutsideLabelPlacement,
                typeof(FunnelLabelPlacement),
                "Right",
                SR.DescriptionCustomAttributePyramidOutsideLabelPlacement,
                chartTypes,
                true,
                true));

            // "CalloutLineColor" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.CalloutLineColor,
                typeof(Color),
                "Black",
                SR.DescriptionCustomAttributeCalloutLineColor,
                chartTypes,
                true,
                true));

            // "PyramidValueType" attribute
            registeredCustomProperties.Add(new CustomPropertyInfo(
                CustomPropertyName.PyramidValueType,
                typeof(PyramidValueType),
                "Linear",
                SR.DescriptionCustomAttributePyramidValueType,
                chartTypes,
                true,
                false));
        }

        #endregion Properties Regestering methods

        #region Registry methods

        /// <summary>
        /// Adds custom attribute information into the registry.
        /// </summary>
        /// <param name="customPropertyInfo">Custom attribute information.</param>
        public void Register(CustomPropertyInfo customPropertyInfo)
        {
            // Add custom attribute information to the hash table
            registeredCustomProperties.Add(customPropertyInfo);
        }

        #endregion Registry methods
    }

    /// <summary>
    /// CustomPropertyInfo class stores information about single
    /// custom attribute. It includes Name, Description, Default
    /// Value, any restrictions and the conditions when it can
    /// be used.
    ///
    /// Most of the custom attribute can only be used when specific
    /// chart type is selected. Some of the properties only work
    /// in 2D or 3D mode and some can be applied to the whole
    /// series or data points only.
    /// </summary>
    internal class CustomPropertyInfo
    {
        #region Public Fields

        /// <summary>
        /// Attribute name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Attribute value type.
        /// </summary>
        public Type ValueType;

        /// <summary>
        /// Attribute default value.
        /// </summary>
        public object DefaultValue = null;

        /// <summary>
        /// Attribute description.
        /// </summary>
        public string Description;

        /// <summary>
        /// Array of chart type supported by the attribute
        /// </summary>
        public SeriesChartType[] AppliesToChartType = null;

        /// <summary>
        /// Indicates that attribute can be applied on series.
        /// </summary>
        public bool AppliesToSeries;

        /// <summary>
        /// Indicates that attribute can be applied on data point.
        /// </summary>
        public bool AppliesToDataPoint;

        /// <summary>
        /// Indicates that attribute can be applied on 3D chart type.
        /// </summary>
        public bool AppliesTo3D = true;

        /// <summary>
        /// Indicates that attribute can be applied on 2D chart type.
        /// </summary>
        public bool AppliesTo2D = true;

        /// <summary>
        /// Attribute minimum value.
        /// </summary>
        public object MinValue = null;

        /// <summary>
        /// Attribute maximum value.
        /// </summary>
        public object MaxValue = null;

        #endregion Public Fields

        #region Constructor

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="valueType">Attribute value type.</param>
        /// <param name="defaultValue">Attribute default value.</param>
        /// <param name="description">Attribute description.</param>
        /// <param name="appliesToChartType">Array of chart types where attribute used.</param>
        /// <param name="appliesToSeries">True if properties can be set in series.</param>
        /// <param name="appliesToDataPoint">True if properties can be set in data point.</param>
        public CustomPropertyInfo(
            string name,
            Type valueType,
            object defaultValue,
            string description,
            SeriesChartType[] appliesToChartType,
            bool appliesToSeries,
            bool appliesToDataPoint)
        {
            Name = name;
            ValueType = valueType;
            DefaultValue = defaultValue;
            Description = description;
            AppliesToChartType = appliesToChartType;
            AppliesToSeries = appliesToSeries;
            AppliesToDataPoint = appliesToDataPoint;
        }

        #endregion Constructor
    }
}