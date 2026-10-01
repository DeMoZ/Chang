using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Core;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services
{
    public class VocabularyRepetitionService : AbstractRepetitionService
    {
        public VocabularyRepetitionService(ProfileService profileService) : base(profileService)
        {
        }

        /// <summary>
        /// Logs are taken by section lessons keys, log section is ignored
        /// </summary>
        public async UniTask<List<VocabularyQuestLog>> GetSectionRepetitionAsync(int amount, VocabularySection section, CancellationToken ct)
        {
            Dictionary<string, VocabularyQuestLog> log = ProfileService.VocabularyProgress.Log;
            IEnumerable<string> sectionKeys = section.Lessons.SelectMany(lesson => lesson.Keys).Distinct();

            // todo chang issues with thread pool in web build. How to make sorting and filtering on main thread faster ?
            // return await UniTask.RunOnThreadPool(() =>
            // {
            //     ct.ThrowIfCancellationRequested();
            //
            //     var progressList = sectionKeys
            //         .Where(log.ContainsKey)
            //         .Select(key => log[key])
            //         .OrderByDescending(OrderByWeight)
            //         .Take(amount)
            //         .ToList();
            //
            //     ct.ThrowIfCancellationRequested();
            //     progressList.Shuffle();
            //     return progressList;
            // }, cancellationToken: ct);
            await UniTask.Yield(ct);
            return sectionKeys
                .Where(log.ContainsKey)
                .Select(key => log[key])
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();
        }

        public async UniTask<List<VocabularyQuestLog>> GetGeneralRepetitionAsync(int amount, CancellationToken ct)
        {
            Languages language = ProfileService.ProfileData.LearnLanguage;
            Dictionary<string, VocabularyQuestLog> log = ProfileService.VocabularyProgress.Log;

            // todo chang issues with thread pool in web build. How to make sorting and filtering on main thread faster ?
            // return await UniTask.RunOnThreadPool(() =>
            // {
            //     ct.ThrowIfCancellationRequested();
            //
            //     var progressList = log
            //         .Select(q => q.Value)
            //         .OrderByDescending(OrderByWeight)
            //         .Take(amount)
            //         .ToList();
            //     
            //     ct.ThrowIfCancellationRequested();
            //     progressList.Shuffle();
            //     return progressList;
            // }, cancellationToken: ct);

            await UniTask.Yield(ct);
            return log
                .Select(q => q.Value)
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();
        }

        private float OrderByWeight(VocabularyQuestLog vocabularyQuestLog)
        {
            double timeWeight = (DateTime.UtcNow - vocabularyQuestLog.UtcTime).TotalMinutes * TimeWeight;
            double weight = vocabularyQuestLog.Mark * MarkWeight + vocabularyQuestLog.SuccessSequence * SequenceWeight + timeWeight;
            return (float)weight;
        }
    }
}