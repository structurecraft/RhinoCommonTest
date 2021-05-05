using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rhino.Geometry;

namespace SplitCurves.Lib
{
	public static class Curves
	{

		public static List<Curve> DivideCurve(Curve boundary, List<Plane> planes)
		{
			List<Plane> planes_N = PlaneNormalDirectionCheck(planes);

			List<Curve> Plane_Lines = PlaneLines(boundary, planes_N);

			
			
			

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

					}
					else
                    {
						Current_Plane.Flip();
						planes_N.Add(Current_Plane);
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

		public static List<Curve> PlaneLines(Curve boundary, List<Plane> planes_N)
        {
			List<Curve> PlaneLines = new List<Curve>();

			for (int i = 0; i < planes_N.Count; i++)
            {
				Rhino.Geometry.Intersect.CurveIntersections X = Rhino.Geometry.Intersect.Intersection.CurvePlane(boundary, planes_N[i], 0.01);

				if(X.Count == 2 )
                {
					Line CurrentLine = new Line(X[0].PointA, X[1].PointA);
					PlaneLines.Add(CurrentLine.ToNurbsCurve());
				}
				else
                {
					PlaneLines.Add(null);
				}
			}

			return PlaneLines;
        }


	}
}
