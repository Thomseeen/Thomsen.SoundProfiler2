
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Thomsen.SoundProfiler2.Models.Configuration;

using Util.MVVM;

namespace Thomsen.SoundProfiler2.Models {
    public class ProfileModel : BaseModel, IConfigurationCollection {
        #region Private Fields
        private string name;

        private ObservableCollection<CategoryVolumeModel> categoryVolumes;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CategoryVolumeModel> CategoryVolumes {
            get => categoryVolumes;
            set { categoryVolumes = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Constructors
        public ProfileModel() { }

        public ProfileModel(string name, IEnumerable<CategoryVolumeModel> volumes) {
            Name = name;
            CategoryVolumes = new ObservableCollection<CategoryVolumeModel>(volumes);
        }
        #endregion Constructors

        #region Public Methods
        public static ProfileModel[] GetDefaultModels() {
            return new ProfileModel[] {
                   new ProfileModel() {
                        Name = "Default",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Communication",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Multimedia",
                                Volume = 1f
                            }
                        }
                    },
                    new ProfileModel() {
                        Name = "Focus",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Communication",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Multimedia",
                                Volume = 0f
                            }
                        }
                    },
                    new ProfileModel() {
                        Name = "Casual",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Communication",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 0.25f
                            },
                            new CategoryVolumeModel() {
                                Name = "Multimedia",
                                Volume = 0.25f
                            }
                        }
                    },
                    new ProfileModel() {
                        Name = "Immersion",
                        CategoryVolumes = new ObservableCollection<CategoryVolumeModel>() {
                            new CategoryVolumeModel() {
                                Name = "Communication",
                                Volume = 0.25f
                            },
                            new CategoryVolumeModel() {
                                Name = "Game",
                                Volume = 1f
                            },
                            new CategoryVolumeModel() {
                                Name = "Multimedia",
                                Volume = 0f
                            }
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
