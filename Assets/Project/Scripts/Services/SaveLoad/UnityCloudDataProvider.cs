using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services.DataProvider
{
    public class UnityCloudDataProvider : IDataProvider
    {
        private readonly ErrorHandler _errorHandler;

        public string PlayerId => AuthenticationService.Instance.PlayerId;

        private readonly JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        public UnityCloudDataProvider(ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public void Dispose()
        {
        }

        private bool CheckSession()
        {
            bool isAuthenticated = AuthenticationService.Instance.IsSignedIn;
            if (!isAuthenticated)
            {
                Debug.LogError("User is not authenticated.");
            }

            return isAuthenticated;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct)
        {
            await SaveAsync(DataProviderConstants.ProgressDataKey, data, ct);
        }

        public async UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
                return null; // todo chang should be exception or callback to start authorization

            ProgressData<VocabularyQuestLog> result = await LoadDataAsync<ProgressData<VocabularyQuestLog>>($"{language}_{DataProviderConstants.VocabularyProgressDataKey}", ct);
            // result ??= new ProgressData<VocabularyQuestLog>(); // todo chang  uncomment 
            {
                // todo chang remove block
                result ??= await TempMockVocabularyProgressWithOldProgress(ct);
            }

            return result;
        }

        public async UniTask<ProgressData<SentenceQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return null; // todo chang should be exception or callback to start authorization
            }

            ProgressData<SentenceQuestLog> result = await LoadDataAsync<ProgressData<SentenceQuestLog>>($"{language}_{DataProviderConstants.SentencesProgressDataKey}", ct);
            result ??= new ProgressData<SentenceQuestLog>();

            return result;
        }

        public async UniTask SaveVocabularyProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> data, CancellationToken ct)
        {
            await SaveAsync($"{language}_{DataProviderConstants.VocabularyProgressDataKey}", data, ct);
        }

        public async UniTask SaveSentencesProgressDataAsync(Languages language, ProgressData<SentenceQuestLog> data, CancellationToken ct)
        {
            await SaveAsync($"{language}_{DataProviderConstants.SentencesProgressDataKey}", data, ct);
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return null;
            }

            ProfileData profileData = await LoadDataAsync<ProfileData>(DataProviderConstants.ProfileDataKey, ct);
            if (profileData == null)
            {
                profileData = new ProfileData();
                await SaveProfileDataAsync(profileData, ct);
            }

            return profileData;
        }

        private async UniTask SaveAsync<T>(string key, T data, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return;
            }

            Dictionary<string, object> dataDict = new Dictionary<string, object> { { key, data } };

            try
            {
                await CloudSaveService.Instance.Data.Player
                    .SaveAsync(dataDict)
                    .AsUniTask()
                    .AttachExternalCancellation(ct);

                Debug.Log($"{key} saved.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on saving data type: {typeof(T).Name}, for key: {key}, error:\n{e}");
                HandleError(e);
            }
        }

        private async UniTask<T> LoadDataAsync<T>(string key, CancellationToken ct) where T : class
        {
            try
            {
                Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player
                    .LoadAsync(new HashSet<string> { key })
                    .AsUniTask()
                    .AttachExternalCancellation(ct);

                string rawJson = JsonConvert.SerializeObject(savedData);
                Debug.Log($"Loaded raw data for key: {key}:\n{rawJson}");

                if (savedData.TryGetValue(key, out var value))
                {
                    string jsonString = JsonConvert.SerializeObject(value.Value, _jSettings);
                    Debug.Log($"Extract type: {typeof(T).Name}, for key: {key}:\n{jsonString}");

                    T deserializedObject = JsonConvert.DeserializeObject<T>(jsonString);
                    return deserializedObject;
                }

                Debug.LogWarning($"No saved data found for key: {key}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on loading data type: {typeof(T).Name}, for key: {key}, error:\n{e}");
                HandleError(e);
                return null;
            }
        }

        private void HandleError(Exception e)
        {
            // todo chang solve exceptions
            // <exception cref="CloudSaveException">Thrown if request is unsuccessful.</exception>
            // <exception cref="CloudSaveValidationException">Thrown if the service returned validation error.</exception>
            // <exception cref="CloudSaveRateLimitedException">Thrown if the service returned rate limited error.</exception>
            // todo chang add error handling, probably internet issue

            _errorHandler.HandleError(e, "Failed to save data");
        }

        // todo chang remove when all users will have new data
        private async UniTask<ProgressData<VocabularyQuestLog>> TempMockVocabularyProgressWithOldProgress(CancellationToken ct)
        {
            OldProgressData oldResult = await LoadOldProgressDataAsync(ct);
            oldResult ??= new OldProgressData();

            ProgressData<VocabularyQuestLog> result = new ProgressData<VocabularyQuestLog>(oldResult.UtcTime, oldResult.Log);
            return result;
        }

        // todo chang remove when all users will have new data
        private async UniTask<OldProgressData> LoadOldProgressDataAsync(CancellationToken ct)
        {
            var isOk = CheckSession();
            if (!isOk)
            {
                return null; // todo chang should be exception or callback to start authorization
            }

            OldProgressData result = await LoadDataAsync<OldProgressData>(DataProviderConstants.ProgressDataKey, ct);
            result ??= new OldProgressData();

            return result;
        }
    }
}