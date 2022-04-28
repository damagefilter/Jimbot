using System;
using System.Threading.Tasks;
using Jimbot.Di;
using Jimbot.Discord;
using Jimbot.Logging;
using Jimbot.Plugins;
using Ninject;

namespace Jimbot {
    class Jimbo {
        public static void Main(string[] args) {
            LogManager.ConfigureLogger();
            Logger log = LogManager.GetLogger(typeof(Jimbo));
            DiContainer di = PrepareDi();
            
            // Start the damn thing in an async context
            try {
                new Jimbo().MainAsync(di, log).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                log.Fatal(e.Message, e);
            }


        }

        public async Task MainAsync(DiContainer di, Logger log) {
            await BootstrapPlugins(di.Get<PluginLoader>());
            var bot = di.Get<DiscordBot>();
            try {
                log.Info("Starting Bot");
                await bot.Run();
            }
            catch (Exception e) {
                log.Fatal("Something crashed the bot. We need to end it here, I'm afraid.", e);
            }
        }

        private static DiContainer PrepareDi() {
            var kernel = new StandardKernel(new BotInjectionModule());
            // kernel.Load(new BotInjectionModule());
            return kernel.Get<DiContainer>();
        }

        private static async Task BootstrapPlugins(PluginLoader pluginLoader) {
            await pluginLoader.LoadPlugins();
            pluginLoader.EnsureInstallations();
            pluginLoader.EnablePlugins();
        }
    }
}