namespace Chang.Profile
{
    public interface IQuestLog
    {
        public string Section { get; set; }
        public ChangTypes QuestionType { get; set; }
    }
}