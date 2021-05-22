// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Resources;

namespace WebCharts.Services
{
    internal interface IChartTypeRegistry
    {
        ResourceManager ResourceManager { get; }

        IChartType GetChartType(SeriesChartType chartType);

        IChartType GetChartType(string name);

        void Register(string name, Type chartType);
    }
}