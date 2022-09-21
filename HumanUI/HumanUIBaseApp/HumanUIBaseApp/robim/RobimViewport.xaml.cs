using System;
using System.Windows;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using RobotIcPlus.RhinoModule.Type;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Grasshopper.Kernel.Types;
using System.Diagnostics;

namespace RobotIcPlus.RhinoModule
{
    /// <summary>
    /// RobimViewport.xaml 的交互逻辑
    /// </summary>
    public partial class RobimViewport : UserControl
    {
        public RobimViewportControl RobimViewportControl { get { return robimViewportControl; } }
        public RobimDoc MyDoc { get; }
        internal GHSelector MyGHSeletor { get; }
        internal GHDisplayer MyGHDisplayer { get; }
        internal RhinoViewport ShadowViewport { get; private set; }
        private IntPtr handle;
        private IntPtr wrapHandle;
        public RobimViewport()
        {
            InitializeComponent();
            handle = Process.GetCurrentProcess().MainWindowHandle;
            MyDoc = new RobimDoc(this);
            MyGHSeletor = new GHSelector(this);
            MyGHDisplayer = new GHDisplayer(this);
        }

        //private void OnInitialized(object sender, EventArgs e)
        //{
        //    handle = Process.GetCurrentProcess().MainWindowHandle;
        //    GHRunner.Initialize(handle);
        //}

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //RhinoDoc.Open(@"C:\Users\timye\Desktop\RobimRhinoInsideApp\未命名.3dm", out bool open);
            RobimViewportControl.Viewport.DisplayMode = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.ShadedId);
            RobimViewportControl.Viewport.ChangeToPerspectiveProjection(true, 60);
            RobimViewportControl.owner = this;
            RobimViewportControl.SizeChanged += RobimViewportControl_SizeChanged;
            //LayoutUpdated += RobimViewport_LayoutUpdated;
            AddASameView();
            //foreach (var obj in RhinoDoc.ActiveDoc.Objects)
            //{
            //    if (obj is BrepObject brp) MyDoc.Objects.Add(new SteelPlate(brp.BrepGeometry));
            //    else if (obj is CurveObject crv) MyDoc.Objects.Add(new SteelWeld(crv.CurveGeometry));
            //}
            //RhinoDoc.ActiveDoc.Objects.Clear();
            RobimViewportControl.Invalidate();
            RobimViewportControl.Refresh();


        }

        private void RobimViewport_LayoutUpdated(object sender, EventArgs e)
        {
            var relativePoint = TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            SetWindowPos(wrapHandle, IntPtr.Zero, (int)relativePoint.X + 5, (int)relativePoint.Y + 5, 100, 100, 0x0040);

        }

        private void RobimViewportControl_SizeChanged(object sender, EventArgs e)
        {
            CopyViewport();
        }

        private void OnUnload(object sender, EventArgs e)
        {
            GHRunner.Release();
        }

        private void AddASameView()
        {
            var vw = RhinoDoc.ActiveDoc.Views.Add("matchView", DefinedViewportProjection.Perspective, new System.Drawing.Rectangle(0, 0, 100, 100), true);
            var vwp = vw.MainViewport;
            ShadowViewport = vwp;
            CopyViewport();
            wrapHandle = FindWindow(null, "Rhino 工作视窗");
            //SetWindowLong(handle, -8, wrapHandle);
            SetWindowLong(wrapHandle, GWL_EXSTYLE, GetWindowLong(wrapHandle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
            SetLayeredWindowAttributes(wrapHandle, 0, 0, LWA_ALPHA);
            var relativePoint = TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            SetWindowPos(wrapHandle, IntPtr.Zero, (int)relativePoint.X + 5, (int)relativePoint.Y + 5, 100, 100, 0x0040);
        }

        private void CopyViewport()
        {
            if (ShadowViewport != null)
            {
                var vp = RobimViewportControl.Viewport;
                var vwp = ShadowViewport;
                var bb = RobimViewportControl.Bounds;
                SetWindowPos(vwp.ParentView.Handle, IntPtr.Zero, 0, 0, bb.Width, bb.Height, 0x0040);
                vwp.ChangeToPerspectiveProjection(true, vp.Camera35mmLensLength);
                vwp.SetCameraLocation(vp.CameraLocation, false);
                vwp.SetCameraTarget(vp.CameraTarget, false);
                vwp.ParentView.Redraw();
            }
        }


        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndlnsertAfter, int X, int Y, int cx, int cy, uint Flags);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);


        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;
    }
}
