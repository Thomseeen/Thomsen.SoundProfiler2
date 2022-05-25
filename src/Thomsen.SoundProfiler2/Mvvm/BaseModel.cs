using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Thomsen.WpfTools.Mvvm {
    public abstract class BaseModel : INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged
    }
}
