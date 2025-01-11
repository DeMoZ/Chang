using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.UI
{
    public class DemonstrationWordView : CScreen
    {
        [SerializeField] private Image _questionImage;
        [SerializeField] private ChangText _questionWord;
        [SerializeField] private CToggle _mixWordPrefab;
        [SerializeField] private Transform _mixWordContent;
        [SerializeField] private ToggleGroup _toggleGroup;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.DemonstrationWord;

        public void Init(bool questInStudiedLanguage, PhraseData correctWord, Action<bool> onToggleValueChanged)
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
            var mix = Instantiate(_mixWordPrefab, _mixWordContent);
            var word = questInStudiedLanguage ? correctWord.Word.GetTranslation() : correctWord.Word.Word;
            mix.Set(word, correctWord.Word.Phonetic, _toggleGroup, onToggleValueChanged);
            mix.EnablePhonetic(!questInStudiedLanguage);
        }
    }
}