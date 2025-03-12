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
        [SerializeField] private PlayStopButton _playStopBtn;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.DemonstrationWord;
        
        private Action _onClickPlaySound;
        
        public void Init(PhraseData correctWord,
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
            var quesWord = correctWord.Word.LearnWord;
            _questionWord.Set(quesWord, correctWord.Word.Phonetic);
            _questionWord.EnablePhonetic(true);

            // init translation words
            var mix = Instantiate(_mixWordPrefab, _mixWordContent);
            var word = correctWord.Word.GetTranslation();
            mix.Set(word, correctWord.Word.Phonetic, _toggleGroup, onToggleValueChanged);
            mix.EnablePhonetics(false);
            PagesSoundController.RegisterListener(correctWord.AudioClip.name, OnSoundPlay);
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
        }
    }
}