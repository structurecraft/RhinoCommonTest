using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SplitCurve.Lib;
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
            pManager.AddCurveParameter("Boundaries", "boundaries", "Closed curves to sub divide.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Angle", "angle", "Azimuth angle to slice given boundaries.", GH_ParamAccess.item, 180);
            pManager.AddIntegerParameter("Count", "count", "Count of division area.", GH_ParamAccess.item, 4);
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
            double angle = 180;
            int count = 4;
            if (!DA.GetDataList<Curve>(0, boundaries) || boundaries.Count == 0) return;
            if (!DA.GetData<double>(1, ref angle)) return;
            if (!DA.GetData<int>(2, ref count)) return;

            DataTree<Curve> dataTree = new DataTree<Curve>();
            int pathIndex = 0;
            foreach (Curve boundary in boundaries)
            {
                IEnumerable<Plane> cutters = DividerCreator.FromAzimuth(boundary, angle, count);
                IEnumerable<Curve> subCurves = Curves.DivideCurve(boundary, cutters);

                dataTree.AddRange(subCurves, new Grasshopper.Kernel.Data.GH_Path(pathIndex));
                pathIndex++;
            }

            DA.SetDataTree(0, dataTree);
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
