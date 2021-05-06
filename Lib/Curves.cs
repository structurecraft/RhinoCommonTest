using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace SplitCurves.Lib
{
    /// <summary>
    /// Includes curve related static methods.
    /// </summary>
    public static class Curves
    {
        /// <summary>
        /// Divides given boundary into sub boundaries with given planes.
        /// </summary>
        /// <param name="boundary"> Closed boundary to slice.</param>
        /// <param name="planes"> Slicers of boundary.</param>
        /// <returns> Collection of closed boundaries.</returns>
        public static IEnumerable<Curve> DivideCurve(Curve boundary, IEnumerable<Plane> planes)
        {
            if (planes == null || planes.Count() == 0)
			{
                throw new System.Exception("There are no any planes to split boundary!");
			}

            // Create planar brep to split later.
            Brep brep = Brep.CreatePlanarBreps(boundary, 10e5).First();

            // Get cutter curves from planes.
            IEnumerable<Curve> cutters = GetCutters(boundary, planes);

            // Split brep with cutters.
            IEnumerable<Brep> subSurfaces = brep.Split(cutters, 1);

            // Duplicate borders from splitted breps.
            return GetBorderCurveFromBreps(subSurfaces);
        }

        /// <summary>
        /// Validates curves either appropriate for splitting or not.
        /// </summary>
        /// <param name="sourceCurves"> Will be queried curves.</param>
        /// <returns> Collection of appropriate curves.</returns>
        public static IEnumerable<Curve> ValidateForSplitting(IEnumerable<Curve> sourceCurves)
		{
			foreach (Curve curve in sourceCurves)
			{
				if (curve.IsPlanar() && curve.IsValid && curve.IsInPlane(Plane.WorldXY))
				{
                    yield return curve;
				}
			}
		}

        /// <summary>
        /// Creates extended cutter curves from planes.
        /// </summary>
        /// <param name="boundary"> Closed boundary to slice.</param>
        /// <param name="planes"> Slicers of boundary.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates curve parallel to Y-axis of plane.
        /// </summary>
        /// <param name="plane"> Source plane.</param>
        /// <returns> Curve that parallel to Y-axis of plane.</returns>
        private static Curve GetCutter(Plane plane)
        {
            // Initialize transfrom
            Transform moveParallelToPlane = Transform.Translation(plane.YAxis);

            // Move point
            Point3d movedPoint = new Point3d(plane.Origin);
            movedPoint.Transform(moveParallelToPlane);

            return new LineCurve(plane.Origin, movedPoint);
        }

        /// <summary>
        /// Creates border curves from breps.
        /// </summary>
        /// <param name="breps"> Collection of source breps.</param>
        /// <returns> Collection of borders.</returns>
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

           IEnumerable<Curve> borderCurve = Curve.JoinCurves(curves, 1).AsEnumerable();
            return borderCurve.First();
        }

    }
}
