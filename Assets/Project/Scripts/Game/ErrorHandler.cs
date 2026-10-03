using System;
using System.Collections.Generic;
using DMZ.DebugSystem;
using Popup;

namespace Chang
{
    /// <summary>
    /// Logs errors and shows them to the user one popup at a time.
    /// Errors raised while a popup is shown are queued, duplicates are skipped:
    /// several requests usually fail at once for the same reason (e.g. no internet).
    /// </summary>
    public class ErrorHandler : IDisposable
    {
        private readonly PopupManager _popupManager;
        private readonly Queue<string> _pendingMessages = new();

        private PopupController<ErrorPopupModel> _errorController;
        private string _shownMessage;

        public ErrorHandler(PopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        public void Dispose()
        {
            _pendingMessages.Clear();
            ClosePopup();
        }

        /// <param name="userMessage">human readable description shown to the user</param>
        public void HandleError(Exception exception, string userMessage)
        {
            DMZLogger.LogError(exception, userMessage);

            var message = BuildMessage(exception, userMessage);
            if (message == _shownMessage || _pendingMessages.Contains(message))
            {
                return;
            }

            if (_errorController != null)
            {
                _pendingMessages.Enqueue(message);
                return;
            }

            ShowPopup(message);
        }

        private static string BuildMessage(Exception exception, string userMessage)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // technical details help while testing, but must not be shown to players
            return $"{userMessage}\n{exception.Message}";
#else
            return userMessage;
#endif
        }

        private void ShowPopup(string message)
        {
            var model = new ErrorPopupModel();
            model.LabelText.Value = message;
            model.OnOkClicked += OnErrorPopupOkClicked;

            _shownMessage = message;
            _errorController = _popupManager.ShowErrorPopup(model);
        }

        private void OnErrorPopupOkClicked()
        {
            ClosePopup();

            if (_pendingMessages.Count > 0)
            {
                ShowPopup(_pendingMessages.Dequeue());
            }
        }

        private void ClosePopup()
        {
            if (_errorController == null)
            {
                return;
            }

            // PopupManager may be destroyed before this service on application quit, then it already disposed the popup
            if (_popupManager != null)
            {
                _popupManager.DisposePopup(_errorController);
            }

            _errorController = null;
            _shownMessage = null;
        }
    }
}
