using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace SplitCurves.Lib
{
    /// <summary>
    /// Creates boundaries for test purposes.
    /// </summary>
    public static class BoundaryCreator
    {
        /// <summary>
        /// Creates squared curve at the origin.
        /// </summary>
        /// <param name="edgeLength"> Length of edge of the square.</param>
        /// <returns> Squared NurbsCurve.</returns>
        public static Curve SquareCurve(double edgeLength)
        {
            Point3d crossCorner1 = new Point3d(edgeLength / 2, edgeLength / 2, 0);
            Point3d crossCorner2 = new Point3d((edgeLength / 2) * (-1), (edgeLength / 2) * (-1), 0);

            Rectangle3d rectangle = new Rectangle3d(new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1)), crossCorner1, crossCorner2);
            NurbsCurve nurbsCurve = rectangle.ToNurbsCurve();
            return nurbsCurve.DuplicateCurve();
        }

        /// <summary>
        /// Creates rectangular curve.
        /// </summary>
        /// <param name="center"> Left-bottom corner of the rectangular curve.</param>
        /// <param name="width"> Width of rectangular curve.</param>
        /// <param name="length"> Length of rectangular curve.</param>
        /// <returns> Rectangular NurbsCurve.</returns>
        public static Curve RectangularCurve(Point3d center, double width, double length)
        {
            Rectangle3d rectangle = new Rectangle3d(new Plane(center, new Vector3d(0, 0, 1)), width, length);
            NurbsCurve nurbsCurve = rectangle.ToNurbsCurve();
            return nurbsCurve.DuplicateCurve();
        }

        /// <summary>
        /// Creates circled curve.
        /// </summary>
        /// <param name="center"> Center of the circled curve.</param>
        /// <param name="radius"> Radius of the circled curve.</param>
        /// <returns> Circle NurbsCurve.</returns>
        public static Curve CircleCurve(Point3d center, double radius)
        {
            Circle circle = new Circle(center, radius);
            NurbsCurve nurbsCurve = circle.ToNurbsCurve();
            return nurbsCurve.DuplicateCurve();
        }

        /// <summary>
        /// Creates interpolated nurbs curve from given points.
        /// </summary>
        /// <param name="points"> Control points.</param>
        /// <param name="degree"> Degree of the curve.</param>
        /// <returns></returns>
        public static Curve InterpolatedCurve(IEnumerable<Point3d> points, int degree)
        {
            return NurbsCurve.Create(true, degree, points);
        }

        /// <summary>
        /// Creates elliptic curve.
        /// </summary>
        /// <param name="center"> Center of the elliptic curve.</param>
        /// <param name="radius1"> First radius of the elliptic curve.</param>
        /// <param name="radius2"> Second radius of the elliptic curve.</param>
        /// <returns> Elliptic NurbsCurve.</returns>
        public static Curve EllipseCurve(Point3d center, double radius1, double radius2)
        {
            Ellipse ellipse = new Ellipse(new Plane(center, new Vector3d(0, 0, 1)), radius1, radius2);
            NurbsCurve nurbsCurve = ellipse.ToNurbsCurve();
            return nurbsCurve.DuplicateCurve();
        }

        /// <summary>
        /// Create polyline curve from points.
        /// </summary>
        /// <param name="cornerPoints"> Collection of corner points of polyline.</param>
        /// <returns> Curve that created from given points.</returns>
        public static Curve PolylineCurve(IEnumerable<Point3d> cornerPoints)
        {
            List<LineCurve> edges = cornerPoints.Zip(cornerPoints.Skip(1), (first, second) => new LineCurve(first, second)).ToList();
            edges.Add(new LineCurve(cornerPoints.Last(), cornerPoints.First()));
            return Curve.JoinCurves(edges).First();
        }
    }
}
