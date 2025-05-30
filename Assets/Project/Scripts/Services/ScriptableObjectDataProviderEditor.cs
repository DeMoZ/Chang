using System;
using System.Collections.Generic;
using System.Threading;
using Chang.Profile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services.DataProvider
{
    [CreateAssetMenu(menuName = "Chang/Services/SaveLoad Config", fileName = "SaveLoadConfig")]
    public class ScriptableObjectDataProviderEditor : ScriptableObject, IDataProvider
    {
        public string PlayerId => string.Empty; // todo chang save and load player ID
        
        private JsonSerializerSettings _jSettings = new()
        {
            Formatting = Formatting.Indented,
        };
        
        public ProfileData ProfileData;
        public SerializableProgressData ProgressData;

        public UniTask<ProfileData> LoadProfileDataAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public UniTask<ProgressData> LoadProgressDataAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }


        public async UniTask SaveProfileDataAsync(ProfileData data)
        {
            ProfileData = data;
            await UniTask.Yield();
            Debug.Log($"{nameof(SaveProfileDataAsync)} Saved {data}");
        }

        public async UniTask SaveProgressDataAsync(ProgressData data)
        {
            ProgressData = data as SerializableProgressData;

            if (ProgressData != null)
            {
                ProgressData.SerializedUtcTime = ProgressData.UtcTime.ToString();

                ProgressData.SerializedQuestions = new List<(string, QuestLog)>();
                Dictionary<string, QuestLog> logs = data.GetQuestLogs(Languages.Thai); // todo chang use current language
                foreach (var pair in logs)
                {
                    ProgressData.SerializedQuestions.Add(new ValueTuple<string, QuestLog>(pair.Key, pair.Value));
                }
            }

            await UniTask.Yield();
            Debug.Log($"{nameof(SaveProgressDataAsync)} Saved {data}");
        }

        public void Dispose()
        {
        }
    }

    [Serializable]
    public class SerializableProgressData : ProgressData
    {
        public string SerializedUtcTime;
        
        [SerializeField]
        public List<(string, QuestLog)> SerializedQuestions;
    }
}