namespace Chang.FSM
{
    public interface IQuestionResult
    {
        public string Key { get; }
        public bool IsCorrect { get; }
        public QuestionType Type { get; }
        object[] Info { get; }
    }
}