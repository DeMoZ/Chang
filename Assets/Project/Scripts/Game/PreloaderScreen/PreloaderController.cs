using Zenject;

namespace Chang
{
    public class PreloaderController : IViewController
    {
        private PreloaderView _view;

        [Inject]
        public PreloaderController(PreloaderView view)
        {
            _view = view;
        }

        public void Dispose()
        {
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}