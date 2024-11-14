using System;
using System.Threading;
using Chang.Profile;
using Chang.Services.SaveLoad;
using Cysharp.Threading.Tasks;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services
{
    public partial class ProfileService : IDisposable
    {
        private readonly PlayerProfile _playerProfile;
        private readonly ISaveLoad _prefsSaveLoad;
        private readonly ISaveLoadWithInit _unityCloudSaveLoad;

        private CancellationTokenSource _cancellationTokenSource;

        [Inject]
        public ProfileService(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
            _prefsSaveLoad = new PrefsSaveLoad();
            _unityCloudSaveLoad = new UnityCloudSaveLoad();
        }

        public void Dispose()
        {
            _prefsSaveLoad.Dispose();
            _unityCloudSaveLoad.Dispose();
        }

        public async UniTask LoadStoredData()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var profileData = await _prefsSaveLoad.LoadProfileDataAsync();
            var progressData = await _prefsSaveLoad.LoadProgressDataAsync();
            _playerProfile.ProfileData = profileData;
            _playerProfile.ProgressData = progressData; //!progressData.IsInitialized ? new ProgressData() : progressData;

            await SaveIntoScriptableObject();

            await _unityCloudSaveLoad.InitAsync(_cancellationTokenSource.Token);
            await _unityCloudSaveLoad.LoadProfileDataAsync(); // todo roman Token
            await _unityCloudSaveLoad.LoadProgressDataAsync(); // todo roman Token

            // todo roman await MergePrefsAndUnityData();
        }

        public async UniTask SaveAsync()
        {
            _playerProfile.ProgressData.SetTime(DateTime.UtcNow);

            await _prefsSaveLoad.SaveProgressDataAsync(_playerProfile.ProgressData);
            await _unityCloudSaveLoad.SaveProgressDataAsync(_playerProfile.ProgressData);
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

        public void FirstMethod()
        {
        }
    }
}