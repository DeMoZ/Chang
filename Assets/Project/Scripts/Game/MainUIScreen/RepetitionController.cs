using System;

namespace Chang
{
    public class RepetitionController : IDisposable
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
    }
}