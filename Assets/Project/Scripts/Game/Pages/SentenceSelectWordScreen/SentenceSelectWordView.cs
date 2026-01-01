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
        [SerializeField] private ToggleGroup _displayTogglesGroup;
        [SerializeField] private ToggleGroup _mixTogglesGroup;

        [ShowInInspector, ReadOnly] public override QuestionType ScreenType { get; } = QuestionType.SentenceSelectWords;

        private Action<int, int> OnToggleValueChanged;

        public void Init(bool isQuestInTranslation,
            List<SequencePhraseData> displaySequence,
            List<SequencePhraseData> mixWords,
            Sprite sprite,
            Action<int, int> onToggleValueChanged,
            Action onClickPlaySound)
        {
            Clear(_displaySequenceContent);
            Clear(_mixSequenceContent);
            _questionImage.sprite = sprite;
            OnToggleValueChanged = onToggleValueChanged;
            UpdateDisplaySequence(displaySequence);
            UpdateMixSequence(mixWords);
        }

        private void Clear(Transform parent)
        {
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        }

        public void ShowHint()
        {
            throw new NotImplementedException();
        }

        public void UpdateDisplaySequence(List<SequencePhraseData> sequence)
        {
            Clear(_displaySequenceContent);

            for (var i = 0; i < sequence.Count; i++)
            {
                CToggle displayWord = Instantiate(_displayWordPrefab, _displaySequenceContent);

                string word = sequence[i].Word.LearnWord;
                int index = i;
                displayWord.Set(word, sequence[i].Word.Phonetic, null, isOn => OnToggleValueChanged(index, -1));
                displayWord.SetGroup(_displayTogglesGroup);
                displayWord.SetInteractable(sequence[i].IsInteractable);
                displayWord.SetActive(sequence[i].IsInteractable);
            }
        }

        public void UpdateMixSequence(List<SequencePhraseData> sequence)
        {
            Clear(_mixSequenceContent);

            for (var i = 0; i < sequence.Count; i++)
            {
                CToggle mixWord = Instantiate(_mixWordPrefab, _mixSequenceContent);
                string word = sequence[i].Word.LearnWord;
                int index = i;
                mixWord.Set(word, sequence[i].Word.Phonetic, null, isOn => OnToggleValueChanged(-1, index));
                mixWord.SetGroup(_mixTogglesGroup);
                mixWord.SetInteractable(sequence[i].IsInteractable);
                mixWord.SetActive(sequence[i].IsInteractable);
            }
        }
    }
}