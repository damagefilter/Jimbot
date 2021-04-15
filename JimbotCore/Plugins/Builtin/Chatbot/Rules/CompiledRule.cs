using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jimbot.Plugins.Builtin.Chatbot.Ai;

namespace Jimbot.Plugins.Builtin.Chatbot.Rules {
    /// <summary>
    /// This is the response rule data in a format
    /// that can be accessed quickly.
    /// </summary>
    public class CompiledRule {
        /// <summary>
        /// A number of messages to chose from when responding.
        /// These correlate to the mood.
        /// </summary>
        private readonly List<ResponseDescriptor> messages;

        /// <summary>
        /// Pre-compiled regex matching pattern sourced from the primary words pool
        /// in the chatter rules file.
        /// </summary>
        private readonly Regex primaryMatchPattern;

        /// <summary>
        /// Pre-compiled regex matching pattern sourced from the secondary words pool
        /// in the chatter rules file.
        /// </summary>
        private readonly Regex secondaryMatchPattern;

        private readonly int primaryMatches;
        private readonly int secondaryMatches;
        private readonly bool canIgnorePrimaryInConversation;
        private readonly bool allowMoodMismatch;
        private readonly Mood representedMood;

        private readonly Random random;


        private CompiledRule(Mood thisMood, List<ResponseDescriptor> messages, RawResponseNode node) {
            representedMood = thisMood;
            this.messages = messages;
            primaryMatchPattern = new Regex(string.Join("|", node.PrimaryWordPool), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            secondaryMatchPattern = new Regex(string.Join("|", node.SecondaryWordPool), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            primaryMatches = node.MinPrimaryMatches;
            secondaryMatches = node.MinSecondaryMatches;
            canIgnorePrimaryInConversation = node.CanIgnorePrimary;
            allowMoodMismatch = node.AllowNeutralAnswerInMoods;
            random = new Random();
        }

        /// <summary>
        /// Test if this compiled response is matching the message.
        /// That is match counts are
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="isInConversation"></param>
        /// <returns></returns>
        public bool Matches(string msg, bool isInConversation) {
            // todo: MatchCollection will have an effect on accumulated garbage, that needs addressing eventually.
            // but afaik this is, at least when using regex, the cheapest option to go for.
            // Future plan is to train a NN from the bot chatter file to sort out responses.
            // at that point we don't need to manually match. Maybe it's not faster but less GC intensive for sure.
            var matches = secondaryMatchPattern.Matches(msg);
            bool countMatches = matches.Count >= secondaryMatches;

            if (isInConversation && canIgnorePrimaryInConversation) {
                return countMatches;
            }

            var primary = primaryMatchPattern.Matches(msg);
            return primary.Count >= primaryMatches && countMatches;
        }

        public bool Matches(string msg, Mood mood, bool isInConversation) {
            if (representedMood != mood && !allowMoodMismatch) {
                return false;
            }
            // todo: MatchCollection will have an effect on accumulated garbage, that needs addressing eventually.
            // but afaik this is, at least when using regex, the cheapest option to go for.
            // Future plan is to train a NN from the bot chatter file to sort out responses.
            // at that point we don't need to manually match. Maybe it's not faster but less GC intensive for sure.
            var matches = secondaryMatchPattern.Matches(msg);
            bool countMatches = matches.Count >= secondaryMatches;

            if (isInConversation && canIgnorePrimaryInConversation) {
                return countMatches;
            }

            var primary = primaryMatchPattern.Matches(msg);
            return primary.Count >= primaryMatches && countMatches;
        }

        /// <summary>
        /// Returns a random message from the message pool.
        /// </summary>
        /// <returns></returns>
        public string GetRandomMessage() {
            int baseProbability = random.Next(1, 101); // chance that we respond with anything at all
            // -1 probability in nodes means it should always be considered.
            return GetWightedRandomItem(messages.Where(x => x.Probability >= baseProbability || x.Probability < 0), x => x.Probability < 0 ? 100 : x.Probability).Message;
        }

        // https://stackoverflow.com/a/9141878
        private T GetWightedRandomItem<T>(IEnumerable<T> itemsEnumerable, Func<T, int> weightKey) {
            var items = itemsEnumerable.ToList();

            var totalWeight = items.Sum(weightKey);
            var randomWeightedIndex = random.Next(totalWeight);
            var itemWeightedIndex = 0;
            foreach (var item in items) {
                itemWeightedIndex += weightKey(item);
                if (randomWeightedIndex <= itemWeightedIndex)
                    return item;
            }

            return default;
        }

        public static CompiledRule CompileForMood(Mood targetMood, RawResponseNode node, ChatbotConfig cfg) {
            List<ResponseDescriptor> responsesForMood = node.Responses.Where(x => x.Mood == targetMood).ToList();
            SubstituteBotNicks(cfg, responsesForMood);

            return responsesForMood.Count == 0 ? null : new CompiledRule(targetMood, responsesForMood, node);
        }

        private static void SubstituteBotNicks(ChatbotConfig cfg, List<ResponseDescriptor> descriptors) {
            List<ResponseDescriptor> replacements = new List<ResponseDescriptor>();
            for (int i = 0; i < descriptors.Count; i++) {
                var descriptor = descriptors[i];

                // if we find the occurence of bot nick placeholder,
                // remove it from the list and prepare replacement versions for each available bot nick.
                if (Regex.IsMatch(descriptor.Message, "({bot name})")) {
                    replacements.AddRange(cfg.BotNicks.Select(t =>  new ResponseDescriptor(descriptor, Regex.Replace(descriptor.Message, "({bot name})", t))));
                    descriptors.RemoveAt(i--);
                }
            }

            // then put in the replacement values that contain the concrete (ro)bot nicks
            descriptors.AddRange(replacements);
        }
    }
}