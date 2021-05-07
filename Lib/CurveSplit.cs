using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rhino.Geometry;
using Grasshopper;

namespace SplitCurves.Lib
{
	public static class Curves
	{

		public static List<Curve> DivideCurve(Curve boundary, List<Plane> planes)
		{
			Brep[] Cutter_Splits = SplitingBrep(boundary, planes);

			List<Curve> Split_Loops = CutterSplitLoops(Cutter_Splits);

			return Split_Loops;

		}

		public static Brep[] SplitingBrep(Curve boundary, List<Plane> planes)
		{
			Brep[] Boundary_Brep = Rhino.Geometry.Brep.CreatePlanarBreps(boundary, 0.001);

			Brep cutter = new Rhino.Geometry.Brep();
			// Loop to retrive every cutting brep derieved from planes
			for (int i = 0; i < planes.Count; i++)
			{
				Curve[] Crv_Pln_X;

				Point3d[] Crv_Pln_pt;

				// Creating bounding box to determine the maximum size of the cutter brep which may be needed
				BoundingBox brepbox = Boundary_Brep[0].GetBoundingBox(true);

				// Diagonal length of the bounding box
				double max_dist = brepbox.Diagonal.Length;

				Rectangle3d Plane_rect = new Rectangle3d(planes[i], max_dist, max_dist);

				Line rect_line = new Line(Plane_rect.Center, planes[i].Origin);

				// Move geometry to the center of the plane
				Plane_rect.Transform(Rhino.Geometry.Transform.Translation(rect_line.Direction));

				Brep[] Plane_brep = Rhino.Geometry.Brep.CreatePlanarBreps(Plane_rect.ToNurbsCurve(), 0.001);

				Rhino.Geometry.Intersect.Intersection.BrepBrep(Boundary_Brep[0], Plane_brep[0], 0.001, out Crv_Pln_X, out Crv_Pln_pt);

				cutter.Append(Plane_brep[0]);

			}


			// Spliting with boundary Brep with Cutter(Disjoined)
			Brep[] cutter_Splits = Boundary_Brep[0].Split(cutter, 0.01);

			// List of Split breps
			return cutter_Splits;

		}

		public static List<Curve> CutterSplitLoops(Brep [] Cutter_Splits)
		{
			List<Curve> Split_Loops = new List<Curve>();

			// Retrive loops from every split boundary brep
			foreach (Brep splits_i in Cutter_Splits)
			{
				BrepLoop loop = splits_i.Faces[0].OuterLoop;

				Split_Loops.Add(loop.To3dCurve());

			}

			return Split_Loops;

		}

	}

}
