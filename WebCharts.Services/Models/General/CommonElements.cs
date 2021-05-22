// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	CommonElements class provides references to common
//              chart classes like DataManager, ChartTypeRegistry,
//              ImageLoader and others. It is passed to different
//              chart elements to simplify access to those common
//              classes.
//

using System;
using System.Globalization;

namespace WebCharts.Services
{
    /// <summary>
    /// CommonElements class provides references to common chart classes like
    /// DataManager, ChartTypeRegistry, ImageLoader and others. It is passed
    /// to different chart elements to simplify access to those common classes.
    /// </summary>
    public class CommonElements: IDisposable
    {
        #region Fields

        private readonly ChartService _chart;

        // Reference to Chart Graphics Object
        internal ChartGraphics graph = null;

        /// <summary>
        /// Indicates painting mode
        /// </summary>
        internal bool processModePaint = true;

        /// <summary>
        /// Indicates selection mode
        /// </summary>
        internal bool processModeRegions = false;
        private bool disposedValue;

        #endregion Fields

        #region Properties

        public CommonElements(ChartService chart)
        {
            _chart = chart;
            DataManager = new DataManager(this);
            CustomPropertyRegistry = new CustomPropertyRegistry();
            ImageLoader = new ImageLoader(this);
            ChartTypeRegistry = new ChartTypeRegistry();
            BorderTypeRegistry = new BorderTypeRegistry();
            FormulaRegistry = new FormulaRegistry();
            HotRegionsList = new(this);
        }

        /// <summary>
        /// Reference to the Data Manager
        /// </summary>
        internal DataManager DataManager { get; set; }

        internal ICustomPropertyRegistry CustomPropertyRegistry { get; set; }

        /// <summary>
        /// True if painting mode is active
        /// </summary>
        public bool ProcessModePaint
        {
            get
            {
                return processModePaint;
            }
        }

        /// <summary>
        /// True if Hot region or image maps mode is active
        /// </summary>
        public bool ProcessModeRegions
        {
            get
            {
                return processModeRegions;
            }
        }

        /// <summary>
        /// Reference to the hot regions object
        /// </summary>
        internal HotRegionsList HotRegionsList { get; set; }

        /// <summary>
        /// Reference to the ImageLoader
        /// </summary>
        internal ImageLoader ImageLoader { get; set; }

        /// <summary>
        /// Reference to the Chart
        /// </summary>
        internal ChartService Chart
        {
            get
            {
                return _chart;
            }
        }

        /// <summary>
        /// Reference to the ChartTypeRegistry
        /// </summary>
        internal ChartTypeRegistry ChartTypeRegistry { get; set; }

        /// <summary>
        /// Reference to the BorderTypeRegistry
        /// </summary>
        internal BorderTypeRegistry BorderTypeRegistry { get; set; }

        /// <summary>
        /// Reference to the FormulaRegistry
        /// </summary>
        internal FormulaRegistry FormulaRegistry { get; set; }

        /// <summary>
        /// Reference to the ChartPicture
        /// </summary>
        internal ChartImage ChartPicture { get; set; }

        /// <summary>
        /// Width of the chart picture
        /// </summary>
        internal int Width { get; set; } = 0;

        /// <summary>
        /// Height of the chart picture
        /// </summary>
        internal int Height { get; set; } = 0;

        #endregion Properties

        #region String convertion helper methods

        /// <summary>
        /// Converts string to double.
        /// </summary>
        /// <param name="stringToParse">String to convert.</param>
        /// <returns>Double result.</returns>
        internal static double ParseDouble(string stringToParse)
        {
            return ParseDouble(stringToParse, false);
        }

        /// <summary>
        /// Converts string to double.
        /// </summary>
        /// <param name="stringToParse">String to convert.</param>
        /// <param name="throwException">if set to <c>true</c> the exception thrown.</param>
        /// <returns>Double result.</returns>
        internal static double ParseDouble(string stringToParse, bool throwException)
        {
            double result;
            if (throwException)
            {
                result = double.Parse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            else
            {
                bool parseSucceed = double.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                if (!parseSucceed)
                {
                    double.TryParse(stringToParse, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts string to double.
        /// </summary>
        /// <param name="stringToParse">String to convert.</param>
        /// <returns>Double result.</returns>
        internal static float ParseFloat(string stringToParse)
        {
            bool parseSucceed = float.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out float result);

            if (!parseSucceed)
            {
                float.TryParse(stringToParse, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && HotRegionsList != null)
                {
                    HotRegionsList.Dispose();
                    HotRegionsList = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion String convertion helper methods


    }
}