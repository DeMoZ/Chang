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

        private bool CheckSession()
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
            await SaveAsync(DataProviderConstants.ProfileDataKey, data, ct);
        }

        public async UniTask<ProgressData<VocabularyQuestLog>> LoadVocabularyProgressDataAsync(Languages language, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
                return null;

            ProgressData<VocabularyQuestLog> result = await LoadDataAsync<ProgressData<VocabularyQuestLog>>($"{language}_{DataProviderConstants.VocabularyProgressDataKey}", ct);
            result ??= new ProgressData<VocabularyQuestLog>();
            return result;
        }

        public async UniTask<ProgressData<SentenceQuestLog>> LoadSentencesProgressDataAsync(Languages language, CancellationToken ct)
        {
            bool isOk = CheckSession();
            if (!isOk)
            {
                return null;
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
                await RequestAsync(() => CloudSaveService.Instance.Data.Player.SaveAsync(dataDict), ct);

                Debug.Log($"{key} saved.");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Saving data type: {typeof(T).Name}, for key: {key} was cancelled.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on saving data type: {typeof(T).Name}, for key: {key}, error:\n{e}");
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