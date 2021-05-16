// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Class is used to store relative position of the chart
//				elements like Legend, Title and others. It uses
//              relative coordinate system where top left corner is
//              0,0 and bottom right is 100,100.
//              :
//              If Auto property is set to true, all position properties 
//              (X,Y,Width and Height) are ignored and they automatically
//              calculated during chart rendering.
//              :
//              Note that setting any of the position properties will 
//              automatically set Auto property to false.
//


using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Utilities
{
    /// <summary>
    /// ElementPosition is the base class for many chart visual 
    /// elements like Legend, Title and ChartArea. It provides 
    /// the position of the chart element in relative coordinates, 
    /// from (0,0) to (100,100).
    /// </summary>
    [
        SRDescription("DescriptionAttributeElementPosition_ElementPosition"),
    ]
    public class ElementPosition : ChartElement
    {
        #region Fields

        // Private data members, which store properties values
        private float _x = 0;
        private float _y = 0;
        private float _width = 0;
        private float _height = 0;
        internal bool _auto = true;

        // Indicates the auto position of all areas must be reset
        internal bool resetAreaAutoPosition = false;

        #endregion

        #region Constructors

        /// <summary>
        /// ElementPosition default constructor
        /// </summary>
        public ElementPosition()
        {
        }

        /// <summary>
        /// ElementPosition default constructor
        /// </summary>
        internal ElementPosition(IChartElement parent)
            : base(parent)
        {
        }


        /// <summary>
        /// ElementPosition constructor.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public ElementPosition(float x, float y, float width, float height)
        {
            _auto = false;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert element position into SKRect
        /// </summary>
        /// <returns>SKRect structure.</returns>
        public SKRect ToSKRect()
        {
            return new SKRect(_x, _y, _x + _width, _y + _height);
        }

        /// <summary>
        /// Initializes ElementPosition from SKRect
        /// </summary>
        /// <param name="rect">SKRect structure.</param>
        public void FromSKRect(SKRect rect)
        {
            if (rect == SKRect.Empty)
                throw new ArgumentNullException(nameof(rect));

            _x = rect.Left;
            _y = rect.Top;
            _width = rect.Width;
            _height = rect.Height;
            _auto = false;
        }

        /// <summary>
        /// Gets the size of the ElementPosition object.
        /// </summary>
        /// <returns>The size of the ElementPosition object.</returns>
        public SKSize Size
        {
            get { return new SKSize(_width, _height); }
        }

        /// <summary>
        /// Gets the bottom position in relative coordinates.
        /// </summary>
        /// <returns>Bottom position.</returns>
        public float Bottom
        {
            get { return _y + _height; }
        }

        /// <summary>
        /// Gets the right position in relative coordinates.
        /// </summary>
        /// <returns>Right position.</returns>
        public float Right
        {
            get { return _x + _width; }
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object.
        /// </summary>
        /// <param name="obj">The Object to compare with the current Object.</param>
        /// <returns>true if the specified Object is equal to the current Object; otherwise, false.</returns>
		internal override bool EqualsInternal(object obj)
        {
            if (obj is ElementPosition pos)
            {
                if (_auto && _auto == pos._auto)
                {
                    return true;
                }
                else if (_x == pos._x && _y == pos._y &&
                        _width == pos._width && _height == pos._height)
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Returns a string that represents the element position data.
        /// </summary>
        /// <returns>Element position data as a string.</returns>
        internal override string ToStringInternal()
        {
            string posString = Constants.AutoValue;
            if (!_auto)
            {
                posString =
                    _x.ToString(System.Globalization.CultureInfo.CurrentCulture) + ", " +
                    _y.ToString(System.Globalization.CultureInfo.CurrentCulture) + ", " +
                    _width.ToString(System.Globalization.CultureInfo.CurrentCulture) + ", " +
                    _height.ToString(System.Globalization.CultureInfo.CurrentCulture);
            }
            return posString;
        }

        /// <summary>
        /// Set the element position without modifying the "Auto" property
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        internal void SetPositionNoAuto(float x, float y, float width, float height)
        {
            bool oldValue = _auto;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _auto = oldValue;
        }

        #endregion

        #region Element Position properties

        /// <summary>
        /// X position of element.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeElementPosition_X"),
        ]
        public float X
        {
            get
            {
                return _x;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionElementPositionArgumentOutOfRange));
                }
                _x = value;
                Auto = false;

                // Adjust width
                if ((_x + Width) > 100)
                {
                    Width = 100 - _x;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Y position of element.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeElementPosition_Y"),
        ]
        public float Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionElementPositionArgumentOutOfRange));
                }
                _y = value;
                Auto = false;

                // Adjust heigth
                if ((_y + Height) > 100)
                {
                    Height = 100 - _y;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Width of element.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeElementPosition_Width"),
        ]
        public float Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionElementPositionArgumentOutOfRange));
                }
                _width = value;
                Auto = false;

                // Adjust x
                if ((_x + Width) > 100)
                {
                    _x = 100 - Width;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Height of element.
        /// </summary>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeElementPosition_Height"),
        ]
        public float Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (value < 0.0 || value > 100.0)
                {
                    throw (new ArgumentOutOfRangeException(nameof(value), SR.ExceptionElementPositionArgumentOutOfRange));
                }
                _height = value;
                Auto = false;

                // Adjust y
                if ((_y + Height) > 100)
                {
                    _y = 100 - Height;

                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether positioning is on.
        /// </summary>
		[
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeElementPosition_Auto"),
        ]
        public bool Auto
        {
            get
            {
                return _auto;
            }
            set
            {
                if (value != _auto)
                {
                    if (value)
                    {
                        _x = 0;
                        _y = 0;
                        _width = 0;
                        _height = 0;
                    }
                    _auto = value;

                    Invalidate();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Used for invoking windows forms MesageBox dialog.
    /// </summary>
    internal interface IDesignerMessageBoxDialog
    {
        /// <summary>
        /// Shows Yes/No MessageBox.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// true if user confirms with Yes
        /// </returns>
        bool ShowQuestion(string message);
    }
}
