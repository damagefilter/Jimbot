using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jimbot.Db;
using Jimbot.Di;
using Jimbot.Discord;
using Jimbot.Logging;

namespace Jimbot.Plugins.Builtin.ReactionRole {
    public class ReactionRolePlugin : Plugin {
        private DbRepository dbRepo;
        private DiscordBot bot;
        private Logger log;
        public override void ProvideResources(DiContainer diContainer) {
            // we need to keep the state via this as singleton because discords command handler
            // instantiates new command instances each time, instead of recycling existing ones. Grah!
            diContainer.GetImplementation().Bind<MessageConfiguration>().ToSelf().InSingletonScope();
        }

        public override void Enable(DiContainer diContainer) {
            bot = diContainer.Get<DiscordBot>();
            dbRepo = diContainer.Get<DbRepository>();
            log = diContainer.Get<Logger>("plugin");

            bot.DiscordClient.ReactionAdded += OnReactionAdded;
            bot.DiscordClient.ReactionRemoved += OnReactionRemoved;
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) {
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
                    await guild.DownloadUsersAsync();
                    var user = guild.GetUser(reaction.UserId);
                    if (user == null) {
                        log.Warn("Cannot get user from channel. Well crap.");
                        return;
                    }
                    await user.RemoveRoleAsync(role);
                    log.Info($"Role {role.Name} removed from {user.Nickname}!");

                }
                catch (Exception e) {
                    log.Error($"Failed removing role. {e.Message}", e);
                }
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) {
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
                    log.Info($"Role {role.Name} added to {user.Nickname}!");

                }
                catch (Exception e) {
                    log.Error($"Failed adding role. {e.Message}", e);
                }
            }
        }

        public override void InstallRoutine(DiContainer di) {
            if (dbRepo == null) {
                dbRepo = di.Get<DbRepository>();
            }
            dbRepo.CreateOrMigrateTable<ReactiveMessage>();
        }

        public override void UpdateRoutine(string installedVersion, DiContainer di) {
            if (dbRepo == null) {
                dbRepo = di.Get<DbRepository>();
            }

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