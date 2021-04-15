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

        public List<string> PrimaryWordPool { get; set; }

        public List<string> SecondaryWordPool { get; set; }

        public List<ResponseDescriptor> Responses { get; set; }

    }
}