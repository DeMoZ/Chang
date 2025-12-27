using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Chang.UI
{
    public class SentenceSelectWordView : CScreen
    {
        [SerializeField] private Image _questionImage;
        [SerializeField] private Transform _displaySequenceContent;
        [SerializeField] private Transform _mixSequenceContent;
        [SerializeField] private CToggle _displayWordPrefab;
        [SerializeField] private CToggle _mixWordPrefab;
        
        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.SentenceSelectWords;

        public void Init(bool isQuestInTranslation,
            List<PhraseData> displaySequence,
            List<PhraseData> mixWords,
            Sprite onToggleValueChanged,
            Action<int, bool> onClickPlaySound,
            Action action)
        {
            Clear();
            
            _questionImage.sprite = onToggleValueChanged;
            
            for (var i = 0; i < displaySequence.Count; i++)
            {
                var displayWord = Instantiate(_displayWordPrefab, _displaySequenceContent);
                var index = i;

                var word = !isQuestInTranslation ? displaySequence[i].Word.Translation : displaySequence[i].Word.LearnWord;
                displayWord.Set(word, displaySequence[i].Word.Phonetic, null,
                    isOn => onClickPlaySound(index, isOn));
                displayWord.SetActive(!displaySequence[i].IsPlaceHolder);
            }
            
            for (var i = 0; i < mixWords.Count; i++)
            {
                var mixWord = Instantiate(_mixWordPrefab, _mixSequenceContent);
                var index = i;

                var word = !isQuestInTranslation ? mixWords[i].Word.Translation : mixWords[i].Word.LearnWord;
                mixWord.Set(word, mixWords[i].Word.Phonetic, null,
                    isOn => onClickPlaySound(index, isOn));
            }
        }

        private void Clear()
        {
            foreach (Transform child in _displaySequenceContent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in _mixSequenceContent)
            {
                Destroy(child.gameObject);
            }
        }

        public void ShowHint()
        {
            throw new NotImplementedException();
        }
    }
}