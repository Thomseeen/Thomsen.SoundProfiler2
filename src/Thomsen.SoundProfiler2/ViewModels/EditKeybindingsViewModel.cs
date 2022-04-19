using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Thomsen.SoundProfiler2.Models;
using Thomsen.SoundProfiler2.Views;

using Util.MVVM;

namespace Thomsen.SoundProfiler2.ViewModels {
    public class EditKeybindingsViewModel : BaseViewModel {
        #region Private Fields
        private ObservableCollection<KeybindingModel> _loadedKeybindings;

        #region Commands
        private ICommand _closeCommand;
        private ICommand _saveCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public override string WindowTitle => $"Edit Keybindings";

        public ObservableCollection<KeybindingModel> LoadedKeybindings {
            get => _loadedKeybindings;
            set { _loadedKeybindings = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand CloseCommand => _closeCommand ??= new CommandHandler(param => ExitDialog(false), () => true);
        public ICommand SaveCommand => _saveCommand ??= new CommandHandler(param => ExitDialog(true), () => true);
        #endregion Commands
        #endregion Public Properties

        #region Constructors
        public EditKeybindingsViewModel(IEnumerable<KeybindingModel> keybindings) {
            LoadedKeybindings = new ObservableCollection<KeybindingModel>(keybindings);

            View = new EditKeybindingsView {
                DataContext = this
            };
        }
        #endregion Constructors
    }
}
