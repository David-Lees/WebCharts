// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Every property in the chart references images by names.
//              This means that you can set MarkerImage property to a
//              full image path or URL. In case when the user wants to
//              dynamically generate an image or load it from other
//              location (like database) you can use named image
//              collection which is exposed as Images property of the
//              chart. Any Image can be added to this collection with
//              unique name and than this name can be used in all the
//              chart properties which require image names.
//

using SkiaSharp;

namespace WebCharts.Services
{
    /// <summary>
    /// The NamedImagesCollection class is a strongly typed collection of NamedImage
    /// objects.
    /// </summary>
    public class NamedImagesCollection : ChartNamedElementCollection<NamedImage>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        internal NamedImagesCollection() : base(null)
        {
        }

        #endregion Constructor
    }

    /// <summary>
    /// The NamedImage class stores a single Image with its unique name.
    /// </summary>
    [
        SRDescription("DescriptionAttributeNamedImage_NamedImage"),
    ]
    public class NamedImage : ChartNamedElement
    {
        #region Fields

        private string _name = string.Empty;
        private SKImage _image = null;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// NamedImage constructor.
        /// </summary>
        public NamedImage()
        {
        }

        /// <summary>
        /// NamedImage constructor.
        /// </summary>
        /// <param name="name">Image name.</param>
        /// <param name="image">Image object.</param>
        public NamedImage(string name, SKImage image)
        {
            _name = name;
            _image = image;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the image name.
        /// </summary>
        [
        SRDescription("DescriptionAttributeNamedImage_Name"),
        ]
        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the image object.
        /// </summary>
        [
        SRDescription("DescriptionAttributeNamedImage_Image"),
        ]
        public SKImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}