using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects.Tables;
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
		/// <summary>
		/// This command allow the user split a curve by a serie of planes. The workflow is as follows:
		/// 
		/// 1. Select a curve in the model.
		/// 2. Specifies the cutting planes direction by two points.
		/// 3. Pick points that defines the planes origin. (The planes will pass through these points)
		/// </summary>
		/// <param name="doc">The current document.</param>
		/// <param name="mode">The command running mode.</param>
		/// <returns>The command result code.</returns>
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{

            Result selectCurveResult = GetSelectedCurve(out Curve selectedCurve);
            if (selectCurveResult != Result.Success)
				return selectCurveResult;

			Result planeNormalVectorResult = GetPlaneNormalVector(out Vector3d planeNormalVector);
			if (planeNormalVectorResult != Result.Success)
				return planeNormalVectorResult;

			Result splitPointsResult = GetPlanesOriginPoints(out List<Point3d> splitPoints);
			if (splitPointsResult != Result.Success)
				return splitPointsResult;

			List<Plane> splitPlanes = GetSplitPlanes(planeNormalVector, splitPoints);

			try
			{
				List<Curve> curves = Curves.DivideCurve(selectedCurve, splitPlanes);
				foreach (Curve curve in curves)
				{
					doc.Objects.AddCurve(curve);
				}
				doc.Views.Redraw();
				return Result.Success;
			}
			catch (ArgumentException e)
			{
				RhinoApp.WriteLine("Error executing the command: \"{0}\"", e.Message);
				return Result.Failure;
			}
		}
		/// <summary>
		/// Select a curve in the document.
		/// </summary>
		/// <param name="selectedCurve">The selected curve.</param>
		/// <returns>A command result code.</returns>
		private Result GetSelectedCurve(out Curve selectedCurve)
        {
			selectedCurve = null;

			GetObject go = new GetObject();

			go.GeometryFilter = ObjectType.Curve;
			go.GeometryAttributeFilter = GeometryAttributeFilter.ClosedCurve;
			go.SetCommandPrompt("Select curve to divide");
			go.EnablePreSelect(true, false);
			go.EnableClearObjectsOnEntry(false);
			go.EnableUnselectObjectsOnExit(false);
			go.DeselectAllBeforePostSelect = false;

			go.Get();

			if (go.CommandResult() != Result.Success)
				return go.CommandResult();

			selectedCurve = go.Object(0).Curve();

			return Result.Success;
		}
		/// <summary>
		/// Select two points on the screen and calculates the normal vector to the vector defined by them.
		/// </summary>
		/// <param name="planeNormalVector">Normal vector defining the cutting planes.</param>
		/// <returns>A command result code.</returns>
		private Result GetPlaneNormalVector(out Vector3d planeNormalVector)
        {
			planeNormalVector = new Vector3d();
			GetPoint gp = new GetPoint();
			gp.DynamicDrawColor = System.Drawing.Color.IndianRed;

			gp.SetCommandPrompt("Select start point to define the cutting planes direction");
			gp.Get();
			if (gp.CommandResult() != Result.Success)
				return gp.CommandResult();

			Point3d planeDirectionStartPoint = gp.Point();

			gp.SetCommandPrompt("Select end point to define the cutting planes direction");
			gp.SetBasePoint(planeDirectionStartPoint, false);
			gp.DrawLineFromPoint(planeDirectionStartPoint, true);
			gp.Get();
			if (gp.CommandResult() != Result.Success)
				return gp.CommandResult();

			Point3d planeDirectionEndPoint = gp.Point();

			Vector3d v = planeDirectionEndPoint - planeDirectionStartPoint;
			v.Z = 0;
			v.Rotate(Math.PI / 2, Vector3d.ZAxis);
			planeNormalVector = v;

			return Result.Success;
		}
		/// <summary>
		/// Pick points on the screen to define the planes origin.
		/// </summary>
		/// <param name="originPoints">List of points.</param>
		/// <returns>A command result code.</returns>
		private Result GetPlanesOriginPoints(out List<Point3d> originPoints)
        {
			originPoints = new List<Point3d> ();
			GetPoint gp = new GetPoint();
			gp.AcceptNothing(true);

			while (true)
            {
				gp.ClearCommandOptions();
				gp.SetCommandPrompt(string.Format("Select points to split the curve [{0} point(s) added]. Press \"ENTER\" to finish", originPoints.Count));

				GetResult res = gp.Get();

				if (res != GetResult.Point && res != GetResult.Cancel)
                {
					return Result.Success;
                }

				if (gp.CommandResult() != Result.Success)
                {
					return gp.CommandResult();
				}

				originPoints.Add(gp.Point());
			}

        }
		/// <summary>
		/// Create a list of parallel planes based on a normal vector and a list of origin points.
		/// </summary>
		/// <param name="normalVector">Planes normal vector.</param>
		/// <param name="originPoints">List of points defining the planes origin.</param>
		/// <returns>A command result code.</returns>
		private static List<Plane> GetSplitPlanes(Vector3d normalVector, List<Point3d> originPoints)
        {
			return originPoints.Select(p => new Plane(p, normalVector)).ToList();
        }

	}
}
