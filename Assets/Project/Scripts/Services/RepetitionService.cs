using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Profile;
using Zenject;

namespace Chang.Services
{
    public class RepetitionService : IDisposable
    {
        private readonly ProfileService _profileService;

        [Inject]
        public RepetitionService(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public List<QuestLog> GetSectionRepetition(Languages language, int amount, string key)
        {
            Dictionary<string, QuestLog> progressQuestions = _profileService.ProgressData.Questions;
            var progressList = progressQuestions
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
        public List<QuestLog> GetGeneralRepetition(Languages language, int amount)
        {
            // todo chang filter by language
            var progressQuestions = _profileService.ProgressData.Questions;
            var progressList = progressQuestions.Select(q => q.Value).Where(q => q.SuccessSequence < 10);

            // var sortedWords = words
            //     .OrderBy(word => word.Mark)
            //     .ThenBy(word => word.LastReviewed)
            //     .Take(10)
            //     .ToList();

            // todo chang what to count?
            // 1. Iteration
            // 2. Date
            // 3. Mark

            // todo chang should be sorted by the sequence and time  too
            var sortedList = progressList.OrderBy(w => w.Mark)
                //.ThenBy(w => w.UtcTime)
                .Take(amount)
                .ToList();

            return sortedList;
        }

        public void Dispose()
        {
        }
    }
}