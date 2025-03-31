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
        private readonly AddressablesDownloader _addressablesDownloader;
        private readonly AuthorizationService _authorizationService;
        
        private CancellationTokenSource _cts;

        [Inject]
        public Bootstrap(AddressablesDownloader addresablesDownloader, AuthorizationService authorizationService)
        {
            _addressablesDownloader = addresablesDownloader;
            _authorizationService = authorizationService;

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
                //0 *skip for now download game settings from unity cloud ? Without authorization?

                //1 download addressables Base
                await _addressablesDownloader.PreloadGameStartAddressables(_cts.Token);
                
                //2 authorization   
                await _authorizationService.AuthenticateAsync();
                
                DMZLogger.Log($"{nameof(LoadingSequenceAsync)}: Finish");
                
                // to the next scene
                SceneManager.LoadScene("Game");
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
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

public class Bootstrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}