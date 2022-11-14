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
                Thread.Sleep(5000);
                //test.Test();
                LLKB h = new();

                try
                {
#pragma warning disable CS8600, CS8602
                    ((MainWindowViewModel)DataContext).greeting = Marshal.GetLastPInvokeError().ToString() + " was the last pinvoke error";
                    ((MainWindowViewModel)DataContext).Change();
#pragma warning restore CS8600, CS8602
                }
                catch (System.NullReferenceException)
                {

                }
            };
        }
    }
}
