using System.IO;

namespace Jimbot.Tools {
    public static class IoHelper {
        public static void EnsurePath(string filePath) {
            var dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }
        }
        public static void EnsureFileAndPath(string filePath) {
            EnsurePath(filePath);

            if (!File.Exists(filePath)) {
                var fileHandle = File.Create(filePath);
                fileHandle.Close();
            }
        }
    }
}