using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCD_Sharp
{
    public class WindowHook
    {
        private Win32Interop.EventProc windowProc;

        public event EventHandler<EventEventArgs> WindowEvent;

        public IntPtr Hook
        {
            get;
            private set;
        } = IntPtr.Zero;

        public WindowHook()
        {
            windowProc = OnFocusChange;
        }

        public void InstallHook()
        {
            if (Hook == IntPtr.Zero)
            {
                Hook = Win32Interop.SetWinEventHook((uint)Win32Interop.EVENT.EVENT_OBJECT_FOCUS, (uint)Win32Interop.EVENT.EVENT_OBJECT_FOCUS, IntPtr.Zero, windowProc, 0, 0, 0);
            }
        }

        public void UninstallHook()
        {
            if (Hook != IntPtr.Zero)
            {
                if (Win32Interop.UnhookWinEvent(Hook))
                {
                    Hook = IntPtr.Zero;
                }
            }
        }

        public void OnFocusChange(IntPtr HWINEVENTHOOK, uint dwEvent, IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            WindowEvent(this, new());
        }

        public class EventEventArgs : EventArgs
        {
            public EventEventArgs()
            {

            }
        }
    }
}
