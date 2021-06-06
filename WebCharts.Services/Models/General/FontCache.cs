// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	This file contains classes, which are used for Image 
//				creation and chart painting. This file has also a 
//				class, which is used for Paint events arguments.
//


using SkiaSharp;
using System;
using System.Collections.Generic;

namespace WebCharts.Services
{

    /// <summary>
    /// Font cache class helps ChartElements to reuse the Font instances
    /// </summary>
    internal class FontCache : IDisposable
    {
        #region Static

        /// <summary>
        /// Gets the default font family name.
        /// </summary>
        /// <value>The default font family name.</value>
        public static string DefaultFamilyName
        {
            get
            {
                return "Microsoft Sans Serif";
            }
        }
        #endregion

        #region Fields

        // Cached fonts dictionary 
        private readonly Dictionary<KeyInfo, SKFont> _fontCache = new(new KeyInfo.EqualityComparer());

        #endregion // Fields

        #region Properties
        /// <summary>
        /// Gets the default font.
        /// </summary>
        /// <value>The default font.</value>
        public SKFont DefaultFont
        {
            get { return GetFont(DefaultFamilyName, 8); }
        }

        /// <summary>
        /// Gets the default font.
        /// </summary>
        /// <value>The default font.</value>
        public SKFont DefaultBoldFont
        {
            get { return GetFont(DefaultFamilyName, 8, SKFontStyle.Bold); }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the font.
        /// </summary>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="size">The size.</param>
        /// <returns>Font instance</returns>
        public SKFont GetFont(string familyName, int size)
        {
            KeyInfo key = new(familyName, size);
            if (!_fontCache.ContainsKey(key))
            {
                _fontCache.Add(key, new SKFont(SKTypeface.FromFamilyName(familyName), size));
            }
            return _fontCache[key];
        }

        /// <summary>
        /// Gets the font.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="size">The size.</param>
        /// <param name="style">The style.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>Font instance</returns>
        public SKFont GetFont(string family, float size, SKFontStyle style)
        {
            KeyInfo key = new(family, size, style);
            if (!_fontCache.ContainsKey(key))
            {
                _fontCache.Add(key, new SKFont(SKTypeface.FromFamilyName(family, style), size));
            }
            return _fontCache[key];
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (SKFont font in _fontCache.Values)
                {
                    font.Dispose();
                }
                _fontCache.Clear();
            }
        }

        #endregion

        #region FontKeyInfo struct
        /// <summary>
        /// Font key info
        /// </summary>
        private class KeyInfo
        {
            readonly string _familyName;
            readonly float _size;
            readonly SKFontStyle _style = SKFontStyle.Normal;
            readonly int _gdiCharSet = 1;

            #region IEquatable<FontKeyInfo> Members
            /// <summary>
            /// KeyInfo equality comparer
            /// </summary>
            internal class EqualityComparer : IEqualityComparer<KeyInfo>
            {
                /// <summary>
                /// Determines whether the specified objects are equal.
                /// </summary>
                /// <param name="x">The first object of type <paramref name="x"/> to compare.</param>
                /// <param name="y">The second object of type <paramref name="y"/> to compare.</param>
                /// <returns>
                /// true if the specified objects are equal; otherwise, false.
                /// </returns>
                public bool Equals(KeyInfo x, KeyInfo y)
                {
                    return
                        x._size == y._size &&
                        x._familyName == y._familyName &&
                        x._style == y._style &&
                        x._gdiCharSet == y._gdiCharSet;
                }

                /// <summary>
                /// Returns a hash code for the specified object.
                /// </summary>
                /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
                /// <returns>A hash code for the specified object.</returns>
                /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
                public int GetHashCode(KeyInfo obj)
                {
                    return obj._familyName.GetHashCode() ^ obj._size.GetHashCode();
                }
            }
            #endregion
        }
        #endregion
    }



}
