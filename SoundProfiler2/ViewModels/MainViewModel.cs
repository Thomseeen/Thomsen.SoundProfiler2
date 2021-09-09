using Newtonsoft.Json;

using SoundProfiler2.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class MainViewModel : BaseViewModel, IDisposable {
        #region Private Constants
        private const string DEFAULT_SETTINGS_FILEPATH = "settings.json";
        private const string DEFAULT_PROFILES_FILEPATH = "profiles.json";

        private const string UNMAPPED_CATEGORY = "unmapped";
        #endregion Private Constants

        #region Private Fields
        private bool disposedValue;
        private readonly Timer refreshTimer = new(100);

        private readonly object mixerApplicationsLock = new();

        private readonly SettingsModel loadedSettings;
        private readonly ProfilesModel loadedProfiles;

        private ProfileModel activeProfile;

        private ObservableCollection<MixerApplicationModel> mixerApplications = new();

        #region Commands
        private ICommand refreshCommand;
        private ICommand exitCommand;

        private ICommand testCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public ProfileModel ActiveProfile {
            get => activeProfile;
            set { activeProfile = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProfileModel> LoadedProfiles {
            get => loadedProfiles.Profiles;
            set { loadedProfiles.Profiles = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MixerApplicationModel> MixerApplications {
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
            JsonSerializer jsonSerializer = new();

            try {
                using StreamReader settingsFileReader = new(DEFAULT_SETTINGS_FILEPATH);
                using JsonTextReader settingsJsonReader = new(settingsFileReader);
                loadedSettings = jsonSerializer.Deserialize<SettingsModel>(settingsJsonReader);

                using StreamReader profilesFileReader = new(DEFAULT_PROFILES_FILEPATH);
                using JsonTextReader profilesJsonReader = new(profilesFileReader);
                loadedProfiles = jsonSerializer.Deserialize<ProfilesModel>(profilesJsonReader);
            } catch (FileNotFoundException) {
                /* Create defaults */
                Test();
            }

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
            refreshTimer.Stop();
            await Task.Run(() => {
                MixerApplicationModel[] newMixerApplications = CoreAudioWrapper.GetMixerApplications();

                /* Lock so multiple firing events don't overwrite each other, causing duplicate entries */
                lock (mixerApplicationsLock) {
                    /* Only add new apps and refresh volume on old ones */
                    foreach (MixerApplicationModel newMixerApplication in newMixerApplications) {
                        if (!MixerApplications.Contains(newMixerApplication)) {
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

                    /* Delete old ones */
                    MixerApplications.Where(mixerApp => !newMixerApplications.Contains(mixerApp)).ToList().ForEach(mixerApp => Application.Current.Dispatcher.Invoke(() => MixerApplications.Remove(mixerApp)));

                    /* Map application category */
                    if (loadedSettings != null) {
                        foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                            mixerApplication.Category = loadedSettings.CategoryMappings.FirstOrDefault(category => category.Programs.Any(prog => mixerApplication.ProcessName.ToLower().Contains(prog.ToLower())))?.Name ?? UNMAPPED_CATEGORY;
                        }
                    }

                    /* Adapt sound */
                    if (loadedProfiles != null && loadedProfiles.Profiles?.Count > 0) {
                        if (ActiveProfile == null) {
                            ActiveProfile = loadedProfiles.Profiles.First();
                        }

                        foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                            CategoryVolumeModel volumeCategory = ActiveProfile.CategoryVolumes.FirstOrDefault(category => category.Name == mixerApplication.Category);
                            if (volumeCategory != null) {
                                mixerApplication.VolumeLevel = volumeCategory.Volume;
                                CoreAudioWrapper.SetMixerApplicationVolume(mixerApplication);
                            }
                        }
                    }
                }
            });
            refreshTimer.Start();
        }

        private void MixerApplication_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (sender is MixerApplicationModel mixerApplication && e.PropertyName == nameof(MixerApplicationModel.VolumeLevel)) {
                /* Stop timer so the value isn't refreshed while user adjusts values */
                refreshTimer.Stop();
                CoreAudioWrapper.SetMixerApplicationVolume(mixerApplication);
                refreshTimer.Start();
            }
        }

        private static void Test() {
            SettingsModel settings = new() {
                FilePath = DEFAULT_SETTINGS_FILEPATH,
                CategoryMappings = new ObservableCollection<CategoryMappingModel>() {
                    new CategoryMappingModel() {
                        Name = "Music",
                        Programs = new ObservableCollection<string> {"firefox", "spotify"}
                    },
                    new CategoryMappingModel() {
                        Name = "Game",
                        Programs = new ObservableCollection<string> {"hunt", "trackmania"}
                    },
                    new CategoryMappingModel() {
                        Name = "Voice",
                        Programs = new ObservableCollection<string> {"ts3", "discord", "teams"}
                    }
                }
            };

            ProfilesModel profiles = new() {
                FilePath = DEFAULT_PROFILES_FILEPATH,
                Profiles = new ObservableCollection<ProfileModel>() {
                    new ProfileModel() {
                        Name = "Full Immersion",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Music",
                                Volume = 0f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Voice",
                                Volume = 1f
                            }
                        }
                    },
                    new ProfileModel() {
                        Name = "Casual",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Music",
                                Volume = 0.25f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 0.5f
                            },
                            new CategoryVolumeModel() {
                                Name = "Voice",
                                Volume = 1f
                            }
                        }
                    },
                    new ProfileModel() {
                        Name = "Shut Up",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Music",
                                Volume = 0f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Voice",
                                Volume = 0.25f
                            }
                        }
                    }
                }
            };

            JsonSerializer jsonSerializer = new();

            using StreamWriter settingsFileWriter = new(settings.FilePath);
            using JsonTextWriter settingsJsonWriter = new(settingsFileWriter);
            jsonSerializer.Serialize(settingsJsonWriter, settings);

            using StreamWriter profilesFileWriter = new(profiles.FilePath);
            using JsonTextWriter profilesJsonWriter = new(profilesFileWriter);
            jsonSerializer.Serialize(profilesJsonWriter, profiles);
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
