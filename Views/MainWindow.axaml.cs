using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using socd.ViewModels;
using System.Threading;

namespace socd.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //Thread.Sleep(5000);
            InitializeComponent();

            Opened += (object? o, EventArgs e) =>
            {
                #if DEBUG
                MessageBox(PlatformImpl.Handle.Handle, "Debug", "Box", 0);
                #endif

                DBTextBox.AddHandler(KeyUpEvent, OnDBTextInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            };

            Closed += (object? o, EventArgs e) =>
            {
                ((MainWindowViewModel)DataContext).Dispose();
            };
        }

        public void OnDBTextInput(object? o, KeyEventArgs e)
        {
            ((MainWindowViewModel)DataContext)?.OnDBTextInput(DBTextBox, e);
        }

        #if DEBUG
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
        #endif
    }
}
