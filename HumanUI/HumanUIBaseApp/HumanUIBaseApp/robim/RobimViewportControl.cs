using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RhinoWindows.Forms.Controls;
using Rhino.Runtime;
using Rhino.ApplicationSettings;
using Rhino;
using System.Drawing;
using System;

namespace RobotIcPlus.RhinoModule
{
    /// <summary>
    /// Rhino内嵌视窗
    /// </summary>
   public class RobimViewportControl : ViewportControl
    {
        /// <summary>
        /// 重写的视窗刷新方法
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            owner?.ShadowViewport.ParentView.Redraw();
        }

        internal RobimViewport owner;
        public new Point PreviousMouseLocation { get; private set; } = Point.Empty;

        public new event EventHandler<MouseEventArgs> MouseMove;

        /// <summary>
        /// 重写的鼠标滚轮触发的动作
        /// </summary>
        /// <param name="e">鼠标事件参数</param>
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
                Viewport.Magnify(magnificationFactor, false);
                owner?.ShadowViewport.Magnify(magnificationFactor, false);
            }
            Refresh();
        }

        /// <summary>
        /// 重写的鼠标移动触发的动作
        /// </summary>
        /// <param name="e">鼠标事件参数</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (HostUtils.RunningInRhino)
            {
                Point location = e.Location;
                bool flag = false;
                if (!this.PreviousMouseLocation.IsEmpty)
                {
                    if (e.Button == MouseButtons.Right && ModifierKeys == Keys.None)
                    {
                        this.Viewport.MouseRotateAroundTarget(this.PreviousMouseLocation, location);
                        owner?.ShadowViewport.MouseRotateAroundTarget(this.PreviousMouseLocation, location);
                        flag = true;
                    }
                    if (e.Button == MouseButtons.Right && ModifierKeys == Keys.Shift)
                    {
                        this.Viewport.MouseLateralDolly(this.PreviousMouseLocation, location);
                        owner?.ShadowViewport.MouseLateralDolly(this.PreviousMouseLocation, location);
                        flag = true;
                    }
                }
                if (flag)
                {
                    Refresh();
                }
                PreviousMouseLocation = location;
            }
            MouseMove.Invoke(this, e);
        }
    }
}
