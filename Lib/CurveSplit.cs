using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace SplitCurves.Lib
{
	public static class Curves
	{
        /// <summary>
        /// Divides curve by a collection of planes.
        /// </summary>
        /// <param name="boundary">Closed curve.</param>
        /// <param name="planes">Collection of planes, must be parallel.</param>
        /// <param name="tolerance">Tolerance set per default to 1e-3.</param>
        /// <returns>A set of closed curve.</returns>
        public static List<Curve> DivideCurve(Curve boundary, List<Plane> planes, double tolerance = 1e-3)
        {
            RoutineChecks(boundary, planes);

            Curve splitBoundary = boundary;
            List<Curve> crvResult = new List<Curve>();

            for (int i = 0; i < planes.Count; i++)
            {
                CurveIntersections intersection = Intersection.CurvePlane(splitBoundary, planes[i], tolerance);
                if (intersection == null || intersection.Count == 0)
                {
                    if (i == planes.Count - 1) crvResult.Add(splitBoundary);
                    continue;
                }

                Curve[] crvs = splitBoundary.Split(new double[] {intersection[0].ParameterA, intersection[1].ParameterA});
                Curve tempCurve = new Line(intersection[0].PointA, intersection[1].PointA).ToNurbsCurve();

                // Close the two halves using the curve generated between the intersection points.
                // Retrieve the signed distance to understand which side of the plane is the the new boundary.
                Curve c0 = Curve.JoinCurves(new Curve[] {tempCurve, crvs[0]}, tolerance).First();
                double d0 = Utility.SignedDistance(planes[i], c0);

                Curve c1 = Curve.JoinCurves(new Curve[] {tempCurve, crvs[1]}, tolerance).First();
                double d1 = Utility.SignedDistance(planes[i], c1);

                // Collect one boundary and update the other for the next split.
                if (d0 < d1)
                {
                    splitBoundary = c0;
                    crvResult.Add(c1);
                }
                else
                {
                    splitBoundary = c1;
                    crvResult.Add(c0);
                }

                if (i != planes.Count - 1) continue;
                crvResult.Add(splitBoundary);
            }

            return crvResult;
        }

        /// <summary>
        /// Divides curve by a collection of planes.
        /// This solution is probably heavier, converting curve into Brep and cuts it, and required boundary to be closed and planar.
        /// </summary>
        /// <param name="boundary">Closed curve and planar.</param>
        /// <param name="planes">Collection of planes, must be parallel.</param>
        /// <param name="tolerance">Tolerance set per default to 1e-3.</param>
        /// <returns>A set of closed curve.</returns>
        public static List<Curve> DivideCurve2(Curve boundary, List<Plane> planes, double tolerance = 1e-3)
        {
            RoutineChecks(boundary, planes);

            // ToDo: if the curve has to be planar this test can be moved inside the RoutineChecks.
            if (!boundary.IsPlanar())
            {
                throw new Exception("Boundary should be Planar!");
            }

            // Convert the boundary in a Brep, and extract the face as a BrepFace allowing the use of the split method.
            Brep[] breps = Brep.CreatePlanarBreps(boundary, tolerance);
            BrepFace srfToCut = breps[0].Faces[0];
            List<Curve> cutters = new List<Curve>();

            foreach (Plane pl in planes)
            {
                CurveIntersections intersection = Intersection.CurvePlane(boundary, pl, tolerance);
                if (intersection == null || intersection.Count == 0) continue;

                // Collect the curves used for cutting the brep.
                cutters.Add(new Line(intersection[0].PointA, intersection[1].PointA).ToNurbsCurve());
            }

            if (cutters.Count == 0) return null;

            Brep spitBrep = srfToCut.Split(cutters, tolerance);
            // Extract and join the edges.
            return spitBrep.Faces.Select(face => Curve.JoinCurves(face.DuplicateFace(false).Edges, tolerance)).Select(s => s.First()).ToList();
        }

        /// <summary>
        /// Routine checks to verify the quality of the inputs.
        /// </summary>
        private static void RoutineChecks(Curve boundary, List<Plane> planes)
        {
            if (planes == null || planes.Count == 0)
            {
                throw new Exception("Input planes is null or empty!");
            }

            if (planes.Count > 1)
            {
                for (int i = 1; i < planes.Count; i++)
                {
                    if (planes[0].Normal.IsParallelTo(planes[i].Normal) == 0)
                    {
                        throw new Exception("Planes should be parallel!");
                    }
                }
            }

            if (!boundary.IsClosed)
            {
                throw new Exception("Boundary should be closed!");
            }
        }
    }
}
