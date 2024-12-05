using System;
using Chang.FSM;
using Chang.UI;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    // todo roman need to be FSM for overlay elements
    public class GameOverlayController : IViewController
    {
        private GameOverlayView _view;

        public Action OnCheck;
        public Action OnContinue;
        public Action OnReturn;
        public Action<bool> OnPhonetic;

        [Inject]
        public GameOverlayController(GameOverlayView view)
        {
            _view = view;
            _view.Init(OnCheckBtn, OnContinueBtn, OnReturnBtn, OnPhoneticTgl);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void EnableCheckButton(bool enable)
        {
            _view.EnableCheckButton(enable);
        }
        
        public void SetContinueButtonInfo(ContinueButtonInfo info)
        {
            _view.SetContinueButtonInfo(info);
        }
        
        public void EnableContinueButton(bool enable)
        {
            _view.EnableBlocker(enable);
            _view.EnableContinueButton(enable);
        }

        private void OnCheckBtn()
        {
            EnableCheckButton(false);
            OnCheck?.Invoke();
        }

        private void OnContinueBtn()
        {
            EnableContinueButton(false);
            OnContinue?.Invoke();
        }

        private void OnReturnBtn()
        {
            throw new NotImplementedException();
        }

        private void OnPhoneticTgl(bool obj)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}