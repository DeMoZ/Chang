using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
        public Action<string> OnGameBookLessonClicked;
        public Action<string> OnGameBookSectionRepeatClicked;
        public Action OnRepeatClicked;

        [Inject]
        public MainScreenBus()
        {

        }

        public void Dispose()
        {
        }
    }
}