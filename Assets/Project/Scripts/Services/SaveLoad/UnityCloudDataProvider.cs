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
        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

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
            await SaveProgressDataAsync(DataProviderConstants.ProfileDataKey, data);
        }

        public async UniTask SaveProgressDataAsync(ProgressData data)
        {
            await SaveProgressDataAsync(DataProviderConstants.ProgressDataKey, data);
        }

        public async UniTask<ProgressData> LoadProgressDataAsync(CancellationToken ct)
        {
            var isOk = CheckSession();
            if (!isOk)
                return null; // todo roman should be exception or callback to start authorization

            var result = await LoadDataAsync<ProgressData>(DataProviderConstants.ProgressDataKey).AttachExternalCancellation(ct);
            result ??= new ProgressData();

            return result;
        }

        public async UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            var isOk = CheckSession();
            if (!isOk)
                return null;

            return await LoadDataAsync<ProfileData>(DataProviderConstants.ProfileDataKey).AttachExternalCancellation(ct);
        }

        private async UniTask SaveProgressDataAsync<T>(string key, T data)
        {
            var isOk = CheckSession();
            if (!isOk)
                return;

            var dataDict = new Dictionary<string, object> { { key, data } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataDict);
            Debug.Log($"{key} saved.");

            // todo roman solve exceptions
            // <exception cref="CloudSaveException">Thrown if request is unsuccessful.</exception>
            // <exception cref="CloudSaveValidationException">Thrown if the service returned validation error.</exception>
            // <exception cref="CloudSaveRateLimitedException">Thrown if the service returned rate limited error.</exception>
        }

        private async UniTask<T> LoadDataAsync<T>(string key) where T : class
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            
            var wholeJson = JsonConvert.SerializeObject(savedData, _jSettings);
            Debug.Log($"Loaded data for key {key}:\n {wholeJson}");
            
            if (savedData.TryGetValue(key, out var value))
            {
                var valString = JsonConvert.SerializeObject(value, _jSettings);
                Debug.Log($"Loaded data for key {key}:\n {valString}");

                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(valString);
                if (jsonObject != null && jsonObject.TryGetValue("Value", out var valuePart))
                {
                    var valueString = JsonConvert.SerializeObject(valuePart, _jSettings);
                    var valObject = JsonConvert.DeserializeObject<T>(valueString);
                    return valObject;
                }
            }

            Debug.LogWarning($"No saved data found for key: {key}");
            return default;
        }

        public void Dispose()
        {
        }
    }
}