using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chang.Profile
{
    [Serializable]
    public class SentenceQuestLog : IQuestLog
    {
        private readonly (int min, int max) _successSequenceRange = (0, 10);

        public string FileName { get; set; } // key: "Thai/Sentences/Market/Do_not_add_sugar" is not the file name but log key
        public Queue<LogUnit> Log { get; set; }

        /// <summary>
        /// Mark for quest [0-10]
        /// 0 - show demonstration again
        /// 9 - perfect, don't show the word for a long time again
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

        public SentenceQuestLog(string key)
        {
            FileName = key;
            Mark = ProjectConstants.DEFAULT_MARK;
            SuccessSequence = ProjectConstants.DEFAULT_SUCCESS;
            Log = new Queue<LogUnit>();
        }

        [JsonConstructor]
        public SentenceQuestLog(
            string fileName,
            int mark,
            int successSequence,
            Queue<LogUnit> log)
        {
            FileName = fileName;
            Mark = mark;
            SuccessSequence = successSequence;
            Log = log ?? new Queue<LogUnit>();
        }

        public void SetTime(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        public void AddLog(LogUnit unit)
        {
            Log.Enqueue(unit);
            if (Log.Count > ProjectConstants.LOG_LIMIT)
            {
                Log.Dequeue();
            }

            int increment = 0;
            if (unit.NeedIncrementMark)
            {
                increment = ProjectConstants.MARK_INCREMENT;
            }

            Mark = Math.Clamp(Mark + (unit.IsCorrect 
                ? increment : ProjectConstants.MARK_DICREMENT), ProjectConstants.MARK_MIN, ProjectConstants.MARK_MAX);
            SuccessSequence = unit.IsCorrect ? Math.Clamp(SuccessSequence + 1, _successSequenceRange.min, _successSequenceRange.max) : 0;
        }
    }
}