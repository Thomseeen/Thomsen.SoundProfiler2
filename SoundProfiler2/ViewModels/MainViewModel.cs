using Newtonsoft.Json;

using SoundProfiler2.Models;
using SoundProfiler2.Views;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using Util;
using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class MainViewModel : BaseViewModel {
        #region Private Constants
        private const string UNMAPPED_CATEGORY = "unmapped";
        #endregion Private Constants

        #region Private Fields
        private bool isDisposed = false;
        private readonly Timer refreshTimer = new(100);

        private readonly object mixerApplicationsLock = new();

        private SettingsModel loadedSettings;
        private ProfilesModel loadedProfiles;

        private ProfileModel activeProfile;

        private bool isProfileNameEditing;

        private ObservableCollection<MixerApplicationModel> mixerApplications = new();

        #region Commands
        private ICommand refreshCommand;
        private ICommand exitCommand;

        private ICommand testCommand;

        private ICommand addProfileCommand;
        private ICommand deleteProfileCommand;
        private ICommand renameProfileCommand;
        private ICommand endRenameProfileCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public ProfileModel ActiveProfile {
            get => activeProfile;
            set { activeProfile = value; OnPropertyChanged(); }
        }

        public ProfilesModel LoadedProfiles {
            get => loadedProfiles;
            set { loadedProfiles = value; OnPropertyChanged(); }
        }

        public bool IsProfileNameEditing {
            get => isProfileNameEditing;
            set { isProfileNameEditing = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MixerApplicationModel> MixerApplications {
            get => mixerApplications;
            set { mixerApplications = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand RefreshCommand => refreshCommand ??= new CommandHandler(() => RefreshAsync(), () => true);
        public ICommand ExitCommand => exitCommand ??= new CommandHandler(() => Application.Current.Shutdown(), () => true);
        public ICommand TestCommand => testCommand ??= new CommandHandler(() => Test(), () => true);

        public ICommand AddProfileCommand => addProfileCommand ??= new CommandHandler(() => AddProfile(), () => !IsProfileNameEditing);
        public ICommand DeleteProfileCommand => deleteProfileCommand ??= new CommandHandler(() => DeleteProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand RenameProfileCommand => renameProfileCommand ??= new CommandHandler(() => RenameProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand EndRenameProfileCommand => endRenameProfileCommand ??= new CommandHandler(() => EndRenameProfile(), () => IsProfileNameEditing);
        #endregion Commands
        #endregion Public properties

        #region Constructors
        public MainViewModel() {
            try {
                ReadSettings();
            } catch (Exception ex) when (ex is FileNotFoundException || ex is JsonSerializationException) {
                /* Create defaults */
                loadedSettings = SettingsModel.GetDefaultModel();
                WriteSettings();
            }

            try {
                ReadProfiles();
            } catch (Exception ex) when (ex is FileNotFoundException || ex is JsonSerializationException) {
                /* Create defaults */
                loadedProfiles = ProfilesModel.GetDefaultModel();
                WriteProfiles();
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

                            SafeDispatcher.Invoke(() => {
                                MixerApplications.Add(newMixerApplication);
                            });
                        } else {

                            SafeDispatcher.Invoke(() => {
                                MixerApplications.Single(mixerApp => mixerApp.Equals(newMixerApplication)).VolumeLevel = newMixerApplication.VolumeLevel;
                            });
                        }
                    }

                    /* Delete old ones */
                    MixerApplications.Where(mixerApp => !newMixerApplications.Contains(mixerApp)).ToList().ForEach(mixerApp => SafeDispatcher.Invoke(() => MixerApplications.Remove(mixerApp)));

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

        private void AddProfile() {
            ProfileModel newProfile = new() {
                Name = "",
                CategoryVolumes = new ObservableCollection<CategoryVolumeModel>(loadedSettings.CategoryMappings.Select(mapping =>
                   new CategoryVolumeModel() {
                       Name = mapping.Name,
                       Volume = 1
                   }))
            };
            LoadedProfiles.Profiles.Add(newProfile);
            ActiveProfile = newProfile;

            IsProfileNameEditing = true;
        }

        private void DeleteProfile() {
            LoadedProfiles.Profiles.Remove(ActiveProfile);
            ActiveProfile = LoadedProfiles.Profiles.First();

            WriteProfiles();
        }

        private void RenameProfile() {
            IsProfileNameEditing = true;
        }

        private void EndRenameProfile() {
            IsProfileNameEditing = false;

            WriteProfiles();

            ProfileModel tmpModel = ActiveProfile;
            LoadedProfiles.Profiles.Remove(tmpModel);
            LoadedProfiles.Profiles.Add(tmpModel);
            ActiveProfile = tmpModel;

            var test = (View as MainView).profileComboBox;
        }

        private void ReadSettings() {
            JsonSerializer jsonSerializer = new();

            using StreamReader settingsFileReader = new(SettingsModel.DEFAULT_SETTINGS_FILEPATH);
            using JsonTextReader settingsJsonReader = new(settingsFileReader);
            loadedSettings = jsonSerializer.Deserialize<SettingsModel>(settingsJsonReader);
        }

        private void WriteSettings() {
            JsonSerializer jsonSerializer = new();

            using StreamWriter settingsFileWriter = new(loadedSettings.FilePath);
            using JsonTextWriter settingsJsonWriter = new(settingsFileWriter);
            jsonSerializer.Serialize(settingsJsonWriter, loadedSettings);
        }

        private void ReadProfiles() {
            JsonSerializer jsonSerializer = new();

            using StreamReader profilesFileReader = new(ProfilesModel.DEFAULT_PROFILES_FILEPATH);
            using JsonTextReader profilesJsonReader = new(profilesFileReader);
            loadedProfiles = jsonSerializer.Deserialize<ProfilesModel>(profilesJsonReader);
        }

        private void WriteProfiles() {
            JsonSerializer jsonSerializer = new();

            using StreamWriter profilesFileWriter = new(loadedProfiles.FilePath);
            using JsonTextWriter profilesJsonWriter = new(profilesFileWriter);
            jsonSerializer.Serialize(profilesJsonWriter, loadedProfiles);
        }

        private void ProfileUp() {
            int index = LoadedProfiles.Profiles.IndexOf(ActiveProfile) + 1;
            index = index >= LoadedProfiles.Profiles.Count ? 0 : index;

            ActiveProfile = LoadedProfiles.Profiles[index];
        }

        private void ProfileDown() {
            int index = LoadedProfiles.Profiles.IndexOf(ActiveProfile) - 1;
            index = index < 0 ? LoadedProfiles.Profiles.Count - 1 : index;

            ActiveProfile = LoadedProfiles.Profiles[index];
        }

        private void Test() {
            ProfileUp();
        }
        #endregion Private Methods

        #region BaseViewModel
        protected override void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                }

                WriteProfiles();
                WriteSettings();
                isDisposed = true;

                base.Dispose(true);
            }
        }

    }
    #endregion IDisposable
}
