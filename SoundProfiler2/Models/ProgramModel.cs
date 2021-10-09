
using Util.MVVM;

namespace SoundProfiler2.Models {
    public class ProgramModel : BaseModel {
        #region Private Fields
        private string name;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => name;
            set {
                name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnifiedName));
            }
        }

        public string UnifiedName => Name.ToLowerInvariant().Replace(" ", "");
        #endregion Public Properties

        #region Constructor
        public ProgramModel() { }

        public ProgramModel(string name) {
            Name = name;
        }
        #endregion Constructor

        #region Base Overrides
        public override string ToString() {
            return $"{Name} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}
