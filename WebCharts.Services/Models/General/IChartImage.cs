// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using SkiaSharp;

namespace WebCharts.Services
{
    internal interface IChartImage
    {
        int Compression { get; set; }
        object DataSource { get; set; }

        SKBitmap GetImage();
        SKBitmap GetImage(float resolution);
    }
}