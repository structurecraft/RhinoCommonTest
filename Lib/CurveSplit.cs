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
			List<Plane> planes_N = PlaneNormalDirectionCheck(planes);

			List<Plane> planes_NS = PlanesinOrder(planes_N);

			List<Curve> Plane_Lines = PlaneLines(boundary, planes);

			



			return Plane_Lines;

			
		}

		public static List<Plane> PlaneNormalDirectionCheck(List<Plane> planes)
        {
			
			
			List<Plane> planes_N = new List<Plane>();
			if (planes.Count > 1)
            {
				Plane Plane_Ref = planes[0];

				for (int i = 0; i < planes.Count; i++)
                {
					Plane Current_Plane = planes[i];

					if (Vector3d.Multiply(Plane_Ref.ZAxis,Current_Plane.ZAxis) > 0)
                    {
						planes_N.Add(Current_Plane);
						//Pln_CP.Add(Current_Plane.Origin);
					}
					else
                    {
						Current_Plane.Flip();
						planes_N.Add(Current_Plane);
						//Pln_CP.Add(Current_Plane.Origin);

					}
				}
				return planes_N;


			}
			else
            {
				if (planes.Count != 0)
				{
					planes_N.Add(planes[0]);


					return planes_N;
				}
				else
                {
					return null;
                }

            }

		}

		public static List<Plane> PlanesinOrder(List<Plane> plane_N)
        {
			List<Point3d> Pln_CP = new List<Point3d>();
			foreach (Plane pl in plane_N)
            {
				Pln_CP.Add(pl.Origin);
			}

			PointCloud pt_Cld = new PointCloud( Pln_CP);



			BoundingBox Box = pt_Cld.GetBoundingBox(true);
			List<Point3d> EndPoints = new List<Point3d>();

			for (int i = 0; i < Pln_CP.Count; i++)
			{
				if (Pln_CP[i] != null)
				{

					Point3d box_pt = Box.ClosestPoint(Pln_CP[i]);
					double d = Pln_CP[i].DistanceTo(box_pt);

					if (d < 0.01)
					{
						EndPoints.Add(Pln_CP[i]);
					}
				}

			}

			Line Plane_Centeral_Line = new Line(EndPoints[0], EndPoints[1]);

			List<double> Line_t = new List<double>();

			foreach (Point3d pt in Pln_CP)
			{
				double t;
				Plane_Centeral_Line.ToNurbsCurve().ClosestPoint(pt, out t);
				Line_t.Add(t);
			}

			List<double> Line_t_Sort = new List<double>(Line_t);

			List<Plane> planes_NS = new List<Plane>();

			Line_t_Sort.Sort();
			foreach (double t in Line_t_Sort)
			{
				Plane Current_Plane = plane_N[Line_t.IndexOf(t)];
				planes_NS.Add(Current_Plane);
			}

			return planes_NS;
		}

		public static List<Curve> PlaneLines(Curve boundary, List<Plane> planes_NS)
        {
			Brep[] brep = Rhino.Geometry.Brep.CreatePlanarBreps(boundary,0.001);

			

			
			
			List<Curve> PlaneLines = new List<Curve>();

			List<double> split_t = new List<double>();

			List<Curve> Loops = new List<Curve>();





			for (int i = 0; i < planes_NS.Count; i++)
            {

				Curve[] Crv_Pln_X;

				Point3d[] Crv_Pln_pt;

				BoundingBox brepbox =  brep[0].GetBoundingBox(true);

				double max_dist = brepbox.Diagonal.Length;

				Rectangle3d Plane_rect = new Rectangle3d(planes_NS[i], max_dist, max_dist);
				 
				Line rect_line =  new Line(Plane_rect.Center, planes_NS[i].Origin);

				Plane_rect.Transform(Rhino.Geometry.Transform.Translation(rect_line.Direction));

				Brep[] Plane_brep = Rhino.Geometry.Brep.CreatePlanarBreps(Plane_rect.ToNurbsCurve(), 0.001);


				Rhino.Geometry.Intersect.Intersection.BrepBrep(brep[0], Plane_brep[0], 0.001, out Crv_Pln_X, out Crv_Pln_pt);



				if (Crv_Pln_X.Length > 0)
                {
					
				
					PlaneLines.Add(Crv_Pln_X[0]);

					
					
				}


				/*
				if (X != null)
				{ 
					if (X.Count == 2)
					{
						split_t.Add(X[0].ParameterA);
						split_t.Add(X[1].ParameterA);

						Line CurrentLine = new Line(X[0].PointA, X[1].PointA);
						PlaneLines.Add(CurrentLine.ToNurbsCurve());
					}
					else
					{
						PlaneLines.Add(null);
					}

				}
				*/
				foreach( Curve crv in PlaneLines)
                {
					brep[0].AddTrimCurve(crv);

				}


				Rhino.Geometry.Collections.BrepTrimList trims = brep[0].Trims;

				


				foreach (BrepTrim face in trims)
                {
					BrepLoop trimedges = face.Loop;

					//Curve[] Joined_Crv = Rhino.Geometry.Curve.JoinCurves(trimedges);

					trimedges.To3dCurve();

					Loops.Add(trimedges.To3dCurve());
				}
				
				
				/*
				Brep[] brep_spl = brep[0].Split(PlaneLines,0.001);

				

				foreach (Brep item in brep_spl)
                {
					Curve[] Joined_Crv =  Rhino.Geometry.Curve.JoinCurves(item.Edges);

					Loops.Add(Joined_Crv[0]);

				}
				

				

				Rhino.Geometry.Collections.BrepLoopList brepLoops = brep_spl[0].Loops;

				PlaneLines.Add(brepLoops[0].To3dCurve());

				
				foreach (Brep brep_i in brep_spl)
                {
					brep_i.Edges;
                }

				
				DataTree<Curve> Loops = new DataTree<Curve>();

				Curve[] boundary_splits = boundary.Split(split_t);

				if (planes_NS.Count > 1)
                {
					for ( i = 0; i < planes_NS.Count - 1; i++ )
                    {
						Plane first_plane = planes_NS[i];
						Plane second_plane = planes_NS[i + 1];

						foreach (Curve crv in boundary_splits)
                        {
							crv.Rebuild(2, 1, false);
							Point3d test_point = crv.PointAt(0.5);

							Point3d first_plane_point = test_point;
							Point3d second_plane_point = test_point;

							first_plane_point.Transform(Rhino.Geometry.Transform.PlanarProjection(first_plane));
							second_plane_point.Transform(Rhino.Geometry.Transform.PlanarProjection(first_plane));

							Vector3d First_Vect = new Line(test_point, first_plane_point).UnitTangent;
								
							Vector3d Second_Vect = new Line(test_point, second_plane_point).UnitTangent;

							double angle = Vector3d.VectorAngle(First_Vect, Second_Vect);


							if (angle > 1.5708 && angle < 4.71239)
                            {
								Loops.Add(crv, new Grasshopper.Kernel.Data.GH_Path(i));

								Loops.Add(PlaneLines[i], new Grasshopper.Kernel.Data.GH_Path(i));

								Loops.Add(PlaneLines[i + 1], new Grasshopper.Kernel.Data.GH_Path(i));

							}


						}
                    }




                }
				else
                {

					Loops.Add(boundary_splits[0], new Grasshopper.Kernel.Data.GH_Path(0));
					Loops.Add(PlaneLines[0], new Grasshopper.Kernel.Data.GH_Path(0));

					Loops.Add(boundary_splits[1], new Grasshopper.Kernel.Data.GH_Path(1));
					Loops.Add(PlaneLines[0], new Grasshopper.Kernel.Data.GH_Path(1));

				}

				*/

			}

			return Loops;
        }


	}
}
