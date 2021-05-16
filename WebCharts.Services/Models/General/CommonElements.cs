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


using System.Globalization;
using WebCharts.Services.Models.Utilities;
using WebCharts.Services.Models.DataManager;
using WebCharts.Services.Models.Borders3D;
using WebCharts.Services.Models.ChartTypes;
using System;
using WebCharts.Services.Models.Formulas;

namespace WebCharts.Services.Models.General
{
    /// <summary>
    /// CommonElements class provides references to common chart classes like 
    /// DataManager, ChartTypeRegistry, ImageLoader and others. It is passed 
    /// to different chart elements to simplify access to those common classes.
    /// </summary>
    public class CommonElements
	{
		#region Fields

        private ChartService _chart;
        private ChartImage _chartPicture; 

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

        #endregion

        #region Properties

        private readonly IServiceProvider _provider;

        public CommonElements(IServiceProvider serviceProvider)
        {
			_provider = serviceProvider;
        }

		/// <summary>
		/// Reference to the Data Manager
		/// </summary>
		internal IDataManager DataManager => (IDataManager)_provider.GetService(typeof(IDataManager));

		internal ICustomPropertyRegistry CustomPropertyRegistry => (ICustomPropertyRegistry)_provider.GetService(typeof(ICustomPropertyRegistry));


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
		internal HotRegionsList HotRegionsList
		{
			get
			{
				return ChartPicture.hotRegionsList;
			}
		}

		/// <summary>
		/// Reference to the Data Manipulator
		/// </summary>
		public DataManipulator DataManipulator
		{
			get
			{
				return ChartPicture.DataManipulator;
			}
		}

		/// <summary>
		/// Reference to the ImageLoader
		/// </summary>
		internal ImageLoader ImageLoader => (ImageLoader)_provider.GetService(typeof(IImageLoader));

		/// <summary>
		/// Reference to the Chart
		/// </summary>
		internal ChartService Chart
		{
			get
			{
				
				if (_chart==null)
                    _chart = (ChartService)_provider.GetService(typeof(ChartService));
                return _chart;
			}
		}

		/// <summary>
		/// Reference to the ChartTypeRegistry
		/// </summary>
		internal ChartTypeRegistry ChartTypeRegistry
		{
			get
			{
				return (ChartTypeRegistry)_provider.GetService(typeof(ChartTypeRegistry));
			}
		}

		/// <summary>
		/// Reference to the BorderTypeRegistry
		/// </summary>
		internal BorderTypeRegistry BorderTypeRegistry
		{
			get
			{
				return (BorderTypeRegistry)_provider.GetService(typeof(BorderTypeRegistry));
			}
		}

		/// <summary>
		/// Reference to the FormulaRegistry
		/// </summary>
		internal FormulaRegistry FormulaRegistry
		{
			get
			{
				return (FormulaRegistry)_provider.GetService(typeof(IFormulaRegistry));
			}
		}



		/// <summary>
		/// Reference to the ChartPicture
		/// </summary>
		internal ChartImage ChartPicture
		{
			get
			{
				if (_chartPicture ==null)
                    _chartPicture = (ChartImage)_provider.GetService(typeof(IChartImage));
                return _chartPicture;
			}
		}

        /// <summary>
        /// Width of the chart picture
        /// </summary>
        internal int Width { get; set; } = 0;

        /// <summary>
        /// Height of the chart picture
        /// </summary>
        internal int Height { get; set; } = 0;

        #endregion

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

		#endregion
	}
}
