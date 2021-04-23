using System.Collections.Generic;
using Jimbot.Plugins.Builtin.Chatbot.Ai;
using Ninject;

namespace Jimbot.Plugins.Builtin.Chatbot.Placeholder {
    /// <summary>
    /// Handles replacement of palceholders in messages
    /// </summary>
    public class PlaceholderHandler {
        private ChatbotConfig botCfg;
        private List<Provider> providers = new List<Provider>();


        [Inject]
        public PlaceholderHandler(ChatbotConfig cfg) {
            botCfg = cfg;
        }

        public void RegisterProvider(Provider p) {
            providers.Add(p);
        }

        public string Replace(string input, Conversation conversation) {
            foreach (var provider in providers) {
                input = provider.Replace(input, conversation);
            }
            return input;
        }

    }
}