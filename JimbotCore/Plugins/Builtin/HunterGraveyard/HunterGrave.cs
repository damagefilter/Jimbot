using SQLite;

namespace Jimbot.Plugins.Builtin.HunterGraveyard {
    /// <summary>
    /// DB Object defining a hunters grave.
    /// </summary>
    public class HunterGrave {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // [Indexed]
        public string HunterName { get; set; }

        public string DeathBy { get; set; }

        public int Kills { get; set; } = 0;

        public int RoundsSurvived { get; set; } = 0;
        
        public int CharacterLevel { get; set; } = 1;

        [Indexed]
        public string UserId { get; set; }

        [Ignore]
        public ulong DiscordUid => string.IsNullOrEmpty(UserId) ? 0 : ulong.Parse(UserId);
    }
}