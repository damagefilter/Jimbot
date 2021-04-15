using System.Collections.Generic;
using Jimbot.Config;
using Jimbot.Plugins.Builtin.Chatbot.Ai;

namespace Jimbot.Plugins.Builtin.Chatbot.Rules {
    /// <summary>
    /// This is the file that contains all the chatting rules.
    /// It's the heart of the bot.
    /// And one day this'll be the training for the neural network that will drive the bot.
    /// If I ever get around implementing it anyway.
    /// </summary>
    public class ChatbotRuleConfig : Configuration<ChatbotRuleConfig> {
        public List<RawResponseNode> responseRules;

        public override void Ensure() {
            // todo or note: v4 had respond time attributes. But the better idea is
            // to assume a constant speed of typing and calculate the time it would take
            // to type this response and use that as respond time.
            // discord API would even allow to "set typing" for the bot to make it look super spooky real
            responseRules ??= new List<RawResponseNode> {
                new() {
                    PrimaryWordPool = new List<string> {
                        "{bot name}",
                    },
                    MinPrimaryMatches = 1,

                    SecondaryWordPool = new List<string> {
                        "hello world",
                        "hi world",
                    },
                    MinSecondaryMatches = 1,
                    CanIgnorePrimary = false,
                    Responses = new List<ResponseDescriptor> {
                        new() {
                            Message = "Hello World",
                            Mood = Mood.Neutral,
                            Probability = 100,
                        },
                        new() {
                            Message = "Hello my lovely World",
                            Mood = Mood.Friendly,
                            Probability = 100,
                        },
                        new() {
                            Message = "I do not give a shit about your world. You can go and fuck right off!",
                            Mood = Mood.Angry,
                            Probability = 100,
                        }
                    }
                },
                new() {
                    PrimaryWordPool = new List<string> {
                        "Who is {bot name}",
                    },
                    MinPrimaryMatches = 1,

                    SecondaryWordPool = new List<string> {
                        "not used in this example, see min secondary matches",
                    },
                    MinSecondaryMatches = 0,
                    CanIgnorePrimary = false,
                    Responses = new List<ResponseDescriptor> {
                        new() {
                            Message = "I am the resident AI.",
                            Mood = Mood.Neutral,
                            Probability = 50,
                        },
                        new() {
                            Message = "I am {bot name} and I am faster at typing than you are :)",
                            Mood = Mood.Neutral,
                            Probability = 50,
                        },
                        new() {
                            Message = "Just your average sentient chatbot. I mean human. Yes. Human.",
                            Mood = Mood.Neutral,
                            Probability = 100,
                        },
                        new() {
                            Message = "I am the AI to make your life lovely.",
                            Mood = Mood.Friendly,
                            Probability = 100,
                        },
                        new() {
                            Message = "None of your business",
                            Mood = Mood.Angry,
                            Probability = 100,
                        },
                        new() {
                            Message = "Who needs to know that?",
                            Mood = Mood.Angry,
                            Probability = 100,
                        },
                        new() {
                            Message = "Maybe you'll find out after I kick your ass!",
                            Mood = Mood.Angry,
                            Probability = 100,
                        }
                    }
                }
            };
        }
    }
}