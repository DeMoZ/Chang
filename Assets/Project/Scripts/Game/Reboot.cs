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
    public class Reboot : IInitializable, IDisposable
    {
        private readonly AddressablesDownloader _assetDownloader;
        private readonly AuthorizationService _authorizationService;
        private readonly PopupManager _popupManager;

        private CancellationTokenSource _cts;

        [Inject]
        public Reboot(AddressablesDownloader addressablesDownloader, AuthorizationService authorizationService, PopupManager popupManager)
        {
            _assetDownloader = addressablesDownloader;
            _authorizationService = authorizationService;
            _popupManager = popupManager;
        }

        public void Initialize()
        {
            LoadingSequenceAsync().Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTaskVoid LoadingSequenceAsync()
        {
            DMZLogger.Log($"{nameof(LoadingSequenceAsync)}: Start");

            // todo chang on every step need to emulate error with disposing everything that supposed to
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();

                _cts = new CancellationTokenSource();
                var loadingModel = new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar | LoadingElements.Percent);
                var loadingUiController = _popupManager.ShowLoadingUi(loadingModel);
                loadingUiController.SimulateProgress(2f, from: 0, to: 0.1f).Forget();

                //0 *skip for now download game settings from unity cloud ? Without authorization?

                //1 download addressables Base

                await _assetDownloader.PreloadAtGameStartAsync(percent => { loadingUiController.SetProgress(percent); }, _cts.Token);
                loadingUiController.SetProgress(1);

                //2 authorization   
                await _authorizationService.AuthenticateAsync();

                DMZLogger.Log($"{nameof(LoadingSequenceAsync)}: Finish");
                SceneManager.LoadScene(ProjectConstants.GAME_SCENE);
                _popupManager.DisposePopup(loadingUiController);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}