using System;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using Rhino.Display;

namespace HumanUIBaseApp
{
    public partial class MainForm : Form
    {
        Rhino.Runtime.InProcess.RhinoCore _rhinoCore;
        GH_Document ghdoc;
        DrawMeshConduit conduit;
        Param_Geometry pg;
        Param_OGLShader po;
        bool refresh;
       public static MainForm SharedWindow;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            // This is a good spot to start Rhino.Inside as we now have a
            // handle to the top level parent window for Rhino
            _rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "-appmode" }, Rhino.Runtime.InProcess.WindowStyle.Hidden, Handle);
            base.OnHandleCreated(e);
        }

        static Grasshopper.Plugin.GH_RhinoScriptInterface RunGH()
        {
            var pluginObject = Rhino.RhinoApp.GetPlugInObject("Grasshopper") as Grasshopper.Plugin.GH_RhinoScriptInterface;
            pluginObject.RunHeadless();
            return pluginObject;
        }

        Grasshopper.Kernel.GH_Document OpenFile(string filePath)
        {
            Console.WriteLine("open gh file");

            var io = new Grasshopper.Kernel.GH_DocumentIO();
            if (!io.Open(filePath)) { }
            else
            {
                var doc = io.Document;
                CatchParam(doc);
                // Documents are typically only enabled when they are loaded
                // into the Grasshopper canvas. In this case we -may- want to
                // make sure our document is enabled before using it.
                doc.Enabled = true;
                if (pg!= null) pg.SolutionExpired += Pg_SolutionExpired;
                doc.SolutionEnd += Doc_SolutionEnd;
                doc.NewSolution(true);
                viewportControl1.Refresh();
                Console.WriteLine("file ok");
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
                viewportControl1.Refresh();
            }
            refresh = false;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _rhinoCore.Dispose();
            _rhinoCore = null;
            base.OnHandleDestroyed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var displayModes = Rhino.Display.DisplayModeDescription.GetDisplayModes();
            foreach (var mode in displayModes)
            {
                var item = viewToolStripMenuItem.DropDownItems.Add(mode.EnglishName);
                item.Click += (s, evt) =>
                {
                    viewportControl1.Viewport.DisplayMode = mode;
                    viewportControl1.Invalidate();
                };
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "GH file (*.gh) | *.gh";
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                // Make sure Rhino doesn't attempt to show a "save modified" UI
                Rhino.RhinoDoc.ActiveDoc.Modified = false;
                UseWaitCursor = true;
                var gh = RunGH();
                ghdoc = OpenFile(ofd.FileName);
                //viewportControl1.Viewport.ZoomExtents();
                viewportControl1.Refresh();
                UseWaitCursor = false;

            }
        }

        public class DrawMeshConduit : Rhino.Display.DisplayConduit
        {
          public DrawMeshConduit():base()
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
                    foreach(var msh in MeshList)
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

        private void OnViewportMouseDown(object sender, MouseEventArgs e)
        {
            var doc = Rhino.RhinoDoc.ActiveDoc;
            using (var pick = new Rhino.Input.Custom.PickContext())
            {
                bool subObjects = ModifierKeys.HasFlag(Keys.Control);
                if (subObjects)
                    pick.SubObjectSelectionEnabled = true;
                var pickTransform = viewportControl1.Viewport.GetPickTransform(e.Location);
                pick.SetPickTransform(pickTransform);
                pick.PickStyle = Rhino.Input.Custom.PickStyle.PointPick;
                var objects = doc.Objects.PickObjects(pick);
                doc.Objects.UnselectAll();

                if (subObjects)
                {
                    foreach (var obj in objects)
                    {
                        obj.Object().SelectSubObject(obj.GeometryComponentIndex, true, true);
                    }
                }
                else
                {
                    doc.Objects.Select(objects);
                }
                viewportControl1.Refresh();
            }
        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void elementHost1_ChildChanged_1(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void viewportControl1_Click(object sender, EventArgs e)
        {

        }
    }
}
