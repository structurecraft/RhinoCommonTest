using System;
using System.Linq;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using SplitCurves.Lib;
using SplitCurve.Lib;

namespace SplitCurves.Plugin
{
	public class SplitCurveCommand : Command
	{
		private const double AngleDefault = 180;
		private const int CountDefault = 4;
		private const bool DrawDefault = false;

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
			bool draw = Settings.GetBool("SplitCommandDraw", DrawDefault);

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
                OptionToggle drawOpt = new OptionToggle(draw, "Off", "On");

                var angleIndex = go.AddOptionDouble("Angle", ref angleOpt);
                var countIndex = go.AddOptionInteger("Count", ref countOpt);
                var drawIndex = go.AddOptionToggle("Draw", ref drawOpt);
				var resetIndex = go.AddOption("Reset");

				go.GetMultiple(1, 100);

                if (go.Objects().Length > 0)
                {
					selectedCurves = go.Objects().Select(o => o.Curve()).AsEnumerable();

                    foreach (Curve curve in selectedCurves)
                    {
						IEnumerable<Plane> planes = DividerCreator.FromAzimuth(curve, angle, count);
						IEnumerable<Curve> subCurves = Curves.DivideCurve(curve, planes);

                        foreach (Curve subCurve in subCurves)
                        {
							doc.Objects.AddCurve(subCurve);
                        }
					}

					RhinoApp.WriteLine("Curves divided into {0} sub boundaries.", count);

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
						else if (option.Index == drawIndex)
                        {
							draw = drawOpt.CurrentValue;
                        }
						else if (option.Index == resetIndex)
                        {
							angle = AngleDefault;
							count = CountDefault;
							draw = DrawDefault;
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
                Settings.SetBool("SplitCommandDraw", draw);
            }

            return res;
		}
	}
}
