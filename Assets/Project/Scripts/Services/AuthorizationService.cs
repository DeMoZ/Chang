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
        private readonly MainScreenBus _mainScreenBus;
        private readonly LogInController _logInController;
        private readonly CancellationTokenSource _cts = new();

        [Inject]
        public AuthorizationService(MainScreenBus mainScreenBus, LogInController logInController)
        {
            _mainScreenBus = mainScreenBus;
            _logInController = logInController;
            
            _mainScreenBus.OnLogOutClicked += OnLogOutClicked;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            _mainScreenBus.OnLogOutClicked -= OnLogOutClicked;
        }
        
        public async UniTask AuthenticateAsync()
        {
            _logInController.SetViewActive(true);
            await _logInController.LoginAsync();
            _logInController.SetViewActive(false);
        }
        
        private void OnLogOutClicked()
        {
            _logInController.LogOutAsync();
        }
    }
}