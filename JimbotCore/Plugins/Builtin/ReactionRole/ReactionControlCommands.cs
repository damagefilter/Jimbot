using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jimbot.Db;
using Jimbot.Discord;

namespace Jimbot.Plugins.Builtin.ReactionRole {
    /// <summary>
    ///
    /// </summary>
    public class ReactionControlCommands : ModuleBase<SocketCommandContext> {

        private readonly MessageConfiguration msgCfg;
        private DiscordBot bot;
        private DbRepository repo;

        public ReactionControlCommands(MessageConfiguration msgCfg, DiscordBot bot, DbRepository repo) {
            this.msgCfg = msgCfg;
            this.bot = bot;
            this.repo = repo;
        }

        [Command("jrr begin")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BeginCommand() {
            if (msgCfg.currentState == ConfigurationState.None) {
                msgCfg.currentState = ConfigurationState.ConfigActive;
                await ReplyAsync("Okay! Neue Role Reaction Message!");
            }
            else {
                await ReplyAsync("Da läuft schon eine Reaction Message config, hömma! Nutze `jrr purge` um die zu löschen.");
            }
        }

        [Command("jrr add")]
        [Summary("Usage: jrr add <emote> <role name>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddCommand([Remainder]string txt) {
            if (msgCfg.currentState == ConfigurationState.None) {
                await ReplyAsync("Da läuft gerade keine Config. Mach ne neue mit `jrr begin`!");
                return;
            }

            string[] args = txt.Split(' ');
            if (args.Length < 2) {
                await ReplyAsync("Da fehlt mir info. Ich will einen emote und eine Rolle!");
                return;
            }

            // This is a discord emote
            IEmote emote;
            bool isUnicode;
            if (Emote.TryParse(args[0], out Emote result)) {
                emote = result;
                isUnicode = false;
            }
            else {
                emote = new Emoji(args[0]);
                isUnicode = true;
            }

            var role = Context.Guild.GetRoleByName(args[1]);
            if (role == null) {
                await ReplyAsync($"Ich hab keine Rolle mit dem namen {args[1]} gefunden.");
                return;
            }
            msgCfg.emoteToRoleMapping.Add(new EmoteAndRole {
                emote = emote,
                roleId = role.Id,
                isUnicode = isUnicode
            });
            await ReplyAsync($"Alles klar. {emote} zur Rolle {args[1]}! Verwende `jrr commit` um die config abzuschließen.");

        }

        [Command("jrr channel")]
        [Summary("Usage: jrr channel <channel name>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChannelCommand(string channelName) {
            if (msgCfg.currentState == ConfigurationState.None) {
                await ReplyAsync("Da läuft gerade keine Config. Mach ne neue mit `jrr begin`!");
                return;
            }

            Match match = Regex.Match(channelName, "<#(\\d+)>");
            SocketGuildChannel channel;
            if (match.Success) {
                channel = Context.Guild.GetChannel(ulong.Parse(match.Groups[1].Value));
            }
            else {
                channel = Context.Guild.GetChannelByName(channelName);
            }

            if (channel == null) {
                await ReplyAsync($"Der channel {channelName} ist mir völlig unbekannt. Wo soll das sein?");
                return;
            }

            msgCfg.channelId = channel.Id;
            msgCfg.guildId = Context.Guild.Id;
            await ReplyAsync($"Message wird im Channel {channel.Name} abgesetzt. Verwende `jrr commit` um die config abzuschließen.");
        }

        [Command("jrr message")]
        [Summary("Usage: jrr message <channel name>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task MessageCommand([Remainder] string channelName) {
            if (msgCfg.currentState == ConfigurationState.None) {
                await ReplyAsync("Da läuft gerade keine Config. Mach ne neue mit `jrr begin`!");
                return;
            }

            msgCfg.message = channelName.Trim();
            // gott replace the emotes
            msgCfg.message = Regex.Replace(msgCfg.message, "<[a:]+(.+?):>", ":$1:");
            await ReplyAsync($"Message gesichert. Verwende `jrr commit` um die config abzuschließen.");
        }

        [Command("jrr commit")]
        [Summary("Usage: jrr message <channel name>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CommitCommand() {
            if (msgCfg.currentState == ConfigurationState.None) {
                await ReplyAsync("Da läuft gerade keine Config. Mach ne neue mit `jrr begin`!");
                return;
            }

            if (string.IsNullOrEmpty(msgCfg.message) || msgCfg.channelId == 0 || msgCfg.emoteToRoleMapping.Count == 0) {
                await ReplyAsync("Ich hab noch nicht genug info um das Ding zu commiten. Nutze `jrr review` um zu schauen wo's brennt.");
                return;
            }

            // // todo: remove this when we're done. doesn't need checking the schema quite so often, normally
            // repo.CreateOrMigrateTable<ReactiveMessage>();


            if (!(bot.DiscordClient.GetChannel(msgCfg.channelId) is ISocketMessageChannel channel)) {
                Console.WriteLine("provided channel is not a socket message channel");
            }
            else {
                var message = await channel.SendMessageAsync(msgCfg.message);
                foreach (var reaction in msgCfg.emoteToRoleMapping) {
                    await message.AddReactionAsync(reaction.emote);
                    var dbMessage = new ReactiveMessage();
                    dbMessage.Emote = reaction.emote.Name;
                    dbMessage.IsUnicodeEmote = reaction.isUnicode;
                    dbMessage.ChannelIdNum = msgCfg.channelId;
                    dbMessage.MessageIdNum = message.Id;
                    dbMessage.DiscordRoleIdNum = reaction.roleId;
                    dbMessage.GuildIdNum = msgCfg.guildId;
                    repo.Insert(dbMessage);
                }
            }

        }

        [Command("jrr review")]
        [Summary("Usage: jrr review")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ReviewCommand() {
            if (msgCfg.currentState == ConfigurationState.None) {
                await ReplyAsync("Da läuft gerade keine Config. Mach ne neue mit `jrr begin`!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            string channelName;
            if (msgCfg.channelId > 0) {
                channelName = Context.Guild.GetChannel(msgCfg.channelId).Name;
            }
            else {
                channelName = "keiner";
            }

            sb.AppendLine($"** Ziel Channel: ** {channelName}");
            sb.AppendLine("** Emotes -> Rolle **");
            foreach (var emoteAndRole in msgCfg.emoteToRoleMapping) {
                var role = Context.Guild.GetRole(emoteAndRole.roleId);
                sb.AppendLine($"{emoteAndRole.emote} -> {role.Name}");
            }

            string msg = string.IsNullOrEmpty(msgCfg.message) ? "keine" : msgCfg.message;
            sb.AppendLine($"** Nachricht: **\n{msg}\n");

            await ReplyAsync(sb.ToString());
        }

        [Command("jrr list")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListCommand() {
            var messages = repo.FindAll<ReactiveMessage>(x => true);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**Known Role Reaction Messages:**");
            for (int i = 0; i < messages.Count; ++i) {
                var msg = messages[i];
                sb.AppendLine($"**-------- Message --------**");
                sb.AppendLine($"**ID: **{msg.Id}");
                sb.AppendLine($"**Message ID **{msg.MessageId}");
                sb.AppendLine($"**Channel ID **{msg.ChannelId}");
                var channel = bot.DiscordClient.GetChannel(msg.ChannelIdNum);
                if (channel is IMessageChannel msgChannel) {
                    var targetMessage = await msgChannel.GetMessageAsync(msg.MessageIdNum);
                    sb.AppendLine($"**Content:**\n {targetMessage.Content.Substring(0, Math.Max(Math.Min(150, targetMessage.Content.Length), targetMessage.Content.Length / 3))} ...");
                }
                else {
                    sb.AppendLine($"**Content:**\n could not find message in channel.");
                }
            }

            await ReplyAsync(sb.ToString());
        }

        [Command("jrr purge")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PurgeCommand() {
            if (msgCfg.currentState == ConfigurationState.None) {
                await ReplyAsync("Gibt nichts zu löschen.");
            }
            else {
                msgCfg.currentState = ConfigurationState.None;
                msgCfg.channelId = 0;
                msgCfg.messageId = 0;
                msgCfg.emoteToRoleMapping.Clear();
                await ReplyAsync("Gelöscht. Mach eine neuen Reaction Message mit `jrr beging`");
            }
        }

        [Command("jrr remove")]
        [Summary("Usage: jrr remove <message id>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveCommand([Remainder ]string messageId) {

            if (!ulong.TryParse(messageId, out ulong id)) {
                await ReplyAsync("Das war keine Nummer von der ich was wüsste. Und ich kenn all die Nummern!");
                return;
            }

            var toRemove = repo.FindAll<ReactiveMessage>(x => x.MessageId == messageId);
            if (toRemove.Count == 0) {
                await ReplyAsync("Zu dieser message ID gibt es nichts zu löschen.");
                return;
            }

            var channel = Context.Client.GetChannel(toRemove[0].ChannelIdNum) as IMessageChannel;
            if (channel == null) {
                await ReplyAsync($"Ah, irgendwas mit dem Channel ({toRemove[0].ChannelId}) war nicht in Ordnung ... das ging daneben.");
                return;
            }

            var chanMsg = await channel.GetMessageAsync(toRemove[0].MessageIdNum);
            if (chanMsg == null) {
                await ReplyAsync($"Ah, irgendwas mit der Message ({toRemove[0].MessageId}) war nicht in Ordnung ... das ging daneben.");
                return;
            }

            for (int i = 0; i < toRemove.Count; i++) {
                repo.Delete(toRemove[i]);
            }

            await channel.DeleteMessageAsync(chanMsg);
            await ReplyAsync("Alles zu dieser message gelöscht.");
        }
    }
}