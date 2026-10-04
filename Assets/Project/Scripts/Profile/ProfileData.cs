using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Chang.Profile
{
    [Serializable]
    public class ProfileData
    {
        /// <summary>
        /// if the player has played the field will be true, or may be need to check the authorization and get fresh progress from there 
        /// </summary>
        [field: SerializeField]
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// On any write SaveTime updated with current time. Used for profile synchronisation 
        /// </summary>
        [field: SerializeField]
        public DateTime UtcTime { get; private set; }

        /// <summary>
        /// Unity Cloud Save Player Id
        /// </summary>
        [field: SerializeField]
        public string UnityCloudSavePlayerId { get; private set; }

        /// <summary>
        /// Player name
        /// </summary>
        [JsonProperty]
        [field: SerializeField]
        public string Name { get; set; }
        
        /// <summary>
        /// Player gender
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        [field: SerializeField]
        public GenderType Gender { get; set; } = GenderType.No;

        /// <summary>
        /// The player's mascot (profile → mascot editor)
        /// </summary>
        [JsonProperty]
        [field: SerializeField]
        public MascotLook Mascot { get; set; } = new();

        [field: SerializeField]
        public Languages LearnLanguage { get; set; } = Languages.Thai;
        public Languages NativeLanguage { get; set; } = Languages.English;
        
        [JsonConstructor]
        public ProfileData(bool isInitialized, DateTime utcTime, string unityCloudSavePlayerId)
        {
            IsInitialized = isInitialized;
            UtcTime = utcTime;
            UnityCloudSavePlayerId = unityCloudSavePlayerId;
        }

        public ProfileData()
        {
            UtcTime = DateTime.UtcNow;
            IsInitialized = true;
        }
        
        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        public void SetPlayerId(string playerId)
        {
            UnityCloudSavePlayerId = playerId;
        }
    }
}