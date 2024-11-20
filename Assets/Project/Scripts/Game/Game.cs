using Chang.FSM;
using Chang.Resources;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class Game : IInitializable
    {
        private readonly IResourcesManager _resourcesManager;
        
        private GameFSM _gameFSM;

        [Inject]
        public Game(IResourcesManager resourcesManager, GameFSM gameFSM)
        {
            _resourcesManager = resourcesManager;
            _gameFSM = gameFSM;
        }

        public async void Initialize()
        {
            Debug.Log($"{nameof(Game)} Initialize");

            await _resourcesManager.InitAsync();

            _gameFSM.Initialize();
        }
        
        public void Dispose()
        {
        }
    }
}