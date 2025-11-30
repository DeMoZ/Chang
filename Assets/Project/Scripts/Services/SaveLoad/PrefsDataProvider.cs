using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Services.DataProvider
{
    public class PrefsDataProvider : IDataProvider
    {
        public string PlayerId => string.Empty; // todo chang save and load player ID 
        
        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };
        
        public void Dispose()
        {
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            string json = PlayerPrefs.GetString(DataProviderConstants.ProfileDataKey, "{}");
            ProfileData data = JsonConvert.DeserializeObject<ProfileData>(json);

            await UniTask.Yield(ct);

            return data;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct)
        {
            string json = JsonConvert.SerializeObject(data, _jSettings);
            PlayerPrefs.SetString(DataProviderConstants.ProfileDataKey, json);

            await UniTask.Yield(ct);
        }

        public async UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct)
        {
            string json = PlayerPrefs.GetString($"{language}_{DataProviderConstants.VocabularyProgressDataKey}", "{}");
            ProgressData<VocabularyQuestLog> data = JsonConvert.DeserializeObject<ProgressData<VocabularyQuestLog>>(json);

            await UniTask.Yield(ct);

            return data;
        }

        public async UniTask<ProgressData<SentencesQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct)
        {
            string json = PlayerPrefs.GetString($"{language}_{DataProviderConstants.SentencesProgressDataKey}", "{}");
            ProgressData<SentencesQuestLog> data = JsonConvert.DeserializeObject<ProgressData<SentencesQuestLog>>(json);

            await UniTask.Yield(ct);

            return data;
        }
        
        public async UniTask SaveVocabularyProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> data, CancellationToken ct)
        {
            string json = JsonConvert.SerializeObject(data, _jSettings);
            PlayerPrefs.SetString($"{language}_{DataProviderConstants.VocabularyProgressDataKey}", json);

            await UniTask.Yield(ct);
        }
        
        public async UniTask SaveSentencesProgressDataAsync(Languages language, ProgressData<SentencesQuestLog> data, CancellationToken ct)
        {
            string json = JsonConvert.SerializeObject(data, _jSettings);
            PlayerPrefs.SetString($"{language}_{DataProviderConstants.SentencesProgressDataKey}", json);

            await UniTask.Yield(ct);
        }
    }
}