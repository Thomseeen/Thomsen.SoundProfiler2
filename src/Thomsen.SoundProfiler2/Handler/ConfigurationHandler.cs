using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Thomsen.SoundProfiler2.Models.Configuration;

namespace Thomsen.SoundProfiler2.Handler {
    public static class ConfigurationHandler {
        #region Public Methods
        #region IConfiguration
        public static void WriteConfiguration<T>(T setting, string filePath) where T : IConfiguration {
            JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            using StreamWriter configurationFileWriter = new(filePath);
            using JsonTextWriter configurationJsonWriter = new(configurationFileWriter);
            jsonSerializer.Serialize(configurationJsonWriter, setting);
        }

        public static T ReadConfiguration<T>(string filePath) where T : IConfiguration {
            JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            using StreamReader configurationFileReader = new(filePath);
            using JsonTextReader configurationJsonReader = new(configurationFileReader);
            return jsonSerializer.Deserialize<T>(configurationJsonReader) ?? throw new InvalidDataException();
        }

        public static T ReadOrWriteDefaultConfiguration<T>(string filePath, T defaults) where T : IConfiguration {
            try {
                return ReadConfiguration<T>(filePath) ?? throw new InvalidDataException();
            } catch (Exception ex) when (ex is FileNotFoundException or JsonReaderException or JsonSerializationException or InvalidDataException) {
                /* Backup invalid file */
                if (File.Exists(filePath) && !File.Exists($"{filePath}.dirty")) {
                    File.Move(filePath, $"{filePath}.dirty", true);
                }
                /* Create defaults */
                WriteConfiguration(defaults, filePath);
                return defaults;
            }
        }
        #endregion IConfiguration

        #region IConfigurationCollection
        public static void WriteConfigurationCollection<T>(IEnumerable<T> settings, string filePath) where T : IConfigurationCollection {
            JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            using StreamWriter configurationFileWriter = new(filePath);
            using JsonTextWriter configurationJsonWriter = new(configurationFileWriter);
            jsonSerializer.Serialize(configurationJsonWriter, settings.OrderBy(setting => setting.Name).ToList());
        }

        public static IEnumerable<T> ReadConfigurationCollection<T>(string filePath) where T : IConfigurationCollection {
            JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            using StreamReader configurationFileReader = new(filePath);
            using JsonTextReader configurationJsonReader = new(configurationFileReader);
            return jsonSerializer.Deserialize<IEnumerable<T>>(configurationJsonReader)?.OrderBy(setting => setting.Name) ?? throw new InvalidDataException();
        }

        public static IEnumerable<T> ReadOrWriteDefaultConfigurationCollection<T>(string filePath, IEnumerable<T> defaults) where T : IConfigurationCollection {
            try {
                return ReadConfigurationCollection<T>(filePath) ?? throw new InvalidDataException();
            } catch (Exception ex) when (ex is FileNotFoundException or JsonReaderException or JsonSerializationException or InvalidDataException) {
                /* Backup invalid file */
                if (File.Exists(filePath)) {
                    File.Move(filePath, $"{filePath}.dirty", true);
                }
                /* Create defaults */
                WriteConfigurationCollection(defaults, filePath);
                return defaults;
            }
        }
        #endregion IConfigurationCollection
        #endregion Public Methods
    }
}
