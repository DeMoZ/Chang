using System;
using Chang.FSM;
using Chang.Services;
using UnityEngine.SceneManagement;
using Zenject;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class Game : IInitializable, IDisposable
    {
        private readonly GameFSM _gameFSM;
        private readonly AuthorizationService _authorizationService;

        [Inject]
        public Game(GameFSM gameFSM, AuthorizationService authorizationService)
        {
            _gameFSM = gameFSM;
            _authorizationService = authorizationService;
            _authorizationService.OnPlayerLoggedOut += OnLoggedOut;
        }

        public void Initialize()
        {
            Debug.Log($"{nameof(Initialize)}");
            _gameFSM.Initialize();;
        }

        public void Dispose()
        {
            _authorizationService.OnPlayerLoggedOut -= OnLoggedOut;
        }
        
        private void OnLoggedOut()
        {
            Debug.Log("OnLoggedOut");
            SceneManager.LoadScene(ProjectConstants.REBOOT_SCENE);
        }
    }
}