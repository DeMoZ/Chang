namespace Chang.Profile
{
    public interface IQuestLog
    {
        public string Section { get; set; }
        public QuestionType QuestionType { get; set; }
    }
}