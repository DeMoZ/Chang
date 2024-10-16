using System.Collections;
using System.Collections.Generic;
using Chang.UI;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class GameOverlayController : IViewController
    {
        private GameOverlayView _view;

        [Inject]
        public GameOverlayController(GameOverlayView view)
        {
            _view = view;
        }

        public void EnableCheckButton(bool isOn)
        {
            _view.EnableCheckButton(isOn);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}