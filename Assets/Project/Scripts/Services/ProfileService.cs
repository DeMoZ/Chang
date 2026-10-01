using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IDataProvider _prefsDataProvider;
        private readonly IDataProvider _unityCloudDataProvider;

        public ProgressData<VocabularyQuestLog> VocabularyProgress => _playerProfile.VocabularyProgress;
        public ProgressData<SentenceQuestLog> SentencesProgress => _playerProfile.SentencesProgress;
        public ProfileData ProfileData => _playerProfile.ProfileData;
        public string PlayerId => _unityCloudDataProvider.PlayerId;
        public Dictionary<string, VocabularySection> ReorderedVocabularySections => _playerProfile.ReorderedVocabularySections;
        public Dictionary<string, SentencesSection> ReorderedSentencesSections => _playerProfile.ReorderedSentencesSections;
        public Languages LearnLanguage => _playerProfile.ProfileData.LearnLanguage;
        public string ReorderedSectionKey(string section) => $"{LearnLanguage}/{section}";
        
        [Inject]
        public ProfileService(PlayerProfile playerProfile, ErrorHandler errorHandler, AuthorizationService authorizationService)
        {
            _playerProfile = playerProfile;
            _prefsDataProvider = new PrefsDataProvider();
            _unityCloudDataProvider = new UnityCloudDataProvider(errorHandler, authorizationService.RequestAuthentication);
        }

        public void Dispose()
        {
            _prefsDataProvider.Dispose();
            _unityCloudDataProvider.Dispose();
        }

        /// <returns>false if the player is not authenticated, authentication is requested then</returns>
        public async UniTask<bool> LoadStoredData(CancellationToken ct)
        {
            ProfileData unityProfileData = await _unityCloudDataProvider.LoadProfileDataAsync(ct);
            if (unityProfileData == null)
            {
                return false;
            }

            Languages language = unityProfileData.LearnLanguage;

            ProgressData<VocabularyQuestLog> vocabularyProgress = await _unityCloudDataProvider.LoadVocabularyProgressDataAsync(language, ct);
            ProgressData<SentenceQuestLog> sentencesProgress = await _unityCloudDataProvider.LoadSentencesProgressDataAsync(language, ct);

            // todo chang merge data with prefs. But for now will use only cloud data

            _playerProfile.ProfileData = unityProfileData;
            _playerProfile.VocabularyProgressDict[language] = vocabularyProgress;
            _playerProfile.SentencesProgressDict[language] = sentencesProgress;
            return true;
        }

        public async UniTask SaveProfileDataAsync(CancellationToken ct)
        {
            _playerProfile.ProfileData.SetTime(DateTime.UtcNow);

            await _prefsDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData, ct);
            await _unityCloudDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData, ct);
            await SaveIntoScriptableObject(ct);
        }

        // todo chang depend on the logic need probably save progress for sentences or vocabulary one at a time
        public async UniTask SaveProgressAsync(CancellationToken ct)
        {
            _playerProfile.VocabularyProgress.SetTime(DateTime.UtcNow);

            await _prefsDataProvider.SaveVocabularyProgressDataAsync(_playerProfile.ProfileData.LearnLanguage, _playerProfile.VocabularyProgress, ct);
            await _prefsDataProvider.SaveSentencesProgressDataAsync(_playerProfile.ProfileData.LearnLanguage, _playerProfile.SentencesProgress, ct);
            await _unityCloudDataProvider.SaveVocabularyProgressDataAsync(_playerProfile.ProfileData.LearnLanguage, _playerProfile.VocabularyProgress, ct);
            await _unityCloudDataProvider.SaveSentencesProgressDataAsync(_playerProfile.ProfileData.LearnLanguage, _playerProfile.SentencesProgress, ct);
            await SaveIntoScriptableObject(ct);
        }

        public async UniTask SaveVocabularyProgressAsync(CancellationToken ct)
        {
            await UniTask.Yield(ct); // todo chang delete
            throw new NotImplementedException();
        }

        public async UniTask SaveSentencesProgressAsync(CancellationToken ct)
        {
            await UniTask.Yield(ct); // todo chang delete
            throw new NotImplementedException();
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

        /// <summary>
        /// Redistributes section keys between lessons ordered by mark descending, keeping lessons sizes
        /// </summary>
        public void ReorderVocabularySection(VocabularySection section)
        {
            VocabularySection newSection = new VocabularySection
            {
                Language = section.Language,
                Section = section.Section,
                SectionKey = section.SectionKey,
                Lessons = ReorderLessons(section.Lessons, key => GetVocabularyMark(key)),
            };

            newSection.PopulateQuestions();

            _playerProfile.AddReorderVocabularySection(ReorderedSectionKey(section.Section), newSection);
        }

        /// <summary>
        /// Redistributes section keys between lessons ordered by mark descending, keeping lessons sizes
        /// </summary>
        public void ReorderSentencesSection(SentencesSection section)
        {
            SentencesSection newSection = new SentencesSection
            {
                Language = section.Language,
                Section = section.Section,
                SectionKey = section.SectionKey,
                SectionLessons = ReorderLessons(section.SectionLessons, GetSentencesMark),
            };

            newSection.PopulateQuestions();

            _playerProfile.AddReorderSentencesSection(ReorderedSectionKey(section.Section), newSection);
        }

        private static List<Lesson> ReorderLessons(List<Lesson> lessons, Func<string, float> getMark)
        {
            Queue<string> keysQueue = new Queue<string>(lessons.SelectMany(lesson => lesson.Keys).OrderByDescending(getMark));
            List<Lesson> newLessons = new();

            foreach (Lesson lesson in lessons)
            {
                List<string> keys = new();

                for (int i = 0; i < lesson.Keys.Count; i++)
                {
                    keys.Add(keysQueue.Dequeue());
                }

                newLessons.Add(new Lesson(lesson.Language, lesson.Section, keys));
            }

            return newLessons;
        }
    }
}