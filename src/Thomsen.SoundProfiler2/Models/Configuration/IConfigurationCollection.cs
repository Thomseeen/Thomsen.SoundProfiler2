
using System;
using System.Collections.Generic;

namespace Thomsen.SoundProfiler2.Models.Configuration {
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
