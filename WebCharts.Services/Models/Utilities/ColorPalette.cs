// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	A utility class which defines chart palette colors.
//              These palettes are used to assign unique colors to 
//              different chart series. For some chart types, like 
//              Pie, different colors are applied on the data point 
//              level.
//              Selected chart series/points palette is exposed 
//              through Chart.Palette property. Series.Palette 
//              property should be used to set different palette 
//              color for each point of the series. 
//


using SkiaSharp;
using System;
using System.Drawing;
using WebCharts.Services.Models.Common;

namespace WebCharts.Services.Models.Utilities
{
    #region Color palettes enumeration

    /// <summary>
    /// Chart color palettes enumeration
    /// </summary>
    public enum ChartColorPalette
	{ 
		/// <summary>
		/// Palette not set.
		/// </summary>
		None, 

		/// <summary>
        /// Bright palette.
		/// </summary>
		Bright, 

		/// <summary>
		/// Palette with gray scale colors.
		/// </summary>
		Grayscale, 

		/// <summary>
		/// Palette with Excel style colors.
		/// </summary>
		Excel,

		/// <summary>
		/// Palette with LightStyle style colors.
		/// </summary>
		Light,

		/// <summary>
		/// Palette with Pastel style colors.
		/// </summary>
		Pastel,

		/// <summary>
		/// Palette with Earth Tones style colors.
		/// </summary>
		EarthTones,

		/// <summary>
		/// Palette with SemiTransparent style colors.
		/// </summary>
		SemiTransparent, 

		/// <summary>
		/// Palette with Berry style colors.
		/// </summary>
		Berry,

		/// <summary>
		/// Palette with Chocolate style colors.
		/// </summary>
		Chocolate,

		/// <summary>
		/// Palette with Fire style colors.
		/// </summary>
		Fire,

		/// <summary>
		/// Palette with SeaGreen style colors.
		/// </summary>
		SeaGreen,

		/// <summary>
		/// Bright pastel palette.
		/// </summary>
		BrightPastel
	};

	#endregion	

    /// <summary>
    /// ChartPaletteColors is a utility class which provides access 
    /// to the predefined chart color palettes. These palettes are 
    /// used to assign unique colors to different chart series. 
    /// For some chart types, like Pie, different colors are applied 
    /// on the data point level.
    /// 
    /// GetPaletteColors method takes a ChartColorPalette enumeration 
    /// as a parameter and returns back an array of Colors. Each 
    /// palette contains different number of colors but it is a 
    /// good practice to keep this number around 15.
    /// </summary>
    internal static class ChartPaletteColors
	{
		#region Fields

		// Fields which store the palette color values
        private static readonly SKColor[] _colorsGrayScale = InitializeGrayScaleColors();
		private static readonly SKColor[] _colorsDefault = {
			SKColors.Green,
			SKColors.Blue,
			SKColors.Purple,
			SKColors.Lime,
			SKColors.Fuchsia,
			SKColors.Teal,
			SKColors.Yellow,
			SKColors.Gray,
			SKColors.Aqua,
			SKColors.Navy,
			SKColors.Maroon,
			SKColors.Red,
			SKColors.Olive,
			SKColors.Silver,
			SKColors.Tomato,
			SKColors.Moccasin
			};
		
		private static readonly SKColor[] _colorsPastel = {
													SKColors.SkyBlue,
													SKColors.LimeGreen,
													SKColors.MediumOrchid,
													SKColors.LightCoral,
													SKColors.SteelBlue,
													SKColors.YellowGreen,
													SKColors.Turquoise,
													SKColors.HotPink,
													SKColors.Khaki,
													SKColors.Tan,
													SKColors.DarkSeaGreen,
													SKColors.CornflowerBlue,
													SKColors.Plum,
													SKColors.CadetBlue,
													SKColors.PeachPuff,
													SKColors.LightSalmon
												};

		private static readonly SKColor[] _colorsEarth = {
												   Color.FromArgb(255, 128, 0),
												   SKColors.DarkGoldenrod,
												   Color.FromArgb(192, 64, 0),
												   SKColors.OliveDrab,
												   SKColors.Peru,
												   Color.FromArgb(192, 192, 0),
												   SKColors.ForestGreen,
												   SKColors.Chocolate,
												   SKColors.Olive,
												   SKColors.LightSeaGreen,
												   SKColors.SandyBrown,
												   Color.FromArgb(0, 192, 0),
												   SKColors.DarkSeaGreen,
												   SKColors.Firebrick,
												   SKColors.SaddleBrown,
												   Color.FromArgb(192, 0, 0)
											   };

		private static readonly SKColor[] _colorsSemiTransparent = {
													Color.FromArgb(150, 255, 0, 0),
													Color.FromArgb(150, 0, 255, 0),
													Color.FromArgb(150, 0, 0, 255),
													Color.FromArgb(150, 255, 255, 0),
													Color.FromArgb(150, 0, 255, 255),
													Color.FromArgb(150, 255, 0, 255),
													Color.FromArgb(150, 170, 120, 20),
													Color.FromArgb(80, 255, 0, 0),
													Color.FromArgb(80, 0, 255, 0),
													Color.FromArgb(80, 0, 0, 255),
													Color.FromArgb(80, 255, 255, 0),
													Color.FromArgb(80, 0, 255, 255),
													Color.FromArgb(80, 255, 0, 255),
													Color.FromArgb(80, 170, 120, 20),
													Color.FromArgb(150, 100, 120, 50),
													Color.FromArgb(150, 40, 90, 150)
											  };
		
		private static readonly SKColor[] _colorsLight = {
												   SKColors.Lavender,
												   SKColors.LavenderBlush,
												   SKColors.PeachPuff,
												   SKColors.LemonChiffon,
												   SKColors.MistyRose,
												   SKColors.Honeydew,
												   SKColors.AliceBlue,
												   SKColors.WhiteSmoke,
												   SKColors.AntiqueWhite,
												   SKColors.LightCyan
											   };

		private static readonly SKColor[] _colorsExcel = {
			Color.FromArgb(153,153,255),
			Color.FromArgb(153,51,102),
			Color.FromArgb(255,255,204),
			Color.FromArgb(204,255,255),
			Color.FromArgb(102,0,102),
			Color.FromArgb(255,128,128),
			Color.FromArgb(0,102,204),
			Color.FromArgb(204,204,255),
			Color.FromArgb(0,0,128),
			Color.FromArgb(255,0,255),
			Color.FromArgb(255,255,0),
			Color.FromArgb(0,255,255),
			Color.FromArgb(128,0,128),
			Color.FromArgb(128,0,0),
			Color.FromArgb(0,128,128),
			Color.FromArgb(0,0,255)};

		private static readonly SKColor[] _colorsBerry = {
												  SKColors.BlueViolet,
												  SKColors.MediumOrchid,
												  SKColors.RoyalBlue,
												  SKColors.MediumVioletRed,
												  SKColors.Blue,
												  SKColors.BlueViolet,
												  SKColors.Orchid,
												  SKColors.MediumSlateBlue,
												  Color.FromArgb(192, 0, 192),
												  SKColors.MediumBlue,
												  SKColors.Purple
											  };

		private static readonly SKColor[] _colorsChocolate = {
												  SKColors.Sienna,
												  SKColors.Chocolate,
												  SKColors.DarkRed,
												  SKColors.Peru,
												  SKColors.Brown,
												  SKColors.SandyBrown,
												  SKColors.SaddleBrown,
												  Color.FromArgb(192, 64, 0),
												  SKColors.Firebrick,
												  Color.FromArgb(182, 92, 58)
											  };

		private static readonly SKColor[] _colorsFire = {
													  SKColors.Gold,
													  SKColors.Red,
													  SKColors.DeepPink,
													  SKColors.Crimson,
													  SKColors.DarkOrange,
													  SKColors.Magenta,
													  SKColors.Yellow,
													  SKColors.OrangeRed,
													  SKColors.MediumVioletRed,
													  Color.FromArgb(221, 226, 33)
												  };

		private static readonly SKColor[] _colorsSeaGreen = {
												 SKColors.SeaGreen,
												 SKColors.MediumAquamarine,
												 SKColors.SteelBlue,
												 SKColors.DarkCyan,
												 SKColors.CadetBlue,
												 SKColors.MediumSeaGreen,
												 SKColors.MediumTurquoise,
												 SKColors.LightSteelBlue,
												 SKColors.DarkSeaGreen,
												 SKColors.SkyBlue
											 };

        private static readonly SKColor[] _colorsBrightPastel = {
												   Color.FromArgb(65, 140, 240),
												   Color.FromArgb(252, 180, 65),
												   Color.FromArgb(224, 64, 10),
												   Color.FromArgb(5, 100, 146),
												   Color.FromArgb(191, 191, 191),
												   Color.FromArgb(26, 59, 105),
												   Color.FromArgb(255, 227, 130),
												   Color.FromArgb(18, 156, 221),
												   Color.FromArgb(202, 107, 75),
												   Color.FromArgb(0, 92, 219),
												   Color.FromArgb(243, 210, 136),
												   Color.FromArgb(80, 99, 129),
												   Color.FromArgb(241, 185, 168),
												   Color.FromArgb(224, 131, 10),
												   Color.FromArgb(120, 147, 190)
											   };

		#endregion
		
		#region Constructor

		/// <summary>
		/// Initializes the GrayScale color array
		/// </summary>
		private static SKColor[] InitializeGrayScaleColors()
		{
			// Define gray scale colors
			SKColor[] grayScale = new SKColor[16];
			for(int i = 0; i < grayScale.Length; i++)
			{
				int colorValue = 200 - i * (180/16);
				grayScale[i] = Color.FromArgb(colorValue, colorValue, colorValue);
			}

            return grayScale;
		}

		#endregion

		#region Methods

        /// <summary>
        /// Return array of colors for the specified palette. Number of
        /// colors returned varies depending on the palette selected.
        /// </summary>
        /// <param name="palette">Palette to get the colors for.</param>
        /// <returns>Array of colors.</returns>
		public static SKColor[] GetPaletteColors(ChartColorPalette palette)
		{
			switch(palette)
			{
				case(ChartColorPalette.None):
				{
                    throw new ArgumentException(SR.ExceptionPaletteIsEmpty);
				}
				case(ChartColorPalette.Bright):
					return _colorsDefault;
				case(ChartColorPalette.Grayscale):
                    return _colorsGrayScale;
				case(ChartColorPalette.Excel):
					return _colorsExcel;
				case(ChartColorPalette.Pastel):
					return _colorsPastel;
				case(ChartColorPalette.Light):
					return _colorsLight;
				case(ChartColorPalette.EarthTones):
					return _colorsEarth;
				case(ChartColorPalette.SemiTransparent):
					return _colorsSemiTransparent;
				case(ChartColorPalette.Berry):
					return _colorsBerry;
				case(ChartColorPalette.Chocolate):
					return _colorsChocolate;
				case(ChartColorPalette.Fire):
					return _colorsFire;
				case(ChartColorPalette.SeaGreen):
					return _colorsSeaGreen;
				case(ChartColorPalette.BrightPastel):
                    return _colorsBrightPastel;
			}
			return null;
		}

		#endregion
	}
}
