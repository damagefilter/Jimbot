using System;
using System.Collections.Generic;
using System.IO;
using Jimbot.Config;
using Jimbot.Plugins.Builtin.Chatbot.Ai;
using Jimbot.Tools;
using Newtonsoft.Json;

namespace Jimbot.Plugins.Builtin.Chatbot.Rules {
    /// <summary>
    /// This is the file that contains all the chatting rules.
    /// It's the heart of the bot.
    /// And one day this'll be the training for the neural network that will drive the bot.
    /// If I ever get around implementing it anyway.
    /// </summary>
    public class ChatbotRuleConfig {
        public List<RawResponseNode> responseRules;

        public void Ensure() {
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

        public void Save(ChatbotConfig cfg) {
            string path = Path.Combine(cfg.RulesPath, "__example_delete_me.json");
            IoHelper.EnsureFileAndPath(path);
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ChatbotRuleConfig Load(ChatbotConfig cfg) {
            // I love late static binding
            string path = cfg.RulesPath;

            IoHelper.EnsurePath(path);

            bool hasFiles = false;
            ChatbotRuleConfig combined = new ChatbotRuleConfig();
            combined.responseRules = new List<RawResponseNode>();
            foreach (var file in Directory.EnumerateFiles(path, "*.json")) {
                ChatbotRuleConfig obj = JsonConvert.DeserializeObject<ChatbotRuleConfig>(File.ReadAllText(file));
                if (obj != null) {
                    hasFiles = true;
                    combined.responseRules.AddRange(obj.responseRules);
                }
            }

            if (!hasFiles) {
                // write out a defaults if we got nothing
                combined.Ensure();
                combined.Save(cfg);
            }
            return combined;
        }
    }
}