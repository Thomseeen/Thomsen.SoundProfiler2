using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Thomsen.WpfTools.Mvvm {
    public abstract class BaseViewModel<T> : INotifyPropertyChanged, IDisposable where T : Window, new() {
        #region Private Fields
        private bool _disposed;

        protected Window? _view;
        #endregion Private Fields

        #region Public Properties
        public bool IsViewLoaded => _view?.IsLoaded ?? false;

        public static string DefaultWindowTitle => $"{Assembly.GetExecutingAssembly().GetName().Name} ({Assembly.GetExecutingAssembly().GetName().Version})";
        #endregion Public Properties

        #region Constructors
        public BaseViewModel() {
        }
        #endregion Constructors

        #region Public Methods
        public void Focus() {
            if (_view is not null) {
                _view.Focus();
            }
        }

        public void Show() {
            if (_view is null) {
                _view = new T {
                    DataContext = this
                };

                _view.Closed += (s, e) => {
                    _view = null;
                };
            }

            _view.Loaded += View_Loaded;

            _view.Show();
        }

        public bool? ShowDialog() {
            if (_view is null) {
                _view = new T {
                    DataContext = this
                };
            }

            _view.Loaded += View_Loaded;

            return _view.ShowDialog();
        }

        public void Close() {
            if (_view is not null) {
                _view.Loaded -= View_Loaded;

                _view.Close();
            }
        }

        public void ExitDialog(bool? result) {
            if (_view is not null) {
                _view.Loaded -= View_Loaded;

                _view.DialogResult = result;
                _view.Close();
            }
        }
        #endregion Public Methods

        #region Protected Methods
        protected virtual void View_Loaded(object sender, RoutedEventArgs e) {
        }
        #endregion Protected Methods

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) { }
                _disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
