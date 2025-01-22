using System.Collections.Generic;

namespace Chang
{
    /// <summary>
    /// Used to serialize book into json 
    /// </summary>
    public class SimpleBookData
    {
        public string FileName;
        public List<SimpleLessonData> Lessons;
    }

    public class SimpleLessonData
    {
        public string FileName;
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
    }

    public class SimpleQuestMatchWords : ISimpleQuestion
    {
        public QuestionType QuestionType => QuestionType.MatchWords;
        public List<string> MatchWordsFileNames;
    }

    public class SimpleQuestDemonstrationWord : ISimpleQuestion
    {
        public string CorrectWordFileName;
        public QuestionType QuestionType => QuestionType.DemonstrationWord;
    }
}