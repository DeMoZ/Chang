using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private const int MaxRateLimitedAttempts = 3;
        private const int MaxItemsPerSave = 20;

        private readonly ErrorHandler _errorHandler;
        private readonly Action _onNotAuthenticated;

        public string PlayerId => AuthenticationService.Instance.PlayerId;

        private readonly JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        public UnityCloudDataProvider(ErrorHandler errorHandler, Action onNotAuthenticated)
        {
            _errorHandler = errorHandler;
            _onNotAuthenticated = onNotAuthenticated;
        }

        public void Dispose()
        {
        }

        /// <returns>false if the player is not authenticated, authentication is requested then</returns>
        public bool CheckSession()
        {
            bool isAuthenticated = AuthenticationService.Instance.IsSignedIn;
            if (!isAuthenticated)
            {
                Debug.LogError("User is not authenticated.");
                _onNotAuthenticated?.Invoke();
            }

            return isAuthenticated;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data, CancellationToken ct)
        {
            await SaveAsync(new Dictionary<string, object> { { DataProviderConstants.ProfileDataKey, data } }, ct);
        }

        public async UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return null;
            }

            return await LoadDataAsync<ProgressData<VocabularyQuestLog>>(DataProviderConstants.VocabularyProgressKey(language), ct);
        }

        public async UniTask<ProgressData<SentenceQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return null;
            }

            return await LoadDataAsync<ProgressData<SentenceQuestLog>>(DataProviderConstants.SentencesProgressKey(language), ct);
        }

        public async UniTask SaveProgressDataAsync(Languages language, ProgressData<VocabularyQuestLog> vocabulary, ProgressData<SentenceQuestLog> sentences, CancellationToken ct)
        {
            Dictionary<string, object> dataDict = new();

            if (vocabulary != null)
            {
                dataDict[DataProviderConstants.VocabularyProgressKey(language)] = vocabulary;
            }

            if (sentences != null)
            {
                dataDict[DataProviderConstants.SentencesProgressKey(language)] = sentences;
            }

            if (dataDict.Count == 0)
            {
                return;
            }

            await SaveAsync(dataDict, ct);
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return null;
            }

            return await LoadDataAsync<ProfileData>(DataProviderConstants.ProfileDataKey, ct);
        }

        /// <summary>
        /// Loads the profile and the progress of all languages in one request.
        /// Unlike the other loads, throws on errors: a failed load must not look like an account without data.
        /// </summary>
        public async UniTask<StoredPlayerData> LoadAllAsync(CancellationToken ct)
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                throw new InvalidOperationException("The player is not signed in.");
            }

            HashSet<string> keys = new() { DataProviderConstants.ProfileDataKey };
            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                keys.Add(DataProviderConstants.VocabularyProgressKey(language));
                keys.Add(DataProviderConstants.SentencesProgressKey(language));
            }

            Dictionary<string, Item> items = await RequestAsync(() => CloudSaveService.Instance.Data.Player.LoadAsync(keys), ct);

            StoredPlayerData data = new() { Profile = Extract<ProfileData>(items, DataProviderConstants.ProfileDataKey) };
            foreach (Languages language in Enum.GetValues(typeof(Languages)))
            {
                var vocabulary = Extract<ProgressData<VocabularyQuestLog>>(items, DataProviderConstants.VocabularyProgressKey(language));
                if (vocabulary != null)
                {
                    data.Vocabulary[language] = vocabulary;
                }

                var sentences = Extract<ProgressData<SentenceQuestLog>>(items, DataProviderConstants.SentencesProgressKey(language));
                if (sentences != null)
                {
                    data.Sentences[language] = sentences;
                }
            }

            Debug.Log($"LoadAllAsync: loaded keys: {string.Join(", ", items.Keys)}");
            return data;
        }

        /// <summary>
        /// Saves the data that is not null. Unlike the other saves, throws on errors.
        /// </summary>
        public async UniTask SaveAllAsync(StoredPlayerData data, CancellationToken ct)
        {
            Dictionary<string, object> dataDict = new();
            if (data.Profile != null)
            {
                dataDict[DataProviderConstants.ProfileDataKey] = data.Profile;
            }

            foreach (var (language, vocabulary) in data.Vocabulary)
            {
                dataDict[DataProviderConstants.VocabularyProgressKey(language)] = vocabulary;
            }

            foreach (var (language, sentences) in data.Sentences)
            {
                dataDict[DataProviderConstants.SentencesProgressKey(language)] = sentences;
            }

            // a save request takes a limited number of items
            var pairs = dataDict.ToList();
            for (int i = 0; i < pairs.Count; i += MaxItemsPerSave)
            {
                Dictionary<string, object> part = pairs.Skip(i).Take(MaxItemsPerSave).ToDictionary(pair => pair.Key, pair => pair.Value);
                await RequestAsync(() => CloudSaveService.Instance.Data.Player.SaveAsync(part), ct);
                Debug.Log($"SaveAllAsync: {string.Join(", ", part.Keys)} saved.");
            }
        }

        private T Extract<T>(Dictionary<string, Item> items, string key) where T : class
        {
            if (!items.TryGetValue(key, out Item item))
            {
                return null;
            }

            string json = JsonConvert.SerializeObject(item.Value, _jSettings);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Saves all the data in one request
        /// </summary>
        private async UniTask SaveAsync(Dictionary<string, object> dataDict, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return;
            }

            string keys = string.Join(", ", dataDict.Keys);

            try
            {
                await RequestAsync(() => CloudSaveService.Instance.Data.Player.SaveAsync(dataDict), ct);

                Debug.Log($"{keys} saved.");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Saving data for keys: {keys} was cancelled.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on saving data for keys: {keys}, error:\n{e}");
                HandleError(e, "Failed to save data");
            }
        }

        private async UniTask<T> LoadDataAsync<T>(string key, CancellationToken ct) where T : class
        {
            try
            {
                Dictionary<string, Item> savedData = await RequestAsync(
                    () => CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key }), ct);

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
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Loading data type: {typeof(T).Name}, for key: {key} was cancelled.");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on loading data type: {typeof(T).Name}, for key: {key}, error:\n{e}");
                HandleError(e, "Failed to load data");
                return null;
            }
        }

        /// <summary>
        /// Runs the cloud request, on rate limit waits for the time the service asks for and repeats the request
        /// </summary>
        private async UniTask<TResult> RequestAsync<TResult>(Func<Task<TResult>> request, CancellationToken ct)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    return await request().AsUniTask().AttachExternalCancellation(ct);
                }
                catch (CloudSaveRateLimitedException e) when (attempt < MaxRateLimitedAttempts)
                {
                    Debug.LogWarning($"Cloud save rate limited, attempt {attempt}, retry after {e.RetryAfter} sec.");
                    await UniTask.Delay(TimeSpan.FromSeconds(e.RetryAfter), cancellationToken: ct);
                }
            }
        }

        private void HandleError(Exception e, string description)
        {
            if (e is not CloudSaveException cloudSaveException)
            {
                _errorHandler.HandleError(e, description);
                return;
            }

            switch (cloudSaveException.Reason)
            {
                case CloudSaveExceptionReason.PlayerIdMissing:
                case CloudSaveExceptionReason.AccessTokenMissing:
                case CloudSaveExceptionReason.Unauthorized:
                    _onNotAuthenticated?.Invoke();
                    break;

                case CloudSaveExceptionReason.NoInternetConnection:
                    _errorHandler.HandleError(e, $"{description}. Check the internet connection");
                    break;

                case CloudSaveExceptionReason.TooManyRequests:
                case CloudSaveExceptionReason.ServiceUnavailable:
                    _errorHandler.HandleError(e, $"{description}. Service is unavailable, try again later");
                    break;

                case CloudSaveExceptionReason.InvalidArgument when e is CloudSaveValidationException validationException:
                    string details = string.Join("\n", validationException.Details.Select(d => $"{d.Field}: {string.Join(", ", d.Messages)}"));
                    Debug.LogError($"Cloud save validation error:\n{details}");
                    _errorHandler.HandleError(e, description);
                    break;

                default:
                    _errorHandler.HandleError(e, description);
                    break;
            }
        }
    }
}