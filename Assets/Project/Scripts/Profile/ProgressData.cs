using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

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

        public Dictionary<string, QuestLog> ThaiQuestLogs { get; private set; }
        
        #region "May be move to ProfileData"
        /// <summary>
        /// Position for scroll in GameBook screen
        /// </summary>
        [field: SerializeField]
        public float GameBookScrollPosition { get; set; } = 1f;
        #endregion
        
        [JsonConstructor]
        public ProgressData(DateTime utcTime, Dictionary<string, QuestLog> thaiQuestLogs, Dictionary<string, QuestLog> questions)
        {
            UtcTime = utcTime;
            
            // todo chang questions is Obsolete: Use thaiQuestLogs instead
            // that is also will move into something like dictionary of dictionaries.
            // also ProgressData will be used only for one language and will be loaded as one ProgressData per language
            thaiQuestLogs ??= questions;
            ThaiQuestLogs = ValidateQuestions(thaiQuestLogs);
        }

        public ProgressData()
        {
            UtcTime = DateTime.UtcNow;
            ThaiQuestLogs = new Dictionary<string, QuestLog>();
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        public Dictionary<string, QuestLog> GetQuestLogs(Languages language)
        {
            switch (language)
            {
                case Languages.Thai:
                    return ThaiQuestLogs;
                
                default:
                    Debug.LogWarning($"No QuestLog for Language: {language}");
                    return new Dictionary<string, QuestLog>();
            }
        }
        
        private Dictionary<string, QuestLog> ValidateQuestions(Dictionary<string, QuestLog> questLogs)
        {
            Dictionary<string, QuestLog> result = new();
            questLogs ??= new Dictionary<string, QuestLog>();

            foreach (var pair in questLogs)
            {
                if (pair.Value.QuestionType == QuestionType.None)
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