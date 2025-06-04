using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Profile;
using Zenject;

namespace Chang.Services
{
    public class RepetitionService : IDisposable
    {
        private const float MarkWeight = 0.25f;
        private const float SequenceWeight = 0.4f;
        private const float TimeWeight = 0.015f;

        private readonly ProfileService _profileService;

        [Inject]
        public RepetitionService(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public List<QuestLog> GetSectionRepetition(int amount, string key)
        {
            var language = _profileService.ProfileData.LearnLanguage;
            Dictionary<string, QuestLog> log = _profileService.ProgressData.GetQuestLogs(language);
            var progressList = log
                .Select(q => q.Value)
                .Where(q => q.Language == language && string.Equals(q.Section, key)).ToList();

            progressList.Shuffle();
            return progressList.Take(amount).ToList();
        }
        
        public List<QuestLog> GetGeneralRepetition(int amount)
        {
            Languages language = _profileService.ProfileData.LearnLanguage;
            Dictionary<string, QuestLog> log = _profileService.ProgressData.GetQuestLogs(language);

            var progressList = log
                .Select(q => q.Value)
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();

            return progressList;
        }

        private float OrderByWeight(QuestLog questLog)
        {
            double timeWeight = (DateTime.UtcNow - questLog.UtcTime).TotalMinutes * TimeWeight;
            double weight = questLog.Mark * MarkWeight + questLog.SuccessSequence * SequenceWeight + timeWeight;
            return (float)weight;
        }

        public void Dispose()
        {
        }
    }
}