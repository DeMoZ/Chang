using Chang.Services;
using Zenject;

namespace Chang
{
    public class ProfileController : IViewController
    {
        private readonly GameBus _gameBus;

        // private readonly MainScreenBus _mainScreenBus;
        private readonly ProfileView _view;
        private readonly AuthorizationService _authorizationService;

        [Inject]
        public ProfileController(
            GameBus gameBus,
            // MainScreenBus mainScreenBus,
            ProfileView view,
            AuthorizationService authorizationService)
        {
            _gameBus = gameBus;
            //_mainScreenBus = mainScreenBus;
            _view = view;
            _authorizationService = authorizationService;
        }

        public void Init()
        {
            // _view.Init(_mainScreenBus.OnLogOutClicked);
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }

        public void Set()
        {
        }
    }
}