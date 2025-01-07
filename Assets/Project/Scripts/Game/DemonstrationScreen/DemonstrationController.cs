using System;
using Zenject;
using Chang.UI;

namespace Chang
{
    public class DemonstrationWordController : IViewController
    {
        private DemonstrationWordView _view;

        [Inject]
        public DemonstrationWordController(DemonstrationWordView view)
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
            _view.Init(questInStudiedLanguage, correctWord, onToggleValueChanged);
        }
    }
}