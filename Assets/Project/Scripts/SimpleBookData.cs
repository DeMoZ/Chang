using System.Collections.Generic;

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
    }

    public class SimpleQuestSelectWord : ISimpleQuestion
    {
        public QuestionType QuestionType => QuestionType.SelectWord;
        public string CorrectWordFileName;
        public List<string> MixWordsFileNames;
        public string FileName; // json field
    }

    public class SimpleQuestMatchWords : ISimpleQuestion
    {
        public QuestionType QuestionType => QuestionType.MatchWords;

        public List<string> MatchWordsFileNames;
        public string FileName; // json field
    }

    public class SimpleQuestDemonstrationWord : ISimpleQuestion
    {
        public string CorrectWordFileName;
        public QuestionType QuestionType => QuestionType.DemonstrationWord;
    }
}