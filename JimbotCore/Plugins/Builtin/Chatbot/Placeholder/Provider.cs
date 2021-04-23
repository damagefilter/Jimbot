using Jimbot.Plugins.Builtin.Chatbot.Ai;

namespace Jimbot.Plugins.Builtin.Chatbot.Placeholder {
    public abstract class Provider {
        public abstract string Replace(string input, Conversation conversation);
    }
}