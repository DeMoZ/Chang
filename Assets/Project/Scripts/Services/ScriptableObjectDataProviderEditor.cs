using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services.DataProvider
{
    [CreateAssetMenu(menuName = "Chang/Services/SaveLoad Config", fileName = "SaveLoadConfig")]
    public class ScriptableObjectDataProviderEditor : ScriptableObject, IDataProvider
    {
        public string PlayerId => string.Empty; // todo chang save and load player ID

        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        public ProfileData ProfileData;
        public SerializableVariantProgressData<VocabularyQuestLog> VocabularyProgressData;
        public SerializableVariantProgressData<SentencesQuestLog> SentencesProgressData;

        public void Dispose()
        {
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public UniTask<ProgressData<SentencesQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct)
        {
            ProfileData = data;
            await UniTask.Yield(ct);
            Debug.Log($"{nameof(SaveProfileDataAsync)} Saved {data}");
        }
        
        public async UniTask SaveVocabularyProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> data, CancellationToken ct)
        {
            VocabularyProgressData = data as SerializableVariantProgressData<VocabularyQuestLog>;

            if (VocabularyProgressData != null)
            {
                VocabularyProgressData.SerializedUtcTime = VocabularyProgressData.UtcTime.ToString();

                VocabularyProgressData.SerializedQuestions = new List<(string, VocabularyQuestLog)>();
                Dictionary<string, VocabularyQuestLog> logs = data.Log;
                foreach (var pair in logs)
                {
                    VocabularyProgressData.SerializedQuestions.Add(new ValueTuple<string, VocabularyQuestLog>(pair.Key, pair.Value));
                }
            }

            await UniTask.Yield(ct);
            Debug.Log($"{nameof(SaveVocabularyProgressDataAsync)} Saved {data}");
        }

        public async UniTask SaveSentencesProgressDataAsync(Languages language, ProgressData<SentencesQuestLog> data, CancellationToken ct)
        {
            SentencesProgressData = data as SerializableVariantProgressData<SentencesQuestLog>;

            if (SentencesProgressData != null)
            {
                SentencesProgressData.SerializedUtcTime = SentencesProgressData.UtcTime.ToString();

                SentencesProgressData.SerializedQuestions = new List<(string, SentencesQuestLog)>();
                Dictionary<string, SentencesQuestLog> logs = data.Log;
                foreach (var pair in logs)
                {
                    SentencesProgressData.SerializedQuestions.Add(new ValueTuple<string, SentencesQuestLog>(pair.Key, pair.Value));
                }
            }

            await UniTask.Yield(ct);
            Debug.Log($"{nameof(SaveSentencesProgressDataAsync)} Saved {data}");
        }
    }

    [Serializable]
    public class SerializableVariantProgressData<T> : ProgressData<T> where T : IQuestLog
    {
        public string SerializedUtcTime;

        [SerializeField] public List<(string, T)> SerializedQuestions;
    }
}