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

        [Tooltip("A toggle placed in the screen (the design's reveal chip). When set, it is used instead of instantiating Mix Word Prefab")]
        [SerializeField] private CToggle _translationToggle;

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.DemonstrationWord;

        private Action _onClickPlaySound;
        private Action _onClickPlaySlowSound;

        // the slow clip has the same name as the normal one, so the play state goes to the button that started it
        private bool _isSlowSound;

        public void Init(Word correctWord,
            Action<bool> onToggleValueChanged,
            Action onClickPlaySound,
            Action onClickPlaySlowSound)
        {
            Debug.Log("Init SelectWordView");

            _onClickPlaySound = onClickPlaySound;
            _onClickPlaySlowSound = onClickPlaySlowSound;
            _isSlowSound = false;

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

            _questionImage.sprite = correctWord.Sprite;
        }

        private void OnSoundPlay(bool play)
        {
            PlayStopButton button = _isSlowSound ? _playSlowBtn : _playStopBtn;

            if (button != null)
            {
                button.SetPlay(!play);
            }
        }

        private void OnClickPlaySound()
        {
            SwitchSoundButton(false);
            _onClickPlaySound?.Invoke();
        }

        private void OnClickPlaySlowSound()
        {
            SwitchSoundButton(true);
            _onClickPlaySlowSound?.Invoke();
        }

        // the playing sound is stopped by the next one, reset the button of the stopped sound before switching
        private void SwitchSoundButton(bool isSlowSound)
        {
            if (_isSlowSound != isSlowSound)
            {
                OnSoundPlay(false);
                _isSlowSound = isSlowSound;
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
        }
    }
}