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
    public class RUICurveConduit : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RUIConduit class.
        /// </summary>
        public RUICurveConduit()
          : base("RUICurveConduit", "RUICurveConduit",
              "Provide display conduit to WpfApp",
              "Human UI", "UI Conduit")
        {
            this.SolutionExpired += RUIConduit_SolutionExpired;
        }

        private void RUIConduit_SolutionExpired(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs e)
        {
            if (WindowInfo.SharedWindow != null) WindowInfo.SharedWindow.refresh = true;
        }

        CurveConduit MyConduit;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Curve", "Curve to display on WpfApp", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "Color", "Curve Color", GH_ParamAccess.item,Color.Black);
            pManager.AddIntegerParameter("Thickness", "Thickness", "Curve Thickness", GH_ParamAccess.item, 3);
            pManager.AddBooleanParameter("ForeDraw", "ForeDraw", "Draw Foreground", GH_ParamAccess.item, false);
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
            MyConduit = new CurveConduit();
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
            Curve m = null;
            Color mt = Color.White;
            int thick = 0;
            bool fore = false;
            DA.GetData(0,ref m);
            DA.GetData(1, ref mt);
            DA.GetData(2, ref thick);
            DA.GetData(3, ref fore);
            if (m != null && mt!=null)
            {
                if (fore)
                {
                    MyConduit.fArgs.Add((m, mt, thick));
                }
                else
                {
                    MyConduit.bArgs.Add((m, mt, thick));
                }
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
            get { return new Guid("C71D654A-A5BB-3456-BCDE-9BD247FC0859"); }
        }



        class CurveConduit : DisplayConduit
        {
            internal CurveConduit():base()
            {
                fArgs = new List<(Curve, Color, int)>();
                bArgs = new List<(Curve, Color, int)>();

            }

            internal  List<(Curve,Color,int)> fArgs;
            internal List<(Curve, Color, int)> bArgs;

            protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs args)
            {
                foreach(var c in fArgs)
                args.IncludeBoundingBox(c.Item1.GetBoundingBox(false));
                foreach (var c in bArgs)
                   args.IncludeBoundingBox(c.Item1.GetBoundingBox(false));
            }

            protected override void DrawForeground(DrawEventArgs args)
            {
               fArgs.Select(arg => { args.Display.DrawCurve(arg.Item1, arg.Item2, arg.Item3); return 0; }).ToList();
            }

            protected override void PostDrawObjects(Rhino.Display.DrawEventArgs args)
            {
                bArgs.Select(arg => { args.Display.DrawCurve(arg.Item1, arg.Item2, arg.Item3); return 0; }).ToList();
            }
        }
    }
}