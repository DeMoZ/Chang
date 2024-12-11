using System;
using Chang.FSM;
using Chang.Resources;
using Chang.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Chang
{
    public class Game : IInitializable, IDisposable
    {
        private readonly IResourcesManager _resourcesManager;

        private GameFSM _gameFSM;
        private readonly AuthorizationService _authorizationService;

        [Inject]
        public Game(AuthorizationService authorizationService, IResourcesManager resourcesManager, GameFSM gameFSM)
        {
            _authorizationService = authorizationService;
            _resourcesManager = resourcesManager;
            _gameFSM = gameFSM;

            _authorizationService.OnPlayerLoggedOut += OnLoggedOut;
        }

        public async void Initialize()
        {
            Debug.Log($"{nameof(Game)} Initialize");

            await _resourcesManager.InitAsync();

            _gameFSM.Initialize();
        }

        public void Dispose()
        {
            _authorizationService.OnPlayerLoggedOut -= OnLoggedOut;
        }

        private void OnLoggedOut()
        {
            SceneManager.LoadScene("Bootstrap");
        }
    }
}