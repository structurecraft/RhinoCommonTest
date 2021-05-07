using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
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
            pManager.AddCurveParameter("Boundary", "B", "Closed curve describing a boundary", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Planes", "P", "Planes used to split the closed curve", GH_ParamAccess.list);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
            pManager.AddCurveParameter("Curves", "C", "Closed curves boundary created from the splitting", GH_ParamAccess.list);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
		/// to store data in output parameters.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
            Curve boundary = null;
            List<Plane> pls = new List<Plane>();

            if (!DA.GetData<Curve>(0, ref boundary)) return;
            if (!DA.GetDataList<Plane>(1, pls) || pls == null || pls.Count == 0) return;

            RhinoDoc activeDoc = RhinoDoc.ActiveDoc;
            double tolerance = activeDoc.ModelAbsoluteTolerance;

            try
            {
                DA.SetDataList(0, Curves.DivideCurve(boundary, pls, tolerance));
			}
            catch (Exception e)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
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
