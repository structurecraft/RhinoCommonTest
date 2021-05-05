using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace SplitCurves.Lib
{
    public static class Curves
    {

        public static IEnumerable<Curve> DivideCurve(Curve boundary, IEnumerable<Plane> planes)
        {
            // Create planar brep to split later.
            Brep brep = Brep.CreatePlanarBreps(boundary, 10e5).First();

            // Get cutter curves from planes.
            IEnumerable<Curve> cutters = GetCutters(boundary, planes);

            // Split brep with cutters.
            IEnumerable<Brep> subSurfaces = brep.Split(cutters, 1);

            // Duplicate borders.
            IEnumerable<Curve> newBoundaries = GetBorderCurveFromBreps(subSurfaces);

            return newBoundaries;
        }

        private static IEnumerable<Curve> GetCutters(Curve boundary, IEnumerable<Plane> planes)
        {
            foreach (Plane plane in planes)
            {
                // Create cutter curve
                Curve cutter = GetCutter(plane);

                // Collect boundary to collection.
                List<GeometryBase> geometries = new List<GeometryBase>() { boundary };

                // Extend cutter line to boundary with both sides.
                Curve extendedCutter = cutter.Extend(CurveEnd.Both, CurveExtensionStyle.Line, geometries);

                // Return and add to collection.
                yield return extendedCutter;
            }
        }

        private static Curve GetCutter(Plane plane)
        {
            // Initialize transfrom
            Transform moveParallelToPlane = Transform.Translation(plane.YAxis);

            // Move point
            Point3d movedPoint = new Point3d(plane.Origin);
            movedPoint.Transform(moveParallelToPlane);

            return new LineCurve(plane.Origin, movedPoint);
        }

        private static IEnumerable<Curve> GetBorderCurveFromBreps(IEnumerable<Brep> breps)
        {
            foreach (Brep brep in breps)
            {
                if (brep != null)
                {
                    yield return GetBorderCurveFromBrep(brep);
                }
            }
        }

        /// <summary>
        /// This method create projected curve shape from brep
        /// Similar DupBorder command in Rhino.
        /// </summary>
        /// <param name="floor"> Source brep.</param>
        /// <returns> Border curve of the given brep.</returns>
        private static Curve GetBorderCurveFromBrep(Brep brep)
        {
            List<Curve> curves = new List<Curve>();
            foreach (BrepEdge edge in brep.Edges)
            {
                // Find only the naked edges.
                if (edge.Valence == EdgeAdjacency.Naked)
                {
                    Curve crv = edge.DuplicateCurve();
                    if (crv.IsLinear())
                    {
                        crv = new LineCurve(crv.PointAtStart, crv.PointAtEnd);
                    }

                    curves.Add(crv);
                }
            }

            Curve[] borderCurve = Curve.JoinCurves(curves, 1);
            return borderCurve[0];
        }

    }
}
