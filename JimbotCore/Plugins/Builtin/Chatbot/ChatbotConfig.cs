using System.Collections.Generic;
using Jimbot.Config;

namespace Jimbot.Plugins.Builtin.Chatbot {
    public class ChatbotConfig : Configuration<ChatbotConfig> {
        /// <summary>
        /// Names by which the bot can feel spoken to.
        /// For instance, if the current user or nick name is "HerbertTheBot",
        /// we will automatically take this into account. But with these nicks you can
        /// extend that to make the bot react to "Herbert" or "Bot" as well.
        /// Make it feel more natural.
        /// </summary>
        public string RulesPath { get; set; }
        /// <summary>
        /// Names by which the bot can feel spoken to.
        /// For instance, if the current user or nick name is "HerbertTheBot",
        /// we will automatically take this into account. But with these nicks you can
        /// extend that to make the bot react to "Herbert" or "Bot" as well.
        /// Make it feel more natural.
        /// </summary>
        public List<string> BotNicks { get; set; }

        /// <summary>
        /// The default time format to format any timestamps in chat.
        /// (This is used to display the time of day)
        /// </summary>
        public string TimeFormat { get; set; }

        /// <summary>
        /// The default date format to format any timestamps in chat.
        /// (This is used to display the date without time)
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Words affecting the conversation mood in a negative fashion
        /// </summary>
        public List<string> NegativeWords { get; set; }

        /// <summary>
        /// Words affecting the conversation mood in a positive fashion
        /// </summary>
        public List<string> PositiveWords { get; set; }

        public override void Ensure() {

            RulesPath ??= "config/ChatBotRules";

            BotNicks ??= new List<string>() {
                "Herbert",
                "Bot",
                "Botman"
            };

            NegativeWords ??= new List<string>() {
                "asshole",
                "bumfuck",
                "shithead",
                "think of something evil for this, come on!"
            };
            PositiveWords ??= new List<string>() {
                "love you",
                "awesome",
                "amazing",
                "lovely",
                "enlightening",
                "thank you",
                "think of some positive terms, yeah?"
            };
            DateFormat ??= "dd.MM.yyyy";
            TimeFormat ??= "HH:mm";
        }
    }
}