using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
        public Action<string, int> OnLessonClicked;
        public Action<string> OnSectionRepeatClicked;
        public Action OnRepeatClicked;
        public Action OnLogOutClicked;

        public bool IsLoading;

        [Inject]
        public MainScreenBus()
        {

        }

        public void Dispose()
        {
        }
    }
}