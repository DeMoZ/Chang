using System;
using System.Linq;
using System.Threading;
using Chang.Profile;
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
        private readonly LocalizationService _localizationService;
        private readonly LanguagesConfig _languagesConfig;
        private readonly MascotEditorController _mascotEditorController;

        private PopupController<ChangeNamePopupModel> _changeNameController;
        private PopupController<ChangeGenderPopupModel> _changeGenderController;
        private PopupController<ChangeLanguagePopupModel> _changeLanguageController;
        private LoadingUiController _loadingUiController;
        private CancellationTokenSource _cts = new();

        [Inject]
        public ProfileController(
            MainScreenBus mainScreenBus,
            ProfileView view,
            ProfileService profileService,
            PopupManager popupManager,
            LocalizationService localizationService,
            LanguagesConfig languagesConfig,
            MascotEditorController mascotEditorController)
        {
            _mainScreenBus = mainScreenBus;
            _view = view;
            _profileService = profileService;
            _popupManager = popupManager;
            _localizationService = localizationService;
            _languagesConfig = languagesConfig;
            _mascotEditorController = mascotEditorController;
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

            if (_changeGenderController != null)
            {
                _popupManager.DisposePopup(_changeGenderController);
                _changeGenderController = null;
            }

            if (_changeLanguageController != null)
            {
                _popupManager.DisposePopup(_changeLanguageController);
                _changeLanguageController = null;
            }
        }

        public void Init()
        {
            _view.Init(_mainScreenBus.OnLogOutClicked, OnChangeNameClicked, OnChangeGenderClicked, OnChangeLanguageClicked,
                OnEditMascotClicked);
        }

        private void OnEditMascotClicked()
        {
            _mascotEditorController.Open(UpdateScreen);
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
            OnChangeNameSubmitAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid OnChangeNameSubmitAsync(CancellationToken ct)
        {
            _profileService.ProfileData.Name = _changeNameController.Model.NameInput.Value;

            try
            {
                _loadingUiController = _popupManager.ShowLoadingUi(new LoadingUiModel(LoadingElements.Animation));
                await _profileService.SaveProfileDataAsync(ct);

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

        private void OnChangeGenderClicked()
        {
            ChangeGenderPopupModel model = new();
            model.Gender.Value = _profileService.ProfileData.Gender;
            model.OnChangeGenderCancel += OnChangeGenderCancel;
            model.OnChangeGenderSubmit += OnChangeGenderSubmit;

            _changeGenderController = _popupManager.ShowChangeGenderPopup(model);
        }

        private void OnChangeGenderCancel()
        {
            _popupManager.DisposePopup(_changeGenderController);
            _changeGenderController = null;
        }

        private void OnChangeGenderSubmit()
        {
            OnChangeGenderSubmitAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid OnChangeGenderSubmitAsync(CancellationToken ct)
        {
            _profileService.ProfileData.Gender = _changeGenderController.Model.Gender.Value;

            try
            {
                _loadingUiController = _popupManager.ShowLoadingUi(new LoadingUiModel(LoadingElements.Animation));
                await _profileService.SaveProfileDataAsync(ct);

                if (_changeGenderController != null)
                {
                    _popupManager.DisposePopup(_changeGenderController);
                    _changeGenderController = null;
                }

                UpdateScreen();
            }
            finally
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }
        }

        private void OnChangeLanguageClicked()
        {
            ChangeLanguagePopupModel model = new();
            model.Options = _languagesConfig.EnabledLanguages.ToArray();
            model.OptionNames = Array.ConvertAll(model.Options, _languagesConfig.GetDisplayName);
            model.Language.Value = _profileService.ProfileData.NativeLanguage;
            model.OnChangeLanguageCancel += OnChangeLanguageCancel;
            model.OnChangeLanguageSubmit += OnChangeLanguageSubmit;

            _changeLanguageController = _popupManager.ShowChangeLanguagePopup(model);
        }

        private void OnChangeLanguageCancel()
        {
            _popupManager.DisposePopup(_changeLanguageController);
            _changeLanguageController = null;
        }

        private void OnChangeLanguageSubmit()
        {
            OnChangeLanguageSubmitAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid OnChangeLanguageSubmitAsync(CancellationToken ct)
        {
            Languages language = _changeLanguageController.Model.Language.Value;

            try
            {
                _loadingUiController = _popupManager.ShowLoadingUi(new LoadingUiModel(LoadingElements.Animation));
                await _localizationService.SelectLanguageAsync(language, ct);

                if (_changeLanguageController != null)
                {
                    _popupManager.DisposePopup(_changeLanguageController);
                    _changeLanguageController = null;
                }

                UpdateScreen();
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
            _view.SetGender(_profileService.ProfileData.Gender);
            _view.SetLanguage(_languagesConfig.GetDisplayName(_profileService.ProfileData.NativeLanguage));
            _view.SetMascot(_profileService.ProfileData.Mascot ?? new MascotLook());
        }

        public async UniTask SetAsync(CancellationToken ct)
        {
            await UniTask.NextFrame(ct);
        }
    }
}