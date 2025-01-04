using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public abstract class QuestBase
    {
        public virtual QuestionType QuestionType { get; protected set; } = QuestionType.None;

        public void OverrideType(QuestionType type)
        {
            QuestionType = type;
        }

        public string EditorInfo()
        {
            // var gender = GenderType == GenderType.None ? string.Empty : GenderType.ToString();
            // return $"{GetEditorInfo()} {gender}";
            return $"{GetEditorInfo()}";
        }

        protected abstract string GetEditorInfo();
    }

    public class QuestSelectWord : QuestBase
    {
        public override QuestionType QuestionType { get; protected set; } = QuestionType.SelectWord;

        [InlineEditor(Expanded = true)] public PhraseConfig CorrectWord;
        public List<PhraseConfig> MixWords;

        protected override string GetEditorInfo()
        {
            return CorrectWord == null ? string.Empty : CorrectWord.Key;
        }
    }

    public class QuestMatchWord : QuestBase
    {
        public override QuestionType QuestionType { get; protected set; } = QuestionType.MatchWords;
        public string Question = "Select words";
        public bool C;

        protected override string GetEditorInfo()
        {
            throw new NotImplementedException();
        }
    }
}