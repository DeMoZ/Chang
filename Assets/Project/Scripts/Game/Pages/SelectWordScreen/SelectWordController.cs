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

        public void Dispose()
        {
        }

        public void Init(bool questInStudiedLanguage, PhraseData correctWord, List<PhraseData> mixWords, Action<int, bool> onToggleValueChanged)
        {
            _view.Init(questInStudiedLanguage, correctWord, mixWords, onToggleValueChanged);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}