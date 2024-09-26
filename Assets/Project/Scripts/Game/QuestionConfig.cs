using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
//using Debug = DMZ.Debug; // todo roman import debugger

[Serializable]
public class Question
{
    [field: SerializeField, ReadOnly] public QuestionType QuestionType { get; private set; }
    [SerializeField, ReadOnly] private string info = string.Empty;
    [SerializeReference] public QuestBase QuestionData;

    public void Init()
    {

        // if (QuestionData == null || QuestionData.GetQuestionType != QuestionType)
        // {
        //     Debug.Log("on validate QuestionMatchWord");
        //     info = QuestionData.GetEditorInfo();

        //     QuestionData = QuestionType switch
        //     {
        //         QuestionType.DemonstrationWord => new QuestDemonstration(),
        //         QuestionType.SelectWord => new QuestSelectWord(),
        //         QuestionType.MatchWords => new QuestMatchWord(),

        //         QuestionType.DemonstrationDialogue => new QuestDemonstrationDialogue(),
        //         _ => null,
        //     };
        // }
        QuestionType = QuestionData == null ? QuestionType.None : QuestionData.QuestionType;
        info = QuestionData == null ? string.Empty : QuestionData.EditorInfo();
    }
}

public abstract class QuestBase
{
    public virtual QuestionType QuestionType { get; } = QuestionType.None;
    public GenderType GenderType;//{get;} = GenderType.None;

    public string EditorInfo()
    {
        var gender = GenderType == GenderType.None ? string.Empty : GenderType.ToString();
        return $"{GetEditorInfo()} {gender}";
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