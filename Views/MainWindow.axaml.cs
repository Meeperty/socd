using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using SOCD_Sharp.ViewModels;
using System;
using SOCD_Sharp;
using System.Runtime.InteropServices;

namespace SOCD_Sharp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Opened += (_, _) =>
            {
                DataContextCast?.Init();
                DataContextCast.ErrorBox += MakeErrorBox;
            };
            Closing += (_, _) =>
            {
                DataContextCast?.kbhook.RemoveHook();
            };
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public void MakeErrorBox(object? sender, string content)
        {
            IntPtr titleANSI = Marshal.StringToHGlobalAnsi("Error");
            IntPtr contentANSI = Marshal.StringToHGlobalAnsi(content);
            Win32Interop.MessageBox(Handle, contentANSI, titleANSI, 0);
        }

        public MainWindowViewModel? DataContextCast
        {
            get { return (MainWindowViewModel)DataContext; }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IntPtr Handle
        {
            get { return this.PlatformImpl.Handle.Handle; }
        }
    }
}
