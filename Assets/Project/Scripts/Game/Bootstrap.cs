using System;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using Popup;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Chang
{
    public class Bootstrap : IInitializable, IDisposable
    {
        private readonly AddressablesDownloader _assetDownloader;
        private readonly AuthorizationService _authorizationService;
        private readonly PopupManager _popupManager;

        private LoadingUiController _loadingUiController;
        private CancellationTokenSource _cts;

        [Inject]
        public Bootstrap(AddressablesDownloader addresablesDownloader,
            AuthorizationService authorizationService,
            PopupManager popupManager)
        {
            _assetDownloader = addresablesDownloader;
            _authorizationService = authorizationService;
            _popupManager = popupManager;

            _authorizationService.OnPlayerLoggedOut += OnLoggedOut;
        }

        public void Initialize()
        {
            DMZLogger.Log($"{nameof(Initialize)}");
            LoadingSequenceAsync().Forget();
        }

        private async UniTask LoadingSequenceAsync()
        {
            DMZLogger.Log($"{nameof(LoadingSequenceAsync)}: Start");
            RunRestartTriggerAsync();

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();

                _cts = new CancellationTokenSource();

                // todo chang on every step need to emulate error with disposing everything that supposed to
                DMZLogger.Log($"Initialize start");

                _loadingUiController = _popupManager.ShowLoadingUi(
                    new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar | LoadingElements.Percent));
                _loadingUiController.SimulateProgress(2f, from: 0, to: 0.1f, ct: _cts.Token).Forget();

                //0 *skip for now download game settings from unity cloud ? Without authorization?

                //1 download addressables Base
                await _assetDownloader.PreloadGameStartAddressables(_cts.Token);

                //2 authorization   
                await _authorizationService.AuthenticateAsync();

                DMZLogger.Log($"{nameof(LoadingSequenceAsync)}: Finish");

                // to the next scene
                SceneManager.LoadScene("Game");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        // todo chang remove
        private async void RunRestartTriggerAsync()
        {
            DMZLogger.Log("Waiting for spacebar press...");
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                await UniTask.Yield();
            }

            DMZLogger.Log("Spacebar pressed! Restarting...");
            SceneManager.LoadScene("Bootstrap");
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _authorizationService.OnPlayerLoggedOut -= OnLoggedOut;
        }

        private void OnLoggedOut()
        {
            SceneManager.LoadScene("Bootstrap");
        }
    }
}