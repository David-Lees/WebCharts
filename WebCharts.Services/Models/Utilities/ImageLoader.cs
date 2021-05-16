// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	ImageLoader utility class loads specified image and 
//              caches it in the memory for the future use.
//


using SkiaSharp;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Utilities
{
    /// <summary>
    /// ImageLoader utility class loads and returns specified image 
    /// form the File, URI, Web Request or Chart Resources. 
    /// Loaded images are stored in the internal hashtable which 
    /// allows to improve performance if image need to be used 
    /// several times.
    /// </summary>
    internal class ImageLoader : IDisposable, IImageLoader
    {
        #region Fields

        // Image storage
        private Hashtable _imageData = null;
        private readonly IServiceProvider _serviceContainer;

        #endregion

        #region Constructors and Initialization

        public ImageLoader(IServiceProvider serviceProvider)
        {
            _serviceContainer = serviceProvider;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _imageData != null)
            {
                foreach (DictionaryEntry entry in _imageData)
                {
                    if (entry.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                _imageData = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads image from URL. Checks if image already loaded (cached).
        /// </summary>
        /// <param name="imageURL">Image name (FileName, URL, Resource).</param>
        /// <returns>Image object.</returns>
        public SKImage LoadImage(string imageURL)
        {
            return LoadImage(imageURL, true);
        }

        /// <summary>
        /// Loads image from URL. Checks if image already loaded (cached).
        /// </summary>
        /// <param name="imageURL">Image name (FileName, URL, Resource).</param>
        /// <param name="saveImage">True if loaded image should be saved in cache.</param>
        /// <returns>Image object</returns>
        public SKImage LoadImage(string imageURL, bool saveImage)
        {
            SKImage image = null;

            // Check if image is defined in the chart image collection
            if (_serviceContainer != null)
            {
                ChartService chart = (ChartService)_serviceContainer.GetService(typeof(ChartService));
                if (chart != null)
                {
                    foreach (NamedImage namedImage in chart.Images)
                    {
                        if (namedImage.Name == imageURL)
                        {
                            return namedImage.Image;
                        }
                    }
                }
            }

            // Create new hashtable
            if (_imageData == null)
            {
                _imageData = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }

            // First check if image with this name already loaded
            if (_imageData.Contains(imageURL))
            {
                image = (SKImage)_imageData[imageURL];
            }

            // Try to load image from resource
            if (image == null)
            {
                try
                {

                    // Check if resource class type was specified
                    int columnIndex = imageURL.IndexOf("::", StringComparison.Ordinal);
                    if (columnIndex > 0)
                    {
                        string resourceRootName = imageURL.Substring(0, columnIndex);
                        string resourceName = imageURL[(columnIndex + 2)..];
                        ResourceManager resourceManager = new(resourceRootName, Assembly.GetExecutingAssembly());
                        image = (SKImage)(resourceManager.GetObject(resourceName));
                    }
                    else if (Assembly.GetEntryAssembly() != null)
                    {
                        // Check if resource class type was specified
                        columnIndex = imageURL.IndexOf(':');
                        if (columnIndex > 0)
                        {
                            string resourceRootName = imageURL.Substring(0, columnIndex);
                            string resourceName = imageURL[(columnIndex + 1)..];
                            ResourceManager resourceManager = new(resourceRootName, Assembly.GetEntryAssembly());
                            image = (SKImage)resourceManager.GetObject(resourceName);
                        }
                        else
                        {
                            // Try to load resource from every type defined in entry assembly
                            Assembly entryAssembly = Assembly.GetEntryAssembly();
                            if (entryAssembly != null)
                            {
                                foreach (Type type in entryAssembly.GetTypes())
                                {
                                    ResourceManager resourceManager = new(type);
                                    try
                                    {
                                        image = (SKImage)(resourceManager.GetObject(imageURL));
                                    }
                                    catch (ArgumentNullException)
                                    {
                                        // Do nothing
                                    }
                                    catch (MissingManifestResourceException)
                                    {
                                        // Do nothing
                                    }

                                    // Check if image was loaded
                                    if (image != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (MissingManifestResourceException)
                {
                    // Do nothing
                }
            }


            // Try to load image using the Web Request
            if (image == null)
            {
                Uri imageUri = null;
                try
                {
                    // Try to create URI directly from image URL (will work in case of absolute URL)
                    imageUri = new Uri(imageURL);
                }
                catch (UriFormatException)
                {
                    // Do nothing
                }


                // Load image from file or web resource
                if (imageUri != null)
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(imageUri);
                        image = SKImage.FromEncodedData(request.GetResponse().GetResponseStream());
                    }
                    catch (ArgumentException)
                    {
                        // Do nothing
                    }
                    catch (NotSupportedException)
                    {
                        // Do nothing
                    }
                    catch (SecurityException)
                    {
                        // Do nothing
                    }
                }
            }

            // absolute uri(without Server.MapPath)in web is not allowed. Loading from replative uri Server[Page].MapPath is done above.
            // Try to load as file
            if (image == null)
            {
                image = LoadFromFile(imageURL);
            }

            // Error loading image
            if (image == null)
            {
                throw (new ArgumentException(SR.ExceptionImageLoaderIncorrectImageLocation(imageURL)));
            }

            // Save new image in cache
            if (saveImage)
            {
                _imageData[imageURL] = image;
            }

            return image;
        }

        /// <summary>
        /// Helper function which loads image from file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>Loaded image or null.</returns>
        private static SKImage LoadFromFile(string fileName)
        {
            // Try to load image from file
            try
            {
                return SKImage.FromEncodedData(fileName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the image size taking the image DPI into consideration.
        /// </summary>
        /// <param name="name">Image name (FileName, URL, Resource).</param>
        /// <param name="graphics">Graphics used to calculate the image size.</param>
        /// <param name="size">Calculated size.</param>
        /// <returns>false if it fails to calculate the size, otherwise true.</returns>
        internal bool GetAdjustedImageSize(string name, SKCanvas graphics, ref SKSize size)
        {
            SKImage image = LoadImage(name);

            if (image == null)
                return false;

            GetAdjustedImageSize(image, graphics, ref size);

            return true;
        }

        /// <summary>
        /// Returns the image size taking the image DPI into consideration.
        /// </summary>
        /// <param name="image">Image for whcih to calculate the size.</param>
        /// <param name="graphics">Graphics used to calculate the image size.</param>
        /// <param name="size">Calculated size.</param>
        internal static void GetAdjustedImageSize(SKImage image, SKCanvas graphics, ref SKSize size)
        {
            size.Width = image.Width;
            size.Height = image.Height;
        }

        #endregion
    }
}
