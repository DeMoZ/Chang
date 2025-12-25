using System.Collections.Generic;

namespace Chang.Sentences
{
    public class SentencesBookData
    {
        // public string FileName; // json field
        public List<SectionData> Sections;
        // public Languages Language;
        //
        // private Dictionary<string, SimpleSection> _sectionsDict;
        // public Dictionary<string, SimpleSection> SectionsDict => _sectionsDict ??= Sections.ToDictionary(s => s.Section);
    }

    public class SectionData
    {
        public string Section;
        public List<LessonData> Lessons;
    }

    public class LessonData
    {
        public string FileName; // json field used as a dictioanary key
        public string SectionName;
        // public string Name;
        public List<IQuestion> Questions;
    }

    public class SentenceSelectWords : IQuestion
    {
        private HashSet<string> _keys;
        public QuestionType QuestionType => QuestionType.SentenceSelectWords;

        public string ImageFileName;
        public string SoundFileName;
        public List<string> CompareWordsFileNames;
        public List<string> DisplayWordsFileNames;
        public List<string> MixWordsFileNames;

        public HashSet<string> GetConfigKeys() => Keys;
        public HashSet<string> GetSoundKeys() => Keys;
        public HashSet<string> GetImageKeys() => Keys;

        public HashSet<string> GetNeedDemonstrationKeys()
        {
            return new HashSet<string>(CompareWordsFileNames);
        }

        public HashSet<string> Keys => _keys ??= GetWords();

        private HashSet<string> GetWords()
        {
            HashSet<string> newList = new HashSet<string>(CompareWordsFileNames);
            newList.UnionWith(MixWordsFileNames);
            return newList;
        }
    }
}