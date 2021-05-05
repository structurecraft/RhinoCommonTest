using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SplitCurvesWithPlanesComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SplitCurvesWithPlanesComponent()
          : base("SplitCurvesWithPlanes", 
                 "/Curve",
                 "Splits Curves",
                 "Geometry", 
                 "Curves")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundaries", "boundaries", "Closed curves to sub divide.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planes", "planes", "Planes to slice given boundaries.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "curves", "Inner curves that created from the splitting action.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> boundaries = new List<Curve>();
            List<Plane> planes = new List<Plane>();

            if (!DA.GetDataList<Curve>(0, boundaries) || boundaries.Count == 0) return;
            if (!DA.GetDataList<Plane>(1, planes)) return;

            DataTree<Curve> dataTree = new DataTree<Curve>();
            int pathIndex = 0;
            foreach (Curve boundary in boundaries)
            {
                IEnumerable<Curve> subCurves = Curves.DivideCurve(boundary, planes);

                dataTree.AddRange(subCurves, new Grasshopper.Kernel.Data.GH_Path(pathIndex));
                pathIndex++;
            }

            DA.SetDataTree(0, dataTree);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.splitWithPlanes;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("80F9D0A3-5005-422A-A3AF-E7DBF690081E");

    }
}
