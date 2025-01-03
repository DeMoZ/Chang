using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    // [Serializable]
    // public class Question
    // {
    //     [field: SerializeField, ReadOnly] public QuestionType QuestionType { get; private set; }
    //     [SerializeField, ReadOnly] private string info = string.Empty;
    //
    //     [SerializeReference, InlineEditor(Expanded = true)]
    //     public QuestionConfig QuestionConfig;
    //
    //   //  [SerializeReference] public QuestBase QuestionData;
    //
    //     public void Init()
    //     {
    //         QuestionType = QuestionConfig == null ? QuestionType.None : QuestionConfig.QuestionType;
    //         info = QuestionConfig == null ? string.Empty : QuestionConfig.Info;
    //     }
    // }

    public abstract class QuestBase
    {
        public virtual QuestionType QuestionType { get; } = QuestionType.None;
        // public GenderType GenderType;

        public string EditorInfo()
        {
            // var gender = GenderType == GenderType.None ? string.Empty : GenderType.ToString();
            // return $"{GetEditorInfo()} {gender}";
            return $"{GetEditorInfo()}";
        }

        protected abstract string GetEditorInfo();
    }

    public class QuestDemonstration : QuestBase
    {
        public override QuestionType QuestionType { get; } = QuestionType.DemonstrationWord;

        [InlineEditor(Expanded = true)] public PhraseConfig PhraseConfig;

        public Sprite Sprite;
        public string Explanation;

        protected override string GetEditorInfo()
        {
            return PhraseConfig == null ? string.Empty : PhraseConfig.Key ?? string.Empty;
        }
    }

    public class QuestDemonstrationDialogue : QuestBase
    {
        public override QuestionType QuestionType { get; } = QuestionType.DemonstrationDialogue;

        public List<PhraseConfig> Dialogue;

        public Sprite Sprite;
        public string Explanation;

        protected override string GetEditorInfo()
        {
            return string.Join("=>", Dialogue.Where(phrase => phrase != null).Select(phrase => phrase.Key));
        }
    }

    public class QuestSelectWord : QuestBase
    {
        public override QuestionType QuestionType { get; } = QuestionType.SelectWord;

        [InlineEditor(Expanded = true)] public PhraseConfig CorrectWord;
        public List<PhraseConfig> MixWords;

        protected override string GetEditorInfo()
        {
            return CorrectWord == null ? string.Empty : CorrectWord.Key;
        }
        // todo roman addd some mix of data depending on question type
    }

    public class QuestMatchWord : QuestBase
    {
        public override QuestionType QuestionType { get; } = QuestionType.MatchWords;
        public string Question = "Select words";
        public bool C;

        protected override string GetEditorInfo()
        {
            throw new NotImplementedException();
        }
        // todo roman addd some mix of data depending on question type
    }
}