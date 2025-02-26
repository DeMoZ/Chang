using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chang.Profile
{
    [Serializable]
    public class QuestLog
    {
        private readonly (int min, int max) MarkRange = (0, 10);
        private readonly (int min, int max) SuccesSequeseRange = (0, 10);

        public Languages Language { get; set; }
        public string Section { get; set; }
        public string FileName { get; set; }
        public string Presentation { get; set; }
        public QuestionType QuestionType { get; set; }
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

        public QuestLog(string path, string presentation, QuestionType type)
        {
            FileName = path;
            Presentation = presentation;
            QuestionType = type;

            string[] parts = path.Split('/');
            Language = parts[0].ToEnum<Languages>();
            Section = parts[2];

            Mark = ProjectConstants.DEFAULT_MARK;
            SuccessSequence = ProjectConstants.DEFAULT_SUCCESS;
            Log = new Queue<LogUnit>();
        }

        [JsonConstructor]
        public QuestLog(
            string fileName,
            string presentation,
            QuestionType type,
            Languages language,
            string section,
            int mark,
            int successSequence,
            Queue<LogUnit> log)
        {
            FileName = fileName;
            Presentation = presentation;
            QuestionType = type;
            Language = language;
            Section = section;
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

            Mark = Math.Clamp(Mark + (unit.IsCorrect ? increment : ProjectConstants.MARK_DICREMENT), MarkRange.min, MarkRange.max);
            SuccessSequence = unit.IsCorrect ? Math.Clamp(SuccessSequence + 1, SuccesSequeseRange.min, SuccesSequeseRange.max) : 0;
        }
    }
}