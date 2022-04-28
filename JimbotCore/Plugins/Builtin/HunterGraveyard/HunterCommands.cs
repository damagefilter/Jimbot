using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jimbot.Db;
using Jimbot.Discord;
using Jimbot.Logging;

namespace Jimbot.Plugins.Builtin.HunterGraveyard {
    public class HunterCommands : ModuleBase<SocketCommandContext> {
        private DiscordBot bot;
        private DbRepository repo;
        private Logger log;
        
        public HunterCommands(DiscordBot bot, DbRepository repo) {
            this.bot = bot;
            this.repo = repo;
            log = bot.GetLogger(GetType());
        }
        
        [Command("hunter help")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task HelpCommand() {
            await ReplyAsync("Hunter beerdigen: **!hunter bury** Dr. Pavlo Lenietsky, **kills**: 234, **runden**: 15, **killer**: Suizid durch Sprengfalle, **level**: 25");
            await ReplyAsync("Letzten Hunter ansehen: **!hunter last**");
            await ReplyAsync("Hunter stats ansehen: **!hunter stats** [discord user]");
        }
        
        [Command("hunter last")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task LastHunterCommand() {
            string uid = Context.User.Id.ToString();
            var hunter = repo.FindOne<HunterGrave>(x => x.UserId == uid, y => y.Id, true);
            if (hunter == null) {
                await ReplyAsync($"Du hast noch keinen Hunter beerdigt, {Context.User.Username}");
                return;
            }

            string unspecified = "unbekannte Umstände";
            
            await ReplyAsync($"**{Context.User.Username}**'s letzter Hunter: **{hunter.HunterName}**");
            await ReplyAsync($"Er lebte für **{hunter.RoundsSurvived} Runden** und brachte dabei **{hunter.Kills} andere Hunter ins Grab**. Die Round/Kill Ratio war **{(float)hunter.RoundsSurvived / hunter.Kills}**");
            await ReplyAsync($"{hunter.HunterName} starb durch **{hunter.DeathBy ?? unspecified}** im Alter von **{hunter.CharacterLevel} leveln**");
        }
        
        [Command("hunter stats")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task HunterStatsCommand(SocketUser user = null) {
            string uid = user != null ? user.Id.ToString() : Context.User.Id.ToString();
            string userName = user != null ? user.Username : Context.User.Username;
            
            var hunters = repo.FindAll<HunterGrave>(x => x.UserId == uid);
            int numHunters = hunters.Count;
            int totalKills = 0;
            int totalRounds = 0;
            int totalLevels = 0;
            
            foreach (var hunter in hunters) {
                totalKills += hunter.Kills;
                totalRounds += hunter.RoundsSurvived;
                totalLevels += hunter.CharacterLevel;
            }

            float avgKills = (float)totalKills / numHunters;
            float avgRounds = (float)totalRounds / numHunters;
            float avgLevels = (float)totalLevels / numHunters;
            
            await ReplyAsync($"**{userName}** beerdigte schon **{numHunters} Hutners**!");
            await ReplyAsync($"Insgesamt kommen wir auf **{totalKills}** Kills, **{totalRounds}** Runden, **{avgLevels}** erreichte Level.");
            await ReplyAsync($"Im Durchschnitt kommen wir auf **{avgKills}** Kills, **{avgRounds}** Runden, **{avgLevels}** erreichte Level pro Hutner.");
            await ReplyAsync("Und das ist alles, im Großen und Ganzen");
        }
        
        [Command("hunter bury")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        public async Task KillCommand([Remainder]string txt) {
            var hunterName = Regex.Match(txt, "^\\s*([a-zA-Z\u00C0-\u024F\\s.-_:0-9]+),*");
            // I have to have this.
            if (!hunterName.Success) {
                await ReplyAsync("Da fehlt mir jetzt ein \"name:Hutner Name\"");
                return;
            }
            
            var killCount = Regex.Match(txt, "(?:kills|killed)\\s*:\\s*(\\d+),*");
            var survived = Regex.Match(txt, "(?:runden|rounds|survived)\\s*:\\s*(\\d+),*");
            var deathBy = Regex.Match(txt, "(?:killer|death by|killed by)\\s*:\\s*([a-zA-Z\u00C0-\u024F\\s.-_:0-9]+),*");
            var level = Regex.Match(txt, "(?:level|levels|age|alter)\\s*:\\s*(\\d+),*");

            var grave = new HunterGrave();
            grave.HunterName = hunterName.Groups[1].Value;
            if (killCount.Success) {
                grave.Kills = int.Parse(killCount.Groups[1].Value);
            }

            if (survived.Success) {
                grave.RoundsSurvived = int.Parse(survived.Groups[1].Value);
            }

            if (deathBy.Success) {
                grave.DeathBy = deathBy.Groups[1].Value;
            }

            if (level.Success) {
                grave.CharacterLevel = int.Parse(level.Groups[1].Value);
            }

            grave.UserId = Context.User.Id.ToString();

            if (!repo.Insert(grave)) {
                await ReplyAsync("Es hat einen Fehler beim begraben gegeben. Versuchs noch einmal.");
                return;
            }

            await ReplyAsync($"{grave.HunterName} von {Context.User.Username} ruht nun in Frieden ...");
            if (grave.Kills == 0 && grave.RoundsSurvived == 0) {
                await ReplyAsync("... naja. Der hat's versucht.");
            }
            
            else if (grave.Kills > 0 && grave.RoundsSurvived == 0) {
                await ReplyAsync("... ein echtes One-Round-Wonder.");
            }
            
            else if (grave.Kills > 0 && grave.RoundsSurvived > 0) {
                await ReplyAsync("... es war ein guter Run!");
            }
            else {
                await ReplyAsync("... es ist besser so.");
            }
        }
    }
}