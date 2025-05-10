using System;
using Chang.UI;
using Popup;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    // todo chang need to be FSM for overlay elements
    public class GameOverlayController : IViewController
    {
        private readonly GameOverlayView _view;
        private readonly PopupManager _popupManager;

        private PopupController<YesNoPopupModel> _exitPopupController;

        public Action OnCheck;
        public Action OnContinue;
        public Action OnReturnFromGame;
        public Action OnHint;

        [Inject]
        public GameOverlayController(GameOverlayView view, GameBus gameBus, PopupManager popupManager)
        {
            _view = view;
            _popupManager = popupManager;

            _view.Init(OnCheckBtn, OnContinueBtn, OnReturnBtn, OnHintBtn);
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void OnExitToLobby()
        {
            EnableReturnButton(false);
            EnableCheckButton(false);
            EnableContinueButton(false);
            EnableHintButton(false);
        }

        public void EnableReturnButton(bool enable)
        {
            _view.EnableReturnButton(enable);
        }

        public void EnableCheckButton(bool enable)
        {
            _view.EnableCheckButton(enable);
        }

        public void EnableHintButton(bool enable)
        {
            _view.EnableHintButton(enable);
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
            var yesModel = new YesNoPopupModel();
            yesModel.HeaderText.Value = string.Empty;
            yesModel.LabelText.Value = "Are you sure want to exit the lesson";
            yesModel.OnOkClicked += () => OnConfirmReturn(true);
            yesModel.OnCancelClicked += () => OnConfirmReturn(false);
            _exitPopupController = _popupManager.ShowYesNoPopup(yesModel);
        }

        private void OnConfirmReturn(bool confirm)
        {
            Debug.Log($"OnConfirmReturn {confirm}");
            _popupManager.DisposePopup(_exitPopupController);
            _exitPopupController = null;

            if (confirm)
            {
                OnReturnFromGame?.Invoke();
            }
        }

        private void OnHintBtn()
        {
            Debug.Log($"OnHintButton");
            OnHint?.Invoke();
        }

        public void Dispose()
        {
            _view.Clean();
        }
    }
}