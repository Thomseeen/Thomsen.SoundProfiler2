using Newtonsoft.Json;
using SoundProfiler2.Handler;
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
using System.Windows.Controls;
using System.Windows.Input;

using Util;
using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class MainViewModel : BaseViewModel {
        #region Private Constants
        private const string UNMAPPED_CATEGORY = "unmapped";

        private const string DEFAULT_PROFILES_FILEPATH = "profiles.json";
        private const string DEFAULT_MAPPINGS_FILEPATH = "mappings.json";
        #endregion Private Constants

        #region Private Fields
        private bool isDisposed = false;
        private readonly Timer refreshTimer = new(100);

        private readonly object mixerApplicationsLock = new();
        private readonly object mappingsLock = new();

        private ProfileModel activeProfile;
        private ObservableCollection<ProfileModel> loadedProfiles;
        private ObservableCollection<CategoryMappingModel> loadedMappings;
        private ObservableCollection<MixerApplicationModel> mixerApplications = new();

        private bool isProfileNameEditing;

        #region Commands
        private ICommand refreshCommand;
        private ICommand exitCommand;
        private ICommand testCommand;

        private ICommand addProfileCommand;
        private ICommand removeProfileCommand;
        private ICommand beginRenameProfileCommand;
        private ICommand endRenameProfileCommand;

        private ICommand editKeybindingsCommand;
        private ICommand editMappingsCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public ProfileModel ActiveProfile {
            get => activeProfile;
            set { activeProfile = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ProfileModel> LoadedProfiles {
            get => loadedProfiles;
            set { loadedProfiles = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CategoryMappingModel> LoadedMappings {
            get => loadedMappings;
            set { loadedMappings = value; OnPropertyChanged(); }
        }
        public ObservableCollection<MixerApplicationModel> MixerApplications {
            get => mixerApplications;
            set { mixerApplications = value; OnPropertyChanged(); }
        }
        public bool IsProfileNameEditing {
            get => isProfileNameEditing;
            set { isProfileNameEditing = value; OnPropertyChanged(); }
        }


        #region Commands
        public ICommand RefreshCommand => refreshCommand ??= new CommandHandler(param => RefreshAsync(), () => true);
        public ICommand ExitCommand => exitCommand ??= new CommandHandler(param => Application.Current.Shutdown(), () => true);
        public ICommand TestCommand => testCommand ??= new CommandHandler(param => Test(), () => true);

        public ICommand AddProfileCommand => addProfileCommand ??= new CommandHandler(param => AddProfile(), () => !IsProfileNameEditing);
        public ICommand RemoveProfileCommand => removeProfileCommand ??= new CommandHandler(param => RemoveProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand BeginRenameProfileCommand => beginRenameProfileCommand ??= new CommandHandler(param => BeginRenameProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand EndRenameProfileCommand => endRenameProfileCommand ??= new CommandHandler(param => EndRenameProfile(), () => IsProfileNameEditing);

        public ICommand EditKeybindingsCommand => editKeybindingsCommand ??= new CommandHandler(param => EditKeybindings(), () => true);
        public ICommand EditMappingsCommand => editMappingsCommand ??= new CommandHandler(param => EditMappings(), () => true);
        #endregion Commands
        #endregion Public properties

        #region Constructors
        public MainViewModel() {
            LoadedMappings = new ObservableCollection<CategoryMappingModel>(SettingsHandler.ReadOrWriteDefaultSettings(DEFAULT_MAPPINGS_FILEPATH, CategoryMappingModel.GetDefaultModels()));
            LoadedProfiles = new ObservableCollection<ProfileModel>(SettingsHandler.ReadOrWriteDefaultSettings(DEFAULT_PROFILES_FILEPATH, ProfileModel.GetDefaultModels()));

            View = new MainView {
                DataContext = this
            };

            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();
        }
        #endregion Constructors

        #region Private Methods
        #region Sliders/Volumes Handling
        private async void RefreshAsync() {
            refreshTimer.Stop();
            await Task.Run(() => {
                /* Lock so multiple firing events don't overwrite each other, causing duplicate entries */
                lock (mixerApplicationsLock) {
                    /* Only add new apps and refresh volume on old ones */
                    MergeRefreshedAppsIntoActivesMixerApps(CoreAudioWrapper.GetMixerApplications());

                    //MixerApplications = new ObservableCollection<MixerApplicationModel>(MixerApplications.OrderBy(app => app.FriendlyName));

                    /* Map application category */
                    if (LoadedMappings is not null) {
                        MapCategoriesIntoActiveMixerApps(LoadedMappings);
                    }

                    /* Apply profile mix */
                    lock (mappingsLock) {
                        if (LoadedProfiles is not null && LoadedProfiles?.Count > 0 && !IsProfileNameEditing) {
                            if (ActiveProfile is null) {
                                ActiveProfile = LoadedProfiles.First();
                            }

                            ApplyProfileToActiveMixerApps(ActiveProfile);
                        }
                    }
                }
            });
            refreshTimer.Start();
        }

        private void MergeRefreshedAppsIntoActivesMixerApps(MixerApplicationModel[] newMixerApplications) {
            foreach (MixerApplicationModel newApp in newMixerApplications) {
                if (!MixerApplications.Any(activeApp => activeApp.ProcessName == newApp.ProcessName)) {
                    newApp.PropertyChanged += MixerApplication_PropertyChanged;

                    SafeDispatcher.Invoke(() => {
                        MixerApplications.Add(newApp);
                    });
                } else {

                    SafeDispatcher.Invoke(() => {
                        MixerApplications.Single(activeApp => activeApp.ProcessName == newApp.ProcessName).VolumeLevel = newApp.VolumeLevel;
                    });
                }
            }

            /* Delete old ones */
            MixerApplications.Where(activeApp => !newMixerApplications.Any(newApp => newApp.ProcessName == activeApp.ProcessName)).ToList().ForEach(mixerApp => SafeDispatcher.Invoke(() => MixerApplications.Remove(mixerApp)));
        }

        private void MapCategoriesIntoActiveMixerApps(ObservableCollection<CategoryMappingModel> loadedMappings) {
            foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                mixerApplication.Category = loadedMappings.FirstOrDefault(category => category.Programs.Any(prog =>
                    mixerApplication.UnifiedProcessName.Contains(prog.UnifiedName)
                ))?.Name ?? UNMAPPED_CATEGORY;
            }
        }

        private void ApplyProfileToActiveMixerApps(ProfileModel activeProfile) {
            foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                CategoryVolumeModel volumeCategory = activeProfile.CategoryVolumes.FirstOrDefault(category => category.Name == mixerApplication.Category);
                if (volumeCategory is not null) {
                    mixerApplication.VolumeLevel = volumeCategory.Volume;
                }
            }
        }
        #endregion Sliders/Volumes Handling

        #region Profile Handling
        private void AddProfile() {
            ProfileModel newProfile = new() {
                Name = "New Profile",
                CategoryVolumes = new ObservableCollection<CategoryVolumeModel>(LoadedMappings.Select(mapping =>
                   new CategoryVolumeModel() {
                       Name = mapping.Name,
                       Volume = 1
                   }))//.OrderBy(cat => cat.Name))
            };

            lock (mappingsLock) {
                LoadedProfiles.Add(newProfile);
                ActiveProfile = newProfile;
            }

            SettingsHandler.WriteSettings(LoadedProfiles, DEFAULT_PROFILES_FILEPATH);

            IsProfileNameEditing = true;
            SetTextBoxFocusAndCursor((View as MainView).profileRenameBox, true);
        }

        private void RemoveProfile() {
            lock (mappingsLock) {
                LoadedProfiles.Remove(ActiveProfile);
                ActiveProfile = LoadedProfiles.First();
            }

            SettingsHandler.WriteSettings(LoadedProfiles, DEFAULT_PROFILES_FILEPATH);
        }

        private void BeginRenameProfile() {
            IsProfileNameEditing = true;
            SetTextBoxFocusAndCursor((View as MainView).profileRenameBox, false);
        }

        private void EndRenameProfile() {
            IsProfileNameEditing = false;
            SettingsHandler.WriteSettings(LoadedProfiles, DEFAULT_PROFILES_FILEPATH);
        }

        private void ProfileUp() {
            int index = LoadedProfiles.IndexOf(ActiveProfile) + 1;
            index = index >= LoadedProfiles.Count ? 0 : index;

            lock (mappingsLock) {
                ActiveProfile = LoadedProfiles[index];
            }
        }

        private void ProfileDown() {
            int index = LoadedProfiles.IndexOf(ActiveProfile) - 1;
            index = index < 0 ? LoadedProfiles.Count - 1 : index;

            lock (mappingsLock) {
                ActiveProfile = LoadedProfiles[index];
            }
        }
        #endregion Profile Handling

        #region Dialog Handling
        private void EditKeybindings() { }

        private void EditMappings() {
            lock (mappingsLock) {
                using EditMappingsViewModel mappingsDialog = new(LoadedMappings);
                bool? result = mappingsDialog.ShowDialog();
                if (result.HasValue && result.Value) {
                    /* Save changed mappings */
                    LoadedMappings = mappingsDialog.LoadedMappings;
                    SettingsHandler.WriteSettings(LoadedMappings, DEFAULT_MAPPINGS_FILEPATH);
                } else {
                    /* Read old unchanged mappings from disk */
                    LoadedMappings = new ObservableCollection<CategoryMappingModel>(SettingsHandler.ReadSettings<CategoryMappingModel>(DEFAULT_MAPPINGS_FILEPATH));
                }
            }
        }
        #endregion DialogHandling

        #region UI Handling
        private static void SetTextBoxFocusAndCursor(TextBox textBox, bool selectAll) {
            textBox.Focus();
            /* #BUG: Not working as intended */
            textBox.Select(textBox.Text.Length, selectAll ? textBox.Text.Length : 0);
        }
        #endregion

        private void Test() {
            ProfileUp();
        }
        #endregion Private Methods

        #region Event Handler
        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e) {
            RefreshAsync();
        }

        private void MixerApplication_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (sender is MixerApplicationModel mixerApplication && e.PropertyName == nameof(MixerApplicationModel.VolumeLevel)) {
                /* Stop timer so the value isn't refreshed while user adjusts values */
                refreshTimer.Stop();
                CoreAudioWrapper.SetMixerApplicationVolume(mixerApplication);
                refreshTimer.Start();
            }
        }
        #endregion Event Handler

        #region BaseViewModel
        protected override void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                }

                SettingsHandler.WriteSettings(LoadedProfiles, DEFAULT_PROFILES_FILEPATH);
                SettingsHandler.WriteSettings(LoadedMappings, DEFAULT_MAPPINGS_FILEPATH);

                isDisposed = true;

                base.Dispose(true);
            }
        }
        #endregion BaseViewModel
    }
}
