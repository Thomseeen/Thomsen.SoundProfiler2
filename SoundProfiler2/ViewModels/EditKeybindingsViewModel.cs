using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using SoundProfiler2.Models;
using SoundProfiler2.Views;

using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class EditKeybindingsViewModel : BaseViewModel {
        #region Private Fields
        private ObservableCollection<KeybindingModel> loadedKeybindings;

        #region Commands
        private ICommand closeCommand;
        private ICommand saveCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public override string WindowTitle => $"Edit Keybindings";

        public ObservableCollection<KeybindingModel> LoadedKeybindings {
            get => loadedKeybindings;
            set { loadedKeybindings = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand CloseCommand => closeCommand ??= new CommandHandler(param => ExitDialog(false), () => true);
        public ICommand SaveCommand => saveCommand ??= new CommandHandler(param => ExitDialog(true), () => true);
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
