using System;
using Zenject;
using Chang.UI;

namespace Chang
{
    public class DemonstrationWordController : IViewController
    {
        private DemonstrationWordView _view;

        [Inject]
        public DemonstrationWordController(DemonstrationWordView view, PagesSoundController pagesSoundController)
        {
            _view = view;
            _view.SetPagesSoundController(pagesSoundController);
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Init(PhraseData correctWord,
            Action<bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            _view.Init(correctWord, onToggleValueChanged, onClickPlaySound);
        }
    }
}