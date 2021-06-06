// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Class that implements Emboss 3D border style.
//

using SkiaSharp;
using System;

namespace WebCharts.Services
{
    /// <summary>
    /// Implements emboss 3D border.
    /// </summary>
    internal class EmbossBorder : IBorderType
    {
        #region Border properties and methods

        /// <summary>
        /// Default border radius size (relative)
        /// </summary>
        public float defaultRadiusSize = 15f;

        public float resolution = 96f;

        /// <summary>
        /// Array of corner radius
        /// </summary>
        internal float[] cornerRadius = { 15f, 15f, 15f, 15f, 15f, 15f, 15f, 15f };

        /// <summary>
        /// Public constructor
        /// </summary>
        public EmbossBorder()
        {
        }

        /// <summary>
        /// Chart type name
        /// </summary>
        public virtual string Name { get { return "Emboss"; } }

        public virtual float Resolution
        {
            set
            {
                resolution = value;
                float radius = 15f * value / 96.0f;
                defaultRadiusSize = radius;
                cornerRadius = new float[] { radius, radius, radius, radius, radius, radius, radius, radius };
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
        /// Adjust areas rectangle coordinate to fit the 3D border.
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="areasRect">Position to adjust.</param>
		public virtual void AdjustAreasPosition(ChartGraphics graph, ref SKRect areasRect)
        {
            SKSize borderSize = new(defaultRadiusSize / 2f, defaultRadiusSize / 2f);
            borderSize = graph.GetRelativeSize(borderSize);

            // Do not do anything if rectangle is too small
            if (borderSize.Width < 30f)
            {
                areasRect.Top += borderSize.Width;
                areasRect.Right -= Math.Min(areasRect.Width, borderSize.Width * 2.5f);
            }

            if (borderSize.Height < 30f)
            {
                areasRect.Top += borderSize.Height;
                areasRect.Bottom -= Math.Min(areasRect.Height, borderSize.Height * 2.5f);
            }

            if (areasRect.Left + areasRect.Width > 100f)
            {
                areasRect.Left -= 100f - areasRect.Width;
            }
            if (areasRect.Top + areasRect.Height > 100f)
            {
                areasRect.Top -= 100f - areasRect.Height;
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
            SKRect absolute = ChartGraphics.Round(rect);

            // Calculate shadow colors (0.2 - 0.6)
            float colorDarkeningIndex = 0.2f + (0.4f * (borderSkin.PageColor.Red + borderSkin.PageColor.Green + borderSkin.PageColor.Blue) / 765f);
            SKColor shadowColor = new(
                (byte)(borderSkin.PageColor.Red * colorDarkeningIndex),
                (byte)(borderSkin.PageColor.Green * colorDarkeningIndex),
                (byte)(borderSkin.PageColor.Blue * colorDarkeningIndex));
            if (borderSkin.PageColor == SKColors.Transparent)
            {
                shadowColor = new(0, 0, 0, 60);
            }

            colorDarkeningIndex += 0.2f;
            SKColor shadowLightColor = new(
                (byte)(borderSkin.PageColor.Red * colorDarkeningIndex),
                (byte)(borderSkin.PageColor.Green * colorDarkeningIndex),
                (byte)(borderSkin.PageColor.Blue * colorDarkeningIndex));

            // Calculate rounded rect radius
            float radius = defaultRadiusSize;
            radius = Math.Max(radius, 2f * resolution / 96.0f);
            radius = Math.Min(radius, rect.Width / 2f);
            radius = Math.Min(radius, rect.Height / 2f);
            radius = (float)Math.Ceiling(radius);

            // Fill page background color
            using (SKPaint brush = new() { Style = SKPaintStyle.Fill, Color = borderSkin.PageColor })
            {
                graph.FillRectangle(brush, rect);
            }

            // Top/Left shadow
            SKRect shadowRect = absolute;
            shadowRect.Right -= radius * .3f;
            shadowRect.Bottom -= radius * .3f;
            graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius + 1 * resolution / 96.0f, shadowLightColor, borderSkin.PageColor, 1.4f);

            // Bottom/Right shadow
            shadowRect = absolute;
            shadowRect.Left = absolute.Left + radius / 3f;
            shadowRect.Top = absolute.Top + radius / 3f;
            shadowRect.Right -= radius / 3.5f;
            shadowRect.Bottom -= radius / 3.5f;
            graph.DrawRoundedRectShadowAbs(shadowRect, cornerRadius, radius, shadowColor, borderSkin.PageColor, 1.3f);

            // Draw Background
            shadowRect = absolute;
            shadowRect.Left = absolute.Left + 3f * resolution / 96.0f;
            shadowRect.Top = absolute.Top + 3f * resolution / 96.0f;
            shadowRect.Right -= radius * .75f;
            shadowRect.Bottom -= radius * .75f;
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
                PenAlignment.Inset);

            // Dispose Graphic path
            if (path != null)
                path.Dispose();

            // Bottom/Right inner shadow
            SKRegion innerShadowRegion = new(
                ChartGraphics.CreateRoundedRectPath(
                new SKRect(
                shadowRect.Left - radius,
                shadowRect.Top - radius,
                shadowRect.Width + radius - radius * 0.25f,
                shadowRect.Height + radius - radius * 0.25f),
                cornerRadius));

            innerShadowRegion.Op(ChartGraphics.CreateRoundedRectPath(shadowRect, cornerRadius), SKRegionOperation.Difference);
            graph.Clip = innerShadowRegion;
            graph.DrawRoundedRectShadowAbs(
                shadowRect,
                cornerRadius,
                radius,
                SKColors.Transparent,
                new(SKColors.Gray.Red, SKColors.Gray.Green, SKColors.Gray.Blue, 128),
                .5f);
            graph.Clip = new();
        }

        #endregion Border properties and methods
    }
}