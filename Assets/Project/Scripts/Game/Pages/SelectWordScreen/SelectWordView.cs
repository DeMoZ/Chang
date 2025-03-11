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
        [SerializeField] private PlayStopButton _playStopBtn;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.SelectWord;

        private readonly List<CToggle> _mixWordToggles = new();
        
        private bool _isQuestInTranslation;

        public void Init(bool isQuestInTranslation, PhraseData correctWord, List<PhraseData> mixWords, Action<int, bool> onToggleValueChanged)
        {
            Debug.Log("Init SelectWordView");

            _isQuestInTranslation = isQuestInTranslation;

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            var quesWord = _isQuestInTranslation ? correctWord.Word.GetTranslation() : correctWord.Word.LearnWord;
            _questionWord.Set(quesWord, correctWord.Word.Phonetic);
            _questionWord.EnablePhonetic(!_isQuestInTranslation && correctWord.ShowPhonetics);

            // init mix words
            for (var i = 0; i < mixWords.Count; i++)
            {
                var mix = Instantiate(_mixWordPrefab, _mixWordContent);
                var index = i;

                var word = !_isQuestInTranslation ? mixWords[i].Word.GetTranslation() : mixWords[i].Word.LearnWord;
                mix.Set(word, mixWords[i].Word.Phonetic, _toggleGroup, isOn => onToggleValueChanged(index, isOn));
                mix.EnablePhonetics(_isQuestInTranslation && mixWords[i].ShowPhonetics);

                _mixWordToggles.Add(mix);
            }
            
            PagesSoundController.RegisterListener(correctWord.AudioClip.name, OnSoundPlay);
        }

        private void OnSoundPlay(bool play)
        {
            _playStopBtn.SetPlay(play);
        }

        private void OnDisable()
        {
            foreach(var toggle in _mixWordToggles)
            {
                Destroy(toggle.gameObject);
            }
            
            _mixWordToggles.Clear();
        }

        public void ShowHint()
        {
            _questionWord.EnablePhonetic(!_isQuestInTranslation);

            foreach (var toggle in _mixWordToggles)
            {
                toggle.EnablePhonetics(_isQuestInTranslation);
            }
        }
    }
}