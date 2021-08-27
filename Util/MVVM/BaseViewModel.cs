using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Util.MVVM {
    public class BaseViewModel : INotifyPropertyChanged {
        #region Private Fields
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
    }
}
