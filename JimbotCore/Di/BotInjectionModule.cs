using Discord;
using Discord.Commands;
using Jimbot.Config;
using Jimbot.Db;
using Jimbot.Discord;
using Jimbot.Logging;
using Jimbot.Plugins;
using Ninject;
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
            Bind<Logger>().ToConstant(new Logger(log4net.LogManager.GetLogger("Main"))).InSingletonScope();
            Bind<Logger>().ToMethod(di => new Logger(log4net.LogManager.GetLogger("Plugins"))).InSingletonScope().Named("plugin");
            Bind<Logger>().ToMethod(di => new Logger(log4net.LogManager.GetLogger("Database"))).InSingletonScope().Named("db");
            
            // todo: find a way to inject unnamed loggers as fallback option
            // Bind<Logger>().ToMethod(di => new Logger(log4net.LogManager.GetLogger("Main")));
        }
    }
}