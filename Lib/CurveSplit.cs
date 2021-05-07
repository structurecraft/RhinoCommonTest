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

			//DataTree<Curve> Plane_Lines = PlaneLines(boundary, planes);

			//List<Brep> Splits = SurfaceSplits(boundary, planes);

			//List<Curve> Split_Loops = SplitLoops(Splits);

			Brep[] Cutter_Splits = SplitingBrep(boundary, planes);

			List<Curve> Split_Loops = CutterSplitLoops(Cutter_Splits);


			return Split_Loops;


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

					if (Vector3d.Multiply(Plane_Ref.ZAxis, Current_Plane.ZAxis) > 0)
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

			PointCloud pt_Cld = new PointCloud(Pln_CP);



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
			if (Pln_CP.Count > 1)
			{
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
			else
			{
				return plane_N;

			}


		}


		public static Brep SplitingBrep(Curve boundary, List<Plane> planes_NS)
		{
			Brep[] brep = Rhino.Geometry.Brep.CreatePlanarBreps(boundary, 0.001);

			List<Curve> PlaneLines = new List<Curve>();

			List<double> split_t = new List<double>();

			//List<Curve> Loops_L = new List<Curve>();
			DataTree<Curve> Loops = new DataTree<Curve>();

			Brep cutter = new Rhino.Geometry.Brep();

			for (int i = 0; i < planes_NS.Count; i++)
			{

				Curve[] Crv_Pln_X;

				Point3d[] Crv_Pln_pt;

				BoundingBox brepbox = brep[0].GetBoundingBox(true);

				double max_dist = brepbox.Diagonal.Length;

				Rectangle3d Plane_rect = new Rectangle3d(planes_NS[i], max_dist, max_dist);

				Line rect_line = new Line(Plane_rect.Center, planes_NS[i].Origin);

				Plane_rect.Transform(Rhino.Geometry.Transform.Translation(rect_line.Direction));

				Brep[] Plane_brep = Rhino.Geometry.Brep.CreatePlanarBreps(Plane_rect.ToNurbsCurve(), 0.001);


				Rhino.Geometry.Intersect.Intersection.BrepBrep(brep[0], Plane_brep[0], 0.001, out Crv_Pln_X, out Crv_Pln_pt);



				cutter.Append(Plane_brep[0]);

			}

			Brep[] cutter_Splits = brep[0].Split(cutter, 0.01);

			return cutter_Splits;

		}

		public static List<Curve> CutterSplitLoops(Brep [] Cutter_Splits)
		{
			List<Curve> Split_Loops = new List<Curve>();

			List<double> area = new List<double>();

			Rhino.Geometry.colle Cutter_Splits[0].Faces;

			foreach (Brep splits_i in Splits)
			{
				area.Add(splits_i.GetArea());

				Surface Current_Surf = splits_i.Faces[0].DuplicateSurface();

				Curve[] EdgeCurve = Current_Surf.ToBrep().DuplicateEdgeCurves();

				Curve[] EdgeCurve_J = Rhino.Geometry.Curve.JoinCurves(EdgeCurve);

				Split_Loops.Add(EdgeCurve_J[0]);
			}

			return Split_Loops;

		}


		/*

		public static DataTree<Curve> PlaneLines(Curve boundary, List<Plane> planes_NS)
        {
			Brep[] brep = Rhino.Geometry.Brep.CreatePlanarBreps(boundary,0.001);

			List<Curve> PlaneLines = new List<Curve>();

			List<double> split_t = new List<double>();

			//List<Curve> Loops_L = new List<Curve>();
			DataTree<Curve> Loops = new DataTree<Curve>();

			Brep cutter = new Rhino.Geometry.Brep();

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

				
				
				cutter.Append(Plane_brep[0]);






				


				brep[0].Split(cutter, 0.01);


				if (Crv_Pln_X.Length > 0)
                {
				
					PlaneLines.Add(Crv_Pln_X[0]);

					double St_t;
					double Ed_t;

					boundary.ClosestPoint(Crv_Pln_X[0].PointAtStart,out St_t);
					boundary.ClosestPoint(Crv_Pln_X[0].PointAtEnd, out Ed_t);

					split_t.Add(St_t);
					split_t.Add(Ed_t);




				}

				

				IEnumerable<double> X = split_t;


				
				

				if (planes_NS.Count > 1)
                {
					for ( i = 0; i < planes_NS.Count - 1; i++ )
                    {
						Plane first_plane = planes_NS[i];
						Plane second_plane = planes_NS[i + 1];

						List<Curve> boundary_splits_A = new List<Curve>();
						List<Curve> boundary_splits_B = new List<Curve>();


						Rhino.Geometry.Intersect.CurveIntersections First_X = Rhino.Geometry.Intersect.Intersection.CurvePlane(boundary, first_plane, 0.01);
						Rhino.Geometry.Intersect.CurveIntersections Second_X = Rhino.Geometry.Intersect.Intersection.CurvePlane(boundary, second_plane, 0.01);

						if (First_X != null && Second_X != null)
                        {
							

							Curve[] Curve_A1 = boundary.Split(First_X[0].ParameterA);

							foreach (Curve crv in Curve_A1)
                            {
								Curve[] Curve_A2 = crv.Split(Second_X[0].ParameterA);

								if(Curve_A2 != null)
                                {
									foreach (Curve Crv in Curve_A2)
									{
										boundary_splits_A.Add(Crv);

									}
								}
                            }

							List<Curve> Crv_A = CurveSelect(boundary_splits_A, first_plane, second_plane);
							
							
							
							if (Crv_A.Count == 1)
							{
								Loops.Add(Crv_A[0], new Grasshopper.Kernel.Data.GH_Path(i));
							}


							Curve[] Curve_B1 = boundary.Split(First_X[1].ParameterA);

							foreach (Curve crv in Curve_B1)
							{
								Curve[] Curve_B2 = crv.Split(Second_X[1].ParameterA);

								if (Curve_B2 != null)
								{
									foreach (Curve Crv in Curve_B2)
									{
										boundary_splits_B.Add(Crv);

									}
								}
							}

							List<Curve> Crv_B = CurveSelect(boundary_splits_B, first_plane, second_plane);

							if (Crv_B.Count == 1)
							{
								Loops.Add(Crv_B[0], new Grasshopper.Kernel.Data.GH_Path(i));
							}
						}
						
                    }
                }
				else
                {
					Rhino.Geometry.Intersect.CurveIntersections First_X = Rhino.Geometry.Intersect.Intersection.CurvePlane(boundary, planes_NS[0], 0.01);

					Curve[] boundary_splits = boundary.Split(Plane_brep[0],0.01,0.1);

					

					
							


					Loops.Add(boundary_splits[0], new Grasshopper.Kernel.Data.GH_Path(0));
					Loops.Add(PlaneLines[0], new Grasshopper.Kernel.Data.GH_Path(0));

					Loops.Add(boundary_splits[1], new Grasshopper.Kernel.Data.GH_Path(1));
					Loops.Add(PlaneLines[0], new Grasshopper.Kernel.Data.GH_Path(1));

				}

				

			}

			return Loops;
        }
		
		public static List<Brep> SurfaceSplits(Curve boundary, List<Plane> planes)
        {

			Brep[] brep = Rhino.Geometry.Brep.CreatePlanarBreps(boundary, 0.001);

			List<Curve> PlaneLines = new List<Curve>();

			List<double> split_t = new List<double>();

			List<Brep> Split_Breps = new List<Brep>();


			if (planes.Count > 1)
			{
				for (int i = 0; i < planes.Count - 1; i++)
				{
					Plane first_plane = planes[i];
					Plane second_plane = planes[i + 1];

					List<Curve> boundary_splits_A = new List<Curve>();
					List<Curve> boundary_splits_B = new List<Curve>();

					//List<Curve> Loops_L = new List<Curve>();
					DataTree<Curve> Loops = new DataTree<Curve>();

					Line Center_Line = new Line(first_plane.Origin, second_plane.Origin);

					Point3d Center_Point = Center_Line.PointAt(0.5);

					Vector3d first_Vector = new Line(Center_Point, first_plane.Origin).UnitTangent;
					Vector3d second_Vector = new Line(Center_Point, second_plane.Origin).UnitTangent;


					
					if (Vector3d.Multiply(first_plane.ZAxis, first_Vector) > 0)
                    {
						brep[0].Trim(first_plane, 0.01);

						if(Vector3d.Multiply(second_plane.ZAxis, second_Vector) > 0)
                        {
								
							
							brep[0].Trim(second_plane, 0.01);

							double d = brep[0].GetArea();

							Split_Breps.Add(brep[0]);
						}
						else
                        {
							second_plane.Flip();
							brep[0].Trim(second_plane, 0.01);

							double d = brep[0].GetArea();

							Split_Breps.Add(brep[0]);
						}
					}

					else
                    {
						first_plane.Flip();
						brep[0].Trim(first_plane, 0.01);

						if (Vector3d.Multiply(second_plane.ZAxis, second_Vector) > 0)
						{
							brep[0].Trim(second_plane, 0.01);

							double d = brep[0].GetArea();

							Split_Breps.Add(brep[0]);
						}
						else
						{
							second_plane.Flip();
							brep[0].Trim(second_plane, 0.01);

							double d = brep[0].GetArea();

							Split_Breps.Add(brep[0]);
						}
					}
				}
			}

			return Split_Breps;

		}

		
		public static List<Curve> SplitLoops(List<Brep> Splits)
        {
			List<Curve> Split_Loops = new List<Curve>();

			List<double> area =new List<double>();

			foreach (Brep splits_i in Splits)
			{
				area.Add(splits_i.GetArea());

				Surface Current_Surf = splits_i.Faces[0].DuplicateSurface();

				Curve[] EdgeCurve = Current_Surf.ToBrep().DuplicateEdgeCurves();

				Curve[] EdgeCurve_J = Rhino.Geometry.Curve.JoinCurves(EdgeCurve);

				Split_Loops.Add(EdgeCurve_J[0]);
			}

			return Split_Loops;
        }

	
		
		public static List<Curve> CurveSelect(List<Curve> boundary_splits,Plane first_plane,Plane Second_plane)
        {
			List<Curve> S_Curve = new List<Curve>();

			foreach (Curve crv in boundary_splits)
            {
				if (crv != null)
                {
					crv.Domain = new Interval(0, 1);
					Point3d test_point = crv.PointAt(0.5);

					Point3d first_plane_point = test_point;
					Point3d second_plane_point = test_point;

					first_plane_point.Transform(Rhino.Geometry.Transform.PlanarProjection(first_plane));
					second_plane_point.Transform(Rhino.Geometry.Transform.PlanarProjection(Second_plane));

					Vector3d First_Vect = new Line(test_point, first_plane_point).UnitTangent;

					Vector3d Second_Vect = new Line(test_point, second_plane_point).UnitTangent;

					double angle = Vector3d.VectorAngle(First_Vect, Second_Vect);


					if (angle > 1.5708 && angle < 4.71239)
					{

						S_Curve.Add(crv);
						//return crv;

						
						Loops.Add(crv, new Grasshopper.Kernel.Data.GH_Path(i));

						Loops.Add(PlaneLines[i], new Grasshopper.Kernel.Data.GH_Path(i));

						Loops.Add(PlaneLines[i + 1], new Grasshopper.Kernel.Data.GH_Path(i));
						

						
					}

				}
			
			}
			return S_Curve;
		}
		*/
	}
	
}
