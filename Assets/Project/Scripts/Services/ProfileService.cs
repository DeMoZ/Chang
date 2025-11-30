using System;
using System.Collections.Generic;
using System.Linq;
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

        public ProgressData<VocabularyQuestLog> VocabularyProgress => _playerProfile.VocabularyProgress;
        public ProgressData<SentencesQuestLog> SentencesProgress => _playerProfile.SentencesProgress;
        public ProfileData ProfileData => _playerProfile.ProfileData;
        public string PlayerId => _unityCloudDataProvider.PlayerId;
        public Dictionary<string, Vocabulary.SectionData> ReorderedSections => _playerProfile.ReorderedSections;
        public string ReorderedSectionKey(string section) => $"{_playerProfile.ProfileData.LearnLanguage}/{section}";

        [Inject]
        public ProfileService(PlayerProfile playerProfile, ErrorHandler errorHandler)
        {
            _playerProfile = playerProfile;
            _prefsDataProvider = new PrefsDataProvider();
            _unityCloudDataProvider = new UnityCloudDataProvider(errorHandler);
        }

        public void Dispose()
        {
            _prefsDataProvider.Dispose();
            _unityCloudDataProvider.Dispose();
        }

        public async UniTask LoadStoredData(CancellationToken ct)
        {
            ProfileData unityProfileData = await _unityCloudDataProvider.LoadProfileDataAsync(ct);

            Languages language = unityProfileData.LearnLanguage;

            ProgressData<VocabularyQuestLog> vocabularyProgress = await _unityCloudDataProvider.LoadVocabularyProgressDataAsync(language, ct);
            ProgressData<SentencesQuestLog> sentencesProgress = await _unityCloudDataProvider.LoadSentencesProgressDataAsync(language, ct);

            // todo chang merge data with prefs. But for now will use only cloud data

            _playerProfile.ProfileData = unityProfileData;
            _playerProfile.VocabularyProgressDict[language] = vocabularyProgress;
            _playerProfile.SentencesProgressDict[language] = sentencesProgress;
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
            throw new NotImplementedException();
        }

        public async UniTask SaveSentencesProgressAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public void AddVocabularyLog(string key, string presentation, QuestionType type, bool isCorrect, bool needIncrement = true)
        {
            Debug.LogWarning($"AddLog key: {key}, isCorrect {isCorrect}");
            Dictionary<string, VocabularyQuestLog> logs = _playerProfile.VocabularyProgress.Log;

            if (!logs.TryGetValue(key, out var questLog))
            {
                questLog = new VocabularyQuestLog(key, presentation, type);
                logs[key] = questLog;
            }

            var logUnit = new LogUnit(DateTime.UtcNow, isCorrect, needIncrement);
            _playerProfile.VocabularyProgress.SetTime(logUnit.UtcTime);
            questLog.SetTime(logUnit.UtcTime);
            questLog.AddLog(logUnit);
        }

        public int GetVocabularyMark(string key)
        {
            Dictionary<string, VocabularyQuestLog> logs = _playerProfile.VocabularyProgress.Log;

            if (logs.TryGetValue(key, out var questLog))
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

        public void ReorderSection(Vocabulary.SectionData sectionData)
        {
            Vocabulary.SectionData newSectionData = new Vocabulary.SectionData
            {
                Section = sectionData.Section,
                Lessons = new List<Vocabulary.LessonData>()
            };

            string key = ReorderedSectionKey(sectionData.Section);
            List<Vocabulary.IQuestion> questions = sectionData.Lessons.SelectMany(lesson => lesson.Questions).ToList();
            IOrderedEnumerable<Vocabulary.IQuestion> orderedQuests = questions.OrderByDescending(GetQuestMark);
            Queue<Vocabulary.IQuestion> questQueue = new Queue<Vocabulary.IQuestion>(orderedQuests);

            foreach (var lesson in sectionData.Lessons)
            {
                int count = lesson.Questions.Count;
                List<Vocabulary.IQuestion> quests = new();

                for (int i = 0; i < count; i++)
                {
                    quests.Add(questQueue.Dequeue());
                }

                var newLesson = new Vocabulary.LessonData
                {
                    SectionName = lesson.SectionName,
                    GenerateQuestMatchWordsData = true,
                    Questions = quests,
                };

                newSectionData.Lessons.Add(newLesson);
            }

            _playerProfile.AddReorderSection(key, newSectionData);

            return;

            int GetQuestMark(Vocabulary.IQuestion quest)
            {
                if (quest is Vocabulary.QuestSelectWord selectWord)
                {
                    return GetVocabularyMark(selectWord.CorrectWordFileName);
                }

                throw new NotImplementedException($"Question type {quest.QuestionType} is not implemented");
            }
        }
    }
}