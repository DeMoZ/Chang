using System;
using System.Collections.Generic;
using Zenject;
using Chang.UI;

namespace Chang
{
    public class SelectWordController : IViewController
    {
        private readonly SelectWordView _view;

        [Inject]
        public SelectWordController(SelectWordView view, PagesSoundController pagesSoundController)
        {
            _view = view;
            _view.SetPagesSoundController(pagesSoundController);
        }

        public void Dispose()
        {
        }

        public void Init(bool isQuestInTranslation,
            PhraseData correctWord,
            List<PhraseData> mixWords,
            Action<int, bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            _view.Init(isQuestInTranslation, correctWord, mixWords, onToggleValueChanged, onClickPlaySound);
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