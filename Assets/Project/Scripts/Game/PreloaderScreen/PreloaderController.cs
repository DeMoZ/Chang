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
            throw new System.NotImplementedException();
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}