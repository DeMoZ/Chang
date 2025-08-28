using System;
using Zenject;
using Chang.UI;
using UnityEngine;

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
            Sprite sprite,
            Action<bool> onToggleValueChanged,
            Action onClickPlaySound)
        {
            _view.Init(correctWord, sprite, onToggleValueChanged, onClickPlaySound);
        }
    }
}