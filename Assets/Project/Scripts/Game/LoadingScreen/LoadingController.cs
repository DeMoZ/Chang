using System;
using Chang.Resources;
using Zenject;

namespace Chang
{
    public class LoadingController : IDisposable
    {
        private readonly LoadingView _view;
        private readonly AddressablesDownloadModel _downloadModel;

        [Inject]
        public LoadingController(LoadingView view, AddressablesDownloadModel downloadModel)
        {
            _view = view;
            _downloadModel = downloadModel;
            
            downloadModel.ShowUi.Subscribe(OnShowUi);
            downloadModel.Progress.Subscribe(OnProgress);
        }

        private void OnProgress(float value)
        {
            _view.SetProgress(value);
        }

        private void OnShowUi(bool enable)
        {
            _view.gameObject.SetActive(enable);
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}