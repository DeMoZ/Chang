using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Services.DataProvider
{
    public class PrefsDataProvider : IDataProvider
    {
        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };

        public async UniTask<ProfileData> LoadProfileDataAsync()
        {
            var json = PlayerPrefs.GetString(SaveLoadConstants.ProfileDataKey, "{}");
            var data = JsonConvert.DeserializeObject<ProfileData>(json);

            await UniTask.Yield();

            return data;
        }

        public async UniTask SaveProfileDataAsync(ProfileData data)
        {
            var json = JsonConvert.SerializeObject(data, _jSettings);
            PlayerPrefs.SetString(SaveLoadConstants.ProfileDataKey, json);

            await UniTask.Yield();
        }

        public async UniTask<ProgressData> LoadProgressDataAsync()
        {
            var json = PlayerPrefs.GetString(SaveLoadConstants.ProgressDataKey, "{}");
            var data = JsonConvert.DeserializeObject<ProgressData>(json);

            await UniTask.Yield();

            return data;
        }

        public async UniTask SaveProgressDataAsync(ProgressData data)
        {
            var json = JsonConvert.SerializeObject(data, _jSettings);
            PlayerPrefs.SetString(SaveLoadConstants.ProgressDataKey, json);

            await UniTask.Yield();
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}