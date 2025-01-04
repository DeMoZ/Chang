using System.Collections.Generic;

namespace Chang.FSM
{
    public interface IQuestionResult
    {
        public bool IsCorrect { get; }
        public QuestionType Type { get; }
        object[] Info { get; }
    }
}