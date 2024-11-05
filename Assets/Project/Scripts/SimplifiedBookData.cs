using System.Collections.Generic;

namespace Chang
{
    /// <summary>
    /// Used to serialize book into json 
    /// </summary>
    public class SimplifiedBookData
    {
        public string FileName;
        public List<SimplifiedLessonData> Lessons;
    }

    public class SimplifiedLessonData
    {
        public string FileName;
        public List<SimpQuestionDataBase> Questions;
    }

    public class SimpQuestionDataBase
    {
        public string FileName;
        public virtual QuestionType QuestionType { get; }
    }

    public class SimpQuestSelectWordData : SimpQuestionDataBase
    {
        public override QuestionType QuestionType => QuestionType.None;
        public string CorrectWordFileName;
        public List<string> MixWordsFileName;
    }
}