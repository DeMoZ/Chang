using System.Collections.Generic;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services.SaveLoad
{
    public class UnityCloudSaveLoad : ISaveLoadWithInit
    {
        private const string NewUserLoginKey = "UnityCloudSaveLoadLastLoginIdKey";
        private bool _isInitialized;

        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        public async UniTask InitAsync(CancellationToken token)
        {
            Debug.Log("Init");
            var initTask = UnityServices.InitializeAsync();
            await initTask;

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("User already signed in.");
                // return;
            }
            else
            {
                // Try sign in anonymously
                AuthenticationService.Instance.SignedIn += () => Debug.Log("Signed in anonymously.");
                AuthenticationService.Instance.SignedOut += () => Debug.Log("Signed out.");

                AuthenticationService.Instance.SignInFailed += (error) => Debug.LogError($"Sign-in failed: {error}");

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                var newUserKey = PlayerPrefs.GetInt(NewUserLoginKey, 0);

                var isNewUser = newUserKey == 0;

                if (isNewUser)
                {
                    Debug.Log("New user registered anonymously.");
                    // todo roman New user initialization logic?
                }
                else
                {
                    Debug.Log("Existing user signed in.");
                }
            }
        }

        private async UniTask<bool> CheckSession()
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
            var isOk = await CheckSession();
            if (!isOk)
                return null;

            return await LoadProgressDataAsync<ProgressData>(SaveLoadConstants.ProgressDataKey);
        }

        public async UniTask<ProfileData> LoadProfileDataAsync()
        {
            var isOk = await CheckSession();
            if (!isOk)
                return null;
            
            return await LoadProgressDataAsync<ProfileData>(SaveLoadConstants.ProfileDataKey);
        }
        
        private async UniTask SaveProgressDataAsync<T>(string key, T data)
        {
            var isOk = await CheckSession();
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
            // TODO roman release managed resources here
        }
    }
}