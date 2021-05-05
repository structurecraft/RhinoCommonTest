using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SplitCurves;

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
		  : base("SplitCurvesWithPlanes", "SplitCurvesWithPlanes",
			  "Splits input curves with parallel planes and outputs closed planar loops",
			  "Geometry", "Curves")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			/// input Curves to Split
			pManager.AddCurveParameter("EvaluationCurve", "Eval_Crv", "Curve to Split", GH_ParamAccess.list);

			
			/// input Planes used for Spliting Curve
			pManager.AddPlaneParameter("SplitingPlane", "Split_Pln", "Plane used for spliting", GH_ParamAccess.list);
		
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			/// output Closed Loop Curves
			pManager.AddCurveParameter("ClosedLoopCurves", "Loop_Crv", "Closed Loop Curve derived form Input curve and Spliting Plane", GH_ParamAccess.list);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
		/// to store data in output parameters.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			List<Curve> Eval_Crv = new List<Curve>();
			List<Plane> Split_Pln = new List<Plane>();
			
			// Then we need to access the input parameters individually. 
			// When data cannot be extracted from a parameter, we should abort this method.
			if (!DA.GetDataList(0, Eval_Crv)) return;
			if (!DA.GetDataList(1, Split_Pln)) return;

			if (Eval_Crv.Count > 1)
			{
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please Join The Curve");
				return;
			}
			else
            {
				bool planar_test = Eval_Crv[0].IsPlanar();


				if (planar_test == true)
                {
					List<Curve> Loop_Crv = SplitCurves.Lib.Curves.DivideCurve(Eval_Crv[0], Split_Pln);
					DA.SetDataList(0, Loop_Crv);
				}
				else
                {
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The Curve Should be Planar");
					return;

				}
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
