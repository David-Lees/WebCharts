// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	ChartTypeRegistry is a repository for all standard 
//              and custom chart types. Each chart type has unique 
//              name and IChartType derived class which provides
//              behaviour information about the chart type and
//              also contains drwaing functionality.
//              ChartTypeRegistry can be used by user for custom 
//              chart type registering and can be retrieved using 
//              Chart.GetService(typeof(ChartTypeRegistry)) method.
//


using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using WebCharts.Services.Enums;
using WebCharts.Services.Interfaces;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.ChartTypes
{
    /// <summary>
    /// ChartTypeName class contains constant strings defining
    /// names of all ChartTypes used in the Chart.
    /// </summary>
    internal static class ChartTypeNames
    {
        #region Chart type names

        internal const string Area = "Area";
        internal const string RangeBar = "RangeBar";
        internal const string Bar = "Bar";
        internal const string SplineArea = "SplineArea";
        internal const string BoxPlot = "BoxPlot";
        internal const string Bubble = "Bubble";
        internal const string Column = "Column";
        internal const string RangeColumn = "RangeColumn";
        internal const string Doughnut = "Doughnut";
        internal const string ErrorBar = "ErrorBar";
        internal const string FastLine = "FastLine";
        internal const string FastPoint = "FastPoint";
        internal const string Funnel = "Funnel";
        internal const string Pyramid = "Pyramid";
        internal const string Kagi = "Kagi";
        internal const string Spline = "Spline";
        internal const string Line = "Line";
        internal const string PointAndFigure = "PointAndFigure";
        internal const string Pie = "Pie";
        internal const string Point = "Point";
        internal const string Polar = "Polar";
        internal const string Radar = "Radar";
        internal const string SplineRange = "SplineRange";
        internal const string Range = "Range";
        internal const string Renko = "Renko";
        internal const string OneHundredPercentStackedArea = "100%StackedArea";
        internal const string StackedArea = "StackedArea";
        internal const string OneHundredPercentStackedBar = "100%StackedBar";
        internal const string StackedBar = "StackedBar";
        internal const string OneHundredPercentStackedColumn = "100%StackedColumn";
        internal const string StackedColumn = "StackedColumn";
        internal const string StepLine = "StepLine";
        internal const string Candlestick = "Candlestick";
        internal const string Stock = "Stock";
        internal const string ThreeLineBreak = "ThreeLineBreak";

        #endregion // Keyword Names
    }

    /// <summary>
    /// ChartTypeRegistry class is a repository for all standard and custom 
    /// chart types. In order for the chart control to display the chart 
    /// type, it first must be registered using unique name and IChartType 
    /// derived class which provides the description of the chart type and 
    /// also responsible for all drawing and hit testing.
    /// 
    /// ChartTypeRegistry can be used by user for custom chart type registering 
    /// and can be retrieved using Chart.GetService(typeof(ChartTypeRegistry)) 
    /// method.
    /// </summary>
    internal class ChartTypeRegistry : IServiceProvider, IDisposable, IChartTypeRegistry
    {
        #region Fields

        // Chart types image resource manager
        private ResourceManager _resourceManager = null;

        // Storage for registered/created chart types
        internal Hashtable registeredChartTypes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Hashtable _createdChartTypes = new(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor and Services

        /// <summary>
        /// Chart types registry public constructor.
        /// </summary>
        public ChartTypeRegistry()
        {
        }

        /// <summary>
        /// Returns chart type registry service object.
        /// </summary>
        /// <param name="serviceType">Service type to get.</param>
        /// <returns>Chart type registry service.</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(ChartTypeRegistry))
            {
                return this;
            }
            throw (new ArgumentException(SR.ExceptionChartTypeRegistryUnsupportedType(serviceType.ToString())));
        }

        #endregion

        #region Registry methods

        /// <summary>
        /// Adds chart type into the registry.
        /// </summary>
        /// <param name="name">Chart type name.</param>
        /// <param name="chartType">Chart class type.</param>
        public void Register(string name, Type chartType)
        {
            // First check if chart type with specified name already registered
            if (registeredChartTypes.Contains(name))
            {
                // If same type provided - ignore
                if (registeredChartTypes[name].GetType() == chartType)
                {
                    return;
                }

                // Error - throw exception
                throw (new ArgumentException(SR.ExceptionChartTypeNameIsNotUnique(name)));
            }

            // Make sure that specified class support IChartType interface
            bool found = false;
            Type[] interfaces = chartType.GetInterfaces();
            foreach (Type type in interfaces)
            {
                if (type == typeof(IChartType))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                throw (new ArgumentException(SR.ExceptionChartTypeHasNoInterface));
            }

            // Add chart type to the hash table
            registeredChartTypes[name] = chartType;
        }

        /// <summary>
        /// Returns chart type object by name.
        /// </summary>
        /// <param name="chartType">Chart type.</param>
        /// <returns>Chart type object derived from IChartType.</returns>
        public IChartType GetChartType(SeriesChartType chartType)
        {
            return GetChartType(Series.GetChartTypeName(chartType));
        }

        /// <summary>
        /// Returns chart type object by name.
        /// </summary>
        /// <param name="name">Chart type name.</param>
        /// <returns>Chart type object derived from IChartType.</returns>
        public IChartType GetChartType(string name)
        {
            // First check if chart type with specified name registered
            if (!registeredChartTypes.Contains(name))
            {
                throw (new ArgumentException(SR.ExceptionChartTypeUnknown(name)));
            }

            // Check if the chart type object is already created
            if (!_createdChartTypes.Contains(name))
            {
                // Create chart type object
                _createdChartTypes[name] =
                    ((Type)registeredChartTypes[name]).Assembly.
                    CreateInstance(((Type)registeredChartTypes[name]).ToString());
            }

            return (IChartType)_createdChartTypes[name];
        }

        /// <summary>
        /// Chart images resource manager.
        /// </summary>
        public ResourceManager ResourceManager
        {
            get
            {
                // Create chart images resource manager
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager(typeof(ChartService).Namespace + ".Design", Assembly.GetExecutingAssembly());
                }
                return _resourceManager;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resource
                foreach (string name in _createdChartTypes.Keys)
                {
                    IChartType chartType = (IChartType)_createdChartTypes[name];
                    chartType.Dispose();
                }
                _createdChartTypes.Clear();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
