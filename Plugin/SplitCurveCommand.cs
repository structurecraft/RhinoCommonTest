using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace SplitCurves.Plugin
{
	public class SplitCurveCommand : Command
	{
		public SplitCurveCommand()
		{
			// Rhino only creates one instance of each command class defined in a
			// plug-in, so it is safe to store a refence in a static property.
			Instance = this;
		}



		///<summary>The only instance of this command.</summary>
		public static SplitCurveCommand Instance { get; private set; }

		///<returns>The command name as it appears on the Rhino command line.</returns>
		public override string EnglishName => "SplitCurve";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			ObjRef eval_Crv;
			Rhino.Input.RhinoGet.GetOneObject("Please Select the Curve", false, Rhino.DocObjects.ObjectType.Curve, out eval_Crv);

			ObjRef[] plane_point;
			Rhino.Input.RhinoGet.GetMultipleObjects("Please Select spliting planes", false, Rhino.DocObjects.ObjectType.Point, out plane_point);

			

			Vector3d Camera_Direction = Rhino.DocObjects.ViewportInfo.DefaultCameraDirection;

			List<Plane> plane = new List<Plane>();

			foreach (ObjRef pt in plane_point)
            {
				Point3d Currentpoint = pt.SelectionPoint();

				Plane pre_plane = new Plane(Currentpoint, Camera_Direction);

				Plane final_plane = new Plane(Currentpoint, Camera_Direction, pre_plane.XAxis);

				plane.Add(final_plane);
				
            }


			List<Curve> LoopCurve = SplitCurves.Lib.Curves.DivideCurve(eval_Crv.Curve(), plane);

			foreach (Curve crv in LoopCurve )
            {
				if (doc.Objects.AddCurve(crv) != System.Guid.Empty )
                {
					doc.Objects.AddCurve(crv);
                }
			}

			return Result.Success;
		}
	}
}
