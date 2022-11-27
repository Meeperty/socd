using Avalonia.Controls;
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
            };

            Closed += (object? o, EventArgs e) =>
            {
                ((MainWindowViewModel)DataContext).Dispose();
            };
        }

        #if DEBUG
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
        #endif
    }
}
