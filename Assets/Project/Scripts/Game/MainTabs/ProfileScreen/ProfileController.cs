using System;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Popup;
using Zenject;

namespace Chang
{
    public class ProfileController : IViewController
    {
        private readonly MainScreenBus _mainScreenBus;
        private readonly ProfileView _view;
        private readonly ProfileService _profileService;
        private readonly PopupManager _popupManager;

        private PopupController<ChangeNamePopupModel> _changeNameController;
        private LoadingUiController _loadingUiController;

        [Inject]
        public ProfileController(
            MainScreenBus mainScreenBus,
            ProfileView view,
            ProfileService profileService,
            PopupManager popupManager)
        {
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _popupManager = popupManager;
        }

        public void Dispose()
        {
            if (_loadingUiController != null)
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }

            if (_changeNameController != null)
            {
                _popupManager.DisposePopup(_changeNameController);
                _changeNameController = null;
            }
        }

        public void Init()
        {
            _view.Init(_mainScreenBus.OnLogOutClicked, OnChangeNameClicked);
        }

        private void OnChangeNameClicked()
        {
            ChangeNamePopupModel model = new();
            model.NameInput.Value = _profileService.ProfileData.Name;
            model.OnChangeNameCancel += OnChangeNameCancel;
            model.OnChangeNameSubmit += OnChangeNameSubmit;

            _changeNameController = _popupManager.ShowChangeNamePopup(model);
        }

        private void OnChangeNameCancel()
        {
            _popupManager.DisposePopup(_changeNameController);
            _changeNameController = null;
        }

        private void OnChangeNameSubmit()
        {
            OnChangeNameSubmitAsync().Forget();
        }

        private async UniTaskVoid OnChangeNameSubmitAsync()
        {
            _profileService.ProfileData.Name = _changeNameController.Model.NameInput.Value;

            try
            {
                _loadingUiController = _popupManager.ShowLoadingUi(new LoadingUiModel(LoadingElements.Animation));
                await _profileService.SaveProfileDataAsync();

                if (_changeNameController != null)
                {
                    _popupManager.DisposePopup(_changeNameController);
                    _changeNameController = null;
                }

                UpdateScreen();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);

            if (!active)
            {
                return;
            }

            UpdateScreen();
        }

        private void UpdateScreen()
        {
            _view.SetUserId(_profileService.PlayerId);
            _view.SetUserName(_profileService.ProfileData.Name);
        }

        public void Set()
        {
        }
    }
}