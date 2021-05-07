using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SplitCurves.Lib;

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
            // Gets the boundary.
            var selectedBoundary = RhinoGet.GetOneObject(
                "Select one closed curve", false, ObjectType.Curve, out ObjRef boundary);
            if (selectedBoundary != Result.Success)
                return selectedBoundary;

            // Gets the locations where the cuts will be made.
            var getPoint = new GetPoint();
            List<Point3d> pts = new List<Point3d>();
            while (true)
            {
                getPoint.SetCommandPrompt("click location to create point. Click esc to exit.");
                getPoint.AcceptNothing(true);
                getPoint.Get();
                if (getPoint.CommandResult() != Result.Success)
                    break;
                pts.Add(getPoint.Point());
            }
            
            // Gets the normal, used to generate the planes.
            var getPlaneNormal = new GetVector("End point of vector, representing the normal of the cutting planes.");
            getPlaneNormal.Get();
            if (getPlaneNormal.CommandResult() != Result.Success)
                return getPlaneNormal.CommandResult();

            Vector3d normal = getPlaneNormal.Point() - getPlaneNormal.basePt;
            List<Plane> planes = pts.Select(pt => new Plane(pt, normal)).ToList();

            try
            {
                var crv = Curves.DivideCurve(boundary.Curve(), planes, doc.ModelAbsoluteTolerance);
                foreach (var curve in crv)
                {
                    doc.Objects.AddCurve(curve);
                }
                return Result.Success;
            }
            catch (Exception e)
            {
                RhinoApp.WriteLine($"SplitCurve failed: {e.Message}");
                return Result.Failure;
            }
        }
    }
}
