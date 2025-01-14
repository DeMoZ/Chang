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

        public void Init(bool questInStudiedLanguage, PhraseData correctWord, List<PhraseData> mixWords, Action<int, bool> onToggleValueChanged)
        {
            Debug.Log("Init SelectWordView");

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            // init thai word
            var quesWord = !questInStudiedLanguage ? correctWord.Word.GetTranslation() : correctWord.Word.Word;
            _questionWord.Set(quesWord, correctWord.Word.Phonetic);
            _questionWord.EnablePhonetic(questInStudiedLanguage);

            // init mix words
            for (var i = 0; i < mixWords.Count; i++)
            {
                var mix = Instantiate(_mixWordPrefab, _mixWordContent);
                var index = i;

                var word = questInStudiedLanguage ? mixWords[i].Word.GetTranslation() : mixWords[i].Word.Word;
                mix.Set(word, mixWords[i].Word.Phonetic, _toggleGroup, isOn => onToggleValueChanged(index, isOn));
                mix.EnablePhonetic(!questInStudiedLanguage);
            }
        }

        public void EnablePhonetic(bool enable)
        {
            // todo roman if the quest is from questInStudiedLanguage phonetic can be enabled
            // otherwise no phonetic
        }
    }
}