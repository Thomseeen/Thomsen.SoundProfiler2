using System.Collections.ObjectModel;

using Thomsen.WpfTools.Mvvm;

namespace Thomsen.SoundProfiler2.Models.Configuration {
    public class SoundProfilerConfigurationModel : BaseModel, IConfiguration {
        #region Private Fields
        private string _name = null!;
        private string _path = null!;

        private ObservableCollection<ProfileModel> _profiles = null!;
        private ObservableCollection<CategoryMappingModel> _mappings = null!;
        private ObservableCollection<KeybindingModel> _keybindings = null!;

        private CategoryMappingModel _hiddenProgramsMapping = null!;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }
        public string Path {
            get => _path;
            set { _path = value; OnPropertyChanged(); }
        }

        public CategoryMappingModel HiddenProgramsMapping {
            get => _hiddenProgramsMapping;
            set { _hiddenProgramsMapping = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ProfileModel> Profiles {
            get => _profiles;
            set { _profiles = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CategoryMappingModel> Mappings {
            get => _mappings;
            set { _mappings = value; OnPropertyChanged(); }
        }
        public ObservableCollection<KeybindingModel> Keybindings {
            get => _keybindings;
            set { _keybindings = value; OnPropertyChanged(); }
        }
        #endregion Public properties

        #region Public Methods
        public static SoundProfilerConfigurationModel GetDefaultModel(string path) {
            return new SoundProfilerConfigurationModel {
                Name = "default",
                Path = path,
                HiddenProgramsMapping = new CategoryMappingModel("Hidden", new ObservableCollection<ProgramModel>() {
                        new ProgramModel("rtx"),
                        new ProgramModel("explorer")
                }),
                Profiles = new ObservableCollection<ProfileModel>(ProfileModel.GetDefaultModels()),
                Mappings = new ObservableCollection<CategoryMappingModel>(CategoryMappingModel.GetDefaultModels()),
                Keybindings = new ObservableCollection<KeybindingModel>(KeybindingModel.GetDefaultModels())
            };
        }
        #endregion Public Methods

        #region Base Overrides
        public override string ToString() {
            return $"{Name} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}
