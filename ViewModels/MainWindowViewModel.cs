using System.ComponentModel;

namespace socd.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public event PropertyChangedEventHandler pChanged;
        public string Greeting => "Welcome to Avalonia!";
    }
}
