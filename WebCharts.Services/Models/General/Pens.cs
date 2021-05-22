// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using SkiaSharp;

namespace WebCharts.Services
{
    internal static class Pens
    {
        public static SKPaint Cyan => new() { StrokeWidth = 1, Style = SKPaintStyle.Stroke, Color = SKColors.Cyan };
        public static SKPaint Red => new() { StrokeWidth = 1, Style = SKPaintStyle.Stroke, Color = SKColors.Red };
        public static SKPaint Blue => new() { StrokeWidth = 1, Style = SKPaintStyle.Stroke, Color = SKColors.Blue };
        public static SKPaint Green => new() { StrokeWidth = 1, Style = SKPaintStyle.Stroke, Color = SKColors.Green };
    }
}