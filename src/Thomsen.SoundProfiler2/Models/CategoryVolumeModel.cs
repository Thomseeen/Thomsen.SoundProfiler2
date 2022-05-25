
using Thomsen.WpfTools.Mvvm;

namespace Thomsen.SoundProfiler2.Models {
    public class CategoryVolumeModel : BaseModel {
        #region Private Fields
        private string _name = null!;

        private float _volume;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => _name;
            set {
                _name = value; OnPropertyChanged();
            }
        }

        public float Volume {
            get => _volume;
            set {
                _volume = value < 0 ? 0 : value > 1 ? 1 : value; OnPropertyChanged();
            }
        }
        #endregion Public Properties

        #region Constructors
        public CategoryVolumeModel() {
        }

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
