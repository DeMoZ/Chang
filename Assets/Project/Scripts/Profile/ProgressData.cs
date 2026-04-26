using System;
using System.Collections.Generic;
using DMZ.DebugSystem;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Profile
{
    public class ProgressData<T> where T : IQuestLog
    {
        /// <summary>
        /// On any write SaveTime updated with current time. Used for profile synchronization 
        /// </summary>
        [field: SerializeField]
        public DateTime UtcTime { get; private set; }

        [JsonProperty("ThaiQuestLogs")]
        public Dictionary<string, T> Log { get; private set; }
        
        [field: SerializeField]
        public float ScrollPosition { get; set; } = 1f;
        
        [JsonConstructor]
        public ProgressData(DateTime utcTime, Dictionary<string, T> log)
        {
            UtcTime = utcTime;
            Log = Validate(log);
        }
        
        public ProgressData()
        {
            UtcTime = DateTime.UtcNow;
            Log = new Dictionary<string, T>();
        }

        
        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }
        
        private Dictionary<string, T> Validate(Dictionary<string, T> log)
        {
            Dictionary<string, T> result = new();
            log ??= new Dictionary<string, T>();

            foreach (var pair in log)
            {
                if (pair.Value.QuestionType == ChangTypes.None)
                {
                    DMZLogger.LogWarning($"ValidateQuestion: QuestionType is None for {pair.Key}");
                    continue;
                }

                if (string.IsNullOrEmpty(pair.Value.Section))
                {
                    DMZLogger.LogWarning($"ValidateQuestion: Section is null for {pair.Key}");
                    continue;
                }

                result.Add(pair.Key, pair.Value);
            }

            return result;
        }
    }
}