using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jimbot.Di;
using Jimbot.Discord;
using Jimbot.Logging;
using Jimbot.Plugins.Builtin.Chatbot.Ai;
using Ninject;

namespace Jimbot.Plugins.Builtin.Chatbot {
    public class ConversationHandler {
        private Logger log;
        private DiContainer di; // todo: replace with conversation factory to avoid having another di root

        private Dictionary<ulong, Conversation> conversations = new();

         [Inject]
        public ConversationHandler([Named("plugin")]Logger log, DiscordBot bot, DiContainer di) {
            this.log = log;
            this.di = di;

            bot.DiscordClient.MessageReceived += HandleChatMessage;
        }

        private async Task HandleChatMessage(SocketMessage msg) {
            if (msg.Author.IsBot || !(msg is SocketUserMessage message) || message.Channel is IDMChannel) {
                return;
            }

            // ignore commands
            if (message.Content == null || message.Content.StartsWith("!")) {
                return;
            }

            if (!conversations.ContainsKey(message.Author.Id)) {
                var convo = di.Get<Conversation>();
                convo.SetUser(message.Author);
                conversations.Add(message.Author.Id, convo);
            }

            await conversations[message.Author.Id].HandleMessage(msg);
        }
    }
}