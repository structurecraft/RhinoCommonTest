﻿using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
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
			pManager.AddCurveParameter("Curves", "Curves", "Closed curves to split.", GH_ParamAccess.list);
			pManager.AddPlaneParameter("Planes", "Planes", "Planes to split the curve(s).", GH_ParamAccess.list);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddCurveParameter("Suburves", "Curves", "Curves created from split original curves.", GH_ParamAccess.tree);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
		/// to store data in output parameters.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			List<Curve> entryCurves = new List<Curve>();
			List<Plane> entryPlanes = new List<Plane>();
			if (!DA.GetDataList(0, entryCurves) || !DA.GetDataList(1, entryPlanes))
			{
				return;
			}
			if (entryCurves.Count == 0 || entryPlanes.Count == 0)
			{
				return;
			}

			DataTree<Curve> outCurves = new DataTree<Curve>();
			int path = 0;
			foreach (Curve curve in entryCurves)
			{
				var gp = new Grasshopper.Kernel.Data.GH_Path(path);
				try
				{
					List<Curve> subCurves = Curves.DivideCurve(curve, entryPlanes);
					// var gp = new Grasshopper.Kernel.Data.GH_Path(path);
					outCurves.AddRange(subCurves, gp);
				}
				catch (ArgumentException)
				{
					outCurves.AddRange(new List<Curve> (), gp); // Keeping the path index
				}
				path += 1;
			}
			DA.SetDataTree(0, outCurves);
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
