// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	3D borders related classes:
//				  BorderTypeRegistry	- known borders registry.
//				  IBorderType			- border class interface.
//				  BorderSkin	        - border visual properties.
//

using SkiaSharp;
using System;
using System.Collections;
using System.Reflection;
using System.Resources;

namespace WebCharts.Services
{
    #region Border style enumeration

    /// <summary>
    /// Styles of the border skin.
    /// </summary>
    public enum BorderSkinStyle
    {
        /// <summary>
        /// Border not used.
        /// </summary>
        None,

        /// <summary>
        /// Emboss border.
        /// </summary>
        Emboss,

        /// <summary>
        /// Raised border.
        /// </summary>
        Raised,

        /// <summary>
        /// Sunken border.
        /// </summary>
        Sunken,

        /// <summary>
        /// Thin border with rounded corners.
        /// </summary>
        FrameThin1,

        /// <summary>
        /// Thin border with rounded top corners.
        /// </summary>
        FrameThin2,

        /// <summary>
        /// Thin border with square corners.
        /// </summary>
        FrameThin3,

        /// <summary>
        /// Thin border with square outside corners and rounded inside corners.
        /// </summary>
        FrameThin4,

        /// <summary>
        /// Thin border with rounded corners and screws.
        /// </summary>
        FrameThin5,

        /// <summary>
        /// Thin border with square inside corners and rounded outside corners.
        /// </summary>
        FrameThin6,

        /// <summary>
        /// Border with rounded corners. Supports title text.
        /// </summary>
        FrameTitle1,

        /// <summary>
        /// Border with rounded top corners. Supports title text.
        /// </summary>
        FrameTitle2,

        /// <summary>
        /// Border with square corners. Supports title text.
        /// </summary>
        FrameTitle3,

        /// <summary>
        /// Border with rounded inside corners and square outside corners. Supports title text.
        /// </summary>
        FrameTitle4,

        /// <summary>
        /// Border with rounded corners and screws. Supports title text.
        /// </summary>
        FrameTitle5,

        /// <summary>
        /// Border with rounded outside corners and square inside corners. Supports title text.
        /// </summary>
        FrameTitle6,

        /// <summary>
        /// Border with rounded corners. No border on the right side. Supports title text.
        /// </summary>
        FrameTitle7,

        /// <summary>
        /// Border with rounded corners on top and bottom sides only. Supports title text.
        /// </summary>
        FrameTitle8
    }

    #endregion Border style enumeration

    /// <summary>
    /// Drawing properties of the 3D border skin.
    /// </summary>
    [
        SRDescription("DescriptionAttributeBorderSkin_BorderSkin"),
    ]
    public class BorderSkin : ChartElement
    {
        #region Fields

        // Private data members, which store properties values
        private SKColor _pageColor = SKColors.White;

        private BorderSkinStyle _skinStyle = BorderSkinStyle.None;
        private GradientStyle _backGradientStyle = GradientStyle.None;
        private SKColor _backSecondaryColor = SKColor.Empty;
        private SKColor _backColor = SKColors.Gray;
        private string _backImage = "";
        private ChartImageWrapMode _backImageWrapMode = ChartImageWrapMode.Tile;
        private SKColor _backImageTransparentColor = SKColor.Empty;
        private ChartImageAlignmentStyle _backImageAlignment = ChartImageAlignmentStyle.TopLeft;
        private SKColor _borderColor = SKColors.Black;
        private int _borderWidth = 1;
        private ChartDashStyle _borderDashStyle = ChartDashStyle.NotSet;
        private ChartHatchStyle _backHatchStyle = ChartHatchStyle.None;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public BorderSkin() : base()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
        internal BorderSkin(IChartElement parent) : base(parent)
        {
        }

        #endregion Constructors

        #region Border skin properties

        /// <summary>
        /// Gets or sets the page color of a border skin.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin_PageColor"),
        ]
        public SKColor PageColor
        {
            get
            {
                return _pageColor;
            }
            set
            {
                _pageColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the style of a border skin.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin_SkinStyle"),
        ]
        public BorderSkinStyle SkinStyle
        {
            get
            {
                return _skinStyle;
            }
            set
            {
                _skinStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background color of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeFrameBackColor"),
        ]
        public SKColor BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                _backColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the border color of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderColor"),
        ]
        public SKColor BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background hatch style of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeFrameBackHatchStyle")
        ]
        public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return _backHatchStyle;
            }
            set
            {
                _backHatchStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background image of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImage"),
        ]
        public string BackImage
        {
            get
            {
                return _backImage;
            }
            set
            {
                _backImage = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the drawing mode for the background image of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageWrapMode"),
        ]
        public ChartImageWrapMode BackImageWrapMode
        {
            get
            {
                return _backImageWrapMode;
            }
            set
            {
                _backImageWrapMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color
        /// while drawing the background image of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        ]
        public SKColor BackImageTransparentColor
        {
            get
            {
                return _backImageTransparentColor;
            }
            set
            {
                _backImageTransparentColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background image alignment of a skin frame.
        /// </summary>
        /// <remarks>
        /// Used by ClampUnscale drawing mode.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackImageAlign"),
        ]
        public ChartImageAlignmentStyle BackImageAlignment
        {
            get
            {
                return _backImageAlignment;
            }
            set
            {
                _backImageAlignment = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the background gradient style of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle")
        ]
        public GradientStyle BackGradientStyle
        {
            get
            {
                return _backGradientStyle;
            }
            set
            {
                _backGradientStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the secondary background color of a skin frame.
        /// </summary>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
		[

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin_FrameBackSecondaryColor"),
        ]
        public SKColor BackSecondaryColor
        {
            get
            {
                return _backSecondaryColor;
            }
            set
            {
                _backSecondaryColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the width of the border line of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin_FrameBorderWidth"),
        ]
        public int BorderWidth
        {
            get
            {
                return _borderWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionBorderWidthIsNotPositive));
                }
                _borderWidth = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the style of the border line of a skin frame.
        /// </summary>
        [

        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBorderSkin_FrameBorderDashStyle"),
        ]
        public ChartDashStyle BorderDashStyle
        {
            get
            {
                return _borderDashStyle;
            }
            set
            {
                _borderDashStyle = value;
                Invalidate();
            }
        }

        #endregion Border skin properties
    }

    /// <summary>
    /// Keep track of all registered 3D borders.
    /// </summary>
    internal class BorderTypeRegistry : IServiceProvider, IBorderTypeRegistry
    {
        #region Fields

        // Border types image resource manager
        private ResourceManager _resourceManager = null;

        // Storage for all registered border types
        internal Hashtable registeredBorderTypes = new(StringComparer.OrdinalIgnoreCase);

        private readonly Hashtable _createdBorderTypes = new(StringComparer.OrdinalIgnoreCase);

        #endregion Fields

        #region Constructors and services

        /// <summary>
        /// Border types registry public constructor
        /// </summary>
        public BorderTypeRegistry()
        {
        }

        /// <summary>
        /// Returns border type registry service object
        /// </summary>
        /// <param name="serviceType">Service type to get.</param>
        /// <returns>Border registry service.</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(BorderTypeRegistry))
            {
                return this;
            }
            throw (new ArgumentException(SR.ExceptionBorderTypeRegistryUnsupportedType(serviceType.ToString())));
        }

        #endregion Constructors and services

        #region Methods

        /// <summary>
        /// Adds 3D border type into the registry.
        /// </summary>
        /// <param name="name">Border type name.</param>
        /// <param name="borderType">Border class type.</param>
        public void Register(string name, Type borderType)
        {
            // First check if border type with specified name already registered
            if (registeredBorderTypes.Contains(name))
            {
                // If same type provided - ignore
                if (registeredBorderTypes[name].GetType() == borderType)
                {
                    return;
                }

                // Error - throw exception
                throw (new ArgumentException(SR.ExceptionBorderTypeNameIsNotUnique(name)));
            }

            // Make sure that specified class support IBorderType interface
            bool found = false;
            Type[] interfaces = borderType.GetInterfaces();
            foreach (Type type in interfaces)
            {
                if (type == typeof(IBorderType))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                throw (new ArgumentException(SR.ExceptionBorderTypeHasNoInterface));
            }

            // Add border type to the hash table
            registeredBorderTypes[name] = borderType;
        }

        /// <summary>
        /// Returns border type object by name.
        /// </summary>
        /// <param name="name">Border type name.</param>
        /// <returns>Border type object derived from IBorderType.</returns>
        public IBorderType GetBorderType(string name)
        {
            // First check if border type with specified name registered
            if (!registeredBorderTypes.Contains(name))
            {
                throw (new ArgumentException(SR.ExceptionBorderTypeUnknown(name)));
            }

            // Check if the border type object is already created
            if (!_createdBorderTypes.Contains(name))
            {
                // Create border type object
                _createdBorderTypes[name] =
                    ((Type)registeredBorderTypes[name]).Assembly.
                    CreateInstance(((Type)registeredBorderTypes[name]).ToString());
            }

            return (IBorderType)_createdBorderTypes[name];
        }

        /// <summary>
        /// Border images resource manager.
        /// </summary>
        public ResourceManager ResourceManager
        {
            get
            {
                // Create border images resource manager
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager("System.Web.UI.DataVisualization.Charting", Assembly.GetExecutingAssembly());
                }
                return _resourceManager;
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Interface which defines the set of standard methods and
    /// properties for each border type.
    /// </summary>
	internal interface IBorderType
    {
        #region Properties and Method

        /// <summary>
        /// Border type name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Sets/Gets the resolution to draw with;
        /// </summary>
        float Resolution
        {
            set;
        }

        /// <summary>
        /// Draws 3D border.
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="borderSkin">Border skin object.</param>
        /// <param name="rect">Rectangle of the border.</param>
        /// <param name="backColor">Color of rectangle.</param>
        /// <param name="backHatchStyle">Hatch style.</param>
        /// <param name="backImage">Back Image.</param>
        /// <param name="backImageWrapMode">Image mode.</param>
        /// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
        /// <param name="backGradientStyle">Gradient type.</param>
        /// <param name="backSecondaryColor">Gradient End Color.</param>
        /// <param name="borderColor">Border Color.</param>
        /// <param name="borderWidth">Border Width.</param>
        /// <param name="borderDashStyle">Border Style.</param>
		void DrawBorder(
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
            ChartDashStyle borderDashStyle);

        /// <summary>
        /// Adjust areas rectangle coordinate to fit the 3D border.
        /// </summary>
        /// <param name="graph">Graphics to draw the border on.</param>
        /// <param name="areasRect">Position to adjust.</param>
		void AdjustAreasPosition(ChartGraphics graph, ref SKRect areasRect);

        /// <summary>
        /// Returns the position of the rectangular area in the border where
        /// title should be displayed. Returns empty rect if title can't be shown in the border.
        /// </summary>
        /// <returns>Title position in border.</returns>
        SKRect GetTitlePositionInBorder();

        #endregion Properties and Method
    }
}