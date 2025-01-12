using System;
using Zenject;
using Chang.UI;

namespace Chang
{
    public class MatchWordsController: IViewController
    {
        private MatchWordsView _view;

        [Inject]
        public MatchWordsController(MatchWordsView view)
        {
            _view = view;
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(bool questInStudiedLanguage, PhraseData correctWord, Action<bool> onToggleValueChanged)
        {
            //_view.Init(questInStudiedLanguage, correctWord, onToggleValueChanged);
        }
    }
}