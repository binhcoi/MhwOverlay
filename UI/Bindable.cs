using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MhwOverlay.UI
{
    public abstract class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(property, value))
                return;
            property = value;
            NotifyPropertyChanged(propertyName);
        }
    }
}
