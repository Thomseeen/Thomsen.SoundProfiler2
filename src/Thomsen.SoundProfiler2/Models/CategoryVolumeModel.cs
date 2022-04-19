
using Util.MVVM;

namespace Thomsen.SoundProfiler2.Models {
    public class CategoryVolumeModel : BaseModel {
        #region Private Fields
        private string name;

        private float volume;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public float Volume {
            get => volume;
            set { volume = value < 0 ? 0 : value > 1 ? 1 : value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Constructors
        public CategoryVolumeModel() { }

        public CategoryVolumeModel(string name, float volume) {
            Name = name;
            Volume = volume;
        }
        #endregion Constructors

        #region Base Overrides
        public override string ToString() {
            return $"{Name} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}
