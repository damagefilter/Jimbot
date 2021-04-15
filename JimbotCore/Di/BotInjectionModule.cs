using Jimbot.Config;
using Jimbot.Db;
using Jimbot.Discord;
using Jimbot.Logging;
using Jimbot.Plugins;
using Ninject.Modules;

namespace Jimbot.Di {
    public class BotInjectionModule : NinjectModule {
        public override void Load() {
            Bind<DiscordBot>().ToSelf().InSingletonScope();
            Bind<AppConfig>().ToMethod(di => AppConfig.Load()).InSingletonScope();
            Bind<DbRepository>().ToSelf().InSingletonScope();
            Bind<PluginLoader>().ToSelf().InSingletonScope();
            Bind<DiContainer>().ToMethod(di => new DiContainer(di.Kernel)).InSingletonScope();

            // some default logger channels
            Bind<Logger>().ToMethod(di => LogManager.GetLogger(typeof(Plugin))).InSingletonScope().Named("plugin");
            Bind<Logger>().ToMethod(di => LogManager.GetLogger(typeof(DbRepository))).InSingletonScope().Named("db");
            // todo: find a way to inject unnamed loggers as fallback option
            // Bind<Logger>().ToMethod(di => LogManager.GetLogger(typeof(Jimbo))).InSingletonScope();
        }
    }
}