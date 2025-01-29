using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Debug = DMZ.DebugSystem.DMZLogger;

/// <summary>
/// Questions data used in Configs only
/// </summary>
namespace Chang
{
    public abstract class QuestBase
    {
        public QuestionType QuestionType { get; protected set; }

        public string EditorInfo()
        {
            return $"{GetEditorInfo()}";
        }

        public abstract QuestDataBase GetQuestData();

        protected abstract string GetEditorInfo();
    }

    public class QuestSelectWord : QuestBase
    {
        public QuestSelectWord()
        {
            QuestionType = QuestionType.SelectWord;
        }

        [InlineEditor(Expanded = true)] public PhraseConfig CorrectWord;
        public List<PhraseConfig> MixWords;

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

    public class QuestMatchWords : QuestBase
    {
        public QuestMatchWords()
        {
            QuestionType = QuestionType.MatchWords;
        }

        public List<PhraseConfig> MatchWords;

        public override QuestDataBase GetQuestData()
        {
            return new QuestMatchWordsData(MatchWords?.Select(m => m.PhraseData).ToList());
        }

        protected override string GetEditorInfo()
        {
            // todo roman add some info
            return string.Empty;
        }
    }
}