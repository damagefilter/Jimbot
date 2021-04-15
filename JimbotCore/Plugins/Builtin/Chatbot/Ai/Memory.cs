using System.Collections.Generic;
using System.Linq;
using Jimbot.Plugins.Builtin.Chatbot.Rules;
using Ninject;

namespace Jimbot.Plugins.Builtin.Chatbot.Ai {
    /// <summary>
    /// Remembers all our responses and the required preconditions to trigger them
    /// </summary>
    public class Memory {
        private Dictionary<Mood, List<CompiledRule>> responses;

        [Inject]
        public Memory(ChatbotConfig cfg, ChatbotRuleConfig rules) {
            responses = RuleCompiler.CompileRules(rules.responseRules, cfg);
        }

        /// <summary>
        /// Used inside a Conversation object to find rules for the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="idealMood"></param>
        /// <param name="conversationRunning"></param>
        /// <param name="rules"></param>
        /// <returns></returns>
        public int FindRules(string message, Mood idealMood, bool conversationRunning, CompiledRule[] rules) {
            var idealRules = responses[idealMood];
            var defaultRules = responses[Mood.Neutral];
            int maxRules = rules.Length;
            int currentRule = 0;

            if (idealRules.Count > 0) {
                foreach (var rule in idealRules.Where(rule => rule.Matches(message, conversationRunning))) {
                    rules[currentRule++] = rule;
                    if (currentRule >= maxRules) {
                        return maxRules;
                    }
                }
            }

            if (defaultRules.Count > 0 && currentRule < maxRules) {
                foreach (var rule in defaultRules.Where(rule => rule.Matches(message, conversationRunning))) {
                    rules[currentRule++] = rule;
                    if (currentRule >= maxRules) {
                        return maxRules;
                    }
                }
            }

            return currentRule;
        }
    }
}