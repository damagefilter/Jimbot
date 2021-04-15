using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jimbot.Extensions;
using Jimbot.Logging;
using Jimbot.Plugins.Builtin.Chatbot.Rules;
using Ninject;

namespace Jimbot.Plugins.Builtin.Chatbot.Ai {
    public class Conversation {
        private readonly Memory memory;

        private readonly Logger log;
        private SocketUser user;
        private CompiledRule[] ruleCache = new CompiledRule[10];

        /// <summary>
        /// Ranges from 0 to 1 where each third of the spectrum accounts for one
        /// part of the mood spectrum (angry, neutral, friendly)
        /// </summary>
        private float currentMoodValue;

        /// <summary>
        /// Time we last responded to our conversation partner.
        /// </summary>
        private DateTime lastSpokenTo;

        private readonly Regex negativeWordMatchPattern;
        private readonly Regex positiveWordMatchPattern;
        private bool conversationRunning;
        private Random random;

        [Inject]
        public Conversation(Memory memory, ChatbotConfig config, [Named("plugin")]Logger log) {
            this.memory = memory;
            currentMoodValue = .5f;
            negativeWordMatchPattern = new Regex(string.Join("|", config.NegativeWords), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            positiveWordMatchPattern = new Regex(string.Join("|", config.PositiveWords), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.log = log;
            random = new Random();
        }

        public void SetUser(SocketUser user) {
            this.user = user;
        }

        public async Task HandleMessage(SocketMessage incomingMessage) {

            if (incomingMessage == null) {
                return;
            }
            float originalMood = currentMoodValue; // need for resetting in case we get a derp and update a message we dont respond to.
            UpdateConversationMood(incomingMessage.Content);

            var numRules = memory.FindRules(incomingMessage.Content, GetConversationMood(), conversationRunning, ruleCache);
            if (numRules == 0) {
                // reset mood because this was likely not directed at the bot
                currentMoodValue = originalMood;
                return;
            }
            if (!conversationRunning) {
                log.Info("Conversation with " + user.GetDisplayName() + " is marked running.");
                conversationRunning = true;
            }
            // we use this to make the bot forget it was spoken to in order to not drag a conversation along for hours.
            if (lastSpokenTo != DateTime.MinValue && (DateTime.Now - lastSpokenTo).Minutes > 2) {
                log.Info("Conversation with " + user.GetDisplayName() + " has timed out. Minutes value is " + (DateTime.Now - lastSpokenTo).Minutes);
                conversationRunning = false;
            }
            lastSpokenTo = DateTime.Now;
            await SendResponse(ruleCache[random.Next(0, numRules)], incomingMessage.Channel);
        }

        private async Task SendResponse(CompiledRule rule, ISocketMessageChannel channel) {
            var msg = rule.GetRandomMessage();
            if (msg == null) {
                return;
            }

            // looks as if the bot is typing
            var isTypingState = channel.EnterTypingState();
            // calculate some average typing speed
            // float kph = 15000; // 15k keystrokes per hour
            // float kpm = kph / 60; // keystrokes per minute
            // float kps = kpm / 60; // keystrokes per second.
            float keysPerSec = 6.0f;
            float timeToWait = (1f / keysPerSec) * msg.Length;

            await Task.Delay(TimeSpan.FromSeconds(timeToWait));
            isTypingState.Dispose();
            await channel.SendMessageAsync(msg);
        }

        private void UpdateConversationMood(string message) {
            float badMatches = .01f * negativeWordMatchPattern.Matches(message).Count;
            float goodMatches = .01f * positiveWordMatchPattern.Matches(message).Count;

            currentMoodValue = Math.Clamp(currentMoodValue + (goodMatches - badMatches), 0f, 1f) ;
        }

        private Mood GetConversationMood() {
            if (currentMoodValue > .66f) {
                return Mood.Friendly;
            }

            if (currentMoodValue > .33f) {
                return Mood.Neutral;
            }

            return Mood.Angry;
        }

    }
}