using Newtonsoft.Json;

namespace Jimbot.Config {
    /// <summary>
    /// This is NOT the data from App.config.
    /// This is a custom config file in user space.
    /// </summary>
    public class AppConfig : Configuration<AppConfig> {


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

        public override void Ensure() {
            if (string.IsNullOrEmpty(dbPath)) {
                dbPath = "config/database.db";
            }

            if (string.IsNullOrEmpty(pluginPath)) {
                pluginPath = "plugins/";
            }

            if (string.IsNullOrEmpty(botGame)) {
                botGame = "mit Bytes das Tango Game";
            }

            if (string.IsNullOrEmpty(botToken)) {
                botToken = "Get this from discord developer central.";
            }
        }
    }
}