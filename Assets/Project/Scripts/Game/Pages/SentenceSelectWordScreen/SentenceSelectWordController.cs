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
            List<PhraseData> compareSequence,
            List<PhraseData> displaySiquence,
            List<PhraseData> mixWords,
            Sprite sprite,
            Action<int, bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            _view.Init(isQuestInTranslation, displaySiquence, mixWords, sprite, onToggleValueChanged, onClickPlaySound);
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
    }
}