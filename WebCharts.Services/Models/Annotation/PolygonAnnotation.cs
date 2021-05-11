// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Polyline and polygon annotation classes.
//


using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using WebCharts.Services.Enums;
using WebCharts.Services.Models.Common;
using WebCharts.Services.Models.General;

namespace WebCharts.Services.Models.Annotation
{
    /// <summary>
    /// <b>PolylineAnnotation</b> is a class that represents a polyline annotation.
    /// </summary>
    [
        SRDescription("DescriptionAttributePolylineAnnotation_PolylineAnnotation"),
    ]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
    public class PolylineAnnotation : Annotation
    {
        #region Fields

        // Path with polygon points.
        private SKPath _defaultSKPath = new SKPath();
        private SKPath _SKPath;

        // Indicates that path was changed
        internal bool pathChanged = false;

        // Collection of path points exposed at design-time
        private AnnotationPathPointCollection _pathPoints;

        // Indicate that filled polygon must be drawn
        internal bool isPolygon = false;

        // Indicates that annotation will be placed using free-draw style
        internal bool isFreeDrawPlacement = false;

        // Line start/end caps
        private LineAnchorCapStyle _startCap = LineAnchorCapStyle.None;
        private LineAnchorCapStyle _endCap = LineAnchorCapStyle.None;

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public PolylineAnnotation()
            : base()
        {
            _pathPoints = new AnnotationPathPointCollection(this);

            _SKPath = _defaultSKPath;
        }

        #endregion

        #region Properties

        #region Polyline Visual Attributes

        /// <summary>
        /// Gets or sets a cap style used at the start of an annotation line.
        /// <seealso cref="EndCap"/>
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value used for a cap style used at the start of an annotation line.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeStartCap3"),
        ]
        virtual public LineAnchorCapStyle StartCap
        {
            get
            {
                return _startCap;
            }
            set
            {
                _startCap = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a cap style used at the end of an annotation line.
        /// <seealso cref="StartCap"/>
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value used for a cap style used at the end of an annotation line.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeStartCap3"),
        ]
        virtual public LineAnchorCapStyle EndCap
        {
            get
            {
                return _endCap;
            }
            set
            {
                _endCap = value;
                Invalidate();
            }
        }

        #endregion

        #region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
        /// <value>
        /// A <see cref="ContentAlignment"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public ContentAlignment Alignment
        {
            get
            {
                return base.Alignment;
            }
            set
            {
                base.Alignment = value;
            }
        }

        /// <summary>
        /// Gets or sets an annotation's text style.
        /// <seealso cref="Font"/>
        /// 	<seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="TextStyle"/> value used to draw an annotation's text.
        /// </value>
        public override TextStyle TextStyle
        {
            get
            {
                return base.TextStyle;
            }
            set
            {
                base.TextStyle = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// <seealso cref="Font"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeForeColor"),
        ]
        override public SKColor ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// <seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="Font"/> object.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public SKFont Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public SKColor BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return base.BackHatchStyle;
            }
            set
            {
                base.BackHatchStyle = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public GradientStyle BackGradientStyle
        {
            get
            {
                return base.BackGradientStyle;
            }
            set
            {
                base.BackGradientStyle = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public SKColor BackSecondaryColor
        {
            get
            {
                return base.BackSecondaryColor;
            }
            set
            {
                base.BackSecondaryColor = value;
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type 
        /// (e.g. Line, Rectangle, Ellipse). 
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAnnotationType"),
        ]
        public override string AnnotationType
        {
            get
            {
                return "Polyline";
            }
        }

        /// <summary>
        /// Gets or sets an annotation selection points style.
        /// </summary>
        /// <value>
        /// A <see cref="SelectionPointsStyle"/> value that represents the annotation 
        /// selection style.
        /// </value>
        /// <remarks>
        /// This property is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeSelectionPointsStyle"),
        ]
        override internal SelectionPointsStyle SelectionPointsStyle
        {
            get
            {
                return SelectionPointsStyle.Rectangle;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether an annotation should be placed using the free-draw mode.
        /// </summary>
        /// <value>
        /// <b>True</b> if an annotation should be placed using free-draw mode, 
        /// <b>false</b> otherwise.  Defaults to <b>false</b>.
        /// </value>
        /// <remarks>
        /// Two different placement modes are supported when the Annotation.BeginPlacement 
        /// method is called. Set this property to <b>true</b> to switch from the default 
        /// mode to free-draw mode, which allows the caller to free-draw while moving the mouse cursor.
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeFreeDrawPlacement"),
        ]
        virtual public bool IsFreeDrawPlacement
        {
            get
            {
                return isFreeDrawPlacement;
            }
            set
            {
                isFreeDrawPlacement = value;
            }
        }

        /// <summary>
        /// Gets or sets the path points of a polyline at run-time.
        /// </summary>
        /// <value>
        /// A <see cref="SKPath"/> object with the polyline shape.
        /// </value>
        /// <remarks>
        /// A polyline must use coordinates relative to an annotation object, where (0,0) is 
        /// the top-left coordinates and (100,100) is the bottom-right coordinates of the annotation.  
        /// <para>
        /// This property is not accessible at design time (at design-time, use the 
        /// <see cref="SKPathPoints"/> property instead).
        /// </para>
        /// </remarks>
        [
        SRCategory("CategoryAttributePosition"),
        SRDescription("DescriptionAttributePath"),
        ]
        virtual public SKPath SKPath
        {
            get
            {
                return _SKPath;
            }
            set
            {
                _SKPath = value;
                this.pathChanged = true;
            }
        }



        #endregion

        #endregion

        #region Methods

        #region Painting

        /// <summary>
        /// Paints an annotation object on the specified graphics.
        /// </summary>
        /// <param name="graphics">
        /// A <see cref="ChartGraphics"/> object, used to paint an annotation object.
        /// </param>
        /// <param name="chart">
        /// Reference to the <see cref="Chart"/> owner control.
        /// </param>
        override internal void Paint(Chart chart, ChartGraphics graphics)
        {
            // Check for empty path
            if (_SKPath.PointCount == 0)
            {
                return;
            }

            // Get annotation position in relative coordinates
            SKPoint firstPoint = SKPoint.Empty;
            SKPoint anchorPoint = SKPoint.Empty;
            SKSize size = SKSize.Empty;
            GetRelativePosition(out firstPoint, out size, out anchorPoint);
            SKPoint secondPoint = new(firstPoint.X + size.Width, firstPoint.Y + size.Height);

            // Create selection rectangle
            SKRect selectionRect = new(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

            // Get position
            SKRect rectanglePosition = new(selectionRect.Left, selectionRect.Top, selectionRect.Right, selectionRect.Bottom);
            if (rectanglePosition.Width < 0 || rectanglePosition.Height < 0)
            {
                rectanglePosition = rectanglePosition.Standardized;
            }

            // Check if position is valid
            if (float.IsNaN(rectanglePosition.Left) ||
                float.IsNaN(rectanglePosition.Top) ||
                float.IsNaN(rectanglePosition.Right) ||
                float.IsNaN(rectanglePosition.Bottom))
            {
                return;
            }

            // Get annotation absolute position
            SKRect rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

            // Calculate scaling
            float groupScaleX = rectanglePositionAbs.Width / 100.0f;
            float groupScaleY = rectanglePositionAbs.Height / 100.0f;

            // Convert path to pixel coordinates
            SKPoint[] pathPoints = _SKPath.Points;
            var pathAbs = new SKPath();
            //byte[] pathTypes = _SKPath.PathTypes;
            for (int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
            {
                pathPoints[pointIndex].X = rectanglePositionAbs.Left + pathPoints[pointIndex].X * groupScaleX;
                pathPoints[pointIndex].Y = rectanglePositionAbs.Top + pathPoints[pointIndex].Y * groupScaleY;
                if (pointIndex == 0)
                {
                    pathAbs.MoveTo(pathPoints[pointIndex]);
                }
                else
                {
                    pathAbs.LineTo(pathPoints[pointIndex]);
                }
            }

            // Painting mode
            if (this.Common.ProcessModePaint)
            {
                if (this.isPolygon)
                {
                    // Draw polygon
                    pathAbs.Close();
                    graphics.DrawPathAbs(
                        pathAbs,
                        this.BackColor,
                        this.BackHatchStyle,
                        String.Empty,
                        ChartImageWrapMode.Scaled,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        this.BackGradientStyle,
                        this.BackSecondaryColor,
                        this.LineColor,
                        this.LineWidth,
                        this.LineDashStyle,
                        PenAlignment.Center,
                        this.ShadowOffset,
                        this.ShadowColor);
                }
                else
                {
                    // Draw polyline
                    graphics.DrawPathAbs(
                        pathAbs,
                        SKColors.Transparent,
                        ChartHatchStyle.None,
                        String.Empty,
                        ChartImageWrapMode.Scaled,
                        SKColor.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        SKColor.Empty,
                        this.LineColor,
                        this.LineWidth,
                        this.LineDashStyle,
                        PenAlignment.Center,
                        this.ShadowOffset,
                        this.ShadowColor);
                }
            }

            if (this.Common.ProcessModeRegions)
            {
                // Create line graphics path
                SKPath selectionPath = null;
                SKPath newPath = null;

                if (this.isPolygon)
                {
                    selectionPath = pathAbs;
                }
                else
                {
                    newPath = new SKPath();
                    selectionPath = newPath;
                    selectionPath.AddPath(pathAbs);
                    using var pen = graphics.Pen.Clone();
                    // Increase pen size by 2 pixels
                    pen.StrokeWidth += 2;
                }

                // Add hot region
                this.Common.HotRegionsList.AddHotRegion(
                    graphics,
                    selectionPath,
                    false,
                    ReplaceKeywords(this.ToolTip),
                    String.Empty,
                    String.Empty,
                    String.Empty,
                    this,
                    ChartElementType.Annotation);

                //Clean up
                if (newPath != null)
                    newPath.Dispose();
            }

            // Paint selection handles
            PaintSelectionHandles(graphics, rectanglePosition, pathAbs);
        }

        #endregion // Painting

        #region Position Changing
        /// <summary>
        /// Changes annotation position, so it exactly matches the bounary of the 
        /// polyline path.
        /// </summary>
        private void ResizeToPathBoundary()
        {
            if (_SKPath.PointCount > 0)
            {
                // Get current annotation position in relative coordinates
                SKPoint firstPoint = SKPoint.Empty;
                SKPoint anchorPoint = SKPoint.Empty;
                SKSize size = SKSize.Empty;
                GetRelativePosition(out firstPoint, out size, out anchorPoint);

                // Get path boundary and convert it to relative coordinates
                _SKPath.GetBounds(out SKRect pathBoundary);
                var w = pathBoundary.Width;
                var h = pathBoundary.Height;
                pathBoundary.Left *= size.Width / 100f;
                pathBoundary.Top *= size.Height / 100f;
                pathBoundary.Left += firstPoint.X;
                pathBoundary.Top += firstPoint.Y;
                pathBoundary.Right = pathBoundary.Left + (w * size.Width / 100f);
                pathBoundary.Bottom = pathBoundary.Top + (h * size.Height / 100f);

                // Scale all current points
                var matrix = new SKMatrix
                {
                    ScaleX = size.Width / pathBoundary.Width,
                    ScaleY = size.Height / pathBoundary.Height,
                    TransX = -pathBoundary.Left,
                    TransY = -pathBoundary.Top
                };
                _SKPath.Transform(matrix);

                // Set new position for annotation
                this.SetPositionRelative(pathBoundary, anchorPoint);
            }
        }
        /// <summary>
        /// Adjust annotation location and\or size as a result of user action.
        /// </summary>
        /// <param name="movingDistance">Distance to resize/move the annotation.</param>
        /// <param name="resizeMode">Resizing mode.</param>
        /// <param name="pixelCoord">Distance is in pixels, otherwise relative.</param>
        /// <param name="userInput">Indicates if position changing was a result of the user input.</param>
        override internal void AdjustLocationSize(SKSize movingDistance, ResizingMode resizeMode, bool pixelCoord, bool userInput)
        {
            // Call base class when not resizing the path points
            if (resizeMode != ResizingMode.MovingPathPoints)
            {
                base.AdjustLocationSize(movingDistance, resizeMode, pixelCoord, userInput);
                return;
            }

            // Get annotation position in relative coordinates
            SKPoint firstPoint = SKPoint.Empty;
            SKPoint anchorPoint = SKPoint.Empty;
            SKSize size = SKSize.Empty;
            GetRelativePosition(out firstPoint, out size, out anchorPoint);

            // Remember path before moving operation
            if (userInput == true && startMovePathRel == null)
            {
                this.startMovePathRel = new SKPath(_SKPath);
                this.startMovePositionRel = new SKRect(firstPoint.X, firstPoint.Y, firstPoint.X + size.Width, firstPoint.Y + size.Height);
                this.startMoveAnchorLocationRel = new SKPoint(anchorPoint.X, anchorPoint.Y);
            }

            // Convert moving distance to coordinates relative to the anotation
            if (pixelCoord)
            {
                movingDistance = this.GetGraphics().GetRelativeSize(movingDistance);
            }
            movingDistance.Width /= startMovePositionRel.Width / 100.0f;
            movingDistance.Height /= startMovePositionRel.Height / 100.0f;

            // Get path points and adjust position of one of them
            if (_SKPath.PointCount > 0)
            {
                SKPath pathToMove = (userInput) ? startMovePathRel : _SKPath;
                SKPoint[] pathPoints = pathToMove.Points;

                for (int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
                {
                    // Adjust position
                    if (currentPathPointIndex == pointIndex ||
                        currentPathPointIndex < 0 ||
                        currentPathPointIndex >= pathPoints.Length)
                    {
                        pathPoints[pointIndex].X -= movingDistance.Width;
                        pathPoints[pointIndex].Y -= movingDistance.Height;
                    }
                }


                // Adjust annotation position to the boundary of the path
                if (userInput && this.AllowResizing)
                {
                    // Get path bounds in relative coordinates
                    _defaultSKPath.Dispose();
                    _defaultSKPath = new SKPath();
                    for (int i = 0; i< pathPoints.Length; i++)
                    {
                        if (i == 0)
                        {
                            _defaultSKPath.MoveTo(pathPoints[i]);
                        }
                        else
                        {
                            _defaultSKPath.LineTo(pathPoints[i]);
                        }
                    }

                    _SKPath = _defaultSKPath;

                    _SKPath.GetBounds(out SKRect pathBounds);
                    var w = pathBounds.Width;
                    var h = pathBounds.Height;
                    pathBounds.Left *= startMovePositionRel.Width / 100f;
                    pathBounds.Top *= startMovePositionRel.Height / 100f;
                    pathBounds.Left += startMovePositionRel.Left;
                    pathBounds.Top += startMovePositionRel.Top;
                    pathBounds.Right = pathBounds.Left + (w * startMovePositionRel.Width / 100f);
                    pathBounds.Bottom = pathBounds.Top + (h * startMovePositionRel.Height / 100f);

                    // Set new annotation position
                    this.SetPositionRelative(pathBounds, anchorPoint);

                    // Adjust path point position
                    for (int pointIndex = 0; pointIndex < pathPoints.Length; pointIndex++)
                    {

                        pathPoints[pointIndex].X = startMovePositionRel.Left + pathPoints[pointIndex].X * (startMovePositionRel.Width / 100f);
                        pathPoints[pointIndex].Y = startMovePositionRel.Top + pathPoints[pointIndex].Y * (startMovePositionRel.Height / 100f);

                        pathPoints[pointIndex].X = (pathPoints[pointIndex].X - pathBounds.Left) / (pathBounds.Width / 100f);
                        pathPoints[pointIndex].Y = (pathPoints[pointIndex].Y - pathBounds.Top) / (pathBounds.Height / 100f);
                    }
                }


                // Position changed
                this.positionChanged = true;

                // Recreate path with new points
                _defaultSKPath.Dispose();
                _defaultSKPath = new SKPath();
                for (int i = 0; i < pathPoints.Length; i++)
                {
                    if (i == 0)
                    {
                        _defaultSKPath.MoveTo(pathPoints[i]);
                    }
                    else
                    {
                        _defaultSKPath.LineTo(pathPoints[i]);
                    }
                }

                _SKPath = _defaultSKPath;
                this.pathChanged = true;

                // Invalidate annotation
                this.Invalidate();
            }
        }

        #endregion // Position Changing

        #region Placement Methods

        /// <summary>
        /// Ends user placement of an annotation.
        /// </summary>
        /// <remarks>
        /// Ends an annotation placement operation previously started by a 
        /// <see cref="Annotation.BeginPlacement"/> method call.
        /// <para>
        /// Calling this method is not required, since placement will automatically
        /// end when an end user enters all required points. However, it is useful when an annotation 
        /// placement operation needs to be aborted for some reason.
        /// </para>
        /// </remarks>
        override public void EndPlacement()
        {
            // Call base method
            base.EndPlacement();

            // Position was changed
            if (this.Chart != null)
            {
                this.Chart.OnAnnotationPositionChanged(this);
            }

            // Reset last placement position
            this.lastPlacementPosition = SKPoint.Empty;

            // Resize annotation to the boundary of the polygon
            ResizeToPathBoundary();

            // Position changed
            this.positionChanged = true;
        }

        #endregion // Placement Methods

        #endregion

        #region IDisposable override 
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_defaultSKPath != null)
                {
                    _defaultSKPath.Dispose();
                    _defaultSKPath = null;
                }
                if (_pathPoints != null)
                {
                    _pathPoints.Dispose();
                    _pathPoints = null;
                }

            }
            base.Dispose(disposing);
        }
        #endregion

    }

    /// <summary>
    /// <b>PolygonAnnotation</b> is a class that represents a polygon annotation.
    /// </summary>
    [
        SRDescription("DescriptionAttributePolygonAnnotation_PolygonAnnotation"),
    ]
    public class PolygonAnnotation : PolylineAnnotation
    {
        #region Construction and Initialization

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public PolygonAnnotation()
            : base()
        {
            this.isPolygon = true;
        }

        #endregion

        #region Properties

        #region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

        /// <summary>
        /// Not applicable to this annotation type.
        /// <seealso cref="EndCap"/>
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public LineAnchorCapStyle StartCap
        {
            get
            {
                return base.StartCap;
            }
            set
            {
                base.StartCap = value;
            }
        }

        /// <summary>
        /// Not applicable to this annotation type.
        /// <seealso cref="StartCap"/>
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        ]
        override public LineAnchorCapStyle EndCap
        {
            get
            {
                return base.EndCap;
            }
            set
            {
                base.EndCap = value;
            }
        }

        #endregion

        #region Applicable Annotation Appearance Attributes (set as Browsable)

        /// <summary>
        /// Gets or sets the background color of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the background of an annotation.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackColor"),
        ]
        override public SKColor BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background hatch style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackHatchStyle"),
        ]
        override public ChartHatchStyle BackHatchStyle
        {
            get
            {
                return base.BackHatchStyle;
            }
            set
            {
                base.BackHatchStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the background gradient style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackGradientStyle"),
        ]
        override public GradientStyle BackGradientStyle
        {
            get
            {
                return base.BackGradientStyle;
            }
            set
            {
                base.BackGradientStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the secondary background color of an annotation.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of an annotation background with 
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        ]
        override public SKColor BackSecondaryColor
        {
            get
            {
                return base.BackSecondaryColor;
            }
            set
            {
                base.BackSecondaryColor = value;
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type 
        /// (e.g. Line, Rectangle, Ellipse). 
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        /// </remarks>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAnnotationType"),
        ]
        public override string AnnotationType
        {
            get
            {
                return "Polygon";
            }
        }

        /// <summary>
        /// Gets or sets an annotation's selection points style.
        /// </summary>
        /// <value>
        /// A <see cref="SelectionPointsStyle"/> value that represents an annotation's 
        /// selection style.
        /// </value>
        /// <remarks>
        /// This property is for internal use and is hidden at design and run time.
        /// </remarks>
        [
        SRCategory("CategoryAttributeAppearance"),
        SRDescription("DescriptionAttributeSelectionPointsStyle"),
        ]
        override internal SelectionPointsStyle SelectionPointsStyle
        {
            get
            {
                return SelectionPointsStyle.Rectangle;
            }
        }

        #endregion

        #endregion
    }

    /// <summary><b>AnnotationPathPointCollection</b> is a collection of polyline 
    /// annotation path points, and is only available via the <b>SKPathPoints</b> 
    /// property at design-time.
    /// <seealso cref="PolylineAnnotation.SKPathPoints"/></summary>
    /// <remarks>
    /// This collection is used at design-time only, and uses serialization to expose the 
    /// shape of the polyline and polygon via their SKPathPoints collection property.
    /// At run-time, use Path property to set the path of a polyline or polygon
    /// </remarks>
    [
        SRDescription("DescriptionAttributeAnnotationPathPointCollection_AnnotationPathPointCollection"),
    ]
    public class AnnotationPathPointCollection : ChartElementCollection<AnnotationPathPoint>
    {
        #region Fields

        internal PolylineAnnotation annotation = null;
        private SKPath _SKPath = null;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public AnnotationPathPointCollection(PolylineAnnotation annotation)
            : base(annotation)
        {
            this.annotation = annotation;
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Forces the invalidation of the chart element
        /// </summary>
        public override void Invalidate()
        {
            if (this.annotation != null)
            {
                //Dispose previously instantiated graphics path
                if (this._SKPath != null)
                {
                    this._SKPath.Dispose();
                    this._SKPath = null;
                }

                // Recreate polyline annotation path
                if (this.Count > 0)
                {
                    SKPoint[] points = new SKPoint[this.Count];
                    byte[] types = new byte[this.Count];
                    for (int index = 0; index < this.Count; index++)
                    {
                        points[index] = new SKPoint(this[index].X, this[index].Y);
                    }
                    this._SKPath = new SKPath();
                    _SKPath.AddPath(points);
                }
                else
                {
                    this._SKPath = new SKPath();
                }

                // Invalidate annotation
                this.annotation.SKPath = this._SKPath;
                this.annotation.Invalidate();
            }
            base.Invalidate();
        }

        #endregion // Methods

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free up managed resources
                if (this._SKPath != null)
                {
                    this._SKPath.Dispose();
                    this._SKPath = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    /// The <b>AnnotationPathPoint</b> class represents a path point of a polyline or polygon, 
    /// and is stored in their <b>SKPathPoints</b> property, which is only available at design-time.
    /// </summary>
    /// <remarks>
    /// At run-time, use <b>Path</b> property to set the path of a polyline or polygon.
    /// </remarks>
    [
        SRDescription("DescriptionAttributeAnnotationPathPoint_AnnotationPathPoint"),
    ]
    public class AnnotationPathPoint : ChartElement
    {
        #region Fields

        // Point X value
        private float _x = 0f;

        // Point Y value
        private float _y = 0f;

        // Point type
        private byte _pointType = 1;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public AnnotationPathPoint()
        {
        }

        /// <summary>
        /// Constructor that takes X and Y parameters.
        /// </summary>
        /// <param name="x">Point's X value.</param>
        /// <param name="y">Point's Y value.</param>
        public AnnotationPathPoint(float x, float y)
        {
            this._x = x;
            this._y = y;
        }

        /// <summary>
        /// Constructor that takes X, Y and point type parameters.
        /// </summary>
        /// <param name="x">Point's X value.</param>
        /// <param name="y">Point's Y value.</param>
        /// <param name="type">Point type.</param>
        public AnnotationPathPoint(float x, float y, byte type)
        {
            this._x = x;
            this._y = y;
            this._pointType = type;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets an annotation path point's X coordinate.
        /// </summary>
        /// <value>
        /// A float value for the point's X coordinate.
        /// </value>
        [
        SRCategory("CategoryAttributePosition"),
        SRDescription("DescriptionAttributeAnnotationPathPoint_X"),
        ]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        public float X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        /// <summary>
        /// Gets or sets an annotation path point's Y coordinate.
        /// </summary>
        /// <value>
        /// A float value for the point's Y coordinate.
        /// </value>
        [
        SRCategory("CategoryAttributePosition"),
        SRDescription("DescriptionAttributeAnnotationPathPoint_Y"),
        ]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        public float Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        /// <summary>
        /// Gets or sets an annotation path point's type.
        /// </summary>
        /// <value>
        /// A byte value.
        /// </value>
        /// <remarks>
        /// See the <see cref="PathPointType"/> enumeration for more details.
        /// </remarks>
        [
        SRCategory("CategoryAttributePosition"),
        SRDescription("DescriptionAttributeAnnotationPathPoint_Name"),
        ]
        public byte PointType
        {
            get
            {
                return _pointType;
            }
            set
            {
                _pointType = value;
            }
        }

        /// <summary>
        /// Gets or sets an annotation path point's name.
        /// </summary>
        /// <para>
        /// This property is for internal use and is hidden at design and run time.
        /// </para>
        [
        SRCategory("CategoryAttributeMisc"),
        SRDescription("DescriptionAttributeAnnotationPathPoint_Name"),
        ]
        public string Name
        {
            get
            {
                return "PathPoint";
            }
        }

        #endregion // Properties

    }
}
