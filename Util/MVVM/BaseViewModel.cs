using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Util.MVVM {
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable {
        #region Private Fields
        private bool isDisposed;
        private Window view;

        private string mainWindowTitle = $"{Assembly.GetExecutingAssembly().GetName().Name} ({Assembly.GetExecutingAssembly().GetName().Version})";
        #endregion Private Fields

        #region Public Properties
        public string MainWindowTitle {
            get => mainWindowTitle;
            set { mainWindowTitle = value; OnPropertyChanged(); }
        }

        public Window View {
            get => view;
            set { view = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Public Methods
        public void Show() {
            view.Show();
        }
        #endregion Public Methods

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
