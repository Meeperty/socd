using socd.ViewModels;

namespace socd
{
    public class KeyBindDataBindings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void Update()
        {
            PropertyChanged?.Invoke(this, new(nameof(CodeString)));
        }

        private string codeString;
        public string CodeString
        {
            get
            {
                return codeString;
            }
            set
            {
                codeString = value;
                if (ushort.TryParse(codeString, System.Globalization.NumberStyles.HexNumber, null, out ushort b))
                {
                    bind = b;
                }
                PropertyChanged?.Invoke(this, new(nameof(CodeString)));
                PropertyChanged?.Invoke(this, new(nameof(KeyString)));
            }
        }

        public string KeyString
        {
            get
            {
                if (((VKShort)bind).ToString() != bind.ToString())
                    return $" = {(VKShort)bind}";
                else
                    return " = not a key :(";
            }
        }

        private ushort bind;
        public ushort Bind
        {
            get
            {
                return bind;
            }
            set
            {
                bind = value;
                CodeString = bind.ToString("X");
            }
        }
    }
}
