using System;

namespace Chang.Profile
{
    public class PlayerProfile : IDisposable
    {
        public ProfileData ProfileData;
        public ProgressData ProgressData;

        public PlayerProfile()
        {
        }

        /// <summary>
        /// initialize with saved data from Prefs/Remote etc.
        /// </summary>
        public void Init()
        {
        }

        public void Dispose()
        {
        }
    }
}