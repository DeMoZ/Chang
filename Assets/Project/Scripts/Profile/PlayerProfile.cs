using System;
using System.Collections.Generic;

namespace Chang.Profile
{
    public class PlayerProfile : IDisposable
    {
        public ProfileData ProfileData;
        public ProgressData ProgressData;

        /// <summary>
        /// key Thai/Fruits, value section
        /// </summary>
        public Dictionary<string, SimpleSection> ReorderedSections { get; private set; } = new();

        public PlayerProfile()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// initialize with saved data from Prefs/Remote etc.
        /// </summary>
        public void Init()
        {
        }

        public void AddReorderSection(string key, SimpleSection section)
        {
            ReorderedSections[key] = section;
        }
    }
}