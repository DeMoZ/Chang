using Chang.FSM;

namespace Chang
{
    public class VocabularyBus
    {
        public Lesson CurrentLesson { get; set; }
        public IQuestionResult QuestionResult { get; set; }
    }
}