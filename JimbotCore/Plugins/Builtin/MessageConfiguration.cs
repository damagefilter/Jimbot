using System.Collections.Generic;
using Discord;

namespace Jimbot.Plugins.Builtin {
    public struct EmoteAndRole {
        public IEmote emote;
        public bool isUnicode;
        public ulong roleId;

        public string EmoteDisplay => isUnicode ? emote.Name : $":{emote.Name}:";
    }

    public enum ConfigurationState {
        None,
        ConfigActive
    }
    public class MessageConfiguration {
        public ConfigurationState currentState;
        public ulong channelId;
        public ulong guildId;
        public List<EmoteAndRole> emoteToRoleMapping = new List<EmoteAndRole>(3);
        public string message; // temporary field until we get the message ID
        public ulong messageId;
    }
}