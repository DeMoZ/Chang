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
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class Bootstrap : IInitializable, IDisposable
    {
        private readonly AddressablesDownloader _assetDownloader;
        private readonly AuthorizationService _authorizationService;
        private readonly PopupManager _popupManager;

        private LoadingUiController _loadingUiController;
        private CancellationTokenSource _cts;
        private bool _restarterIsRunning;

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
#if UNITY_EDITOR
            RunRestartTriggerAsync().Forget();
#endif
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _authorizationService.OnPlayerLoggedOut -= OnLoggedOut;
        }

        private void OnLoggedOut()
        {
            Debug.Log("OnLoggedOut");
            LoadRebootScene();
        }

        public void LoadRebootScene()
        {
            DMZLogger.Log($"Load Reboot Scene");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
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
                _loadingUiController = _popupManager.ShowLoadingUi(
                    new LoadingUiModel(LoadingElements.Background | LoadingElements.Bar | LoadingElements.Percent));
                _loadingUiController.SimulateProgress(2f, from: 0, to: 0.1f).Forget();

                //0 *skip for now download game settings from unity cloud ? Without authorization?

                //1 download addressables Base
                await _assetDownloader.PreloadGameStartAddressables(_cts.Token);

                //2 authorization   
                await _authorizationService.AuthenticateAsync();

                DMZLogger.Log($"{nameof(LoadingSequenceAsync)}: Finish");

                SceneManager.LoadScene(ProjectConstants.GAME_SCENE);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async UniTaskVoid RunRestartTriggerAsync()
        {
            if (_restarterIsRunning)
            {
                return;
            }
            
            DMZLogger.Log("Waiting for spacebar press...");
            _restarterIsRunning = true;
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                await UniTask.Yield();
            }

            OnLoggedOut();
        }
    }
}