using System;
using System.Collections.Generic;
using Chang.Core;
using TriInspector;
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
        [SerializeField] private PlayStopButton _playSyllablesBtn;

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.SelectWord;

        private readonly List<CToggle> _mixWordToggles = new();
        
        private bool _isQuestInTranslation;
        private Action _onClickPlaySound;
        private Action _onClickPlaySyllablesSound;

        // the syllables clip has the same name as the normal one, so the play state goes to the button that started it
        private PlayStopButton _playingBtn;

        public void Init(bool isQuestInTranslation, 
            Word correctWord, 
            List<Word> mixWords,
            Action<int, bool> onToggleValueChanged,
            Action onClickPlaySound,
            Action onClickPlaySyllablesSound)
        {
            Debug.Log("Init SelectWordView");

            _isQuestInTranslation = isQuestInTranslation;
            _onClickPlaySound = onClickPlaySound;
            _onClickPlaySyllablesSound = onClickPlaySyllablesSound;
            _playingBtn = _playStopBtn;

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            string quesWord = _isQuestInTranslation ? correctWord.Translation : correctWord.LearnWord;
            _questionWord.Set(quesWord, correctWord.Phonetics);
            _questionWord.EnablePhonetic(!_isQuestInTranslation && correctWord.IsShowPhonetics);
            
            // init mix words
            for (var i = 0; i < mixWords.Count; i++)
            {
                var mix = Instantiate(_mixWordPrefab, _mixWordContent);
                var index = i;
            
                var word = !_isQuestInTranslation ? mixWords[i].Translation : mixWords[i].LearnWord;
                mix.Set(word, mixWords[i].Phonetics, _toggleGroup, isOn => onToggleValueChanged(index, isOn));
                mix.EnablePhonetics(_isQuestInTranslation && mixWords[i].IsShowPhonetics);
            
                _mixWordToggles.Add(mix);
            }
            
            _questionImage.sprite = correctWord.Sprite;
            
            PagesSoundController.RegisterListener(correctWord.Key, OnSoundPlay);
            if (_playStopBtn != null)
            {
                _playStopBtn.OnClick += OnClickPlaySound;
            }

            // shown only for a Thai question word with a syllables recording
            if (_playSyllablesBtn != null)
            {
                _playSyllablesBtn.gameObject.SetActive(onClickPlaySyllablesSound != null);
                _playSyllablesBtn.OnClick += OnClickPlaySyllablesSound;
            }
        }

        private void OnSoundPlay(bool play)
        {
            if (_playingBtn != null)
            {
                _playingBtn.SetPlay(!play);
            }
        }

        private void OnClickPlaySound()
        {
            SwitchSoundButton(_playStopBtn);
            _onClickPlaySound?.Invoke();
        }

        private void OnClickPlaySyllablesSound()
        {
            SwitchSoundButton(_playSyllablesBtn);
            _onClickPlaySyllablesSound?.Invoke();
        }

        // the playing sound is stopped by the next one, reset the button of the stopped sound before switching
        private void SwitchSoundButton(PlayStopButton button)
        {
            if (_playingBtn != button)
            {
                OnSoundPlay(false);
                _playingBtn = button;
            }
        }
        
        private void OnDisable()
        {
            if (_playStopBtn != null)
            {
                _playStopBtn.OnClick -= OnClickPlaySound;
            }

            if (_playSyllablesBtn != null)
            {
                _playSyllablesBtn.OnClick -= OnClickPlaySyllablesSound;
            }
            
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