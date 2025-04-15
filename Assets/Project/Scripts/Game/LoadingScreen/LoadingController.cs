using System;
using Chang.Resources;
using Zenject;

namespace Chang
{
    public class LoadingController : IDisposable
    {
        private readonly LoadingView _view;

        [Inject]
        public LoadingController(LoadingView view, DownloadModel downloadModel)
        {
            _view = view;
            
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
        }
    }
}