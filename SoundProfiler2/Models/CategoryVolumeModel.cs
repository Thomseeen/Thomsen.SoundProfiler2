
using Util.MVVM;

namespace SoundProfiler2.Models {
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
            set { volume = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Base Overrides
        public override string ToString() {
            return $"{Name} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}
