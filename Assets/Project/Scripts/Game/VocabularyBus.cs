using Chang.FSM;
using Chang.Resources;

namespace Chang
{
    public class VocabularyBus
    {
        public ScreenManager ScreenManager { get; set; }
        public Lesson CurrentLesson { get; set; }
        public IQuestionResult QuestionResult { get; set; }
    }
}