using Newtonsoft.Json;
using SoundProfiler2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProfiler2.Handler {
    public class SettingsHandler {
        #region Public Methods
        public static void WriteSettings<T>(IEnumerable<T> settings, string filePath) {
            JsonSerializer jsonSerializer = new();

            using StreamWriter settingsFileWriter = new(filePath);
            using JsonTextWriter settingsJsonWriter = new(settingsFileWriter);
            jsonSerializer.Serialize(settingsJsonWriter, settings);
        }

        public static IEnumerable<T> ReadSettings<T>(string filePath) {
            JsonSerializer jsonSerializer = new();

            using StreamReader settingsFileReader = new(filePath);
            using JsonTextReader settingsJsonReader = new(settingsFileReader);
            return jsonSerializer.Deserialize<IEnumerable<T>>(settingsJsonReader);
        }

        public static IEnumerable<T> ReadOrWriteDefaultSettings<T>(string filePath, IEnumerable<T> defaults) {
            try {
                return ReadSettings<T>(filePath);
            } catch (Exception ex) when (ex is FileNotFoundException or JsonReaderException or JsonSerializationException) {
                /* Backup invalid file */
                if (File.Exists(filePath)) {
                    File.Move(filePath, $"{filePath}.dirty");
                }
                /* Create defaults */
                WriteSettings(defaults, filePath);
                return defaults;
            }
        }
        #endregion Public Methods
    }
}
