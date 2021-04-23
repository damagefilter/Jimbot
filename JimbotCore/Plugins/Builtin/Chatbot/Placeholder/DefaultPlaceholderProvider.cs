using System;
using Jimbot.Extensions;
using Jimbot.Plugins.Builtin.Chatbot.Ai;

namespace Jimbot.Plugins.Builtin.Chatbot.Placeholder {
    public class DefaultPlaceholderProvider : Provider {
        private ChatbotConfig botCfg;

        public DefaultPlaceholderProvider(ChatbotConfig cfg) {
            botCfg = cfg;
        }
        public override string Replace(string input, Conversation conversation) {
            input = input.Replace("{user}", conversation.ConversationPartner.GetDisplayName());
            input = input.Replace("{time}", DateTime.Now.ToString(botCfg.TimeFormat));
            input = input.Replace("{date}", DateTime.Now.ToString(botCfg.DateFormat));
            return input;
        }
    }
}