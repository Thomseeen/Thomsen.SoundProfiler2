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
        #endregion Private Fields

        #region Public Properties
        public virtual string WindowTitle => $"{Assembly.GetExecutingAssembly().GetName().Name} ({Assembly.GetExecutingAssembly().GetName().Version})";

        public Window View {
            get => view;
            set { view = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Public Methods
        public void Show() => view.Show();

        public bool? ShowDialog() => view.ShowDialog();

        public void ExitDialog(bool? result) {
            view.DialogResult = result;
            view.Close();
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
