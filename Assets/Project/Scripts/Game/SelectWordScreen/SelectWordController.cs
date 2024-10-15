using System;
using System.Collections.Generic;
using Zenject;
using Chang.UI;

namespace Chang
{
    public class SelectWordController : IViewController
    {
        private SelectWordView _view;

        [Inject]
        public SelectWordController(SelectWordView view)
        {
            _view = view;
        }

        // todo roman here should be a classes, not a configx
        // todo roman refactoring is required
        public void Init(bool questInStudiedLanguage, PhraseConfig correctWord, List<PhraseConfig> mixWords,
            Action<QuestionTypeStateResult> onContinue)
        {
            _view.Init(questInStudiedLanguage, correctWord, mixWords);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}