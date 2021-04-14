using SQLite;

namespace Jimbot.Plugins.Builtin.ReactionRole {
    /// <summary>
    /// There can be many with the same message ID
    /// but different emote IDs
    /// </summary>
    public class ReactiveMessage {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Emote { get; set; }

        public bool IsUnicodeEmote { get; set; }

        // These are ulong normally but sqlite-net doesn't do ulong
        public string ChannelId { get; set; }

        public string MessageId { get; set; }

        public string GuildId { get; set; }

        public string DiscordRoleId { get; set; }


        [Ignore]
        public ulong DiscordRoleIdNum {
            get => ulong.Parse(DiscordRoleId);
            set => DiscordRoleId = value.ToString();
        }

        [Ignore]
        public ulong ChannelIdNum {
            get => ulong.Parse(ChannelId);
            set => ChannelId = value.ToString();
        }

        [Ignore]
        public ulong GuildIdNum {
            get => ulong.Parse(GuildId);
            set => GuildId = value.ToString();
        }

        [Ignore]
        public ulong MessageIdNum {
            get => ulong.Parse(MessageId);
            set => MessageId = value.ToString();
        }
    }
}