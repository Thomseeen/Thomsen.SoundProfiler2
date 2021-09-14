using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Util.MVVM {
    public class BaseViewModel : INotifyPropertyChanged, IDisposable {
        #region Private Fields
        private bool isDisposed;
        private string mainWindowTitle = $"{Assembly.GetExecutingAssembly().GetName().Name} ({Assembly.GetExecutingAssembly().GetName().Version})";
        #endregion Private Fields

        #region Public Properties
        public string MainWindowTitle {
            get => mainWindowTitle;
            set { mainWindowTitle = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) { }
                isDisposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
