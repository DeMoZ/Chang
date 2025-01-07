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
        public List<SimpleQuestionBase> Questions;
    }
    
    public class SimpleQuestionBase
    { 
        public string FileName { get; set; }
        public virtual QuestionType QuestionType { get; set; } // set for dissetialization
    }

    public class SimpleQuestSelectWord : SimpleQuestionBase
    {
        public override QuestionType QuestionType => QuestionType.SelectWord;
        public string CorrectWordFileName;
        public List<string> MixWordsFileName;
    }
    
    public class SimpleQuestDemonstrationWord : SimpleQuestionBase
    {
        public override QuestionType QuestionType => QuestionType.DemonstrationWord;
        public string CorrectWordFileName;
    }
}