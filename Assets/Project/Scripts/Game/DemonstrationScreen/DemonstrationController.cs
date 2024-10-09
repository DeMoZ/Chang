using Zenject;

namespace Chang
{
    public class DemonstrationWordController
    {
        private DemonstrationWordView _view;

        [Inject]
        public DemonstrationWordController(DemonstrationWordView view)
        {
            _view = view;
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}