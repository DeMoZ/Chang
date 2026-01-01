using System.Collections.Generic;
using System.Text;

namespace Chang.Sentences
{
    public class SentencesBookData
    {
        // public string FileName; // JSON field
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
        public string FileName; // JSON field used as a dictioanary key
        public string SectionName;
        // public string Name;
        public List<IQuestion> Questions;
    }

    public interface ISentenceQuestion : IQuestion
    {
        // "Coconut.Mango.Banana.Watermelon" to keep in the log and find the quest in the book data
        string LogKey { get; }
    }
    
    public class SentenceSelectWords : ISentenceQuestion
    {
        private HashSet<string> _keys;
        private string _logKey;
        
        public QuestionType QuestionType => QuestionType.SentenceSelectWords;

        public string ImageFileName;
        public string SoundFileName;
        public List<string> CompareWordsFileNames;
        public List<string> DisplayWordsFileNames;
        public List<string> MixWordsFileNames;
        
        private HashSet<string> Keys => _keys ??= GetWords();
        
        public HashSet<string> GetConfigKeys() => Keys;
        public HashSet<string> GetSoundKeys() => Keys;
        public HashSet<string> GetImageKeys() => Keys;
        
        public string LogKey => _logKey ??= GetLogKey();

        public HashSet<string> GetNeedDemonstrationKeys()
        {
            return new HashSet<string>(CompareWordsFileNames);
        }
        
        private HashSet<string> GetWords()
        {
            HashSet<string> newList = new HashSet<string>(CompareWordsFileNames);
            newList.UnionWith(MixWordsFileNames);
            return newList;
        }
        
        private string GetLogKey()
        {
            StringBuilder logKey = new();
            foreach (string key in CompareWordsFileNames)
            {
                string[] split = key.Split('/');
                logKey.AppendJoin('.',  split[^1]);
            }
                            
            return logKey.ToString();
        }
    }
}