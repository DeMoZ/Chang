using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DMZ.Legacy.LoginScreen;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang.Services
{
    public class AuthorizationService : IDisposable
    {
        public Action OnPlayerLoggedOut;
        public Action OnAuthenticationRequired;
        /// <summary>a guest signed in to an existing account from the account view, another player is signed in now</summary>
        public Action OnAccountSwitched;

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
            _logInController.OnAccountSwitched += OnLogInAccountSwitched;
        }

        /// <param name="beforeAccountSwitch">awaited while the guest is still signed in, before it switches to an existing account; an exception cancels the switch</param>
        /// <param name="accountSwitchFailed">the sign-in to the account failed, the guest is signed in again</param>
        public void SetGuestUpgradeHandlers(Func<Task> beforeAccountSwitch, Action accountSwitchFailed)
        {
            _logInController.BeforeAccountSwitch = beforeAccountSwitch;
            _logInController.AccountSwitchFailed = accountSwitchFailed;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _mainScreenBus.OnLogOutClicked -= OnLogOutClicked;
            _logInController.OnLoggedOut -= OnLoggedOut;
            _logInController.OnAccountSwitched -= OnLogInAccountSwitched;
            _logInController.BeforeAccountSwitch = null;
            _logInController.AccountSwitchFailed = null;
        }

        public async UniTask AuthenticateAsync()
        {
            Debug.Log("AuthorizeAsync start");
            _logInController.SetViewActive(true);
            await _logInController.LoginAsync();
            _logInController.SetViewActive(false);
            Debug.Log("AuthorizeAsync end");
        }

        public void RequestAuthentication()
        {
            Debug.Log("RequestAuthentication");
            OnAuthenticationRequired?.Invoke();
        }

        private void OnLogOutClicked()
        {
            Debug.Log("OnLogOutClicked: the account view is shown");
            _logInController.LogOutAsync().AsUniTask().Forget();
        }

        private void OnLogInAccountSwitched()
        {
            Debug.Log("OnAccountSwitched");
            OnAccountSwitched?.Invoke();
        }

        private void OnLoggedOut()
        {
            Debug.Log("OnLoggedOut");
            OnPlayerLoggedOut?.Invoke();
        }
    }
}