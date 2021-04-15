using Discord.WebSocket;

namespace Jimbot.Extensions {
    public static class DiscordApiExtensions {
        public static string GetDisplayName(this SocketGuildUser user) {
            return string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
        }
    }
}