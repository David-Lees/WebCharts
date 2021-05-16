// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Classes that implement different 3D border styles.
//


using SkiaSharp;
using System;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.General;
using WebCharts.Services.Models.Utilities;

namespace WebCharts.Services.Models.Borders3D
{
    /// <summary>
    /// Implements frame border.
    /// </summary>
    internal class FrameTitle1Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle1Border()
		{
			sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle1";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }
		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override SKRect GetTitlePositionInBorder()
		{
			return new SKRect(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle2Border : FrameThin2Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle2Border()
		{
			sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle2";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override SKRect GetTitlePositionInBorder()
		{
			return new SKRect(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle3Border : FrameThin3Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle3Border()
		{
			sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle3";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override SKRect GetTitlePositionInBorder()
		{
			return new SKRect(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
		#endregion
	}
	
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle4Border : FrameThin4Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle4Border()
		{
			sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle4";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override SKRect GetTitlePositionInBorder()
		{
			return new SKRect(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
	
		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle5Border : FrameThin5Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle5Border()
		{
			sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize*2f);
			drawScrews = true;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle5";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override SKRect GetTitlePositionInBorder()
		{
			return new SKRect(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
	
		#endregion
	}
	
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle6Border : FrameThin6Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle6Border()
		{
			sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize*2f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle6";}}

        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeLeftTop = new SKSize(sizeLeftTop.Width, defaultRadiusSize * 2f);
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public override SKRect GetTitlePositionInBorder()
		{
			return new SKRect(
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 0.25f,
				defaultRadiusSize * 1.6f);
		}
	
		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle7Border : FrameTitle1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle7Border()
		{
			sizeRightBottom = new SKSize(0, sizeRightBottom.Height);
			float[] corners = {15f, 1f, 1f, 1f, 1f, 15f, 15f, 15f};
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle7";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                sizeRightBottom = new SKSize(0, sizeRightBottom.Height);
                float largeRadius = 15f * resolution / 96.0f;
                float smallRadius = 1 * resolution / 96.0f;
                float[] corners = { largeRadius, smallRadius, smallRadius, smallRadius, smallRadius, largeRadius, largeRadius, largeRadius };
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameTitle8Border : FrameTitle1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameTitle8Border()
		{
			sizeLeftTop = new SKSize(0, sizeLeftTop.Height);
			sizeRightBottom = new SKSize(0, sizeRightBottom.Height);
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameTitle8";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;

                sizeLeftTop = new SKSize(0, sizeLeftTop.Height);
                sizeRightBottom = new SKSize(0, sizeRightBottom.Height);
                float radius = 1 * resolution / 96.0f;
                float[] corners = { radius, radius, radius, radius, radius, radius, radius, radius };
                innerCorners = corners;
            }
        }

		#endregion
	}
	
	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin2Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin2Border()
		{
			float[] corners = {15f, 15f, 15f, 1f, 1f, 1f, 1f, 15f};
			cornerRadius = corners;
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin2";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;

                float largeRadius = 15f * resolution / 96.0f;
                float smallRadius = 1 * resolution / 96.0f;
                float[] corners = { largeRadius, largeRadius, largeRadius, smallRadius, smallRadius, smallRadius, smallRadius, largeRadius };
                cornerRadius = corners;
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin3Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin3Border()
		{
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			cornerRadius = corners;
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin3";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = resolution / 96.0f;
                float[] corners = { radius, radius, radius, radius, radius, radius, radius, radius };
                cornerRadius = corners;
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin4Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin4Border()
		{
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			cornerRadius = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin4";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = 1f * resolution / 96.0f;
                cornerRadius = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
            }
        }

        #endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin5Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin5Border()
		{
			drawScrews = true;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin5";}}

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin6Border : FrameThin1Border
	{
		#region Border properties and methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin6Border()
		{
			float[] corners = {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f};
			innerCorners = corners;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin6";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = resolution / 96.0f;
                float[] corners = { radius, radius, radius, radius, radius, radius, radius, radius };
                innerCorners = corners;
            }
        }

		#endregion
	}

	/// <summary>
	/// Implements frame border.
	/// </summary>
	internal class FrameThin1Border : RaisedBorder
	{
		#region Border properties and methods

		/// <summary>
		/// Inner corners radius array
		/// </summary>
        internal float[] innerCorners = { 15f, 15f, 15f, 15f, 15f, 15f, 15f, 15f };

		/// <summary>
		/// Default constructor
		/// </summary>
		public FrameThin1Border()
		{
			sizeLeftTop = new SKSize(defaultRadiusSize * .8f, defaultRadiusSize * .8f);
			sizeRightBottom = new SKSize(defaultRadiusSize * .8f, defaultRadiusSize * .8f);
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "FrameThin1";}}


        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                float radius = 15.0f * resolution / 96.0f;
                innerCorners = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
                sizeLeftTop = new SKSize(defaultRadiusSize * .8f, defaultRadiusSize * .8f);
                sizeRightBottom = new SKSize(defaultRadiusSize * .8f, defaultRadiusSize * .8f);

            }
        }

        /// <summary>
        /// Draws 3D border.
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="borderSkin">Border skin object.</param>
        /// <param name="rect">Rectangle of the border.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type</param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
		public override void DrawBorder(
			ChartGraphics graph, 
			BorderSkin borderSkin,
			SKRect rect, 
			SKColor backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			SKColor backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			SKColor backSecondaryColor, 
			SKColor borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle)	
		{
			drawBottomShadow = true;
			sunken = false;
			outsideShadowRate = .9f;
			drawOutsideTopLeftShadow = false;
			bool oldScrewsFlag = drawScrews;
			drawScrews = false;
			base.DrawBorder(
				graph, 
				borderSkin, 
				rect, 
				borderSkin.BackColor, 
				borderSkin.BackHatchStyle, 
				borderSkin.BackImage, 
				borderSkin.BackImageWrapMode, 
				borderSkin.BackImageTransparentColor, 
				borderSkin.BackImageAlignment, 
				borderSkin.BackGradientStyle, 
				borderSkin.BackSecondaryColor, 
				borderSkin.BorderColor, 
				borderSkin.BorderWidth, 
				borderSkin.BorderDashStyle);

			drawScrews = oldScrewsFlag;
			rect.Left += sizeLeftTop.Width;
			rect.Top += sizeLeftTop.Height;
			rect.Right -= sizeRightBottom.Width + sizeLeftTop.Width;
			rect.Bottom -= sizeRightBottom.Height + sizeLeftTop.Height;
			if(rect.Width > 0 && rect.Height > 0 )
			{
				float[] oldCorners = new float[8];
				oldCorners = (float[])cornerRadius.Clone();
				cornerRadius = innerCorners;
				drawBottomShadow = false;
				sunken = true;
				drawOutsideTopLeftShadow = true;
				outsideShadowRate = 1.4f;
				SKColor oldPageColor = borderSkin.PageColor;
				borderSkin.PageColor = SKColors.Transparent;
				base.DrawBorder(
					graph, 
					borderSkin,
					rect, 
					backColor, 
					backHatchStyle, 
					backImage, 
					backImageWrapMode, 
					backImageTransparentColor, 
					backImageAlign, 
					backGradientStyle, 
					backSecondaryColor, 
					borderColor, 
					borderWidth, 
					borderDashStyle	);
				borderSkin.PageColor = oldPageColor;
				cornerRadius = oldCorners;
			}
		}

		#endregion
	}


	/// <summary>
	/// Implements raised border.
	/// </summary>
	internal class RaisedBorder : SunkenBorder
	{
		#region Border properties and methods

		/// <summary>
		/// Public constructor
		/// </summary>
		public RaisedBorder()
		{
			sunken = false;
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return "Raised";}}

		#endregion
	}

	/// <summary>
	/// Implements embed 3D border.
	/// </summary>
	internal class SunkenBorder : IBorderType
	{
		#region Border properties and methods

		/// <summary>
		/// Radius for rounded rectangle
		/// </summary>
        internal float defaultRadiusSize = 15f;

		/// <summary>
		/// Outside shadow rate
		/// </summary>
        internal float outsideShadowRate = .9f;
		
		/// <summary>
		/// Indicates that sunken shadows should be drawn
		/// </summary>
        internal bool sunken = true;

		/// <summary>
		/// Indicates that bottom shadow should be drawn
		/// </summary>
        internal bool drawBottomShadow = true;

		/// <summary>
		/// Indicates that top left outside dark shadow must be drawn
		/// </summary>
        internal bool drawOutsideTopLeftShadow = false;

		/// <summary>
		/// Array of corner radius
		/// </summary>
        internal float[] cornerRadius = { 15f, 15f, 15f, 15f, 15f, 15f, 15f, 15f };

		/// <summary>
		/// Border top/left size 
		/// </summary>
        internal SKSize sizeLeftTop = SKSize.Empty;

		/// <summary>
		/// Border right/bottom size
		/// </summary>
        internal SKSize sizeRightBottom = SKSize.Empty;

		/// <summary>
		/// Indicates that screws should be drawn in the corners of the frame
		/// </summary>
        internal bool drawScrews = false;


        internal float resolution = 96f;


		/// <summary>
		/// Public constructor
		/// </summary>
		public SunkenBorder()
		{
		}

		/// <summary>
		/// Chart type name
		/// </summary>
		public virtual string Name			{ get{ return "Sunken";}}


        public virtual float Resolution
        {
			get
            {
				return resolution;
            }
            set
            {
                resolution = value;
                defaultRadiusSize = 15 * resolution / 96;
                //X = defaultRadiusSize;
                //Y = defaultRadiusSize;
                cornerRadius = new float[] { defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize, defaultRadiusSize };
            }
        }

		/// <summary>
		/// Returns the position of the rectangular area in the border where
		/// title should be displayed. Returns empty rect if title can't be shown in the border.
		/// </summary>
		/// <returns>Title position in border.</returns>
		public virtual SKRect GetTitlePositionInBorder()
		{
			return SKRect.Empty;
		}

        /// <summary>
        /// Adjust areas rectangle coordinate to fit the 3D border
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="areasRect">Position to adjust.</param>
		public virtual void AdjustAreasPosition(ChartGraphics graph, ref SKRect areasRect)
		{
			SKSize relSizeLeftTop = new(sizeLeftTop.Width, sizeLeftTop.Height);
			SKSize relSizeRightBottom = new(sizeRightBottom.Width, sizeRightBottom.Height);
			relSizeLeftTop.Width += defaultRadiusSize * 0.7f;
			relSizeLeftTop.Height += defaultRadiusSize * 0.85f;
			relSizeRightBottom.Width += defaultRadiusSize * 0.7f;
			relSizeRightBottom.Height += defaultRadiusSize * 0.7f;
			relSizeLeftTop = graph.GetRelativeSize(relSizeLeftTop);
			relSizeRightBottom = graph.GetRelativeSize(relSizeRightBottom);

			if(relSizeLeftTop.Width > 30f)
				relSizeLeftTop.Width = 0;
			if(relSizeLeftTop.Height > 30f)
				relSizeLeftTop.Height = 0;
			if(relSizeRightBottom.Width > 30f)
				relSizeRightBottom.Width = 0;
			if(relSizeRightBottom.Height > 30f)
				relSizeRightBottom.Height = 0;


			areasRect.Left += relSizeLeftTop.Width;
			areasRect.Right -= Math.Min(areasRect.Width, relSizeLeftTop.Width + relSizeRightBottom.Width);
			areasRect.Top += relSizeLeftTop.Height;
			areasRect.Bottom -= Math.Min(areasRect.Height, relSizeLeftTop.Height + relSizeRightBottom.Height);

			if(areasRect.Right > 100f)
			{
				if(areasRect.Width > 100f - areasRect.Right)
					areasRect.Right -= 100f - areasRect.Right;
				else
					areasRect.Left -= 100f - areasRect.Right;
			}
			if(areasRect.Bottom > 100f)
			{
				if(areasRect.Height > 100f - areasRect.Bottom)
					areasRect.Bottom -= 100f - areasRect.Bottom;
				else
					areasRect.Top -= 100f - areasRect.Bottom;

			}
		}

        /// <summary>
        /// Draws 3D border
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="borderSkin">Border skin object.</param>
        /// <param name="rect">Rectangle of the border.</param>
        /// <param name="backColor">Color of rectangle</param>
        /// <param name="backHatchStyle">Hatch style</param>
        /// <param name="backImage">Back Image</param>
        /// <param name="backImageWrapMode">Image mode</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
        /// <param name="backGradientStyle">Gradient type</param>
        /// <param name="backSecondaryColor">Gradient End Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Width</param>
        /// <param name="borderDashStyle">Border Style</param>
		public virtual void DrawBorder(
			ChartGraphics graph, 
			BorderSkin borderSkin,
			SKRect rect, 
			SKColor backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			SKColor backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			SKColor backSecondaryColor, 
			SKColor borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle)	
		{
			SKRect absolute = ChartGraphics.Round( rect );

            // Calculate shadow colors (0.2 - 0.6)
            float colorDarkeningIndex = 0.3f + (0.4f * (borderSkin.PageColor.Red + borderSkin.PageColor.Green + borderSkin.PageColor.Blue) / 765f);
            SKColor	shadowColor = new(
				(byte)(backColor.Red*colorDarkeningIndex), 
				(byte)(backColor.Green*colorDarkeningIndex), 
				(byte)(backColor.Blue*colorDarkeningIndex));

			colorDarkeningIndex += 0.2f;
			SKColor	shadowLightColor = new(
				(byte)(borderSkin.PageColor.Red*colorDarkeningIndex), 
				(byte)(borderSkin.PageColor.Green*colorDarkeningIndex), 
				(byte)(borderSkin.PageColor.Blue*colorDarkeningIndex));
			if(borderSkin.PageColor == SKColors.Transparent)
			{
				shadowLightColor = new SKColor(0, 0, 0, 60);
			}
			
			// Calculate rounded rect radius
			float	radius = defaultRadiusSize;
            radius = Math.Max(radius, 2f * resolution / 96.0f);
			radius = Math.Min(radius, rect.Width/2f);
			radius = Math.Min(radius, rect.Height/2f);
			radius = (float)Math.Ceiling(radius);

			// Fill page background color
			using (SKPaint brush = new() { Color = borderSkin.PageColor, Style = SKPaintStyle.Fill })
			{
				graph.FillRectangle(brush, rect);
			}

            SKRect shadowRect;
            if (drawOutsideTopLeftShadow)
            {
                // Top/Left outside shadow
                shadowRect = absolute;
                shadowRect.Left -= radius * 0.3f;
                shadowRect.Top -= radius * 0.3f;
                shadowRect.Right -= radius * .3f;
                shadowRect.Bottom -= radius * .3f;
                graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius, Color.FromArgb(128, SKColors.Black), borderSkin.PageColor, outsideShadowRate);
            }

            // Bottom/Right outside shadow
            shadowRect = absolute;
			shadowRect.Left += radius * 0.3f;
			shadowRect.Top += radius * 0.3f;
			shadowRect.Right -= radius * .3f;
			shadowRect.Bottom -= radius * .3f;
			graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius, shadowLightColor, borderSkin.PageColor, outsideShadowRate);

			// Background
			shadowRect = absolute;
			shadowRect.Right -= radius * .3f;
			shadowRect.Bottom -= radius * .3f;
			SKPath path = ChartGraphics.CreateRoundedRectPath(shadowRect, cornerRadius);
			graph.DrawPathAbs(
				path, 
				backColor, 
				backHatchStyle, 
				backImage, 
				backImageWrapMode, 
				backImageTransparentColor,
				backImageAlign,
				backGradientStyle, 
				backSecondaryColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle, 
				PenAlignment.Inset );

			// Dispose Graphic path
			if( path != null )
				path.Dispose();

			// Draw screws imitation in the corners of the farame
			if(drawScrews)
			{
				// Left/Top screw
				SKRect	screwRect = SKRect.Empty;
				float offset = radius * 0.4f;
				screwRect.Left = shadowRect.Left + offset;
				screwRect.Top = shadowRect.Top + offset;
				screwRect.Size = new(radius * 0.55f,screwRect.Width);
				DrawScrew(graph, screwRect);

				// Right/Top screw
				screwRect.Left = shadowRect.Right - offset - screwRect.Width;
				DrawScrew(graph, screwRect);

				// Right/Bottom screw
				screwRect.Left = shadowRect.Right - offset - screwRect.Width;
				screwRect.Top = shadowRect.Bottom - offset - screwRect.Height;
				DrawScrew(graph, screwRect);
		
				// Left/Bottom screw
				screwRect.Left = shadowRect.Left + offset;
				screwRect.Top = shadowRect.Bottom - offset - screwRect.Height;
				DrawScrew(graph, screwRect);
			}

			// Bottom/Right inner shadow
			SKRegion	innerShadowRegion = null;
			if(drawBottomShadow)
			{
				shadowRect = absolute;
				shadowRect.Right -= radius * .3f;
				shadowRect.Bottom -= radius * .3f;
				innerShadowRegion = new SKRegion(
                    ChartGraphics.CreateRoundedRectPath(
					new SKRect(
					shadowRect.Left - radius, 
					shadowRect.Top - radius, 
					shadowRect.Width + 0.5f*radius, 
					shadowRect.Height + 0.5f*radius),
                    cornerRadius));
                
				// TODO: innerShadowRegion.Complement(graph.CreateRoundedRectPath(shadowRect, cornerRadius));
				
				graph.Clip = innerShadowRegion;

				shadowRect.Left -= 0.5f*radius;
				shadowRect.Top -= 0.5f*radius;
				shadowRect.Right += 0.5f*radius;
				shadowRect.Bottom += 0.5f*radius;

				graph.DrawRoundedRectShadowAbs(
					shadowRect, 
					cornerRadius,
					radius,
					SKColors.Transparent, 
					Color.FromArgb(175, (sunken) ? SKColors.White : shadowColor), 
					1.0f);
				graph.Clip = new SKRegion();
			}

			// Top/Left inner shadow					
			shadowRect = absolute;
			shadowRect.Right -= radius * .3f;
			shadowRect.Bottom -= radius * .3f;
			innerShadowRegion = new(
                ChartGraphics.CreateRoundedRectPath(
				new SKRect(
				shadowRect.Left + radius*.5f, 
				shadowRect.Top + radius*.5f, 
				shadowRect.Width - .2f*radius, 
				shadowRect.Height - .2f*radius), 
				cornerRadius));

			SKRect shadowWithOffset = shadowRect;
			shadowWithOffset.Right += radius;
			shadowWithOffset.Bottom += radius;
			// TODO: innerShadowRegion.Complement(graph.CreateRoundedRectPath(shadowWithOffset, cornerRadius));
			
			innerShadowRegion.SetPath(ChartGraphics.CreateRoundedRectPath(shadowRect, cornerRadius), innerShadowRegion);
			graph.Clip = innerShadowRegion;
			graph.DrawRoundedRectShadowAbs(
				shadowWithOffset, 
				cornerRadius, 
				radius, 
				SKColors.Transparent, 
				Color.FromArgb(175, (sunken) ? shadowColor : SKColors.White), 
				1.0f);
			graph.Clip = new();
		}

		/// <summary>
		/// Helper function, which draws a screw on the frame
		/// </summary>
		/// <param name="graph">Chart graphics to use.</param>
		/// <param name="rect">Screw position.</param>
		private void DrawScrew(ChartGraphics graph, SKRect rect)
		{
			// Draw screw
			SKPaint screwPen = new() { Color = Color.FromArgb(128, 255, 255, 255), StrokeWidth = 1, Style = SKPaintStyle.Stroke };
			graph.DrawEllipse(screwPen, rect.Left, rect.Top, rect.Width, rect.Height);
            graph.DrawLine(screwPen, rect.Left + 2 * resolution / 96.0f, rect.Top + rect.Height - 2 * resolution / 96.0f, rect.Right - 2 * resolution / 96.0f, rect.Top + 2 * resolution / 96.0f);
			screwPen.Color = Color.FromArgb(128, SKColors.Black);
            graph.DrawEllipse(screwPen, rect.Left + 1 * resolution / 96.0f, rect.Top + 1 * resolution / 96.0f, rect.Width, rect.Height);
            graph.DrawLine(screwPen, rect.Left + 3 * resolution / 96.0f, rect.Top + rect.Height - 1 * resolution / 96.0f, rect.Right - 1 * resolution / 96.0f, rect.Top + 3 * resolution / 96.0f);
		}

		#endregion
	}
}
