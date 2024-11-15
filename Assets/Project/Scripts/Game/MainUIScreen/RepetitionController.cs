using System;

namespace Chang
{
    public class RepetitionController : IViewController
    {
        private RepetitionView _view;

        public RepetitionController(RepetitionView view)
        {
            _view = view;
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _view = null;
        }

        public void SetViewActive(bool active)
        {
            _view.gameObject.SetActive(active);
        }
    }
}