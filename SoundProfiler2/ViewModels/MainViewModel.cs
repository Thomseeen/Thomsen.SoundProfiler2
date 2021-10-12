using Newtonsoft.Json;

using SoundProfiler2.Handler;
using SoundProfiler2.Models;
using SoundProfiler2.Views;

using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

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
        private const string DEFAULT_CONFIGURATION_FILEPATH = "configuration.json";

        private const float DEFAULT_VOLUME_INC = 0.05f;
        #endregion Private Constants

        #region Private Fields
        private bool isDisposed = false;
        private readonly Timer refreshTimer = new(500);

        private readonly object mixerApplicationsLock = new();
        private readonly object mappingsLock = new();

        private readonly List<GlobalHotKeyHandler> globalKeybindingHandlers = new();

        private ProfileModel activeProfile;

        private SoundProfilerConfigurationModel loadedConfiguration;
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

        private ICommand profileUpCommand;
        private ICommand profileDownCommand;

        private ICommand volumeUpCommand;
        private ICommand volumeDownCommand;

        private ICommand editKeybindingsCommand;
        private ICommand editMappingsCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public ProfileModel ActiveProfile {
            get => activeProfile;
            set { activeProfile = value; OnPropertyChanged(); }
        }

        public SoundProfilerConfigurationModel LoadedConfiguration {
            get => loadedConfiguration;
            set {
                loadedConfiguration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoadedProfiles));
                OnPropertyChanged(nameof(LoadedMappings));
                OnPropertyChanged(nameof(LoadedKeybindings));
                OnPropertyChanged(nameof(LoadedHiddenProgramsMapping));
            }
        }
        public CategoryMappingModel LoadedHiddenProgramsMapping {
            get => LoadedConfiguration.HiddenProgramsMapping;
            set { LoadedConfiguration.HiddenProgramsMapping = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ProfileModel> LoadedProfiles {
            get => LoadedConfiguration.Profiles;
            set { LoadedConfiguration.Profiles = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CategoryMappingModel> LoadedMappings {
            get => LoadedConfiguration.Mappings;
            set { LoadedConfiguration.Mappings = value; OnPropertyChanged(); }
        }
        public ObservableCollection<KeybindingModel> LoadedKeybindings {
            get => LoadedConfiguration.Keybindings;
            set { LoadedConfiguration.Keybindings = value; OnPropertyChanged(); }
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
        public ICommand TestCommand => testCommand ??= new CommandHandler(param => Test(param), () => true);

        public ICommand AddProfileCommand => addProfileCommand ??= new CommandHandler(param => AddProfile(), () => !IsProfileNameEditing);
        public ICommand RemoveProfileCommand => removeProfileCommand ??= new CommandHandler(param => RemoveProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand BeginRenameProfileCommand => beginRenameProfileCommand ??= new CommandHandler(param => BeginRenameProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand EndRenameProfileCommand => endRenameProfileCommand ??= new CommandHandler(param => EndRenameProfile(), () => IsProfileNameEditing);

        public ICommand ProfileUpCommand => profileUpCommand ??= new CommandHandler(param => ProfileUp(), () => true);
        public ICommand ProfileDownCommand => profileDownCommand ??= new CommandHandler(param => ProfileDown(), () => true);

        public ICommand VolumeUpCommand => volumeUpCommand ??= new CommandHandler(param => VolumeUp(param as string), () => true);
        public ICommand VolumeDownCommand => volumeDownCommand ??= new CommandHandler(param => VolumeDown(param as string), () => true);

        public ICommand EditKeybindingsCommand => editKeybindingsCommand ??= new CommandHandler(param => EditKeybindings(), () => true);
        public ICommand EditMappingsCommand => editMappingsCommand ??= new CommandHandler(param => EditMappings(), () => true);

        public Dictionary<string, ICommand> KeybindableCommands => new() {
            { nameof(ProfileUpCommand), ProfileUpCommand },
            { nameof(ProfileDownCommand), ProfileDownCommand },
            { nameof(VolumeUpCommand), VolumeUpCommand },
            { nameof(VolumeDownCommand), VolumeDownCommand }
        };
        #endregion Commands
        #endregion Public properties

        #region Constructors
        public MainViewModel() {
            LoadedConfiguration = ConfigurationHandler.ReadOrWriteDefaultConfiguration(DEFAULT_CONFIGURATION_FILEPATH, SoundProfilerConfigurationModel.GetDefaultModel());

            View = new MainView {
                DataContext = this
            };

            View.Loaded += View_Loaded;
        }
        #endregion Constructors

        #region Private Methods
        #region Sliders/Volumes Handling
        private async void RefreshAsync() {
            try {
                refreshTimer.Stop();
                await Task.Run(() => {
                    /* Lock so multiple firing events don't overwrite each other, causing duplicate entries */
                    lock (mixerApplicationsLock) {
                        /* Filter apps by hidden mapping */
                        MixerApplicationModel[] newApps = FilterMixerApps(CoreAudioHandler.GetMixerApplications());

                        /* Only add new apps and refresh volume on old ones */
                        MergeRefreshedAppsIntoActivesMixerApps(newApps);

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
            } catch (ObjectDisposedException) {
                /* Closing */
            }
        }

        private MixerApplicationModel[] FilterMixerApps(MixerApplicationModel[] mixerApplications) {
            return mixerApplications.Where(app => !LoadedHiddenProgramsMapping.Programs.Any(prog => app.UnifiedProcessName.Contains(prog.UnifiedName))).ToArray();
        }

        private void MergeRefreshedAppsIntoActivesMixerApps(MixerApplicationModel[] newMixerApplications) {
            foreach (MixerApplicationModel newApp in newMixerApplications) {
                if (!MixerApplications.Any(activeApp => activeApp.ProcessId == newApp.ProcessId)) {
                    newApp.PropertyChanged += MixerApplication_PropertyChanged;

                    SafeDispatcher.Invoke(() => {
                        MixerApplications.Add(newApp);
                    });
                } else {

                    SafeDispatcher.Invoke(() => {
                        MixerApplications.Single(activeApp => activeApp.ProcessId == newApp.ProcessId).VolumeLevel = newApp.VolumeLevel;
                    });
                }
            }

            /* Delete old ones */
            MixerApplications.Where(activeApp => !newMixerApplications.Any(newApp => newApp.ProcessId == activeApp.ProcessId)).ToList().ForEach(mixerApp => SafeDispatcher.Invoke(() => MixerApplications.Remove(mixerApp)));
        }

        private void MapCategoriesIntoActiveMixerApps(ObservableCollection<CategoryMappingModel> loadedMappings) {
            foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                CategoryMappingModel matchCategory = null;
                int matchScore = 0;
                foreach (CategoryMappingModel category in loadedMappings) {
                    foreach (ProgramModel program in category.Programs) {
                        if (mixerApplication.UnifiedProcessName.Contains(program.UnifiedName)) {
                            if (program.UnifiedName.Length > matchScore) {
                                matchCategory = category;
                                matchScore = program.UnifiedName.Length;
                            }
                        }
                    }
                }
                mixerApplication.Category = matchCategory?.Name ?? MixerApplicationModel.UNMAPPED_CATEGORY;
            }
        }

        private void ApplyProfileToActiveMixerApps(ProfileModel activeProfile) {
            foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                CategoryVolumeModel category = activeProfile.CategoryVolumes.FirstOrDefault(category => category.Name == mixerApplication.Category);
                if (category is not null) {
                    mixerApplication.VolumeLevel = category.Volume;
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
                   }).OrderBy(cat => cat.Name))
            };

            lock (mappingsLock) {
                LoadedProfiles.Add(newProfile);
                ActiveProfile = newProfile;
            }

            ConfigurationHandler.WriteConfiguration(LoadedConfiguration, DEFAULT_CONFIGURATION_FILEPATH);

            IsProfileNameEditing = true;
            SetTextBoxFocusAndCursor((View as MainView).profileRenameBox, true);
        }

        private void RemoveProfile() {
            lock (mappingsLock) {
                LoadedProfiles.Remove(ActiveProfile);
                ActiveProfile = LoadedProfiles.First();
            }

            ConfigurationHandler.WriteConfiguration(LoadedConfiguration, DEFAULT_CONFIGURATION_FILEPATH);
        }

        private void BeginRenameProfile() {
            IsProfileNameEditing = true;
            SetTextBoxFocusAndCursor((View as MainView).profileRenameBox, false);
        }

        private void EndRenameProfile() {
            IsProfileNameEditing = false;
            ConfigurationHandler.WriteConfiguration(LoadedConfiguration, DEFAULT_CONFIGURATION_FILEPATH);
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

        private void VolumeUp(string categoryName) {
            CategoryVolumeModel category = ActiveProfile.CategoryVolumes.SingleOrDefault(cat => cat.Name == categoryName);

            if (category is not null) {
                category.Volume += DEFAULT_VOLUME_INC;
            }
        }

        private void VolumeDown(string categoryName) {
            CategoryVolumeModel category = ActiveProfile.CategoryVolumes.SingleOrDefault(cat => cat.Name == categoryName);

            if (category is not null) {
                category.Volume -= DEFAULT_VOLUME_INC;
            }
        }
        #endregion Profile Handling

        #region Dialog Handling
        private void EditKeybindings() {
            using EditKeybindingsViewModel keybindingsDialog = new(LoadedKeybindings);
            bool? result = keybindingsDialog.ShowDialog();
            if (result.HasValue && result.Value) {
                /* Save changed mappings */
                LoadedKeybindings = keybindingsDialog.LoadedKeybindings;
                ConfigurationHandler.WriteConfiguration(LoadedConfiguration, DEFAULT_CONFIGURATION_FILEPATH);
            } else {
                /* Read old unchanged mappings from disk */
                LoadedKeybindings = ConfigurationHandler.ReadConfiguration<SoundProfilerConfigurationModel>(DEFAULT_CONFIGURATION_FILEPATH).Keybindings;
            }

            //ApplyLocalKeybindings();
            AppyGlobalKeybindings();
        }

        private void EditMappings() {
            lock (mappingsLock) {
                using EditMappingsViewModel mappingsDialog = new(LoadedHiddenProgramsMapping, LoadedMappings);
                bool? result = mappingsDialog.ShowDialog();
                if (result.HasValue && result.Value) {
                    /* Save changed mappings */
                    LoadedHiddenProgramsMapping = mappingsDialog.LoadedHiddenProgramsMapping;
                    LoadedMappings = mappingsDialog.LoadedMappings;
                    ConfigurationHandler.WriteConfiguration(LoadedConfiguration, DEFAULT_CONFIGURATION_FILEPATH);
                } else {
                    /* Read old unchanged mappings from disk */
                    LoadedHiddenProgramsMapping = ConfigurationHandler.ReadConfiguration<SoundProfilerConfigurationModel>(DEFAULT_CONFIGURATION_FILEPATH).HiddenProgramsMapping;
                    LoadedMappings = ConfigurationHandler.ReadConfiguration<SoundProfilerConfigurationModel>(DEFAULT_CONFIGURATION_FILEPATH).Mappings;
                }
            }
        }
        #endregion DialogHandling

        #region UI Handling
        private void View_Loaded(object sender, RoutedEventArgs e) {
            //ApplyLocalKeybindings();
            AppyGlobalKeybindings();

            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();
        }

        private static void SetTextBoxFocusAndCursor(TextBox textBox, bool selectAll) {
            textBox.Focus();
            /* #BUG: Not working as intended */
            textBox.Select(textBox.Text.Length, selectAll ? textBox.Text.Length : 0);
        }
        #endregion

        private void Test(object param) {
            GC.Collect();
        }

        private void ApplyLocalKeybindings() {
            View.InputBindings.Clear();

            foreach (KeybindingModel keybinding in LoadedKeybindings) {
                if (keybinding is CategoryKeybindingModel catKeybinding) {
                    View.InputBindings.Add(
                        new KeyBinding(
                            KeybindableCommands[catKeybinding.Name],
                            catKeybinding.Key,
                            catKeybinding.Modifier
                        ) { CommandParameter = catKeybinding.CategoryName }
                    );
                } else {
                    View.InputBindings.Add(
                        new KeyBinding(
                            KeybindableCommands[keybinding.Name],
                            keybinding.Key,
                            keybinding.Modifier
                        )
                    );
                }
            }
        }

        private void AppyGlobalKeybindings() {
            foreach (GlobalHotKeyHandler hotkey in globalKeybindingHandlers) {
                hotkey.Dispose();
            }

            globalKeybindingHandlers.Clear();

            foreach (KeybindingModel keybinding in LoadedKeybindings) {
                GlobalHotKeyHandler hotKey = new(keybinding.Modifier, keybinding.Key);

                if (keybinding is CategoryKeybindingModel catKeybinding) {
                    hotKey.Register(View, KeybindableCommands[catKeybinding.Name], catKeybinding.CategoryName);
                } else {
                    hotKey.Register(View, KeybindableCommands[keybinding.Name]);
                }

                globalKeybindingHandlers.Add(hotKey);
            }
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
                CoreAudioHandler.SetMixerApplicationVolume(mixerApplication);
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

                    foreach (GlobalHotKeyHandler hotkey in globalKeybindingHandlers) {
                        hotkey.Dispose();
                    }
                }

                ConfigurationHandler.WriteConfiguration(LoadedConfiguration, DEFAULT_CONFIGURATION_FILEPATH);

                isDisposed = true;

                base.Dispose(true);
            }
        }
        #endregion BaseViewModel
    }
}
