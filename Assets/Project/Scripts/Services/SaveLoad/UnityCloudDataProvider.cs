using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
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

        private bool CheckSession()
        {
            var isAuthenticated = AuthenticationService.Instance.IsSignedIn;
            if (!isAuthenticated)
            {
                Debug.LogError("User is not authenticated.");
            }

            return isAuthenticated;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data)
        {
            await SaveAsync(DataProviderConstants.ProfileDataKey, data);
        }

        public async UniTask SaveProgressDataAsync(ProgressData data)
        {
            await SaveAsync(DataProviderConstants.ProgressDataKey, data);
        }

        public async UniTask<ProgressData> LoadProgressDataAsync(CancellationToken ct)
        {
            var isOk = CheckSession();
            if (!isOk)
                return null; // todo chang should be exception or callback to start authorization

            var result = await LoadDataAsync<ProgressData>(DataProviderConstants.ProgressDataKey).AttachExternalCancellation(ct);
            result ??= new ProgressData();

            return result;
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            var isOk = CheckSession();
            if (!isOk)
                return null;

            var profileData = await LoadDataAsync<ProfileData>(DataProviderConstants.ProfileDataKey).AttachExternalCancellation(ct);
            if (profileData == null)
            {
                profileData = new ProfileData();
                await SaveProfileDataAsync(profileData);
            }

            return profileData;
        }

        private async UniTask SaveAsync<T>(string key, T data)
        {
            var isOk = CheckSession();
            if (!isOk)
                return;

            var dataDict = new Dictionary<string, object> { { key, data } };

            try
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(dataDict);
                Debug.Log($"{key} saved.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error on saving data type: {typeof(T).Name}, for key: {key}, error:\n{e}");
                HandleError(e);
            }
        }

        private async UniTask<T> LoadDataAsync<T>(string key) where T : class
        {
            try
            {
                var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
                var rawJson = JsonConvert.SerializeObject(savedData);
                Debug.Log($"Loaded raw data for key: {key}:\n{rawJson}");

                if (savedData.TryGetValue(key, out var value))
                {
                    var jsonString = JsonConvert.SerializeObject(value.Value, _jSettings);
                    Debug.Log($"Extract type: {typeof(T).Name}, for key: {key}:\n{jsonString}");

                    var deserializedObject = JsonConvert.DeserializeObject<T>(jsonString);
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

        public void Dispose()
        {
        }
    }
}