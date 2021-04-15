using System.IO;
using Jimbot.Tools;
using Newtonsoft.Json;

namespace Jimbot.Config {
    /// <summary>
    /// This is the base for a configuration file that offers out of the box save/load features.
    /// This is basically a json object so make use of newtonsoft json attributes to control
    /// exactly how you want your configuration data to be stored.
    /// </summary>
    /// <typeparam name="T">The implementing type</typeparam>
    public abstract class Configuration<T> : IConfiguration where T : IConfiguration, new() {
        private const string ConfigPath = "config/";


        public abstract void Ensure();
        // public abstract IConfiguration GetTemplate();

        public void Save() {
            string path = Path.Combine(ConfigPath, GetType().FullName + ".json");
            IoHelper.EnsureFileAndPath(path);
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }


        public static T Load() {
            // I love late static binding
            var type = typeof(T);
            string path = Path.Combine(ConfigPath, type.FullName + ".json");
            IoHelper.EnsureFileAndPath(path);

            T obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) ?? default;
            if (obj == null) {
                // write out a default version of this config
                obj = new T();
                obj.Ensure();
                obj.Save();
            }
            else {
                obj.Ensure();
            }

            return obj;
        }
    }
}