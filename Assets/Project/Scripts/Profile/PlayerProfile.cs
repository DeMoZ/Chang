using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Profile
{
    public class PlayerProfile : IDisposable
    {
        public ProfileData ProfileData;
        public ProgressData ProgressData;

        public PlayerProfile()
        {
        }

        /// <summary>
        /// initialize with saved data from Prefs/Remote etc.
        /// </summary>
        public void Init()
        {
        }

        public void Dispose()
        {
        }
    }

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
    }

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

    [Serializable]
    public class QuestLog
    {
        private const int LogLimit = 10;
        private const int DefaultMark = 4;
        private const int MinMark = 0;
        private const int MaxMark = 9;

        /// <summary>
        /// Last time quest approach
        /// </summary>
        [field: SerializeField]
        public DateTime UtcTime { get; set; }

        /// <summary>
        /// Mark for quest [0-10]
        /// 0 - show demonstration again
        /// 9 - perfect, dont show the word for a long time again
        /// </summary>
        [field: SerializeField]
        public int Mark { get; private set; }

        [field: SerializeField] public Queue<LogUnit> Log { get; private set; }

        [field: SerializeField] public string FileName { get; private set; }

        [JsonConstructor]
        public QuestLog(string fileName, int mark, Queue<LogUnit> log)
        {
            FileName = fileName;
            Mark = mark;
            Log = log ?? new Queue<LogUnit>();
        }

        public QuestLog(string fileName)
        {
            FileName = fileName;
            Mark = DefaultMark;
            Log = new Queue<LogUnit>();
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        public void AddLog(LogUnit unit)
        {
            Log.Enqueue(unit);

            while (Log.Count > LogLimit)
                Log.Dequeue();

            Mark += unit.IsCorrect ? 1 : -1;
            Math.Clamp(Mark, MinMark, MaxMark);
        }
    }

    [Serializable]
    public class LogUnit
    {
        [field: SerializeField] public DateTime UtcTime { get; private set; }
        [field: SerializeField] public bool IsCorrect { get; private set; }

        public LogUnit(DateTime utcTime, bool isCorrect)
        {
            UtcTime = utcTime;
            IsCorrect = isCorrect;
        }
    }
}