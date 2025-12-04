using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
        public Action OnRepeatClicked; // todo chang this should be removed after Repetition changes
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