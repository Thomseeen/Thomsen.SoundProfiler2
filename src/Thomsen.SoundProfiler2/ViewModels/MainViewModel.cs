using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Thomsen.SoundProfiler2.Handler;
using Thomsen.SoundProfiler2.Models;
using Thomsen.SoundProfiler2.Models.Configuration;
using Thomsen.SoundProfiler2.Properties;
using Thomsen.SoundProfiler2.Views;

using Thomsen.WpfTools.Util;
using Thomsen.WpfTools.Mvvm;

namespace Thomsen.SoundProfiler2.ViewModels {
    public class MainViewModel : BaseViewModel<MainView> {
        #region Private Constants
        private const string DEFAULT_CONFIGURATION_FILEPATH = "configuration.json";

        public const float DEFAULT_VOLUME_INC = 0.01f;
        #endregion Private Constants

        #region Private Fields
        private bool _disposed = false;
        private bool _shutingDown = false;
        private readonly System.Timers.Timer _refreshTimer = new(500);

        private readonly object _mixerApplicationsLock = new();
        private readonly object _mappingsLock = new();

        private readonly List<GlobalHotKeyHandler> _globalKeybindingHandlers = new();

        private string _currentTime = null!;

        private ProfileModel _activeProfile = null!;

        private SoundProfilerConfigurationModel _loadedConfiguration = null!;
        private ObservableCollection<MixerApplicationModel> _mixerApplications = new();

        private bool _isProfileNameEditing;

        #region Commands
        private ICommand? _refreshCommand;
        private ICommand? _exitCommand;
        private ICommand? _testCommand;

        private ICommand? _addProfileCommand;
        private ICommand? _removeProfileCommand;
        private ICommand? _beginRenameProfileCommand;
        private ICommand? _endRenameProfileCommand;

        private ICommand? _profileUpCommand;
        private ICommand? _profileDownCommand;

        private ICommand? _volumeUpCommand;
        private ICommand? _volumeDownCommand;

        private ICommand? _editKeybindingsCommand;
        private ICommand? _editMappingsCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public string CurrentTime {
            get => _currentTime;
            set {
                _currentTime = value; OnPropertyChanged();
            }
        }

        public ProfileModel ActiveProfile {
            get => _activeProfile;
            set {
                _activeProfile = value; OnPropertyChanged();
            }
        }

        public SoundProfilerConfigurationModel LoadedConfiguration {
            get => _loadedConfiguration;
            set {
                _loadedConfiguration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoadedProfiles));
                OnPropertyChanged(nameof(LoadedMappings));
                OnPropertyChanged(nameof(LoadedKeybindings));
                OnPropertyChanged(nameof(LoadedHiddenProgramsMapping));
            }
        }
        public CategoryMappingModel LoadedHiddenProgramsMapping {
            get => LoadedConfiguration.HiddenProgramsMapping;
            set {
                LoadedConfiguration.HiddenProgramsMapping = value; OnPropertyChanged();
            }
        }
        public ObservableCollection<ProfileModel> LoadedProfiles {
            get => LoadedConfiguration.Profiles;
            set {
                LoadedConfiguration.Profiles = value; OnPropertyChanged();
            }
        }
        public ObservableCollection<CategoryMappingModel> LoadedMappings {
            get => LoadedConfiguration.Mappings;
            set {
                LoadedConfiguration.Mappings = value; OnPropertyChanged();
            }
        }
        public ObservableCollection<KeybindingModel> LoadedKeybindings {
            get => LoadedConfiguration.Keybindings;
            set {
                LoadedConfiguration.Keybindings = value; OnPropertyChanged();
            }
        }
        public ObservableCollection<MixerApplicationModel> MixerApplications {
            get => _mixerApplications;
            set {
                _mixerApplications = value; OnPropertyChanged();
            }
        }
        public bool IsProfileNameEditing {
            get => _isProfileNameEditing;
            set {
                _isProfileNameEditing = value; OnPropertyChanged();
            }
        }


        #region Commands
        public ICommand RefreshCommand => _refreshCommand ??= new CommandHandler(param => RefreshAsync(), () => true);
        public ICommand ExitCommand => _exitCommand ??= new CommandHandler(param => Application.Current.Shutdown(), () => true);
        public ICommand TestCommand => _testCommand ??= new CommandHandler(param => Test(param!), () => true);

        public ICommand AddProfileCommand => _addProfileCommand ??= new CommandHandler(param => AddProfile(), () => !IsProfileNameEditing);
        public ICommand RemoveProfileCommand => _removeProfileCommand ??= new CommandHandler(param => RemoveProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand BeginRenameProfileCommand => _beginRenameProfileCommand ??= new CommandHandler(param => BeginRenameProfile(), () => ActiveProfile != null && !IsProfileNameEditing);
        public ICommand EndRenameProfileCommand => _endRenameProfileCommand ??= new CommandHandler(param => EndRenameProfile(), () => IsProfileNameEditing);

        public ICommand ProfileUpCommand => _profileUpCommand ??= new CommandHandler(param => ProfileUp(), () => true);
        public ICommand ProfileDownCommand => _profileDownCommand ??= new CommandHandler(param => ProfileDown(), () => true);

        public ICommand VolumeUpCommand => _volumeUpCommand ??= new CommandHandler(param => VolumeUp((param as string)!), () => true);
        public ICommand VolumeDownCommand => _volumeDownCommand ??= new CommandHandler(param => VolumeDown((param as string)!), () => true);

        public ICommand EditKeybindingsCommand => _editKeybindingsCommand ??= new CommandHandler(param => EditKeybindings(), () => true);
        public ICommand EditMappingsCommand => _editMappingsCommand ??= new CommandHandler(param => EditMappings(), () => true);

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
            string configPath = Settings.Default.LastConfigPath;

            if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath)) {
                _loadedConfiguration = ConfigurationHandler.ReadOrWriteDefaultConfiguration(configPath, SoundProfilerConfigurationModel.GetDefaultModel(configPath));
            } else {
                _loadedConfiguration = ConfigurationHandler.ReadOrWriteDefaultConfiguration(DEFAULT_CONFIGURATION_FILEPATH, SoundProfilerConfigurationModel.GetDefaultModel(DEFAULT_CONFIGURATION_FILEPATH));
            }

            string profileName = Settings.Default.LastProfileName;
            if (!string.IsNullOrEmpty(profileName) && LoadedConfiguration.Profiles.Any(profile => profile.Name == profileName)) {
                _activeProfile = LoadedConfiguration.Profiles.Single(profile => profile.Name == profileName);
            } else {
                _activeProfile = LoadedConfiguration.Profiles.First();
            }
        }
        #endregion Constructors

        #region Private Methods
        #region Sliders/Volumes Handling
        private async void RefreshAsync() {
            try {
                _refreshTimer.Stop();
                await Task.Run(() => {
                    /* Lock so multiple firing events don't overwrite each other, causing duplicate entries */
                    lock (_mixerApplicationsLock) {

                        /* Don't do anything if we are shutting down */
                        if (_shutingDown) {
                            return;
                        }

                        /* Filter apps by hidden mapping */
                        MixerApplicationModel[] newApps = FilterMixerApps(CoreAudioHandler.GetMixerApplications());

                        /* Only add new apps and refresh volume on old ones */
                        MergeRefreshedAppsIntoActivesMixerApps(newApps);

                        /* Map application category */
                        if (LoadedMappings is not null) {
                            MapCategoriesIntoActiveMixerApps(LoadedMappings);
                        }

                        /* Apply profile mix */
                        lock (_mappingsLock) {
                            if (LoadedProfiles is not null && LoadedProfiles?.Count > 0 && !IsProfileNameEditing) {
                                if (ActiveProfile is null) {
                                    ActiveProfile = LoadedProfiles.First();
                                }

                                ApplyProfileToActiveMixerApps(ActiveProfile);
                            }
                        }
                    }
                });
                _refreshTimer.Start();
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

                    SafeDispatcher.SafeInvoke(() => {
                        MixerApplications.Add(newApp);
                    });
                } else {

                    SafeDispatcher.SafeInvoke(() => {
                        MixerApplicationModel oldApp = MixerApplications.Single(activeApp => activeApp.ProcessId == newApp.ProcessId);
                        oldApp.VolumeLevel = newApp.VolumeLevel;
                        oldApp.ApplicationIcon = newApp.ApplicationIcon;
                        oldApp.FriendlyName = newApp.FriendlyName;
                        oldApp.ProcessName = newApp.ProcessName;
                    });
                }
            }

            /* Delete old ones */
            MixerApplications.Where(activeApp => !newMixerApplications.Any(newApp => newApp.ProcessId == activeApp.ProcessId)).ToList().ForEach(mixerApp => SafeDispatcher.SafeInvoke(() => MixerApplications.Remove(mixerApp)));
        }

        private void MapCategoriesIntoActiveMixerApps(ObservableCollection<CategoryMappingModel> loadedMappings) {
            foreach (MixerApplicationModel mixerApplication in MixerApplications) {
                CategoryMappingModel? matchCategory = null;
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
                CategoryVolumeModel? category = activeProfile.CategoryVolumes.FirstOrDefault(category => category.Name == mixerApplication.Category);
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

            lock (_mappingsLock) {
                LoadedProfiles.Add(newProfile);
                ActiveProfile = newProfile;
            }

            ConfigurationHandler.WriteConfiguration(LoadedConfiguration, LoadedConfiguration.Path);

            IsProfileNameEditing = true;
            if (_view is not null && _view is MainView mainView) {
                SetTextBoxFocusAndCursor(mainView.profileRenameBox, true);
            }
        }

        private void RemoveProfile() {
            lock (_mappingsLock) {
                LoadedProfiles.Remove(ActiveProfile);
                ActiveProfile = LoadedProfiles.First();
            }

            ConfigurationHandler.WriteConfiguration(LoadedConfiguration, LoadedConfiguration.Path);
        }

        private void BeginRenameProfile() {
            IsProfileNameEditing = true;
            if (_view is not null && _view is MainView mainView) {
                SetTextBoxFocusAndCursor(mainView.profileRenameBox, true);
            }
        }

        private void EndRenameProfile() {
            IsProfileNameEditing = false;
            ConfigurationHandler.WriteConfiguration(LoadedConfiguration, LoadedConfiguration.Path);
        }

        private void ProfileUp() {
            int index = LoadedProfiles.IndexOf(ActiveProfile) + 1;
            index = index >= LoadedProfiles.Count ? 0 : index;

            lock (_mappingsLock) {
                ActiveProfile = LoadedProfiles[index];
            }
        }

        private void ProfileDown() {
            int index = LoadedProfiles.IndexOf(ActiveProfile) - 1;
            index = index < 0 ? LoadedProfiles.Count - 1 : index;

            lock (_mappingsLock) {
                ActiveProfile = LoadedProfiles[index];
            }
        }

        private void VolumeUp(string categoryName) {
            CategoryVolumeModel? category = ActiveProfile.CategoryVolumes.SingleOrDefault(cat => cat.Name == categoryName);

            if (category is not null) {
                category.Volume += DEFAULT_VOLUME_INC;
            }
        }

        private void VolumeDown(string categoryName) {
            CategoryVolumeModel? category = ActiveProfile.CategoryVolumes.SingleOrDefault(cat => cat.Name == categoryName);

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
                ConfigurationHandler.WriteConfiguration(LoadedConfiguration, LoadedConfiguration.Path);
            } else {
                /* Read old unchanged mappings from disk */
                LoadedKeybindings = ConfigurationHandler.ReadConfiguration<SoundProfilerConfigurationModel>(LoadedConfiguration.Path).Keybindings;
            }

            //ApplyLocalKeybindings();
            AppyGlobalKeybindings();
        }

        private void EditMappings() {
            lock (_mappingsLock) {
                using EditMappingsViewModel mappingsDialog = new(LoadedHiddenProgramsMapping, LoadedMappings);
                bool? result = mappingsDialog.ShowDialog();
                if (result.HasValue && result.Value) {
                    /* Save changed mappings */
                    LoadedHiddenProgramsMapping = mappingsDialog.LoadedHiddenProgramsMapping;
                    LoadedMappings = mappingsDialog.LoadedMappings;
                    ConfigurationHandler.WriteConfiguration(LoadedConfiguration, LoadedConfiguration.Path);
                } else {
                    /* Read old unchanged mappings from disk */
                    LoadedHiddenProgramsMapping = ConfigurationHandler.ReadConfiguration<SoundProfilerConfigurationModel>(LoadedConfiguration.Path).HiddenProgramsMapping;
                    LoadedMappings = ConfigurationHandler.ReadConfiguration<SoundProfilerConfigurationModel>(LoadedConfiguration.Path).Mappings;
                }
            }
        }
        #endregion DialogHandling

        #region UI Handling
        protected override void View_Loaded(object sender, RoutedEventArgs e) {
            //ApplyLocalKeybindings();
            AppyGlobalKeybindings();

            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.Start();
        }

        private static void SetTextBoxFocusAndCursor(TextBox textBox, bool selectAll) {
            textBox.Focus();
            /* #BUG: Not working as intended */
            textBox.Select(textBox.Text.Length, selectAll ? textBox.Text.Length : 0);
        }
        #endregion

        private static void Test(object _) {
            GC.Collect();
        }

        private void AppyGlobalKeybindings() {
            foreach (GlobalHotKeyHandler hotkey in _globalKeybindingHandlers) {
                hotkey.Dispose();
            }

            _globalKeybindingHandlers.Clear();

            if (_view is null) {
                return;
            }

            foreach (KeybindingModel keybinding in LoadedKeybindings) {
                GlobalHotKeyHandler hotKey = new(keybinding.Modifier, keybinding.Key);

                if (keybinding is CategoryKeybindingModel catKeybinding) {
                    hotKey.Register(_view, KeybindableCommands[catKeybinding.Name], catKeybinding.CategoryName);
                } else {
                    hotKey.Register(_view, KeybindableCommands[keybinding.Name]);
                }

                _globalKeybindingHandlers.Add(hotKey);
            }
        }
        #endregion Private Methods

        #region Event Handler
        private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e) {
            RefreshAsync();

            SafeDispatcher.SafeInvoke(() => CurrentTime = DateTime.Now.ToString("H:mm d.M.yyyy"));
        }

        private void MixerApplication_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (sender is MixerApplicationModel mixerApplication && e.PropertyName == nameof(MixerApplicationModel.VolumeLevel)) {
                /* Stop timer so the value isn't refreshed while user adjusts values */
                _refreshTimer.Stop();
                CoreAudioHandler.SetMixerApplicationVolume(mixerApplication);
                _refreshTimer.Start();
            }
        }
        #endregion Event Handler

        #region BaseViewModel
        protected override void Dispose(bool disposing) {
            _shutingDown = true;
            Thread.Sleep(100);

            if (!_disposed) {
                if (disposing) {
                    _refreshTimer.Stop();
                    _refreshTimer.Dispose();

                    foreach (GlobalHotKeyHandler hotkey in _globalKeybindingHandlers) {
                        hotkey.Dispose();
                    }
                }

                ConfigurationHandler.WriteConfiguration(LoadedConfiguration, LoadedConfiguration.Path);

                Settings.Default.LastConfigPath = LoadedConfiguration.Path;
                Settings.Default.LastProfileName = ActiveProfile.Name;
                Settings.Default.Save();

                _disposed = true;

                base.Dispose(true);
            }
        }
        #endregion BaseViewModel
    }
}
