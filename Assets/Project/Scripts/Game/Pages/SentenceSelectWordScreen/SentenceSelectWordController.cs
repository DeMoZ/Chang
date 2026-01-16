using System;
using System.Collections.Generic;
using Chang.UI;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class SentenceSelectWordController : IViewController
    {
        private readonly SentenceSelectWordView _view;

        [Inject]
        public SentenceSelectWordController(SentenceSelectWordView view, PagesSoundController pagesSoundController)
        {
            _view = view;
            _view.SetPagesSoundController(pagesSoundController);
        }

        public void Dispose()
        {
        }

        public void Init(bool isQuestInTranslation,
            List<SequencePhraseData> displaySequence,
            List<SequencePhraseData> mixWords,
            string translation,
            Sprite sprite,
            Action<int, int> onToggleValueChanged,
            Action onClickPlaySound)
        {
            _view.Init(isQuestInTranslation, displaySequence, mixWords, translation, sprite, onToggleValueChanged, onClickPlaySound);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void ShowHint()
        {
            _view.ShowHint();
        }

        public void Clear()
        {
        }

        public void UpdateDisplaySequence(List<SequencePhraseData> sequence)
        {
            _view.UpdateDisplaySequence(sequence);
        }

        public void UpdateMixSequence(List<SequencePhraseData> sequence)
        {
            _view.UpdateMixSequence(sequence);
        }
    }
}