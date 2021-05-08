using Rhino.Geometry;
using Xunit;

namespace SplitCurves.Testing
{
    public static class CurveCollection
    {
        public static TheoryData<Curve> Boundaries = new TheoryData<Curve>
        {
            RectangularBoundary,
            PolylineBoundary,
            InterpolatedBoundary,
            CircleBoundary
        };

        public static Curve RectangularBoundary => new Rectangle3d(Plane.WorldXY, 100, 50).ToNurbsCurve();

        public static Curve PolylineBoundary => new Polyline(new[]
        {
            new Point3d(0,0,0),
            new Point3d(20, 0, 0),
            new Point3d(20, 25, 0),
            new Point3d(0, 25, 0),
            new Point3d(0,0,0)
        }).ToNurbsCurve();

        public static Curve InterpolatedBoundary => Curve.CreateInterpolatedCurve(new[]
        {
            new Point3d(0, 0, 0),
            new Point3d(50, 0, 0),
            new Point3d(70, 50, 0),
            new Point3d(0, 50, 0),
            new Point3d(0, 0, 0)
        }, 3);

        public static Curve CircleBoundary => new Circle(Plane.WorldXY, Point3d.Origin, 30).ToNurbsCurve();

        public static Curve OpenBoundary => new Polyline(new[]
        {
            new Point3d(0,0,0),
            new Point3d(20, 0, 0),
            new Point3d(20, 25, 0),
            new Point3d(0, 25, 0)
        }).ToNurbsCurve();

        public static Curve NonPlanarBoundary => new Polyline(new[]
        {
            new Point3d(0,0,0),
            new Point3d(20, 0, 10),
            new Point3d(20, 25, 0),
            new Point3d(0, 25, 10),
            new Point3d(0,0,0)
        }).ToNurbsCurve();
    }
}
