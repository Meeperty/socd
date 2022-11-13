using System.ComponentModel;

namespace socd.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Greeting => greeting;
        public string greeting = "Welcome to Avalonia!";

        //LLKB kbHook = new();

        public void Change()
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Greeting"));
        }
    }
}
