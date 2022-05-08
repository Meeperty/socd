using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCD_Sharp
{
    class WindowHook
    {
        private Win32Interop.HookProc windowProc;

        public IntPtr Hook
        {
            get;
            private set;
        }

        public WindowHook()
        {
            //windowProc
        }

        public void InstallHook()
        {
            if (Hook == IntPtr.Zero)
            {
                //Hook = Win32Interop.SetWinEventHook(Win32Interop.EVENT.);
            }
        }
    }
}
