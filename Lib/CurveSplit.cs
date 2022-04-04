using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace SplitCurves.Lib
{
	public static class Curves
	{

		public static List<Curve> DivideCurve(Curve boundary, List<Plane> planes)
        {
            List<Curve> curves = new List<Curve>();

            try
            {
                curves.Add(GetExteriorCurves(boundary, planes[0])[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            List<List<Point3d>> intersectionPairs = GetIntersectionPairs(boundary, planes);
            for (var i = 0; i < intersectionPairs.Count - 1; i++)
            {
                Point3d a = intersectionPairs[i][0];
                Point3d b = intersectionPairs[i][1];
                Point3d c = intersectionPairs[i+1][0];
                Point3d d = intersectionPairs[i+1][1];

                curves.Add(GetInteriorCurve(boundary, a, b, c, d));
            }

            curves.Add(GetExteriorCurves(boundary, planes[planes.Count - 1])[1]);
            
            return curves;
		}

        /// <summary>
        /// Get pairs of curve/plane intersection points, ordered by the global y axis
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="planes"></param>
        /// <returns></returns>
        private static List<List<Point3d>> GetIntersectionPairs(Curve boundary, List<Plane> planes)
        {
            List<List<Point3d>> intersectionPairs = new List<List<Point3d>>();
            foreach (var plane in planes)
            {
                List<Point3d> points = new List<Point3d>(); 
                var events = Intersection.CurvePlane(boundary, plane, 0.001);
                List<Point3d> pts = events.Select(e => e.PointA).OrderBy(pt => pt.Y).ToList();
                intersectionPairs.Add(pts);
            }

            return intersectionPairs;
        }

        /// <summary>
        /// Create closed curves in-between cutting planes
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static Curve GetInteriorCurve(Curve boundary, Point3d a, Point3d b, Point3d c, Point3d d)
        {
            boundary.ClosestPoint(a, out var ta);
            boundary.ClosestPoint(b, out var tb);
            boundary.ClosestPoint(c, out var tc);
            boundary.ClosestPoint(d, out var td);

            List<double> acParams = new List<double>() { ta, tc };
            List<double> bdParams = new List<double>() { tb, td };

            Curve ab = new Line(a, b).ToNurbsCurve();
            Curve cd = new Line(c, d).ToNurbsCurve();

            Curve ac = boundary.Split(acParams).OrderBy(crv => crv.GetLength()).ToList().FirstOrDefault();
            Curve bd = boundary.Split(bdParams).OrderBy(crv => crv.GetLength()).ToList().FirstOrDefault();

            Curve[] curves = Curve.JoinCurves(new Curve[] { ab, cd, ac, bd }, 1.0);
            Curve curve = curves.FirstOrDefault();

            return curve;
        }

        /// <summary>
        /// Create closed curves at outmost ends of boundary curve along the global x axis
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        private static List<Curve> GetExteriorCurves(Curve boundary, Plane plane)
        {
            var events = Intersection.CurvePlane(boundary, plane, 0.001);
            List<Point3d> pts = events.Select(e => e.PointA).ToList();
            List<double> parameters = events.Select(e => e.ParameterA).ToList();

            var subcurves = boundary.Split(parameters).OrderBy(crv => crv.GetBoundingBox(Plane.WorldXY).Center.X).ToList();
            Curve line = new Line(pts[0], pts[1]).ToNurbsCurve();

            List<Curve> curves = new List<Curve>();
            foreach (Curve subcurve in subcurves)
            {
                Curve curve = Curve.JoinCurves(new Curve[] { subcurve, line }).FirstOrDefault();
                curves.Add(curve);
            }
            
            return curves;
        }

        /// <summary>
        /// Create cutting planes with normal aligned with the global x axis
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="loopCount"></param>
        /// <returns></returns>
        public static List<Plane> CreateDivisionPlanes(Curve boundary, int loopCount)
        {
            var bbox = boundary.GetBoundingBox(Plane.WorldXY);

            Point3d startPt = bbox.Min;
            Point3d endPt = bbox.Max;
            Line line = new Line(startPt, endPt);
            Curve curve = new LineCurve(line);

            Point3d[] pts;
            curve.DivideByCount(loopCount, false, out pts);

            List<Plane> planes = new List<Plane>();
            foreach (Point3d pt in pts)
            {
                planes.Add(new Plane(pt, Vector3d.XAxis));
            }

            return planes;
        }
    }
}
