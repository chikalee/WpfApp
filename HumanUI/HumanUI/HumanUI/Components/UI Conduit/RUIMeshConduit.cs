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
    public class RUIMeshConduit : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RUIConduit class.
        /// </summary>
        public RUIMeshConduit()
          : base("RUIMeshConduit", "RUIMeshConduit",
              "Provide display conduit to WpfApp",
              "Human UI", "UI Conduit")
        {
            this.SolutionExpired += RUIConduit_SolutionExpired;
        }

        private void RUIConduit_SolutionExpired(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs e)
        {
            if (WindowInfo.SharedWindow != null) WindowInfo.SharedWindow.refresh = true;
        }

        MeshConduit MyConduit;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to display on WpfApp", GH_ParamAccess.item);
            var p = new Param_OGLShader();
            p.SetPersistentData(new GH_Material(Color.Plum));
            pManager.AddParameter(p, "Material", "M", "The material override", GH_ParamAccess.item);
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
                //lock (MainWindow1._Sync)
                //{
                //    if (MyConduit != null)
                //    {
                //        MyConduit.Enabled = false;
                //        WindowInfo.SharedWindow.rbDisplayConduits.Remove(MyConduit);
                //    }
                //}
                WindowInfo.SharedWindow.Invoke(() =>
                {
                    if (MyConduit != null)
                    {
                        MyConduit.Enabled = false;
                        WindowInfo.SharedWindow.rbDisplayConduits.Remove(MyConduit);
                    }
                });

            }
            MyConduit = new MeshConduit();
        }

        protected override void AfterSolveInstance()
        {
            if (WindowInfo.SharedWindow != null)
            {
                //lock (MainWindow1._Sync)
                //{
                //    if (MyConduit != null)
                //    {
                //        WindowInfo.SharedWindow.rbDisplayConduits.Add(MyConduit);
                //    }
                //}
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
            Mesh m = null;
            DisplayMaterial mt = null;
            DA.GetData(0, ref m);
            DA.GetData(1, ref mt);
            if (m != null && mt != null)
            {
                MyConduit.mshs.Add(m);
                MyConduit.materials.Add(mt);
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
            get { return new Guid("C71D654A-A5BB-4569-BCDE-9BD247FC0859"); }
        }



        class MeshConduit : DisplayConduit
        {
            internal MeshConduit() : base()
            {
                mshs = new List<Mesh>();
                materials = new List<DisplayMaterial>();
            }

            internal List<Mesh> mshs;
            internal List<DisplayMaterial> materials;
            protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs args)
            {
                foreach (var m in mshs)
                    args.IncludeBoundingBox(m.GetBoundingBox(false));
            }

            protected override void DrawForeground(DrawEventArgs args)
            {
            }

            protected override void PostDrawObjects(Rhino.Display.DrawEventArgs args)
            {

                mshs.Zip(materials, (mh, mt) => { args.Display.DrawMeshShaded(mh, mt); return 0; }).ToList();
            }
        }
    }
}