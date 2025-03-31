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
        private readonly GameFSM _gameFSM;
        
        private CancellationTokenSource _cts;

        [Inject]
        public Game(GameFSM gameFSM)
        {
            _gameFSM = gameFSM;
        }

        public void Initialize()
        {
            Debug.Log($"{nameof(Initialize)}");
            LoadingSequenceAsync();
        }
        
        private async void LoadingSequenceAsync()
        {
            Debug.Log($"{nameof(LoadingSequenceAsync)}: Start");
            
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            
                _cts = new CancellationTokenSource();
                
                Debug.Log($"{nameof(LoadingSequenceAsync)}: Finish");
                _gameFSM.Initialize();
            }
            catch (Exception e)
            {
                throw; // TODO handle exception
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}