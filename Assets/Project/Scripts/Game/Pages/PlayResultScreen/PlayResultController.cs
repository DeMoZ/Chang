using System;
using System.Collections.Generic;
using Chang.UI;
using Zenject;

namespace Chang
{
    public class PlayResultController : IViewController
    {
        private PlayResultView _view;

        [Inject]
        public PlayResultController(PlayResultView view)
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

        public void Init(List<Chang.FSM.ResultItem> lessonLog, Action onContinueClick)
        {
            _view.Init(onContinueClick);
            
            foreach (var item in lessonLog)
            {
                _view.AddItem(item.Presentation, isUp: item.IsCorrect);
            }
        }
    }
}