using System;
using Jimbot.Db;
using Jimbot.Di;
using Jimbot.Discord;
using Jimbot.Logging;

namespace Jimbot.Plugins.Builtin.HunterGraveyard {
    public class HunterGraveyardPlugin : Plugin {
        public override void ProvideResources(DiContainer diContainer) {
        }

        public override async void Enable(DiContainer diContainer) {
            var bot = diContainer.Get<DiscordBot>();
            var log = diContainer.Get<Logger>("plugin");
            
            try {
                log.Info("Registering Hunter Commands");
                await bot.RegisterCommands<HunterCommands>();
            }
            catch (Exception e) {
                log.Error("Failed to register hunter commands", e);
            }
        }

        public override void InstallRoutine(DiContainer di) {
            di.Get<DbRepository>().CreateOrMigrateTable<HunterGrave>();
        }

        public override void UpdateRoutine(string installedVersion, DiContainer di) {
            di.Get<DbRepository>().CreateOrMigrateTable<HunterGrave>();
        }

        protected override void InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "Chris";
            descriptor.Name = "Hunter Graveyard";
            descriptor.Version = "1.0.2";
        }
    }
}