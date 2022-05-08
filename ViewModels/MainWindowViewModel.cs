using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


namespace SOCD_Sharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        public event EventHandler<string> ErrorBox;

        const string internalSettingsPath = @"socdSettings.conf";
        string settingsPath;
        const int maxProcessListLength = 200;
        public List<string> processList;

        public KeyboardHookWindows kbhook = new();

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
                ErrorBox(this, "No settings path in socdSettings.conf");
            }
            reader.Close();

            kbhook.NewKeyboardMessage += KeyHandler;
            kbhook.InstallHook();

            Thread.Sleep(2000);
            uint e1Tag = 0xe1000000;
            uint e0Tag = 0xe0000000;
            //uint test = Win32Interop.MapVirtualKeyW(0x25 + e0Tag, (uint)Win32Interop.MAPTYPE.MAPVK_VK_TO_VSC_EX); 
            //uint test = Win32Interop.MapVirtualKeyW(0x41, (uint)Win32Interop.MAPTYPE.MAPVK_VK_TO_VSC);

            Win32Interop.INPUT[] arr = new Win32Interop.INPUT[1];
            arr[0] = new();
            arr[0].type = 1;
            arr[0].union.ki.dwFlags = (uint)KEYEVENTF.EXTENDEDKEY; 
            arr[0].union.ki.wScan = 0xe1;
            arr[0].union.ki.wVk = 0x27;
            Win32Interop.SendInput(1, arr, Marshal.SizeOf<Win32Interop.INPUT>());

            Thread.Sleep(1);
            arr[0] = new();
            arr[0].type = 1;
            arr[0].union.ki.dwFlags = (uint)(KEYEVENTF.KEYUP | KEYEVENTF.EXTENDEDKEY);
            arr[0].union.ki.wScan = 0xe1;
            arr[0].union.ki.wVk = 0x27;
            Win32Interop.SendInput(1, arr, Marshal.SizeOf<Win32Interop.INPUT>());
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
                    ErrorBox(this, "Not enough keys in settings file");
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
                keySets.TryGetValue("WASD", out wasd);

                KeySet arrows;
                keySets.TryGetValue("Arrows", out arrows);

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
                    keySets.Add("Custom", keys);

                    SelectedKeys = "Custom";
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
            kbd.wVk = (ushort)opposing;
            //int opposingScan = (int)Win32Interop.MapVirtualKeyA((uint)opposing, (uint)Win32Interop.MAPTYPE.MAPVK_VK_TO_VSC);
            //kbd.wScan = opposingScan;
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
                    kbd.dwFlags = 0x2 | 0x1;// | 0x8;
                    input.union.ki = kbd;
                    if (Win32Interop.SendInput(1, new Win32Interop.INPUT[] {input}, Marshal.SizeOf<Win32Interop.INPUT>()) == 0)
                    {
                        ErrorBox(this, "Down input failed to send");
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
                    kbd.dwFlags = 0 | 0x1;//0x8;
                    input.union.ki = kbd;
                    if (Win32Interop.SendInput(1, new Win32Interop.INPUT[] { input }, Marshal.SizeOf<Win32Interop.INPUT>()) == 0)
                    {
                        ErrorBox(this, "Up input failed to send");
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
            KeySet keys = CurrentKeys;
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
            get 
            {
                KeySet? set = new();
                keySets.TryGetValue(SelectedKeys, out set);

                if (set != null) { return set; }
                else { return new(); }
            }
        }

        ScanCodeSet CurrentScans
        {
            get
            {
                ScanCodeSet? set = new();
                scanSets.TryGetValue(SelectedKeys, out set);

                if (set != null) { return set; }
                else { return new(); }
            }
        }

        public Dictionary<string, KeySet> keySets = new Dictionary<string, KeySet> 
        {
            //left, right, up, down
            { "WASD", new(65,68,87,83) },
            { "Arrows", new(37,39,38,40) },
        };

        public Dictionary<string, ScanCodeSet> scanSets = new Dictionary<string, ScanCodeSet>
        {
            //left, right, up, down
            { "WASD", new(0x01E, 0x020, 0x011, 0x01F) },
            { "Arrows", new(0x14B, 0x14D, 0x148, 0x150) }
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

        public class ScanCodeSet
        {
            public int left;
            public int right;
            public int up;
            public int down;

            public ScanCodeSet(int l, int r, int u, int d)
            {
                left = l;
                right = r;
                up = u;
                down = d;
            }
            public ScanCodeSet()
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

        [Flags]
        enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }
    }
}
