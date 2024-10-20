using System.Collections.Generic;
using Chang.FSM;
using Chang.Resources;

namespace Chang
{
    public class VocabularyBus
    {
        public ScreenManager ScreenManager { get; set; }
        public IResourcesManager ResourcesManager { get; set; }
        public Queue<Question> Questions { get; set; }
        public Question CurrentQuestion { get; set; }
        public IQuestionResult QuestionResult { get; set; }
    }
}