using Grasshopper.Kernel;
using Grasshopper.Plugin;
using MahApps.Metro.Controls;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Runtime;
using Rhino.Runtime.InProcess;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using happ = HumanUIBaseApp;
using System.Reflection;
using GH_IO.Serialization;
using System.Resources;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.IO;
using RhinoWindows.Forms.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Point = System.Windows.Point;

namespace HumanUIBaseApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow1 : MetroWindow
    {
        readonly public static object _Sync = new object();
        RhinoCore _rhinoCore;
        GH_RhinoScriptInterface gh;
        public bool refresh;
        ViewportControl vp;
        GH_DocumentIO ghIO;
        GH_Document myDoc;
        public static Assembly assembly;
        public List<DisplayConduit> rbDisplayConduits = new List<DisplayConduit>();
        private int _expireInterval;
        private bool _expireToken;
        private bool allowToNewSolution = true;
        public List<Action> expireAction = new List<Action>();
        public bool allowToExpire = true;

        private IntPtr ViewportHandle = IntPtr.Zero;

        public void FuckRhino(IntPtr viewportHandle)
        {
            ViewportHandle = viewportHandle;
            SetChildren();
            SetPos();
        }


        private void SetChildren()
        {
            if (ViewportHandle != IntPtr.Zero)
            {
                Win32API.SetParent(ViewportHandle, new WindowInteropHelper(this).Handle);
                Win32API.SetWindowLong(new HandleRef(this, ViewportHandle), Win32API.GWL_STYLE, Win32API.WS_VISIBLE);
            }
        }

        private void SetPos()
        {
            if (ViewportHandle != IntPtr.Zero)
            {
                var scale = VisualTreeHelper.GetDpi(this);
                var pt = rv.TransformToAncestor(this) .Transform(new Point(0, 0));
                Win32API.MoveWindow(ViewportHandle, (int)(pt.X * scale.DpiScaleX), (int)(pt.Y*scale.DpiScaleY), (int) Math.Ceiling(rv.ActualWidth * scale.DpiScaleX), (int)Math.Ceiling(rv.ActualHeight * scale.DpiScaleY), true);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPos();
        }
        public bool expireToken
        {
            get { return _expireToken; }
            set
            {
                if (_expireToken != value)
                {
                    _expireToken = value;
                    if (_expireToken)
                    {
                        try
                        {
                            WaitThenNewSolution();
                        }
                        catch { }
                    }
                }
            }
        }


        public int expireInterval
        {
            get { return _expireInterval; }
            set
            {
                if (_expireInterval == 0 || (value < _expireInterval && value > 0))
                    _expireInterval = value;
            }
        }

        public static int solutionWaitNum = 0;
        public static int solutionEndNum = 0;

        public async void WaitThenNewSolution()
        {
            try
            {
                Console.WriteLine("SolutionWaiting" + solutionWaitNum.ToString());
                solutionWaitNum++;
                await Task.Run(() =>
                {
                    while (!allowToNewSolution)
                    {
                    };
                }).ConfigureAwait(false);
                _expireToken = false;
                await SolveAsync(10).ConfigureAwait(true);
                Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
                Console.WriteLine("SolutionEnd" + solutionEndNum.ToString());
                solutionEndNum++;
            }
            catch { }
        }


        public MainWindow1()
        {
            InitializeComponent();
        }

        void RunGH()
        {
            gh = RhinoApp.GetPlugInObject("Grasshopper") as GH_RhinoScriptInterface;
            gh.RunHeadless();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // This is a good spot to start Rhino.Inside as we now have a
            // handle to the top level parent window for Rhino
            _rhinoCore = new RhinoCore(new string[] { "-appmode" }, Rhino.Runtime.InProcess.WindowStyle.Hidden, this.CriticalHandle);
            Rhino.RhinoDoc.ActiveDoc.Modified = false;
            RunGH();
            RhinoDoc.ActiveDoc.Views.Add("RhinoView", DefinedViewportProjection.Perspective, new System.Drawing.Rectangle(0, 0, 200, 200), true);
            var handle = Win32API.FindWindow(null, "Rhino 工作视窗");
            FuckRhino(handle);
            RhinoDoc.ActiveDoc.Views.ActiveView.TitleVisible = false;
            RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.DisplayMode = DisplayModeDescription.GetDisplayMode(DisplayModeDescription.ShadedId);
            happ.WindowInfo.SharedWindow = this;
            happ.WindowInfo.rv = vp;
            happ.WindowInfo.SharedWindow = this;
            //vp.mw = this;
        }


        GH_Document OpenFile(string filePath)
        {
            ghIO = new GH_DocumentIO();
            if (!ghIO.Open(filePath)) { }
            else
            {
                var doc = ghIO.Document;
                myDoc = doc;
                ProcessDoc();
                return doc;
            }
            return null;
        }


        private void ProcessDoc()
        {
            myDoc.Enabled = true;
            refresh = true;
            myDoc.SolutionStart += Doc_SolutionStart;
            myDoc.SolutionEnd += Doc_SolutionEnd;
            myDoc.NewSolution(true);

        }

        private void Doc_SolutionStart(object sender, GH_SolutionEventArgs e)
        {
            allowToNewSolution = false;
            allowToExpire = false;
            foreach (var ac in expireAction) ac.Invoke();
            expireAction.Clear();
            allowToExpire = true;
        }

        private async Task SolveAsync(int val)
        {
            allowToNewSolution = false;
            await Task.Run(() =>
            {
                Thread.Sleep(Math.Max(val, 1));
                myDoc.NewSolution(false, GH_SolutionMode.Silent);
            }).ConfigureAwait(false);

        }

        GH_Document OpenFileStream(string file)
        {
            if (assembly == null) return null;
            GH_Document doc = new GH_Document();
            ResourceManager rm = new ResourceManager("WpfApp.MyDocument", assembly);
            var res = (byte[])rm.GetObject(file);
            if (res == null) return null;
            GH_Archive archive = new GH_Archive();
            archive.Deserialize_Binary(res);
            archive.ExtractObject(doc, "Definition");
            myDoc = doc;
            ProcessDoc();
            return doc;
        }

        public void Con_SolutionExpired(IGH_DocumentObject sender, GH_SolutionExpiredEventArgs e)
        {
            refresh = true;
        }

        private void Doc_SolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            if (refresh)
            {
                this.Invoke(new Action(() =>
                {
                    var doc = Rhino.RhinoDoc.ActiveDoc;
                    rbDisplayConduits.Select(cd => { cd.Enabled = true; return 0; }).ToList();
                    //doc.Views.Redraw();
                }));
            }
            refresh = false;
            allowToNewSolution = true;
        }

        private void ElementContainer_Loaded(object sender, RoutedEventArgs e)
        {

        }


        public void setWindowName(string name)
        {
            Title = name;

        }

        /// <summary>
        /// Sets the data context.
        /// </summary>
        /// <param name="o">The object.</param>
        public void setDataContext(object o)
        {
            this.DataContext = o;
        }

        public bool HorizontalScrollingEnabled
        {
            get => MasterScrollViewer.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto;
            set => MasterScrollViewer.HorizontalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        }


        /// <summary>
        /// Sets the font of most text elements in the window.
        /// </summary>
        /// <param name="fontName">Name of the font.</param>
        public void setFont(string fontName)
        {
            FontFamily ff = new System.Windows.Media.FontFamily(fontName);
            TextElement.SetFontFamily(MasterStackPanel, ff);
        }


        /// <summary>
        /// Adds the element to the master stack panel.
        /// </summary>
        /// <param name="elem">The element to add.</param>
        /// <returns>The index of the added element</returns>
        public int AddElement(UIElement elem)
        {
            return MasterStackPanel.Children.Add(elem);
        }

        /// <summary>
        /// Adds the element to the master stack panel at the specified index.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <param name="index">The index.</param>
        public void AddElement(UIElement elem, int index)
        {
            MasterStackPanel.Children.Insert(index, elem);
        }

        /// <summary>
        /// Removes the element from the master stack panel.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <returns>the index of the removed element.</returns>
        public int RemoveFromStack(UIElement elem)
        {
            int i = MasterStackPanel.Children.IndexOf(elem);
            MasterStackPanel.Children.Remove(elem);
            return i;
        }

        /// <summary>
        /// Adds the element to the main grid.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <param name="zIndex">Z index of the element</param>
        public void AddToGrid(UIElement elem, int zIndex)
        {
            AbsPosGrid.Children.Add(elem);
            Grid.SetZIndex(elem, zIndex);
        }

        /// <summary>
        /// Removes the element from the grid.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <returns>the z index of the removed element</returns>
        public int RemoveFromGrid(UIElement elem)
        {
            int i = Grid.GetZIndex(elem);
            AbsPosGrid.Children.Remove(elem);
            return i;
        }

        /// <summary>
        /// Moves the element from stack to grid.
        /// </summary>
        /// <param name="elem">The element.</param>
        public void MoveFromStackToGrid(UIElement elem)
        {
            int index = RemoveFromStack(elem);
            AddToGrid(elem, index);
        }

        /// <summary>
        /// Moves the element from grid to stack.
        /// </summary>
        /// <param name="elem">The element.</param>
        public void MoveFromGridToStack(UIElement elem)
        {
            int index = RemoveFromGrid(elem);
            AddElement(elem, index);
        }


        /// <summary>
        /// Clears the elements from the grid and the master stack panel, then replaces the master stack panel in the grid.
        /// </summary>
        public void clearElements()
        {
            MasterStackPanel.Children.Clear();
            AbsPosGrid.Children.Clear();
            AbsPosGrid.Children.Add(MasterStackPanel);
        }

        private void OnCtnLoaded(object sender, RoutedEventArgs e)
        {
            var files = Directory.GetFiles(@"Resources\");
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".gh")
                {
                    OpenFile(file);
                    break;
                }
            }
        }



        private void SetGH()
        {
            gh.ShowEditor();
            Grasshopper.Instances.DocumentEditor.Hide();
        }


        private void OnUnload(object sender, EventArgs e)
        {
            ghIO = null;
            gh.CloseAllDocuments();
            Commands.Run_GrasshopperUnloadPlugin();
            gh = null;
            _rhinoCore.Dispose();
            _rhinoCore = null;
        }

    }
}
