using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jimbot.Config;
using Jimbot.Db;
using Jimbot.Di;
using Jimbot.Logging;
using System.Linq;
using System.Threading.Tasks;
using Jimbot.Discord;
using Ninject;

namespace Jimbot.Plugins {
    /// <summary>
    /// Reads a pre-configured path for plugin assemblies.
    /// Loads the assemblies.
    /// </summary>
    public class PluginLoader {
        // public const string pluginPath = "plugins"; // relative path to plugins (from where the exe is executed)

        private List<Plugin> plugins;

        private Logger log;
        private DbRepository database;
        private AppConfig cfg;
        private DiscordBot discordBot;
        private DiContainer di;

        [Inject]
        public PluginLoader(DbRepository db, AppConfig cfg, DiscordBot bot, DiContainer di, [Named("plugin")]Logger log) {
            plugins = new List<Plugin>();
            this.log = log;
            database = db;
            discordBot = bot;
            this.cfg = cfg;
            this.di = di;
        }

        public async Task LoadPlugins(string pluginPath = null) {
            // When we run as service, we must specify an absolute path because
            // the service working directory is not the directory where the actual executable is
            if (string.IsNullOrEmpty(pluginPath)) {
                pluginPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), cfg.PluginPath);
            }

            //var pluginPath = "plugins";
            log.Info("*** Loading plugins ...");
            if (plugins != null && plugins.Count > 0) {
                throw new PluginLoaderException("Plugins appear to be loaded already.");
            }
            plugins = new List<Plugin>();

            if (!Directory.Exists(pluginPath)) {
                log.Info($"Plugins directory ({pluginPath}) doesn't exist. Creating new one.");
                Directory.CreateDirectory(pluginPath);
            }
            Type pluginType = typeof(Plugin);

            var builtInPlugins = Assembly.GetExecutingAssembly().GetTypes().Where(cls => pluginType.IsAssignableFrom(cls) && !cls.IsAbstract && !cls.IsInterface).ToList();

            foreach (var type in builtInPlugins) {
                try {
                    
                    var instance = (Plugin)di.Get(type);
                    if (instance == null) {
                        log.Error($"Loading plugin {type} failed!");
                        continue;
                    }
                    log.Info($"Loading plugin {type} from built in resource");
                    plugins.Add(instance);
                    instance.ProvideResources(di);
                }
                catch (Exception e) {
                    log.Error($"Loading plugin {type} from built in resource", e);
                }
            }

            foreach (var file in Directory.EnumerateFiles(pluginPath, "*.dll")) {
                // load the dll
                var dll = Assembly.LoadFrom(file);

                // Scan the dll for stuff implementing Plugins.

                List<Type> pluginTypes = dll.GetTypes().Where(cls => pluginType.IsAssignableFrom(cls) && !cls.IsAbstract && !cls.IsInterface).ToList();

                foreach (var type in pluginTypes) {
                    try {
                        var instance = (Plugin)di.Get(type);
                        if (instance == null) {
                            log.Error($"Loading plugin {type} failed!");
                            continue;
                        }
                        log.Info($"Loading plugin {type} from {file}");
                        plugins.Add(instance);
                        instance.ProvideResources(di);
                    }
                    catch (Exception e) {
                        log.Error($"Loading plugin {type} from {file} failed.", e);
                    }
                }
                log.Info($"Registering commands from {file}");
                await discordBot.Commands.AddModulesAsync(dll, di.GetImplementation().Kernel);
            }
        }

        public void EnsureInstallations() {
            log.Info("*** Ensuring Plugin installations ...");
            database.CreateOrMigrateTable<DbPlugin>();
            try {
                database.BeginTransaction();
                foreach (Plugin plugin in plugins) {
                    var descriptor = plugin.GetDescriptor();

                    var databasePluginData = database.FindOne<DbPlugin>(x => x.Name == descriptor.Name);
                    if (databasePluginData == null) {
                        plugin.InstallRoutine(di);
                        databasePluginData = new DbPlugin {
                            Name = descriptor.Name,
                            Author = descriptor.Author,
                            Version = descriptor.Version
                        };
                        if (database.Insert(databasePluginData)) {
                            log.Info($"{descriptor.Name} installed ...");
                        }
                        else {
                            log.Warn($"{descriptor.Name} installation failed ...");
                        }
                        
                    }
                    else {
                        if (descriptor.Version != databasePluginData.Version) {
                            plugin.UpdateRoutine(databasePluginData.Version, di);
                            log.Info($"{descriptor.Name} updated from {databasePluginData.Version} to {descriptor.Version}");
                        }
                        else {
                            log.Info($"{descriptor.Name} does not need updating");
                        }
                    }
                }
                database.CommitTransaction();
            }
            catch (Exception e) {
                log.Error("Failed to ensure plugin installations", e);
                database.Rollback();
            }
        }

        public void EnablePlugins() {
            log.Info("*** Enabling plugins ...");
            foreach (var plugin in plugins) {
                log.Info("Enabling " + plugin.GetDescriptor().Name);
                plugin.Enable(di);
            }
        }
    }
}