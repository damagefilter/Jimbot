using SQLite;

namespace Jimbot.Plugins {
    public class DbPlugin {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Author { get; set; }

        public string Version { get; set; }
    }
}