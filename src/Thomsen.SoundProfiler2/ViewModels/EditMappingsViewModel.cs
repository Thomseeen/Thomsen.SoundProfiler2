using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Thomsen.SoundProfiler2.Models;
using Thomsen.SoundProfiler2.Views;

using Thomsen.WpfTools.Mvvm;

namespace Thomsen.SoundProfiler2.ViewModels {
    public class EditMappingsViewModel : BaseViewModel<EditMappingsView> {
        #region Private Fields
        private CategoryMappingModel _loadedHiddenProgramsMapping = null!;
        private ObservableCollection<CategoryMappingModel> _loadedMappings = null!;

        #region Commands
        private ICommand? _addProgramCommand;
        private ICommand? _removeProgramCommand;

        private ICommand? _closeCommand;
        private ICommand? _saveCommand;
        #endregion Commands
        #endregion Private Fields

        #region Public Properties
        public CategoryMappingModel LoadedHiddenProgramsMapping {
            get => _loadedHiddenProgramsMapping;
            set {
                _loadedHiddenProgramsMapping = value; OnPropertyChanged();
            }
        }
        public ObservableCollection<CategoryMappingModel> LoadedMappings {
            get => _loadedMappings;
            set {
                _loadedMappings = value; OnPropertyChanged();
            }
        }

        #region Commands
        public ICommand AddProgramCommand => _addProgramCommand ??= new CommandHandler(param => AddProgram((param as CategoryMappingModel)!), () => true);
        public ICommand RemoveProgramCommand => _removeProgramCommand ??= new CommandHandler(param => RemoveProgram((param as ProgramModel)!), () => true);

        public ICommand CloseCommand => _closeCommand ??= new CommandHandler(param => ExitDialog(false), () => true);
        public ICommand SaveCommand => _saveCommand ??= new CommandHandler(param => ExitDialog(true), () => true);
        #endregion Commands
        #endregion Public Properties

        #region Constructors
        public EditMappingsViewModel(CategoryMappingModel hiddenProgrammMapping, IEnumerable<CategoryMappingModel> mappings) {
            LoadedHiddenProgramsMapping = hiddenProgrammMapping;
            LoadedMappings = new ObservableCollection<CategoryMappingModel>(mappings);
        }
        #endregion Constructors

        #region Private Methods
        private static void AddProgram(CategoryMappingModel mapping) {
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
