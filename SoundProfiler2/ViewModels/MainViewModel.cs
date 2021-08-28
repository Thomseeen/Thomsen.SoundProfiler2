using SoundProfiler2.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Util;
using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class MainViewModel : BaseViewModel {
        #region Private Fields
        private BindingList<MixerApplicationModel> mixerApplications = new();

        #region Commands
        private ICommand testCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public BindingList<MixerApplicationModel> MixerApplications {
            get => mixerApplications;
            set { mixerApplications = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand TestCommand => testCommand ??= new CommandHandler(() => TestAsync(), () => true);
        #endregion Commands
        #endregion Public properties

        #region Private Methods
        private async void TestAsync() {
            using WaitCursor cursor = new();

            await Task.Run(() => {
                MixerApplicationModel[] newMixerApplications = CoreAudioWrapper.GetMixerApplications();

                /* Only add new apps */
                foreach (MixerApplicationModel newMixerApplication in newMixerApplications) {
                    if (!MixerApplications.Any(mixerApp => mixerApp.Equals(newMixerApplication))) {
                        MixerApplications.Add(newMixerApplication);
                    }
                }
            });
        }
        #endregion Private Methods
    }
}
