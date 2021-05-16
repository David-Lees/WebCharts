// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Collection of annotation objects.
//


using SkiaSharp;
using System;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Annotations
{
    /// <summary>
    /// <b>AnnotationCollection</b> is a collection that stores chart annotation objects.
    /// <seealso cref="Charting.Chart.Annotations"/>
    /// </summary>
    /// <remarks>
    /// All chart annotations are stored in this collection.  It is exposed as 
    /// a <see cref="Charting.Chart.Annotations"/> property of the chart. It is also used to 
    /// store annotations inside the <see cref="AnnotationGroup"/> class.
    /// <para>
    /// This class includes methods for adding, inserting, iterating and removing annotations.
    /// </para>
    /// </remarks>
    [
        SRDescription("DescriptionAttributeAnnotations3"),
    ]
    public class AnnotationCollection : ChartNamedElementCollection<Annotation>
    {
        #region Fields

        /// <summary>
        /// Group this collection belongs too
        /// </summary>
        internal AnnotationGroup AnnotationGroup { get; set; }

        // Annotation object that was last clicked on
        internal Annotation lastClickedAnnotation = null;

        // Annotation object which is currently placed on the chart
        internal Annotation placingAnnotation = null;

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
		internal AnnotationCollection(IChartElement parent) : base(parent)
        {
        }

        #endregion

        #region Items Inserting and Removing Notification methods

        /// <summary>
        /// Initializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal override void Initialize(Annotation item)
        {
            if (item != null)
            {
                //If the collection belongs to annotation group we need to pass a ref to this group to all the child annotations
                if (AnnotationGroup != null)
                {
                    item.annotationGroup = AnnotationGroup;
                }

                item.ResetCurrentRelativePosition();
            }
            base.Initialize(item);
        }

        /// <summary>
        /// Deinitializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal override void Deinitialize(Annotation item)
        {
            if (item != null)
            {
                item.annotationGroup = null;
                item.ResetCurrentRelativePosition();
            }
            base.Deinitialize(item);
        }


        /// <summary>
		/// Finds an annotation in the collection by name.
		/// </summary>
		/// <param name="name">
		/// Name of the annotation to find.
		/// </param>
		/// <returns>
		/// <see cref="Annotation"/> object, or null (or nothing) if it does not exist.
		/// </returns>
		public override Annotation FindByName(string name)
        {
            foreach (Annotation annotation in this)
            {
                // Compare annotation name 
                if (annotation.Name == name)
                {
                    return annotation;
                }

                // Check if annotation is a group
                if (annotation is AnnotationGroup annotationGroup)
                {
                    Annotation result = annotationGroup.Annotations.FindByName(name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Painting

        /// <summary>
        /// Paints all annotation objects in the collection.
        /// </summary>
        /// <param name="chartGraph">Chart graphics used for painting.</param>
        /// <param name="drawAnnotationOnly">Indicates that only annotation objects are redrawn.</param>
        internal void Paint(ChartGraphics chartGraph, bool drawAnnotationOnly)
        {
            ChartPicture chartPicture = Chart.chartPicture;

            // Restore previous background using double buffered bitmap
            if (!chartPicture.isSelectionMode &&
                Count > 0 /*&&
				!this.Chart.chartPicture.isPrinting*/)
            {
                chartPicture.backgroundRestored = true;
                if (chartPicture.nonTopLevelChartBuffer == null || !drawAnnotationOnly)
                {
                    // Dispose previous bitmap
                    if (chartPicture.nonTopLevelChartBuffer != null)
                    {
                        chartPicture.nonTopLevelChartBuffer.Dispose();
                        chartPicture.nonTopLevelChartBuffer = null;
                    }
                }
                else if (chartPicture.nonTopLevelChartBuffer != null)
                {
                    // Restore previous background
                }

                // Draw all annotation objects
                foreach (Annotation annotation in this)
                {
                    // Reset calculated relative position
                    annotation.ResetCurrentRelativePosition();

                    if (annotation.IsVisible())
                    {
                        bool resetClip = false;

                        // Check if anchor point assosiated with plot area is inside the scaleView
                        if (annotation.IsAnchorVisible())
                        {
                            // Set annotation object clipping
                            if (annotation.ClipToChartArea.Length > 0 &&
                                annotation.ClipToChartArea != Constants.NotSetValue &&
                                Chart != null)
                            {
                                int areaIndex = Chart.ChartAreas.IndexOf(annotation.ClipToChartArea);
                                if (areaIndex >= 0)
                                {
                                    // Get chart area object
                                    ChartArea chartArea = Chart.ChartAreas[areaIndex];
                                    chartGraph.SetClip(chartArea.PlotAreaPosition.ToSKRect());
                                    resetClip = true;
                                }
                            }

                            // Draw annotation object
                            annotation.Paint(Chart, chartGraph);

                            // Reset clipping region
                            if (resetClip)
                            {
                                chartGraph.ResetClip();
                            }
                        }
                    }
                }
            }

            #endregion

        }
    }
}
