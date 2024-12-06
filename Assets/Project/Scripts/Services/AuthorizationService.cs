using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.Legacy.LoginScreen;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services
{
    public class AuthorizationService : IDisposable
    {
        private readonly LogInController _logInController;
        private readonly CancellationTokenSource _cts = new();

        [Inject]
        public AuthorizationService(LogInController logInController)
        {
            _logInController = logInController;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTask AuthenticateAsync()
        {
            _logInController.SetViewActive(true);
            var loggedInData = await _logInController.Login(_cts.Token);
            
            Debug.Log($"loggedInData: {loggedInData}");
            _logInController.SetViewActive(false);
        }
    }
}