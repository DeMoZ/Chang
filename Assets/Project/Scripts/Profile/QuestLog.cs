using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        
        public string FileName { get; set; }
        public Queue<LogUnit> Log { get; set; }
        
        /// <summary>
        /// Mark for quest [0-10]
        /// 0 - show demonstration again
        /// 9 - perfect, dont show the word for a long time again
        /// </summary>
        public int Mark { get; set; }
       
        /// <summary>
        /// Every correct answer increase this value, every wrong answer set this value to 0
        /// </summary>
        public int SuccessSequence { get; set; }
        
        /// <summary>
        /// Last time quest approach
        /// </summary>
        public DateTime UtcTime { get; set; }
        
        public QuestLog(string fileName)
        {
            FileName = fileName;
            Mark = DefaultMark;
            SuccessSequence = DefaultSuccess;
            Log = new Queue<LogUnit>();
        }
        
        [JsonConstructor]
        public QuestLog(string fileName, int mark, int successSequence, Queue<LogUnit> log)
        {
            FileName = fileName;
            Mark = mark;
            SuccessSequence = successSequence;
            Log = log ?? new Queue<LogUnit>();
        }

        // public void SetDefaultData(string key)
        // {
        //     FileName = key;
        //     Mark = DefaultMark;
        //     SuccessSequence = DefaultSuccess;
        //     Log = new Queue<LogUnit>();
        // }
        
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
            SuccessSequence = unit.IsCorrect ? Math.Clamp(SuccessSequence + 1, SuccesSequeseRange.min, SuccesSequeseRange.max) : 0;
        }
    }
}