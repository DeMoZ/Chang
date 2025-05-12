using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using Popup;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chang
{
    // todo chang implement
    public class ErrorHandler : IDisposable
    {
        private readonly PopupManager _popupManager;
        
        private PopupController<ErrorPopupModel> _errorController;
        private CancellationTokenSource _cts;
        private bool _restarterIsRunning;
     
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
            
#if UNITY_EDITOR
            _cts = new CancellationTokenSource();
            RunRestartTriggerAsync().Forget();
#endif
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            _errorController?.Dispose();
            _errorController = null;
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
            _errorController = null;
            // todo chang may be restart application
        }
        
        private async UniTaskVoid RunRestartTriggerAsync()
        {
            if (_restarterIsRunning)
            {
                return;
            }

            DMZLogger.Log("Waiting for spacebar press...");
            _restarterIsRunning = true;
            try
            {
                while (!Input.GetKeyDown(KeyCode.Space))
                {
                    await UniTask.Yield(cancellationToken: _cts.Token);
                }

                if (_cts.Token.IsCancellationRequested)
                {
                    DMZLogger.Log("Restart trigger cancelled after spacebar press, before loading reboot scene.");
                    return;
                }

                LoadRebootScene();
            }
            catch (OperationCanceledException)
            {
                DMZLogger.Log("Restart trigger operation was cancelled by token.");
            }
        }

        private void LoadRebootScene()
        {
            DMZLogger.Log("LoadRebootScene");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
        }
    }
}