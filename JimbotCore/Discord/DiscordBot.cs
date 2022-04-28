using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jimbot.Config;
using Jimbot.Di;
using Jimbot.Logging;
using Ninject;

namespace Jimbot.Discord {
    /// <summary>
    /// It's the life-line of this application to a discord server.
    /// </summary>
    public class DiscordBot {
        private DiscordSocketClient client;
        private CommandService commands;

        private Logger log;
        private AppConfig cfg;
        private IKernel ninjectKernel; // discords command service forces us to expose this lol

        public DiscordSocketClient DiscordClient => client;
        public CommandService Commands => commands;

        public DiscordBot(AppConfig cfg, DiContainer di) {
            client = new DiscordSocketClient(new DiscordSocketConfig {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true // apparently the API hangs indefinitely when this is not set, everytime we request a user data. go figure.
            });
            
            client.MessageReceived += HandleCommands;

            log = LogManager.GetLogger(GetType());
            this.cfg = cfg;
            ninjectKernel = di.GetImplementation().Kernel;

            commands = new CommandService(new CommandServiceConfig {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
            });

        }

        public async Task RegisterCommands<T>() where T : ModuleBase<SocketCommandContext> {
            await commands.AddModuleAsync(typeof(T), ninjectKernel);
        }

        public Logger GetLogger(Type type) {
            // Because ninject when inside discords app domain, cannot resolve loggers for some reason, we need to get them via the bot
            // in order to stay somewhat clean with the logger factory stuff
            return LogManager.GetLogger(type);
        }

        public async Task Run() {
            if (client.ConnectionState != ConnectionState.Disconnected) {
                log.Warn("Got an attempt to connect to Discord. But we're already connected.");
                return;
            }

            await client.SetGameAsync(cfg.BotGame);
            await client.LoginAsync(TokenType.Bot, cfg.BotToken);
            await client.StartAsync();
            // Block task from stopping until the program quits
            await Task.Delay(-1);
        }

        private async Task HandleCommands(SocketMessage msg) {
            if (msg.Author.IsBot || !(msg is SocketUserMessage message) || message.Channel is IDMChannel) {
                return;
            }

            int argPos = 0;

            if (!message.HasStringPrefix("!", ref argPos)) {
                return;
            }

            var context = new SocketCommandContext(client, message);

            var res = commands.Search(context, argPos);
            if (res.IsSuccess) {
                var result = await commands.ExecuteAsync(context, argPos, ninjectKernel);
                if (!result.IsSuccess) {
                    await context.Channel.SendMessageAsync($"Das ging fürchterlich den Bach runter: {result.ErrorReason}");
                }
            }
            else {
                log.Info("Encountered unknown command " + message.Content);
            }
        }
    }
}