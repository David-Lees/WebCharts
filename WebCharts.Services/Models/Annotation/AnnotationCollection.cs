// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Collection of annotation objects.
//


using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Annotation
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

        // Start point of annotation moving or resizing
        private SKPoint _movingResizingStartPoint = SKPoint.Empty;

        // Current resizing mode
        private readonly ResizingMode _resizingMode = ResizingMode.None;

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
                TextAnnotation textAnnotation = item as TextAnnotation;
                if (textAnnotation != null && string.IsNullOrEmpty(textAnnotation.Text) && Chart != null && Chart.IsDesignMode())
                {
                    textAnnotation.Text = item.Name;
                }

                //If the collection belongs to annotation group we need to pass a ref to this group to all the child annotations
                if (this.AnnotationGroup != null)
                {
                    item.annotationGroup = this.AnnotationGroup;
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
                AnnotationGroup annotationGroup = annotation as AnnotationGroup;
                if (annotationGroup != null)
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
            ChartPicture chartPicture = this.Chart.chartPicture;

            // Restore previous background using double buffered bitmap
            if (!chartPicture.isSelectionMode &&
                this.Count > 0 /*&&
				!this.Chart.chartPicture.isPrinting*/)
            {
                chartPicture.backgroundRestored = true;
                SKRect chartPosition = new(0, 0, chartPicture.Width, chartPicture.Height);
                if (chartPicture.nonTopLevelChartBuffer == null || !drawAnnotationOnly)
                {
                    // Dispose previous bitmap
                    if (chartPicture.nonTopLevelChartBuffer != null)
                    {
                        chartPicture.nonTopLevelChartBuffer.Dispose();
                        chartPicture.nonTopLevelChartBuffer = null;
                    }

                    // Copy chart area plotting rectangle from the chart's dubble buffer image into area dubble buffer image
                    if (this.Chart.paintBufferBitmap != null &&
                        this.Chart.paintBufferBitmap.Size.Width >= chartPosition.Size.Width &&
                        this.Chart.paintBufferBitmap.Size.Height >= chartPosition.Size.Height)
                    {
                        chartPicture.nonTopLevelChartBuffer = this.Chart.paintBufferBitmap.Clone(
                            chartPosition, this.Chart.paintBufferBitmap.PixelFormat);
                    }
                }
                else if (drawAnnotationOnly && chartPicture.nonTopLevelChartBuffer != null)
                {
                    // Restore previous background
                    this.Chart.paintBufferBitmapGraphics.DrawImageUnscaled(
                        chartPicture.nonTopLevelChartBuffer,
                        chartPosition);
                }
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

                        // Start Svg Selection mode
                        string url = String.Empty;
                        chartGraph.StartHotRegion(
                            annotation.ReplaceKeywords(url),
                            annotation.ReplaceKeywords(annotation.ToolTip));

                        // Draw annotation object
                        annotation.Paint(Chart, chartGraph);


                        // End Svg Selection mode
                        chartGraph.EndHotRegion();

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

        #region Mouse Events Handlers

        /// <summary>
        /// Mouse was double clicked.
        /// </summary>
        internal void OnDoubleClick()
        {
            if (lastClickedAnnotation != null &&
                lastClickedAnnotation.AllowTextEditing)
            {
                TextAnnotation textAnnotation = lastClickedAnnotation as TextAnnotation;

                if (textAnnotation == null)
                {
                    AnnotationGroup group = lastClickedAnnotation as AnnotationGroup;

                    if (group != null)
                    {
                        // Try to edit text annotation in the group
                        foreach (Annotation annot in group.Annotations)
                        {
                            TextAnnotation groupAnnot = annot as TextAnnotation;
                            if (groupAnnot != null &&
                                groupAnnot.AllowTextEditing)
                            {
                                // Get annotation position in relative coordinates
                                SKPoint firstPoint = SKPoint.Empty;
                                SKPoint anchorPoint = SKPoint.Empty;
                                SKSize size = SKSize.Empty;
                                groupAnnot.GetRelativePosition(out firstPoint, out size, out anchorPoint);
                                SKRect textPosition = new SKRect(firstPoint.X, firstPoint.Y, firstPoint.X + size.Width, firstPoint.Y + size.Height);

                                // Check if last clicked coordinate is inside this text annotation
                                if (groupAnnot.GetGraphics() != null &&
                                    textPosition.Contains(groupAnnot.GetGraphics().GetRelativePoint(this._movingResizingStartPoint)))
                                {
                                    textAnnotation = groupAnnot;
                                    lastClickedAnnotation = textAnnotation;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if specified point is contained by any of the selection handles.
        /// </summary>
        /// <param name="point">Point which is tested in pixel coordinates.</param>
        /// <param name="resizingMode">Handle containing the point or None.</param>
        /// <returns>Annotation that contains the point or Null.</returns>
        internal Annotation HitTestSelectionHandles(SKPoint point, ref ResizingMode resizingMode)
        {
            Annotation annotation = null;

            if (Common != null &&
                Common.graph != null)
            {
                SKPoint pointRel = Common.graph.GetRelativePoint(point);
                foreach (Annotation annot in this)
                {
                    // Reset selcted path point
                    annot.currentPathPointIndex = -1;

                    // Check if annotation is selected
                    if (annot.IsSelected)
                    {
                        if (annot.selectionRects != null)
                        {
                            for (int index = 0; index < annot.selectionRects.Length; index++)
                            {
                                if (!annot.selectionRects[index].IsEmpty &&
                                    annot.selectionRects[index].Contains(pointRel))
                                {
                                    annotation = annot;
                                    if (index > (int)ResizingMode.AnchorHandle)
                                    {
                                        resizingMode = ResizingMode.MovingPathPoints;
                                        annot.currentPathPointIndex = index - 9;
                                    }
                                    else
                                    {
                                        resizingMode = (ResizingMode)index;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return annotation;
        }


        #endregion

        #region Event handlers
        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            // If all the chart areas are removed and then a new one is inserted - Annotations don't get bound to it by default
            if (e.OldElement == null)
                return;

            foreach (Annotation annotation in this)
            {
                if (annotation.ClipToChartArea == e.OldName)
                    annotation.ClipToChartArea = e.NewName;

                AnnotationGroup group = annotation as AnnotationGroup;
                if (group != null)
                {
                    group.Annotations.ChartAreaNameReferenceChanged(sender, e);
                }
            }
        }
        #endregion

    }
}
