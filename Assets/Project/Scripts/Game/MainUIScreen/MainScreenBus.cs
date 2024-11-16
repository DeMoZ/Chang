using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
        public Action<string> OnGameBookLessonClicked ;

        [Inject]
        public MainScreenBus()
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}