using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
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