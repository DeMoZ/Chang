using System;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.DataProvider
{
    public interface IDataProvider : IDisposable
    {
        /// <returns>null if there is no stored data</returns>
        UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct);
        UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct);
        
        /// <returns>null if there is no stored data</returns>
        UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct);
        /// <returns>null if there is no stored data</returns>
        UniTask<ProgressData<SentenceQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct);
        
        /// <summary>
        /// Saves the progress at once, null progress is skipped
        /// </summary>
        UniTask SaveProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> vocabulary, ProgressData<SentenceQuestLog> sentences, CancellationToken ct);
    }
}
