using System;
using System.Collections.Generic;

namespace Chang.GoogleSheets
{
    [Serializable]
    public class SentencesBookSection
    {
        public Languages Language;
        public string Section;
        public string SectionKey;
        public List<Lesson> SectionLessons;
    }
}