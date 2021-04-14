using System.Linq;
using Discord.WebSocket;

namespace Jimbot.Discord {
    public static class Extensions {
        public static SocketRole GetRoleByName(this SocketGuild guild, string roleName) {
            return guild.Roles.FirstOrDefault(x => x.Name == roleName);
        }

        public static SocketGuildChannel GetChannelByName(this SocketGuild guild, string channelName) {
            return guild.Channels.FirstOrDefault(x => x.Name == channelName);
        }
    }
}