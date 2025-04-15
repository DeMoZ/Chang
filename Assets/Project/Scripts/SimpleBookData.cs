using System.Collections.Generic;
using Sirenix.Utilities;

namespace Chang
{
    public class SimpleBookData
    {
        public string FileName; // json field
        public List<SimpleSection> Sections;
        public Languages Language;
    }

    public class SimpleSection
    {
        public string Section;
        public List<SimpleLessonData> Lessons;
    }

    public class SimpleLessonData
    {
        public string FileName; // json field
        public string Section;
        public string Name;
        public bool GenerateQuestMatchWordsData;
        public List<ISimpleQuestion> Questions;
    }

    public interface ISimpleQuestion
    {
        QuestionType QuestionType { get; }
        HashSet<string> GetConfigKeys();
        HashSet<string> GetSoundKeys();
        HashSet<string> GetImageKeys();
        HashSet<string> GetNeedDemonstrationKeys();
    }

    public class SimpleQuestSelectWord : ISimpleQuestion
    {
        public QuestionType QuestionType => QuestionType.SelectWord;
        public string CorrectWordFileName;
        public List<string> MixWordsFileNames;
        public string FileName; // json field

        public HashSet<string> GetConfigKeys()
        {
            var keys = new HashSet<string> { CorrectWordFileName };
            keys.AddRange(MixWordsFileNames);
            return keys;
        }

        public HashSet<string> GetSoundKeys()
        {
            var keys = new HashSet<string> { CorrectWordFileName };
            keys.AddRange(MixWordsFileNames);
            return keys;
        }
        
        public HashSet<string> GetImageKeys()
        {
            return new HashSet<string> { CorrectWordFileName };
        }

        public HashSet<string> GetNeedDemonstrationKeys()
        {
            return new HashSet<string> { CorrectWordFileName };
        }
    }

    public class SimpleQuestMatchWords : ISimpleQuestion
    {
        public QuestionType QuestionType => QuestionType.MatchWords;

        public List<string> MatchWordsFileNames;
        public string FileName; // json field

        public HashSet<string> GetConfigKeys() => new(MatchWordsFileNames);
        public HashSet<string> GetSoundKeys() => new(MatchWordsFileNames);
        public HashSet<string> GetImageKeys() => new();
        public HashSet<string> GetNeedDemonstrationKeys() => new(MatchWordsFileNames);
    }

    public class SimpleQuestDemonstrationWord : ISimpleQuestion
    {
        public string CorrectWordFileName;
        public QuestionType QuestionType => QuestionType.DemonstrationWord;

        public HashSet<string> GetConfigKeys() => new() { CorrectWordFileName };
        public HashSet<string> GetSoundKeys() => new() { CorrectWordFileName };
        public HashSet<string> GetImageKeys() => new() { CorrectWordFileName };
        public HashSet<string> GetNeedDemonstrationKeys() => new() { CorrectWordFileName };
    }
}