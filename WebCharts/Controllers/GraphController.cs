using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebCharts.Models;
using WebCharts.Services;

namespace WebCharts.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GraphController : ControllerBase
    {
        private readonly IServiceProvider _provider;

        public GraphController(IServiceProvider provider)
        {
            _provider = provider;
        }

        [HttpGet("line")]
        public ActionResult Line(string id, string parcode, string datasource, DateTime start, bool showforecast, bool multi = false)
        {
            // limit the amount of data to return to one year
            if (start < DateTime.Now.AddYears(-1))
            {
                start = DateTime.Now.AddYears(-1);
            }

            ChartService chart = new(_provider, 860, 415);

            chart.ChartAreas.Add("Area 1");

            // fetch the axis label and location
            chart.ChartAreas[0].AxisY.Title = "Height (m)";

            chart.ChartAreas[0].AxisY.TitleFont = new SKFont(SKTypeface.FromFamilyName("Trebuchet MS", SKFontStyle.Normal), 10);

            //add annotation
            chart.Annotations.Add(new TextAnnotation
            {
                X = 0,
                Y = 95.5,
                Width = 100,
                Text = "Test annontation",
                Font = new SKFont(SKTypeface.FromFamilyName("Trebuchet MS", SKFontStyle.Normal), 9),
                Alignment = ContentAlignment.TopRight
            });

            // Instance of series for observed data
            Series seriesObserved = new("Observed");

            // If timescale is less than 7 days, use marker points
            // Else, the markers will merge too much so use FastLine to speed up rendering.
            if (Math.Abs((DateTime.Now - start).TotalDays) < 7)
            {
                seriesObserved.ChartType = SeriesChartType.Line;
                seriesObserved.MarkerSize = 5;
                seriesObserved.MarkerStyle = MarkerStyle.Circle;
            }
            else
            {
                seriesObserved.ChartType = SeriesChartType.FastLine;
            }
            seriesObserved.Color = SKColors.DarkBlue;
            seriesObserved.BorderWidth = 1;

            // Rather than copy the entire data into a list, just use an enumeration with lazy load.
            // Data won't get fetched until the loop. This should be more memory efficient.
            var dataItems = new List<LineData>()
            {
                new LineData { DepId = "1", time = DateTime.Parse("16-May-21 19:00"), value = 0.71 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 18:30"), value =    0.69 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 18:00"), value =    0.67 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 17:30"), value =    0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 17:00"), value =   0.69 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 16:30"), value =    0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 16:00"), value =    0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 15:30"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 15:00"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 14:30"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 14:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 13:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 13:00"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 12:30"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 12:00"), value =   0.57 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 11:30"), value =   0.55 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 11:00"), value =   0.57 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 10:30"), value =   0.55 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 10:00"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 09:30"), value =   0.59 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 09:00"), value =   0.63 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 08:30"), value =   0.69 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 08:00"), value =   0.71 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 07:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 07:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 07:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 07:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 06:30"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 06:00"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 05:30"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 05:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 04:30"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 04:00"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 03:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 03:00"), value =   0.71 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 02:30"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 02:00"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 01:30"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 01:00"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 00:30"), value =   0.57 },
                new LineData { DepId = "1", time = DateTime.Parse("16 - May - 21 00:00"), value =   0.59 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 23:30"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 23:00"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 22:30"), value =   0.63 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 22:00"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 21:30"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 21:00"), value =   0.61 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 20:30"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("15 / 05 / 2021 20:00"), value =     0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 19:30"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 19:00"), value =   0.63 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 18:30"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 18:00"), value =   0.65 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 17:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 17:00"), value =   0.71 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 16:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 16:00"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 15:30"), value =   0.84 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 15:00"), value =   0.8 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 14:30"), value =   0.8 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 14:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 13:30"), value =   0.82 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 13:00"), value =   0.82 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 12:30"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 12:00"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 11:30"), value =   0.73 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 11:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 10:30"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 10:00"), value =   0.78 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 09:30"), value =   0.71 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 09:00"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 08:30"), value =   0.75 },
                new LineData { DepId = "1", time = DateTime.Parse("15 - May - 21 08:00"), value =   0.75 },
            };

            // Actually retrieve the rows one by one and put into point data.
            var depID = string.Empty;
            var missingData = new List<DateTime>();
            var previousTimestamp = DateTime.Now;
            foreach (var dataItem in dataItems.ToList())
            {
                // Check for breaks between deployments and add null to split line.
                if (!string.IsNullOrWhiteSpace(depID) && depID != dataItem.DepId)
                {
                    seriesObserved.Points.Add(new DataPoint
                    {
                        XValue = previousTimestamp.AddSeconds(1).ToOADate(),
                        IsEmpty = true
                    });
                }

                // Add x/y point to series.
                seriesObserved.Points.AddXY(dataItem.time, dataItem.value);

                // Capture depID and timestamp
                depID = dataItem.DepId;
                previousTimestamp = dataItem.time;

                // If data item is null, add to missing data list
                if (dataItem.value == null)
                {
                    missingData.Add(dataItem.time);
                }
            }

            chart.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            chart.ChartAreas[0].BorderColor = SKColors.Black;
            chart.ChartAreas[0].BorderWidth = 1;

            chart.ChartAreas[0].AxisX.LabelStyle.Format = Math.Abs((DateTime.Now - start).TotalDays) < 5
                ? "HH:ss\nd MMM yy"
                : "d MMM yy";
            chart.ChartAreas[0].AxisX.MinorGrid.LineColor = SKColors.Gray;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = SKColors.Gray;

            chart.ChartAreas[0].AxisY.LabelStyle.Format = "f1";
            chart.ChartAreas[0].AxisY.MinorGrid.LineColor = SKColors.Gray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = SKColors.Gray;

            // Add data series to chart
            chart.Series.Add(seriesObserved);

#if false
            // Add forecast data if appropriate
            if (showforecast)
            {
                var queryStringForecast = string.Empty;
                switch (datasource)
                {
                    case "INT":
                        queryStringForecast = SQLQueries.ForecastInt;
                        break;

                    case "EXT":
                        queryStringForecast = SQLQueries.ForecastExt;
                        break;

                    default:
                        break;
                }

                var forecastItems = Enumerable.Empty<ForecastData>();
                if (!string.IsNullOrWhiteSpace(queryStringForecast))
                {
                    forecastItems = db.Database.SqlQuery<ForecastData>(queryStringForecast,
                        new SqlParameter("ID", id),
                        new SqlParameter("ParCode", parcode),
                        new SqlParameter("START", start));
                }
                var forecastItemsList = forecastItems.ToList();

                chart.Legends.Add("Legend");
                chart.Legends[0].Font = new Font("Trebuchet MS", 10, FontStyle.Regular);

                if (multi)
                {
                    // If "multi" is set, split into multiple series
                    int colour = 1;
                    foreach (DateTime forecastTimestamp in (from x
                                                            in forecastItemsList
                                                            select x.ForecastTime.Value)
                                                            .Distinct())
                    {
                        Series forecastSeriesItem = new Series($"Forecast\n({forecastTimestamp:d MMM, HH:ss})");
                        foreach (ForecastData forecastDataItem in forecastItemsList
                                                                   .Where(x => x.ForecastTime == forecastTimestamp))
                        {
                            forecastSeriesItem.Points.AddXY(forecastDataItem.RecordTime, forecastDataItem.ResultValue);
                        }
                        forecastSeriesItem.ChartType = SeriesChartType.Line;
                        forecastSeriesItem.Color = GetColour(colour);
                        colour++;
                        forecastSeriesItem.BorderWidth = 2;
                        if (forecastSeriesItem.Points.Any())
                        {
                            chart.Series.Add(forecastSeriesItem);
                        }
                    }
                }
                else
                {
                    // plot as single series
                    Series seriesForecastAll = new Series("Forecast");
                    foreach (ForecastData forecastDataItem in forecastItemsList)
                    {
                        seriesForecastAll.Points.AddXY(forecastDataItem.RecordTime, forecastDataItem.ResultValue);
                    }
                    seriesForecastAll.ChartType = SeriesChartType.Line;
                    seriesForecastAll.Color = Color.Red;
                    seriesForecastAll.BorderWidth = 2;
                    if (seriesForecastAll.Points.Any())
                    {
                        var lastForecast = forecastItemsList.Max(x => x.ForecastTime);
                        if (lastForecast.HasValue)
                        {
                            seriesForecastAll.Name = $"Forecast\n({lastForecast.Value:d MMM, HH:ss})";
                        }
                        chart.Series.Add(seriesForecastAll);
                    }
                }

                // Add vertical line to show current timestamp
                chart.ChartAreas[0].AxisX.StripLines.Add(new StripLine
                {
                    Interval = 0,
                    IntervalOffset = DateTime.UtcNow.ToOADate(),
                    StripWidth = 0,
                    BorderWidth = 1,
                    BorderColor = Color.Orange,
                    BorderDashStyle = ChartDashStyle.Solid
                });
            }
            else
            {
                // no forecast
                // show legend if null entries encountered
                if (missingData.Any())
                {
                    chart.Legends.Add("Legend");
                    chart.Legends[0].Font = new Font("Trebuchet MS", 10, FontStyle.Regular);
                }
            }
#endif
            // Set Y-axis scale
            var yAxisSettings = new YaxisSettings(parcode ?? "PCode", chart.Series);
            chart.ChartAreas[0].AxisY.Minimum = yAxisSettings.Minimum;
            chart.ChartAreas[0].AxisY.Maximum = yAxisSettings.Maximum;
            chart.ChartAreas[0].AxisY.Interval = yAxisSettings.Interval;
            chart.ChartAreas[0].AxisY.LabelStyle.Format = yAxisSettings.LabelFormat;
            chart.ChartAreas[0].AxisY.MinorGrid.Enabled = yAxisSettings.MinorTickInterval.HasValue;
            chart.ChartAreas[0].AxisY.MinorGrid.Interval = yAxisSettings.MinorTickInterval.HasValue
                ? yAxisSettings.MinorTickInterval.Value
                : 0;
            chart.ChartAreas[0].AxisY.MinorTickMark.Enabled = yAxisSettings.MinorTickInterval.HasValue;
            chart.ChartAreas[0].AxisY.MinorTickMark.Interval = yAxisSettings.MinorTickInterval.HasValue
                ? yAxisSettings.MinorTickInterval.Value
                : 0;

            // If any missing data, add series and create points
            if (missingData.Any())
            {
                var seriesMissing = new Series("Invalid data")
                {
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = MarkerStyle.Cross,
                    MarkerSize = 7,
                    MarkerColor = SKColors.IndianRed,
                    MarkerBorderWidth = 0
                };
                var y = chart.ChartAreas[0].AxisY.Minimum + (chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum) * 1 / 100;
                missingData.ForEach(x => seriesMissing.Points.AddXY(x, y));
                chart.Series.Add(seriesMissing);
            }

            var xMin = chart.Series.SelectMany(x => x.Points).Min(x => x.XValue);
            var xMax = chart.Series.SelectMany(x => x.Points).Max(x => x.XValue);

            // Create stream to represent chart as PNG image
            using (var stream = new MemoryStream())
            {
                chart.SaveImage(stream, ChartImageFormat.Png);
                return new FileContentResult(stream.ToArray(), "image/png");
            }
        }
    }
}