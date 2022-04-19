using System.Windows.Input;

using Thomsen.SoundProfiler2.Models.Configuration;

using Util.MVVM;

namespace Thomsen.SoundProfiler2.Models {
    public class KeybindingModel : BaseModel, IConfigurationCollection {
        #region Private Fields
        private string _name;

        private Key _key;
        private ModifierKeys _modifier;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public Key Key {
            get => _key;
            set { _key = value; OnPropertyChanged(); }
        }

        public ModifierKeys Modifier {
            get => _modifier;
            set { _modifier = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Constructors
        public KeybindingModel() { }

        public KeybindingModel(string name, Key key, ModifierKeys modifier) {
            Name = name;
            Key = key;
            Modifier = modifier;
        }
        #endregion Constructors

        #region Public Methods
        public static KeybindingModel[] GetDefaultModels() {
            return new KeybindingModel[] {
                   new KeybindingModel() {
                        Name = "ProfileUpCommand",
                        Key = Key.Add,
                        Modifier = ModifierKeys.Control
                    },
                    new KeybindingModel() {
                        Name = "ProfileDownCommand",
                        Key = Key.Subtract,
                        Modifier = ModifierKeys.Control
                    },
                    new CategoryKeybindingModel() {
                        Name = "VolumeUpCommand",
                        Key = Key.NumPad7,
                        Modifier = ModifierKeys.Control,
                        CategoryName = "Communication"
                    },
                    new CategoryKeybindingModel() {
                        Name = "VolumeDownCommand",
                        Key = Key.NumPad4,
                        Modifier = ModifierKeys.Control,
                        CategoryName = "Communication"
                    },
                    new CategoryKeybindingModel() {
                        Name = "VolumeUpCommand",
                        Key = Key.NumPad8,
                        Modifier = ModifierKeys.Control,
                        CategoryName = "Game"
                    },
                    new CategoryKeybindingModel() {
                        Name = "VolumeDownCommand",
                        Key = Key.NumPad5,
                        Modifier = ModifierKeys.Control,
                        CategoryName = "Game"
                    },
                    new CategoryKeybindingModel() {
                        Name = "VolumeUpCommand",
                        Key = Key.NumPad9,
                        Modifier = ModifierKeys.Control,
                        CategoryName = "Multimedia"
                    },
                    new CategoryKeybindingModel() {
                        Name = "VolumeDownCommand",
                        Key = Key.NumPad6,
                        Modifier = ModifierKeys.Control,
                        CategoryName = "Multimedia"
                    }
            };
        }
        #endregion Public Methods

        #region Base Overrides
        public override string ToString() {
            return $"{Name}";
        }
        #endregion Base Overrides
    }

    public class CategoryKeybindingModel : KeybindingModel {
        #region Private Fields
        private string _categoryName;
        #endregion Private Fields

        #region Public Properties
        public string CategoryName {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Base Overrides
        public override string ToString() {
            return $"{Name}:{CategoryName}";
        }
        #endregion Base Overrides
    }
}