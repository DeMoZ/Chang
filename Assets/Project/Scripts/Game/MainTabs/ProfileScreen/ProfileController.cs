using Chang.Services;
using Zenject;

namespace Chang
{
    public class ProfileController : IViewController
    {
        private readonly MainScreenBus _mainScreenBus;
        private readonly ProfileView _view;
        private readonly AuthorizationService _authorizationService;
        private readonly ProfileService _profileService;

        [Inject]
        public ProfileController(
            MainScreenBus mainScreenBus,
            ProfileView view,
            AuthorizationService authorizationService,
            ProfileService profileService)
        {
            _mainScreenBus = mainScreenBus;
            _view = view;
            _authorizationService = authorizationService;
            _profileService = profileService;
        }

        public void Init()
        {
            _view.Init(_mainScreenBus.OnLogOutClicked);
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);

            if (!active)
            {
                return;
            }
            
            _view.SetUserId(_profileService.PlayerId);
            _view.SetUserName("temp_name"); // todo chang implement user name
        }

        public void Set()
        {
        }
    }
}