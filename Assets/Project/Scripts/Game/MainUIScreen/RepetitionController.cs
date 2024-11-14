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

        public void Dispose()
        {
            _view = null;
        }

        public void SetViewActive(bool active)
        {
            throw new NotImplementedException();
        }

        internal void Init()
        {
            throw new NotImplementedException();
        }
    }
}