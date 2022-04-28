using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jimbot.Db;
using Jimbot.Di;
using Jimbot.Discord;
using Jimbot.Extensions;
using Jimbot.Logging;
using Ninject;

namespace Jimbot.Plugins.Builtin.ReactionRole {
    public class ReactionRolePlugin : Plugin {
        private DbRepository dbRepo;
        private DiscordBot bot;
        private Logger log;

        public ReactionRolePlugin([Named("plugin")]Logger log) {
            this.log = log;
        }
        public override void ProvideResources(DiContainer diContainer) {
            // we need to keep the state via this as singleton because discords command handler
            // instantiates new command instances each time, instead of recycling existing ones. Grah!
            diContainer.GetImplementation().Bind<MessageConfiguration>().ToSelf().InSingletonScope();
        }

        public override async void Enable(DiContainer diContainer) {
            bot = diContainer.Get<DiscordBot>();
            dbRepo = diContainer.Get<DbRepository>();
            log = diContainer.Get<Logger>("plugin");

            bot.DiscordClient.ReactionAdded += OnReactionAdded;
            bot.DiscordClient.ReactionRemoved += OnReactionRemoved;

            try {
                log.Info("Registering Reaction Control Commands");
                await bot.RegisterCommands<ReactionControlCommands>();
            }
            catch (Exception e) {
                log.Error("Failed to register reaction control commands", e);
            }
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
            string emote = reaction.Emote.Name;
            string msgIdString = message.Id.ToString();
            string channelIdString = channel.Id.ToString();
            var msg = dbRepo.FindOne<ReactiveMessage>(x => x.MessageId == msgIdString && x.ChannelId == channelIdString && x.Emote == emote);
            if (msg == null) {
                log.Warn($"Reaction removed: {emote} from message {msgIdString} - but no message found with this ID ...");
                return;
            }

            try {
                if (!(bot.DiscordClient.Guilds.FirstOrDefault(x => x.Id == msg.GuildIdNum) is SocketGuild guild)) {
                    log.Warn($"Message from unknown server ...???");
                    return;
                }

                // var guild = bot.DiscordClient.Guilds.FirstOrDefault(x => x.Id == msg.GuildIdNum);
                var role = guild.GetRole(msg.DiscordRoleIdNum);
                await guild.DownloadUsersAsync();
                var user = guild.GetUser(reaction.UserId);
                if (user == null) {
                    log.Warn("Cannot get user from channel. Well crap.");
                    return;
                }
                await user.RemoveRoleAsync(role);
                log.Info($"Role {role.Name} removed from {user.GetDisplayName()}!");

            }
            catch (Exception e) {
                log.Error($"Failed removing role. {e.Message}", e);
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
            // if (!reaction.User.IsSpecified) {
            //     Console.WriteLine("No user. can't do.");
            //     return;
            // }
            string emote = reaction.Emote.Name;
            var msg = dbRepo.FindOne<ReactiveMessage>(x => x.Emote == emote);
            if (msg == null) {
                return;
            }

            if (msg.MessageIdNum == message.Id && msg.ChannelIdNum == reaction.Channel.Id && reaction.Emote.Name == msg.Emote) {
                try {
                    if (!(bot.DiscordClient.Guilds.FirstOrDefault(x => x.Id == msg.GuildIdNum) is SocketGuild guild)) {
                        return;
                    }

                    // var guild = bot.DiscordClient.Guilds.FirstOrDefault(x => x.Id == msg.GuildIdNum);
                    var role = guild.GetRole(msg.DiscordRoleIdNum);

                    var user = guild.GetUser(reaction.UserId);
                    if (user == null) {
                        log.Warn("Cannot get user from channel. Will try to download user list. this may hang ...");
                        await guild.DownloadUsersAsync();
                        user = guild.GetUser(reaction.UserId);
                        if (user == null) {
                            log.Error("User still null. Unable to retrieve user.");
                            return;
                        }
                    }
                    await user.AddRoleAsync(role);
                    log.Info($"Role {role.Name} added to {user.GetDisplayName()}!");

                }
                catch (Exception e) {
                    log.Error($"Failed adding role. {e.Message}", e);
                }
            }
        }

        public override void InstallRoutine(DiContainer di) {
            dbRepo ??= di.Get<DbRepository>();
            dbRepo.CreateOrMigrateTable<ReactiveMessage>();
        }

        public override void UpdateRoutine(string installedVersion, DiContainer di) {
            dbRepo ??= di.Get<DbRepository>();

            if (installedVersion != localDescriptor.Version) {
                dbRepo.CreateOrMigrateTable<ReactiveMessage>();
            }
        }

        protected override void InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "Chris";
            descriptor.Name = "Reaction Roles";
            descriptor.Version = "1.0";
        }
    }
}