using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Util;
using Util.MVVM;

namespace SoundProfiler2 {
    public class MainViewModel : BaseViewModel {
        #region Private Fields
        #region Commands
        private ICommand testCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        #region Commands
        public ICommand TestCommand => testCommand ??= new CommandHandler(() => TestAsync(), () => true);
        #endregion Commands
        #endregion Public properties

        #region Private Methods
        private async void TestAsync() {
            using WaitCursor cursor = new();

            await Task.Delay(500);

            MessageBox.Show("Hello World!");
        }
        #endregion Private Methods
    }
}
