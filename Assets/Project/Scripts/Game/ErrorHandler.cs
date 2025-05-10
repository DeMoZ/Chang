using System;
using Popup;

namespace Chang
{
    // todo chang implement
    public class ErrorHandler
    {
        private readonly PopupManager _popupManager;
        private PopupController<ErrorPopupModel> _errorController;

        // private readonly SceneManagerService _sceneManager;
        // private readonly ExtContainer _container;
        //
        // public ErrorHandler(SceneManagerService sceneManager, ExtContainer container)
        // {
        //     _sceneManager = sceneManager;
        //     _container = container;
        // }
        //
        // public void RestartApplication()
        // {
        //     _container.Dispose();
        //     _sceneManager.LoadScene("Bootstrap", true);
        // }

        public ErrorHandler(PopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        public void HandleError(Exception exception, string customDescription)
        {
            var model = new ErrorPopupModel();
            model.LabelText.Value = customDescription + "\n + " + exception.Message;
            model.OnOkClicked += OnErrorPopupOkClicked;
            _errorController = _popupManager.ShowErrorPopup(model);
        }

        private void OnErrorPopupOkClicked()
        {
            _errorController.Dispose();
            // todo chang may be restart application
        }
    }
}