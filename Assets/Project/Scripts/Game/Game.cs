using System;
using System.Threading;
using Chang.FSM;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class Game : IInitializable, IDisposable
    {
        private readonly AddressablesDownloader _addressablesDownloader;
        private readonly AuthorizationService _authorizationService;
        private readonly GameFSM _gameFSM;
        
        private CancellationTokenSource _cts;

        [Inject]
        public Game(AddressablesDownloader addresablesDownloader,
            AuthorizationService authorizationService,
            GameFSM gameFSM)
        {
            _addressablesDownloader = addresablesDownloader;
            _authorizationService = authorizationService;
            _gameFSM = gameFSM;

            _authorizationService.OnPlayerLoggedOut += OnLoggedOut;
        }

        public void Initialize()
        {
            Debug.Log($"{nameof(Initialize)}");
            LoadingSequenceAsync();
        }
        
        private async void LoadingSequenceAsync()
        {
            Debug.Log($"{nameof(LoadingSequenceAsync)}: Start");
            RunRestartTrigger();
            
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            
                _cts = new CancellationTokenSource();
            
                // on every step need to emulate error with disposing everything that supposed to
                Debug.Log($"Initialize start");
                //0 *skip for now download game settings from unity cloud ? Without authorization?

                //1 download addressables Base
                await _addressablesDownloader.PreloadGameStartAddressables(_cts.Token);
                
                //2 authorization   
                await _authorizationService.AuthenticateAsync();
                
                Debug.Log($"{nameof(LoadingSequenceAsync)}: Finish");
                _gameFSM.Initialize();
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
            }
        }
        
        private async void RunRestartTrigger()
        {
            Debug.Log("Waiting for spacebar press...");
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                await UniTask.Yield();
            }
            Debug.Log("Spacebar pressed! Restarting...");
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