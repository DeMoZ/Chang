using System;
using System.Collections.Generic;

/// <summary>
/// Questions data used in game
/// </summary>
namespace Chang
{
    public interface IQuestData
    {
        public QuestionType QuestionType { get; }
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
        public List<PhraseData> MatchWords;
        public override QuestionType QuestionType => QuestionType.MatchWords;
        
        public QuestMatchWordsData(List<PhraseData> words)
        {
            MatchWords = words;
        }
    }
    
    public class QuestSentenceSelectWordData : QuestDataBase, IDisposable
    {
        public List<SequencePhraseData> CompareSequence = new List<SequencePhraseData>();
        public List<SequencePhraseData> DisplaySequence = new List<SequencePhraseData>();
        public List<SequencePhraseData> MixWords = new List<SequencePhraseData>();
        public Queue<SequencePhraseData> PlaceHolderPool = new Queue<SequencePhraseData>();
        public override QuestionType QuestionType => QuestionType.SentenceSelectWords;

        public void Dispose()
        {
            CompareSequence.Clear();
            DisplaySequence.Clear();
            MixWords.Clear();
            PlaceHolderPool.Clear();
        }
    }
}