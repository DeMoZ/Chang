using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Profile
{
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
}