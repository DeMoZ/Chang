using System;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;

namespace Chang.Services.DataProvider
{
    public interface IDataProvider : IDisposable
    {
        UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct);
        UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct);
        
        UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct);
        UniTask<ProgressData<SentencesQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct);
        UniTask SaveVocabularyProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> data, CancellationToken ct);
        UniTask SaveSentencesProgressDataAsync(Languages language, ProgressData<SentencesQuestLog> data, CancellationToken ct);
        
        string PlayerId { get; }
    }
}