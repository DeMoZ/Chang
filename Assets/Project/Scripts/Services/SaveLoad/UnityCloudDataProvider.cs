using System.Collections.Generic;
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
            return AuthenticationService.Instance.IsSignedIn;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data)
        {
            await SaveProgressDataAsync(SaveLoadConstants.ProfileDataKey, data);
        }

        public async UniTask SaveProgressDataAsync(ProgressData data)
        {
            await SaveProgressDataAsync(SaveLoadConstants.ProgressDataKey, data);
        }

        public async UniTask<ProgressData> LoadProgressDataAsync()
        {
            var isOk = CheckSession();
            if (!isOk)
                return null;

            return await LoadProgressDataAsync<ProgressData>(SaveLoadConstants.ProgressDataKey);
        }

        public async UniTask<ProfileData> LoadProfileDataAsync()
        {
            var isOk = CheckSession();
            if (!isOk)
                return null;

            return await LoadProgressDataAsync<ProfileData>(SaveLoadConstants.ProfileDataKey);
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

        private async UniTask<T> LoadProgressDataAsync<T>(string key)
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            if (savedData.TryGetValue(key, out var value))
            {
                // todo roman solve exceptions
                // <exception cref="CloudSaveException">Thrown if request is unsuccessful.</exception>
                // <exception cref="CloudSaveValidationException">Thrown if the service returned validation error.</exception>
                // <exception cref="CloudSaveRateLimitedException">Thrown if the service returned rate limited error.</exception>

                var valString = JsonConvert.SerializeObject(value, _jSettings);
                Debug.Log($"Loaded data for key {key}:\n {valString}");
                var valObject = JsonConvert.DeserializeObject<T>(valString);

                return valObject;
            }

            Debug.LogWarning($"No saved data found for key: {key}");
            return default;
        }

        public void Dispose()
        {
            
        }
    }
}