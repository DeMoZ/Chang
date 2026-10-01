using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chang.Core;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services
{
    public class SentencesRepetitionService : AbstractRepetitionService
    {
        public SentencesRepetitionService(ProfileService profileService) : base(profileService)
        {
        }

        /// <summary>
        /// Logs are taken by section lessons keys, log section is ignored
        /// </summary>
        public async Task<List<SentenceQuestLog>> GetSectionRepetitionAsync(int amount, SentencesSection section, CancellationToken ct)
        {
            Dictionary<string, SentenceQuestLog> log = ProfileService.SentencesProgress.Log;
            IEnumerable<string> sectionKeys = section.SectionLessons.SelectMany(lesson => lesson.Keys).Distinct();

            await UniTask.Yield(ct);

            return sectionKeys
                .Where(log.ContainsKey)
                .Select(key => log[key])
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();
        }

        private float OrderByWeight(SentenceQuestLog vocabularyQuestLog)
        {
            double timeWeight = (DateTime.UtcNow - vocabularyQuestLog.UtcTime).TotalMinutes * TimeWeight;
            double weight = vocabularyQuestLog.Mark * MarkWeight + vocabularyQuestLog.SuccessSequence * SequenceWeight + timeWeight;
            return (float)weight;
        }
    }
}