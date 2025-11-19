using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        
    }
    
    public interface IQuestion
    {
        QuestionType QuestionType { get; }
        HashSet<string> GetConfigKeys();
        HashSet<string> GetSoundKeys();
        HashSet<string> GetImageKeys();
    }
    
    public class SentenceSelectWords : IQuestion
    {
        private HashSet<string> _keys;
        public QuestionType QuestionType => QuestionType.SentenceSelectWords;

        public List<string> CompareWordsFileNames;  // compare result with these words
        public List<string> DisplayWordsFileNames;  // show these words to put words into
        public List<string> MixWordsFileNames;      // mix words to choose from
        
        public HashSet<string> GetConfigKeys() => Keys;
        public HashSet<string> GetSoundKeys() => Keys;
        public HashSet<string> GetImageKeys() => Keys;

        public HashSet<string> Keys => _keys ??= GetWords();
        
        private HashSet<string> GetWords ()
        {
            
            HashSet<string> newList = new HashSet<string>(CompareWordsFileNames);
            newList.UnionWith(DisplayWordsFileNames);
            return newList;
        }
    }
}