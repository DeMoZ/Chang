using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.UI
{
    public class SelectWordView : CScreen
    {
        [SerializeField] private Image _questionImage;
        [SerializeField] private ChangText _questionWord;
        [SerializeField] private CToggle _mixWordPrefab;
        [SerializeField] private Transform _mixWordContent;
        [SerializeField] private ToggleGroup _toggleGroup;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.SelectWord;

        public void Init(bool isQuestInTranslation, PhraseData correctWord, List<PhraseData> mixWords, Action<int, bool> onToggleValueChanged)
        {
            Debug.Log("Init SelectWordView");

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            var quesWord = isQuestInTranslation ? correctWord.Word.GetTranslation() : correctWord.Word.LearnWord;
            _questionWord.Set(quesWord, correctWord.Word.Phonetic);
            _questionWord.EnablePhonetic(!isQuestInTranslation && correctWord.ShowPhonetics);

            // init mix words
            for (var i = 0; i < mixWords.Count; i++)
            {
                var mix = Instantiate(_mixWordPrefab, _mixWordContent);
                var index = i;

                var word = !isQuestInTranslation ? mixWords[i].Word.GetTranslation() : mixWords[i].Word.LearnWord;
                mix.Set(word, mixWords[i].Word.Phonetic, _toggleGroup, isOn => onToggleValueChanged(index, isOn));
                mix.EnablePhonetic(isQuestInTranslation && mixWords[i].ShowPhonetics);
            }
        }
    }
}