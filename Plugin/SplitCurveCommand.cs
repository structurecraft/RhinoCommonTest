using System;
using System.Collections.Generic;
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
            var selectedBoundary = RhinoGet.GetOneObject(
                "Select one closed curve", false, ObjectType.Curve, out ObjRef boundary);
            if (selectedBoundary != Result.Success)
                return selectedBoundary;

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

            var getPlaneNormal = new VectorGetter("End point of vector");
            getPlaneNormal.Get();
            if (getPlaneNormal.CommandResult() != Result.Success)
                return getPlaneNormal.CommandResult();

            Vector3d normal = getPlaneNormal.Point() - getPlaneNormal.m_base;

            List<Plane> planes = new List<Plane>();
            foreach (Point3d pt in pts)
            {
                planes.Add(new Plane(pt, normal));
            }

            var crv = Curves.DivideCurve(boundary.Curve(), planes, doc.ModelAbsoluteTolerance);

            foreach (var curve in crv)
            {
                doc.Objects.AddCurve(curve);
            }
            //doc.Views.Redraw();
            return Result.Success;
        }

        internal class VectorGetter : GetPoint
        {
            internal Point3d m_base = Point3d.Origin;
            internal Line m_line;

            public VectorGetter(string prompt)
            {
                this.SetCommandPrompt(prompt);
                this.AcceptNothing(true);
                this.SetBasePoint(this.m_base, true);
                this.MouseMove += new EventHandler<GetPointMouseEventArgs>(this.LocalMouseMove);
                this.DynamicDraw += new EventHandler<GetPointDrawEventArgs>(this.LocalDynamicDraw);
            }

            private void LocalMouseMove(object sender, GetPointMouseEventArgs e) => this.m_line = new Line(this.m_base, e.Point);

            private void LocalDynamicDraw(object sender, GetPointDrawEventArgs e)
            {
                PointStyle previewPointStyle = PointStyle.Circle;
                e.Display.DrawPoint(this.m_base, previewPointStyle, 10, System.Drawing.Color.DarkBlue);
                if (!this.m_line.IsValid)
                    return;
                e.Display.DrawArrow(this.m_line, System.Drawing.Color.DarkBlue);
            }
        }
    }
}
