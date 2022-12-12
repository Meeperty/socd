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

            Closed += (object? o, EventArgs e) =>
            {
                ((MainWindowViewModel)DataContext).Dispose();
            };
        }
    }
}
