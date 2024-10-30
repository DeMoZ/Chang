using System.Collections.Generic;

namespace Chang.FSM
{
    public interface IQuestionResult
    {
        public bool IsCorrect { get; }
        public QuestionType Type { get; }
        public List<string> Info { get; }
    }
}