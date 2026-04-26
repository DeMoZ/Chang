using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Profile
{
    [Serializable]
    public class OldProgressData
    {
        /// <summary>
        /// On any write SaveTime updated with current time. Used for profile synchronization 
        /// </summary>
        [field: SerializeField]
        public DateTime UtcTime { get; private set; }

        [JsonProperty("ThaiQuestLogs")]
        public Dictionary<string, VocabularyQuestLog> Log { get; private set; }
        
        #region "May be move to ProfileData"
        /// <summary>
        /// Position for scroll in GameBook screen
        /// </summary>
        [field: SerializeField]
        public float GameBookScrollPosition { get; set; } = 1f;
        #endregion
        
        [JsonConstructor]
        public OldProgressData(DateTime utcTime, Dictionary<string, VocabularyQuestLog> log, Dictionary<string, VocabularyQuestLog> questions)
        {
            UtcTime = utcTime;
            Log = Validate(log);
        }

        public OldProgressData()
        {
            UtcTime = DateTime.UtcNow;
            Log = new Dictionary<string, VocabularyQuestLog>();
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        public Dictionary<string, VocabularyQuestLog> GetLogs(Languages language)
        {
            switch (language)
            {
                case Languages.Thai:
                    return Log;
                
                default:
                    Debug.LogWarning($"No QuestLog for Language: {language}");
                    return new Dictionary<string, VocabularyQuestLog>();
            }
        }
        
        private Dictionary<string, VocabularyQuestLog> Validate(Dictionary<string, VocabularyQuestLog> log)
        {
            Dictionary<string, VocabularyQuestLog> result = new();
            log ??= new Dictionary<string, VocabularyQuestLog>();

            foreach (var pair in log)
            {
                if (pair.Value.QuestionType == ChangTypes.None)
                {
                    Debug.LogWarning($"ValidateQuestion: QuestionType is None for {pair.Key}");
                    continue;
                }

                if (string.IsNullOrEmpty(pair.Value.Section))
                {
                    Debug.LogWarning($"ValidateQuestion: Section is null for {pair.Key}");
                    continue;
                }

                result.Add(pair.Key, pair.Value);
            }

            return result;
        }
    }
}