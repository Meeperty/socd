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
            //var viewModel = new MainWindowViewModel();
            //DataContext = viewModel;

            IntPtr content = Marshal.StringToHGlobalAnsi("Hi");
            IntPtr title = Marshal.StringToHGlobalAnsi("Title");
            this.Opened += (_, _) =>
            {
                Win32Interop.MessageBox(Handle, content, title, 0);
                DataContextCast?.Init();
            };
            this.Closing += (_, _) =>
            {
                DataContextCast?.kbhook.RemoveHook();
            };
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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
