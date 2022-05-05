using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SOCD_Sharp
{
    public class KeyboardHookWindows
    {
        private HookProc kbProc;

        public IntPtr KeyboardHook { get; private set; } = IntPtr.Zero;

        public event EventHandler<NewKeyboardMessageEventArgs> NewKeyboardMessage;

        public KeyboardHookWindows()
        {
            kbProc = LowLevelKeyboardProc;
        }

        public void InstallHook()
        {
            if (KeyboardHook == IntPtr.Zero)
            {
                KeyboardHook = Win32Interop.SetWindowsHookEx(HookType.LowLevelKeyboard, kbProc, IntPtr.Zero, 0);
            }
        }

        public void RemoveHook()
        {
            Win32Interop.UnhookWindowsHookEx(KeyboardHook);
            KeyboardHook = IntPtr.Zero;
        }

        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            var st = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
            if (nCode > 0 || (st.flags & 0x10) != 0x10)
            {
                NewKeyboardMessage?.Invoke(this, new NewKeyboardMessageEventArgs(st.vkCode, (KeyboardMessage)wParam));
            }
            return Win32Interop.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public class NewKeyboardMessageEventArgs : EventArgs
        {
            public int VirtKeyCode { get; private set; }
            public KeyboardMessage Message { get; private set; }

            public NewKeyboardMessageEventArgs(int vkCode, KeyboardMessage msg)
            {
                VirtKeyCode = vkCode;
                Message = msg;
            }
        }

        public enum KeyboardMessage
        {
            KeyDown = 0x100,
            KeyUp = 0x101,
            SysKeyDown = 0x104,
            SysKeyUp = 0x105
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardLowLevelHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate IntPtr HookProc(int nCode, UIntPtr wParam, IntPtr lParam);

        public enum HookType : int
        {
            LowLevelKeyboard = 13,
            LowLevelMouse = 14
        }
    }
}
