using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace HelloSpiral
{
    public class SpiralComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SpiralComponent()
          : base("Spiral", "Spi",
              "Create a spiral curve in z direction.",
              "HelloSpiral", "Primitive")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
            pManager.AddPlaneParameter("Plane", "P", "Base plane for spiral", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Inner Radius", "R0", "Inner radius for spiral", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Outer Radius", "R1", "Outer radius for spiral", GH_ParamAccess.item, 5.0);
            pManager.AddIntegerParameter("Turns", "T", "Number of turns between radii", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Height", "H", "Height of the spiral", GH_ParamAccess.item, 20.0);
            pManager.AddIntegerParameter("Waves", "W", "Number of waves on the side of spiral", GH_ParamAccess.item, 2);

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);

            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            Plane plane = Plane.WorldXY;
            double radius0 = 0.0;
            double radius1 = 0.0;
            int turns = 0;
            double height = 0.0;
            int waves = 0;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref radius0)) return;
            if (!DA.GetData(2, ref radius1)) return;
            if (!DA.GetData(3, ref turns)) return;
            if (!DA.GetData(4, ref height)) return;
            if (!DA.GetData(5, ref waves)) return;


            // We should now validate the data and warn the user if invalid data is supplied.
            if (radius0 < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Inner radius must be bigger than or equal to zero");
                return;
            }
            if (radius1 < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Outer radius must be bigger than the inner radius");
                return;
            }
            if (turns <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Spiral turn count must be bigger than or equal to one");
                return;
            }
            if (waves < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Waves must be more than zero");
                return;
            }

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:
            Curve spiral = CreateSpiral(plane, radius0, radius1, turns, height, waves);

            // Finally assign the spiral to the output parameter.
            DA.SetData(0, spiral);
        }

        private Curve CreateSpiral(Plane plane, double r0, double r1, Int32 turns, double height, Int32 waves)
        {
            Line line = new Line(plane.Origin, plane.Origin + plane.ZAxis * height);

            Point3d[] pts;
            line.ToNurbsCurve().DivideByCount(turns * 20, true, out pts);

            for(int i=0; i<pts.Length; i++)
            {
                double angle = i / Convert.ToDouble(pts.Length - 1) * Math.PI * 2.0 * turns;
                double radius = r0 + 0.5 * (r1 - r0) * (1 + Math.Cos(i / Convert.ToDouble(pts.Length - 1) * Math.PI * waves));
                Point3d pt = pts[i];
                Point point = new Point(pt);
                point.Translate(plane.XAxis * radius);
                point.Rotate(angle, plane.ZAxis, plane.Origin);

                pts[i] = point.Location;
            }

            Curve spiral = Curve.CreateInterpolatedCurve(pts, 3);
            

            return spiral;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return HelloSpiral.Properties.Resources.Icon1.ToBitmap();
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("159297ee-6747-4f6d-aa87-92b24a72520d"); }
        }
    }
}
