using System;
using Cysharp.Threading.Tasks;
using DMZ.Legacy.LoginScreen;
using Zenject;

namespace Chang.Services
{
    public class AuthorizationService : IDisposable
    {
        private readonly LogInController _logInController;

        [Inject]
        public AuthorizationService(LogInController logInController)
        {
            _logInController = logInController;
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public async UniTask AuthenticateAsync()
        {
            _logInController.SetViewActive(true);
            await _logInController.TryRestoreCurrentSessionAsync();
        }
    }
    
    public class ExternalAuthorizationService : IDisposable
    {
        private readonly LogInController _logInController;

        [Inject]
        public ExternalAuthorizationService(LogInController logInController)
        {
            _logInController = logInController;
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public async UniTask AuthenticateAsync()
        {
            // await _logInController.TryRestoreCurrentSessionAsync();
            // if (!_logInController.IsSignedIn)
            // {
            //     await _logInController.SignInAsync();
            // }
        }
    }
}