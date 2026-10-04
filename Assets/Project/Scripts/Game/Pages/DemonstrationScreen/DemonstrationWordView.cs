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

        [Tooltip("A toggle placed in the screen (the design's reveal chip). When set, it is used instead of instantiating Mix Word Prefab")]
        [SerializeField] private CToggle _translationToggle;

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.DemonstrationWord;

        private Action _onClickPlaySound;

        public void Init(Word correctWord,
            Action<bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            Debug.Log("Init SelectWordView");

            _onClickPlaySound = onClickPlaySound;

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

            _questionImage.sprite = correctWord.Sprite;
        }

        private void OnSoundPlay(bool play)
        {
            if (_playStopBtn != null)
            {
                _playStopBtn.SetPlay(!play);
            }
        }

        private void OnClickPlaySound()
        {
            _onClickPlaySound?.Invoke();
        }

        private void OnDisable()
        {
            if (_playStopBtn != null)
            {
                _playStopBtn.OnClick -= OnClickPlaySound;
            }
        }
    }
}