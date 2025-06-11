using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
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

        public async UniTask<List<QuestLog>> GetSectionRepetitionAsync(int amount, string section, CancellationToken ct)
        {
            var language = _profileService.ProfileData.LearnLanguage;
            Dictionary<string, QuestLog> log = _profileService.ProgressData.GetQuestLogs(language);

            // todo chang issues with thread pool in web build. How to make sorting and filtering on main thread faster ?
            // return await UniTask.RunOnThreadPool(() =>
            // {
            //     ct.ThrowIfCancellationRequested();
            //
            //     var progressList = log
            //         .Select(q => q.Value)
            //         .Where(q => string.Equals(q.Section, section))
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
                .Where(q => string.Equals(q.Section, section))
                .OrderByDescending(OrderByWeight)
                .Take(amount)
                .ToList();
        }

        public async UniTask<List<QuestLog>> GetGeneralRepetitionAsync(int amount, CancellationToken ct)
        {
            Languages language = _profileService.ProfileData.LearnLanguage;
            Dictionary<string, QuestLog> log = _profileService.ProgressData.GetQuestLogs(language);
            
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