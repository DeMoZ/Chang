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

        private CancellationTokenSource _cancellationTokenSource;

        [Inject]
        public ProfileService(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
            _prefsDataProvider = new PrefsDataProvider();
            _unityCloudDataProvider = new UnityCloudDataProvider();
        }

        public void Dispose()
        {
            _prefsDataProvider.Dispose();
            _unityCloudDataProvider.Dispose();
        }

        public async UniTask LoadStoredData()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var profileData = await _prefsDataProvider.LoadProfileDataAsync();
            var progressData = await _prefsDataProvider.LoadProgressDataAsync();
            _playerProfile.ProfileData = profileData;
            _playerProfile.ProgressData = progressData; //!progressData.IsInitialized ? new ProgressData() : progressData;

            await SaveIntoScriptableObject();

            await _unityCloudDataProvider.InitAsync(_cancellationTokenSource.Token);
            await _unityCloudDataProvider.LoadProfileDataAsync(); // todo roman Token
            await _unityCloudDataProvider.LoadProgressDataAsync(); // todo roman Token

            // todo roman await MergePrefsAndUnityData();
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