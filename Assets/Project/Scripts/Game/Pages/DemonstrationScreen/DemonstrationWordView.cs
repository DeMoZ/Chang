using System;
using Chang.Core;
using TriInspector;
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
        [SerializeField] private PlayStopButton _playStopBtn;
        [SerializeField] private PlayStopButton _playSlowBtn;
        [SerializeField] private PlayStopButton _playSyllablesBtn;

        [Tooltip("A toggle placed in the screen (the design's reveal chip). When set, it is used instead of instantiating Mix Word Prefab")]
        [SerializeField] private CToggle _translationToggle;

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.DemonstrationWord;

        private Action _onClickPlaySound;
        private Action _onClickPlaySlowSound;
        private Action _onClickPlaySyllablesSound;

        // the slow and syllables clips have the same name as the normal one, so the play state goes to the button that started it
        private PlayStopButton _playingBtn;

        public void Init(Word correctWord,
            Action<bool> onToggleValueChanged,
            Action onClickPlaySound,
            Action onClickPlaySlowSound,
            Action onClickPlaySyllablesSound)
        {
            Debug.Log("Init SelectWordView");

            _onClickPlaySound = onClickPlaySound;
            _onClickPlaySlowSound = onClickPlaySlowSound;
            _onClickPlaySyllablesSound = onClickPlaySyllablesSound;
            _playingBtn = _playStopBtn;

            if (_translationToggle == null)
            {
                foreach (Transform child in _mixWordContent)
                {
                    Destroy(child.gameObject);
                }
            }

            // init learning language word
            var quesWord = correctWord.LearnWord;
            _questionWord.Set(quesWord, correctWord.Phonetics);
            _questionWord.EnablePhonetic(true);

            // init translation words
            var mix = _translationToggle != null ? _translationToggle : Instantiate(_mixWordPrefab, _mixWordContent);
            var word = correctWord.Translation;
            mix.Set(word, correctWord.Phonetics, _toggleGroup, onToggleValueChanged);
            mix.EnablePhonetics(false);
            mix.SetIsOnWithoutNotify(false);
            PagesSoundController.RegisterListener(correctWord.Key, OnSoundPlay);
            if (_playStopBtn != null)
            {
                _playStopBtn.OnClick += OnClickPlaySound;
            }

            // shown only when the word has a slow recording
            if (_playSlowBtn != null)
            {
                _playSlowBtn.gameObject.SetActive(onClickPlaySlowSound != null);
                _playSlowBtn.OnClick += OnClickPlaySlowSound;
            }

            // shown only when the word has a syllables recording
            if (_playSyllablesBtn != null)
            {
                _playSyllablesBtn.gameObject.SetActive(onClickPlaySyllablesSound != null);
                _playSyllablesBtn.OnClick += OnClickPlaySyllablesSound;
            }

            _questionImage.sprite = correctWord.Sprite;
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

        private void OnClickPlaySlowSound()
        {
            SwitchSoundButton(_playSlowBtn);
            _onClickPlaySlowSound?.Invoke();
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

            if (_playSlowBtn != null)
            {
                _playSlowBtn.OnClick -= OnClickPlaySlowSound;
            }

            if (_playSyllablesBtn != null)
            {
                _playSyllablesBtn.OnClick -= OnClickPlaySyllablesSound;
            }
        }
    }
}