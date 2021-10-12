using SoundProfiler2.Models;
using SoundProfiler2.Views;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class EditMappingsViewModel : BaseViewModel {
        #region Private Fields
        private CategoryMappingModel loadedHiddenProgramsMapping;
        private ObservableCollection<CategoryMappingModel> loadedMappings;

        #region Commands
        private ICommand addProgramCommand;
        private ICommand removeProgramCommand;

        private ICommand closeCommand;
        private ICommand saveCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public override string WindowTitle => $"Edit Mappings";

        public CategoryMappingModel LoadedHiddenProgramsMapping {
            get => loadedHiddenProgramsMapping;
            set { loadedHiddenProgramsMapping = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CategoryMappingModel> LoadedMappings {
            get => loadedMappings;
            set { loadedMappings = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand AddProgramCommand => addProgramCommand ??= new CommandHandler(param => AddProgram(param as CategoryMappingModel), () => true);
        public ICommand RemoveProgramCommand => removeProgramCommand ??= new CommandHandler(param => RemoveProgram(param as ProgramModel), () => true);

        public ICommand CloseCommand => closeCommand ??= new CommandHandler(param => ExitDialog(false), () => true);
        public ICommand SaveCommand => saveCommand ??= new CommandHandler(param => ExitDialog(true), () => true);
        #endregion Commands
        #endregion Public Properties

        #region Constructors
        public EditMappingsViewModel(CategoryMappingModel hiddenProgrammMapping, IEnumerable<CategoryMappingModel> mappings) {
            LoadedHiddenProgramsMapping = hiddenProgrammMapping;
            LoadedMappings = new ObservableCollection<CategoryMappingModel>(mappings);

            View = new EditMappingsView {
                DataContext = this
            };
        }
        #endregion Constructors

        #region Private Methods
        private void AddProgram(CategoryMappingModel mapping) {
            mapping.Programs.Add(new ProgramModel("newprog"));
        }

        private void RemoveProgram(ProgramModel program) {
            if (LoadedHiddenProgramsMapping.Programs.Contains(program)) {
                LoadedHiddenProgramsMapping.Programs.Remove(program);
            } else {
                LoadedMappings.Single(mapping => mapping.Programs.Contains(program)).Programs.Remove(program);
            }
        }
        #endregion Private Methods
    }
}
