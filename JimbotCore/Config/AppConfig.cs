using System.IO;
using Jimbot.Tools;
using Newtonsoft.Json;

namespace Jimbot.Config {
    /// <summary>
    /// This is NOT the data from App.config.
    /// This is a custom config file in user space.
    /// </summary>
    public class AppConfig {
        private const string FilePath = "config/env.json";

        [JsonProperty]
        private string dbPath;
        [JsonProperty]
        private string pluginPath;
        [JsonProperty]
        private string botToken = null;
        [JsonProperty]
        private string botGame;

        [JsonIgnore]
        public string DbPath => dbPath;

        [JsonIgnore]
        public string PluginPath => pluginPath;

        [JsonIgnore]
        public string BotToken => botToken;

        [JsonIgnore]
        public string BotGame => botGame;

        private void Ensure() {
            if (string.IsNullOrEmpty(dbPath)) {
                dbPath = "config/database.db";
            }

            if (string.IsNullOrEmpty(pluginPath)) {
                pluginPath = "plugins/";
            }

            if (string.IsNullOrEmpty(botGame)) {
                botGame = "mit Bytes das Tango Game";
            }
        }

        public void Save() {
            IoHelper.EnsureFileAndPath(FilePath);
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static AppConfig Load() {
            IoHelper.EnsureFileAndPath(FilePath);

            var obj = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(FilePath)) ?? new AppConfig();
            obj.Ensure();
            return obj;
        }
    }
}