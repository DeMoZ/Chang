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

        public async Task<List<QuestLog>> GetSectionRepetitionAsync(int amount, string section, CancellationToken ct)
        {
            Languages language = ProfileService.ProfileData.LearnLanguage;
            Dictionary<string, QuestLog> log = ProfileService.ProgressData.GetQuestLogs(language); // todo chang this should be the other log.

            await UniTask.Yield(ct);
            return log
                .Select(q => q.Value)
                .Where(q => string.Equals(q.Section, section))
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();
        }

        private float OrderByWeight(QuestLog questLog)
        {
            double timeWeight = (DateTime.UtcNow - questLog.UtcTime).TotalMinutes * TimeWeight;
            double weight = questLog.Mark * MarkWeight + questLog.SuccessSequence * SequenceWeight + timeWeight;
            return (float)weight;
        }
    }
}