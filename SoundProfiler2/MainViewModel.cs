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

            CoreAudioWrapper.GetDeviceNames();
        }
        #endregion Private Methods
    }
}
