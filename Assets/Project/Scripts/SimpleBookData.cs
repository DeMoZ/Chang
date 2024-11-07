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
        public List<SimpleQuestionDataBase> Questions;
    }

    public class SimpleQuestionDataBase
    {
        public string FileName;
        public virtual QuestionType QuestionType { get; }
    }

    public class SimpleQuestSelectWordData : SimpleQuestionDataBase
    {
        public override QuestionType QuestionType => QuestionType.None;
        public string CorrectWordFileName;
        public List<string> MixWordsFileName;
    }
}