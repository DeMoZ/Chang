using System.Collections;
using System.Collections.Generic;
using Chang.FSM;
using Chang.Profile;
using Chang.Resources;
using Chang.Services;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Chang
{
    public class Game : IInitializable
    {
        private readonly IResourcesManager _resourcesManager;
        private readonly ScreenManager _screenManager;
        private readonly GameBus _gameBus;
        private readonly ProfileService _profileService;

        private GameFSM _gameFSM;

        [Inject]
        public Game(IResourcesManager resourcesManager, ScreenManager screenManager, GameBus gameBus, ProfileService profileService)
        {
            _resourcesManager = resourcesManager;
            _screenManager = screenManager;
            _gameBus = gameBus;
            _profileService = profileService;
        }

        public async void Initialize()
        {
            Debug.Log($"{nameof(Game)} Initialize");

            await _resourcesManager.InitAsync();

            _gameFSM = new GameFSM(_gameBus, _resourcesManager, _profileService);
            _gameFSM.Initialize();
        }
        
        public void Dispose()
        {
            _gameFSM.Dispose();
        }
    }
}