using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
        public Action<string, int> OnWordsLessonClicked;
        public Action<string> OnWordsSectionRepeatClicked;
        public Action OnWordsRepeatClicked;
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