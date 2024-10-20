namespace Chang.FSM
{
    public interface IQuestionResult
    {
        public bool IsCorrect { get; }
        public QuestionType Type { get; }
        public string Info { get; }
    }
}