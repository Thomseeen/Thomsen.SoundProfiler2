using SoundProfiler2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Util;
using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class MainViewModel : BaseViewModel, IDisposable {
        #region Private Fields
        private bool disposedValue;
        private readonly Timer refreshTimer = new(100);

        private BindingList<MixerApplicationModel> mixerApplications = new();

        #region Commands
        private ICommand refreshCommand;
        private ICommand exitCommand;

        private ICommand testCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public BindingList<MixerApplicationModel> MixerApplications {
            get => mixerApplications;
            set { mixerApplications = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand RefreshCommand => refreshCommand ??= new CommandHandler(() => RefreshAsync(), () => true);
        public ICommand ExitCommand => exitCommand ??= new CommandHandler(() => Application.Current.Shutdown(), () => true);
        public ICommand TestCommand => testCommand ??= new CommandHandler(() => Test(), () => true);
        #endregion Commands
        #endregion Public properties

        #region Constructors
        public MainViewModel() {
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();
        }
        #endregion Constructors

        #region Event Handler
        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e) {
            RefreshAsync();
        }
        #endregion Event Handler

        #region Private Methods
        private async void RefreshAsync() {
            await Task.Run(() => {
                MixerApplicationModel[] newMixerApplications = CoreAudioWrapper.GetMixerApplications();

                /* Only add new apps and refresh volume on old ones */
                foreach (MixerApplicationModel newMixerApplication in newMixerApplications) {
                    if (!MixerApplications.Any(mixerApp => mixerApp.Equals(newMixerApplication))) {
                        newMixerApplication.PropertyChanged += MixerApplication_PropertyChanged;
                        Application.Current.Dispatcher.Invoke(() => {
                            MixerApplications.Add(newMixerApplication);
                        });
                    } else {
                        Application.Current.Dispatcher.Invoke(() => {
                            MixerApplications.Single(mixerApp => mixerApp.Equals(newMixerApplication)).VolumeLevel = newMixerApplication.VolumeLevel;
                        });
                    }
                }
            });
        }

        private void MixerApplication_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (sender is MixerApplicationModel mixerApplication && e.PropertyName == nameof(MixerApplicationModel.VolumeLevel)) {
                CoreAudioWrapper.SetMixerApplicationVolume(mixerApplication);
            }
        }

        private static void Test() {
            GC.Collect();
        }
        #endregion Private Methods

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
