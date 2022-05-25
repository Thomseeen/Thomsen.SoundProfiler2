using System;

namespace Thomsen.SoundProfiler2.Models.Configuration {
    public interface IConfiguration {
        #region Public Properties
        public string Name { get; set; }
        #endregion Public Properties

        #region Public Methods
        public static IConfiguration GetDefaultModel() {
            throw new NotImplementedException();
        }
        #endregion Public Methods
    }
}
