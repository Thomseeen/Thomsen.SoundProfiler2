using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

using Util.MVVM;

namespace SoundProfiler2.Models {
    public class KeybindingModel : BaseModel, ISetting {
        #region Private Fields
        private string name;

        private Key key;
        private ModifierKeys modifier;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public Key Key {
            get => key;
            set { key = value; OnPropertyChanged(); }
        }

        public ModifierKeys Modifier {
            get => modifier;
            set { modifier = value; OnPropertyChanged(); }
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
            return $"{Name} - {base.ToString()}";
        }
        #endregion Base Overrides
    }

    public class CategoryKeybindingModel : KeybindingModel {
        #region Private Fields
        private string categoryName;
        #endregion Private Fields

        #region Public Properties
        public string CategoryName {
            get => categoryName;
            set { categoryName = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Base Overrides
        public override string ToString() {
            return $"{Name}:{CategoryName} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}