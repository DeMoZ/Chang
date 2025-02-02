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

        public Dictionary<string, QuestLog> Questions { get; private set; }

        [JsonConstructor]
        public ProgressData(DateTime utcTime, Dictionary<string, QuestLog> questions)
        {
            UtcTime = utcTime;
            Questions = ValidateQuestions(questions);
            // Questions = questions;
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

        public Dictionary<string, QuestLog> ValidateQuestions(Dictionary<string, QuestLog> questions)
        {
            Dictionary<string, QuestLog> result = new();

            foreach (var pair in questions)
            {
                if (pair.Value.QuestionType == QuestionType.None)
                {
                    Debug.LogWarning($"ValidateQuestion: QuestionType is None for {pair.Key}");
                    continue;
                }

                result.Add(pair.Key, pair.Value);
            }

            return result;
        }
    }
}