using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SOCD_Sharp.ViewModels;
using System;
using System.Runtime.InteropServices;

namespace SOCD_Sharp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Opened += (_, _) =>
            {
                DataContextCast.ErrorBox += MakeErrorBox;
                MakeErrorBox(this, "Wait for debug");
                DataContextCast?.Init();
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
