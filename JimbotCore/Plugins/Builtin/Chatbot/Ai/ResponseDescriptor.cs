using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jimbot.Plugins.Builtin.Chatbot.Ai {
    /// <summary>
    /// A simple class that contains a response to a given context
    /// and also some meta data.
    /// </summary>
    public class ResponseDescriptor {
        /// <summary>
        /// How probable is it that this response is fired?
        /// 0-100. If not set or -1, 100% probability is assumed.
        /// </summary>
        public int Probability { get; set; } = -1;

        /// <summary>
        /// The mood that this response reflects.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Mood Mood { get; set; }

        /// <summary>
        /// The message to send.
        /// </summary>
        public string Message { get; set; }

        public ResponseDescriptor() {

        }

        /// <summary>
        /// Copy constructor that replaces the message.
        /// Used for substituting bot name placeholders with concrete names.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="replacementMessage"></param>
        public ResponseDescriptor(ResponseDescriptor other, string replacementMessage) {
            Probability = other.Probability;
            Mood = other.Mood;
            Message = replacementMessage;
        }
    }
}