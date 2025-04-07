using System;
using System.Threading;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Chang
{
    public class Bootstrap : IInitializable, IDisposable
    {
        private readonly AddressablesDownloader _assetDownloader;
        private readonly AuthorizationService _authorizationService;
        private readonly DownloadModel _downloadModel;

        private CancellationTokenSource _cts;

        [Inject]
        public Bootstrap(AddressablesDownloader addresablesDownloader,
            AuthorizationService authorizationService,
            DownloadModel downloadModel)
        {
            _assetDownloader = addresablesDownloader;
            _authorizationService = authorizationService;
            _downloadModel = downloadModel;

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
            RunRestartTrigger();

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();

                _cts = new CancellationTokenSource();

                // on every step need to emulate error with disposing everything that supposed to
                DMZLogger.Log($"Initialize start");

                _downloadModel.SimulateProgress(2f, from: 0, to: 0.1f, ct: _cts.Token).Forget();
                _downloadModel.ShowUi.Value = true;

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

        private async void RunRestartTrigger()
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