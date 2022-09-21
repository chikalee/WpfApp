using System;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using HumanUIBaseApp;
using Grasshopper.Kernel;

namespace HumanUI.Components.UI_Output
{
    /// <summary>
    /// Component to modify the contents of a multi-line Textblock object. 
    /// </summary>
    /// <seealso cref="Grasshopper.Kernel.GH_Component" />
    public class SetTextBlock_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetLabel_Component class.
        /// </summary>
        public SetTextBlock_Component()
            : base("Set TextBlock Contents", "SetTextBlock",
                "Modify the contents of an existing Text Block object.",
                "Human UI", "UI Output")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Text Block to modify", "TB", "The text block object to modify", GH_ParamAccess.item);
            pManager.AddTextParameter("New Text Block contents", "C", "The new text to display in the text block", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object TextBlockObject = null;
            string newTextBlockContents = "";
            if (!DA.GetData<string>("New Text Block contents", ref newTextBlockContents)) return;
            if (!DA.GetData<object>("Text Block to modify", ref TextBlockObject)) return;
            //extract the TextBlock object from the generic object
            TextBlock l = HUI_Util.GetUIElement<TextBlock>(TextBlockObject);
           
            if (l != null)
            {
                if (WindowInfo.SharedWindow != null)
                {
                    WindowInfo.SharedWindow.Invoke(new Action(() =>
                    { l.Text = newTextBlockContents; }));
                }
                else l.Text = newTextBlockContents; 
            }


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.SetTextBlock;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{A1E21BA0-2F60-41DF-BB5A-619802C5AF9A}");
    }
}