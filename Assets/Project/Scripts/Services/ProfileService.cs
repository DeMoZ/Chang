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

        public ProgressData ProgressData => _playerProfile.ProgressData;
        public ProfileData ProfileData => _playerProfile.ProfileData;
        public string PlayerId => _unityCloudDataProvider.PlayerId;
        public Dictionary<string, SimpleSection> ReorderedSections => _playerProfile.ReorderedSections;
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
            //var prefsProfileData = await _prefsDataProvider.LoadProfileDataAsync(_cts.Token);
            //var prefsProgressData = await _prefsDataProvider.LoadProgressDataAsync(_cts.Token);

            var unityProfileData = await _unityCloudDataProvider.LoadProfileDataAsync(ct);
            var unityProgressData = await _unityCloudDataProvider.LoadProgressDataAsync(ct);

            // todo chang merge data with prefs. But for now will use only cloud data

            _playerProfile.ProfileData = unityProfileData;
            _playerProfile.ProgressData = unityProgressData;
        }

        public async UniTask SaveProfileDataAsync()
        {
            _playerProfile.ProfileData.SetTime(DateTime.UtcNow);

            await _prefsDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData);
            await _unityCloudDataProvider.SaveProfileDataAsync(_playerProfile.ProfileData);
            await SaveIntoScriptableObject();
        }

        public async UniTask SaveProgressAsync()
        {
            _playerProfile.ProgressData.SetTime(DateTime.UtcNow);

            await _prefsDataProvider.SaveProgressDataAsync(_playerProfile.ProgressData);
            await _unityCloudDataProvider.SaveProgressDataAsync(_playerProfile.ProgressData);
            await SaveIntoScriptableObject();
        }

        public void AddLog(string key, string presentation, QuestionType type, bool isCorrect, bool needIncrement = true)
        {
            Debug.LogWarning($"AddLog key: {key}, isCorrect {isCorrect}");
            Dictionary<string, QuestLog> logs = _playerProfile.ProgressData.GetQuestLogs(_playerProfile.ProfileData.LearnLanguage);
            if (!logs.TryGetValue(key, out var questLog))
            {
                questLog = new QuestLog(key, presentation, type);
                logs[key] = questLog;
            }

            var logUnit = new LogUnit(DateTime.UtcNow, isCorrect, needIncrement);
            _playerProfile.ProgressData.SetTime(logUnit.UtcTime);
            questLog.SetTime(logUnit.UtcTime);
            questLog.AddLog(logUnit);
        }

        public int GetMark(string key)
        {
            Dictionary<string, QuestLog> logs = _playerProfile.ProgressData.GetQuestLogs(_playerProfile.ProfileData.LearnLanguage);
            if (logs.TryGetValue(key, out var questLog))
            {
                return questLog.Mark;
            }

            return 0;
        }

        public bool TryGetLog(string key, out QuestLog questLog)
        {
            Dictionary<string, QuestLog> logs = _playerProfile.ProgressData.GetQuestLogs(_playerProfile.ProfileData.LearnLanguage);
            return logs.TryGetValue(key, out questLog);
        }

        public void ReorderSection(SimpleSection section)
        {
            SimpleSection newSection = new SimpleSection
            {
                Section = section.Section,
                Lessons = new List<SimpleLessonData>()
            };
            
            string key = ReorderedSectionKey(section.Section);
            List<ISimpleQuestion> questions = section.Lessons.SelectMany(lesson => lesson.Questions).ToList();
            IOrderedEnumerable<ISimpleQuestion> orderedQuests = questions.OrderByDescending(GetQuestMark);
            Queue<ISimpleQuestion> questQueue = new Queue<ISimpleQuestion>(orderedQuests);

            foreach (var lesson in section.Lessons)
            {
                int count = lesson.Questions.Count;
                List<ISimpleQuestion> quests = new();

                for (int i = 0; i < count; i++)
                {
                    quests.Add(questQueue.Dequeue());
                }

                var newLesson = new SimpleLessonData
                {
                    Section = lesson.Section,
                    GenerateQuestMatchWordsData = true,
                    Questions = quests,
                };

                newSection.Lessons.Add(newLesson);
            }

            _playerProfile.AddReorderSection(key, newSection);

            return;

            int GetQuestMark(ISimpleQuestion quest)
            {
                if (quest is SimpleQuestSelectWord selectWord)
                {
                    return GetMark(selectWord.CorrectWordFileName);
                }

                throw new NotImplementedException($"Question type {quest.QuestionType} is not implemented");
            }
        }
    }
}