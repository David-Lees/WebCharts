using SkiaSharp;
using System;
using System.Linq;

namespace WebCharts.Services
{
    public static class SkiaSharpExtensions
    {
        public static SKRect GetBounds(this SKPath path)
        {
            path.GetBounds(out var rect);
            return rect;
        }

        public static void AddPie(this SKPath p, float x, float y, float w, float h, float startAngle, float sweepAngle)
        {
            var cx = x + w / 2;
            var cy = y + h / 2;
            p.MoveTo(cx, cy);
            p.ArcTo(new SKRect(x, y, x + w, y + h), startAngle, sweepAngle, false);
            p.Close();
        }

        public static void AddLines(this SKPath path, SKPoint[] points)
        {
            path.MoveTo(points[0]);
            foreach (var p in points.Skip(1))
            {
                path.LineTo(p);
            }
        }

        public static void AddLine(this SKPath path, SKPoint p1, SKPoint p2)
        {
            path.MoveTo(p1);
            path.LineTo(p2);
        }

        public static void AddLine(this SKPath path, float x1, float y1, float x2, float y2)
        {
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
        }

        public static SKPoint GetLastPoint(this SKPath path)
        {
            return path.Points.Last();
        }

        public static void Reverse(this SKPath path)
        {
            var clone = new SKPath(path);
            path = new();
            path.AddPathReverse(clone);
        }

        public static SKMatrix CreateRotationDegrees(float angle, SKPoint p)
        {
            return SKMatrix.CreateRotationDegrees(angle, p.X, p.Y);
        }

        public static SKPoint Round (this SKPoint point)
        {
            return new SKPoint(MathF.Round(point.X), MathF.Round(point.Y));
        }

        public static SKRect Round (this SKRect rect)
        {
            return new SKRect(MathF.Round(rect.Left), MathF.Round(rect.Top), MathF.Round(rect.Right), MathF.Round(rect.Bottom));
        }

        public static SKSize Round(this SKSize size)
        {
            return new SKSize(MathF.Round(size.Width), MathF.Round(size.Height));
        }

        public static void TransformPoints(this SKMatrix matrix, SKPoint[] points)
        {
            matrix.MapPoints(points);
        }

        public static void Translate(this SKMatrix matrix, float x, float y)
        {
            matrix.TransX = x;
            matrix.TransY = y;
        }

        public static void AddBezier(this SKPath path, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            path.MoveTo(x1, y1);
            path.CubicTo(x2, y2, x3, y3, x4, y4);
        }

        /// <summary>
        /// Creates a Spline path through a given set of points.
        /// </summary>
        /// <param name="Points">Points between which the spline will be created.</param>
        /// <returns>Spline path.</returns>
        public static SKPath CreateSpline(params SKPoint[] Points)
        {
            return CreateSpline(null, Points);
        }

        /// <summary>
        /// Creates a Spline path through a given set of points.
        /// </summary>
        /// <param name="AppendTo">Spline should be appended to this path. If null, a new path will be created.</param>
        /// <param name="Points">Points between which the spline will be created.</param>
        /// <returns>Spline path.</returns>
        public static SKPath CreateSpline(SKPath AppendTo, params SKPoint[] Points)
        {
            return CreateSpline(AppendTo, out _, out _, out _, out _, Points);
        }

        private static SKPath CreateSpline(SKPath AppendTo,
            out float[] Ax, out float[] Ay, out float[] Bx, out float[] By,
            params SKPoint[] Points)
        {
            int i, c = Points.Length;
            if (c == 0)
                throw new ArgumentException("No points provided.", nameof(Points));

            if (AppendTo is null)
            {
                AppendTo = new SKPath();
                AppendTo.MoveTo(Points[0]);
            }
            else
            {
                SKPoint P = AppendTo.LastPoint;

                if (P.X != Points[0].X || P.Y != Points[0].Y)
                    AppendTo.LineTo(Points[0]);
            }

            if (c == 1)
            {
                Ax = Ay = Bx = By = null;
                return AppendTo;
            }

            if (c == 2)
            {
                AppendTo.LineTo(Points[1]);
                Ax = Ay = Bx = By = null;
                return AppendTo;
            }

            float[] V = new float[c];

            for (i = 0; i < c; i++)
                V[i] = Points[i].X;

            GetCubicBezierCoefficients(V, out Ax, out Bx);

            for (i = 0; i < c; i++)
                V[i] = Points[i].Y;

            GetCubicBezierCoefficients(V, out Ay, out By);

            for (i = 0; i < c - 1; i++)
                AppendTo.CubicTo(Ax[i], Ay[i], Bx[i], By[i], Points[i + 1].X, Points[i + 1].Y);

            return AppendTo;
        }

        /// <summary>
		/// Gets a set of coefficients for cubic Bezier curves, forming a spline, one coordinate at a time.
		/// </summary>
		/// <param name="V">One set of coordinates.</param>
		/// <param name="A">Corresponding coefficients for first control points.</param>
		/// <param name="B">Corresponding coefficients for second control points.</param>
		public static void GetCubicBezierCoefficients(float[] V, out float[] A, out float[] B)
        {
            // Calculate Spline between points P[0], ..., P[N].
            // Divide into segments, B[0], ...., B[N-1] of cubic Bezier curves:
            //
            // B[i](t) = (1-t)³P[i] + 3t(1-t)²A[i] + 3t²(1-t)B[i] + t³P[i+1]
            //
            // B'[i](t) = (-3+6t-3t²)P[i]+(3-12t+9t²)A[i]+(6t-9t²)B[i]+3t²P[i+1]
            // B"[i](t) = (6-6t)P[i]+(-12+18t)A[i]+(6-18t)B[i]+6tP[i+1]
            //
            // Choose control points A[i] and B[i] such that:
            //
            // B'[i](1) = B'[i+1](0) => A[i+1]+B[i]=2P[i+1], i<N		(eq 1)
            // B"[i](1) = B"[i+1](0) => A[i]-2B[i]+2A[i+1]-B[i+1]=0		(eq 2)
            //
            // Also add the boundary conditions:
            //
            // B"[0](0)=0 => 2A[0]-B[0]=P[0]			(eq 3)
            // B"[N-1](1)=0 => -A[N-1]+2B[N-1]=P[N]		(eq 4)
            //
            // Method solves this linear equation for one coordinate of A[i] and B[i] at a time.
            //
            // First, the linear equation, is reduced downwards. Only coefficients close to
            // the diagonal, and in the right-most column need to be processed. Furthermore,
            // we don't have to store values we know are zero or one. Since number of operations
            // depend linearly on number of vertices, algorithm is O(N).
            //
            // Matrix of system of linear equations has the following form (zeroes excluded):
            //
            // | A0 B0 A1 B1 A2 B2 A3 B3 ... AN BN |  EQ |
            // |-----------------------------------|-----|
            // |  2 -1                             |  P0 | (eq 3)
            // |  1 -2  2 -1                       |   0 | (eq 2)
            // |     1  1                          | 2P1 | (eq 1)
            // |        1 -2  2 -1                 |   0 | (eq 2)
            // |           1  1                    | 2P2 | (eq 1)
            // |              1 -2  2 -1           |   0 | (eq 2)
            // |                 1  1              | 2P3 | (eq 1)
            // |                    ...            |   . |
            // |                       ...         |   . |
            // |                          ...      |   . |
            // |                             -1  2 |  PN | (eq 4)

            int N = V.Length - 1;
            int N2 = N << 1;
            int i = 0;
            int j = 0;
            float r11, r12, r15;               // r13 & r14 always 0.
            float r22, r23, r25;               // r21 & r24 always 0 for all except last equation, where r21 is -1.
            float /*r31,*/ r32, r33, r34, r35;
            float[,] Rows = new float[N2, 3];
            float a;

            A = new float[N];
            B = new float[N];

            r11 = 2;        // eq 3
            r12 = -1;
            r15 = V[j++];

            r22 = 1;        // eq 1
            r23 = 1;
            r25 = 2 * V[j++];

            // r31 = 1;     // eq 2
            r32 = -2;
            r33 = 2;
            r34 = -1;
            r35 = 0;

            while (true)
            {
                a = 1 / r11;
                // r11 = 1
                r12 *= a;
                r15 *= a;

                // r21 is always 0. No need to eliminate column.
                // r22 is always 1. No need to scale row.

                // r31 is always 1 at this point.
                // r31 -= r11
                r32 -= r12;
                r35 -= r15;

                if (r32 != 0)
                {
                    r33 -= r32 * r23;
                    r35 -= r32 * r25;
                    // r32 = 0
                }

                // r33 is always 0.

                // r11 always 1.
                Rows[i, 0] = r12;
                Rows[i, 1] = 0;
                Rows[i, 2] = r15;
                i++;

                // r21, r24 always 0.
                Rows[i, 0] = r22;
                Rows[i, 1] = r23;
                Rows[i, 2] = r25;
                i++;

                if (i >= N2 - 2)
                    break;

                r11 = r33;
                r12 = r34;
                r15 = r35;

                r22 = 1;        // eq 1
                r23 = 1;
                r25 = 2 * V[j++];

                // r31 = 1;        // eq 2
                r32 = -2;
                r33 = 2;
                r34 = -1;
                r35 = 0;
            }

            r11 = r33;
            r12 = r34;
            r15 = r35;

            //r21 = -1		// eq 4
            r22 = 2;
            r23 = 0;
            r25 = V[j++];

            a = 1 / r11;
            //r11 = 1
            r12 *= a;
            r15 *= a;

            //r21 += r11
            r22 += r12;
            r25 += r15;

            r25 /= r22;
            r22 = 1;

            // r11 always 1.
            Rows[i, 0] = r12;
            Rows[i, 1] = 0;
            Rows[i, 2] = r15;
            i++;

            // r21 and r24 always 0.
            Rows[i, 0] = r22;
            Rows[i, 1] = r23;
            Rows[i, 2] = r25;
            i++;

            // Then eliminate back up:

            j--;
            while (i > 0)
            {
                i--;
                if (i < N2 - 1)
                {
                    a = Rows[i, 1];
                    if (a != 0)
                    {
                        Rows[i, 1] = 0;
                        Rows[i, 2] -= a * Rows[i + 1, 2];
                    }
                }

                B[--j] = Rows[i, 2];

                i--;
                a = Rows[i, 0];
                if (a != 0)
                {
                    Rows[i, 0] = 0;
                    Rows[i, 2] -= a * Rows[i + 1, 2];
                }

                A[j] = Rows[i, 2];
            }
        }

    }

    public static class Color
    {
        public static SKColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new SKColor(r, g, b, a);
        }

        public static SKColor FromArgb(int a, int r, int g, int b)
        {
            return new SKColor((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static SKColor FromArgb(byte a, SKColor c)
        {
            return new SKColor(c.Red, c.Green, c.Blue, a);
        }

        public static SKColor FromArgb(int a, SKColor c)
        {
            return new SKColor(c.Red, c.Green, c.Blue, (byte)a);
        }

        internal static SKColor FromArgb(int red, int green, int blue)
        {
            return new SKColor((byte)red, (byte)green, (byte)blue);
        }
    }

    static class PathExtensions
    {
        public static SKPath CloneWithTransform(this SKPath pathIn, Func<SKPoint, SKPoint> transform)
        {
            SKPath pathOut = new();

            using (SKPath.RawIterator iterator = pathIn.CreateRawIterator())
            {
                SKPoint[] points = new SKPoint[4];
                SKPathVerb pathVerb = SKPathVerb.Move;
                SKPoint firstPoint = new();
                SKPoint lastPoint = new();

                while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done)
                {
                    switch (pathVerb)
                    {
                        case SKPathVerb.Move:
                            pathOut.MoveTo(transform(points[0]));
                            firstPoint = lastPoint = points[0];
                            break;

                        case SKPathVerb.Line:
                            SKPoint[] linePoints = Interpolate(points[0], points[1]);

                            foreach (SKPoint pt in linePoints)
                            {
                                pathOut.LineTo(transform(pt));
                            }

                            lastPoint = points[1];
                            break;

                        case SKPathVerb.Cubic:
                            SKPoint[] cubicPoints = FlattenCubic(points[0], points[1], points[2], points[3]);

                            foreach (SKPoint pt in cubicPoints)
                            {
                                pathOut.LineTo(transform(pt));
                            }

                            lastPoint = points[3];
                            break;

                        case SKPathVerb.Quad:
                            SKPoint[] quadPoints = FlattenQuadratic(points[0], points[1], points[2]);

                            foreach (SKPoint pt in quadPoints)
                            {
                                pathOut.LineTo(transform(pt));
                            }

                            lastPoint = points[2];
                            break;

                        case SKPathVerb.Conic:
                            SKPoint[] conicPoints = FlattenConic(points[0], points[1], points[2], iterator.ConicWeight());

                            foreach (SKPoint pt in conicPoints)
                            {
                                pathOut.LineTo(transform(pt));
                            }

                            lastPoint = points[2];
                            break;

                        case SKPathVerb.Close:
                            SKPoint[] closePoints = Interpolate(lastPoint, firstPoint);

                            foreach (SKPoint pt in closePoints)
                            {
                                pathOut.LineTo(transform(pt));
                            }

                            firstPoint = lastPoint = new SKPoint(0, 0);
                            pathOut.Close();
                            break;
                    }
                }
            }
            return pathOut;
        }

        static SKPoint[] Interpolate(SKPoint pt0, SKPoint pt1)
        {
            int count = (int)Math.Max(1, Length(pt0, pt1));
            SKPoint[] points = new SKPoint[count];

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / count;
                float x = (1 - t) * pt0.X + t * pt1.X;
                float y = (1 - t) * pt0.Y + t * pt1.Y;
                points[i] = new SKPoint(x, y);
            }

            return points;
        }

        static SKPoint[] FlattenCubic(SKPoint pt0, SKPoint pt1, SKPoint pt2, SKPoint pt3)
        {
            int count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2) + Length(pt2, pt3));
            SKPoint[] points = new SKPoint[count];

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / count;
                float x = (1 - t) * (1 - t) * (1 - t) * pt0.X +
                          3 * t * (1 - t) * (1 - t) * pt1.X +
                          3 * t * t * (1 - t) * pt2.X +
                          t * t * t * pt3.X;
                float y = (1 - t) * (1 - t) * (1 - t) * pt0.Y +
                          3 * t * (1 - t) * (1 - t) * pt1.Y +
                          3 * t * t * (1 - t) * pt2.Y +
                          t * t * t * pt3.Y;
                points[i] = new SKPoint(x, y);
            }

            return points;
        }

        static SKPoint[] FlattenQuadratic(SKPoint pt0, SKPoint pt1, SKPoint pt2)
        {
            int count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2));
            SKPoint[] points = new SKPoint[count];

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / count;
                float x = (1 - t) * (1 - t) * pt0.X + 2 * t * (1 - t) * pt1.X + t * t * pt2.X;
                float y = (1 - t) * (1 - t) * pt0.Y + 2 * t * (1 - t) * pt1.Y + t * t * pt2.Y;
                points[i] = new SKPoint(x, y);
            }

            return points;
        }

        static SKPoint[] FlattenConic(SKPoint pt0, SKPoint pt1, SKPoint pt2, float weight)
        {
            int count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2));
            SKPoint[] points = new SKPoint[count];

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / count;
                float denominator = (1 - t) * (1 - t) + 2 * weight * t * (1 - t) + t * t;
                float x = (1 - t) * (1 - t) * pt0.X + 2 * weight * t * (1 - t) * pt1.X + t * t * pt2.X;
                float y = (1 - t) * (1 - t) * pt0.Y + 2 * weight * t * (1 - t) * pt1.Y + t * t * pt2.Y;
                x /= denominator;
                y /= denominator;
                points[i] = new SKPoint(x, y);
            }

            return points;
        }

        static double Length(SKPoint pt0, SKPoint pt1)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt0.X, 2) + Math.Pow(pt1.Y - pt0.Y, 2));
        }
    }
}
