using System;
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

        private string Question = "Match Words"; // todo roman remove this

        public List<PhraseConfig> WordsToMatch;
        
        public override QuestDataBase GetQuestData()
        {
            return new QuestMatchWordsData
            {
                Question = Question,
            };
        }

        protected override string GetEditorInfo()
        {
            //throw new NotImplementedException();
            return string.Empty;
        }
    }
}