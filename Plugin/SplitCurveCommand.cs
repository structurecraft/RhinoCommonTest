using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SplitCurves.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SplitCurves.Plugin
{
    public class SplitCurveCommand : Command
    {
        private const double AngleDefault = 180;
        private const int CountDefault = 4;
        private const bool SlicerDefault = false;

        private Point3d SlicerStartPoint { get; set; } = Point3d.Origin;

        private Line Slicer { get; set; }

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
            // Get persistent settings from Registry if set previously
            double angle = Settings.GetDouble("SplitCommandAngle", AngleDefault);
            int count = Settings.GetInteger("SplitCommandCount2", CountDefault);
            bool slicer = Settings.GetBool("SplitCommandSlicer", SlicerDefault);

            var go = new GetObject();
            go.SetCommandPrompt("Select curve(s) to split");
            go.GeometryFilter = ObjectType.Curve;

            IEnumerable<Curve> selectedCurves = new List<Curve>();

            var res = Result.Cancel;

            while (true)
            {
                go.ClearCommandOptions();

                OptionDouble angleOpt = new OptionDouble(angle);
                OptionInteger countOpt = new OptionInteger(count);
                OptionToggle slicerOpt = new OptionToggle(slicer, "Off", "On");

                var angleIndex = go.AddOptionDouble("Angle", ref angleOpt);
                var countIndex = go.AddOptionInteger("Count", ref countOpt);
                var slicerIndex = go.AddOptionToggle("Draw", ref slicerOpt);
                var resetIndex = go.AddOption("Reset");

                go.GetMultiple(1, 100);

                // If draw enabled, user draw line to grab an azimuth angle from it.
                if (slicer)
                {
                    var getPoint = new GetPoint();
                    List<Point3d> startEndPoints = new List<Point3d>();

                    bool startPointSelected = false;
                    bool endPointSelected = false;

                    while (true)
                    {
                        if (!startPointSelected)
                        {
                            getPoint.SetCommandPrompt("Click starting point of slicer.");
                            getPoint.AcceptNothing(true);
                            getPoint.AcceptPoint(true);
                            getPoint.Get();
                            
                            if (getPoint.CommandResult() != Result.Success)
                                break;
                            
                            startEndPoints.Add(getPoint.Point());
                            this.SlicerStartPoint = getPoint.Point();

                            startPointSelected = true;

                            // Subscribe interactive events.
                            getPoint.MouseMove += new EventHandler<GetPointMouseEventArgs>(this.OnMouseMove);
                            getPoint.DynamicDraw += new EventHandler<GetPointDrawEventArgs>(this.OnDynamicDraw);
                        }
                        else if (startPointSelected && !endPointSelected)
                        {
                            getPoint.SetCommandPrompt("Click end point of slicer.");
                            getPoint.AcceptNothing(true);
                            getPoint.AcceptPoint(true);
                            getPoint.Get();
                            
                            if (getPoint.CommandResult() != Result.Success)
                                break;
                            
                            startEndPoints.Add(getPoint.Point());
                            Slicer = new Line(startEndPoints.First(), startEndPoints.Last());
                            endPointSelected = true;
                        }
                        else if (startPointSelected && endPointSelected)
                        {
                            // Ready to process
                            angle = CalculateAzimuthAngle(startEndPoints.First(), startEndPoints.Last());

                            // Command should be success if azimuth calculated.
                            res = Result.Success;
                            break;
                        }
                    }
                }

                if (go.Objects().Length > 0)
                {
                    // Collect selected curves.
                    selectedCurves = go.Objects().Select(o => o.Curve()).AsEnumerable();

                    // Validate selected curves.
                    IEnumerable<Curve> validatedCurves = Curves.ValidateForSplitting(selectedCurves);

                    // Create sub boundaries.
                    foreach (Curve curve in validatedCurves)
                    {
                        IEnumerable<Plane> planes = DividerCreator.FromAzimuth(curve, angle, count);
                        IEnumerable<Curve> subCurves = Curves.DivideCurve(curve, planes);

                        foreach (Curve subCurve in subCurves)
                        {
                            doc.Objects.AddCurve(subCurve);
                        }
                    }

                    RhinoApp.WriteLine("Curves divided into {0} sub boundaries with {1} azimuth angle.", count, angle);

                    doc.Views.Redraw();
                    res = Result.Success;
                }
                else if (go.Result() == GetResult.Option)
                {
                    var option = go.Option();
                    if (option != null)
                    {
                        if (option.Index == angleIndex)
                        {
                            angle = angleOpt.CurrentValue;
                        }
                        else if (option.Index == countIndex)
                        {
                            count = countOpt.CurrentValue;
                        }
                        else if (option.Index == slicerIndex)
                        {
                            slicer = slicerOpt.CurrentValue;
                        }
                        else if (option.Index == resetIndex)
                        {
                            angle = AngleDefault;
                            count = CountDefault;
                            slicer = SlicerDefault;
                        }
                    }
                    continue;
                }

                break;
            }

            if (res == Result.Success)
            {
                // Set persistent settings to Registry
                Settings.SetDouble("SplitCommandAngle", angle);
                Settings.SetInteger("SplitCommandCount2", count);
                Settings.SetBool("SplitCommandSlicer", slicer);
            }

            return res;
        }

        private void OnMouseMove(object sender, GetPointMouseEventArgs e)
        {
            this.Slicer = new Line(this.SlicerStartPoint, e.Point);
        }

        private void OnDynamicDraw(object sender, GetPointDrawEventArgs e)
        {
            if (!this.Slicer.IsValid)
                return;
            e.Display.DrawDottedLine(this.Slicer, System.Drawing.Color.Black);
        }

        private double CalculateAzimuthAngle(Point3d start, Point3d end)
        {
            double xDiff = end.X - start.X;
            double yDiff = end.Y - start.Y;
            double azimuthAngle = (360 + Math.Atan2(xDiff, yDiff) * (180 / Math.PI)) % 360;
            return Math.Round(azimuthAngle, 2, MidpointRounding.ToEven);
        }
    }
}
