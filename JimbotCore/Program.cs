using System;
using System.Threading.Tasks;
using Jimbot.Di;
using Jimbot.Discord;
using Jimbot.Logging;
using Jimbot.Plugins;
using Ninject;

namespace Jimbot {
    internal class Program {
        public static void Main(string[] args) {
            LogManager.ConfigureLogger();
            Logger log = LogManager.GetLogger(typeof(Program));
            DiContainer di = PrepareDi();
            BootstrapPlugins(di);
            // Start the damn thing in an async context
            try {
                new Program().MainAsync(di, log).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                log.Fatal(e.Message, e);
            }


        }

        public async Task MainAsync(DiContainer di, Logger log) {
            var bot = di.Get<DiscordBot>();
            try {
                await bot.Run();
            }
            catch (Exception e) {
                log.Fatal("Something crashed the bot. We need to end it here, I'm afraid.", e);
            }
        }

        private static DiContainer PrepareDi() {
            var kernel = new StandardKernel(new BotInjectionModule());
            return kernel.Get<DiContainer>();
        }

        private static PluginLoader BootstrapPlugins(DiContainer di) {
            PluginLoader pluginLoader = di.Get<PluginLoader>();
            pluginLoader.LoadPlugins();
            pluginLoader.EnsureInstallations(di);
            pluginLoader.EnablePlugins(di);
            return pluginLoader;
        }
    }
}