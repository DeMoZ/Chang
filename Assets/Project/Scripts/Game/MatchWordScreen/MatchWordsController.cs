using Zenject;
using Chang.UI;

namespace Chang
{
    public class MatchWordsController: IViewController
    {
        private MatchWordsView _view;

        [Inject]
        public MatchWordsController(MatchWordsView view)
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