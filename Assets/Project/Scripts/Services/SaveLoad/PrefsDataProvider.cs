using System;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services.DataProvider
{
    public class PrefsDataProvider : IDataProvider
    {
        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        public void Dispose()
        {
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            ProfileData data = Load<ProfileData>(DataProviderConstants.ProfileDataKey);

            await UniTask.Yield(ct);

            return data;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct)
        {
            Save(DataProviderConstants.ProfileDataKey, data);
            PlayerPrefs.Save();

            await UniTask.Yield(ct);
        }

        public async UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct)
        {
            ProgressData<VocabularyQuestLog> data = Load<ProgressData<VocabularyQuestLog>>(DataProviderConstants.VocabularyProgressKey(language));

            await UniTask.Yield(ct);

            return data;
        }

        public async UniTask<ProgressData<SentenceQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct)
        {
            ProgressData<SentenceQuestLog> data = Load<ProgressData<SentenceQuestLog>>(DataProviderConstants.SentencesProgressKey(language));

            await UniTask.Yield(ct);

            return data;
        }

        public async UniTask SaveProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> vocabulary, ProgressData<SentenceQuestLog> sentences, CancellationToken ct)
        {
            if (vocabulary != null)
            {
                Save(DataProviderConstants.VocabularyProgressKey(language), vocabulary);
            }

            if (sentences != null)
            {
                Save(DataProviderConstants.SentencesProgressKey(language), sentences);
            }

            PlayerPrefs.Save();

            await UniTask.Yield(ct);
        }

        /// <summary>
        /// Deletes the profile and the progress for all languages, e.g. when the data belongs to another player
        /// </summary>
        public void Clear()
        {
            PlayerPrefs.DeleteKey(DataProviderConstants.ProfileDataKey);
            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                PlayerPrefs.DeleteKey(DataProviderConstants.VocabularyProgressKey(language));
                PlayerPrefs.DeleteKey(DataProviderConstants.SentencesProgressKey(language));
            }

            PlayerPrefs.Save();
        }

        private T Load<T>(string key) where T : class
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on loading data type: {typeof(T).Name}, for key: {key}, error:\n{e}");
                return null;
            }
        }

        private void Save<T>(string key, T data)
        {
            string json = JsonConvert.SerializeObject(data, _jSettings);
            PlayerPrefs.SetString(key, json);
        }
    }
}
