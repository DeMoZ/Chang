using System.Collections.Generic;

/// <summary>
/// Questions data used in game
/// </summary>
namespace Chang
{
    public interface IQuestData
    {
    }
    
    public abstract class QuestDataBase : IQuestData
    {
        public abstract QuestionType QuestionType { get; }
    }

    public class QuestSelectWordData : QuestDataBase
    {
        public PhraseData CorrectWord;
        public List<PhraseData> MixWords;
        public override QuestionType QuestionType => QuestionType.SelectWord;
    }
    
    public class QuestDemonstrateWordData : QuestDataBase
    {
        public PhraseData CorrectWord;
        public override QuestionType QuestionType => QuestionType.DemonstrationWord;
        
        public QuestDemonstrateWordData(PhraseData correctWord)
        {
            CorrectWord = correctWord;
        }
    }

    public class QuestMatchWordsData : QuestDataBase
    {
        public string Question;
        public bool C;
        public override QuestionType QuestionType => QuestionType.MatchWords;
    }
}