using Newtonsoft.Json;
using SoundProfiler2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProfiler2.Handler {
    public static class SettingsHandler {
        #region Public Methods
        public static void WriteSettings<T>(IEnumerable<T> settings, string filePath) where T : ISetting {
            JsonSerializer jsonSerializer = new();

            using StreamWriter settingsFileWriter = new(filePath);
            using JsonTextWriter settingsJsonWriter = new(settingsFileWriter);
            jsonSerializer.Serialize(settingsJsonWriter, settings);
        }

        public static IEnumerable<T> ReadSettings<T>(string filePath) where T : ISetting {
            JsonSerializer jsonSerializer = new();

            using StreamReader settingsFileReader = new(filePath);
            using JsonTextReader settingsJsonReader = new(settingsFileReader);
            return jsonSerializer.Deserialize<IEnumerable<T>>(settingsJsonReader);//.OrderBy(setting => setting.Name);
        }

        public static IEnumerable<T> ReadOrWriteDefaultSettings<T>(string filePath, IEnumerable<T> defaults) where T : ISetting {
            try {
                return ReadSettings<T>(filePath) ?? throw new InvalidDataException();
            } catch (Exception ex) when (ex is FileNotFoundException or JsonReaderException or JsonSerializationException or InvalidDataException) {
                /* Backup invalid file */
                if (File.Exists(filePath)) {
                    File.Move(filePath, $"{filePath}.dirty", true);
                }
                /* Create defaults */
                WriteSettings(defaults, filePath);
                return defaults;
            }
        }
        #endregion Public Methods
    }
}
