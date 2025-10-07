using System.IO;

namespace Chang.Resources
{
    public class WordPathHelper
    {
        public string GetConfigPath(string key)
        {
            // key = Thai/Words/Fruits/Coconut
            // result Assets/Project/Resources_Bundled/Thai/Words/Fruits/Coconut.asset
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                $"{key}.asset");

            return NormalizePath(path);
        }

        public string GetSoundPath(string key)
        {
            // key = Thai/Words/Fruits/Coconut
            // result Assets/Project/Resources_Bundled/Thai/SoundWords/Fruits/Coconut.mp3
            string[] keyParts = key.Split('/');
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                keyParts[0],
                AssetPaths.Addressables.SoundWords,
                keyParts[2],
                $"{keyParts[3]}.mp3");

            return NormalizePath(path);
        }
        
        public string GetNativeSoundKey(string key, Languages language)
        {
            // key = Thai/Words/Fruits/Coconut
            string[] keyParts = key.Split('/');
            keyParts[0] = language.ToString(); 
            return string.Join("/", keyParts);
        }

        public string GetTexturePath(string key)
        {
            // key = Thai/Words/Fruits/Coconut
            // result Assets/Project/Resources_Bundled/Thai/ImageWords/Fruits/Coconut.png
            string[] keyParts = key.Split('/');
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                keyParts[0],
                AssetPaths.Addressables.ImageWords,
                keyParts[2],
                $"{keyParts[3]}.png");

            return NormalizePath(path);
        }

        public string NormalizePath(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}