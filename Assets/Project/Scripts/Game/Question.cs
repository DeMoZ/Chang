using System.Collections.Generic;
using System.Linq;
using Debug = DMZ.DebugSystem.DMZLogger;

/// <summary>
/// Questions data used in Configs only
/// </summary>
namespace Chang
{
    [System.Serializable]
    public abstract class QuestBase
    {
        public ChangTypes QuestionType { get; protected set; }

        public string EditorInfo()
        {
            return $"{GetEditorInfo()}";
        }

        public abstract QuestDataBase GetQuestData();

        protected abstract string GetEditorInfo();
    }

    [System.Serializable]
    public class QuestSelectWord : QuestBase
    {
        public QuestSelectWord()
        {
            QuestionType = ChangTypes.SelectWord;
        }

        public PhraseConfig CorrectWord;
        public List<PhraseConfig> MixWords;
        public string CorrectWordFileName { get; set; }
        public List<string> MixWordsFileNames { get; set; }

        public override QuestDataBase GetQuestData()
        {
            return new QuestSelectWordData
            {
                CorrectWord = CorrectWord.PhraseData,
                MixWords = MixWords?.Select(m => m.PhraseData).ToList()
            };
        }

        protected override string GetEditorInfo()
        {
            return CorrectWord == null ? string.Empty : CorrectWord.Key;
        }
    }

    [System.Serializable]
    public class QuestMatchWords : QuestBase
    {
        public QuestMatchWords()
        {
            QuestionType = ChangTypes.MatchWords;
        }

        public List<PhraseConfig> MatchWords;

        public override QuestDataBase GetQuestData()
        {
            return new QuestMatchWordsData(MatchWords?.Select(m => m.PhraseData).ToList());
        }

        protected override string GetEditorInfo()
        {
            // todo chang add some info
            return string.Empty;
        }
    }
}