
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Thomsen.SoundProfiler2.Models.Configuration;

using Util.MVVM;

namespace Thomsen.SoundProfiler2.Models {
    public class CategoryMappingModel : BaseModel, IConfigurationCollection {
        #region Private Fields
        private string name;
        private ObservableCollection<ProgramModel> programs;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProgramModel> Programs {
            get => programs;
            set { programs = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Constructors
        public CategoryMappingModel() { }

        public CategoryMappingModel(string name, IEnumerable<ProgramModel> programs) {
            Name = name;
            Programs = new ObservableCollection<ProgramModel>(programs);
        }
        #endregion Constructors

        #region Public Methods
        public static CategoryMappingModel[] GetDefaultModels() {
            return new CategoryMappingModel[]  {
                new CategoryMappingModel() {
                    Name = "Communication",
                    Programs = new ObservableCollection<ProgramModel>() {
                        new ProgramModel("teamspeak"),
                        new ProgramModel("ts3"),
                        new ProgramModel("discord"),
                        new ProgramModel("teams"),
                        new ProgramModel("slack"),
                        new ProgramModel("skype")
                    }
                },
                new CategoryMappingModel() {
                    Name = "Game",
                    Programs = new ObservableCollection<ProgramModel>() {
                        new ProgramModel("dota"),
                        new ProgramModel("hunt"),
                        new ProgramModel("ageofempires"),
                        new ProgramModel("trackmania"),
                        new ProgramModel("newworld")
                    }
                },
                new CategoryMappingModel() {
                    Name = "Multimedia",
                    Programs = new ObservableCollection<ProgramModel>() {
                        new ProgramModel("firefox"),
                        new ProgramModel("edge"),
                        new ProgramModel("chrome"),
                        new ProgramModel("spotify"),
                        new ProgramModel("itunes"),
                        new ProgramModel("vlc")
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
