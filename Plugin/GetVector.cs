using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;

namespace SplitCurves.Plugin
{
    /// <summary>
    /// Class used to retrieve a vector direction.
    /// </summary>
    internal class GetVector : GetPoint
    {
        internal Point3d basePt = Point3d.Origin;
        internal Line vectorLine;

        public GetVector(string prompt)
        {
            this.SetCommandPrompt(prompt);
            this.AcceptNothing(true);
            this.SetBasePoint(this.basePt, true);
            this.MouseMove += new EventHandler<GetPointMouseEventArgs>(this.LocalMouseMove);
            this.DynamicDraw += new EventHandler<GetPointDrawEventArgs>(this.LocalDynamicDraw);
        }

        private void LocalMouseMove(object sender, GetPointMouseEventArgs e) => this.vectorLine = new Line(this.basePt, e.Point);

        private void LocalDynamicDraw(object sender, GetPointDrawEventArgs e)
        {
            PointStyle previewPointStyle = PointStyle.Circle;
            e.Display.DrawPoint(this.basePt, previewPointStyle, 5, System.Drawing.Color.Green);
            if (!this.vectorLine.IsValid)
                return;
            e.Display.DrawArrow(this.vectorLine, System.Drawing.Color.Green);
        }
    }
}
