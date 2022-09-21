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


    class MyViewport:ViewportControl
    {
       internal MainWindow1 mw;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            lock (MainWindow1._Sync)
            {
                base.OnMouseMove(e);
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Viewport.Camera35mmLensLength = 50;
            if (HostUtils.RunningInRhino)
            {
                double magnificationFactor = 1.0 / ViewSettings.ZoomScale;
                magnificationFactor *= magnificationFactor;
                magnificationFactor *= (e.Delta < 0) ? -1.0 : 1.0;
                if (magnificationFactor < 0.0)
                {
                    magnificationFactor = -1.0 / magnificationFactor;
                }
                Viewport.Magnify(magnificationFactor, false);
            }
            lock (MainWindow1._Sync)
            {
                Refresh();
            }
        }
    }
}
