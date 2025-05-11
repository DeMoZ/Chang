using System.IO;

namespace Chang.Resources
{
    public class WordPathHelper
    {
        public string GetConfigPath(string key)
        {
            // Assets/Project/Resources_Bundled/Thai/Words/Fruits/Coconut.asset
            // key = Thai/Words/Fruits/Coconut
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                $"{key}.asset");

            return NormalizePath(path);
        }

        public string GetSoundPath(string key)
        {
            // Assets/Project/Resources_Bundled/Thai/SoundWords/Fruits/Coconut.mp3
            // key = Thai/Words/Fruits/Coconut
            string[] keyParts = key.Split('/');
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                keyParts[0],
                AssetPaths.Addressables.SoundWords,
                keyParts[2],
                $"{keyParts[3]}.mp3");

            return NormalizePath(path);
        }

        public string GetImagePath(string key)
        {
            // Assets/Project/Resources_Bundled/Thai/ImageWords/Fruits/Coconut.jpg
            // key = Thai/Words/Fruits/Coconut
            string[] keyParts = key.Split('/');
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                keyParts[0],
                AssetPaths.Addressables.ImageWords,
                keyParts[2],
                $"{keyParts[3]}.jpg");

            return NormalizePath(path);
        }

        public string NormalizePath(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}