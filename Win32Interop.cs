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

        [DllImport("user32.dll")]
        public static extern uint SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, HookProc pfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

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

        public enum EVENT
        {
            EVENT_AIA_START = 0xA000,
            EVENT_AIA_END = 0xAFFF,
            EVENT_MIN = 0x00000001,
            EVENT_MAX = 0x7FFFFFFF,
            EVENT_OBJECT_ACCELERATORCHANGE = 0x8012,
            EVENT_OBJECT_CLOAKED = 0x8017,
            EVENT_OBJECT_CONTENTSCROLLED = 0x8015,
            EVENT_OBJECT_CREATE = 0x8000,
            EVENT_OBJECT_DEFACTIONCHANGE = 0x8011,
            EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D,
            EVENT_OBJECT_DESTROY = 0x8001,
            EVENT_OBJECT_DRAGSTART = 0x8021,
            EVENT_OBJECT_DRAGCANCEL = 0x8022,
            EVENT_OBJECT_DRAGCOMPLETE = 0x8023,
            EVENT_OBJECT_DRAGENTER = 0x8024,
            EVENT_OBJECT_DRAGLEAVE = 0x8025,
            EVENT_OBJECT_DRAGDROPPED = 0x8026,
            EVENT_OBJECT_END = 0x80FF,
            EVENT_OBJECT_FOCUS = 0x8005,
            EVENT_OBJECT_HELPCHANGE = 0x8010,
            EVENT_OBJECT_HIDE = 0x8003,
            EVENT_OBJECT_HOSTEDOBJECTSINVALIDATED = 0x8020,
            EVENT_OBJECT_IME_HIDE = 0x8028,
            EVENT_OBJECT_IME_SHOW = 0x8027,
            EVENT_OBJECT_IME_CHANGE = 0x8029,
            EVENT_OBJECT_INVOKED = 0x8013,
            EVENT_OBJECT_LIVEREGIONCHANGED = 0x8019,
            EVENT_OBJECT_LOCATIONCHANGE = 0x800B,
            EVENT_OBJECT_NAMECHANGE = 0x800C,
            EVENT_OBJECT_PARENTCHANGE = 0x800F,
            EVENT_OBJECT_REORDER = 0x8004,
            EVENT_OBJECT_SELECTION = 0x8006,
            EVENT_OBJECT_SELECTIONADD = 0x8007,
            EVENT_OBJECT_SELECTIONREMOVE = 0x8008,
            EVENT_OBJECT_SELECTIONWITHIN = 0x8009,
            EVENT_OBJECT_SHOW = 0x8002,
            EVENT_OBJECT_STATECHANGE = 0x800A,
            EVENT_OBJECT_TEXTEDIT_CONVERSIONTARGETCHANGED = 0x8030,
            EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014,
            EVENT_OBJECT_UNCLOAKED = 0x8018,
            EVENT_OBJECT_VALUECHANGE = 0x800E,
            EVENT_OEM_DEFINED_START = 0x0101,
            EVENT_OEM_DEFINED_END = 0x01FF,
            EVENT_SYSTEM_ALERT = 0x0002,
            EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016,
            EVENT_SYSTEM_CAPTUREEND = 0x0009,
            EVENT_SYSTEM_CAPTURESTART = 0x0008,
            EVENT_SYSTEM_CONTEXTHELPEND = 0x000D,
            EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C,
            EVENT_SYSTEM_DESKTOPSWITCH = 0x0020,
            EVENT_SYSTEM_DIALOGEND = 0x0011,
            EVENT_SYSTEM_DIALOGSTART = 0x0010,
            EVENT_SYSTEM_DRAGDROPEND = 0x000F,
            EVENT_SYSTEM_DRAGDROPSTART = 0x000E,
            EVENT_SYSTEM_END = 0x00FF,
            EVENT_SYSTEM_FOREGROUND = 0x0003,
            EVENT_SYSTEM_MENUPOPUPEND = 0x0007,
            EVENT_SYSTEM_MENUPOPUPSTART = 0x0006,
            EVENT_SYSTEM_MENUEND = 0x0005,
            EVENT_SYSTEM_MENUSTART = 0x0004,
            EVENT_SYSTEM_MINIMIZEEND = 0x0017,
            EVENT_SYSTEM_MINIMIZESTART = 0x0016,
            EVENT_SYSTEM_MOVESIZEEND = 0x000B,
            EVENT_SYSTEM_MOVESIZESTART = 0x000A,
            EVENT_SYSTEM_SCROLLINGEND = 0x0013,
            EVENT_SYSTEM_SCROLLINGSTART = 0x0012,
            //the first one :shroompog:
            EVENT_SYSTEM_SOUND = 0x0001,
            EVENT_SYSTEM_SWITCHEND = 0x0015,
            EVENT_SYSTEM_SWITCHSTART = 0x0014,
            EVENT_UIA_EVENTID_START = 0x4E00,
            EVENT_UIA_EVENTID_END = 0x4EFF,
            EVENT_UIA_PROPID_START = 0x7500,
            EVENT_UIA_PROPID_END = 0x75FF
        }
        
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
