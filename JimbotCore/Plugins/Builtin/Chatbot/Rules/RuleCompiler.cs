using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jimbot.Plugins.Builtin.Chatbot.Ai;

namespace Jimbot.Plugins.Builtin.Chatbot.Rules {
    public static class RuleCompiler {
        public static Dictionary<Mood, List<CompiledRule>> CompileRules(IEnumerable<RawResponseNode> rawNodeList, ChatbotConfig cfg) {
            Dictionary<Mood, List<CompiledRule>> sortedResponses = new Dictionary<Mood, List<CompiledRule>> {
                {Mood.Neutral, new List<CompiledRule>()},
                {Mood.Friendly, new List<CompiledRule>()},
                {Mood.Angry, new List<CompiledRule>()}
            };
            // this will replace bot name placeholder with a range of actual bot nicks.
            // will work not so great if there is stuff like
            foreach (var node in rawNodeList) {
                if (node.PrimaryWordPool != null) {
                    SubstituteBotNicks(cfg, node.PrimaryWordPool);
                }

                if (node.SecondaryWordPool != null) {
                    SubstituteBotNicks(cfg, node.SecondaryWordPool);
                }

                var neutral = CompiledRule.CompileForMood(Mood.Neutral, node, cfg);
                if (neutral != null) {
                    sortedResponses[Mood.Neutral].Add(neutral);
                }

                var friendly = CompiledRule.CompileForMood(Mood.Friendly, node, cfg);
                if (friendly != null) {
                    sortedResponses[Mood.Friendly].Add(friendly);
                }

                var angry = CompiledRule.CompileForMood(Mood.Angry, node, cfg);
                if (angry != null) {
                    sortedResponses[Mood.Angry].Add(angry);
                }

            }

            return sortedResponses;
        }

        private static void SubstituteBotNicks(ChatbotConfig cfg, List<string> wordPool) {
            List<string> replacements = new List<string>();
            for (int i = 0; i < wordPool.Count; i++) {
                var keyword = wordPool[i];
                // if we find the occurence of bot nick placeholder,
                // remove it from the list and prepare replacement versions for each available bot nick.
                if (Regex.IsMatch(keyword, "({bot name})")) {
                    replacements.AddRange(cfg.BotNicks.Select(t => Regex.Replace(keyword, "({bot name})", t)));
                    wordPool.RemoveAt(i--);
                }
            }

            // then put in the replacement values that contain the concrete (ro)bot nicks
            wordPool.AddRange(replacements);
        }
    }
}