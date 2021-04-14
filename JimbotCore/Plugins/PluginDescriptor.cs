namespace Jimbot.Plugins {
    public class PluginDescriptor {
        //private PropertiesFile canaryInf;

        public string Name {
            get;
            set;
        }

        public string Version {
            get;
            set;
        }

        public string Author {
            get;
            set;
        }

        public PluginState CurrentState {
            get;
            set;
        }

        public Plugin DescribedPlugin {
            get;
        }

        public PluginDescriptor(Plugin plugin) {
            this.DescribedPlugin = plugin;
            this.CurrentState = PluginState.Known;
        }
    }
}