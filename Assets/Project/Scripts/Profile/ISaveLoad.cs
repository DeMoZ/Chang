using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Profile
{
    public interface ISaveLoad : IDisposable
    {
        UniTask<PlayerData> LoadData();
        UniTask SaveData(PlayerData data);
    }

    public class PrefsSaveLoad : ISaveLoad
    {
        private const string PlayerDataKey = "PlayerData";

        public async UniTask<PlayerData> LoadData()
        {
            var json = PlayerPrefs.GetString(PlayerDataKey, "{}");
            var data = JsonConvert.DeserializeObject<PlayerData>(json);

            return data;
        }

        public async UniTask SaveData(PlayerData data)
        {
            var jSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            
            var json = JsonConvert.SerializeObject(data, jSettings);
            PlayerPrefs.SetString(PlayerDataKey, json);
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }

    // todo roman implement unity cloud save load
    public class RemoteSaveLoad : ISaveLoad
    {
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