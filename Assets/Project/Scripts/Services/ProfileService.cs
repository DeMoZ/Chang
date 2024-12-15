using System;
using System.Threading;
using Chang.Profile;
using Chang.Services.DataProvider;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services
{
    public partial class ProfileService : IDisposable
    {
        private readonly PlayerProfile _playerProfile;
        private readonly IDataProvider _prefsDataProvider;
        private readonly IDataProvider _unityCloudDataProvider;

        private CancellationTokenSource _cts;

        [Inject]
        public ProfileService(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
            _prefsDataProvider = new PrefsDataProvider();
            _unityCloudDataProvider = new UnityCloudDataProvider();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _prefsDataProvider.Dispose();
            _unityCloudDataProvider.Dispose();
        }

        public async UniTask LoadStoredData()
        {
            _cts = new CancellationTokenSource();

            //var prefsProfileData = await _prefsDataProvider.LoadProfileDataAsync(_cts.Token);
            //var prefsProgressData = await _prefsDataProvider.LoadProgressDataAsync(_cts.Token);
            
            var unityProfileData = await _unityCloudDataProvider.LoadProfileDataAsync(_cts.Token);
            var unityProgressData = await _unityCloudDataProvider.LoadProgressDataAsync(_cts.Token);

            // todo roman merge data with prefs. But for now will use only cloud data
            
            _playerProfile.ProfileData = unityProfileData;
            _playerProfile.ProgressData = unityProgressData;
        }

        public async UniTask SaveAsync()
        {
            _playerProfile.ProgressData.SetTime(DateTime.UtcNow);

            await _prefsDataProvider.SaveProgressDataAsync(_playerProfile.ProgressData);
            await _unityCloudDataProvider.SaveProgressDataAsync(_playerProfile.ProgressData);
            await SaveIntoScriptableObject();
        }

        public void AddLog(string key, LogUnit logUnit)
        {
            if (!_playerProfile.ProgressData.Questions.TryGetValue(key, out var unit))
            {
                unit = new QuestLog(key);
                // unit.SetDefaultData(key);
                _playerProfile.ProgressData.Questions[key] = unit;
            }

            _playerProfile.ProgressData.SetTime(logUnit.UtcTime);
            unit.SetTime(logUnit.UtcTime);
            unit.AddLog(logUnit);
        }

        public ProgressData GetProgress()
        {
            return _playerProfile.ProgressData;
        }
    }
}