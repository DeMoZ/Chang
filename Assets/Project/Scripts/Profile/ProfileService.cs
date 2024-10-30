using System;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;

namespace Chang.Profile
{
    public class ProfileService : IDisposable
    {
        private readonly PlayerProfile _playerProfile;

        public ISaveLoad PrefsSaveLoad { get; private set; }
        public ISaveLoad RemoteSaveLoad { get; private set; }

        public ProfileService(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
            PrefsSaveLoad = new PrefsSaveLoad();
            RemoteSaveLoad = new RemoteSaveLoad();
        }

        public void Dispose()
        {
            PrefsSaveLoad.Dispose();
                RemoteSaveLoad.Dispose();
        }

        public async UniTask LoadPrefsData()
        {
            var pData = await PrefsSaveLoad.LoadData();
            _playerProfile.PlayerData = !pData.IsInitialized ? new PlayerData() : pData;
        }

        public async UniTask SavePrefs()
        {
            _playerProfile.PlayerData.SetTime(DateTime.UtcNow);
            PrefsSaveLoad.SaveData(_playerProfile.PlayerData);
        }

        public void AddLog(string key, LogUnit logUnit)
        {
            if (!_playerProfile.PlayerData.Questions.TryGetValue(key, out var unit))
            {
                unit = new QuestLog(key);
                _playerProfile.PlayerData.Questions[key] = unit;
            }

            _playerProfile.PlayerData.SetTime(logUnit.UtcTime);
            unit.SetTime(logUnit.UtcTime);
            unit.AddLog(logUnit);
        }
    }
}