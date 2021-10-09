using System.Collections.ObjectModel;

using Util.MVVM;

namespace SoundProfiler2.Models {
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

        #region Public Methods
        public static CategoryMappingModel[] GetDefaultModels() {
            return new CategoryMappingModel[]  {
                new CategoryMappingModel() {
                    Name = "Multimedia",
                    Programs = new ObservableCollection<string>() {
                        "firefox", "edge", "chrome", "spotify", "itunes", "vlc"
                    }
                },
                new CategoryMappingModel() {
                    Name = "Communication",
                    Programs = new ObservableCollection<string>() {
                        "teamspeak", "ts3", "discord", "teams", "slack", "skype"
                    }
                },
                new CategoryMappingModel() {
                    Name = "Game",
                    Programs = new ObservableCollection<string>() {
                        "dota", "hunt", "ageofempires", "trackmania", "new world"
                    }
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
}
