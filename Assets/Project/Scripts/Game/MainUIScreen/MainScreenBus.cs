using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
        public Action<string, int> OnGameBookLessonClicked;
        public Action<string> OnGameBookSectionRepeatClicked;
        public Action OnRepeatClicked;
        public Action OnLogOutClicked;

        [Inject]
        public MainScreenBus()
        {

        }

        public void Dispose()
        {
        }
    }
}