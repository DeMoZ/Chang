using Zenject;

namespace Chang
{
    public class MatchWordsController
    {
        private MatchWordsView _view;

        [Inject]
        public MatchWordsController(MatchWordsView view)
        {
            _view = view;
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}