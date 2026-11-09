using System;
using System.Collections.Generic;

namespace Chang.Profile
{
    public interface IQuestLog
    {
        public string FileName { get; }
        public Queue<LogUnit> Log { get; set; }
        public int Mark { get; set; }
        public int SuccessSequence { get; set; }
        public DateTime UtcTime { get; set; }
    }
}
