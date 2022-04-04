using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SplitCurves.Lib;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SplitCurves.Component
{
	public class SplitCurvesComponent : GH_Component
	{
		/// <summary>
		/// Each implementation of GH_Component must provide a public 
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear, 
		/// Subcategory the panel. If you use non-existing tab or panel names, 
		/// new tabs/panels will automatically be created.
		/// </summary>
		public SplitCurvesComponent()
		  : base("SplitCurves", "/Curve",
			  "Splits Curves",
			  "Geometry", "Curves")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
            pManager.AddCurveParameter("Curve", "C", "Curve to split into loops", GH_ParamAccess.item);
            pManager.AddIntegerParameter("LoopQty", "Q", "Quantity of loops", GH_ParamAccess.item);

		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
            pManager.AddCurveParameter("Loops", "L", "Loops split from a curve", GH_ParamAccess.list);
		}

        /// <summary>
        /// Handle wrapped Grasshopper objects
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private object UnwrapIfNeeded(object o)
        {
            var wrap = o as GH_ObjectWrapper;
            if (wrap != null)
            {
                return wrap.Value;
            }
            else
            {
                return o;
            }
        }

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
		/// to store data in output parameters.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
            object curve = null;
            int loop_count = 2;
			
            if (!DA.GetData(0, ref curve)) return;
            DA.GetData(1, ref loop_count);

            curve = UnwrapIfNeeded(curve);
            GH_Curve ghCurve = curve as GH_Curve;
            Curve rhCurve = ghCurve.Value;
			
			List<Plane> planes = Curves.CreateDivisionPlanes(rhCurve, loop_count);

            List<Curve> loops = Curves.DivideCurve(rhCurve, planes);

            List<GH_Curve> ghLoops = new List<GH_Curve>();
            foreach (Curve loop in loops)
            {
                ghLoops.Add(new GH_Curve(loop));
            }

            // Return output
            DA.SetDataList(0, ghLoops);
        }

		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => null;

		/// <summary>
		/// Each component must have a unique Guid to identify it. 
		/// It is vital this Guid doesn't change otherwise old ghx files 
		/// that use the old ID will partially fail during loading.
		/// </summary>
		public override Guid ComponentGuid => new Guid("e7e354c2-012c-4849-a641-9babbe769d67");

	}
}
