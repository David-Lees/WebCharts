// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using SkiaSharp;
using System;

namespace WebCharts.Services
{
    internal interface IImageLoader: IDisposable
    {
        SKImage LoadImage(string imageURL);
        SKImage LoadImage(string imageURL, bool saveImage);
    }
}