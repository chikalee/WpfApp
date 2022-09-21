using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RhinoWindows.Forms.Controls;
using Rhino.Runtime;
using Rhino.ApplicationSettings;

namespace HumanUIBaseApp
{
    class MyViewportControl:ViewportControl
    {
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (HostUtils.RunningInRhino)
            {
                double magnificationFactor = 1.0 / ViewSettings.ZoomScale;
                magnificationFactor *= magnificationFactor;
                magnificationFactor *= (e.Delta < 0) ? -1.0 : 1.0;
                if (magnificationFactor < 0.0)
                {
                    magnificationFactor = -1.0 / magnificationFactor;
                }
                this.Viewport.Magnify(magnificationFactor, false);
                base.OnMouseWheel(e);
                this.Refresh();
            }

        }
    }
}
