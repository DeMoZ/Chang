using System;
using Chang.FSM;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class Game : IInitializable, IDisposable
    {
        private readonly GameFSM _gameFSM;

        [Inject]
        public Game(GameFSM gameFSM)
        {
            _gameFSM = gameFSM;
        }

        public void Initialize()
        {
            Debug.Log($"{nameof(Initialize)}");
            _gameFSM.Initialize();;
        }

        public void Dispose()
        {
        }
    }
}