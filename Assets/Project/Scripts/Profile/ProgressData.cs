using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Profile
{
    [Serializable]
    public class ProgressData
    {
        /// <summary>
        /// On any write SaveTime updated with current time. Used for profile synchronisation 
        /// </summary>
        [field: SerializeField]
        public DateTime UtcTime { get; private set; }

        public Dictionary<string, QuestLog> Questions { get; private set; }

        [JsonConstructor]
        public ProgressData(DateTime utcTime, Dictionary<string, QuestLog> questions)
        {
            UtcTime = utcTime;
            Questions = questions ?? new Dictionary<string, QuestLog>();
        }

        public ProgressData()
        {
            UtcTime = DateTime.UtcNow;
            Questions = new Dictionary<string, QuestLog>();
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }
    }
}