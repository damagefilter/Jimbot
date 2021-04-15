using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jimbot.Plugins.Builtin.Chatbot.Ai {
    /// <summary>
    /// This is the data we read from bot chatter file.
    /// It comes as a list of a lot of these nodes.
    /// Each node covering a topic. It's unsorted and needs sorting out in a later step.
    /// </summary>
    public class RawResponseNode {
        public int MinPrimaryMatches { get; set; }

        public int MinSecondaryMatches { get; set; }

        public bool CanIgnorePrimary { get; set; }

        /// <summary>
        /// We have a cache of max 10 possible compiled responses.
        /// Each tied to a specific mood.
        /// With this option to true, we fill up mood answers (angry or friendly) with neutral ones
        /// to get a bigger possible answer pool. However, this is not always wanted so it can be turned off.
        /// </summary>
        public bool AllowNeutralAnswerInMoods { get; set; } = true;

        public List<string> PrimaryWordPool { get; set; }

        public List<string> SecondaryWordPool { get; set; }

        public List<ResponseDescriptor> Responses { get; set; }

    }
}