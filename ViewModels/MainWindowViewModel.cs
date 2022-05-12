using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia;
using SOCD_Sharp;
using System.IO;


namespace SOCD_Sharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        public event EventHandler<string> DebugBox;

        const string internalSettingsPath = @"socdSettings.conf";
        string settingsPath;
        const int maxProcessListLength = 200;
        public List<string> processList;

        public KeyboardHookWindows kbhook = new();
        public WindowHook wndHook = new();

        public void Init()
        {
            StreamReader reader = new(internalSettingsPath);
            if (reader.Peek() != -1)
            {
                settingsPath = reader.ReadLine();
            }
            if (settingsPath != null)
            {
                ReadSettings();
            }
            else
            {
                DebugBox(this, "No settings path in socdSettings.conf");
            }
            reader.Close();

            kbhook.NewKeyboardMessage += KeyHandler;
            kbhook.InstallHook();

            wndHook.WindowEvent += DetectFocusedWindow;
            wndHook.InstallHook();
        }

        //binding property for the key options
        private string[] keyOptions = new string[3] { "WASD", "Arrows", "Custom" };
        public string[] KeyOptions
        {
            get { return keyOptions; }
            set
            {
                keyOptions = value;
                PropertyChanged?.Invoke(this, new(nameof(KeyOptions)));
            }
        }

        //binding property for the selected key option
        private string selectedKeys = "";
        public string SelectedKeys
        {
            get { return selectedKeys; }
            set
            {
                selectedKeys = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedKeys)));
            }
        }

        public void ReadSettings()
        {
            KeySet keys = new();
            List<string> processes = new();
            StreamReader reader = new(settingsPath);
            int l = new();
            int r = new();
            int u = new();
            int d = new();
            try
            {
#pragma warning disable CS8604 // Possible null reference argument.
                l = int.Parse(reader.ReadLine());
                r = int.Parse(reader.ReadLine());
                u = int.Parse(reader.ReadLine());
                d = int.Parse(reader.ReadLine());
                keys = new(l, r, u, d);
                if (KeySet.SameKeys(keys, new(0, 0, 0, 0)))
                {
                    DebugBox(this, "Not enough keys in settings file");
                }

                for (int i = 0; i < maxProcessListLength; i++)
                {
                    if (reader.Peek() != -1) { processes.Add(reader.ReadLine()); }
                    else { break; }
                }
                foreach (var item in processes)
                {
                    if (item != null)
                    {
                        processList.Add(item);
                    }
                }
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (NullReferenceException) { }

            if (keys != null)
            {
                KeySet wasd;
                keyTypes.TryGetValue("WASD", out wasd);
                KeySet arrows;
                keyTypes.TryGetValue("Arrows", out arrows);
                if (KeySet.SameKeys(wasd, keys))
                {
                    SelectedKeys = "WASD";
                }
                else if (KeySet.SameKeys(arrows, keys))
                {
                    SelectedKeys = "Arrows";
                }
                else
                {
                    keyTypes.Add("Custom", new(l,r,u,d));
                }
            }

            reader.Close();
        }

        public bool[] realKeys = new bool[4];
        public bool[] virtualKeys = new bool[4];

        public void KeyHandler(object? sender, KeyboardHookWindows.NewKeyboardMessageEventArgs args)
        {
            Win32Interop.INPUT input = new();
   
            input.type = 1;
            Win32Interop.KEYBDINPUT kbd = new();
            kbd.wScan = 0;
            kbd.time = 0;
            kbd.dwExtraInfo = (IntPtr)0;

            int key = args.VirtKeyCode;
            
            int opposing = FindOpposingKey(key);
            kbd.wVk = opposing;
            // if the keycode isn't in the KeySet, opposing returns -1
            if (opposing == -1) { return; }

            int index = FindIndexByKey(key);
            int opposingIndex = FindIndexByKey(opposing);

            KeyboardHookWindows.KeyboardMessage message = args.Message;

            if (message == KeyboardHookWindows.KeyboardMessage.KeyDown || message == KeyboardHookWindows.KeyboardMessage.SysKeyDown)
            {
                realKeys[index] = true;
                virtualKeys[index] = true;
                if (realKeys[opposingIndex] && virtualKeys[opposingIndex])
                {
                    kbd.dwFlags = 0x2;
                    input.ki = kbd;
                    if (Win32Interop.SendInput(1, new Win32Interop.INPUT[] {input}, Marshal.SizeOf<Win32Interop.INPUT>()) == 0)
                    {
                        DebugBox(this, "Down input failed to send");
                    }
#if DEBUG
                    else
                    {
                        debuggingKeyLog.Add(DateTime.Now, input);
                    }
#endif
                    virtualKeys[opposingIndex] = false;
                }
            }
            if (message == KeyboardHookWindows.KeyboardMessage.KeyUp || message == KeyboardHookWindows.KeyboardMessage.SysKeyUp)
            {
                realKeys[index] = false;
                virtualKeys[index] = false;
                if (realKeys[opposingIndex])
                {
                    kbd.dwFlags = 0;
                    input.ki = kbd;
                    if (Win32Interop.SendInput(1, new Win32Interop.INPUT[] { input }, Marshal.SizeOf<Win32Interop.INPUT>()) == 0)
                    {
                        DebugBox(this, "Up input failed to send");
                    }
#if DEBUG
                    else
                    {
                        debuggingKeyLog.Add(DateTime.Now, input);
                    }
#endif

                }
            }
        }

        public void DetectFocusedWindow(object? sender, WindowHook.EventEventArgs args)
        {
            string currFocusedWindow = GetFocusedWindowName();
            DebugBox(this, currFocusedWindow);
        }

        public string GetFocusedWindowName()
        {
            //HWND inspected_window = GetForegroundWindow();
            //DWORD process_id = 0;
            //GetWindowThreadProcessId(inspected_window, &process_id);
            //if (process_id == 0)
            //{
            //    // Sometimes when you minimize a window nothing is focused for a brief moment,
            //    // in this case windows sends "System Idle Process" as currently focused window
            //    // for some reason. Just ignore it
            //    return;
            //}
            //HANDLE hproc = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_QUERY_LIMITED_INFORMATION, 0, process_id);
            //if (hproc == NULL)
            //{
            //    // Sometimes we can't open a process (#5 "access denied"). Ignore it I guess.
            //    if (GetLastError() == 5)
            //    {
            //        return;
            //    }
            //    error_message("Couldn't open active process, error code is: %d");
            //}
            //DWORD filename_size = MAX_PATH;
            //// This function API is so fucking weird. Read its docs extremely carefully
            //QueryFullProcessImageName(hproc, 0, focused_program, &filename_size);
            //CloseHandle(hproc);
            //PathStripPath(focused_program);
            //printf("Window activated: %s\n", focused_program);

            IntPtr inspectedWindow = Win32Interop.GetForegroundWindow();
            uint processId = 0;
            if (inspectedWindow != IntPtr.Zero)
            {
                processId = Win32Interop.GetWindowThreadProcessId(inspectedWindow);
            }

            if (processId == 0)
            {
                return "";
            }

            IntPtr hproc = Win32Interop.OpenProcess(0x400 | 0x1000, false, processId);
            if (hproc == IntPtr.Zero)
            {
                if (Win32Interop.GetLastError() == 5)
                {
                    return "";
                }
            }
            IntPtr filenameSize = Marshal.AllocHGlobal(Marshal.SizeOf<uint>());

            string imageName;
            Win32Interop.QueryFullProcessImageName(hproc, 0, out imageName, filenameSize);
            Win32Interop.CloseHandle(hproc);
            Path.GetFileName(imageName);
            return imageName;
        }

        public int FindIndexByKey(int key)
        {
            KeySet keys = CurrentKeys;
            if (key == keys.left)
                return 0;
            if (key == keys.right)
                return 1;
            if (key == keys.up)
                return 2;
            if (key == keys.down)
                return 3;
            return 4;
        }

        public int FindOpposingKey(int key)
        {
            KeySet keys = currKeys();
            if (key == keys.left)
                return keys.right;
            if (key == keys.right)
                return keys.left;
            if (key == keys.up)
                return keys.down;
            if (key == keys.down)
                return keys.up;
            return -1;
        }

        KeySet CurrentKeys
        {
            get { return currKeys(); }
        }

        public KeySet currKeys()
        {
            KeySet? set = new();
            string selectedSet = SelectedKeys;

            //switch (SelectedKeys)
            //{
            //    case "WASD":
            //        keyTypes.TryGetValue("WASD", out set);
            //        break;

            //    case "Arrows":
            //        keyTypes.TryGetValue("Arrows", out set);
            //        break;

            //    case "Custom":
            //        keyTypes.TryGetValue("Custom", out set);
            //        break;
            //}
            keyTypes.TryGetValue(selectedSet, out set);

            if (set != null) { return set; }
            else { return new(); }
        }

        public Dictionary<string, KeySet> keyTypes = new Dictionary<string, KeySet> 
        {
            { "WASD", new KeySet(65,68,87,83) },
            { "Arrows", new KeySet(37,39,38,40) },
        };

#if DEBUG
        public Dictionary<DateTime, Win32Interop.INPUT> debuggingKeyLog = new();
#endif

        public class KeySet
        {
            public int left;
            public int right;
            public int up;
            public int down;

            public KeySet(int l, int r, int u, int d)
            {
                left = l;
                right = r;
                up = u;
                down = d;
            }
            public KeySet()
            {
                left = 0;
                right = 0;
                up = 0;
                down = 0;
            }

            public static bool SameKeys(KeySet first, KeySet second)
            {
                bool same = true;
                if (first.left != second.left) { same = false; }
                if (first.right != second.right) { same = false; }
                if (first.up != second.up) { same = false; }
                if (first.down != second.down) { same = false; }
                return same;
            }

            public bool ContainsKey(int k)
            {
                if (left == k || right == k || up == k || down == k) { return true; }
                return false;
            }
        }
    }
}
