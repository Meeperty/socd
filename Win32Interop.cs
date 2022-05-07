using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SOCD_Sharp;

namespace SOCD_Sharp
{
    /*
     * Functions:
     * MessageBox
     * CallNextHookEx
     * 
     */
    public static class Win32Interop
    {
        /// <summary>Creates a message box</summary>
        /// <param name="hWnd">Handle to the owner window of the message box to be created</param>
        /// <param name="lpText">Pointer to the message to be displayed as ANSI</param>
        /// <param name="lpCaption">Pointer to the title of the box as ANSI</param>
        /// <param name="uType">Contents and behavior</param>
        /// <returns>an integer indicating which button the user clicked</returns>
        [DllImport("user32.dll")]
        public static extern int MessageBox(IntPtr hWnd, IntPtr lpText, IntPtr lpCaption, uint uType);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(KeyboardHookWindows.HookType idHook, KeyboardHookWindows.HookProc lpfn, IntPtr hMod, int dwThreadId);
        
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr _, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint cInputs, INPUT[] input, int cbSize);
        //public static uint SendInput(uint cInputs, INPUT input, int cbSize) => SendInput(cInputs, new INPUT[] { input }, cbSize);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate IntPtr HookProc(int nCode, UIntPtr wParam, IntPtr lParam);

        public enum MB : long
        {
            MB_ABORTRETRYIGNORE = 0x2L,
            MB_CANCELRETRYCONTINUE = 0x6L,
            MB_HELP = 0x4000L,
            MB_OK = 0x0L,
            MB_OKCANCEL = 0x1L,
            MB_RETRYCANCEL = 0x5L,
            MB_YESNO = 0x4L,
            MB_YESNOCANCEL = 0x3L
        }

        //assumes that this is an INPUT representing a keyboard input for simplicity
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public UInt32 type;
            public KEYBDINPUT ki;
        }

        public struct KEYBDINPUT
        {
            public int wVk;
            public int wScan;
            public long dwFlags;
            public long time;
            //points to a DWORD
            public IntPtr dwExtraInfo;
        }
    }
}
