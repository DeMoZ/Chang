using System;
using System.Collections.Generic;
using Google.Type;
using Newtonsoft.Json;
using DateTime = System.DateTime;

namespace Chang.Profile
{
    public class PlayerProfile : IDisposable
    {
        public PlayerData PlayerData;

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

    public class PlayerData
    {
        /// <summary>
        /// if the player has played the field will be true, or may be need to check the authorization and get fresh progress from there 
        /// </summary>
        public bool IsInitialized;

        /// <summary>
        /// On any write SaveTime updated with current time. Used for profile synchronisation 
        /// </summary>
        public DateTime UtcTime { get; private set; }

        public Dictionary<string, QuestLog> Questions;

        [JsonConstructor]
        public PlayerData(DateTime utcTime, Dictionary<string, QuestLog> questions, bool isInitialized)
        {
            UtcTime = utcTime;
            Questions = questions?? new Dictionary<string, QuestLog>();
            IsInitialized = isInitialized;
        }
        
        public PlayerData()
        {
            UtcTime = DateTime.UtcNow;
            Questions = new Dictionary<string, QuestLog>();
            IsInitialized = true;
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }
    }

    public class QuestLog
    {
        private const int LogLimit = 10;
        private const int DefaultMark = 4;
        private const int MinMark = 0;
        private const int MaxMark = 9;


        /// <summary>
        /// Last time quest approach
        /// </summary>
        public DateTime UtcTime { get; set; }

        /// <summary>
        /// Mark for quest [0-10]
        /// 0 - show demonstration again
        /// 9 - perfect, dont show the word for a long time again
        /// </summary>
        public int Mark { get; private set; }

        public Queue<LogUnit> Log { get; private set; }

        public string FileName { get; private set; }

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

    public class LogUnit
    {
        public DateTime UtcTime { get; private set; }
        public bool IsCorrect { get; private set; }

        public LogUnit(DateTime utcTime, bool isCorrect)
        {
            UtcTime = utcTime;
            IsCorrect = isCorrect;
        }
    }
}