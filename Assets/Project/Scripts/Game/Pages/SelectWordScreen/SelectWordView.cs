using System;
using System.Collections.Generic;
using Chang.Core;
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

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.SelectWord;

        private readonly List<CToggle> _mixWordToggles = new();
        
        private bool _isQuestInTranslation;
        private Action _onClickPlaySound;

        public void Init(bool isQuestInTranslation, 
            Word correctWord, 
            List<Word> mixWords,
            Action<int, bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            Debug.Log("Init SelectWordView");

            _isQuestInTranslation = isQuestInTranslation;
            _onClickPlaySound = onClickPlaySound;

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            string quesWord = _isQuestInTranslation ? correctWord.GetTranslation() : correctWord.LearnWord;
            _questionWord.Set(quesWord, correctWord.Phonetics);
            _questionWord.EnablePhonetic(!_isQuestInTranslation && correctWord.IsShowPhonetics);
            
            // init mix words
            for (var i = 0; i < mixWords.Count; i++)
            {
                var mix = Instantiate(_mixWordPrefab, _mixWordContent);
                var index = i;
            
                var word = !_isQuestInTranslation ? mixWords[i].GetTranslation() : mixWords[i].LearnWord;
                mix.Set(word, mixWords[i].Phonetics, _toggleGroup, isOn => onToggleValueChanged(index, isOn));
                mix.EnablePhonetics(_isQuestInTranslation && mixWords[i].IsShowPhonetics);
            
                _mixWordToggles.Add(mix);
            }
            
            _questionImage.sprite = correctWord.Sprite;
            
            PagesSoundController.RegisterListener(correctWord.Key, OnSoundPlay);
            _playStopBtn.OnClick += OnClickPlaySound;
        }

        private void OnSoundPlay(bool play)
        {
            _playStopBtn.SetPlay(!play);
        }

        private void OnClickPlaySound()
        {
            _onClickPlaySound?.Invoke();
        }
        
        private void OnDisable()
        {
            _playStopBtn.OnClick -= OnClickPlaySound;
            
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