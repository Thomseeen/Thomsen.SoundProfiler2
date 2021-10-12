﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MVVM;

namespace SoundProfiler2.Models {
    public class SoundProfilerConfigurationModel : BaseModel, IConfiguration {
        #region Private Fields
        private string name;

        private ObservableCollection<ProfileModel> profiles;
        private ObservableCollection<CategoryMappingModel> mappings;
        private ObservableCollection<KeybindingModel> keybindings;

        private CategoryMappingModel hiddenProgramsMapping;
        #endregion Private Fields

        #region Public Properties
        public string Name {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public CategoryMappingModel HiddenProgramsMapping {
            get => hiddenProgramsMapping;
            set { hiddenProgramsMapping = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ProfileModel> Profiles {
            get => profiles;
            set { profiles = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CategoryMappingModel> Mappings {
            get => mappings;
            set { mappings = value; OnPropertyChanged(); }
        }
        public ObservableCollection<KeybindingModel> Keybindings {
            get => keybindings;
            set { keybindings = value; OnPropertyChanged(); }
        }
        #endregion Public properties

        #region Public Methods
        public static SoundProfilerConfigurationModel GetDefaultModel() {
            return new SoundProfilerConfigurationModel {
                Name = "default",
                HiddenProgramsMapping = new CategoryMappingModel("Hidden", new ObservableCollection<ProgramModel>() {
                        new ProgramModel("rtx"),
                        new ProgramModel("explorer")
                }),
                Profiles = new ObservableCollection<ProfileModel>(ProfileModel.GetDefaultModels()),
                Mappings = new ObservableCollection<CategoryMappingModel>(CategoryMappingModel.GetDefaultModels()),
                Keybindings = new ObservableCollection<KeybindingModel>(KeybindingModel.GetDefaultModels())
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