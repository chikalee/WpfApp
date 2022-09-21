using MahApps.Metro.Controls;
using System.Windows;
using happ = HumanUIBaseApp;
using Rhino;
using Rhino.Runtime.InProcess;
using Grasshopper.Plugin;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Rhino.Display;
using System.Collections.Generic;

namespace HumanUIBaseApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Param_Geometry pg;
        Param_OGLShader po;
        RhinoCore _rhinoCore;
         GH_RhinoScriptInterface gh;
        bool refresh;
        DrawMeshConduit conduit;

        public MainWindow()
        {
            InitializeComponent();
        }

         void RunGH()
        {
            gh = RhinoApp.GetPlugInObject("Grasshopper") as Grasshopper.Plugin.GH_RhinoScriptInterface;
            gh.RunHeadless();
        }

        private  void OnLoad(object sender, RoutedEventArgs e)
        {
            // This is a good spot to start Rhino.Inside as we now have a
            // handle to the top level parent window for Rhino
            _rhinoCore = new RhinoCore(new string[] { "-appmode" }, Rhino.Runtime.InProcess.WindowStyle.Hidden, this.CriticalHandle);
            Rhino.RhinoDoc.ActiveDoc.Modified = false;
            RunGH();
            happ.WindowInfo.SharedWindow = this;
            //OpenFile(@"C:\Users\timye\Desktop\unnamed.gh");
        }

        private void CatchParam(GH_Document doc)
        {
            foreach (var obj in doc.Objects)
            {
                var param = obj as IGH_Param;
                if (param != null)
                {
                    if (param.NickName == "RobimGeo") pg = param as Param_Geometry;
                    if (param.NickName == "RobimMat") po = param as Param_OGLShader;
                }
            }
        }

        GH_Document OpenFile(string filePath)
        {
            var io = new GH_DocumentIO();
            if (!io.Open(filePath)) { }
            else
            {
                var doc = io.Document;
                CatchParam(doc);
                // Documents are typically only enabled when they are loaded
                // into the Grasshopper canvas. In this case we -may- want to
                // make sure our document is enabled before using it.
                doc.Enabled = true;
                if (pg != null) pg.SolutionExpired += Pg_SolutionExpired;
                doc.SolutionEnd += Doc_SolutionEnd;
                doc.NewSolution(true);
                 wfHost.Child.Refresh();
                return doc;
            }
            return null;
        }

        private void Pg_SolutionExpired(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs e)
        {
            refresh = true;
        }

        private void Doc_SolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            if (refresh)
            {
                var doc = Rhino.RhinoDoc.ActiveDoc;
                if (conduit != null) conduit.Enabled = false;
                conduit = new DrawMeshConduit();
                if (pg != null && po != null)
                {
                    var geos = pg.VolatileData.AllData(true);
                    var mats = po.VolatileData.AllData(true);

                    foreach (var geo in geos)
                    {
                        switch (geo)
                        {
                            case GH_Mesh msh:
                                conduit.MeshList.Add(msh.Value);
                                break;
                            case GH_Brep brp:
                                Mesh m = new Mesh();
                                m.Append(Mesh.CreateFromBrep(brp.Value, MeshingParameters.Default));
                                conduit.MeshList.Add(m);
                                break;
                        }
                    }
                    foreach (var mat in mats)
                    {
                        DisplayMaterial dm = null;
                        (mat as GH_Material).CastTo<DisplayMaterial>(ref dm);
                        conduit.MatList.Add(dm);
                    }
                }

                conduit.Enabled = true;
                doc.Views.Redraw();
                wfHost.Child.Refresh();

            }
            refresh = false;
        }

        public class DrawMeshConduit : Rhino.Display.DisplayConduit
        {
            public DrawMeshConduit() : base()
            {
                MeshList = new List<Mesh>();
                MatList = new List<DisplayMaterial>();
            }
            public List<Mesh> MeshList { get; set; }
            public List<DisplayMaterial> MatList { get; set; }

            protected override void CalculateBoundingBox(Rhino.Display.CalculateBoundingBoxEventArgs e)
            {
                if (MeshList.Count > 0)
                {
                    foreach (var msh in MeshList)
                        e.IncludeBoundingBox(msh.GetBoundingBox(false));
                }
            }

            protected override void PostDrawObjects(Rhino.Display.DrawEventArgs e)
            {
                int n = MeshList.Count;
                if (n > 0)
                {
                    //Rhino.Display.DisplayMaterial material = new Rhino.Display.DisplayMaterial();
                    //material.IsTwoSided = true;
                    //material.Diffuse = System.Drawing.Color.Blue;
                    //material.BackDiffuse = System.Drawing.Color.Red;
                    //e.Display.EnableLighting(true);
                    for (int i = 0; i < n; i++)
                    {
                        e.Display.DrawMeshShaded(MeshList[i], MatList[i]);
                        //e.Display.DrawMeshWires(Mesh, System.Drawing.Color.Black);
                    }
                }
            }
        }

        private void ElementContainer_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            OpenFile(@"C:\Users\timye\Desktop\unnamed.gh");
        }
    }
}
