using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
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
            const ObjectType filter = ObjectType.Curve;
            ObjRef objref;
            var rc = RhinoGet.GetOneObject("Select curve to divide", false, filter, out objref);
            if (rc != Result.Success)
                return rc;

            var curve = objref.Curve();
            if (curve == null)
                return Result.Failure;

            var loop_count = 2;
			rc = RhinoGet.GetInteger("Divide curve into how many segments?", false, ref loop_count);
            if (rc != Result.Success)
                return rc;

            List<Plane> planes = Curves.CreateDivisionPlanes(curve, loop_count);

            List<Curve> loops = Curves.DivideCurve(curve, planes);
			
			foreach (var loop in loops)
                doc.Objects.AddCurve(loop);

			return Result.Success;
		}
	}
}
