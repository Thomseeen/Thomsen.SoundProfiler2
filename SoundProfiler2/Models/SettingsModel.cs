using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

using Util.MVVM;

namespace SoundProfiler2.Models {
    public class SettingsModel : BaseModel {
        #region Private Fields
        private string filePath;

        private ObservableCollection<CategoryMappingModel> categoryMappings;
        #endregion Private Fields

        #region Properties
        public string FilePath {
            get => filePath;
            set { filePath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CategoryMappingModel> CategoryMappings {
            get => categoryMappings;
            set { categoryMappings = value; OnPropertyChanged(); }
        }
        #endregion Properties

        #region Base Overrides
        public override int GetHashCode() {
            return FilePath.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is SettingsModel model &&
                   FilePath == model.FilePath;
        }

        public override string ToString() {
            return $"{Path.GetFileName(FilePath)} [{base.ToString()}]";
        }
        #endregion Base Overrides
    }

    public class CategoryMappingModel : BaseModel {
        #region Private Fields
        private string name;
        private ObservableCollection<string> programs;
        #endregion Private Fields

        #region Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Programs {
            get => programs;
            set { programs = value; OnPropertyChanged(); }
        }
        #endregion Properties

        #region Base Overrides
        public override int GetHashCode() {
            return name.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is CategoryMappingModel model &&
                   Name == model.Name;
        }

        public override string ToString() {
            return $"{Name} [{base.ToString()}]";
        }
        #endregion Base Overrides
    }
}
