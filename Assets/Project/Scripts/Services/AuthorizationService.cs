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
        public Action OnPlayerLoggedOut;

        private readonly MainScreenBus _mainScreenBus;
        private readonly LogInController _logInController;
        private readonly CancellationTokenSource _cts = new();

        [Inject]
        public AuthorizationService(LogInController logInController, MainScreenBus mainScreenBus)
        {
            _mainScreenBus = mainScreenBus;
            _logInController = logInController;

            _mainScreenBus.OnLogOutClicked += OnLogOutClicked;
            _logInController.OnLoggedOut += OnLoggedOut;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _mainScreenBus.OnLogOutClicked -= OnLogOutClicked;
            _logInController.OnLoggedOut -= OnLoggedOut;
        }

        public async UniTask AuthenticateAsync()
        {
            Debug.Log("AuthorizeAsync start");
            _logInController.SetViewActive(true);
            await _logInController.LoginAsync();
            _logInController.SetViewActive(false);
            Debug.Log("AuthorizeAsync end");
        }

        private void OnLogOutClicked()
        {
            _logInController.LogOutAsync();
        }

        private void OnLoggedOut()
        {
            OnPlayerLoggedOut?.Invoke();
        }
    }
}