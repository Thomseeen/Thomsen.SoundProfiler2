using System.Collections.Generic;
using System.IO;
using Util.MVVM;

namespace SoundProfiler2.Models {
    public class ProfilesModel : BaseModel {
        #region Private Fields
        private string filePath;

        private List<ProfileModel> profiles;
        #endregion Private Fields

        #region Properties
        public string FilePath {
            get => filePath;
            set { filePath = value; OnPropertyChanged(); }
        }

        public List<ProfileModel> Profiles {
            get => profiles;
            set { profiles = value; OnPropertyChanged(); }
        }
        #endregion Properties

        #region Base Overrides
        public override int GetHashCode() {
            return FilePath.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is ProfilesModel model &&
                   FilePath == model.FilePath;
        }

        public override string ToString() {
            return $"{Path.GetFileName(FilePath)} [{base.ToString()}]";
        }
        #endregion Base Overrides
    }

    public class ProfileModel : BaseModel {
        #region Private Fields
        private string name;

        private Dictionary<string, float> categoryVolumes;
        #endregion Private Fields

        #region Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public Dictionary<string, float> CategoryVolumes {
            get => categoryVolumes;
            set { categoryVolumes = value; OnPropertyChanged(); }
        }
        #endregion Properties

        #region Base Overrides
        public override int GetHashCode() {
            return name.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is ProfileModel model &&
                   Name == model.Name;
        }

        public override string ToString() {
            return $"{Name} [{base.ToString()}]";
        }
        #endregion Base Overrides
    }
}
