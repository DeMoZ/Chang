using System;
using System.IO;

namespace Chang.Resources
{
    public class WordPathHelper : IDisposable
    {
        public string GetConfigPath(string key)
        {
            // Assets/Project/Resources_Bundled/Thai/Words/WeekDays/Yesterday.asset
            // key = Thai/Words/Fruits/Coconut
            return Path.Combine(
                AssetPaths.Addressables.Root,
                $"{key}.asset");
        }

        public string GetSoundPath(string key)
        {
            // Assets/Project/Resources_Bundled/Thai/SoundWords/Fruits/Watermelon.mp3
            // key = Thai/Words/Fruits/Coconut
            string[] keyParts = key.Split('/');
            return Path.Combine(
                AssetPaths.Addressables.Root,
                keyParts[0],
                AssetPaths.Addressables.SoundWords,
                keyParts[2],
                $"{keyParts[3]}.mp3");
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}