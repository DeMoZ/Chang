using System;
using System.Threading;
using Chang.Profile;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Popup;
using Zenject;

namespace Chang
{
    /// <summary>
    /// Mascot editor over the profile tab. The player changes a copy of the mascot; Save stores it in the profile,
    /// Back leaves it unchanged.
    /// </summary>
    public class MascotEditorController : IDisposable
    {
        private readonly MascotEditorView _view;
        private readonly ProfileService _profileService;
        private readonly PopupManager _popupManager;
        private readonly Random _random = new();
        private readonly CancellationTokenSource _cts = new();

        private MascotLook _look;
        private MascotPart _part;
        private Action _onClosed;
        private LoadingUiController _loadingUiController;

        [Inject]
        public MascotEditorController(MascotEditorView view, ProfileService profileService, PopupManager popupManager)
        {
            _view = view;
            _profileService = profileService;
            _popupManager = popupManager;

            _view.Init(Close, OnRandom, OnSave, OnPart, OnOption);
            _view.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();

            if (_loadingUiController != null)
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }
        }

        /// <param name="onClosed">Called when the editor is closed, saved or not.</param>
        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            _look = (_profileService.ProfileData.Mascot ?? new MascotLook()).Clone();
            _part = MascotPart.Head;
            _view.gameObject.SetActive(true);
            UpdateView();
        }

        private void Close()
        {
            _view.gameObject.SetActive(false);
            var onClosed = _onClosed;
            _onClosed = null;
            onClosed?.Invoke();
        }

        private void OnPart(MascotPart part)
        {
            _part = part;
            UpdateView();
        }

        private void OnOption(int option)
        {
            _look[_part] = option;
            UpdateView();
        }

        private void OnRandom()
        {
            _look = MascotLook.Random(_random);
            UpdateView();
        }

        private void OnSave()
        {
            SaveAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid SaveAsync(CancellationToken ct)
        {
            if (_loadingUiController != null)
            {
                return;
            }

            _profileService.ProfileData.Mascot = _look.Clone();

            try
            {
                _loadingUiController = _popupManager.ShowLoadingUi(new LoadingUiModel(LoadingElements.Animation));
                await _profileService.SaveProfileDataAsync(ct);
                Close();
            }
            finally
            {
                _popupManager.DisposePopup(_loadingUiController);
                _loadingUiController = null;
            }
        }

        private void UpdateView()
        {
            _view.Set(_look, _part);
        }
    }
}
