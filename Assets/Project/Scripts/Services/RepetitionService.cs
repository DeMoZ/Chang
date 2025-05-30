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

        /// <summary>
        /// First iteration of the repetition filter
        /// </summary>
        /// <returns></returns>
        // todo chang add more logic to the filter
        public List<QuestLog> _GetGeneralRepetition(int amount)
        {
            // todo chang what to count?
            // 1. Success sequence
            // 2. Date
            // 3. Mark
            
            var language = _profileService.ProfileData.LearnLanguage;
            Dictionary<string, QuestLog> log = _profileService.ProgressData.GetQuestLogs(language);
            var progressList = log
                .Select(q => q.Value)
                .Where(q => q.SuccessSequence < 10)
                .OrderBy(q => q.SuccessSequence);

            // var sortedWords = words
            //     .OrderBy(word => word.Mark)
            //     .ThenBy(word => word.LastReviewed)
            //     .Take(10)
            //     .ToList();

            
            // todo chang should be sorted by the sequence and time  too
            var sortedList = progressList.OrderBy(w => w.Mark)
                //.ThenBy(w => w.UtcTime)
                .Take(amount)
                .ToList();

            return sortedList;
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