using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProfiler2.Models {
    public interface IConfigurationCollection {
        #region Public Properties
        public string Name { get; set; }
        #endregion Public Properties

        #region Public Methods
        public static IEnumerable<IConfigurationCollection> GetDefaultModels() {
            return Array.Empty<IConfigurationCollection>();
        }
        #endregion Public Methods
    }
}
