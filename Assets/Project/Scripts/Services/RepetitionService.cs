using System;
using System.Collections.Generic;
using System.Linq;
using Chang.Profile;
using Cysharp.Threading.Tasks;
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

        /// <summary>
        /// First iteration of the repetition filter
        /// todo roman add more logic to the filter
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<QuestLog> GetGeneralRepetition(int amount)
        {
            var progressQuestions = _profileService.GetProgress().Questions;
            var progressList = progressQuestions.Select(q => q.Value).Where(q => q.SuccesSequese < 10);

            // var sortedWords = words
            //     .OrderBy(word => word.Mark)
            //     .ThenBy(word => word.LastReviewed)
            //     .Take(10)
            //     .ToList();

            // todo roman what to count?
            // 1. Iteration
            // 2. Date
            // 3. Mark

            // todo roman should be sorted by the sequence and time  too
            var sortedList = progressList.OrderBy(w => w.Mark)
            //.ThenBy(w => w.UtcTime)
            .Take(amount)
            .ToList();

            return sortedList;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}