using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Platform;
using SOCD_Sharp;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace SOCD_Sharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        const string internalSettingsPath = @"socdSettings.conf";
        string settingsPath;
        const int maxProcessListLength = 200;
        List<string> processList;

        public KeyboardHookWindows kbhook = new();

        public void Init()
        {
            StreamReader reader = new StreamReader(internalSettingsPath);
            if (reader.Peek() != -1)
            {
                settingsPath = reader.ReadLine();
            }
            if (settingsPath != null)
            {
                ReadSettings();
            }
            reader.Close();

            kbhook.NewKeyboardMessage += KeyHandler;
            kbhook.InstallHook();
        }

        //binding property for the key options
        private string[] keyOptions = new string[3] { "WASD", "Arrows", "Custom" };
        public string[] KeyOptions
        {
            get { return keyOptions; }
            set
            {
                keyOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KeyOptions)));
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
            KeySet keys = new(1,1,1,1);
            List<string> processes = new();
            StreamReader reader = new StreamReader(settingsPath);
            int l = new();
            int r = new();
            int u = new();
            int d = new();
            try
            {
                l = int.Parse(reader.ReadLine());
                r = int.Parse(reader.ReadLine());
                u = int.Parse(reader.ReadLine());
                d = int.Parse(reader.ReadLine());
                keys = new(l, r, u, d);

                for (int i = 0; i < maxProcessListLength; i++)
                {
                    if (reader.Peek() != -1) { processes.Add(reader.ReadLine()); }
                }
                foreach (var item in processes)
                {
                    if (item != null)
                    {
                        processList.Add(item);
                    }
                }
            }
            catch (NullReferenceException) { }

            if (keys != null)
            {
                KeySet wasd;
                keyTypes.TryGetValue("WASD", out wasd);
                KeySet arrows;
                keyTypes.TryGetValue("Arrows", out arrows);
                if (keys == wasd)
                {
                    SelectedKeys = "WASD";
                }
                else if (keys == arrows)
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
            kbd.wVk = key;
            int opposing = FindOpposingKey(key);
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
                    Win32Interop.SendInput(1, input, Marshal.SizeOf<Win32Interop.INPUT>());
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
                    Win32Interop.SendInput(1, input, Marshal.SizeOf<Win32Interop.INPUT>());
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
            get { return currKeys(); }
        }

        public KeySet currKeys()
        {
            KeySet? set = new();

            switch (SelectedKeys)
            {
                case "WASD":
                    keyTypes.TryGetValue("WASD", out set);
                    break;

                case "Arrows":
                    keyTypes.TryGetValue("Arrows", out set);
                    break;

                case "Custom":
                    keyTypes.TryGetValue("Custom", out set);
                    break;
            }

            if (set != null) { return set; }
            else { return new KeySet(); }
        }

        public Dictionary<string, KeySet> keyTypes = new Dictionary<string, KeySet> 
        {
            { "WASD", new KeySet(41,44,57,53) },
            { "Arrows", new KeySet(25,27,26,28) },
        };

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
        }
    }
}
