using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Display;
using Grasshopper.Kernel.Types;
using System.Drawing;
using HumanUIBaseApp;
using System.Linq;
using MahApps.Metro.Controls;

namespace HumanUI.Components.UI_Main
{
    public class RUIDotConduit : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RUIConduit class.
        /// </summary>
        public RUIDotConduit()
          : base("RUIDotConduit", "RUIDotConduit",
              "Provide display conduit to WpfApp",
              "Human UI", "UI Conduit")
        {
            this.SolutionExpired += RUIConduit_SolutionExpired;
        }

        private void RUIConduit_SolutionExpired(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs e)
        {
            if (WindowInfo.SharedWindow != null) WindowInfo.SharedWindow.refresh = true;
        }

        TextDotConduit MyConduit;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Point", "Point to display on WpfApp", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "DotText", GH_ParamAccess.item,"");
            pManager.AddColourParameter("DotColor", "DotColor", "DotColor", GH_ParamAccess.item, Color.Black);
            pManager.AddColourParameter("TextColor", "TextColor", "TextColor", GH_ParamAccess.item, Color.White);
            pManager.AddBooleanParameter("ForeDraw", "ForeDraw", "Draw Foreground", GH_ParamAccess.item, true);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void BeforeSolveInstance()
        {
            if (WindowInfo.SharedWindow != null)
            {
                WindowInfo.SharedWindow.Invoke(() =>
                {
                    if (MyConduit != null)
                    {
                        MyConduit.Enabled = false;
                        WindowInfo.SharedWindow.rbDisplayConduits.Remove(MyConduit);
                    }
                });
            }
            MyConduit = new  TextDotConduit();
        }

        protected override void AfterSolveInstance()
        {
            if (WindowInfo.SharedWindow != null)
            {
                WindowInfo.SharedWindow.Invoke(() =>
                {
                    if (MyConduit != null)
                    {
                        WindowInfo.SharedWindow.rbDisplayConduits.Add(MyConduit);
                    }
                });
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d p = Point3d.Unset;
            string s = "";
            Color c1 = Color.White;
            Color c2 = Color.White;
            bool fore = true;
            DA.GetData(0,ref p);
            DA.GetData(1, ref s);
            DA.GetData(2, ref c1);
            DA.GetData(3, ref c2);
            DA.GetData(4, ref fore);
            if (fore)
            {
                MyConduit.dArgs.Add((p,s,c1,c2));
            }
            else
            {
                MyConduit.dBArgs.Add((p, s, c1, c2));
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C71D654A-1111-3456-BCDE-9BD247FC0859"); }
        }



        class TextDotConduit : DisplayConduit
        {
            internal TextDotConduit():base()
            {
                dArgs = new List<(Point3d, string, Color, Color)>();
                dBArgs = new List<(Point3d, string, Color, Color)>();

            }

            internal  List<(Point3d,string,Color,Color)> dArgs;
            internal List<(Point3d, string, Color, Color)> dBArgs;

            protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs args)
            {
                args.IncludeBoundingBox(new BoundingBox( dArgs.Select(d=>d.Item1).ToList()));
            }

            protected override void DrawForeground(DrawEventArgs args)
            {
                dArgs.Select(arg => { args.Display.DrawDot(arg.Item1, arg.Item2, arg.Item3,arg.Item4); return 0; }).ToList();
            }

            protected override void PostDrawObjects(Rhino.Display.DrawEventArgs args)
            {
                dBArgs.Select(arg => { args.Display.DrawDot(arg.Item1, arg.Item2, arg.Item3, arg.Item4); return 0; }).ToList();
            }
        }
    }
}