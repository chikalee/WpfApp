﻿using System;
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
    public class RUIDotPolylineConduit : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RUIConduit class.
        /// </summary>
        public RUIDotPolylineConduit()
          : base("RUIDotPolylineConduit", "RUIDotPolylineConduit",
              "Provide display conduit to WpfApp",
              "Human UI", "UI Conduit")
        {
            this.SolutionExpired += RUIConduit_SolutionExpired;
        }

        private void RUIConduit_SolutionExpired(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs e)
        {
            if (WindowInfo.SharedWindow != null) WindowInfo.SharedWindow.refresh = true;
        }

        DotPolylineConduit MyConduit;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "Polyline", "Polyline to display on WpfApp", GH_ParamAccess.item);
            pManager.AddColourParameter("Color", "Color", "Polyline Color", GH_ParamAccess.item,Color.Black);
            pManager.AddIntegerParameter("Pattern", "Pattern", "Polyline Pattern", GH_ParamAccess.item, 0x00001111);
            pManager.AddIntegerParameter("Thickness", "Thickness", "Polyline Thickness", GH_ParamAccess.item, 3);
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
            MyConduit = new DotPolylineConduit();
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
            int pattern = 0x00001111;
            DA.GetData(0,ref m);
            DA.GetData(1, ref mt);
            DA.GetData(2, ref pattern);
            DA.GetData(3, ref thick);
            if (m != null && mt != null) {
                if (m.TryGetPolyline(out Polyline pl))
                {
                    MyConduit.fArgs.Add((pl,mt, pattern,thick));
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not a valid polyline!");
                    return;
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
            get { return new Guid("C712354A-A5BB-3456-BCDE-9BD247FC0859"); }
        }



        class DotPolylineConduit : DisplayConduit
        {
            internal DotPolylineConduit():base()
            {
                fArgs = new List<(Polyline, Color, int, int)>();

            }

            internal  List<(Polyline,Color,int,int)> fArgs;

            protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs args)
            {
                foreach (var c in fArgs)
                   args.IncludeBoundingBox(new BoundingBox(c.Item1.ToArray()));
            }

            protected override void DrawForeground(DrawEventArgs args)
            {
            }

            protected override void PostDrawObjects(Rhino.Display.DrawEventArgs args)
            {
                fArgs.Select(arg => { args.Display.DrawPatternedPolyline(arg.Item1.ToArray(), arg.Item2, arg.Item3,arg.Item4,arg.Item1.IsClosed); return 0; }).ToList();
            }
        }
    }
}