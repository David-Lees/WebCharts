// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using SkiaSharp;
using System;
using System.Collections;
using System.IO;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Annotations;
using WebCharts.Services.Models.Borders3D;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.General;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services
{
    public interface IChartService
    {
        AnnotationCollection Annotations { get; }
        AntiAliasingStyles AntiAliasing { get; set; }
        SKColor BackColor { get; set; }
        GradientStyle BackGradientStyle { get; set; }
        ChartHatchStyle BackHatchStyle { get; set; }
        string BackImage { get; set; }
        ChartImageAlignmentStyle BackImageAlignment { get; set; }
        SKColor BackImageTransparentColor { get; set; }
        ChartImageWrapMode BackImageWrapMode { get; set; }
        SKColor BackSecondaryColor { get; set; }
        SKColor BorderColor { get; set; }
        ChartDashStyle BorderDashStyle { get; set; }
        SKColor BorderlineColor { get; set; }
        ChartDashStyle BorderlineDashStyle { get; set; }
        int BorderlineWidth { get; set; }
        BorderSkin BorderSkin { get; set; }
        int BorderWidth { get; set; }
        string BuildNumber { get; }
        ChartAreaCollection ChartAreas { get; }
        DataManipulator DataManipulator { get; }
        object DataSource { get; set; }
        SKFont Font { get; set; }
        SKColor ForeColor { get; set; }
        NamedImagesCollection Images { get; }
        bool IsSoftShadows { get; set; }
        LegendCollection Legends { get; }
        ChartColorPalette Palette { get; set; }
        SKColor[] PaletteCustomColors { get; set; }
        double RenderingDpiX { get; set; }
        double RenderingDpiY { get; set; }
        SeriesCollection Series { get; }
        SKSize Size { get; set; }
        bool SuppressExceptions { get; set; }
        TextAntiAliasingQuality TextAntiAliasingQuality { get; set; }
        TitleCollection Titles { get; }

        event EventHandler AnnotationPlaced;
        event EventHandler AnnotationPositionChanged;
        event EventHandler<AnnotationPositionChangingEventArgs> AnnotationPositionChanging;
        event EventHandler AnnotationSelectionChanged;
        event EventHandler AnnotationTextChanged;
        event EventHandler<ScrollBarEventArgs> AxisScrollBarClicked;
        event EventHandler<ViewEventArgs> AxisViewChanged;
        event EventHandler<ViewEventArgs> AxisViewChanging;
        event EventHandler Customize;
        event EventHandler<CustomizeLegendEventArgs> CustomizeLegend;
        event EventHandler<FormatNumberEventArgs> FormatNumber;
        event EventHandler<ChartPaintEventArgs> PostPaint;
        event EventHandler<ChartPaintEventArgs> PrePaint;

        void AlignDataPointsByAxisLabel();
        void AlignDataPointsByAxisLabel(PointSortOrder sortingOrder);
        void AlignDataPointsByAxisLabel(string series);
        void AlignDataPointsByAxisLabel(string series, PointSortOrder sortingOrder);
        void ApplyPaletteColors();
        void BeginInit();
        void DataBind();
        void DataBindCrossTable(IEnumerable dataSource, string seriesGroupByField, string xField, string yFields, string otherFields);
        void DataBindCrossTable(IEnumerable dataSource, string seriesGroupByField, string xField, string yFields, string otherFields, PointSortOrder sortingOrder);
        void DataBindTable(IEnumerable dataSource);
        void DataBindTable(IEnumerable dataSource, string xField);
        void EndInit();
        object GetService(Type serviceType);
        void ResetAutoValues();
        void SaveImage(Stream imageStream, ChartImageFormat format);
        void SaveImage(string imageFileName, ChartImageFormat format);
    }
}