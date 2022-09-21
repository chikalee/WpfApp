using System;
using Grasshopper.Kernel;
using HumanUIBaseApp;
using RhinoWindows.Forms.Controls;
using Rhino.Display;

namespace HumanUI.Components.UI_Main
{
    /// <summary>
    /// Represents the ownership status of a window - whether it is a child of Rhino, Grasshopper, or set to be always on top. 
    /// </summary>


    /// <summary>
    /// This component launches an empty HumanUIBaseApp.MainWindow.
    /// </summary>
    /// <seealso cref="Grasshopper.Kernel.GH_Component" />
    public class SharedWindow : GH_Component
    {

        protected MainWindow1 mw;
        protected RhinoWindows.Forms.Controls.ViewportControl rv;
        protected Rhino.Display.RhinoViewport shadow;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SharedWindow()
            : base("Shared Window", "Shared Window", "This component get a new blank control window.", "Human UI", "UI Main")
        {
            //UpdateMenu();
        }

        //Alternate Constructor to be overridden by Transparent Window component
        public SharedWindow(string name, string nickname, string description, string category, string subcategory)
            : base(name,nickname,description,category,subcategory)
        {
            //UpdateMenu();   
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Window Object", "W", "The window object. Other components can access this to add controls or gather data from the window.", GH_ParamAccess.item);
            pManager.AddGenericParameter("ViewportCtrl Object", "W", "The ViewportCtrl object. Other components can access this to add controls or gather data from the window.", GH_ParamAccess.item);
            pManager.AddGenericParameter("ShadowViewport Object", "S", "The ShadowViewport object. Other components can access this to add controls or gather data from the window.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            mw = WindowInfo.SharedWindow;
            rv = WindowInfo.rv;
            shadow = WindowInfo.shadow;
            if (mw != null)
                DA.SetData("Window Object", mw);
            if(rv!=null)
                DA.SetData(1, rv);
            if (shadow != null)
                DA.SetData(2, shadow);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.LaunchWindow;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{0A6B8A40-57A4-4D8D-9F09-F34869222D1E}");



        public override GH_Exposure Exposure => GH_Exposure.primary;

 


    }
}
