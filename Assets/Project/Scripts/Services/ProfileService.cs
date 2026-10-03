using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Core;
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
        private readonly PrefsDataProvider _prefsDataProvider;
        private readonly UnityCloudDataProvider _unityCloudDataProvider;
        private readonly LanguagesConfig _languagesConfig;

        private bool _isVocabularyChanged;
        private bool _isSentencesChanged;

        public ProgressData<VocabularyQuestLog> VocabularyProgress => _playerProfile.VocabularyProgress;
        public ProgressData<SentenceQuestLog> SentencesProgress => _playerProfile.SentencesProgress;
        public ProfileData ProfileData => _playerProfile.ProfileData;
        public string PlayerId => _unityCloudDataProvider.PlayerId;
        public Dictionary<string, VocabularySection> ReorderedVocabularySections => _playerProfile.ReorderedVocabularySections;
        public Dictionary<string, SentencesSection> ReorderedSentencesSections => _playerProfile.ReorderedSentencesSections;
        public Languages LearnLanguage => _playerProfile.ProfileData.LearnLanguage;
        public string ReorderedSectionKey(string section) => $"{LearnLanguage}/{section}";
        
        [Inject]
        public ProfileService(PlayerProfile playerProfile, ErrorHandler errorHandler, AuthorizationService authorizationService,
            LanguagesConfig languagesConfig)
        {
            _playerProfile = playerProfile;
            _languagesConfig = languagesConfig;
            _prefsDataProvider = new PrefsDataProvider();
            _unityCloudDataProvider = new UnityCloudDataProvider(errorHandler, authorizationService.RequestAuthentication);
        }

        public void Dispose()
        {
            _prefsDataProvider.Dispose();
            _unityCloudDataProvider.Dispose();
        }

        /// <summary>
        /// Loads the cloud and the local data, the newer one wins.
        /// The local data of another player is ignored
        /// </summary>
        /// <returns>false if the player is not authenticated, authentication is requested then</returns>
        public async UniTask<bool> LoadStoredData(CancellationToken ct)
        {
            if (!_unityCloudDataProvider.CheckSession())
            {
                return false;
            }

            string playerId = _unityCloudDataProvider.PlayerId;

            ProfileData cloudProfile = await _unityCloudDataProvider.LoadProfileDataAsync(ct);
            ProfileData prefsProfile = await _prefsDataProvider.LoadProfileDataAsync(ct);

            if (prefsProfile != null && prefsProfile.UnityCloudSavePlayerId != playerId)
            {
                Debug.LogWarning($"Local data belongs to the player: {prefsProfile.UnityCloudSavePlayerId}, it is deleted.");
                _prefsDataProvider.Clear();
                prefsProfile = null;
            }

            ProfileData profile = SelectNewer(cloudProfile, prefsProfile, data => data.UtcTime)
                ?? new ProfileData { NativeLanguage = _languagesConfig.GetDeviceLanguage(UnityEngine.Application.systemLanguage) };
            profile.SetPlayerId(playerId);
            Languages language = profile.LearnLanguage;

            ProgressData<VocabularyQuestLog> cloudVocabulary = await _unityCloudDataProvider.LoadVocabularyProgressDataAsync(language, ct);
            ProgressData<SentenceQuestLog> cloudSentences = await _unityCloudDataProvider.LoadSentencesProgressDataAsync(language, ct);
            ProgressData<VocabularyQuestLog> prefsVocabulary = await _prefsDataProvider.LoadVocabularyProgressDataAsync(language, ct);
            ProgressData<SentenceQuestLog> prefsSentences = await _prefsDataProvider.LoadSentencesProgressDataAsync(language, ct);

            ProgressData<VocabularyQuestLog> vocabulary = SelectNewer(cloudVocabulary, prefsVocabulary, data => data.UtcTime);
            ProgressData<SentenceQuestLog> sentences = SelectNewer(cloudSentences, prefsSentences, data => data.UtcTime);

            _playerProfile.ProfileData = profile;
            _playerProfile.VocabularyProgressDict[language] = vocabulary ?? new ProgressData<VocabularyQuestLog>();
            _playerProfile.SentencesProgressDict[language] = sentences ?? new ProgressData<SentenceQuestLog>();
            _isVocabularyChanged = false;
            _isSentencesChanged = false;

            // only the local data that won goes to the cloud, the defaults never overwrite the cloud
            if (profile == prefsProfile)
            {
                await _unityCloudDataProvider.SaveProfileDataAsync(profile, ct);
            }

            await _unityCloudDataProvider.SaveProgressDataAsync(language,
                vocabulary == prefsVocabulary ? vocabulary : null,
                sentences == prefsSentences ? sentences : null,
                ct);

            // the local data is synchronized with the cloud one and gets the player id
            await _prefsDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData, ct);
            await _prefsDataProvider.SaveProgressDataAsync(language, _playerProfile.VocabularyProgress, _playerProfile.SentencesProgress, ct);
            RefreshPrefsDataView();

            return true;
        }

        public async UniTask SaveProfileDataAsync(CancellationToken ct)
        {
            _playerProfile.ProfileData.SetTime(DateTime.UtcNow);

            await _prefsDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData, ct);
            await _unityCloudDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData, ct);
            RefreshPrefsDataView();
        }

        /// <summary>
        /// Saves only the progress changed since the last save, the cloud gets it in one request
        /// </summary>
        public async UniTask SaveProgressAsync(CancellationToken ct)
        {
            if (!_isVocabularyChanged && !_isSentencesChanged)
            {
                return;
            }

            ProgressData<VocabularyQuestLog> vocabulary = _isVocabularyChanged ? _playerProfile.VocabularyProgress : null;
            ProgressData<SentenceQuestLog> sentences = _isSentencesChanged ? _playerProfile.SentencesProgress : null;
            _isVocabularyChanged = false;
            _isSentencesChanged = false;

            await _prefsDataProvider.SaveProgressDataAsync(LearnLanguage, vocabulary, sentences, ct);
            await _unityCloudDataProvider.SaveProgressDataAsync(LearnLanguage, vocabulary, sentences, ct);
            RefreshPrefsDataView();
        }

        public void AddVocabularyLog(string key, string presentation, ChangTypes type, bool isCorrect, bool needIncrement = true)
        {
            Debug.Log($"Add vocabulary Log key: {key}, isCorrect {isCorrect}");
            Dictionary<string, VocabularyQuestLog> logs = _playerProfile.VocabularyProgress.Log;

            if (!logs.TryGetValue(key, out VocabularyQuestLog questLog))
            {
                questLog = new VocabularyQuestLog(key, presentation, type);
                logs[key] = questLog;
            }

            LogUnit logUnit = new LogUnit(DateTime.UtcNow, isCorrect, needIncrement);
            _playerProfile.VocabularyProgress.SetTime(logUnit.UtcTime);
            questLog.SetTime(logUnit.UtcTime);
            questLog.AddLog(logUnit);
            _isVocabularyChanged = true;
        }

        public void AddSentenceLog(string key, string presentation, ChangTypes type, bool isCorrect, bool needIncrement = true)
        {
            Debug.Log($"Add sentence Log key: {key}, isCorrect {isCorrect}");
            Dictionary<string, SentenceQuestLog> logs = _playerProfile.SentencesProgress.Log;

            if (!logs.TryGetValue(key, out SentenceQuestLog questLog))
            {
                questLog = new SentenceQuestLog(key, presentation, type);
                logs[key] = questLog;
            }
            
            LogUnit logUnit = new LogUnit(DateTime.UtcNow, isCorrect, needIncrement);
            _playerProfile.SentencesProgress.SetTime(logUnit.UtcTime);
            questLog.SetTime(logUnit.UtcTime);
            questLog.AddLog(logUnit);
            _isSentencesChanged = true;
        }

        public int GetVocabularyMark(string key)
        {
            Dictionary<string, VocabularyQuestLog> logs = _playerProfile.VocabularyProgress.Log;

            if (logs.TryGetValue(key, out VocabularyQuestLog questLog))
            {
                return questLog.Mark;
            }

            return 0;
        }

        public float GetSentencesMark(string key)
        {
            Dictionary<string, SentenceQuestLog> logs = _playerProfile.SentencesProgress.Log;

            if (logs.TryGetValue(key, out SentenceQuestLog questLog))
            {
                return questLog.Mark;
            }

            return 0;
        }
        
        public bool TryGetVocabularyLog(string key, out VocabularyQuestLog vocabularyQuestLog)
        {
            Dictionary<string, VocabularyQuestLog> logs = _playerProfile.VocabularyProgress.Log;
            return logs.TryGetValue(key, out vocabularyQuestLog);
        }

        /// <returns>the newer data, the first one on equal time</returns>
        private static T SelectNewer<T>(T first, T second, Func<T, DateTime> getUtcTime) where T : class
        {
            if (first == null || second == null)
            {
                return first ?? second;
            }

            return getUtcTime(second) > getUtcTime(first) ? second : first;
        }
    }
}