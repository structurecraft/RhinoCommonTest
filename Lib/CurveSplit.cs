using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace SplitCurves.Lib
{
	public static class Curves
	{
		public static double tolerance = 0.01;
		/// <summary>
		/// Given a curve, it will be splitted by a series of planes into smaller loops. To acchieve this task, an auxiliary brep it's created and divided by the planes. This will create a list of smaller breps from which the boundary curves are obtained.  
		/// </summary>
		/// <param name="boundary">The curve to be splitted.</param>
		/// <param name="planes">The planes that split the curve.</param>
		/// <returns>A list of closed curves.</returns>
		public static List<Curve> DivideCurve(Curve boundary, List<Plane> planes)
		{
			if (!planes.Any())
			{
				throw new ArgumentException("At least one plane must be provided to divide the curve.");
			}

			/*	
			// Checking if the boundary in planar. It may not be necessary but the method ("CreatePlanarBreps") documentation does not specify anything.
			if (!boundary.IsPlanar())
            {
				throw new ArgumentException("The boundary curve must be planar.");
            }
			*/

			// Element to be divided.
			Brep auxBrepFromBoundary = Brep.CreatePlanarBreps(boundary, tolerance).First();

			// Intersection curves from planes.
			IEnumerable<Curve> intersectionCurves = GetIntersectionCurves(auxBrepFromBoundary, planes);

			if (!intersectionCurves.Any())
            {
				throw new ArgumentException("There are no planes intersecting the curve.");
            }
            else
            {
				// Getting subbreps from the splitted brep.
				Brep[] subBreps = auxBrepFromBoundary.Split(intersectionCurves, tolerance);

				// Getting the edges curves for every brep and joining them.
				return subBreps.Select(sb => sb.DuplicateEdgeCurves()).Select(crvs => Curve.JoinCurves(crvs)).SelectMany(joinedCrvs => joinedCrvs).ToList();
            }
		}

		/// <summary>
		/// This method get the intersection curves between a brep a serie of planes.
		/// </summary>
		/// <param name="brep">A Rhino Brep.</param>
		/// <param name="planes">The planes that intersect the brep.</param>
		/// <returns>A sequence of curves.</returns>
		private static IEnumerable<Curve> GetIntersectionCurves(Brep brep, List<Plane> planes)
        {
			foreach (Plane plane in planes)
			{	
				Curve[] intersectionCurvesByPlane = new Curve[] { };
				Point3d[] intersectionPointsByPlane = new Point3d[] { };
				bool isIntersected = Intersection.BrepPlane(brep, plane, tolerance, out intersectionCurvesByPlane, out intersectionPointsByPlane);
				if (isIntersected && intersectionCurvesByPlane.Any())
                {
					foreach (Curve curve in intersectionCurvesByPlane)
                    {
						yield return curve;
                    }
                }
			}
		}
	}
}
