using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services
{
    public class SentencesRepetitionService : AbstractRepetitionService
    {
        public SentencesRepetitionService(ProfileService profileService) : base(profileService)
        {
        }

        public async Task<List<SentencesQuestLog>> GetSectionRepetitionAsync(int amount, string section, CancellationToken ct)
        {
            Dictionary<string, SentencesQuestLog> log = ProfileService.SentencesProgress.Log;

            await UniTask.Yield(ct);

            return log
                .Select(q => q.Value)
                .Where(q => string.Equals(q.Section, section))
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();
        }

        private float OrderByWeight(SentencesQuestLog vocabularyQuestLog)
        {
            double timeWeight = (DateTime.UtcNow - vocabularyQuestLog.UtcTime).TotalMinutes * TimeWeight;
            double weight = vocabularyQuestLog.Mark * MarkWeight + vocabularyQuestLog.SuccessSequence * SequenceWeight + timeWeight;
            return (float)weight;
        }
    }
}