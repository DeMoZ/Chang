using System;
using System.Collections.Generic;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services.SaveLoad
{
    public class UnityCloudSaveLoad : ISaveLoad
    {
        public UnityCloudSaveLoad()
        {
            TestSave().Forget();
        }

        private async UniTask TestSave()
        {
            Debug.Log("Init");
            await UnityServices.InitializeAsync();
            Debug.Log("SingIn");
            await SignIn();
            
            try
            {
                await CloudSaveService.Instance.Data.ForceSaveAsync(new Dictionary<string, object>
                {
                    { "level", 11 },
                    { "score", 1101 }
                });
                Debug.Log("Progress saved to the cloud.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to save progress: " + e);
            }
        }

        async UniTask SignIn()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Player signed in: " + AuthenticationService.Instance.PlayerId);
            }
            else
            {
                Debug.Log("Player signed in already" + AuthenticationService.Instance.PlayerId);
            }
        }

        public async UniTask<PlayerData> LoadData()
        {
            throw new NotImplementedException();
        }

        public async UniTask SaveData(PlayerData data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}