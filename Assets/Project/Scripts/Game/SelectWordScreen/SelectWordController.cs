using Zenject;

namespace Chang
{
    public class SelectWordController
    {
        private SelectWordView _view;

        [Inject]
        public SelectWordController(SelectWordView view)
        {
            _view = view;
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}