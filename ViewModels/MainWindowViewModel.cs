namespace socd.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        public string Greeting => greeting;
        public string greeting = "Welcome to Avalonia!";
    }
}
