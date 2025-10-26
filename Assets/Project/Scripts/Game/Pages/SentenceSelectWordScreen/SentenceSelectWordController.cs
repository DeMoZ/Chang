using System;
using System.Collections.Generic;
using Chang.UI;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class SentenceSelectWordController: IViewController
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
            List<PhraseData> correctSequence,
            List<PhraseData> sequence,
            Sprite sprite,
            List<PhraseData> mixWords,
            Action<int, bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            _view.Init(isQuestInTranslation, sequence, sprite, mixWords, onToggleValueChanged, onClickPlaySound);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void ShowHint()
        {
            _view.ShowHint();
        }
    }
}