using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

namespace Chang.Vocabulary
{
    public class VocabularyBookData
    {
        public string FileName; // json field
        public List<SectionData> Sections;
        public Languages Language;

        private Dictionary<string, SectionData> _sectionsDict;
        public Dictionary<string, SectionData> SectionsDict => _sectionsDict ??= Sections.ToDictionary(s => s.Section);
    }

    public class SectionData
    {
        public string Section;
        public List<LessonData> Lessons;
    }

    public class LessonData
    {
        public string FileName; // JSON field
        public string SectionName;
        public string Name;
        public bool GenerateQuestMatchWordsData;
        public List<IQuestion> Questions;
    }

    public class QuestSelectWord : IQuestion
    {
        public QuestionType QuestionType => QuestionType.SelectWord;
        public string CorrectWordFileName;
        public List<string> MixWordsFileNames;
        public string FileName; // JSON field

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

    public class QuestMatchWords : IQuestion
    {
        public QuestionType QuestionType => QuestionType.MatchWords;

        public List<string> MatchWordsFileNames;
        public string FileName; // json field

        public HashSet<string> GetConfigKeys() => new(MatchWordsFileNames);
        public HashSet<string> GetSoundKeys() => new(MatchWordsFileNames);
        public HashSet<string> GetImageKeys() => new();
        public HashSet<string> GetNeedDemonstrationKeys() => new(MatchWordsFileNames);
    }

    public class QuestDemonstrationWord : IQuestion
    {
        public string CorrectWordFileName;
        public QuestionType QuestionType => QuestionType.DemonstrationWord;

        public HashSet<string> GetConfigKeys() => new() { CorrectWordFileName };
        public HashSet<string> GetSoundKeys() => new() { CorrectWordFileName };
        public HashSet<string> GetImageKeys() => new() { CorrectWordFileName };
        public HashSet<string> GetNeedDemonstrationKeys() => new() { CorrectWordFileName };
    }
}