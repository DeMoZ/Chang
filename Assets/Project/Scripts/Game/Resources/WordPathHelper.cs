using System.IO;

namespace Chang.Resources
{
    public class WordPathHelper
    {
        public string GetSoundPath(string key)
        {
            // key = Thai/Words/Fruits/Coconut
            // result Assets/Project/Resources_Bundled/SoundWords/Thai/Fruits/Coconut.mp3
            
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }
            
            key =  key.Replace("Vocabulary/", "");
            
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                AssetPaths.Addressables.SoundWords,
                $"{key}.mp3");

            return NormalizePath(path);
        }

        public string GetNativeSoundKey(string key, Languages language)
        {
            // key = Thai/Words/Fruits/Coconut
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            string[] keyParts = key.Split('/');
            keyParts[0] = language.ToString();
            return string.Join("/", keyParts);
        }

        public string GetTexturePath(string key)
        {
            // key = Thai/Words/Fruits/Coconut
            // result Assets/Project/Resources_Bundled/ImageWords/Thai/Fruits/Coconut.png
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            key =  key.Replace("Vocabulary/", "");
            
            string path = Path.Combine(
                AssetPaths.Addressables.Root,
                AssetPaths.Addressables.ImageWords,
                $"{key}.png");

            return NormalizePath(path);
        }

        public string NormalizePath(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}