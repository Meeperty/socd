using Avalonia.Controls;
using socd.ViewModels;
using System.Threading;

namespace socd.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Thread.Sleep(5000);
            InitializeComponent();

            Opened += (object? o, EventArgs e) =>
            {
                Thread.Sleep(2000);
                //test.Test();
                LLKB h = new();
                
                ((MainWindowViewModel)DataContext).greeting = Marshal.GetLastPInvokeError().ToString() + " was the last pinvoke error";
                ((MainWindowViewModel)DataContext).Change();
            };
        }
    }
}
