using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WpfApp
{
    static class PopUpKiller
    {
        public static void KillKillKill(string lpClassName, string lpWindowName)
        {
             Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                bool close = false;
                TimeSpan ts = TimeSpan.Zero;
                sw.Start();
                while (!close)
                {
                    close = KillPopUp(lpClassName, lpWindowName);
                    sw.Stop();
                    ts = sw.Elapsed;
                    if (ts.TotalSeconds < 5)
                        sw.Start();
                    else close = true;
                };
            }).ConfigureAwait(false);
        }
        public static bool KillPopUp(string lpClassName, string lpWindowName)
        {
            IntPtr hWnd = FindWindow("#32770", "Rhino 7  Tabbed dockbars cleanup error");
            if (hWnd != IntPtr.Zero)
                SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            else
                return false;
            return true;

        }

        const uint WM_CLOSE = 0x10;

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }

}
