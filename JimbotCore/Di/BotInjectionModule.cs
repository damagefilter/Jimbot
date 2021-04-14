using Discord.Commands;
using Jimbot.Config;
using Jimbot.Db;
using Jimbot.Discord;
using Jimbot.Plugins;
using Ninject;
using Ninject.Modules;

namespace Jimbot.Di {
    public class BotInjectionModule : NinjectModule {
        public override void Load() {
            Bind<DiscordBot>().ToMethod(di => new DiscordBot(di.Kernel.Get<AppConfig>(), di.Kernel.Get<DiContainer>())).InSingletonScope();
            Bind<AppConfig>().ToMethod(di => AppConfig.Load()).InSingletonScope();
            Bind<DbRepository>().ToMethod(di => new DbRepository(di.Kernel.Get<AppConfig>())).InSingletonScope();
            Bind<PluginLoader>().ToMethod(di => new PluginLoader(di.Kernel.Get<DbRepository>(), di.Kernel.Get<AppConfig>(), di.Kernel.Get<DiscordBot>(), di.Kernel.Get<DiContainer>())).InSingletonScope();
            Bind<DiContainer>().ToMethod(di => new DiContainer(di.Kernel)).InSingletonScope();
        }
    }
}