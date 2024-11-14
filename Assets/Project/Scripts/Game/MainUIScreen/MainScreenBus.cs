using System;
using Zenject;

namespace Chang
{
    public class MainScreenBus : IDisposable
    {
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