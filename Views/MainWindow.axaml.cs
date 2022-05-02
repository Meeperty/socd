using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SOCD_Sharp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        //string buttonText = "Click Me!";

        //public string ButtonText
        //{
        //    get => buttonText;
        //    set 
        //    {
        //        buttonText = value;
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonText)));
        //    }
        //}

        //public void ButtonClicked() => ButtonText = "Hello, Avalonia!";

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Radio Button properties
        public bool wasdSet = false;
        public bool WASDSet
        {
            get => wasdSet;
            set
            {
                wasdSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WASDSet)));
            }
        }

        public bool arrowsSet = false;
        public bool ArrowsSet
        {
            get => arrowsSet;
            set
            {
                arrowsSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArrowsSet)));
            }
        }

        public bool customSet = false;
        public bool CustomSet
        {
            get => customSet;
            set
            {
                customSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomSet)));
            }
        }
        #endregion
    }
}
