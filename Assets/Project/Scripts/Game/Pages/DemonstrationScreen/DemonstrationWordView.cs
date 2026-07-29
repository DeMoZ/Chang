using System;
using Chang.Core;
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
        [SerializeField] private PlayStopButton _playStopBtn;

        [ShowInInspector, ReadOnly] public override ChangTypes ScreenType { get; } = ChangTypes.DemonstrationWord;
        
        private Action _onClickPlaySound;
        
        public void Init(Word correctWord,
            Action<bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            Debug.Log("Init SelectWordView");
            
            _onClickPlaySound = onClickPlaySound;

            foreach (Transform child in _mixWordContent)
            {
                Destroy(child.gameObject);
            }

            // init learning language word
            var quesWord = correctWord.LearnWord;
            _questionWord.Set(quesWord, correctWord.Phonetics);
            _questionWord.EnablePhonetic(true);

            // init translation words
            var mix = Instantiate(_mixWordPrefab, _mixWordContent);
            var word = correctWord.GetTranslation();
            mix.Set(word, correctWord.Phonetics, _toggleGroup, onToggleValueChanged);
            mix.EnablePhonetics(false);
            PagesSoundController.RegisterListener(correctWord.Key, OnSoundPlay);
            _playStopBtn.OnClick += OnClickPlaySound;
            
            _questionImage.sprite = correctWord.Sprite;
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
        }
    }
}