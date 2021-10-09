using SoundProfiler2.Models;
using SoundProfiler2.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MVVM;

namespace SoundProfiler2.ViewModels {
    public class EditMappingsViewModel : BaseViewModel {
        #region Private Fields
        private ObservableCollection<CategoryMappingModel> loadedMappings;
        #endregion Fields

        #region Public Properties
        public override string WindowTitle => $"Edit Mappings";

        public ObservableCollection<CategoryMappingModel> LoadedMappings {
            get => loadedMappings;
            set { loadedMappings = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Constructors
        public EditMappingsViewModel(IEnumerable<CategoryMappingModel> mappings) {
            LoadedMappings = new ObservableCollection<CategoryMappingModel>(mappings);

            View = new EditMappingsView {
                DataContext = this
            };
        }
        #endregion Constructors
    }
}
