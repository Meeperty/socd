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
            };
        }
    }
}
