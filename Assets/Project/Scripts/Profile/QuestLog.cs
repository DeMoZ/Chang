using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Chang.Profile
{
    [Serializable]
    public class QuestLog
    {
        private static readonly int LogLimit = 10;
        private static readonly int DefaultMark = 4;
        private static readonly int DefaultSuccess = 0;

        private readonly (int min, int max) MarkRange = (0, 10);
        private readonly (int min, int max) SuccesSequeseRange = (0, 10);

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

        /// <summary>
        /// Every correct answer increase this value, every wrong answer set this value to 0
        /// </summary>
        public int SuccesSequese { get; private set; }

        [field: SerializeField] public Queue<LogUnit> Log { get; private set; }

        [field: SerializeField] public string FileName { get; private set; }

        [JsonConstructor]
        public QuestLog(string fileName, int mark, int succesSequese, Queue<LogUnit> log)
        {
            FileName = fileName;
            Mark = mark;
            SuccesSequese = succesSequese;
            Log = log ?? new Queue<LogUnit>();
        }

        public QuestLog(string fileName)
        {
            FileName = fileName;
            Mark = DefaultMark;
            SuccesSequese = DefaultSuccess;
            Log = new Queue<LogUnit>();
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        public void AddLog(LogUnit unit)
        {
            Log.Enqueue(unit);
            if (Log.Count > LogLimit) Log.Dequeue();

            // todo roman fix wrong result for calculation
            Mark = Math.Clamp(Mark + (unit.IsCorrect ? 1 : -1), MarkRange.min, MarkRange.max);
            SuccesSequese = unit.IsCorrect ? Math.Clamp(SuccesSequese + 1, SuccesSequeseRange.min, SuccesSequeseRange.max) : 0;
        }
    }
}