namespace Chang.FSM
{
    public interface IQuestionResult
    {
        public ChangTypes Type { get; }
        public string Key { get; }
        public string Presentation { get; }
        public bool IsCorrect { get; }
        public bool IsHintUsed { get; }
    }
}