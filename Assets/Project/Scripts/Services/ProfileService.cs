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

        public string PlayerId => _unityCloudDataProvider.PlayerId;

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

        public async UniTask LoadStoredData(CancellationToken ct)
        {
            //var prefsProfileData = await _prefsDataProvider.LoadProfileDataAsync(_cts.Token);
            //var prefsProgressData = await _prefsDataProvider.LoadProgressDataAsync(_cts.Token);

            var unityProfileData = await _unityCloudDataProvider.LoadProfileDataAsync(ct);
            var unityProgressData = await _unityCloudDataProvider.LoadProgressDataAsync(ct);

            // todo chang merge data with prefs. But for now will use only cloud data

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

        public void AddLog(string key, string presentation, QuestionType type, bool isCorrect, bool needIncrement = true)
        {
            Debug.LogWarning($"AddLog key: {key}, isCorrect {isCorrect}");

            if (!_playerProfile.ProgressData.Questions.TryGetValue(key, out var questLog))
            {
                questLog = new QuestLog(key, presentation, type);
                _playerProfile.ProgressData.Questions[key] = questLog;
            }

            var logUnit = new LogUnit(DateTime.UtcNow, isCorrect, needIncrement);
            _playerProfile.ProgressData.SetTime(logUnit.UtcTime);
            questLog.SetTime(logUnit.UtcTime);
            questLog.AddLog(logUnit);
        }

        public ProgressData GetProgress()
        {
            return _playerProfile.ProgressData;
        }

        public int GetMark(string key)
        {
            if (_playerProfile.ProgressData.Questions.TryGetValue(key, out var questLog))
            {
                return questLog.Mark;
            }

            return 0;
        }

        public bool TryGetLog(string key, out QuestLog questLog)
        {
            return _playerProfile.ProgressData.Questions.TryGetValue(key, out questLog);
        }
    }
}