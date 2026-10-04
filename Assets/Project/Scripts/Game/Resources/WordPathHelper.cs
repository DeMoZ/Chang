using System.IO;

namespace Chang.Resources
{
    public class WordPathHelper
    {
        public string GetSoundPath(string key, SoundVoices voice = SoundVoices.Female)
        {
            // key = Thai/Vocabulary/Fruits/Coconut
            // result Assets/Project/Resources_Bundled/SoundWords/Thai/Female/Fruits/Coconut.mp3
            
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }
            
            key =  key.Replace("Vocabulary/", "");

            // the voice folder goes after the language
            int languageEnd = key.IndexOf('/');
            key = languageEnd < 0 ? $"{voice}/{key}" : key.Insert(languageEnd + 1, $"{voice}/");
            
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