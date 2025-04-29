using System.Text.RegularExpressions;
using Chang.Services;
using Cysharp.Threading.Tasks;
using DMZ.Legacy.LoginScreen;
using Popup;
using Zenject;

namespace Chang
{
    public class ProfileController : IViewController
    {
        private const int MinName = 1;
        private const int MaxName = 20;

        private readonly Regex _nameRegex = new(@"^(?!.*[-\s]{2,})[\p{L}]+(?:[-\s][\p{L}]+)*$", RegexOptions.Compiled);
        private readonly MainScreenBus _mainScreenBus;
        private readonly ProfileView _view;
        private readonly AuthorizationService _authorizationService;
        private readonly ProfileService _profileService;
        private readonly PopupManager _popupManager;

        private PopupController<ChangeNamePopupModel> _changeNameController;

        [Inject]
        public ProfileController(
            MainScreenBus mainScreenBus,
            ProfileView view,
            AuthorizationService authorizationService,
            ProfileService profileService,
            PopupManager popupManager)
        {
            _mainScreenBus = mainScreenBus;
            _view = view;
            _authorizationService = authorizationService;
            _profileService = profileService;
            _popupManager = popupManager;
        }

        public void Init()
        {
            _view.Init(_mainScreenBus.OnLogOutClicked, OnChangeNameClicked);
        }

        public void Dispose()
        {
        }

        private void OnChangeNameClicked()
        {
            var changeNameModel = new ChangeNamePopupModel();
            changeNameModel.OnNameInput += text => OnNameInput(text, changeNameModel);
            changeNameModel.OnChangeNameCancel += OnChangeNameCancel;
            changeNameModel.OnChangeNameSubmit += OnChangeNameSubmit;

            _changeNameController = _popupManager.ShowChangeNamePopup(changeNameModel);
        }

        private void OnNameInput(string text, ChangeNamePopupModel model)
        {
            NameValidationType validation = ValidateInputName(text);
            model.OnNameValidation?.Invoke(validation);
        }

        private void OnChangeNameCancel()
        {
            _popupManager.DisposePopup(_changeNameController);
        }

        private void OnChangeNameSubmit()
        {
            // show loading (popup)
            // ok close popups and update profile screen // _popupManager.DisposePopup(_changeNameController);
            // not ok show error popup
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

        public NameValidationType ValidateInputName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return NameValidationType.MinLength;
            }

            NameValidationType validationType = 0;

            if (value.Length < MinName)
                validationType |= NameValidationType.MinLength;
            if (value.Length > MaxName)
                validationType |= NameValidationType.MaxLength;
            if (!_nameRegex.IsMatch(value))
                validationType |= NameValidationType.InvalidCharacters;

            return validationType;
        }
    }
}