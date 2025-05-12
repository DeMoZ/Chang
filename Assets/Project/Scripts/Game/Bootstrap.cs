using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.DebugSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class Bootstrap : IDisposable
    {
        private CancellationTokenSource _cts;
        private bool _restarterIsRunning;

        [Inject]
        public Bootstrap()
        {
#if UNITY_EDITOR
            _cts = new CancellationTokenSource();
            RunRestartTriggerAsync().Forget();
#endif
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
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
            Debug.Log("LoadRebootScene");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
        }
    }
}